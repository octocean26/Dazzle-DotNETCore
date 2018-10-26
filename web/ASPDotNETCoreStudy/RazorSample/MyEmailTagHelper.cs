using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace RazorSample
{
    [HtmlTargetElement("email")]
    public class MyEmailTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            //评估邮件元素主体的Razor内容
            string body = (await output.GetChildContentAsync()).GetContent();

            //<email>替换为<a>
            output.TagName = "a";
            //准备mailto url
            string to = context.AllAttributes["to"].Value.ToString();
            string subject = context.AllAttributes["subject"].Value.ToString();
            string mailto = "mailto:" + to;
            if (!string.IsNullOrWhiteSpace(subject))
            {
                mailto = $"{mailto}&subject={subject}&body={body}";
            }

            //准备输出
            output.Attributes.Clear();
            output.Attributes.SetAttribute("href", mailto);
            output.Content.Clear();
            output.Content.AppendFormat("邮箱：{0}", to);

        }

    }
}
