using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Schubert.Framework.Data
{
    public class LamdaQueryParser
    {
        public static QueryFilter Where<T>(Expression<Func<T, bool>> expression)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));
            var visitor = new SimpleVisitor();
            visitor.Visit(expression);
            return visitor.Filter;
        }
    }

    internal class SimpleVisitor : ExpressionVisitor
    {
        #region 
        private static readonly IDictionary<ExpressionType, BinaryOperation> OperationMapping = new Dictionary<ExpressionType, BinaryOperation>
        {
            {ExpressionType.Equal,BinaryOperation.Equal},
            {ExpressionType.GreaterThan,BinaryOperation.Greater },
            {ExpressionType.GreaterThanOrEqual,BinaryOperation.GreaterOrEqual },
            {ExpressionType.NotEqual,BinaryOperation.NotEqual },
            {ExpressionType.LessThan,BinaryOperation.Less },
            {ExpressionType.LessThanOrEqual,BinaryOperation.LessOrEqual }
        };
        private static readonly IDictionary<ExpressionType, BooleanClause> ClauseMapping = new Dictionary<ExpressionType, BooleanClause>
        {
            {ExpressionType.And,BooleanClause.And },
            {ExpressionType.AndAlso,BooleanClause.And },
            {ExpressionType.Or,BooleanClause.Or },
            {ExpressionType.OrElse,BooleanClause.Or },
        };
        #endregion

        [System.Diagnostics.DebuggerHidden]
        private static bool IsSupportedConstantType(Type type)
        {
            if (type == typeof(string)) return true;
            var info = type.GetTypeInfo();
            var p1 = new Predicate<TypeInfo>(i => i.BaseType != typeof(object) && (i.IsPrimitive || i.IsEnum));
            var p2 = new Predicate<Type>(i => new Type[] { typeof(Guid), typeof(DateTime), typeof(TimeSpan) }.Any(m => m == i));
            if (p1(info) || p2(type)) return true;
            if (!info.IsGenericType) return false;
            if (info.UnderlyingSystemType != typeof(Nullable<>)) return false;
            var first = info.GenericTypeArguments.Length == 1 ? info.GenericTypeArguments[0] : null;
            if (first == null) return false;
            return p1(first.GetTypeInfo()) || p2(first);
        }

        public QueryFilter Filter => _filter;

        [System.Diagnostics.DebuggerHidden]
        private static object ParseValue(object value)
        {
            if (value == null) return null;
            var type = value.GetType();
            var supported = IsSupportedConstantType(type);
            if (supported) return value;
            throw new NotImplementedException("不支持复合类型的常量值");
        }
        private QueryFilter _filter = new SingleQueryFilter();
        private string _fieldName;
        private object _value;
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var expression = node.Body as BinaryExpression;
            if (expression == null) throw new Exception("不是合法的表达式，请确保表达式是一个符合规范的二元表达式。");
            base.Visit(node.Body);
            return node;
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            BooleanClause clause;
            if (ClauseMapping.TryGetValue(node.NodeType, out clause))
            {
                var leftVisitor = new SimpleVisitor();
                leftVisitor.Visit(node.Left);
                var rightVisitor = new SimpleVisitor();
                rightVisitor.Visit(node.Right);
                _filter = new CombinedQueryFilter(leftVisitor._filter, rightVisitor._filter, clause);
                return node;
            }
            BinaryOperation symbol;
            if (OperationMapping.TryGetValue(node.NodeType, out symbol))
            {
                var leftVisitor = new SimpleVisitor();
                leftVisitor.Visit(node.Left);
                var rightVisitor = new SimpleVisitor();
                rightVisitor.Visit(node.Right);
                var x = new SingleQueryFilter();
                x.AddPredicate(leftVisitor._fieldName, symbol, rightVisitor._value);
                _filter = x;
                return node;
            }
            throw new NotSupportedException($"不支将 Lamda 表达式解析为 {nameof(QueryFilter)}，请确保右侧的表达式是一个符合规范的二元表达式。");
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (string.IsNullOrWhiteSpace(_fieldName))
            {
                _value = node.Value;
            }
            else
            {
                var instance = node.Value;
                var info = instance.GetType().GetMember(_fieldName).FirstOrDefault();
                var minfo = info as MethodInfo;
                if (minfo != null) _value = GetValue(minfo, instance);
                var finfo = info as FieldInfo;
                if (finfo != null) _value = GetValue(finfo, instance);
            }
            return node;
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = node.Expression;
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var value = GetMemberValue(node);
                _value = ParseValue(value);
            }
            else
            {
                var member = node.Member;
                _fieldName = member.Name;
                Visit(expression);
            }
            return node;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Arguments.Count == 0)
            {
                if (node.Object != null)
                {
                    if (node.Object is ConstantExpression)
                    {
                        var instanceExpression = (ConstantExpression)node.Object;
                        _value = GetValue(node.Method, instanceExpression.Value);

                    }
                    if (node.Object is MemberExpression && node.Object.NodeType == ExpressionType.MemberAccess)
                    {
                        var instance = GetMemberValue((MemberExpression)node.Object);
                        _value = GetValue(node.Method, instance);
                    }
                }
                else
                {
                    _value = node.Method.Invoke(null, new object[0]);
                }
                return node;
            }
            throw new NotSupportedException($"不支持解析给定的成员访问表达式，请考虑减少内联调用。");
        }
        [System.Diagnostics.DebuggerHidden]
        private static object GetMemberValue(MemberExpression memberAccessExp)
        {
            object instance = null;
            if (memberAccessExp.NodeType == ExpressionType.MemberAccess)
            {
                var expression = memberAccessExp.Expression;
                if (expression is MemberExpression)
                {
                    var memeberExpression = (MemberExpression)memberAccessExp.Expression;
                    instance = GetMemberValue(memeberExpression);
                }
                else if (expression is ConstantExpression)
                {
                    var valueExpression = (ConstantExpression)memberAccessExp.Expression;
                    instance = valueExpression.Value;
                }
            }
            if (memberAccessExp.Member is FieldInfo) return GetValue((FieldInfo)memberAccessExp.Member, instance);
            if (memberAccessExp.Member is PropertyInfo) return GetValue(((PropertyInfo)memberAccessExp.Member).GetGetMethod(), instance);
            throw new NotSupportedException($"不支持解析给定的成员访问表达式，请考虑减少内联调用。");
        }

        private static object GetValue(MethodInfo info, object instance) => info.Invoke(instance, new object[] { });
        private static object GetValue(FieldInfo info, object instance) => info.GetValue(instance);
    }
}
