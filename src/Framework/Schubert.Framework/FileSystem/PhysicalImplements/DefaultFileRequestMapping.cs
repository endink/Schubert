using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.FileSystem
{
    public class DefaultFileRequestMapping : IFileRequestMapping
    {
        private String _rootUri;
        private String _rootPhysical;
        private String _requestPath;

        public DefaultFileRequestMapping(string physicalRootFolder, String requestRootPath = "/")
        {
            Guard.AbsolutePhysicalPath(physicalRootFolder, nameof(physicalRootFolder));
            Guard.ArgumentIsUri(requestRootPath, nameof(requestRootPath));
            _rootUri = physicalRootFolder;
            if (DirectorySeparator != '/')
            {
                _rootUri = _rootUri.Replace(DirectorySeparator, '/');
            }
            _rootUri = _rootUri.TrimEnd('/');
            _rootPhysical = (DirectorySeparator != '/') ? _rootUri.Replace('/', DirectorySeparator) : _rootUri;
            _requestPath = requestRootPath;
        }

        protected virtual char DirectorySeparator
        {
            get { return SchubertUtility.DirectorySeparator; }
        }
         


        public virtual string CreateAccessUrl(string physicalPath)
        {
            Guard.ArgumentNullOrWhiteSpaceString(physicalPath, nameof(physicalPath));
            if (DirectorySeparator != '/')
            {
                physicalPath = physicalPath.Replace(DirectorySeparator, '/');
            }
            String path = physicalPath.TrimStart(_rootUri);

            if (Uri.IsWellFormedUriString(_requestPath, UriKind.Absolute))
            {
                return $"{_requestPath.TrimEnd('/')}/{path.TrimStart('/')}";
            }

            return $"/{_requestPath.Trim('/')}/{path.TrimStart('/')}";
        }

        

        public virtual string GetFilePath(string relativePath, string scope)
        {
            if (DirectorySeparator != '/')
            {
                relativePath = relativePath.Replace('/', DirectorySeparator).TrimStart(DirectorySeparator);
            }
            else
            {
                relativePath = relativePath.TrimStart(DirectorySeparator);
            }

            if (scope.IsNullOrWhiteSpace())
            {
                return Path.Combine(_rootPhysical, relativePath);
            }
            return Path.Combine(_rootPhysical, scope, relativePath);
        }

        public virtual string GetRelativeApplicationPath(string physicalPath, string scope)
        {
            Guard.AbsolutePhysicalPath(physicalPath, nameof(physicalPath));
            string root = _rootPhysical;
            if (!scope.IsNullOrWhiteSpace())
            {
                root = Path.Combine(_rootPhysical, scope);
            }
            string path = physicalPath.TrimStart(root).TrimStart(DirectorySeparator);
            return path;
        }
    }
}
