using Schubert.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Test.DapperIdentity
{
    public class IIdGenerationServiceImp : IIdGenerationService
    {
        private int _currentId = 999;

        public long GenerateId()
        {
            return System.Threading.Interlocked.Increment(ref _currentId);
        }
    }
}
