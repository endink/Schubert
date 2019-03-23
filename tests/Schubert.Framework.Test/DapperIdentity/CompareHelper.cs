using Microsoft.AspNetCore.Identity;
using Schubert.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test.DapperIdentity
{
    static class CompareHelper
    {
        public static void Compare(DbDataReader dataReader, UserBase entity)
        {
            Assert.Equal(dataReader["id"].ToString(), entity.Id.ToString());
            Assert.Equal(dataReader["access_failed_count"].ToString(), entity.AccessFailedCount.ToString());
            Assert.Equal(dataReader["email"].ToString(), entity.Email.ToString());
            Assert.Equal(Convert.ToBoolean(dataReader["email_confirmed"]), entity.EmailConfirmed);
            Assert.Equal(dataReader["language"].ToString(), entity.Language.ToString());
            Assert.Equal(Convert.ToBoolean(dataReader["lockout_enabled"]), entity.LockoutEnabled);
            Assert.Equal(((DateTime?)dataReader["lockout_end"])?.ToString("yyyy-MM-dd HH:mm:ss"), entity.LockoutEnd?.ToString("yyyy-MM-dd HH:mm:ss"));
            Assert.Equal(dataReader["normalized_user_name"].ToString(), entity.NormalizedUserName.ToString());
            Assert.Equal(dataReader["password_hash"].ToString(), entity.PasswordHash);
            Assert.Equal(dataReader["phone_number"].ToString(), entity.PhoneNumber.ToString());
            Assert.Equal(Convert.ToBoolean(dataReader["phone_number_confirmed"]), entity.PhoneNumberConfirmed);
            Assert.Equal(dataReader["security_stamp"].ToString(), entity.SecurityStamp.ToString());
            Assert.Equal(dataReader["time_zone"].ToString(), entity.TimeZone.ToString());
            Assert.Equal(dataReader["user_name"].ToString(), entity.UserName.ToString());
        }
        public static void Compare(UserBase ub1, UserBase ub2)
        {
            Assert.Equal(ub1.Id, ub2.Id);
            Assert.Equal(ub1.AccessFailedCount, ub2.AccessFailedCount);
            Assert.Equal(ub1.Email, ub2.Email.ToString());
            Assert.Equal(ub1.EmailConfirmed, ub2.EmailConfirmed);
            Assert.Equal(ub1.Language, ub2.Language);
            Assert.Equal(ub1.LockoutEnabled, ub2.LockoutEnabled);
            Assert.Equal(ub1.LockoutEnd?.ToString("yyyy-MM-dd HH:mm:ss"), ub2.LockoutEnd?.ToString("yyyy-MM-dd HH:mm:ss"));
            Assert.Equal(ub1.NormalizedUserName, ub2.NormalizedUserName);
            Assert.Equal(ub1.PasswordHash, ub2.PasswordHash);
            Assert.Equal(ub1.PhoneNumber, ub2.PhoneNumber);
            Assert.Equal(ub1.PhoneNumberConfirmed, ub2.PhoneNumberConfirmed);
            Assert.Equal(ub1.SecurityStamp, ub2.SecurityStamp);
            Assert.Equal(ub1.TimeZone, ub2.TimeZone);
            Assert.Equal(ub1.UserName, ub2.UserName);
        }
        public static void Compare(IList<UserLoginInfo> lists1, IList<UserLoginInfo> lists2)
        {
            Assert.Equal(lists1.Count, lists2.Count);
            foreach (UserLoginInfo ui in lists1)
            {
                int count = lists2.Where(s => s.LoginProvider == ui.LoginProvider && s.ProviderDisplayName == ui.ProviderDisplayName && s.ProviderKey == ui.ProviderKey).Count();
                Assert.Equal(count, 1);
            }
        }

    }
}
