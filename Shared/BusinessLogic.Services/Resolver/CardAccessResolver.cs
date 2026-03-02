using AutoMapper;
using Entities.Models;
using Data.ViewModels;

public class CardAccessMaskedAreaResolver : IValueResolver<CardAccess, CardAccessDto, List<Guid?>>
{
    public List<Guid?> Resolve(CardAccess source, CardAccessDto destination, List<Guid?> destMember, ResolutionContext context)
    {
        return source.CardAccessMaskedAreas?.Select(x => (Guid?)x.MaskedAreaId).ToList() ?? new List<Guid?>();
    }
}

public class CardAccessTimeGroupResolver : IValueResolver<CardAccess, CardAccessDto, List<Guid?>>
{
    public List<Guid?> Resolve(CardAccess source, CardAccessDto destination, List<Guid?> destMember, ResolutionContext context)
    {
        return source.CardAccessTimeGroups?.Select(x => (Guid?)x.TimeGroupId).ToList() ?? new List<Guid?>();
    }
}
