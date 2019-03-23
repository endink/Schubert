using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Test.Dapper
{
    static class MySqlConnectionString
    {
        public readonly static string Value = @"
                server=10.66.2.33;
                Port=3306;
                user id=root;
                password=setpay@123;
                database=test;
                Connection Timeout=180; 
                Charset=utf8;
                Pooling=true;
                sslmode=none;
                Min Pool Size=30;
                Max Pool Size=50;";
    }
}
