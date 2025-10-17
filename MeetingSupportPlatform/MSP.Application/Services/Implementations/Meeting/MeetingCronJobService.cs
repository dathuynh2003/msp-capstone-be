using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MSP.Application.Repositories;
using MSP.Shared.Enums;

namespace MSP.Application.Services.Implementations.Meeting
{
    public class MeetingCronJobService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer? _timer;

        public MeetingCronJobService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Chạy cron job mỗi phút
            _timer = new Timer(UpdateMeetingStatuses, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private void UpdateMeetingStatuses(object? state)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var meetingRepository = scope.ServiceProvider.GetRequiredService<IMeetingRepository>();
                var now = DateTime.UtcNow;

                var meetings = meetingRepository.GetAllAsync().Result;

                foreach (var meeting in meetings)
                {
                    // Nếu giờ hiện tại trùng với StartTime và trạng thái chưa phải Ongoing
                    if (meeting.StartTime <= now && meeting.Status == MeetingEnum.Scheduled.ToString())
                    {
                        meeting.Status = MeetingEnum.Ongoing.ToString();
                        meeting.UpdatedAt = now;
                        meetingRepository.UpdateAsync(meeting).Wait();
                    }

                    // Nếu không có EndTime và đã quá 1 giờ từ StartTime
                    if (!meeting.EndTime.HasValue && meeting.StartTime.AddHours(1) <= now && meeting.Status == MeetingEnum.Ongoing.ToString())
                    {
                        meeting.Status = MeetingEnum.Finished.ToString();
                        meeting.UpdatedAt = now;
                        meetingRepository.UpdateAsync(meeting).Wait();
                    }
                }

                meetingRepository.SaveChangesAsync().Wait();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
