using AutoMapper;
using SVC_Kline.Models.Entities;
using SVC_Kline.Models.Input;
using SVC_Kline.Models.Output;

namespace SVC_Kline.Mappings;

/// <summary>
/// AutoMapper profile for mapping between input models and entity models.
/// </summary>
public class KlineDataMappingProfile : Profile
{
    public KlineDataMappingProfile()
    {
        CreateMap<KlineDataNew, KlineDataEntity>();
        CreateMap<KlineDataEntity, KlineData>();
    }
}
