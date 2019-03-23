using Quartz;
using Schubert.Framework.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework.Scheduling
{
    public class JobContextHolder
    {
        private AsyncLocal<WorkContext> _context;
        private static JobContextHolder _current;
        private static readonly object SyncRoot = new object();

        private JobContextHolder()
        {
            _context = new AsyncLocal<WorkContext>();
        }

        public static JobContextHolder Current
        {
            get
            {
                if (_current == null)
                {
                    lock (SyncRoot)
                    {
                        if (_current == null)
                        {
                            _current = new JobContextHolder();
                        }
                    }
                }
                return _current;
            }
        }
        
        public WorkContext Context
        {
            get { return _context.Value; }
            set { _context.Value = value; }
        }
    }
}
