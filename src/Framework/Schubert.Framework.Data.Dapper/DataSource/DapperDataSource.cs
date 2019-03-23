using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public class DapperDataSource : IEquatable<DapperDataSource>
    {

        internal DapperDataSource(IDatabaseProvider provider, string readingConnectionName, string writingConnectionName)
        {
            this.ReadingConnectionName = readingConnectionName;
            this.WritingConnectionName = writingConnectionName;
            this.DatabaseProvider = provider;
        }

        public IDatabaseProvider DatabaseProvider { get; }

        public String ReadingConnectionName { get; }

        public String WritingConnectionName { get; }

        public bool Equals(DapperDataSource other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(DatabaseProvider, other.DatabaseProvider) && string.Equals(ReadingConnectionName, other.ReadingConnectionName) && string.Equals(WritingConnectionName, other.WritingConnectionName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DapperDataSource)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (DatabaseProvider != null ? DatabaseProvider.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ReadingConnectionName != null ? ReadingConnectionName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WritingConnectionName != null ? WritingConnectionName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
