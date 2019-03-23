using Microsoft.Extensions.Options;
using Schubert.Framework.Environment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    public class SingleQueryFilter : QueryFilter, IEnumerable<FieldPredicate>
    {
        private HashSet<FieldPredicate> _predicateSet = null; 

        private static readonly IEqualityComparer<FieldPredicate> FieldPredicateComparer;

        static SingleQueryFilter()
        {
            FieldPredicateComparer = new GenericEqualityComparer<FieldPredicate>((f1, f2) =>
            {
                return f1.FieldName.CaseInsensitiveEquals(f2.FieldName) &&
                f1.Operation.Equals(f2.Operation) &&
                f1.OperationValue.SafeEquals(f2.OperationValue);
            });
        }

        public SingleQueryFilter(BooleanClause clause = BooleanClause.And)
        {
            this.Clause = clause;

            _predicateSet = new HashSet<FieldPredicate>(FieldPredicateComparer);
        }  
        public bool IsEmpty
        {
            get { return this._predicateSet.Count == 0; }
        }

        #region predicate helpers

        /// <summary>
        /// 添加“等于”筛选。
        /// </summary>
        /// <param name="fieldName">要删选的字段名。</param>
        /// <param name="value">要删选的值。</param>
        /// <returns></returns>
        public SingleQueryFilter AddEqual(string fieldName, object value)
        {
            this._predicateSet.Add(new FieldPredicate(fieldName, BinaryOperation.Equal, value));
            return this;
        }

        /// <summary>
        /// 添加“不等于”筛选。
        /// </summary>
        /// <param name="fieldName">要删选的字段名。</param>
        /// <param name="value">要删选的值。</param>
        /// <returns></returns>
        public SingleQueryFilter AddNotEqual(string fieldName, object value)
        {
            this._predicateSet.Add(new FieldPredicate(fieldName, BinaryOperation.NotEqual, value));
            return this;
        }

        /// <summary>
        /// 添加“大于”筛选。
        /// </summary>
        /// <param name="fieldName">要删选的字段名。</param>
        /// <param name="value">要删选的值。</param>
        /// <returns></returns>
        public SingleQueryFilter AddGreater(string fieldName, object value)
        {
            this._predicateSet.Add(new FieldPredicate(fieldName, BinaryOperation.Greater, value));
            return this;
        }

        /// <summary>
        /// 添加“大于等于”筛选。
        /// </summary>
        /// <param name="fieldName">要删选的字段名。</param>
        /// <param name="value">要删选的值。</param>
        /// <returns></returns>
        public SingleQueryFilter AddGreaterOrEqual(string fieldName, object value)
        {
            this._predicateSet.Add(new FieldPredicate(fieldName, BinaryOperation.GreaterOrEqual, value));
            return this;
        }

        /// <summary>
        /// 添加“小于”筛选。
        /// </summary>
        /// <param name="fieldName">要删选的字段名。</param>
        /// <param name="value">要删选的值。</param>
        /// <returns></returns>
        public SingleQueryFilter AddLess(string fieldName, object value)
        {
            this._predicateSet.Add(new FieldPredicate(fieldName, BinaryOperation.Less, value));
            return this;
        }

        /// <summary>
        /// 添加“小于等于”筛选。
        /// </summary>
        /// <param name="fieldName">要删选的字段名。</param>
        /// <param name="value">要删选的值。</param>
        /// <returns></returns>
        public SingleQueryFilter AddLessOrEqual(string fieldName, object value)
        {
            this._predicateSet.Add(new FieldPredicate(fieldName, BinaryOperation.LessOrEqual, value));
            return this;
        }

        #endregion

        /// <summary>
        /// 添加筛选条件。
        /// </summary>
        /// <param name="fieldName">用于筛选的字段。</param>
        /// <param name="operation">二元操作符。</param>
        /// <param name="value">用于筛选的值。</param>
        public void AddPredicate(string fieldName, BinaryOperation operation, object value)
        {
            this._predicateSet.Add(new FieldPredicate(fieldName, operation, value));
        }

        IEnumerator<FieldPredicate> IEnumerable<FieldPredicate>.GetEnumerator()
        {
            return this._predicateSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._predicateSet.GetEnumerator();
        }


        /// <summary>
        /// 表示查询过滤器中的字段逻辑组合方式。
        /// </summary>
        public BooleanClause Clause { get; set; }
    }
}
