using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace My.TagHelpers.Study.Component
{
    public class AddressTagHelperComponent: TagHelperComponent
    {
        private readonly string _markup;
        public override int Order => 3;

        public AddressTagHelperComponent(string markup=""){
            _markup = markup;
             
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
             if(string.Equals(context.TagName, "address", StringComparison.OrdinalIgnoreCase)&&
             output.Attributes.ContainsName("printable")){
                TagHelperContent childContent = await output.GetChildContentAsync();
                string content = childContent.GetContent();
                output.Content.SetHtmlContent($"<div>{content}<br/>{_markup}</div>");
            }
        }
    }
}
