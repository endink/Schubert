using Schubert.Framework.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test.FileSystem
{
    public class DefaultFileRequestMappingTester
    {

        private class LinuxMapping : DefaultFileRequestMapping
        {
            public LinuxMapping(string physicalRootFolder, string requestRootPath = "/") : base(physicalRootFolder, requestRootPath)
            {
            }

            protected override char DirectorySeparator => '/';
        }

        [Fact(DisplayName ="DefaultFileRequestMapping: Linux 路径测试")]
        public void TestLinux()
        {
            LinuxMapping mapping = new LinuxMapping("/usr/local/labijie/Blobs", "https://cnd.schubert.com/blobs/");
            string f = mapping.GetFilePath("77_2722832169594880/in_images_df13b3e3fb83490d.svg/in_thumbnails_df13b3e3fb83490d_240.png", "artworks-thums");
            String result = mapping.CreateAccessUrl(f);
        }
    }
}
