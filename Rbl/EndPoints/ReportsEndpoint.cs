using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rbl.Models;
using Rbl.Models.Report;
using Rbl.Services;

namespace Rbl.EndPoints
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsEndpoint :ControllerBase
    {
        #region Properties

        private readonly RBLContext _context;
        private readonly IRblDataService _service;
        private readonly IMapper _mapper;

        #endregion

        #region Constructor

        public ReportsEndpoint(IRblDataService service, RBLContext context, IMapper mapper)
        {
            _service = service;
            _context = context;
            _mapper = mapper;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("getScores")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ReportResponse>> GetScores(string ticker)
        {
            var org = await _service.GetOrganizationByTicker(ticker);
           // var scoresAll =  await _service.GetScoresAll();

            var companyResponse = _mapper.Map<ReportResponse>(org);



            var response = new ReportResponse
            {
                Ticker = org.ticker, IndustryCode = org.industry_code, HasExtendedData = org.HasExtendedData
            };


            response.ScoresAll = _mapper.Map<GeneralScoreResponse>(await _service.GetScoresAll());
            response.ScoresByTicker =  _mapper.Map<GeneralScoreResponse>(await _service.GetOrganizationScoresByTicker(ticker));
            response.ScoresTopTen = _mapper.Map<GeneralScoreResponse>(await _service.GetScoresTopTen());
            response.ScoresByIndustry =  _mapper.Map<GeneralScoreResponse>(await _service.GetScoresByIndustry(org.industry_code));


            return Ok(response);
        }

        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public async Task<ActionResult<ReportResponse>> GetReport(ReportCreateRequest model)
        //{
        //    var org = await _service.G(model.Ticker);
        //    var scoresAll =  await _service.GetScoresAll();

        //    var companyResponse = _mapper.Map<ReportResponse>(org);

            

        //    var response = new ReportResponse();
        //    response.Ticker = org.ticker;
        //    response.IndustryCode = org.ticker;
        //    response.HasExtendedData = org.HasExtendedData;


        //    return Ok(response);
        //}



        #endregion


    }
}
