using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Rbl.Services;

namespace Rbl.Pages
{
    public class IndexModel : PageModel
    {
        #region Properties

        private readonly ILogger<IndexModel> _logger;
        private readonly IRblDataService _service;

        [BindProperty] 
        public InputModel Input { get; set; }

        public IList<SelectListItem> TickersList { get; set; }

        #endregion

        #region Constructor

        public IndexModel(ILogger<IndexModel> logger, IRblDataService service)
        {
            _logger = logger;
            _service = service;
        }

        #endregion

        #region Model

        public class InputModel
        {
            [Required]
            [Display(Name = "Company Name")]
            public string CompanyName { get; set; }

            [Required] 
            [Display(Name = "Ticker")] 
            public string Ticker { get; set; }
        }

        #endregion


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            TickersList = await _service.FillTickerDropdown();

            //if (Org == null)
            //{
            //    return NotFound();
            //}
            return Page();
        }

        public IActionResult OnPost()
        {
            
            if (ModelState.IsValid)
            {
                return RedirectToPage("Report", new {ticker = Input.Ticker, Input.CompanyName});
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }



    }
}