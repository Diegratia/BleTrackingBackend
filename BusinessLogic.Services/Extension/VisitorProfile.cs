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
    public class VisitorProfile : Profile
    {
        public VisitorProfile()
        {
            CreateMap<VisitorCreateDto, Visitor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()))
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.FaceImage, opt => opt.Ignore())
                .ForMember(dest => dest.UploadFr, opt => opt.Ignore())
                .ForMember(dest => dest.UploadFrError, opt => opt.Ignore());
            CreateMap<VMSOpenVisitorCreateDto, Visitor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()))
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.FaceImage, opt => opt.Ignore())
                .ForMember(dest => dest.UploadFr, opt => opt.Ignore())
                .ForMember(dest => dest.UploadFrError, opt => opt.Ignore());
            CreateMap<VisitorUpdateDto, Visitor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.FaceImage, opt => opt.Ignore())
                .ForMember(dest => dest.UploadFr, opt => opt.Ignore())
                .ForMember(dest => dest.UploadFrError, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Visitor, VisitorDto>();
            CreateMap<Visitor, OpenVisitorDto>();
            CreateMap<TrxVisitorCreateDto, TrxVisitor>();
            CreateMap<CreateInvitationDto, TrxVisitor>();

            CreateMap<BlacklistReasonDto, Visitor>();
            
            CreateMap<TrxVisitor, TrxVisitorDto>();
            // CreateMap<VisitorUpdateDto, Visitor>();
            CreateMap<TrxVisitorUpdateDto, TrxVisitor>();
            CreateMap<SendEmailInvitationDto, TrxVisitor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VisitorId, opt => opt.Ignore());
            CreateMap<MemberInvitationDto, TrxVisitor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VisitorId, opt => opt.Ignore());
            CreateMap<VisitorInvitationDto, TrxVisitor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VisitorId, opt => opt.Ignore())

                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<VisitorInvitationDto, Visitor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FaceImage, opt => opt.Ignore())
                .ForMember(dest => dest.IdentityType, opt => opt.MapFrom(src => Enum.Parse<IdentityType>(src.IdentityType, true)))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));// karena ini file, tidak bisa di-map langsung
        }
        
    }
}