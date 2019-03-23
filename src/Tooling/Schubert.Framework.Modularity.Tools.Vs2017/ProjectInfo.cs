using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Schubert.Framework.Modularity.Tools.Vs2017
{
    public class ProjectInfo
    {
        public ProjectInfo(string fullPath)
        {
            this.FullPath = fullPath;
            this.ProjectFileName = Path.GetFileName(fullPath);
            this.ProjectName = Path.GetFileNameWithoutExtension(fullPath);
            this.ProjectFolder = Path.GetDirectoryName(fullPath);
        }

        public string FullPath { get; set; }

        public string ProjectFileName { get; set; }

        public string ProjectName { get; set; }

        public string ProjectFolder { get; set; }

        /// <summary>返回表示当前对象的字符串。</summary>
        /// <returns>表示当前对象的字符串。</returns>
        public override string ToString()
        {
            return $"{nameof(this.ProjectName)}: {this.ProjectName}";
        }

        public IEnumerable<String> EnumerateContentFiles()
        {
            return Directory.EnumerateFiles(this.ProjectFolder, "*.*", SearchOption.AllDirectories);
        }
    }
}
