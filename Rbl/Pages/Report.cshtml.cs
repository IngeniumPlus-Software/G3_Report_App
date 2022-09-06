using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rbl.Helpers;
using Rbl.Models;
using Rbl.Services;

namespace Rbl.Pages
{
    public class ReportModel : PageModel
    {
        private readonly Rbl.Models.RBLContext _context;
        private readonly IRblDataService _service;

        public ReportModel(Rbl.Models.RBLContext context, IRblDataService service)
        {
            _context = context;
            _service = service;
        }

        public Organization Organization { get; set; }

       
        public ScoresByTicker ScoresByTicker { get; set; }
        public ScoresAll ScoresAll { get; set; }
        public ScoresByIndustry ScoresIndustry { get; set; }
        public ScoresTopTen ScoresTop10 { get; set; }

        public string CompanyName { get; set; }

        public decimal OrganizationScoreTotal { get; set; }
        public decimal AllScoreTotal { get; set; }
        public decimal TopTenScoreTotal { get; set; }
        public decimal IndustryScoreTotal { get; set; }
        public string[] TalentSentences { get; set; } = new string[0];
        public string[] LeadershipSentences { get; set; } = new string[0];
        public string[] OrganizationSentences { get; set; } = new string[0];
        public string[] HrSentences { get; set; } = new string[0];


        public async Task<IActionResult> OnGetAsync(string ticker, string companyName)
        {
            
            CompanyName = companyName;

            if (String.IsNullOrEmpty(ticker))
            {
                return NotFound();
            }

            Organization = await _service.GetOrganizationByTicker(ticker);

            if (Organization == null)
            {
                return NotFound();
            }

            var allWordTypes = new[] { WordTypesEnum.Talent, WordTypesEnum.Leadership, WordTypesEnum.Organization, WordTypesEnum.Hr };
            var importantWords = await _service.GetImportantWords(allWordTypes);
            var allSentences = await _service.GetSentenceResponse(ticker, importantWords);
            foreach(var t in allWordTypes)
            {
                var tSentences = allSentences.GetRawHtml(t);
                switch(t)
                {
                    case WordTypesEnum.Talent:
                        TalentSentences = tSentences;
                        break;
                    case WordTypesEnum.Leadership:
                        LeadershipSentences = tSentences;
                        break;
                    case WordTypesEnum.Organization:
                        OrganizationSentences = tSentences;
                        break;
                    case WordTypesEnum.Hr:
                        HrSentences = tSentences;
                        break;
                }
            }

            ScoresByTicker = await _service.GetOrganizationScoresByTicker(ticker);
            ScoresAll = await _service.GetScoresAll();
            ScoresIndustry = await _service.GetScoresByIndustry(Organization.industry_code);
            ScoresTop10 = await _service.GetScoresTopTen();

            IndustryScoreTotal = (decimal) (ScoresIndustry.HrScore + ScoresIndustry.LeadershipScore +
                                           ScoresIndustry.OrganizationScore + ScoresIndustry.TalentScore);

            OrganizationScoreTotal = (decimal) (ScoresByTicker.HrScore + ScoresByTicker.LeadershipScore +
                                                ScoresByTicker.OrganizationScore + ScoresByTicker.TalentScore);

            AllScoreTotal = (decimal) (ScoresAll.HrScore + ScoresAll.LeadershipScore +
                                       ScoresAll.OrganizationScore + ScoresAll.TalentScore);

            TopTenScoreTotal = (decimal) (ScoresTop10.HrScore + ScoresTop10.LeadershipScore +
                                          ScoresTop10.OrganizationScore + ScoresTop10.TalentScore);

            return Page();
        }




    }
}