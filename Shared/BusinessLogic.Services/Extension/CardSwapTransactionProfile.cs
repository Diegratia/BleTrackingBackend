using AutoMapper;
using Entities.Models;
using Shared.Contracts;

namespace BusinessLogic.Services.Extension
{
    public class CardSwapTransactionProfile : Profile
    {
        public CardSwapTransactionProfile()
        {
            // Entity -> Read DTO
            CreateMap<CardSwapTransaction, CardSwapTransactionRead>()
                .ForMember(dest => dest.FromCardNumber, 
                    opt => opt.MapFrom(src => src.FromCard != null ? src.FromCard.CardNumber ?? "N/A" : "N/A"))
                .ForMember(dest => dest.ToCardNumber, 
                    opt => opt.MapFrom(src => src.ToCard != null ? src.ToCard.CardNumber ?? "N/A" : "N/A"))
                .ForMember(dest => dest.VisitorName, 
                    opt => opt.MapFrom(src => src.Visitor != null ? src.Visitor.Name ?? "N/A" : "N/A"))
                .ForMember(dest => dest.MaskedAreaName, 
                    opt => opt.MapFrom(src => src.MaskedArea != null ? src.MaskedArea.Name ?? "N/A" : "N/A"))
                .ForMember(dest => dest.CardSwapStatus, 
                    opt => opt.MapFrom(src => src.CardSwapStatus ?? CardSwapStatus.Pending))
                .ForMember(dest => dest.SwapType, 
                    opt => opt.MapFrom(src => src.SwapType ?? SwapType.EnterArea));

            // Create DTO -> Entity
            CreateMap<CardSwapTransactionCreateDto, CardSwapTransaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SwapSequence, opt => opt.Ignore())
                .ForMember(dest => dest.CardSwapStatus, opt => opt.Ignore())
                .ForMember(dest => dest.ExecutedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationId, opt => opt.Ignore());
        }
    }
}
