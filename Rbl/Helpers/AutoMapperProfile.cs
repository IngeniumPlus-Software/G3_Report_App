using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Rbl.Models;
using Rbl.Models.Report;

namespace Rbl.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {

            CreateMap<ScoresAll_2021, GeneralScoreResponse>();
            CreateMap<ScoresByIndustry_2021, GeneralScoreResponse>();
            CreateMap<ScoresTopTen_2021, GeneralScoreResponse>();
            CreateMap<ScoresByTicker_2021, GeneralScoreResponse>();
            CreateMap<ScoresTotal_2021, GeneralScoreResponse>();

            CreateMap<ScoresAll_2022, GeneralScoreResponse>();
            CreateMap<ScoresByIndustry_2022, GeneralScoreResponse>();
            CreateMap<ScoresTopTen_2022, GeneralScoreResponse>();
            CreateMap<ScoresByTicker_2022, GeneralScoreResponse>();
            CreateMap<ScoresTotal_2022, GeneralScoreResponse>();

            CreateMap<Organization, ReportResponse>()
                .ForMember(dest => dest.HasExtendedData, opt => opt.MapFrom(src => src.HasExtendedData))
                .ForMember(dest => dest.Ticker, opt => opt.MapFrom(src => src.ticker));
        }
    }
}
