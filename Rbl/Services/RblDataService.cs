using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rbl.Helpers;
using Rbl.Models;
using Rbl.Models.Report;

namespace Rbl.Services
{
    public class RblDataService : IRblDataService
    {
        #region Properties

        private readonly RBLContext _context;

        #endregion

        #region Constructor

        public RblDataService(RBLContext context)
        {
            _context = context;
        }

        #endregion

        #region Methods

        public async Task<ScoresByIndustry> GetScoresByIndustry(string industryCode)
        {
            return await _context.ScoresByIndustry.Where(x => x.IndustryCode == industryCode).FirstOrDefaultAsync();
        }

        public async Task<ScoresTopTen> GetScoresTopTen()
        {
            return await _context.ScoresTopTen.FirstOrDefaultAsync();
        }

        public async Task<ScoresAll> GetScoresAll()
        {
            return await _context.ScoresAll.FirstOrDefaultAsync();
        }

        public async Task<ScoresByTicker> GetOrganizationScoresByTicker(string ticker)
        {
            return await _context.ScoresByTicker.Where(x => x.Ticker == ticker).FirstOrDefaultAsync();
        }

        public async Task<Organization> GetOrganizationByTicker(string ticker)
        {
            return await _context.Organizations.Where(x => x.ticker == ticker).FirstOrDefaultAsync();
        }

        public async Task<IList<Organization>> GetOrganizationsAll()
        {
            return await _context.Organizations.ToListAsync();
        }

        public async Task<IList<SelectListItem>> FillTickerDropdown()
        {
            return await _context.Organizations.Select(a =>
                new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.ticker
                }).ToListAsync();
        }

        public async Task<IDictionary<WordTypesEnum, IList<string>>> GetImportantWords(params WordTypesEnum[] types)
        {
            var results = new Dictionary<WordTypesEnum, IList<string>>();
            foreach (var wordType in types)
            {
                results[wordType] = await _GetWords(wordType);
            }

            return results;
        }

        private async Task<IList<string>> _GetWords(WordTypesEnum wordType)
        {
            switch (wordType)
            {
                case WordTypesEnum.Talent:
                    return await _context.WordlistTalents.Where(x => x.Total >= 9).Select(x => x.Word).ToListAsync();
                case WordTypesEnum.Leadership:
                    return await _context.WordlistLeaderships.Where(x => x.Total >= 9).Select(x => x.Word).ToListAsync();
                case WordTypesEnum.Organization:
                    return await _context.WordlistOrganizations.Where(x => x.Total >= 9).Select(x => x.Word).ToListAsync();
                case WordTypesEnum.Hr:
                    return await _context.WordlistHrs.Where(x => x.Total >= 9).Select(x => x.Word).ToListAsync();
                default:
                    throw new System.Exception($"No mapping has been created between enum {wordType} and a database table");
            }
        }

        public async Task<TickerHcSentencesResponse> GetSentenceResponse(string ticker, IDictionary<WordTypesEnum, IList<string>> importantWords)
        {
            var tickerDfRatio = _context.DfAllRatiosAvailables.FirstOrDefault(x => x.Ticker == ticker);
            if (tickerDfRatio == null)
                throw new System.Exception($"Could not find a ratio score for ticker '{ticker}'");

            var result = new TickerHcSentencesResponse(tickerDfRatio, importantWords);

            await Task.Delay(500);
            return result;
        }

        public async Task<DfSentence> GetCachedSentences(string ticker)
        {
            return await _context.DfSentences.FirstOrDefaultAsync(x => x.Ticker == ticker);
        }

        public async Task<bool> SaveCachedSentences(DfSentence sentence)
        {
            if (sentence.Id == 0)
                _context.DfSentences.Add(sentence);

            return await _context.SaveChangesAsync() > 0;
        }
    }

    #endregion
}