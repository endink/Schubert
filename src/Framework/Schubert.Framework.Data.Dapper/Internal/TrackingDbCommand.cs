using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    internal class TrackingDbCommand : DbCommand
    {
        private DbCommand _innerCommand = null;
        private ILogger _logger = null;
        public TrackingDbCommand(DbCommand command, ILoggerFactory loggerFactory)
        {
            Guard.ArgumentNotNull(command, nameof(command));
            _innerCommand = command;
            _logger = loggerFactory?.CreateLogger("Database Tracking") ?? NullLogger.Instance;
        }

        public override string CommandText
        {
            get { return this._innerCommand.CommandText; }
            set { this._innerCommand.CommandText = value; }
        }

        public override int CommandTimeout
        {
            get { return this._innerCommand.CommandTimeout; }
            set { this._innerCommand.CommandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return this._innerCommand.CommandType; }
            set { this._innerCommand.CommandType = value; }
        }
        
        protected override DbConnection DbConnection
        {
            get { return _innerCommand.Connection; }
            set { _innerCommand.Connection = value; }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return _innerCommand.Parameters; }
        }

        public override bool DesignTimeVisible
        {
            get { return this._innerCommand.DesignTimeVisible; }
            set { this._innerCommand.DesignTimeVisible = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return _innerCommand.UpdatedRowSource; }
            set { _innerCommand.UpdatedRowSource = value; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return this._innerCommand.Transaction; }
            set { this._innerCommand.Transaction = value; }
        }
        

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _innerCommand?.Dispose();
                _innerCommand = null;
            }
        }
        
        private void WriteFormattedLog()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("tracking T-SQL excuting");
            builder.AppendLine(this.CommandText);
            builder.AppendLine();
            builder.AppendLine("parameters:");
            if (this.Parameters.Count > 0)
            {
                builder.AppendLine(this.Parameters.Cast<IDbDataParameter>()
                    .Select(p => $"@{p.ParameterName} = {GetValueString(p)}").ToArrayString(", "));
            }
            string message = builder.ToString();
            this._logger.WriteDebug(message);
        }

        private static String GetValueString(IDbDataParameter parameter)
        {
            return (parameter.Value is DBNull || parameter.Value == null) ? "NULL" : parameter.Value.ToString();
        }

        public override void Cancel()
        {
            this._innerCommand.Cancel();
        }

        
        public override int ExecuteNonQuery()
        {
            this.WriteFormattedLog();
            return this._innerCommand.ExecuteNonQuery();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            this.WriteFormattedLog();
            return this._innerCommand.ExecuteReader(behavior);
        }

        public override object ExecuteScalar()
        {
            this.WriteFormattedLog();
            return this._innerCommand.ExecuteScalar();
        }

        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            this.WriteFormattedLog();
            return this._innerCommand.ExecuteReaderAsync(behavior, cancellationToken);
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            this.WriteFormattedLog();
            return this._innerCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            this.WriteFormattedLog();
            return this._innerCommand.ExecuteScalarAsync(cancellationToken);
        }

        public override void Prepare()
        {
            this._innerCommand.Prepare();
        }

        protected override DbParameter CreateDbParameter()
        {
            return this._innerCommand.CreateParameter();
        }
        
    }
}
