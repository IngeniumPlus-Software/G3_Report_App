using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rbl.Models;
using Rbl.Services;
using IronPdf;
using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Rbl.Helpers;

namespace Rbl.EndPoints
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsEndpoint : ControllerBase
    {
        #region Properties

        private readonly IRblDataService _service;
        private readonly AppSettings _appSettings;

        #endregion

        #region Constructor

        public ReportsEndpoint(IRblDataService service, IOptions<AppSettings> appSettings)
        {
            _service = service;
            _appSettings = appSettings.Value;
        }

        #endregion

        #region Methods

        [HttpPost]
        [Route("{code}")]
        public async Task<bool> CheckCode(string code)
        {
            try
            {
                if (code.Equals("mdr"))
                    throw new Exception("MDR Exception");

                var ticker = await _service.GetbfuscatedTicker(code);
                return !string.IsNullOrEmpty(ticker);
            } catch(ApplicationException exception)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("{code}")]
        public async Task<IActionResult> Test(string code, bool? forceRegeneration = null)
        {
            try
            {
                var ticker = await _service.GetbfuscatedTicker(code);
                if (string.IsNullOrEmpty(code))
                    return BadRequest("Invalid Code");

                var pdfPath = $"{_appSettings.PdfLocation}/{ticker}.pdf";
                forceRegeneration = forceRegeneration ?? false;
                if (forceRegeneration.Value == false)
                {
                    if (System.IO.File.Exists(pdfPath))
                    {
                        var pdfFile = await System.IO.File.ReadAllBytesAsync(pdfPath);
                        return new FileContentResult(pdfFile, "application/pdf");
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
                var pdf = renderer.RenderUrlAsPdf($"{url}/Report?ticker={ticker}");

                _ApplyFooters(pdf, whiteFooterHtml, blueFooterHtml);

                await System.IO.File.WriteAllBytesAsync(pdfPath, pdf.BinaryData);
                return new FileContentResult(pdf.BinaryData, "application/pdf");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string FooterHtml(string color)
        {
            //return $"<span style='width: 100%;'><img style='height:60px;right50px;position:absolute;bottom:20px;' src='/images/logo_triangle_large.svg' /><h4 style='color:{color};font-size:7px;font-family:'Timesnewroman';display:inline;top:35px;position:absolute;left:40px;font-weight:bold'>CONFIDENTIAL</h4></span>";
            return $"<img style='width:6%;display:inline-block;right:50px;position:absolute;' src='/images/logo_triangle_large.svg'><h4 style=\"color:{color};font-size:7px;font-family:'Timesnewroman';margin-left:50px;font-weight:bold\">CONFIDENTIAL</h4>";
        }

        private void _ApplyFooters(PdfDocument pdf, HtmlHeaderFooter whiteBg, HtmlHeaderFooter blueBg)
        {
            var allpageNumbers = Enumerable.Range(0, pdf.PageCount);
            var blueBgPageNumbers = allpageNumbers.Intersect(new int[] { 3, 4, 8, 10, 20, 24, }.Select(x => x-1));
            var restPageNumbers = allpageNumbers.Except(blueBgPageNumbers).Except(new[] { 0 });

            pdf.AddHtmlFooters(blueBg, 2, blueBgPageNumbers);
            pdf.AddHtmlFooters(whiteBg, 2, restPageNumbers);
        }

        #endregion
    }
}
