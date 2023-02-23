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
        Task<GeneralScoreResponse> GetScoresByIndustry(int year, string industryCode);
        Task<GeneralScoreResponse> GetScoresTopTen(int year);
        Task<GeneralScoreResponse> GetScoresTotalForLastInTopTen(int year);
        Task<GeneralScoreResponse> GetScoresAll(int year);
        Task<bool> OrganizationHasScoreForYear(int year, string ticker);
        Task<GeneralScoreResponse> GetOrganizationScoresByTicker(int year, string ticker);
        Task<Organization> GetOrganizationByTicker(string ticker);
        Task<IList<SelectListItem>> FillTickerDropdown();
    }
}
