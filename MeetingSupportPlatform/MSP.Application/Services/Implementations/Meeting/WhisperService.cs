using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;
using MSP.Application.Services.Interfaces.Meeting;
using MSP.Application.Models.Responses.Meeting;

namespace MSP.Application.Services.Implementations.Meeting
{
    public class WhisperService : IWhisperService
    {
        public async Task<List<TranscriptionLine>> TranscribeVideoAsync(string videoPath)
        {
            if (!File.Exists(videoPath))
                throw new FileNotFoundException(videoPath);

            // 1. Chuyển video -> MP3
            using var mp4Stream = File.OpenRead(videoPath);
            using var mp3Stream = new MemoryStream();

            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(mp4Stream))
                .OutputToPipe(new StreamPipeSink(mp3Stream),
                    o => o.DisableChannel(Channel.Video).ForceFormat("mp3"))
                .ProcessAsynchronously();

            mp3Stream.Position = 0;

            // 2. Resample audio về 16kHz
            using var reader = new Mp3FileReader(mp3Stream);
            var outFormat = new WaveFormat(16000, reader.WaveFormat.Channels);
            using var resampler = new MediaFoundationResampler(reader, outFormat);
            using var waveStream = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(waveStream, resampler);
            waveStream.Position = 0;

            // 3. Segment audio 2 phút
            using var waveReader = new WaveFileReader(waveStream);
            var segmentDuration = TimeSpan.FromMinutes(2);
            var totalDuration = waveReader.TotalTime;
            var numOfSegments = (int)Math.Ceiling(totalDuration.TotalSeconds / segmentDuration.TotalSeconds);

            var results = new List<TranscriptionLine>();

            for (int i = 0; i < numOfSegments; i++)
            {
                waveStream.Position = 0;
                using var segmentReader = new WaveFileReader(waveStream);
                var segment = segmentReader.ToSampleProvider()
                    .Skip(i * segmentDuration)
                    .Take(segmentDuration);

                var segmentProvider = segment.ToWaveProvider16();
                using var segmentStream = new MemoryStream();
                WaveFileWriter.WriteWavFileToStream(segmentStream, segmentProvider);
                segmentStream.Position = 0;

                using var processor = await GetProcessorAsync();
                var durationOffsetSeconds = i * segmentDuration.TotalSeconds;

                // 4. Process segment với Whisper
                await foreach (var item in processor.ProcessAsync(segmentStream))
                {
                    var offset = TimeSpan.FromSeconds(durationOffsetSeconds);

                    results.Add(new TranscriptionLine
                    {
                        Text = item.Text ?? string.Empty,
                        StartTs = (int)(item.Start + offset).TotalMilliseconds,
                        StopTs = (int)(item.End + offset).TotalMilliseconds
                    });

                }
            }

            return results;
        }

        private static async Task<WhisperProcessor> GetProcessorAsync()
        {
            using var memoryStream = new MemoryStream();
            var model = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.TinyEn);
            await model.CopyToAsync(memoryStream);

            var whisperFactory = WhisperFactory.FromBuffer(memoryStream.ToArray());
            return whisperFactory.CreateBuilder()
                .WithLanguage("en")
                .Build();
        }
    }
}
