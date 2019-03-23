namespace Schubert.Framework.Modularity.Tools.Vs2017
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Build.Construction;

    class ResolveItems
    {
        private const string ProjectReferenceType = "ProjectReference";
        internal static IEnumerable<KeyValuePair<string, string>> GetProject()
        {
            string currentFolder = Directory.GetCurrentDirectory();

            Console.WriteLine($"{nameof(currentFolder)}:{currentFolder}");
            var files = Directory.EnumerateFiles(currentFolder, "*.csproj", SearchOption.TopDirectoryOnly);


            Console.WriteLine("Files Exits!");
            if (files != null && files.Any())
            {
                var rootProject = ProjectRootElement.Open(files.First());
                var items = rootProject.ItemGroups.SelectMany(ig => ig.Items)
                    .Where(it => it.ItemType.Equals(ProjectReferenceType, StringComparison.CurrentCultureIgnoreCase)).ToArray();

                Console.WriteLine("Begin for items!");
                foreach (var project in items)
                {
                    String include = project.Include;

                    String fullPath = System.IO.Path.Combine(currentFolder, include);
                    String dir = Path.GetDirectoryName(fullPath);
                    Console.WriteLine($"Get project path{dir}");

                    string projectName = dir.Split(System.IO.Path.DirectorySeparatorChar).Last();

                    yield return new KeyValuePair<string, string>(projectName, dir);
                }
            }
        }
    }
}
