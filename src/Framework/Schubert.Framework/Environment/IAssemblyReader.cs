using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Schubert.Framework.Environment
{
    public interface IAssemblyReader
    {
        Assembly ReadByName(string assemblyName);
        Assembly ReadFile(string path);
        Assembly ReadStream(Stream assemblyStream, Stream assemblySymbols = null);
        Assembly ReadBytes(byte[] assemblyBytes, byte[] assemblySymbols =null);
    }
}