namespace MSP.Application.Models.Responses.Users
{
    public class BusinessReponse
    {
        public Guid Id { get; set; }
        public string BusinessName { get; set; }
        public string BusinessOwnerName { get; set; }
        public int MemberCount { get; set; }
        public int ProjectCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; }
        public bool? CanRequestJoin { get; set; }

    }
}
