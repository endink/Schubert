using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.Environment;
using System;
using System.Data;
using System.Data.Common;

namespace Schubert.Framework.Data
{
    public class DbTransactionScope : IDisposable
    {
        private IDatabaseTransaction _dbTransaction;
        private bool _disposed = false;
        private WorkContext _workContext = null;
        private bool _completed = false;

        public DbTransactionScope(IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted, string connectionName = null)
            : this(SchubertEngine.Current.GetWorkContext(), isolationLevel, connectionName)
        {

        }

        public DbTransactionScope(IServiceProvider serviceProvider, IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted, string connectionName = null)
            : this(serviceProvider.GetRequiredService<IWorkContextAccessor>().GetContext(), isolationLevel, connectionName)
        {

        }

        public DbTransactionScope(WorkContext workContext, IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted, string connectionName = null)
        {
            _workContext = workContext ?? throw new InvalidOperationException("当前环境缺少工作上下文 WorkContext, 请尝试使用其他重载的构造函数。");

            TransactionState state = GetTransactionState(workContext);
            int count = System.Threading.Interlocked.Increment(ref state.ChainCount);
            if (count == 1)
            {
                var context = workContext.ResolveRequired<IDatabaseContext>();
                var dbTransaction = context.BeginTransaction(isolationLevel, connectionName);
                _dbTransaction = dbTransaction;
            }
        }

        public IDatabaseTransaction RawTransaction
        {
            get { return _dbTransaction; }
        }

        private static TransactionState GetTransactionState(WorkContext workContext)
        {
            TransactionState state = workContext.GetState<TransactionState>(TransactionStateProvider.StateName);
            return state;
        }

        public void Complete()
        {
            TransactionState state = GetTransactionState(_workContext);
            int count = System.Threading.Interlocked.Increment(ref state.CommiteCount);
            if (count == state.ChainCount)
            {
                _dbTransaction.Commit();
            }
            _completed = true;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                TransactionState state = GetTransactionState(_workContext);
                if (!_completed)
                {
                    System.Threading.Interlocked.Increment(ref state.RollbackCount);
                }
                if ((state.RollbackCount + state.CommiteCount) == state.ChainCount)
                {
                    if (state.RollbackCount > 0)
                    {
                        _dbTransaction?.Rollback();
                    }
                    state.ChainCount = 0;
                    state.CommiteCount = 0;
                    state.RollbackCount = 0;
                }
                _dbTransaction?.Dispose();
                _dbTransaction = null;
                _workContext = null;
            }
        }
    }
}
