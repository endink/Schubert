using System;
using System.Linq;
using System.Linq.Expressions;
using Schubert.Framework.Data;
using Xunit;

namespace Schubert.Framework.Test.Dapper
{
    [Collection("dapper")]
    public class LamdaQueryParserTest
    {
        public LamdaQueryParserTest()
        {
            Tools.MappingStrategyHelper.SetStrategy(IdentifierMappingStrategy.Underline);
        }
        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public DateTime BirthDay { get; set; }
        }

        [Fact(DisplayName = "LamdaQueryParser: 基本解析测试。")]
        public void TestParse()
        {

            Expression<Func<User, bool>> lamda1 = u => u.Id == 1;
            Expression<Func<User, bool>> lamda2 = u => u.Id == 3 || u.Name == "3";
            Expression<Func<User, bool>> lamda3 = u => u.Id == 3 && u.Name == "3" && u.Id == 0;
            Expression<Func<User, bool>> lamda4 = u => (u.Id == 3 && u.Name == "3") || u.Id == 0;
            Expression<Func<User, bool>> lamda5 = u => (u.Id == 3 && u.Name == "3") || (u.Id == 0 && u.Name == "4");
            #region lamda1
            var query1 = LamdaQueryParser.Where<User>(lamda1) as SingleQueryFilter;
            SingleQueryFilter qf = new SingleQueryFilter();
            qf.AddEqual(nameof(User.Id), 1);
            Compare(query1, qf);
            #endregion

            #region lamda2
            var query2 = LamdaQueryParser.Where<User>(lamda2) as CombinedQueryFilter;

            SingleQueryFilter qf1 = new SingleQueryFilter();
            qf1.AddEqual(nameof(User.Id), 3);
            SingleQueryFilter qf2 = new SingleQueryFilter();
            qf2.AddEqual(nameof(User.Name), "3");


            CombinedQueryFilter cf1 = new CombinedQueryFilter(qf1, qf2, BooleanClause.Or);

            Compare(query2, cf1);
            #endregion

            #region lamda3
            var query3 = LamdaQueryParser.Where<User>(lamda3) as CombinedQueryFilter;

            SingleQueryFilter qf3 = new SingleQueryFilter();
            qf3.AddEqual(nameof(User.Id), 3);
            SingleQueryFilter qf4 = new SingleQueryFilter();
            qf4.AddEqual(nameof(User.Name), "3");

            CombinedQueryFilter cf2 = new CombinedQueryFilter(qf3, qf4, BooleanClause.And);

            SingleQueryFilter qf5 = new SingleQueryFilter();
            qf5.AddEqual(nameof(User.Id), 0);

            CombinedQueryFilter cf3 = new CombinedQueryFilter(cf2, qf5, BooleanClause.And);

            Compare(query3, cf3);

            #endregion

            #region lamda4
            var query4 = LamdaQueryParser.Where<User>(lamda4) as CombinedQueryFilter;

            SingleQueryFilter qf6 = new SingleQueryFilter();
            qf6.AddEqual(nameof(User.Id), 3);
            SingleQueryFilter qf7 = new SingleQueryFilter();
            qf7.AddEqual(nameof(User.Name), "3");

            CombinedQueryFilter cf4 = new CombinedQueryFilter(qf6, qf7, BooleanClause.And);

            SingleQueryFilter qf8 = new SingleQueryFilter();
            qf8.AddEqual(nameof(User.Id), 0);
            CombinedQueryFilter cf5 = new CombinedQueryFilter(cf2, qf5, BooleanClause.Or);
            Compare(query4, cf5);

            #endregion

            #region lamba5
            var query5 = LamdaQueryParser.Where<User>(lamda5) as CombinedQueryFilter;

            SingleQueryFilter sf1 = new SingleQueryFilter();
            sf1.AddEqual(nameof(User.Id), 3);
            SingleQueryFilter sf2 = new SingleQueryFilter();
            sf2.AddEqual(nameof(User.Name), "3");

            CombinedQueryFilter c1 = new CombinedQueryFilter(sf1, sf2, BooleanClause.And);

            SingleQueryFilter sf3 = new SingleQueryFilter();
            sf3.AddEqual(nameof(User.Id), 0);
            SingleQueryFilter sf4 = new SingleQueryFilter();
            sf4.AddEqual(nameof(User.Name), "4");

            CombinedQueryFilter c2 = new CombinedQueryFilter(sf3, sf4, BooleanClause.And);

            CombinedQueryFilter c3 = new CombinedQueryFilter(c1, c2, BooleanClause.Or);

            Compare(query5, c3);
            #endregion 
        }
        [Fact(DisplayName = "LamdaQueryParser: 参数传值测试。")]
        public void TestParameter()
        {
            Parameter("codemonk");
        }
        private void Compare(SingleQueryFilter s1, SingleQueryFilter s2)
        {
            Assert.Equal(s1.Count(), s2.Count());
            s1.ForEach(t =>
            {
                var count = s2.Where(s => s.FieldName == t.FieldName && (int)s.Operation == (int)t.Operation
                && s.OperationValue.ToString() == t.OperationValue.ToString()).Count();
                Assert.Equal(count, 1);
            });
        }

        private void Compare(CombinedQueryFilter s1, CombinedQueryFilter s2)
        {
            Assert.Equal(s1.Clause, s2.Clause);
            Assert.Equal(s1.Filter1 is SingleQueryFilter, s2.Filter1 is SingleQueryFilter);
            Assert.Equal(s1.Filter2 is SingleQueryFilter, s2.Filter2 is SingleQueryFilter);

            if (s1.Filter1 is SingleQueryFilter)
            {
                Compare((SingleQueryFilter)(s1.Filter1), (SingleQueryFilter)(s2.Filter1));
            }
            if (s1.Filter2 is SingleQueryFilter)
            {
                Compare((SingleQueryFilter)(s1.Filter2), (SingleQueryFilter)(s2.Filter2));
            }
            if (s1.Filter1 is CombinedQueryFilter)
            {
                Compare((CombinedQueryFilter)(s1.Filter1), (CombinedQueryFilter)(s2.Filter1));
            }
            if (s1.Filter2 is CombinedQueryFilter)
            {
                Compare((CombinedQueryFilter)(s1.Filter2), (CombinedQueryFilter)(s2.Filter2));
            }
        }


        private void Parameter(string name)
        {
            var where = LamdaQueryParser.Where<User>(u => u.Name == name) as SingleQueryFilter;
            var filter = new SingleQueryFilter();
            filter.AddEqual(nameof(User.Name), name);
            Compare(where, filter);
        }
    }
}
