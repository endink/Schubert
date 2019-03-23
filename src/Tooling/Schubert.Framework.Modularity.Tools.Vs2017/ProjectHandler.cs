using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Schubert.Framework.Modularity.Tools.Vs2017
{
    public class ProjectHandler
    {
        private List<string> _projectFilePaths;
        private string _projectFilePath;


        /// <summary>初始化 <see cref="T:System.Object" /> 类的新实例。</summary>
        public ProjectHandler(string projectFilePath)
        {
            this._projectFilePath = projectFilePath;
            this._projectFilePaths = new List<string>();
            //this._projectFilePaths.Add(projectFilePath);
        }

        public List<ProjectInfo> GetProjectFiles()
        {
            var dir = Path.GetDirectoryName(this._projectFilePath);
            var fileName = Path.GetFileName(this._projectFilePath);

            this.GetAllProjectFile(dir, new[] { fileName });

            var result = this._projectFilePaths
                .Select(x => Path.GetFullPath(x))
                .Distinct()
                .Select(x => new ProjectInfo(x))
                .ToList();
            return result;
        }

        private IEnumerable<string> GetAllProjectFile(string baseDir, IEnumerable<string> paths, bool isRoot = false)
        {
            if (!paths.Any())
            {
                return Enumerable.Empty<string>();
            }

            foreach (string path in paths)
            {
                var fullPath = Path.Combine(baseDir, path);
                var relPaths = this.GetProjectFileRefrence(fullPath);
                var fs = relPaths.Select(x => Path.Combine(Path.GetDirectoryName(fullPath), x));
                this._projectFilePaths.AddRange(fs);
                return fs;
                //不需要递归引用，框架已要求显式引用
                //return this.GetAllProjectFile(Path.GetDirectoryName(fullPath), relPaths);
            }
            return Enumerable.Empty<string>();
        }

        private List<string> GetProjectFileRefrence(string fullPath)
        {
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var xml = XDocument.Load(reader);

            var itemGroups = xml.Element("Project")?.Elements("ItemGroup");

            var subProjects = new List<string>();

            foreach (XElement itemGroup in itemGroups)
            {
                var projects = itemGroup.Elements("ProjectReference");
                foreach (var pElement in projects)
                {
                    var relPath = pElement.Attribute("Include")?.Value;
                    subProjects.Add(relPath.Replace('\\', System.IO.Path.DirectorySeparatorChar));
                }
            }
            return subProjects;
        }

    }
}
