# Vietnamese Transcript Generation API

## Tổng quan
API này tạo transcript tiếng Việt chính xác dựa trên transcript tiếng Anh và video (nếu có). API sử dụng Gemini AI để:
- Phân tích transcript tiếng Anh và ngữ cảnh video
- Tạo transcript tiếng Việt tự nhiên và chính xác
- Thích ứng với văn hóa và thuật ngữ chuyên môn Việt Nam

## Endpoint
```
POST /api/v1/summarize/video-text-analysis
```

## Content-Type
```
multipart/form-data
```

## Request Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| text | string | Yes | Transcript tiếng Anh của cuộc họp/bài thuyết trình |
| video | file | No | File video (tùy chọn) - cung cấp ngữ cảnh visual |

## Supported Video Formats
- MP4
- AVI
- MOV
- WMV
- FLV
- WebM
- MKV

## File Size Limit
- Tối đa 100MB cho video

## Response Format

```json
{
  "success": true,
  "message": "Vietnamese transcript generated successfully",
  "data": {
    "summary": "Transcript tiếng Việt chính...",
    "videoAnalysis": "Thông tin cuộc họp...",
    "combinedAnalysis": "Transcript đầy đủ..."
  }
}
```

## Response Fields

| Field | Type | Description |
|-------|------|-------------|
| summary | string | Transcript tiếng Việt được tạo dựa trên nội dung gốc |
| videoAnalysis | string | Thông tin cuộc họp (người tham gia, bối cảnh, loại cuộc họp) |
| combinedAnalysis | string | Transcript đầy đủ bao gồm thông tin cuộc họp và nội dung chính |

## Example Usage

### 1. English Transcript Only
```bash
curl -X POST "https://localhost:7000/api/v1/summarize/video-text-analysis" \
  -H "Content-Type: multipart/form-data" \
  -F "text=John: Good morning everyone. Let's start our weekly team meeting. Sarah, can you give us an update on the mobile app development project?"
```

### 2. English Transcript + Video
```bash
curl -X POST "https://localhost:7000/api/v1/summarize/video-text-analysis" \
  -H "Content-Type: multipart/form-data" \
  -F "text=John: Good morning everyone. Let's start our weekly team meeting..." \
  -F "video=@team-meeting.mp4"
```

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "English transcript is required",
  "data": null
}
```

### 400 Bad Request - File Size
```json
{
  "success": false,
  "message": "Video file size cannot exceed 100MB!",
  "data": null
}
```

### 400 Bad Request - Unsupported Format
```json
{
  "success": false,
  "message": "Unsupported video format. Supported formats: MP4, AVI, MOV, WMV, FLV, WebM, MKV",
  "data": null
}
```

## Notes
- API sử dụng Gemini AI để tạo transcript tiếng Việt
- Kết quả được trả về bằng tiếng Việt tự nhiên và chính xác
- Video được sử dụng để cung cấp ngữ cảnh visual và xác định người nói
- Transcript được tạo dựa trên ý nghĩa và ngữ cảnh, không phải dịch thuật trực tiếp
- Thích ứng với văn hóa và thuật ngữ chuyên môn Việt Nam
