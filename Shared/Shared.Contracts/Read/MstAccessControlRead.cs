namespace Shared.Contracts.Read
{
    public class MstAccessControlRead : BaseRead
    {
        public Guid? BrandId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public bool? IsAssigned { get; set; }
        public string? Description { get; set; }
        public string? Channel { get; set; }
        public string? DoorId { get; set; }
        public string? Raw { get; set; }
        public Guid? IntegrationId { get; set; }

        public BrandNavigationRead? Brand { get; set; }
        public IntegrationNavigationRead? Integration { get; set; }
    }

    public class BrandNavigationRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }

    public class IntegrationNavigationRead
    {
        public Guid Id { get; set; }
        public string? ApiUrl { get; set; }
    }
}
