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
    public class BleReaderNodeProfile : Profile
    {
        public BleReaderNodeProfile()
        {
            CreateMap<BleReaderNode, BleReaderNodeDto>();
            CreateMap<BleReaderNodeCreateDto, BleReaderNode>();
            CreateMap<BleReaderNodeUpdateDto, BleReaderNode>();
        }
    }
}