using Schubert.Framework.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public class TransactionStateProvider : IWorkContextStateProvider
    {
        public const string StateName = "TransactionState";

        private TransactionState _state = null;

        public TransactionStateProvider()
        {
            _state = new TransactionState();
        }

        public Func<WorkContext, object> Get(string name)
        {
            if (StateName.CaseSensitiveEquals(name))
            {
                return (context) => _state ?? (_state = new TransactionState());
            }
            return null;
        }
    }

    internal sealed class TransactionState
    {
        public int ChainCount;
        public int CommiteCount;
        public int RollbackCount;
    }
}
