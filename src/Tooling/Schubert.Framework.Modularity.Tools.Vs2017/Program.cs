namespace Schubert.Framework.Modularity.Tools.Vs2017
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        private const string DestDirectoryName = "Modules";
        private static int _copyCount;

        //modular --c xxx.csproj --d xx/publish/
        // CLI: dotnet modularity --config $(MSBuildProjectFullPath) --dest $(publishUrl)
        static int Main(string[] args)
        {

            var tuple = GetParameters(args);
            var projectFilePath = tuple.Item1;
            var dest = tuple.Item2;

            if (string.IsNullOrEmpty(projectFilePath) || string.IsNullOrEmpty(dest))
            {
                return 0;
            }

            Console.WriteLine();
            Console.WriteLine();

            var handler = new ProjectHandler(projectFilePath);
            List<ProjectInfo> files = handler.GetProjectFiles();
            List<KeyValuePair<ProjectInfo, string>> sourceFiles = GetCopyFiles(files);
            CopyFile(sourceFiles, dest);
            Console.WriteLine("Finish");

            Console.WriteLine("modularity succeeded!");
            return 0;
        }

        private static Tuple<string, string> GetParameters(string[] args)
        {
            var paramDic = new Dictionary<string, string>();
            if (args.Length < 4)
            {
                Console.WriteLine($"Schubert modulary : parameters length less than 4.".Red().Bright());
            }

            for (int i = 0; i < args.Length; i += 2)
            {
                paramDic.Add(args[i], args[i + 1]);
            }

            string projectFilePath;
            var success = paramDic.TryGetValue("--config", out projectFilePath) || paramDic.TryGetValue("--c", out projectFilePath);


            if (string.IsNullOrEmpty(projectFilePath) || !File.Exists(projectFilePath))
            {
                Console.WriteLine($"Schubert modulary : project file {projectFilePath} not exists.".Red().Bright());
            }

            string dest;
            success = paramDic.TryGetValue("--dest", out dest) || paramDic.TryGetValue("--d", out dest); ;


            if (string.IsNullOrEmpty(dest))
            {
                Console.WriteLine($"Schubert modulary : dest paramter not exists.".Red().Bright());
            }
            return new Tuple<string, string>(projectFilePath, dest);
        }

        static List<KeyValuePair<ProjectInfo, string>> GetCopyFiles(IEnumerable<ProjectInfo> projectFile)
        {
            var result = projectFile
                .Select(
                    x => new
                    {
                        Files = x.EnumerateContentFiles(),
                        Project = x
                    });
            var files = new List<KeyValuePair<ProjectInfo, string>>();
            foreach (var project in result)
            {
                var projectFiles = project.Files.Where(x => IsSearchFile(project.Project.ProjectFolder, x)).Select(x => new KeyValuePair<ProjectInfo, string>(project.Project, x));
                files.AddRange(projectFiles);
            }

            foreach (var file in files)
            {
                Console.WriteLine($"[{file.Key.ProjectName}] : {file.Value}");
            }

            return files;
        }

        static void CopyFile(List<KeyValuePair<ProjectInfo, string>> projectFile, string dest)
        {
            string destRoot = Path.Combine(dest, DestDirectoryName);
            if (Directory.Exists(destRoot))
            {
                Directory.Delete(destRoot, true);
            }
            _copyCount = 0;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            object locked = new object();

            try
            {
                Parallel.ForEach(projectFile, keyValuePair =>
                {
                    var destFolder = Path.Combine(destRoot, keyValuePair.Key.ProjectName);
                    var destPath = keyValuePair.Value.Replace(keyValuePair.Key.ProjectFolder, destFolder);
                    Console.WriteLine($"Schubert modulary : deploy dependency file {destPath}");
                    if (!Directory.Exists(Path.GetDirectoryName(destPath)))
                    {
                        lock (locked)
                        {
                            if (!Directory.Exists(Path.GetDirectoryName(destPath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                            }
                        }
                    }
                    File.Copy(keyValuePair.Value, destPath, true);

                    Interlocked.Increment(ref _copyCount);
                });

                sw.Stop();
                Console.WriteLine($"Schubert modulary completed : {_copyCount} files in {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static bool IsSearchFile(string rootFolder, string filePath)
        {
            string ext = Path.GetExtension(filePath);
            string fileName = Path.GetFileName(filePath);

            bool allowed = ext.Equals(".cshtml", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".html", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".js", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".css", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".xml", StringComparison.OrdinalIgnoreCase);

            string objFolder = Path.Combine(rootFolder, "obj");
            string binFolder = Path.Combine(rootFolder, "bin");

            bool isExcepted = filePath.StartsWith(objFolder, StringComparison.OrdinalIgnoreCase) ||
                filePath.StartsWith(binFolder, StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("project.json", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("project.lock.json", StringComparison.OrdinalIgnoreCase);

            return !isExcepted && allowed;
        }
    }
}