using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rbl.Models;

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
            return await _context.ScoresByIndustry.Where(x=>x.IndustryCode==industryCode).FirstOrDefaultAsync();
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
    }

    #endregion
}