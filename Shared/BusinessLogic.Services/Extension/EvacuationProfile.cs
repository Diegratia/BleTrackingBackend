using AutoMapper;
using Data.ViewModels;
using Entities.Models;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Extension
{
    public class EvacuationProfile : Profile
    {
        public EvacuationProfile()
        {
            // EvacuationAssemblyPoint
            CreateMap<EvacuationAssemblyPointCreateDto, EvacuationAssemblyPoint>();
            CreateMap<EvacuationAssemblyPointUpdateDto, EvacuationAssemblyPoint>();
            CreateMap<EvacuationAssemblyPoint, EvacuationAssemblyPointRead>();

            // EvacuationAlert
            CreateMap<EvacuationAlertCreateDto, EvacuationAlert>();
            CreateMap<EvacuationAlertUpdateDto, EvacuationAlert>();
            CreateMap<EvacuationAlert, EvacuationAlertRead>();

            // EvacuationTransaction
            CreateMap<EvacuationTransaction, EvacuationTransactionRead>();
        }
    }
}
