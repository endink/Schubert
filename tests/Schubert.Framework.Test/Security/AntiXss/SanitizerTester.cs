using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Schubert.Framework.Test.Security.AntiXss
{
    public class SanitizerTester
    {
        [Fact]
        public void TestScript()
        {
            string html = @"<a href=""javascript:void(0)"">测试<a><p>OKOK</p><script type=""text/javascript"">var a = "";</script>";

            var result = Schubert.Framework.Security.AntiXss.Sanitizer.GetSafeHtmlFragment(html);
        }
    }
}
