using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopModernizedWebForms.Pages
{
    public class ErrorModel : PageModel
    {
        public string Message { get; private set; }

        public void OnGet(string message)
        {
            Message = message;
        }
    }
}
