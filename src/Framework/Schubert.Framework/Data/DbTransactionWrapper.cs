using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public class DbTransactionWrapper : IDatabaseTransaction
    {
        private DbTransaction _innerTransaction;
        public DbTransactionWrapper(DbTransaction dbTransaction)
        {
            Guard.ArgumentNotNull(dbTransaction, nameof(dbTransaction));
            _innerTransaction = dbTransaction;
        }
        public void Commit()
        {
            _innerTransaction?.Commit();
        }

        public void Dispose()
        {
            _innerTransaction?.Dispose();
            _innerTransaction = null;
        }

        public void Rollback()
        {
            _innerTransaction?.Rollback();
        }
    }
}
