using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Rbl.Pages
{
    public class Index2022Model : PageModel
    {
        #region Properties

        #endregion

        #region Constructor

        public Index2022Model()
        {
        }

        #endregion

        #region Model

        #endregion

        public IActionResult OnGetAsync(int? id)
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            return Page();
        }
    }
}