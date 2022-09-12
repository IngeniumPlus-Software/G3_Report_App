using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rbl.Helpers;
using Rbl.Models;
using Rbl.Models.Report;

namespace Rbl.Services
{
    public interface IRblDataService
    {
        Task<ScoresByIndustry> GetScoresByIndustry(string industryCode);
        Task<ScoresTopTen> GetScoresTopTen();
        Task<ScoresAll> GetScoresAll();
        Task<ScoresByTicker> GetOrganizationScoresByTicker(string ticker);
        Task<Organization> GetOrganizationByTicker(string ticker);
        Task<IList<SelectListItem>> FillTickerDropdown();
        Task<IDictionary<WordTypesEnum, IList<string>>> GetImportantWords(params WordTypesEnum[] types);
        Task<TickerHcSentencesResponse> GetSentenceResponse(string ticker, IDictionary<WordTypesEnum, IList<string>> importantWords);
        Task<DfSentence> GetCachedSentences(string ticker);
        Task<bool> SaveCachedSentences(DfSentence sentence);
        Task<string?> GetbfuscatedTicker(string code);
    }
}
