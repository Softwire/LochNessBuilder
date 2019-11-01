using System;
using System.Linq.Expressions;

namespace LochNessBuilder.Extensions
{
    public static class ExpressionExtensions
    {
        public static string GetMemberName<TInstance, TProp>(this Expression<Func<TInstance, TProp>> selector)
        {
            var selectorBody = (MemberExpression)selector.Body;
            return selectorBody.Member.Name;
        }
    }
}