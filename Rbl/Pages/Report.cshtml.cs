using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rbl.Models;
using Rbl.Services;
using IronPdf;
using IronPdf.Engines.Chrome;
using Microsoft.CodeAnalysis.CSharp.Syntax;


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

        public  void OnPostReport(string ticker, string companyName)
        {

            var renderer = new IronPdf.ChromePdfRenderer
            {
                RenderingOptions =
                {
                    PrintHtmlBackgrounds = true,
                    PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Portrait,
                    Title = "My PDF Document Name",
                    EnableJavaScript = true,
                    RenderDelay = 5000,
                    CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print,
                    FitToPaperMode = FitToPaperModes.Automatic,
                    Zoom = 100,
                    HtmlFooter = new IronPdf.HtmlHeaderFooter()
                    {
                        MaxHeight = 15, //millimeters
                        // HtmlFragment = "<center><i>{page} of {total-pages}<i></center>",
                        HtmlFragment = "<div class='footer'><h4>CONFIDENTIAL</h4>img src='./images/g3_logo_footer.png' /></div>",
                        DrawDividerLine = false
                    },
                    MarginTop = 0,
                    MarginLeft = 0,
                    MarginRight = 0,
                    MarginBottom = 0
                }
            };


            var url = $"https://localhost:44325/Report?ticker={ticker}&CompanyName={companyName}";
            var saveLocation = $"C:/Users/KP/Desktop/HTML/{ticker}_G3{DateTime.Now.ToString("yyyyMMdd")}.pdf";
            var pdf = renderer.RenderUrlAsPdf(url.ToString());
 
            pdf.SaveAs(saveLocation);
            //return null;
        }

        


    }
}