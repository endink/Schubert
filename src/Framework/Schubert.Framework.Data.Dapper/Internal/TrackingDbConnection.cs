using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework.Data.Internal
{
    internal class TrackingDbConnection : DbConnection
    {
        private DbConnection _innerConnection;
        private ILoggerFactory _loggerFactory = null;
        private static MisuseDetector _misuseDetector;
        private static readonly Object SyncRoot = new object();

        public TrackingDbConnection(DbConnection innerConnection, ILoggerFactory loggerFactory)
        {
            Guard.ArgumentNotNull(innerConnection, nameof(innerConnection));

            CreateMisuseDetectorIfNotExisted(loggerFactory);
            _innerConnection = innerConnection;
            _loggerFactory = loggerFactory;
            _misuseDetector.Increase();
        }

        private static void CreateMisuseDetectorIfNotExisted(ILoggerFactory loggerFactory)
        {
            if (_misuseDetector == null)
            {
                lock (SyncRoot)
                {
                    if (_misuseDetector == null)
                    {
                        _misuseDetector = new MisuseDetector(typeof(DbConnection), loggerFactory, 100);
                    }
                }
            }
        }

        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            return this._innerConnection.OpenAsync(cancellationToken);
        }

        public override string ConnectionString
        {
            get { return this._innerConnection.ConnectionString; }
            set { this._innerConnection.ConnectionString = value; }
        }
        
        public override string Database
        {
            get { return this._innerConnection.Database; }
        }

        public override string DataSource
        {
            get { return this._innerConnection.DataSource; }
        }

        public override string ServerVersion
        {
            get { return this._innerConnection.ServerVersion; }
        }

        public override ConnectionState State
        {
            get { return this._innerConnection.State; }
        }

        public override void ChangeDatabase(string databaseName)
        {
            this._innerConnection.ChangeDatabase(databaseName);
        }
        

        public override void Close()
        {
            this._innerConnection.Close();
        }
        
        public override void Open()
        {
            this._innerConnection.Open();
        }
        

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return this._innerConnection.BeginTransaction(isolationLevel);
        }

        protected override DbCommand CreateDbCommand()
        {
            var dbCommand = this._innerConnection.CreateCommand();
            return new TrackingDbCommand(dbCommand, this._loggerFactory);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _innerConnection?.Dispose();
                _innerConnection = null;
                _misuseDetector.Decrease();
            }
        }
    }
}
