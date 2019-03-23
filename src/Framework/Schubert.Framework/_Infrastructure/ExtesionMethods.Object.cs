using Schubert;
using Schubert.Helpers;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace System
{
    static partial class ExtensionMethods
    {
        public static bool ValidateProperty<T, TProperty>(this T obj, Expression<Func<T, TProperty>> propertyExpression, out string errorMessage)
        {
            string memberName = propertyExpression.GetMemberName();
            return obj.ValidateProperty(memberName, out errorMessage);
        }

        /// <summary>
        /// 通过 DataAnnotations 规则验证对象属性。
        /// </summary>
        public static bool ValidateProperty(this Object obj, string propertyName, out string errorMessage)
        {
            Object instance = obj;
            errorMessage = null;
            var realyPropertyName = propertyName;
            if (propertyName.Contains("."))
            {
                var propertPath = propertyName.Split('.');
                realyPropertyName = propertPath.Last();
                for (var i = 0; i < propertPath.Length; i++)
                {
                    instance = ExpressionHelper.MakePropertyLambda(instance.GetType(), propertPath[i]).Compile().DynamicInvoke(instance);
                    if (instance == null)
                    {
                        return true;
                    }
                    if (i == propertPath.Length - 2)
                    {
                        break;
                    }
                }
            }

            var propertyValue = ExpressionHelper.MakePropertyLambda(obj.GetType(), propertyName).Compile().DynamicInvoke(obj);
            ValidationContext context = new ValidationContext(instance, null, null);
            context.MemberName = realyPropertyName;
            var errors = new Collection<ValidationResult>();
            var valid = Validator.TryValidateProperty(propertyValue, context, errors);
            if (!valid)
            {
                errorMessage = errors.First().ErrorMessage;
            }
            return valid;
        }

        /// <summary>
        /// 通过 DataAnnotations 规则验证对象。
        /// </summary>
        public static bool Validate(this Object obj, out string errorMessage)
        {
            var context = new ValidationContext(obj, null, null);
            var errors = new Collection<ValidationResult>();
            var valid = Validator.TryValidateObject(obj, context, errors, true);
            errorMessage = null;
            if (!valid)
            {
                errorMessage = errors.First().ErrorMessage;
            }
            return valid;
        }

        /// <summary>
        /// 检查对象是否通过 DataAnnotations 规则验证。
        /// </summary>
        public static bool IsValid(this Object data)
        {
            Guard.ArgumentNotNull(data, "data");
            var context = new ValidationContext(data, null, null);
            var errors = new Collection<ValidationResult>();
            var valid = Validator.TryValidateObject(data, context, errors, true);
            return valid;
        }

        /// <summary>
        /// 检查对象是否通过 DataAnnotations 规则验证。
        /// </summary>
        public static bool IsValid(this IEnumerable data)
        {
            Guard.ArgumentNotNull(data, "data");
            foreach (Object obj in data)
            {
                if (!obj.IsValid())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
