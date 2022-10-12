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
            _context= context;
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
        public async Task<IActionResult> GeneratePdf(string code, bool? forceRegeneration = null, bool? shouldReturnPdf = true)
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
                var pdf = renderer.RenderUrlAsPdf($"{url}/Report?ticker={ticker}");

                _ApplyFooters(pdf, whiteFooterHtml, blueFooterHtml);

                await System.IO.File.WriteAllBytesAsync(pdfPath, pdf.BinaryData);
                if(shouldReturnPdf ?? false)
                    return new FileContentResult(pdf.BinaryData, "application/pdf");
                return
                    Ok(pdfPath);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("{action}")]
        public async Task<IActionResult> BatchGeneratePdfs([FromBody] BatchClass model)
        {
            var tasks = new List<Task>();

            if(model.AllCodes)
            {
                if (string.IsNullOrEmpty(model.Key) || string.IsNullOrEmpty(model.Secret))
                    return BadRequest();

                if (!model.Key.Equals(_appSettings.ResetKey, StringComparison.CurrentCulture) || !model.Secret.Equals(_appSettings.ResetSecret, StringComparison.CurrentCulture))
                    return BadRequest();

                model.Codes = _context.OrganizationMaps.Select(x => x.Code).ToList();
            }

            foreach(var code in model.Codes)
            {
                tasks.Add(GeneratePdf(code, model.ForceRegenerate, false));
            }

            Task.WaitAll(tasks.ToArray());

            return Ok($"{tasks.Select(x => x.IsCompletedSuccessfully).Count()} PDFs completed successfully");
        }

        public class BatchClass
        {
            public IList<string> Codes { get; set; } = new List<string>();
            public bool ForceRegenerate { get; set; } = false;
            public bool AllCodes { get; set; } = false;
            public string Key { get; set; } = string.Empty;
            public string Secret { get; set; } = string.Empty;
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
            foreach(var pdfPath in allPdfs)
            {
                try
                {
                    System.IO.File.Delete(pdfPath);
                    cleared++;
                }
                catch(Exception ex)
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
            var blueBgPageNumbers = allpageNumbers.Intersect(new int[] { 3, 4, 8, 10, 20, 24, }.Select(x => x-1));
            var restPageNumbers = allpageNumbers.Except(blueBgPageNumbers).Except(new[] { 0 });

            pdf.AddHtmlFooters(blueBg, 2, blueBgPageNumbers);
            pdf.AddHtmlFooters(whiteBg, 2, restPageNumbers);
        }

        #endregion
    }
}
