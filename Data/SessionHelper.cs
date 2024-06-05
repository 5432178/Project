using Microsoft.AspNetCore.Http;
using System;

namespace Project.Helpers
{
    public static class SessionHelper
    {
        public static string GetSessionValue(IHttpContextAccessor httpContextAccessor, string key)
        {
            return httpContextAccessor.HttpContext?.Session?.GetString(key);
        }
    }
}
