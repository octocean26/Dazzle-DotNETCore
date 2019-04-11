using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace My.TagHelpers.Study.Component
{
    public class AddressScriptTagHelperComponent:TagHelperComponent
    {
        public override int Order => 2;
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if(string.Equals(context.TagName,"body",StringComparison.OrdinalIgnoreCase )){
                var script = await File.ReadAllTextAsync("Views/wy.html");
                output.PostContent.AppendHtml(script);
            }
        }
    }
}
