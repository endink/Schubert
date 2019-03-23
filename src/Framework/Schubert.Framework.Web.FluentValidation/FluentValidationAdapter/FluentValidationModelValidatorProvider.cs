using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Schubert.Framework.Web.Validation
{
    public class FluentValidationModelValidatorProvider : IModelValidatorProvider
    {
        public void CreateValidators(ModelValidatorProviderContext context)
        {
            //1、只要存在IsReusable为false的那就会每次启动调用所有的IModelValidatorProvider的CreateValidators方法
            //2、因为我们初始化rule的时候默认IsReusable的值是false，Validator也是空的，好在这个初始化之后会调用一把我们的CreateValidator方法，
            //   所以我们有必要在CreateValidator中将其置为true，并把Validator的值给赋上
            //3、如果我们只是添加一个新的可复用的Validator,那由于第一次会将其缓存起来，导致第二次之后都会有两个重复的Validator
            //4、具体细节在ValidatorCache类中
            foreach (var item in context.Results)
            {
                if (item.Validator == null)
                {
                    var validateRule = item.ValidatorMetadata as IValidationRule;
                    if (validateRule != null)
                    {
                        item.Validator = new FluentValidationModelValidator(validateRule);
                        item.IsReusable = true;
                    }
                }
            }
            //foreach (var rule in context.ValidatorMetadata.OfType<IValidationRule>())
            //{
            //    //if (!hasRequiredAttribute)
            //    //{
            //    //    foreach (var val in rule.Validators)
            //    //    {
            //    //        hasRequiredAttribute |= (val is NotNullValidator);
            //    //        hasRequiredAttribute |= (val is NotEmptyValidator);
            //    //    }
            //    //}
            //    //context.Results.Add(new ValidatorItem(rule)
            //    //{
            //    //    Validator = new FluentValidationModelValidator(rule),
            //    //    IsReusable = true
            //    //});
            //    //移除 Mvc 自动添加的 Required 验证器。
            //    //if (hasRequiredAttribute && context.ModelMetadata.IsRequired)
            //    //{
            //    //    RequiredAttributeAdapter adatper = context.Validators.OfType<RequiredAttribute>().FirstOrDefault();
            //    //    if (adatper != null)
            //    //    {
            //    //        context.Validators.Remove(adatper);
            //    //    }
            //    //}
            //}
        }
    }
}
