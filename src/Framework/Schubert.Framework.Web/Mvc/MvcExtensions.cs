using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    public static class MvcExtensions
    {
        //public static void AddModelError<TModel>(this ModelStateDictionary state, 
        //    Expression<Func<TModel, Object>> propertyExpression,
        //    string errorMessage)
        //{
        //    Guard.ArgumentNotNull(propertyExpression, nameof(propertyExpression));
        //    state.AddModelError(propertyExpression.GetMemberName(), errorMessage);
            
        //}

        //public static void AddModelError<TModel>(this ModelStateDictionary state,
        //    Expression<Func<TModel, Object>> propertyExpression,
        //    Exception exception)
        //{
        //    Guard.ArgumentNotNull(propertyExpression, nameof(propertyExpression));
        //    state.AddModelError(propertyExpression.GetMemberName(), exception);

        //}

        //public static void Add<TModel>(this ModelStateDictionary dictonary,
        //    Expression<Func<TModel, Object>> propertyExpression,
        //    ModelState state)
        //{
        //    Guard.ArgumentNotNull(propertyExpression, nameof(propertyExpression));
        //    dictonary.Add(propertyExpression.GetMemberName(), state);

        //}
    }
}
