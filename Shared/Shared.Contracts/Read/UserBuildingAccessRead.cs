using System;

namespace Shared.Contracts.Read
{
    /// <summary>
    /// Read DTO for UserBuildingAccess showing user and building relationship
    /// </summary>
    public class UserBuildingAccessRead
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public Guid BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
