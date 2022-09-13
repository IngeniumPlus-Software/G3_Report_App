using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rbl.Models;
using Rbl.Services;
using IronPdf;
using System;
using System.Linq;

namespace Rbl.EndPoints
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsEndpoint : ControllerBase
    {
        #region Properties

        private readonly IRblDataService _service;

        #endregion

        #region Constructor

        public ReportsEndpoint(IRblDataService service)
        {
            _service = service;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("old/{code}")]
        public async Task<IActionResult> DownloadPdfReport(string code)
        {
            var ticker = await _service.GetbfuscatedTicker(code);
            if (string.IsNullOrEmpty(code))
                return BadRequest("Invalid Code");

            var schema = HttpContext.Request.Scheme;
            var host = HttpContext.Request.Host;
            var url = $"{schema}://{host}";


            var renderer = new ChromePdfRenderer();
            renderer.RenderingOptions = new ChromePdfRenderOptions
            {
                PaperSize = IronPdf.Rendering.PdfPaperSize.A4,
                CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print,
                MarginTop = 0,
                MarginBottom = 0,
                MarginLeft = 0,
                MarginRight = 0,
                ApplyMarginToHeaderAndFooter = false,
            };
            var pdf = renderer.RenderUrlAsPdf($"{url}/report?ticker={ticker}");

            return new FileContentResult(pdf.BinaryData, "application/pdf");
        }

        [HttpGet]
        [Route("{code}")]
        public async Task<IActionResult> Test(string code)
        {
            var ticker = await _service.GetbfuscatedTicker(code);
            if (string.IsNullOrEmpty(code))
                return BadRequest("Invalid Code");

            var schema = HttpContext.Request.Scheme;
            var host = HttpContext.Request.Host;
            var url = $"{schema}://{host}";

            var blueFooterHtml = new HtmlHeaderFooter
            {
                BaseUrl = new Uri($"{url}").AbsoluteUri,
                HtmlFragment = "<img style='width:4%;display:inline-block;right:100px;position:absolute' src='/images/g3_logo_footer.png'><h4 style=\"color:white;font-size:7px;font-family:'Timesnewroman';margin-left:20px;font-weight:bold\">CONFIDENTIAL</h4>",
                MaxHeight = 15,
            };

            var whiteFooterHtml = new HtmlHeaderFooter
            {
                BaseUrl = new Uri($"{url}").AbsoluteUri,
                HtmlFragment = "<img style='width:4%;display:inline-block;right:100px;position:absolute' src='/images/g3_logo_footer.png'><h4 style=\"color:black;font-size:7px;font-family:'Timesnewroman';margin-left:20px;font-weight:bold\">CONFIDENTIAL</h4>",
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
            };
            var pdf = renderer.RenderUrlAsPdf($"{url}/test?ticker={ticker}");

            _ApplyFooters(pdf, whiteFooterHtml, blueFooterHtml);

            return new FileContentResult(pdf.BinaryData, "application/pdf");
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
