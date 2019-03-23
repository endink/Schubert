using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MySql.Data.MySqlClient;
using Schubert.Framework.Data;
using Schubert.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test.DapperIdentity
{
    [Collection("dapper")]
    public class DapperRoleStoreTest : IDisposable
    {
        private MySQLHelper helper = MySQLHelper.Default;
        private DapperRoleStoreImp _dapper;
        public DapperRoleStoreTest()
        {
            Tools.MappingStrategyHelper.SetStrategy(IdentifierMappingStrategy.Underline);
            _dapper = MockGenerator();
            helper.CreateDataBaseIfNoExist("test");
            helper.CreateTableIfNoExist(new[] { "user_base", "user_token", "user_login", "user_claim", "role_base", "user_role", "role_claim" });
        }
        private DapperContext context1;
        private DapperContext context2;

        private DapperRoleStoreImp MockGenerator()
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

            DapperRuntime rt = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderRoleBase() });
            DapperRepository<RoleBase> dpRoleBase = new DapperRepository<RoleBase>(context1 = new DapperContext(rt, loggerFactory), loggerFactory);
            DapperRuntime rt1 = new DapperRuntime(optionsMock.Object, new IDapperMetadataProvider[] { new DapperMetadataProviderRoleClaim() });
            DapperRepository<RoleClaim> dpRoleClaim = new DapperRepository<RoleClaim>(context2 = new DapperContext(rt1, loggerFactory), loggerFactory);

            return new DapperRoleStoreImp(new IIdGenerationServiceImp(), dpRoleBase, dpRoleClaim);
        }

        private void ClearRoleBase() => helper.ExecuteSQL(@"delete from test.role_base");
        private void CreateRoleBase(int id = 1, string roleName = "admin", string displayName = "管理员")
        {
            var insertSql = $@"
            INSERT INTO 
              test.role_base (  
              id,
              role_name,
              display_name
              )
              VALUES(
              '{id}',
              '{roleName}',
              '{displayName}'
              );";
            helper.ExecuteSQL(insertSql);
        }
        private void ClearRoleClaim() => helper.ExecuteSQL(@"delete from test.role_claim");
        private void CreateRoleClaim(int id = 1, int roleId = 666, string claimType = "type1", string claimValue = "value1")
        {
            var insertSql = $@"
            INSERT INTO 
              test.role_claim (  
              id,
              role_id,
              claim_type,
              claim_value
              )
              VALUES(
              '{id}',
              '{roleId}',
              '{claimType}',
              '{claimValue}'
              );";
            helper.ExecuteSQL(insertSql);
        }
        #region IRoleStore

        [Fact(DisplayName = "DapperRoleStore:异步创建用户角色")]
        private void CreateAsync()
        {
            ClearRoleBase();

            var id = 1;
            var roleName = "admin";
            var DisplayName = "管理员";


            var role = new RoleBase();
            role.Id = id;
            role.Name = roleName;
            //role.DisplayName = DisplayName;


            _dapper.CreateAsync(role, CancellationToken.None).GetAwaiter().GetResult();

            helper.ExecuteReader(string.Format("select * from role_base where id = '{0}'", id), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["id"].ToString(), id.ToString());
                    Assert.Equal(reader["role_name"], roleName);
                    Assert.Equal(reader["display_name"], DisplayName);
                }
            });
            ClearRoleBase();
        }

        [Fact(DisplayName = "DapperRoleStore:异步删除用户角色")]
        private void DeleteAsync()
        {
            ClearRoleBase();
            var id = 1;
            CreateRoleBase(id);
            var role = new RoleBase() { Id = id };
            _dapper.DeleteAsync(role, CancellationToken.None).GetAwaiter().GetResult();

            //获取最后一条值进行校验
            Assert.True(helper.GetValue($"select count(1)  from test.role_base where id={id} ;", Convert.ToInt32) <= 0);

            ClearRoleBase();
        }

        [Fact(DisplayName = "DapperRoleStore:异步根据Id查询用户角色")]
        private void FindByIdAsync()
        {
            ClearRoleBase();
            var id = 1;
            var roleName = "admin";
            var displayName = "管理员";
            CreateRoleBase(id, roleName, displayName);

            var role = _dapper.FindByIdAsync(id.ToString(), CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from role_base where id = '{0}'", id), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["id"].ToString(), id.ToString());
                    Assert.Equal(reader["role_name"].ToString(), roleName);
                    Assert.Equal(reader["display_name"].ToString(), displayName);
                }
            });

            ClearRoleBase();
        }

        [Fact(DisplayName = "DapperRoleStore:异步根据角色名称查询用户角色")]
        private void FindByNameAsync()
        {
            ClearRoleBase();
            var id = 1;
            var roleName = "admin";
            var displayName = "管理员";
            CreateRoleBase(id, roleName, displayName);

            var role = _dapper.FindByNameAsync(roleName, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from role_base where id = '{0}'", id), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["id"].ToString(), id.ToString());
                    Assert.Equal(reader["role_name"].ToString(), roleName);
                    Assert.Equal(reader["display_name"].ToString(), displayName);
                }
            });
            ClearRoleBase();
        }

        [Fact(DisplayName = "DapperRoleStore:异步获取用户的显示名称")]
        private void SetRoleNameAsync()
        {
            ClearRoleBase();
            var id = 1;
            var roleName = "admin";
            //var displayName = "管理员";

            var role = new RoleBase();
            role.Id = id;
            role.Name = roleName;
            //role.DisplayName = displayName;

            var newRoleName = "新名字";
            _dapper.SetRoleNameAsync(role, newRoleName, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from role_base where id = '{0}'", role.Id), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["display_name"].ToString(), newRoleName);
                }
            });
            ClearRoleBase();
        }

        [Fact(DisplayName = "DapperRoleStore:异步更新用户角色")]
        private void UpdateAsync()
        {
            ClearRoleBase();
            var id = 1;
            var roleName = "admin";
            var displayName = "管理员";
            CreateRoleBase(id, roleName, displayName);


            var newName = "newAdmin";
            var newDisplayName = "新显示名称";
            var role = new RoleBase();
            role.Id = id;
            role.Name = newName;
            //role.DisplayName = newDisplayName;

            _dapper.UpdateAsync(role, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from role_base where id = '{0}'", id), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["id"].ToString(), id.ToString());
                    Assert.Equal(reader["role_name"].ToString(), newName);
                    Assert.Equal(reader["display_name"].ToString(), newDisplayName);
                }
            });
            ClearRoleBase();
        }

        [Fact(DisplayName = "DapperRoleStore:异步获取角色声明")]
        private void GetClaimsAsync()
        {
            ClearRoleClaim();
            var id = 1;
            var roleId = 666;
            var claimType = "type1";
            var claimValue = "value1";
            CreateRoleClaim(id, roleId, claimType, claimValue);


            var role = new RoleBase() { Id = roleId };
            var claim = _dapper.GetClaimsAsync(role, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from role_claim where role_id = '{0}'", roleId), reader =>
            {
                while (reader.Read())
                {
                    Assert.Equal(reader["id"].ToString(), id.ToString());
                    Assert.Equal(reader["role_id"].ToString(), roleId.ToString());
                    Assert.Equal(reader["claim_type"].ToString(), claimType);
                    Assert.Equal(reader["claim_value"].ToString(), claimValue);
                }
            });
            ClearRoleClaim();
        }

        [Fact(DisplayName = "DapperRoleStore:异步添加角色声明")]
        private void AddClaimAsync()
        {
            ClearRoleClaim();
            var claimType = "type1";
            var claimValue = "value1";
            var roleId = 666;


            var role = new RoleBase();
            role.Id = 666;

            var claim = new Claim(claimType, claimValue);

            _dapper.AddClaimAsync(role, claim, CancellationToken.None).GetAwaiter().GetResult();
            helper.ExecuteReader(string.Format("select * from role_claim where role_id = '{0}'", roleId), reader =>
            {
                while (reader.Read())
                {
                    //Assert.Equal(reader["Id"].ToString(), id.ToString());
                    Assert.Equal(reader["role_id"].ToString(), roleId.ToString());
                    Assert.Equal(reader["claim_type"].ToString(), claimType);
                    Assert.Equal(reader["claim_value"].ToString(), claimValue);
                }
            });
            ClearRoleClaim();
        }

        [Fact(DisplayName = "DapperRoleStore:异步移除用户角色声明")]
        private void RemoveClaimAsync()
        {
            ClearRoleClaim();
            var id = 1;
            var roleId = 666;
            var claimType = "type1";
            var claimValue = "value1";
            CreateRoleClaim(id, roleId, claimType, claimValue);

            var role = new RoleBase() { Id = roleId };
            var claim = new Claim(claimType, claimValue);

            _dapper.RemoveClaimAsync(role, claim, CancellationToken.None).GetAwaiter().GetResult();

            //获取最后一条值进行校验
            Assert.True(helper.GetValue($"select count(1)  from test.role_claim where id={id} ;", Convert.ToInt32) <= 0);

            ClearRoleClaim();
        }

        public void Dispose()
        {
            context1?.Dispose();
            context2?.Dispose();
        }

        #endregion
    }



}
