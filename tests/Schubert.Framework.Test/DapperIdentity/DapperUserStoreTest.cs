using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MySql.Data.MySqlClient;
using Schubert.Framework.Data;
using Schubert.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using System.Linq;
using System.Security.Claims;
using Schubert.Framework.Environment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Schubert.Framework.Test.DapperIdentity
{
    [Collection("dapper")]
    [CollectionDefinition("dapper", DisableParallelization = true)]
    public class DapperUserStoreTest : IDisposable
    {
        private MySQLHelper helper = MySQLHelper.Default;
        private DapperContext context1;
        private DapperContext context2;
        private DapperContext context3;
        private DapperContext context4;
        private DapperContext context5;
        private DapperContext context6;
        private DapperUserStoreImp _dapper;
        public DapperUserStoreTest()
        {
            Tools.MappingStrategyHelper.SetStrategy(IdentifierMappingStrategy.Underline);
            helper.CreateDataBaseIfNoExist("test");
            helper.CreateTableIfNoExist(new[] { "user_base", "user_token", "user_login", "user_claim", "role_base", "user_role", "role_claim" });
            _dapper = MockGenerator();
        }
        private UserBase CreateUserBase(int id = 3, string Email = "234", string PhoneNumber = "234", string UserName = "234")
        {
            var ub = new UserBase
            {
                Id = id,
                AccessFailedCount = 1,
                Email = Email,
                EmailConfirmed = true,
                Language = "234",
                LockoutEnabled = true,
                LockoutEnd = new DateTime(2016, 12, 5, 4, 4, 4),
                NormalizedUserName = "234",
                PasswordHash = "234",
                PhoneNumber = PhoneNumber,
                PhoneNumberConfirmed = true,
                SecurityStamp = "234",
                TimeZone = "234",
                UserName = UserName
            };
            string sql =
                   @"insert into user_base(id,access_failed_count,email,email_confirmed,language,lockout_enabled,lockout_end,
                    normalized_user_name,password_hash,phone_number,phone_number_confirmed,security_stamp,time_zone,user_name) value 
                    (@id,@AccessFailedCount,@Email,@EmailConfirmed,@Language,@LockoutEnabled,@LockoutEnd,
                    @NormalizedUserName,@PasswordHash,@PhoneNumber,@PhoneNumberConfirmed,@SecurityStamp,@TimeZone,@UserName)";
            var result = helper.ExecuteSQL(sql, command =>
              {
                  command.Parameters.AddWithValue("?id", ub.Id);
                  command.Parameters.AddWithValue("AccessFailedCount", ub.AccessFailedCount);
                  command.Parameters.AddWithValue("Email", ub.Email);
                  command.Parameters.AddWithValue("EmailConfirmed", ub.EmailConfirmed);
                  command.Parameters.AddWithValue("Language", ub.Language);
                  command.Parameters.AddWithValue("LockoutEnabled", ub.LockoutEnabled);
                  command.Parameters.AddWithValue("LockoutEnd", ub.LockoutEnd);
                  command.Parameters.AddWithValue("NormalizedUserName", ub.NormalizedUserName);
                  command.Parameters.AddWithValue("PasswordHash", ub.PasswordHash);
                  command.Parameters.AddWithValue("PhoneNumber", ub.PhoneNumber);
                  command.Parameters.AddWithValue("PhoneNumberConfirmed", ub.PhoneNumberConfirmed);
                  command.Parameters.AddWithValue("SecurityStamp", ub.SecurityStamp);
                  command.Parameters.AddWithValue("TimeZone", ub.TimeZone);
                  command.Parameters.AddWithValue("UserName", ub.UserName);
                  return command.ExecuteNonQuery();
              });
            return result > 0 ? ub : null;
        }
        private void ClearUserBase() => helper.ExecuteSQL("delete from user_base");
        private void DeleteUserBaes(UserBase ub) => helper.ExecuteSQL(string.Format(@"delete from test.user_base where id = '{0}'", ub.Id));
        private UserClaim CreateUserClaim(int UserId, int Id, string ClaimType = "ClaimType", string ClaimValue = "ClaimValue")
        {
            var ub = new UserClaim
            {
                ClaimType = ClaimType,
                ClaimValue = ClaimValue,
                Id = Id,
                UserId = UserId
            };
            var sql = string.Format(@"insert into user_claim(claim_type,claim_value,id,user_id) value ('{0}','{1}',{2},{3})", ub.ClaimType, ub.ClaimValue, ub.Id, ub.UserId);
            return helper.ExecuteSQL(sql) > 0 ? ub : null;
        }
        private void ClearUserClaim() => helper.ExecuteSQL("delete from user_claim");
        private void DeleteUserClaim(UserClaim ub) => helper.ExecuteSQL(string.Format(@"delete from user_claim where id = '{0}'", ub.Id));
        private UserLogin CreateUserLogin(int id = 3, string loginProvider = "LoginProvider", string providerDisplayName = "DisplayName", string providerKey = "ProviderKey")
        {
            var ub = new UserLogin
            {
                Id = id,
                LoginProvider = loginProvider,
                ProviderDisplayName = providerDisplayName,
                ProviderKey = providerKey,
                UserId = 3
            };
            string sql = string.Format(@"insert into user_login(id,login_provider,provider_display_name,provider_key,user_id) value ({0},'{1}','{2}','{3}',{4})", ub.Id, ub.LoginProvider, ub.ProviderDisplayName, ub.ProviderKey, ub.UserId);
            return helper.ExecuteSQL(sql) > 0 ? ub : null;
        }
        private void DeleteUseLogin(UserLogin ub) => helper.ExecuteSQL(string.Format(@"delete from test.user_login where id = '{0}'", ub.Id));
        private void ClearUserLogin() => helper.ExecuteSQL("delete from user_login");
        private void ClearUserRole() => helper.ExecuteSQL("delete from user_role");
        private void DeleteUserRole(UserRole ur) => helper.ExecuteSQL(string.Format(@"delete from test.user_role where user_id = '{0}' and role_id = '{1}'", ur.UserId, ur.RoleId));
        private UserRole CreateUserRole(long roleId, int userId, string roleName = "RoleName")
        {
            var ub = new UserRole
            {
                RoleId = roleId,
                RoleName = roleName,
                UserId = userId
            };
            var sql = string.Format(@"insert into user_role(user_id,role_id,role_name) value ({0},{1},'{2}')", ub.UserId, ub.RoleId, ub.RoleName);
            return helper.ExecuteSQL(sql) > 0 ? ub : null;
        }
        private void ClearRoleBase() => helper.ExecuteSQL("delete from role_base");
        private void DeleteRoleBase(RoleBase rb) => helper.ExecuteSQL(string.Format(@"delete from test.role_base where Id = '{0}'", rb.Id));
        private RoleBase CreateRoleBase(int Id)
        {
            var ub = new RoleBase
            {
                DisplayName = "DisplayName",
                Id = Id,
                Name = "RoleName"
            };
            var sql = string.Format(@"insert into role_base(display_name,id,role_name) value ('{0}',{1},'{2}')", ub.DisplayName, ub.Id, ub.Name);
            return helper.ExecuteSQL(sql) > 0 ? ub : null;
        }
        private void ClearUserTokens() => helper.ExecuteSQL("delete from user_token");
        public class TestWorkContext : WorkContext
        {
            private readonly IWorkContextStateProvider _transactionProvider = new TransactionStateProvider();
            private readonly DapperRuntime _runtime = null;
            private DapperContext _dbContext = null;
            private LoggerFactory loggerFactory = new LoggerFactory();
            public TestWorkContext(DapperRuntime runtime)
                : base()
            {
                _runtime = runtime;

                loggerFactory.AddDebug(LogLevel.Trace);
            }

            public override object Resolve(Type type)
            {

                if (typeof(IDatabaseContext).Equals(type))
                {
                    return _dbContext ?? (_dbContext = new DapperContext(_runtime, loggerFactory));
                }
                return null;
            }

            public override object ResolveRequired(Type type)
            {
                if (typeof(IDatabaseContext).Equals(type))
                {
                    return _dbContext ?? (_dbContext = new DapperContext(_runtime, loggerFactory));
                }
                throw new NotSupportedException();
            }

            protected override IEnumerable<IWorkContextStateProvider> GetStateProviders()
            {
                return new IWorkContextStateProvider[]
                {
                    _transactionProvider
                };
            }
        }

        private void StartTestEngine(DapperRuntime runtime)
        {
            ServiceCollection collection = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
            collection.AddSchubertFramework(configuration,
                builder =>
                {
                    builder.AddDapperDataFeature();
                });
            collection.AddSingleton(typeof(DapperRuntime), runtime);
            collection.AddScoped<WorkContext, TestWorkContext>();


            SchubertEngine.Current.Start(collection.BuildServiceProvider());

        }

        private DapperUserStoreImp MockGenerator()
        {
            Mock<IOptions<DapperDatabaseOptions>> optionsMock = new Mock<IOptions<DapperDatabaseOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new DapperDatabaseOptions
            {
                DefaultConnectionName = "default",
                ConnectionStrings = new Dictionary<string, string>
                {
                    { "default", helper.ConnectionString }
                }
            });

            LoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddDebug(LogLevel.Trace);

            DapperRuntime rt = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderUserBase() });
            DapperRepository<UserBase> dpUserBase = new DapperRepository<UserBase>(context1 = new DapperContext(rt, loggerFactory), loggerFactory);
            DapperRuntime rt2 = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderUserClaim() });
            DapperRepository<UserClaim> dpUserClaim = new DapperRepository<UserClaim>(context2 = new DapperContext(rt2, loggerFactory), loggerFactory);
            DapperRuntime rt3 = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderUserLogin() });
            DapperRepository<UserLogin> dpUserLogin = new DapperRepository<UserLogin>(context3 = new DapperContext(rt3, loggerFactory), loggerFactory);
            DapperRuntime rt4 = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderRoleBase() });
            DapperRepository<RoleBase> dpRoleBase = new DapperRepository<RoleBase>(context4 = new DapperContext(rt4, loggerFactory), loggerFactory);
            DapperRuntime rt5 = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderUserToken() });
            DapperRepository<UserToken> dpUserToken = new DapperRepository<UserToken>(context5 = new DapperContext(rt5, loggerFactory), loggerFactory);
            DapperRuntime rt6 = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderUserRole() });
            DapperRepository<UserRole> dpUserRole = new DapperRepository<UserRole>(context6 = new DapperContext(rt6, loggerFactory), loggerFactory);
            return new DapperUserStoreImp(new IIdGenerationServiceImp(), dpUserBase, dpRoleBase, dpUserClaim, dpUserLogin, dpUserToken, dpUserRole);
        }

        #region IUserStore
        [Fact(DisplayName = "DapperUserStore:异步创建UserBases")]
        public void CreateAsync()
        {
            UserBase ub = new UserBase
            {
                Id = 3,
                AccessFailedCount = 1,
                Email = "234",
                EmailConfirmed = true,
                Language = "234",
                LockoutEnabled = true,
                LockoutEnd = new DateTime(2016, 12, 5, 8, 10, 4),
                NormalizedUserName = "234",
                PasswordHash = "234",
                PhoneNumber = "234",
                PhoneNumberConfirmed = true,
                SecurityStamp = "234",
                TimeZone = "234",
                UserName = "234"
            };
            try
            {
                ClearUserBase();
                var dapperInstance = _dapper;
                dapperInstance.CreateAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
                helper.ExecuteReader(string.Format(@"select * from user_base where id = '{0}'", ub.Id), reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, ub);
                    }
                });
            }
            finally
            {
                DeleteUserBaes(ub);
            }
        }

        [Fact(DisplayName = "DapperUserStore:异步删除UserBases")]
        public void DeleteAsync()
        {
            ClearUserBase();
            var ub = CreateUserBase();

            try
            {
                var ins = _dapper;
                ins.DeleteAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
                Assert.Equal(helper.GetValue(string.Format(@"select count(*) from user_base where id = '{0}'", ub.Id), Convert.ToInt32), 0);
            }
            finally
            {
                ClearUserBase();
            }
        }

        [Fact(DisplayName = "DapperUserStore:根据Id查找记录")]
        public void FindByIdAsync()
        {
            ClearUserBase();
            try
            {
                var ub = CreateUserBase();
                var dapperInstance = _dapper;
                var userBase = dapperInstance.FindByIdAsync(ub.Id.ToString(), CancellationToken.None).GetAwaiter().GetResult();
                CompareHelper.Compare(ub, userBase);
            }
            finally
            {
                ClearUserBase();
            }
        }

        [Fact(DisplayName = "DapperUserStore:根据UserName查找记录")]
        public void FindByNameAsync()
        {
            ClearUserBase();
            var ub = CreateUserBase();
            try
            {
                var dapperInstance = _dapper;
                var userBase = dapperInstance.FindByNameAsync(ub.NormalizedUserName, CancellationToken.None).GetAwaiter().GetResult();
                CompareHelper.Compare(ub, userBase);
            }
            finally
            {
                ClearUserBase();
            }
        }

        [Fact(DisplayName = "DapperUserStore:获取UserBase的NormalizedUserName")]
        public void GetNormalizedUserNameAsync()
        {
            try
            {
                var ub = new UserBase() { NormalizedUserName = "234" };
                var dapperInstance = _dapper;
                string NormalizedUserName = dapperInstance.GetNormalizedUserNameAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
                Assert.Equal(ub.NormalizedUserName, NormalizedUserName);
            }
            finally { }
        }

        [Fact(DisplayName = "DapperUserStore:获取UserBase的Id")]
        public void GetUserIdAsync()
        {
            try
            {
                var ub = new UserBase() { Id = 3 };
                var dapperInstance = _dapper;
                string Id = dapperInstance.GetUserIdAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
                Assert.Equal(ub.Id.ToString(), Id);
            }
            finally { }
        }

        [Fact(DisplayName = "DapperUserStore:获取UserBase的UserName")]
        public void GetUserNameAsync()
        {
            try
            {
                var ub = new UserBase() { UserName = "234" };
                var dapperInstance = _dapper;
                string UserName = dapperInstance.GetUserNameAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
                Assert.Equal(ub.UserName, UserName);
            }
            finally { }
        }

        [Fact(DisplayName = "DapperUserStore:更新NormalizedUserName")]
        public void SetNormalizedUserNameAsync()
        {
            ClearUserBase();
            var ub = CreateUserBase();
            try
            {
                var dapperInstance = _dapper;
                dapperInstance.SetNormalizedUserNameAsync(ub, "345", CancellationToken.None).GetAwaiter().GetResult();
                var name = helper.GetValue(string.Format(@"select normalized_user_name from user_base where id = '{0}'", ub.Id), o => o + "");
                Assert.Equal(name, "345");
            }
            finally
            {
                ClearUserBase();
            }
        }

        [Fact(DisplayName = "DapperUserStore:更新UserName")]
        public void SetUserNameAsync()
        {
            ClearUserBase();
            var ub = CreateUserBase();

            try
            {
                var dapperInstance = _dapper;
                dapperInstance.SetUserNameAsync(ub, "codemonk", CancellationToken.None).GetAwaiter().GetResult();
                var name = helper.GetValue(string.Format(@"select user_name from user_base where id = '{0}'", ub.Id), o => o + "");
                Assert.Equal(name, "codemonk");
            }
            finally
            {
                ClearUserBase();
            }
        }

        #endregion

        #region IUserEmailStore

        [Fact(DisplayName = "DapperUserStore:更新userbase")]
        public void UpdateAsync()
        {
            ClearUserBase();
            var ub = CreateUserBase();
            try
            {
                var dapperInstance = _dapper;
                ub.UserName = "345";
                ub.LockoutEnd = new DateTime(2016, 12, 12, 4, 4, 4);
                dapperInstance.UpdateAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
                helper.ExecuteReader(string.Format(@"select * from user_base where id = '{0}'", ub.Id), reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, ub);
                    }
                });
            }
            finally
            {
                ClearUserBase();
            }
        }

        [Fact(DisplayName = "DapperUserStore:更新userbase的Email")]
        public void SetEmailAsync()
        {
            ClearUserBase();
            UserBase ub = CreateUserBase();
            try
            {
                var dapperInstance = _dapper;
                dapperInstance.SetEmailAsync(ub, "codemonk@live.cn", CancellationToken.None).GetAwaiter().GetResult();
                var email = helper.GetValue(string.Format(@"select email from user_base where id = '{0}'", ub.Id), s => s + "");
                Assert.Equal(email, "codemonk@live.cn");
            }
            finally
            {
                DeleteUserBaes(ub);
            }
        }

        [Fact(DisplayName = "DapperUserStore:获取userbase的Email")]
        public void GetEmailAsync()
        {
            UserBase ub = new UserBase() { Email = "codemonk@live.cn" };
            var dapperInstance = _dapper;
            string email = dapperInstance.GetEmailAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
            Assert.Equal(ub.Email, email);
        }

        [Fact(DisplayName = "DapperUserStore:获取userbase的EmailConfirmed")]
        public void GetEmailConfirmedAsync()
        {
            UserBase ub = new UserBase() { EmailConfirmed = false };
            var dapperInstance = _dapper;
            bool EmailConfirmed = dapperInstance.GetEmailConfirmedAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
            Assert.Equal(ub.EmailConfirmed, EmailConfirmed);
        }

        [Fact(DisplayName = "DapperUserStore:更新userbase的EmailConfirmed")]
        public void SetEmailConfirmedAsync()
        {
            ClearUserBase();
            UserBase ub = CreateUserBase();
            try
            {
                var value = false;
                var dapperInstance = _dapper;
                dapperInstance.SetEmailConfirmedAsync(ub, value, CancellationToken.None).GetAwaiter().GetResult();
                var confirmed = helper.GetValue(string.Format(@"select email_confirmed from user_base where id = '{0}'", ub.Id), Convert.ToBoolean);
                Assert.Equal(confirmed, value);
            }
            finally
            {
                DeleteUserBaes(ub);
            }
        }


        [Fact(DisplayName = "DapperUserStore:根据Email查找记录")]
        public void FindByEmailAsync()
        {
            ClearUserBase();
            var ub = CreateUserBase();
            try
            {
                var dapperInstance = _dapper;
                UserBase userBase = dapperInstance.FindByEmailAsync(ub.Email, CancellationToken.None).GetAwaiter().GetResult();
                CompareHelper.Compare(ub, userBase);
            }
            finally
            {
                DeleteUserBaes(ub);
            }
        }

        [Fact(DisplayName = "DapperUserStore:获取userbase的EmailConfirmed")]
        public void GetNormalizedEmailAsync()
        {
            UserBase ub = new UserBase() { Email = "codemonk@live.cn" };
            var dapperInstance = _dapper;
            string email = dapperInstance.GetNormalizedEmailAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
            Assert.Equal(ub.Email, email);
        }

        #endregion

        #region IUserLoginStore

        [Fact(DisplayName = "DapperUserStore:添加userlogins")]
        public void AddLoginAsync()
        {
            var ub = new UserBase
            {
                Id = 3,
                AccessFailedCount = 1,
                Email = "234",
                EmailConfirmed = true,
                Language = "234",
                LockoutEnabled = true,
                //LockoutEnd = new DateTime(2016,12,5,8,10,4),
                LockoutEnd = DateTime.Now,
                NormalizedUserName = "234",
                PasswordHash = "234",
                PhoneNumber = "234",
                PhoneNumberConfirmed = true,
                SecurityStamp = "234",
                TimeZone = "234",
                UserName = "234"
            };
            try
            {
                UserLoginInfo ui = new UserLoginInfo("loginProvider", "providerKey", "displayName");
                var instance = _dapper;
                instance.AddLoginAsync(ub, ui, CancellationToken.None).GetAwaiter().GetResult();
                helper.ExecuteReader(@"select * from user_login ORDER BY id DESC LIMIT 1", reader =>
                {
                    while (reader.Read())
                    {
                        Assert.Equal(reader["login_provider"].ToString(), "loginProvider");
                        Assert.Equal(reader["provider_key"].ToString(), "providerKey");
                        Assert.Equal(reader["provider_display_name"].ToString(), "displayName");
                        Assert.Equal(reader["user_id"].ToString(), ub.Id.ToString());
                    }
                });
            }
            finally
            {
                DeleteUserBaes(ub);
            }
        }

        [Fact(DisplayName = "DapperUserStore:删除userlogins")]
        public void RemoveLoginAsync()
        {
            var ub = new UserBase() { Id = 3 };
            ClearUserLogin();
            var ui = CreateUserLogin();
            try
            {
                var instance = _dapper;
                instance.RemoveLoginAsync(ub, "LoginProvider", "ProviderKey", CancellationToken.None).GetAwaiter().GetResult();
                var count = helper.GetValue(string.Format(@"select count(*) from user_login where id = '{0}'", ui.Id), Convert.ToInt32);
                Assert.Equal(count, 0);
            }
            finally
            {
                DeleteUseLogin(ui);
            }
        }

        [Fact(DisplayName = "DapperUserStore:获取GetLogins")]
        public void GetLoginsAsync()
        {
            var ub = new UserBase() { Id = 3 };
            ClearUserLogin();
            var lists = new List<UserLoginInfo>();
            var ui = CreateUserLogin();
            var ui2 = CreateUserLogin(4, "4", "4", "4");
            var ui3 = CreateUserLogin(5, "5", "5", "5");
            try
            {
                lists.Add(new UserLoginInfo(ui.LoginProvider, ui.ProviderKey, ui.ProviderDisplayName));
                lists.Add(new UserLoginInfo(ui2.LoginProvider, ui2.ProviderKey, ui2.ProviderDisplayName));
                lists.Add(new UserLoginInfo(ui3.LoginProvider, ui3.ProviderKey, ui3.ProviderDisplayName));
                var ins = _dapper;
                IList<UserLoginInfo> lists2 = ins.GetLoginsAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
                CompareHelper.Compare(lists, lists2);
            }
            finally
            {
                DeleteUseLogin(ui);
                DeleteUseLogin(ui2);
                DeleteUseLogin(ui3);
            }
        }

        [Fact(DisplayName = "DapperUserStore:通过UserLogins获取UserBase")]
        public void FindByLoginAsync()
        {
            ClearUserLogin();
            ClearUserBase();
            var ub = CreateUserBase();
            var ui = CreateUserLogin(3, "4", "4", "4");

            try
            {
                var ub2 = _dapper.FindByLoginAsync("4", "4", CancellationToken.None).GetAwaiter().GetResult();
                CompareHelper.Compare(ub, ub2);
            }
            finally
            {
                DeleteUserBaes(ub);
                DeleteUseLogin(ui);
            }
        }

        #endregion

        #region IUserLockupStore

        [Fact(DisplayName = "DapperUserStore:添加userrole")]
        public void AddToRoleAsync()
        {
            ClearUserRole();
            ClearRoleBase();
            ClearUserBase();


            var ub = CreateUserBase();
            var rb = CreateRoleBase(2);

            try
            {
                var ins = _dapper;
                ins.AddToRoleAsync(ub, rb.Name, CancellationToken.None).GetAwaiter().GetResult();

                UserRole userRole = new UserRole();
                userRole.UserId = ub.Id;
                userRole.RoleId = rb.Id;
                userRole.RoleName = rb.Name;
                var sql = string.Format(@"select * from user_role where user_id = '{0}' and role_id = '{1}'", ub.Id, rb.Id);
                helper.ExecuteReader(sql, reader =>
                {
                    while (reader.Read())
                    {
                        Assert.Equal(reader["user_id"].ToString(), ub.Id.ToString());
                        Assert.Equal(reader["role_id"].ToString(), rb.Id.ToString());
                        Assert.Equal(reader["role_name"].ToString(), rb.Name.ToString());
                    }
                });
            }
            finally
            {
                DeleteRoleBase(rb);
                DeleteUserBaes(ub);
                DeleteUserRole(new UserRole
                {
                    RoleId = rb.Id,
                    UserId = ub.Id
                });
            }

        }

        [Fact(DisplayName = "DapperUserStore:删除userrole")]
        public void RemoveFromRoleAsync()
        {
            ClearUserRole();
            ClearRoleBase();

            var rb = CreateRoleBase(2);
            CreateUserRole(rb.Id, 3);

            try
            {
                var ins = _dapper;
                ins.RemoveFromRoleAsync(new UserBase { Id = 3 }, rb.Name, CancellationToken.None).GetAwaiter().GetResult();
                var count = helper.GetValue(string.Format(@"select count(*) from user_role where user_id='{0}' and role_id='{1}'", 3, rb.Id), Convert.ToInt32);
                Assert.Equal(0, count);
            }
            finally
            {
                DeleteUserRole(new UserRole { RoleId = rb.Id, UserId = 3 });
                DeleteRoleBase(rb);
            }

        }

        [Fact(DisplayName = "DapperUserStore:通过userid找roles")]
        public void GetRolesAsync()
        {
            ClearUserRole();
            UserRole ur1 = CreateUserRole(2, 2, "rolename2");
            UserRole ur2 = CreateUserRole(3, 2, "rolename3");
            try
            {
                var ins = _dapper;
                IList<string> urs = ins.GetRolesAsync(new UserBase { Id = 2 }, CancellationToken.None).GetAwaiter().GetResult();
                Assert.Equal(urs.Count, 2);
                int count1 = urs.Where(s => s == "rolename2").Count();
                int count2 = urs.Where(s => s == "rolename3").Count();
                Assert.Equal(count1, 1);
                Assert.Equal(count2, 1);
            }
            finally
            {
                DeleteUserRole(ur1);
                DeleteUserRole(ur2);
            }
        }

        [Fact(DisplayName = "DapperUserStore:RoleName是否在UserRoles中")]
        public void IsInRoleAsync()
        {
            ClearUserRole();
            UserRole ur1 = CreateUserRole(2, 2, "rolename2");
            UserRole ur2 = CreateUserRole(3, 2, "rolename3");
            try
            {
                var ins = _dapper;
                bool urs = ins.IsInRoleAsync(new UserBase { Id = 2 }, ur1.RoleName, CancellationToken.None).GetAwaiter().GetResult();
                Assert.True(urs);
            }
            finally
            {
                DeleteUserRole(ur1);
                DeleteUserRole(ur2);
            }
        }

        [Fact(DisplayName = "DapperUserStore:根据rolename查找userbases")]
        public void GetUsersInRoleAsync()
        {
            ClearUserRole();
            ClearUserBase();

            UserRole ur1 = CreateUserRole(2, 2, "rolename");
            UserRole ur2 = CreateUserRole(2, 3, "rolename");

            UserBase ub1 = CreateUserBase(2, "123", "123", "123");
            UserBase ub2 = CreateUserBase(3);
            try
            {
                var ins = _dapper;
                IList<UserBase> ubs = ins.GetUsersInRoleAsync("rolename", CancellationToken.None).GetAwaiter().GetResult();
                UserBase ub3 = ubs.Where(s => s.Id == ub1.Id).FirstOrDefault();
                UserBase ub4 = ubs.Where(s => s.Id == ub2.Id).FirstOrDefault();
                CompareHelper.Compare(ub1, ub3);
                CompareHelper.Compare(ub2, ub4);
            }
            finally
            {
                DeleteUserRole(ur1);
                DeleteUserRole(ur2);
                DeleteUserBaes(ub1);
                DeleteUserBaes(ub2);
            }

        }
        #endregion

        #region IUserClaimStore

        [Fact(DisplayName = "DapperUserStore: 通过User获取Claims")]
        public void GetClaimsAsync()
        {
            ClearUserClaim();
            UserClaim uc1 = CreateUserClaim(3, 1, "1", "1");
            UserClaim uc2 = CreateUserClaim(3, 2, "2", "2");
            try
            {
                var ins = _dapper;
                IList<Claim> lists = ins.GetClaimsAsync(new UserBase { Id = 3 }, CancellationToken.None).GetAwaiter().GetResult();
                int count1 = lists.Where(s => s.Type == "1" && s.Value == "1").Count();
                int count2 = lists.Where(s => s.Type == "2" && s.Value == "2").Count();
                Assert.Equal(count1, 1);
                Assert.Equal(count2, 1);
            }
            finally
            {
                DeleteUserClaim(uc1);
                DeleteUserClaim(uc2);
            }
        }

        [Fact(DisplayName = "DapperUserStore: 添加Claims")]
        public void AddClaimsAsync()
        {
            ClearUserClaim();
            try
            {
                Mock<IOptions<DapperDatabaseOptions>> optionsMock = new Mock<IOptions<DapperDatabaseOptions>>();
                optionsMock.Setup(o => o.Value).Returns(new DapperDatabaseOptions
                {
                    DefaultConnectionName = "default",
                    ConnectionStrings = new Dictionary<string, string>
                    {
                        {"default", helper.ConnectionString}
                    }
                });

                DapperRuntime rt = new DapperRuntime(optionsMock.Object,
                    new IDapperMetadataProvider[] { new DapperMetadataProviderUserClaim() });
                StartTestEngine(rt);

                IList<Claim> cs = new List<Claim>();
                Claim c1 = new Claim("1", "1");
                Claim c2 = new Claim("2", "2");
                cs.Add(c1);
                cs.Add(c2);
                var ins = _dapper;
                ins.AddClaimsAsync(new UserBase { Id = 3 }, cs, CancellationToken.None).GetAwaiter().GetResult();
                helper.ExecuteReader(@"select * from user_claim", reader =>
                {
                    while (reader.Read())
                    {
                        if (reader["claim_type"].ToString() == "1")
                        {
                            Assert.Equal(reader["claim_value"].ToString(), "1");
                        }
                        else if (reader["claim_type"].ToString() == "2")
                        {
                            Assert.Equal(reader["claim_value"].ToString(), "2");
                        }
                        else
                        {
                            Assert.Equal("1", "0");
                        }
                    }
                });
            }
            catch (Exception e)
            {
                //TODO context
                Assert.NotNull(e as InvalidOperationException);
            }
            finally
            {
                ClearUserClaim();
            }
        }

        [Fact(DisplayName = "DapperUserStore: 更新Claims")]
        public void ReplaceClaimAsync()
        {
            ClearUserClaim();
            UserClaim uc1 = CreateUserClaim(3, 1, "1", "1");
            UserClaim uc2 = CreateUserClaim(3, 2, "2", "2");
            try
            {
                var ins = _dapper;
                ins.ReplaceClaimAsync(new UserBase { Id = 3 }, new Claim("1", "1"), new Claim("3", "3"), CancellationToken.None).GetAwaiter().GetResult();
                var value = helper.GetValue(string.Format(@"select claim_value from user_claim where id = {0}", uc1.Id), s => s + "");
                Assert.Equal(value, "3");
            }
            finally
            {
                ClearUserClaim();
            }
        }

        [Fact(DisplayName = "DapperUserStore: 删除Claims")]
        public void RemoveClaimsAsync()
        {
            ClearUserClaim();
            var uc1 = CreateUserClaim(3, 1, "1", "1");
            var uc2 = CreateUserClaim(3, 2, "2", "2");
            var uc3 = CreateUserClaim(3, 3, "3", "3");

            Mock<IOptions<DapperDatabaseOptions>> optionsMock = new Mock<IOptions<DapperDatabaseOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new DapperDatabaseOptions
            {
                DefaultConnectionName = "default",
                ConnectionStrings = new Dictionary<string, string>
                {
                    { "default", helper.ConnectionString }
                }
            });

            DapperRuntime rt = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderUserClaim() });
            StartTestEngine(rt);

            try
            {
                IList<Claim> cs = new List<Claim>();
                cs.Add(new Claim("1", "1"));
                cs.Add(new Claim("2", "2"));
                var ins = _dapper;
                ins.RemoveClaimsAsync(new UserBase { Id = 3 }, cs, CancellationToken.None).GetAwaiter().GetResult();
                var count = helper.GetValue(string.Format("select count(*) from user_claim where user_id={0}", 3), Convert.ToInt32);
                Assert.Equal(count, 1);
            }
            catch (Exception e)
            {
                //TODO context
                Assert.NotNull(e as InvalidOperationException);
            }
            finally
            {
                ClearUserClaim();
            }
        }

        [Fact(DisplayName = "DapperUserStore:根据Claims查找userbase")]
        public void GetUsersForClaimAsync()
        {
            ClearUserBase();
            ClearUserClaim();
            var ub = CreateUserBase();
            var uc = CreateUserClaim((int)(ub.Id), 1, "1", "1");
            try
            {
                var ins = _dapper;
                var ubs = ins.GetUsersForClaimAsync(new Claim("1", "1"), CancellationToken.None).GetAwaiter().GetResult();
                Assert.Equal(ubs.Count, 1);
                CompareHelper.Compare(ub, ubs[0]);
            }
            finally
            {
                ClearUserBase();
                ClearUserClaim();
            }
        }
        #endregion

        #region IUserPasswordStore

        [Fact(DisplayName = "DapperUserStore:修改PasswordHash")]
        public void SetPasswordHashAsync()
        {
            ClearUserBase();
            try
            {
                UserBase ub = CreateUserBase();
                var ins = _dapper;
                ins.SetPasswordHashAsync(ub, "123test", CancellationToken.None);
                var hash = helper.GetValue(string.Format("select password_hash from user_base where id = {0}", ub.Id), s => s + "");
                Assert.Equal(hash, "123test");
            }
            finally
            {
                ClearUserBase();
            }
        }

        [Fact(DisplayName = "DapperUserStore:获取PasswordHash")]
        public void GetPasswordHashAsync()
        {
            var ub = new UserBase
            {
                Id = 3,
                AccessFailedCount = 1,
                Email = "Email",
                EmailConfirmed = true,
                Language = "234",
                LockoutEnabled = true,
                LockoutEnd = new DateTime(2016, 12, 5, 4, 4, 4),
                NormalizedUserName = "234",
                PasswordHash = "234",
                PhoneNumber = "PhoneNumber",
                PhoneNumberConfirmed = true,
                SecurityStamp = "234",
                TimeZone = "234",
                UserName = "UserName"
            };
            var ins = _dapper;
            string PasswordHash = ins.GetPasswordHashAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
            Assert.Equal(PasswordHash, ub.PasswordHash);
        }

        [Fact(DisplayName = "DapperUserStore:是否有PasswordHash")]
        public void HasPasswordAsync()
        {
            var ub = new UserBase
            {
                Id = 3,
                AccessFailedCount = 1,
                Email = "Email",
                EmailConfirmed = true,
                Language = "234",
                LockoutEnabled = true,
                LockoutEnd = new DateTime(2016, 12, 5, 4, 4, 4),
                NormalizedUserName = "234",
                PasswordHash = "234",
                PhoneNumber = "PhoneNumber",
                PhoneNumberConfirmed = true,
                SecurityStamp = "234",
                TimeZone = "234",
                UserName = "UserName"
            };
            var ins = _dapper;
            bool result1 = ins.HasPasswordAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
            Assert.Equal(result1, true);

            var ub2 = new UserBase
            {
                Id = 3,
                AccessFailedCount = 1,
                Email = "Email",
                EmailConfirmed = true,
                Language = "234",
                LockoutEnabled = true,
                LockoutEnd = new DateTime(2016, 12, 5, 4, 4, 4),
                NormalizedUserName = "234",
                PhoneNumber = "PhoneNumber",
                PhoneNumberConfirmed = true,
                SecurityStamp = "234",
                TimeZone = "234",
                UserName = "UserName"
            };
            bool result2 = ins.HasPasswordAsync(ub2, CancellationToken.None).GetAwaiter().GetResult();
            Assert.Equal(result2, false);
        }
        #endregion

        #region IUserSecurityStampStore

        [Fact(DisplayName = "DapperUserStore:修改userbase的SecurityStamp")]
        public void SetSecurityStampAsync()
        {
            ClearUserBase();
            UserBase ub = CreateUserBase();

            try
            {
                var ins = _dapper;
                ins.SetSecurityStampAsync(ub, "stamptest", CancellationToken.None).GetAwaiter().GetResult();
                var stamp = helper.GetValue(string.Format("select security_stamp from user_base where id = {0}", ub.Id), s => s + "");
                Assert.Equal(stamp, "stamptest");
            }
            finally
            {
                ClearUserBase();
            }

        }
        [Fact(DisplayName = "DapperUserStore:获取userbase的SecurityStamp")]
        public void GetSecurityStampAsync()
        {
            var ub = new UserBase
            {
                Id = 3,
                AccessFailedCount = 1,
                Email = "Email",
                EmailConfirmed = true,
                Language = "234",
                LockoutEnabled = true,
                LockoutEnd = new DateTime(2016, 12, 5, 4, 4, 4),
                NormalizedUserName = "234",
                PasswordHash = "234",
                PhoneNumber = "PhoneNumber",
                PhoneNumberConfirmed = true,
                SecurityStamp = "234",
                TimeZone = "234",
                UserName = "UserName"
            };

            var ins = _dapper;
            string SecurityStamp = ins.GetSecurityStampAsync(ub, CancellationToken.None).GetAwaiter().GetResult();
            Assert.Equal(SecurityStamp, ub.SecurityStamp);
        }
        #endregion

        #region IUserLockoutStore
        [Fact(DisplayName = "DapperUserStore:设置用户锁定结束时间")]
        private void SetLockoutEndDateAsync()
        {
            ClearUserBase();
            UserBase ub = CreateUserBase();
            try
            {
                var ins = _dapper;
                ub.LockoutEnd = new DateTime(2016, 12, 12, 4, 4, 4, DateTimeKind.Utc);
                ins.SetLockoutEndDateAsync(ub, ub.LockoutEnd, CancellationToken.None).GetAwaiter().GetResult();
                helper.ExecuteReader(string.Format("select * from user_base where id = {0}", ub.Id), reader =>
                {
                    while (reader.Read())
                    {
                        CompareHelper.Compare(reader, ub);
                    }
                });
            }
            finally
            {
                ClearUserBase();
            }
        }

        [Fact(DisplayName = "DapperUserStore:获取用户锁定结束时间")]
        private void GetLockoutEndDateAsync()
        {
            ClearUserBase();

            var userId = 666;
            var offset = DateTime.UtcNow;

            var insertSql = $@"
INSERT INTO 
  test.user_base (  
  id,
  access_failed_count, 
  email, 
  email_confirmed, 
  language, 
  lockout_enabled, 
  lockout_end, 
  normalized_user_name, 
  password_hash, 
  phone_number, 
  phone_number_confirmed, 
  security_stamp, 
  time_zone, 
  user_name)
  VALUES(
   {userId},
   1,
   '666@qq.com',
   TRUE,
   'zh-Hans',
   TRUE,
   '{offset}',
   'ddd',
   '234',
   '13888888888',
   TRUE,
   'abc1234',
   'china stardand time',
   'ddd'
  );";

            helper.ExecuteSQL(insertSql);

            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId };

            mockInstance.GetLockoutEnabledAsync(user, CancellationToken.None).GetAwaiter().GetResult();
            var lockoutEnd = helper.GetValue(string.Format("select lockout_end from user_base where id = '{0}'", userId), s => s + "");
            Assert.Equal(lockoutEnd, offset.ToString());
            ClearUserBase();
        }

        [Fact(DisplayName = "DapperUserStore:异步增加用户登录失败次数")]
        private void IncrementAccessFailedCountAsync()
        {
            ClearUserBase();

            var userId = 666;
            var offset = DateTime.UtcNow;
            var fieldCount = 1;
            var insertSql = $@"
INSERT INTO 
  test.user_base (  
  id,
  access_failed_count, 
  email, 
  email_confirmed, 
  language, 
  lockout_enabled, 
  lockout_end, 
  normalized_user_name, 
  password_hash, 
  phone_number, 
  phone_number_confirmed, 
  security_stamp, 
  time_zone, 
  user_name)
  VALUES(
   {userId},
   {fieldCount},
   '666@qq.com',
   TRUE,
   'zh-Hans',
   TRUE,
   '{offset}',
   'ddd',
   '234',
   '13888888888',
   TRUE,
   'abc1234',
   'china stardand time',
   'ddd'
  );";

            helper.ExecuteSQL(insertSql);

            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId, AccessFailedCount = fieldCount };

            mockInstance.IncrementAccessFailedCountAsync(user, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from user_base where id = '{0}'", userId), reader =>
            {
                while (reader.Read())
                {
                    var targetCount = fieldCount + 1;
                    Assert.Equal(reader["access_failed_count"].ToString(), targetCount.ToString());
                }
            });
            ClearUserBase();
        }

        [Fact(DisplayName = "DapperUserStore:异步重置用户登录失败次数")]
        private void ResetAccessFailedCountAsync()
        {
            ClearUserBase();
            var userId = 666;
            var offset = DateTime.UtcNow;
            var fieldCount = 1;
            var insertSql = $@"
INSERT INTO 
  test.user_base (  
  id,
  access_failed_count, 
  email, 
  email_confirmed, 
  language, 
  lockout_enabled, 
  lockout_end, 
  normalized_user_name, 
  password_hash, 
  phone_number, 
  phone_number_confirmed, 
  security_stamp, 
  time_zone, 
  user_name)
  VALUES(
   {userId},
   {fieldCount},
   '666@qq.com',
   TRUE,
   'zh-Hans',
   TRUE,
   '{offset}',
   'ddd',
   '234',
   '13888888888',
   TRUE,
   'abc1234',
   'china stardand time',
   'ddd'
  );";

            helper.ExecuteSQL(insertSql);

            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId };

            mockInstance.ResetAccessFailedCountAsync(user, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from user_base where id = '{0}'", userId), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["access_failed_count"].ToString(), "0");
                }
            });
            ClearUserBase();
        }

        [Fact(DisplayName = "DapperUserStore:异步获取用户登录失败次数")]
        private void GetAccessFailedCountAsync()
        {
            ClearUserBase();
            var userId = 666;
            var offset = DateTime.UtcNow;
            var fieldCount = 3;
            var insertSql = $@"
INSERT INTO 
  test.user_base (  
  id,
  access_failed_count, 
  email, 
  email_confirmed, 
  language, 
  lockout_enabled, 
  lockout_end, 
  normalized_user_name, 
  password_hash, 
  phone_number, 
  phone_number_confirmed, 
  security_stamp, 
  time_zone, 
  user_name)
  VALUES(
   {userId},
   {fieldCount},
   '666@qq.com',
   TRUE,
   'zh-Hans',
   TRUE,
   '{offset}',
   'ddd',
   '234',
   '13888888888',
   TRUE,
   'abc1234',
   'china stardand time',
   'ddd'
  );";

            helper.ExecuteSQL(insertSql);

            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId };

            mockInstance.GetAccessFailedCountAsync(user, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from user_base where id = '{0}'", userId), reader =>
             {
                 while (reader.Read())
                 {
                     Assert.Equal(reader["access_failed_count"].ToString(), "3");
                 }
             });
            ClearUserBase();
        }

        [Fact(DisplayName = "DapperUserStore:异步获取用户锁定可用状态")]
        private void GetLockoutEnabledAsync()
        {
            ClearUserBase();
            var userId = 666;
            var offset = DateTime.UtcNow;
            var fieldCount = 3;
            var insertSql = $@"
INSERT INTO 
  test.user_base (  
  id,
  access_failed_count, 
  email, 
  email_confirmed, 
  language, 
  lockout_enabled, 
  lockout_end, 
  normalized_user_name, 
  password_hash, 
  phone_number, 
  phone_number_confirmed, 
  security_stamp, 
  time_zone, 
  user_name)
  VALUES(
   {userId},
   {fieldCount},
   '666@qq.com',
   TRUE,
   'zh-Hans',
   TRUE,
   '{offset}',
   'ddd',
   '234',
   '13888888888',
   TRUE,
   'abc1234',
   'china stardand time',
   'ddd'
  );";

            helper.ExecuteSQL(insertSql);
            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId };

            mockInstance.GetLockoutEnabledAsync(user, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from user_base where id = '{0}'", userId), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["lockout_enabled"].ToString(), "1");
                }
            });
            ClearUserBase();
        }

        [Fact(DisplayName = "DapperUserStore:异步设置用户锁定可用状态")]
        private void SetLockoutEnabledAsync()
        {
            ClearUserBase();
            var userId = 666;
            var offset = DateTime.UtcNow;
            var fieldCount = 3;
            var insertSql = $@"
INSERT INTO 
  test.user_base (  
  id,
  access_failed_count, 
  email, 
  email_confirmed, 
  language, 
  lockout_enabled, 
  lockout_end, 
  normalized_user_name, 
  password_hash, 
  phone_number, 
  phone_number_confirmed, 
  security_stamp, 
  time_zone, 
  user_name)
  VALUES(
   {userId},
   {fieldCount},
   '666@qq.com',
   TRUE,
   'zh-Hans',
   TRUE,
   '{offset}',
   'ddd',
   '234',
   '13888888888',
   TRUE,
   'abc1234',
   'china stardand time',
   'ddd'
  );";

            helper.ExecuteSQL(insertSql);
            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId };

            mockInstance.SetLockoutEnabledAsync(user, false, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from user_base where id = '{0}'", userId), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["lockout_enabled"].ToString(), "0");
                }
            });
            ClearUserBase();
        }
        #endregion

        #region IUserPhoneNumberStore

        [Fact(DisplayName = "DapperUserStore:异步设置用户电话号码")]
        private void SetPhoneNumberAsync()
        {
            ClearUserBase();
            var userId = 666;
            var offset = DateTime.UtcNow;

            var insertSql = $@"
INSERT INTO 
  test.user_base (  
  id,
  access_failed_count, 
  email, 
  email_confirmed, 
  language, 
  lockout_enabled, 
  lockout_end, 
  normalized_user_name, 
  password_hash, 
  phone_number, 
  phone_number_confirmed, 
  security_stamp, 
  time_zone, 
  user_name)
  VALUES(
   {userId},
   1,
   '666@qq.com',
   TRUE,
   'zh-Hans',
   TRUE,
   '{offset}',
   'ddd',
   '234',
   '13888888888',
   TRUE,
   'abc1234',
   'china stardand time',
   'ddd'
  );";

            helper.ExecuteSQL(insertSql);
            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId };
            var phoneNumber = "13333333333";
            mockInstance.SetPhoneNumberAsync(user, phoneNumber, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from user_base where id = '{0}'", userId), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["phone_number"].ToString(), phoneNumber);
                }
            });
            ClearUserBase();
        }

        [Fact(DisplayName = "DapperUserStore:异步获取用户电话号码")]
        private void GetPhoneNumberAsync()
        {
            ClearUserBase();
            var userId = 666;
            var offset = DateTime.UtcNow;
            var phoneNumber = "13888888888";
            var insertSql = $@"
INSERT INTO 
  test.user_base (  
  id,
  access_failed_count, 
  email, 
  email_confirmed, 
  language, 
  lockout_enabled, 
  lockout_end, 
  normalized_user_name, 
  password_hash, 
  phone_number, 
  phone_number_confirmed, 
  security_stamp, 
  time_zone, 
  user_name)
  VALUES(
   {userId},
   1,
   '666@qq.com',
   TRUE,
   'zh-Hans',
   TRUE,
   '{offset}',
   'ddd',
   '234',
   '{phoneNumber}',
   TRUE,
   'abc1234',
   'china stardand time',
   'ddd'
  );";

            helper.ExecuteSQL(insertSql);

            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId };

            mockInstance.GetPhoneNumberAsync(user, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from user_base where id = '{0}'", userId), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["phone_number"].ToString(), phoneNumber);
                }
            });
            ClearUserBase();
        }

        [Fact(DisplayName = "DapperUserStore:异步获取用户电话号码确认状态")]
        private void GetPhoneNumberConfirmedAsync()
        {
            ClearUserBase();
            var userId = 666;
            var offset = DateTime.UtcNow;
            var phoneNumber = "13888888888";
            var insertSql = $@"
INSERT INTO 
  test.user_base (  
  id,
  access_failed_count, 
  email, 
  email_confirmed, 
  language, 
  lockout_enabled, 
  lockout_end, 
  normalized_user_name, 
  password_hash, 
  phone_number, 
  phone_number_confirmed, 
  security_stamp, 
  time_zone, 
  user_name)
  VALUES(
   {userId},
   1,
   '666@qq.com',
   TRUE,
   'zh-Hans',
   TRUE,
   '{offset}',
   'ddd',
   '234',
   '{phoneNumber}',
   TRUE,
   'abc1234',
   'china stardand time',
   'ddd'
  );";

            helper.ExecuteSQL(insertSql);
            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId };

            mockInstance.GetPhoneNumberConfirmedAsync(user, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from user_base where id = '{0}'", userId), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["phone_number_confirmed"].ToString(), "1");
                }
            });
            ClearUserBase();
        }

        [Fact(DisplayName = "DapperUserStore:异步设置用户电话号码确认状态")]
        private void SetPhoneNumberConfirmedAsync()
        {
            ClearUserBase();

            var userId = 666;
            var offset = DateTime.UtcNow;
            var phoneNumber = "13888888888";
            var insertSql = $@"
INSERT INTO 
  test.user_base (  
  id,
  access_failed_count, 
  email, 
  email_confirmed, 
  language, 
  lockout_enabled, 
  lockout_end, 
  normalized_user_name, 
  password_hash, 
  phone_number, 
  phone_number_confirmed, 
  security_stamp, 
  time_zone, 
  user_name)
  VALUES(
   {userId},
   1,
   '666@qq.com',
   TRUE,
   'zh-Hans',
   TRUE,
   '{offset}',
   'ddd',
   '234',
   '{phoneNumber}',
   TRUE,
   'abc1234',
   'china stardand time',
   'ddd'
  );";

            helper.ExecuteSQL(insertSql);
            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId };

            mockInstance.SetPhoneNumberConfirmedAsync(user, false, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from user_base where id = '{0}'", userId), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["phone_number_confirmed"].ToString(), "0");
                }
            });
            ClearUserBase();
        }

        #endregion

        #region IUserAuthenticationTokenStore

        [Fact(DisplayName = "DapperUserStore:异步设置认真令牌")]
        private void SetTokenAsync()
        {
            ClearUserTokens();
            var id = 666;
            var userId = 999;
            var provider = "123";
            var name = "name";
            var value = "value";

            var offset = DateTime.UtcNow;
            var insertSql = $@"
INSERT INTO 
  test.user_token(  
  id,
  user_id,
  login_provider,  
  name,
  value )
  VALUES(
   {id},
   {userId},
   '{provider}',
   '{name}',
   '{value}'
   );";

            helper.ExecuteSQL(insertSql);

            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId };

            mockInstance.SetTokenAsync(user, provider, name, value, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from user_base where id = '{0}'", userId), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["Id"].ToString(), id.ToString());
                    Assert.Equal(reader["UserId"].ToString(), userId.ToString());
                    Assert.Equal(reader["LoginProvider"].ToString(), provider.ToString());
                    Assert.Equal(reader["Name"].ToString(), name.ToString());
                    Assert.Equal(reader["Value"].ToString(), value.ToString());
                }
            });
            ClearUserTokens();
        }

        [Fact(DisplayName = "DapperUserStore:异步删除认真令牌")]
        private void RemoveTokenAsync()
        {
            ClearUserTokens();
            var id = 666;
            var userId = 999;
            var provider = "123";
            var name = "name";
            var value = "value";

            var offset = DateTime.UtcNow;
            var insertSql = $@"
INSERT INTO 
  test.user_token (  
  id,
  user_id,
  login_provider,  
  name,
  value )
  VALUES(
   {id},
   {userId},
   '{provider}',
   '{name}',
   '{value}'
   );";

            helper.ExecuteSQL(insertSql);

            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId };

            mockInstance.RemoveTokenAsync(user, provider, name, CancellationToken.None).GetAwaiter().GetResult();


            //获取最后一条值进行校验
            Assert.True(helper.GetValue($"select count(1)  from test.user_token where id={id} ;", Convert.ToInt32) <= 0);

            ClearUserTokens();

        }

        [Fact(DisplayName = "DapperUserStore:异步获取认真令牌")]
        private void GetTokenAsync()
        {
            ClearUserTokens();
            var id = 666;
            var userId = 999;
            var provider = "123";
            var name = "name";
            var value = "value";

            var offset = DateTime.UtcNow;
            var insertSql = $@"
INSERT INTO 
  test.user_token(  
  id,
  user_id,
  login_provider,  
  name,
  value )
  VALUES(
   {id},
   {userId},
   '{provider}',
   '{name}',
   '{value}'
   );";

            helper.ExecuteSQL(insertSql);
            var mockInstance = _dapper;

            var user = new UserBase() { Id = userId };

            mockInstance.GetTokenAsync(user, provider, name, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from user_token where id = '{0}'", userId), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["id"].ToString(), id.ToString());
                    Assert.Equal(reader["user_id"].ToString(), userId.ToString());
                    Assert.Equal(reader["login_provider"].ToString(), provider.ToString());
                    Assert.Equal(reader["name"].ToString(), name.ToString());
                    Assert.Equal(reader["value"].ToString(), value.ToString());
                }
            });
            ClearUserTokens();
        }

        public void Dispose()
        {
            context1?.Dispose();
            context2?.Dispose();
            context3?.Dispose();
            context4?.Dispose();
            context5?.Dispose();
            context6?.Dispose();
        }

        #endregion


    }
    /// <summary>
    /// 初始化配置参数
    /// </summary>
    public class DapperMetadataProviderRoleClaim : DapperMetadataProvider<RoleClaim>
    {
        protected override void ConfigureModel(DapperMetadataBuilder<RoleClaim> builder)
        {
            builder.HasKey(d => d.Id);
        }
    }
    /// <summary>
    /// 初始化配置参数
    /// </summary>
    public class DapperMetadataProviderUserBase : DapperMetadataProvider<UserBase>
    {
        protected override void ConfigureModel(DapperMetadataBuilder<UserBase> builder)
        {
            builder.HasKey(d => d.Id);
        }
    }
    /// <summary>
    /// 初始化配置参数
    /// </summary>
    public class DapperMetadataProviderRoleBase : DapperMetadataProvider<RoleBase>
    {
        protected override void ConfigureModel(DapperMetadataBuilder<RoleBase> builder)
        {
            builder.HasKey(d => d.Id);
        }
    }
    /// <summary>
    /// 初始化配置参数
    /// </summary>
    public class DapperMetadataProviderUserClaim : DapperMetadataProvider<UserClaim>
    {
        protected override void ConfigureModel(DapperMetadataBuilder<UserClaim> builder)
        {
            builder.HasKey(d => d.Id);
        }
    }

    /// <summary>
    /// 初始化配置参数
    /// </summary>
    public class DapperMetadataProviderUserLogin : DapperMetadataProvider<UserLogin>
    {
        protected override void ConfigureModel(DapperMetadataBuilder<UserLogin> builder)
        {
            builder.HasKey(d => d.Id);
        }
    }
    /// <summary>
    /// 初始化配置参数
    /// </summary>
    public class DapperMetadataProviderUserToken : DapperMetadataProvider<UserToken>
    {
        protected override void ConfigureModel(DapperMetadataBuilder<UserToken> builder)
        {
            builder.HasKey(d => d.Id);
        }
    }

    /// <summary>
    /// 初始化配置参数
    /// </summary>
    public class DapperMetadataProviderUserRole : DapperMetadataProvider<UserRole>
    {
        protected override void ConfigureModel(DapperMetadataBuilder<UserRole> builder)
        {
            builder.HasKey(d => new { d.UserId, d.RoleId });
        }
    }
}
