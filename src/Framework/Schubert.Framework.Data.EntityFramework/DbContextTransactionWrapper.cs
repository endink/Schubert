using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public class DbContextTransactionWrapper : IDatabaseTransaction
    {
        private IDbContextTransaction _dbContextTransaction;
        public DbContextTransactionWrapper(IDbContextTransaction dbContextTransaction)
        {
            Guard.ArgumentNotNull(dbContextTransaction, nameof(dbContextTransaction));
            _dbContextTransaction = dbContextTransaction;
        }

        public void Commit()
        {
            _dbContextTransaction?.Commit();
        }

        public void Dispose()
        {
            _dbContextTransaction?.Dispose();
            _dbContextTransaction = null;
        }

        public void Rollback()
        {
            _dbContextTransaction?.Rollback();
        }
    }
}
