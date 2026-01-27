using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;
using Helpers.Consumer;

namespace BusinessLogic.Services.Extension
{
    public class GeofenceProfile : Profile
    {
        public GeofenceProfile()
        {
            CreateMap<Geofence, GeofenceDto>();
            CreateMap<GeofenceCreateDto, Geofence>();
            CreateMap<GeofenceUpdateDto, Geofence>();
        }
    }
}