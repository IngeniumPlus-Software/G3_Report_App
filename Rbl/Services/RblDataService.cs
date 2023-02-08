using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly Mapper _mapper;

        #endregion

        #region Constructor

        public RblDataService(RBLContext context)
        {
            _context = context;
            var mc = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>());
            _mapper = new Mapper(mc);
        }

        #endregion

        #region Methods

        public async Task<GeneralScoreResponse> GetScoresByIndustry(int year, string industryCode)
        {
            if (year == 2021)
            {
                return _MapResponse(await _context.ScoresByIndustry_2021.Where(x => x.IndustryCode == industryCode).FirstOrDefaultAsync());
            }
            else if (year == 2022)
            {
                return _MapResponse(await _context.ScoresByIndustry_2022.Where(x => x.IndustryCode == industryCode).FirstOrDefaultAsync());
            }
            return null;
        }

        public async Task<GeneralScoreResponse> GetScoresTopTen(int year)
        {
            if (year == 2021)
            {
                return _MapResponse(await _context.ScoresTopTen_2021.FirstOrDefaultAsync());
            }
            else if (year == 2022)
            {
                return _MapResponse(await _context.ScoresTopTen_2022.FirstOrDefaultAsync());
            }
            return null;
        }

        public async Task<GeneralScoreResponse> GetScoresTotalForLastInTopTen(int year)
        {
            if (year == 2021)
            {
                var total = await _context.ScoresTotal_2021.CountAsync();
                var top10Percent = (int)Math.Ceiling(total * .1);

                return _MapResponse(await _context.ScoresTotal_2021.OrderByDescending(x => x.TotalScore).Take(top10Percent).LastOrDefaultAsync());
            }
            else if (year == 2022)
            {
                var total = await _context.ScoresTotal_2022.CountAsync();
                var top10Percent = (int)Math.Ceiling(total * .1);

                return _MapResponse(await _context.ScoresTotal_2022.OrderByDescending(x => x.TotalScore).Take(top10Percent).LastOrDefaultAsync());
            }
            return null;
        }

        public async Task<GeneralScoreResponse> GetScoresAll(int year)
        {
            if (year == 2021)
            {
                return _MapResponse(await _context.ScoresAll_2021.FirstOrDefaultAsync());
            }
            else if (year == 2022)
            {
                return _MapResponse(await _context.ScoresAll_2022.FirstOrDefaultAsync());
            }
            return null;
        }

        public async Task<bool> OrganizationHasScoreForYear(int year, string ticker)
        {
            if (year == 2021)
            {
                return await _context.ScoresByTicker_2021.AnyAsync(x => x.Ticker == ticker);
            }
            else if (year == 2022)
            {
                return await _context.ScoresByTicker_2022.AnyAsync(x => x.Ticker == ticker);
            }

            return false;
        }

        public async Task<GeneralScoreResponse> GetOrganizationScoresByTicker(int year, string ticker)
        {
            if (year == 2021)
            {
                return _MapResponse(await _context.ScoresByTicker_2021.Where(x => x.Ticker == ticker).FirstOrDefaultAsync());
            }
            else if (year == 2022)
            {
                return _MapResponse(await _context.ScoresByTicker_2022.Where(x => x.Ticker == ticker).FirstOrDefaultAsync());
            }
            return null;
        }

        private GeneralScoreResponse _MapResponse<T>(T obj)
        {
            if (obj != null)
            {
                return _mapper.Map<GeneralScoreResponse>(obj);
            }

            return null;
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