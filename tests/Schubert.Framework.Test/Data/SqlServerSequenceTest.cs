using Microsoft.Extensions.Options;
using Moq;
using Schubert.Framework.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test.Data
{
    //public class SqlServerSequenceTest
    //{
    //    public SqlServerSequence CreateSequence(string connectionString = null)
    //    {
    //        var mock = new Mock<IOptions<SchubertDataOptions>>();
    //        mock.Setup(op => op.Value).Returns(new SchubertDataOptions()
    //        {
    //            DbConnectionString = connectionString ?? "Server=(localdb)\\mssqllocaldb;Database=Labijie;Trusted_Connection=True;MultipleActiveResultSets=true",
    //            SequenceNameFormat = "{0}Seq"
    //        });
    //        SqlServerSequence sq = new SqlServerSequence(mock.Object);
    //        return sq;
    //    }

    //    [Fact]
    //    public void Test()
    //    {
    //        using (SqlConnection conn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=Labijie;Trusted_Connection=True;MultipleActiveResultSets=true"))
    //        {
    //            using (var cmd = conn.CreateCommand())
    //            {
    //                cmd.CommandText = @"IF EXISTS (SELECT [NAME] FROM sys.sequences WHERE NAME ='TestSeq')
    //                                                        BEGIN
    //                                                        Drop SEQUENCE [TestSeq]
    //                                                        END
    //                                                        CREATE SEQUENCE [TestSeq]
    //                                                            AS BIGINT
    //                                                            START WITH 1
    //                                                            INCREMENT BY 1
    //                                                            NO CYCLE
    //                                                            CACHE 10"
    //                                                        ;
    //                conn.Open();
    //                cmd.ExecuteNonQuery();
    //            }
    //        }

    //        SqlServerSequence sq = this.CreateSequence();
    //        var v = sq.NextInt64ValueAsync("Test").GetAwaiter().GetResult();
    //        Assert.Equal(1L, v);
    //        var v2 = sq.NextInt64ValueAsync("Test").GetAwaiter().GetResult();
    //        Assert.Equal(2L, v2);
    //    }
    //}
}
