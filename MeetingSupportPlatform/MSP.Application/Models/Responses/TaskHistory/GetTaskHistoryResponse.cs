using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.ProjectTask;

namespace MSP.Application.Models.Responses.TaskHistory
{
    public class GetTaskHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        
        // For assignment/reassignment tracking
        public Guid? FromUserId { get; set; }
        public Guid? ToUserId { get; set; }
        
        // Action tracking
        public string Action { get; set; } // Created, Assigned, Reassigned, Updated, StatusChanged
        public Guid ChangedById { get; set; }
        
        // Field change tracking
        public string? FieldName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public GetTaskResponse? Task { get; set; }
        public GetUserResponse? FromUser { get; set; }
        public GetUserResponse? ToUser { get; set; }
        public GetUserResponse? ChangedBy { get; set; }
        
        // Display properties
        public string ActionDisplay => Action switch
        {
            "Created" => "Tạo mới",
            "Assigned" => "Giao việc",
            "Reassigned" => "Chuyển giao",
            "Updated" => "Cập nhật",
            "StatusChanged" => "Thay đổi trạng thái",
            _ => "Không xác định"
        };
        
        public string ChangeDescription
        {
            get
            {
                return Action switch
                {
                    "Created" => $"Tạo công việc",
                    "Assigned" => $"Giao việc cho {ToUser?.FullName ?? "N/A"}",
                    "Reassigned" => $"Chuyển giao từ {FromUser?.FullName ?? "N/A"} sang {ToUser?.FullName ?? "N/A"}",
                    "StatusChanged" => $"Thay đổi trạng thái từ '{OldValue}' sang '{NewValue}'",
                    "Updated" when FieldName == "Title" => $"Thay đổi tiêu đề từ '{OldValue}' sang '{NewValue}'",
                    "Updated" when FieldName == "Description" => $"Cập nhật mô tả",
                    "Updated" when FieldName == "StartDate" => $"Thay đổi ngày bắt đầu từ {OldValue} sang {NewValue}",
                    "Updated" when FieldName == "EndDate" => $"Thay đổi hạn chót từ {OldValue} sang {NewValue}",
                    "Updated" => $"Cập nhật {FieldName}",
                    _ => "Thay đổi"
                };
            }
        }
    }
}
