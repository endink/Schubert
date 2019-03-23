using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Schubert.Framework.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 表示一个数据库操作上下文接口。
    /// </summary>
    public interface INewDatabaseFlag
    {
        bool IsNew { get; set; }
    }
}
