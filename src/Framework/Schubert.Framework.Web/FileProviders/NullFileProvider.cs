using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Schubert.Framework.Web.FileProviders
{
    public sealed class NullFileProvider : IFileProvider
    {
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return new NullDirectoryContents();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return new NullFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return null;
        }

        private class NullFileInfo : IFileInfo
        {
            public NullFileInfo(string path)
            {
                this.PhysicalPath = path ?? String.Empty;
            }

            public bool Exists
            {
                get
                {
                    return false;
                }
            }

            public bool IsDirectory
            {
                get
                {
                    return false;
                }
            }

            public DateTimeOffset LastModified
            {
                get
                {
                    return DateTimeOffset.MinValue;
                }
            }

            public long Length
            {
                get
                {
                    return 0;
                }
            }

            public string Name
            {
                get
                {
                    return String.Empty;
                }
            }

            public string PhysicalPath { get; }

            public Stream CreateReadStream()
            {
                return null;
            }
        }

        private class NullDirectoryContents : IDirectoryContents
        {
            public bool Exists
            {
                get
                {
                    return false;
                }
            }

            public IEnumerator<IFileInfo> GetEnumerator()
            {
                return Enumerable.Empty<IFileInfo>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)Enumerable.Empty<IFileInfo>()).GetEnumerator();
            }
        }
    }
}
