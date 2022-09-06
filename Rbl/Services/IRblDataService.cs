using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rbl.Models;

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

    }
}
