using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BootstrappingMvcCore
{
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Method,AllowMultiple =false)]
    public class CultureAttribute:ActionFilterAttribute
    {
        public string Name { get; set; }
        public static string CookieName
        {
            get { return "_Culture"; }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var culture = Name;
            if (string.IsNullOrEmpty(culture))
            {
                culture = GetSavedCultureOrDefault(context.HttpContext.Request);
            }
            SetCultureOnThread(culture);

            base.OnActionExecuting(context);
        }

        
        private static string GetSavedCultureOrDefault(HttpRequest request)
        {
            var culture = CultureInfo.CurrentCulture.Name;
            var cookie = request.Cookies[CookieName] ?? culture;
            return culture;
        }

        private static void SetCultureOnThread(string language)
        {
            var _cultureInfo = new CultureInfo(language);
            CultureInfo.CurrentCulture = _cultureInfo;
            CultureInfo.CurrentUICulture = _cultureInfo;
        }

    }
}
