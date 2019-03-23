
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment
{
    using System.Collections.Concurrent;
#if !COREFX
    public class DefaultAssemblyReader : IAssemblyReader
    {
        private IPathProvider _pathProvider = null;
        public DefaultAssemblyReader(IPathProvider pathProvider)
        {
            Guard.ArgumentNotNull(pathProvider, nameof(pathProvider));
            _pathProvider = pathProvider;
        }
        //public IEnumerable<Assembly> ReadAllProjectAssemblies()
        //{
        //    if (_assemblies == null)
        //    {
        //        List<Assembly> list = new List<Assembly>();
        //        string directory = _pathProvider.RootDirectoryPhysicalPath;
        //        var files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);
        //        var schubertLibs = SchubertUtility.GetSchubertLibraries().Select(l => $"{l.ToLower()}.dll");
        //        foreach (var f in files)
        //        {
        //            string fileName = Path.GetFileName(f).ToLower();

        //            if (!schubertLibs.Contains(fileName) && !fileName.StartsWith("microsoft") && !fileName.StartsWith("system"))
        //            {
        //                Assembly assembly = Assembly.LoadFile(f);
        //                list.Add(assembly);
        //            }
        //        }
        //        _assemblies = list.AsReadOnly();
        //    }

        //    return _assemblies;
        //}

        public Assembly ReadByName(string assemblyName)
        {
            return Assembly.Load(assemblyName);
        }

        public Assembly ReadBytes(byte[] assemblyBytes, byte[] assemblySymbols = null)
        {
            return (assemblySymbols == null) ? Assembly.Load(assemblyBytes) : Assembly.Load(assemblyBytes, assemblySymbols);
        }

        public Assembly ReadFile(string path)
        {
            return Assembly.LoadFile(path);
        }

        public Assembly ReadStream(Stream assemblyStream, Stream assemblySymbols = null)
        {
            byte[] stream = assemblyStream.ReadBytesToEnd();
            byte[] symbols = assemblySymbols == null ? null : assemblySymbols.ReadBytesToEnd();
            return this.ReadBytes(stream, symbols);
        }
    }

#else

    using System.Runtime.Loader;
    public class DefaultAssemblyReader : IAssemblyReader
    {
        private AssemblyLoadContext _context = null;
        private IPathProvider _pathProvider = null;
        
        public DefaultAssemblyReader(IPathProvider pathProvider)
        {
            Guard.ArgumentNotNull(pathProvider, nameof(pathProvider));
            _pathProvider = pathProvider; 
             _context = AssemblyLoadContext.Default;
           
        }

        public Assembly ReadByName(string assemblyName)
        {
            AssemblyName name = new AssemblyName(assemblyName);
            return _context.LoadFromAssemblyName(name);
        }

        public Assembly ReadBytes(byte[] assemblyBytes, byte[] assemblySymbols = null)
        {
            using (MemoryStream ms = new MemoryStream(assemblyBytes))
            {
                if (assemblySymbols.IsNullOrEmpty())
                {
                    return _context.LoadFromStream(ms);
                }
                else
                {
                    using (MemoryStream bms = new MemoryStream(assemblySymbols))
                    {
                        return _context.LoadFromStream(ms, bms);
                    }
                }
            }
        }

        public Assembly ReadFile(string path)
        {
            try
            {
                return _context.LoadFromAssemblyPath(path);
            }
            catch (Exception ex)
            {
                ex.ThrowIfNecessary();
                //.net core bug?
                if (ex.GetOriginalException<FileLoadException>() != null)
                {
                    var name = AssemblyLoadContext.GetAssemblyName(path);
                    return _context.LoadFromAssemblyName(name);

                }
                throw;
            }
        }

        public Assembly ReadStream(Stream assemblyStream, Stream assemblySymbols = null)
        {
            if (assemblySymbols == null)
            {
                return _context.LoadFromStream(assemblyStream);
            }
            else
            {
                return _context.LoadFromStream(assemblyStream, assemblySymbols);
            }
        }
    }
#endif
}
