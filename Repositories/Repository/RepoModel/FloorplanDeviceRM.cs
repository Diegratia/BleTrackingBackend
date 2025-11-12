using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;

namespace Repositories.Repository.RepoModel
{
    public class FloorplanDeviceRM : BaseModelDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public Guid FloorplanId { get; set; }
        public Guid AccessCctvId { get; set; }
        public Guid ReaderId { get; set; }
        public Guid AccessControlId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosPxX { get; set; }
        public float PosPxY { get; set; }
        public Guid FloorplanMaskedAreaId { get; set; }
        public Guid ApplicationId { get; set; }
        public string? DeviceStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Status { get; set; } = 1;
        public MinimalFloorplanRM? Floorplan { get; set; }
        public MinimalBleReaderRM? Reader { get; set; }
        public MinimalAccessCctvRM? AccessCctv { get; set; }
        public MinimalAccessControlRM? AccessControl { get; set; }
        public MinimalMaskedAreaRM? FloorplanMaskedArea { get; set; }
    }

        public class FloorplanDeviceRM2
    {
            public Guid Id { get; set; }
            public String? Name { get; set; }
    }
}