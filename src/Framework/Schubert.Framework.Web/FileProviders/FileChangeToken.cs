using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.FileProviders
{
    internal class FileChangeToken
    {
        private Regex _searchRegex;

        public FileChangeToken(string pattern)
        {
            Pattern = pattern;
        }

        public string Pattern { get; }

        private CancellationTokenSource TokenSource { get; } = new CancellationTokenSource();

        private Regex SearchRegex
        {
            get
            {
                if (_searchRegex == null)
                {
                    _searchRegex = new Regex(
                        '^' + Pattern + '$',
                        RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture,
                        TimeSpan.FromMinutes(1));
                }

                return _searchRegex;
            }
        }

        public bool ActiveChangeCallbacks => true;

        public bool HasChanged => TokenSource.Token.IsCancellationRequested;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            return TokenSource.Token.Register(callback, state);
        }

        public bool IsMatch(string relativePath)
        {
            return SearchRegex.IsMatch(relativePath);
        }

        public void Changed()
        {
            Task.Run(() =>
            {
                try
                {
                    TokenSource.Cancel();
                }
                catch
                {
                }
            });
        }
    }

}
