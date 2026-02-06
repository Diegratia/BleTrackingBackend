using System;

namespace Shared.Contracts.Read
{
    public class UserRead : BaseRead
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public Guid GroupId { get; set; }
        public string? GroupName { get; set; }  // Include navigation
        public LevelPriority GroupLevel { get; set; }
        public int IsEmailConfirmation { get; set; }
        public bool IsIntegration { get; set; }
        public string? profilePicture { get; set; }
        public DateTime LastLoginAt { get; set; }
        public StatusEmployee StatusActive { get; set; }
    }
}
