using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Rbl.Models;
using Rbl.Models.Report;

namespace Rbl.Helpers
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            
            CreateMap<ScoresAll, GeneralScoreResponse>();
            CreateMap<ScoresByIndustry, GeneralScoreResponse>();
            CreateMap<ScoresTopTen, GeneralScoreResponse>();
            CreateMap<ScoresByTicker, GeneralScoreResponse>();

            CreateMap<Organization, ReportResponse>()
                .ForMember(dest => dest.HasExtendedData, opt => opt.MapFrom(src => src.HasExtendedData))
                .ForMember(dest => dest.Ticker, opt => opt.MapFrom(src => src.ticker));   
        }
    }
}
