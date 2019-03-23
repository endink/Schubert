using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 抽象工厂接口，用于产生数据库连接。
    /// </summary>
    public interface IDatabaseProvider
    {
        /// <summary>
        /// 通过数据库连接字符串创建数据库连接。
        /// </summary>
        /// <param name="connectionString">用于创建数据库连接的字符串。</param>
        /// <returns></returns>
        DbConnection CreateConnection(string connectionString);

        /// <summary>
        /// 表示主体字段的前缀。
        /// </summary>
        string IdentifierPrefix { get; }

        /// <summary>
        /// 表示主体字段的后缀。
        /// </summary>
        string IdentifierStuffix { get; }
        /// <summary>
        /// 参数前缀
        /// </summary>
        string ParameterPrefix { get; }
        /// <summary>
        /// 构造分页查询TSQL语句。
        /// </summary>
        /// <param name="selectSegment">SELECT 语句片段（格式为 SELECT Column1 Column2 FROM Table）</param>
        /// <param name="whereSegment">WHERE 从句片段 （格式为 A=1 AND A=3）</param>
        /// <param name="orderBySegment">ORDER BY 从句片段（格式为 ORDER BY A DESC）</param>
        /// <param name="pageIndex">页索引（从 0 开始）</param>
        /// <param name="pageSize">页尺寸</param>
        /// <returns></returns>
        string BuildPaginationTSql(int pageIndex, int pageSize, string selectSegment, string orderBySegment, string whereSegment = null);

        /// <summary>
        /// 构造批量插入的 SQL 语句。
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columns">列名</param>
        /// <param name="values">表示要插入的数据。</param>
        /// <param name="buildDbParameter">表示用于生成 DB 参数的回掉方法，该方法第一个参数为DB参数名称，第二个参数为DB参数值。</param>
        /// <returns></returns>
        String BuildBatchInsertSql(string tableName, string[] columns, IEnumerable<object[]> values, Action<String, Object> buildDbParameter);

        /// <summary>
        /// 获取最后插入的 ID（通常用于自增长）。
        /// </summary>
        /// <returns></returns>
        object GetLastedInsertId();

        /// <summary>
        /// 获取一个值，表示获取最后插入的数据 Id 是否被支持。
        /// </summary>
        /// <returns></returns>
        bool GetLastedInsertIdSupported { get; }
        //TODO 将此处修改为方法,因为oraclde不支持一种特定的id写法

        /// <summary>
        /// 获取一个值，指示数据提供程序是否支持生成批量插入操作的 SQL 语句。
        /// </summary>
        bool BuildBatchInsertSqlSupported { get; }
        
    }

    public static class DatabaseProviderExtensions
    {

        public static string DelimitIdentifier(this IDatabaseProvider provider, string identifier) =>
            $"{provider.IdentifierPrefix}{identifier}{provider.IdentifierStuffix}";

        public static string DelimitParameter(this IDatabaseProvider provider, string parameter) =>
                 $"{provider.ParameterPrefix}{parameter}";
    }

}
