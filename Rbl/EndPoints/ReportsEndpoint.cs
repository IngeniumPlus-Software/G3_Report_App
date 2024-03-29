﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rbl.Models;
using Rbl.Services;
using IronPdf;
using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Rbl.Helpers;
using System.Collections.Generic;

namespace Rbl.EndPoints
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsEndpoint : ControllerBase
    {
        #region Properties

        private readonly IRblDataService _service;
        private readonly AppSettings _appSettings;
        private readonly RBLContext _context;

        #endregion

        #region Constructor

        public ReportsEndpoint(IRblDataService service, IOptions<AppSettings> appSettings, RBLContext context)
        {
            _service = service;
            _appSettings = appSettings.Value;
            _context = context;
        }

        #endregion

        #region Methods

        [HttpPost]
        [Route("pdf/{year?}")]
        public async Task<PdfResponse> GetPdf([FromBody] PdfModel model, int? year)
        {
            if (year.HasValue == false)
            {
                year = 2021;
            }

            if (!_appSettings.AdminPassword.Equals(model.Password, StringComparison.InvariantCulture))
            {
                return new PdfResponse
                {
                    Success = false,
                    Message = "Invalid password",
                    Redirect = string.Empty
                };
            }

            if (!_context.Organizations.Any(x => x.ticker == model.Code))
            {
                return new PdfResponse
                {
                    Success = false,
                    Message = "Could not find the Organization",
                    Redirect = string.Empty
                };
            }

            bool foundScore = false;
            if (year == 2021)
            {
                foundScore = _context.ScoresByTicker_2021.Any(x => x.Ticker == model.Code);
            }
            else if (year == 2022)
            {
                foundScore = _context.ScoresByTicker_2022.Any(x => x.Ticker == model.Code);
            }

            if (!foundScore)
            {
                return new PdfResponse
                {
                    Success = false,
                    Message = $"No scores were found for {model.Code}",
                    Redirect = string.Empty
                };
            }

            await _PdfAction(model.Code, year.Value, false, false);

            return new PdfResponse
            {
                Success = true,
                Message = "Redirecting",
                Redirect = $"/api/reports/{model.Code}/{year}"
            };
        }

        public class PdfResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string Redirect { get; set; }
        }

        [HttpGet]
        [Route("{code}/{year}")]
        public async Task<IActionResult> GeneratePdf(string code, int year = 2021, bool? forceRegeneration = null)
        {
            try
            {
                return await _PdfAction(code, year, forceRegeneration);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("{action}/{year}")]
        public async Task<IActionResult> BatchGeneratePdfs(int year, [FromBody] BatchClass model)
        {
            var tasks = new List<Task>();

            foreach (var ticker in model.Codes)
            {
                tasks.Add(_PdfAction(ticker, year, model.ForceRegenerate, false));
            }

            Task.WaitAll(tasks.ToArray());

            return Ok($"{tasks.Select(x => x.IsCompletedSuccessfully).Count()} PDFs completed successfully");
        }

        private async Task<IActionResult> _PdfAction(string ticker, int year, bool? forceRegeneration = null, bool? shouldReturnPdf = true)
        {
            ticker = ticker.ToLower();
            var pdfPath = $"{_appSettings.PdfLocation}/{ticker}_{year}.pdf";
            forceRegeneration = forceRegeneration ?? false;
            if (forceRegeneration.Value == false)
            {
                if (System.IO.File.Exists(pdfPath))
                {
                    if (shouldReturnPdf ?? false)
                    {
                        var pdfFile = await System.IO.File.ReadAllBytesAsync(pdfPath);
                        return new FileContentResult(pdfFile, "application/pdf");
                    }
                    else
                        return Ok(pdfPath);
                }
            }

            var schema = HttpContext.Request.Scheme;
            var host = HttpContext.Request.Host;
            var url = $"{schema}://{host}";

            var blueFooterHtml = new HtmlHeaderFooter
            {
                BaseUrl = new Uri($"{url}").AbsoluteUri,
                HtmlFragment = FooterHtml("#FFF"),
                MaxHeight = 15,
            };

            var whiteFooterHtml = new HtmlHeaderFooter
            {
                BaseUrl = new Uri($"{url}").AbsoluteUri,
                HtmlFragment = FooterHtml("#101010"),
                MaxHeight = 15,

            };

            var renderer = new ChromePdfRenderer();
            renderer.RenderingOptions = new ChromePdfRenderOptions
            {
                FirstPageNumber = 2,
                PaperSize = IronPdf.Rendering.PdfPaperSize.A4,
                CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print,
                MarginTop = 0,
                MarginBottom = 0,
                MarginLeft = 0,
                MarginRight = 0,
                ApplyMarginToHeaderAndFooter = false,
                EnableJavaScript = true,
                RenderDelay = 500
            };
            var pdf = renderer.RenderUrlAsPdf($"{url}/Report?ticker={ticker}&year={year}");

            _ApplyFooters(pdf, whiteFooterHtml, blueFooterHtml);

            await System.IO.File.WriteAllBytesAsync(pdfPath, pdf.BinaryData);
            if (shouldReturnPdf ?? false)
                return new FileContentResult(pdf.BinaryData, "application/pdf");
            return
                Ok(pdfPath);
        }

        public class BatchClass
        {
            public IList<string> Codes { get; set; } = new List<string>();
            public bool ForceRegenerate { get; set; } = false;
            public string Key { get; set; } = string.Empty;
            public string Secret { get; set; } = string.Empty;
        }

        public class PdfModel
        {
            public string Code { get; set; }
            public string Password { get; set; }
        }

        [HttpPost]
        [Route("{key}/reset/{secret}")]
        public IActionResult ClearCachedPdfs(string? key, string? secret)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
                return BadRequest();

            if (!key.Equals(_appSettings.ResetKey, StringComparison.CurrentCulture) || !secret.Equals(_appSettings.ResetSecret, StringComparison.CurrentCulture))
                return BadRequest();

            var allPdfs = System.IO.Directory.GetFiles(_appSettings.PdfLocation);
            int cleared = 0, skipped = 0;
            foreach (var pdfPath in allPdfs)
            {
                try
                {
                    System.IO.File.Delete(pdfPath);
                    cleared++;
                }
                catch (Exception ex)
                {
                    skipped++;
                }
            }

            return Ok($"PDFs deleted: {cleared}, Failed to Delete: {skipped}");
        }

        private string FooterHtml(string color)
        {
            //return $"<span style='width: 100%;'><img style='height:60px;right50px;position:absolute;bottom:20px;' src='/images/logo_triangle_large.svg' /><h4 style='color:{color};font-size:7px;font-family:'Timesnewroman';display:inline;top:35px;position:absolute;left:40px;font-weight:bold'>CONFIDENTIAL</h4></span>";
            return $"<img style='width:6%;display:inline-block;right:50px;position:absolute;' src='/images/logo_triangle_large.svg'><h4 style=\"color:{color};font-size:7px;font-family:'Timesnewroman';margin-left:50px;font-weight:bold\">CONFIDENTIAL</h4>";
        }

        private void _ApplyFooters(PdfDocument pdf, HtmlHeaderFooter whiteBg, HtmlHeaderFooter blueBg)
        {
            var allpageNumbers = Enumerable.Range(0, pdf.PageCount);
            var blueBgPageNumbers = allpageNumbers.Intersect(new int[] { 4, 5, 9, 11, 20, 24, }.Select(x => x - 1));
            var restPageNumbers = allpageNumbers.Except(blueBgPageNumbers).Except(new[] { 0 });

            pdf.AddHtmlFooters(blueBg, 2, blueBgPageNumbers);
            pdf.AddHtmlFooters(whiteBg, 2, restPageNumbers);
        }

        #endregion
    }
}
