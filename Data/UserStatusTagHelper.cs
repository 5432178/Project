using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Project.TagHelpers
{
    [HtmlTargetElement("user-status")]
    public class UserStatusTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserStatusTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                output.TagName = "li";
                output.Content.SetHtmlContent("<a class='nav-link text-dark' href='/Account/Logout'>Logout</a>");
            }
            else
            {
                output.TagName = "li";
                output.Content.SetHtmlContent("<a class='nav-link text-dark' href='/Account/Login'>Login</a>");
            }
        }
    }
}
