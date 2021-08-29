using System;
using System.Linq.Expressions;

namespace LochNessBuilder.Extensions
{
    public static class ExpressionExtensions
    {
        public static string GetMemberName<TInstance, TProp>(this Expression<Func<TInstance, TProp>> selector)
        {
            var unaryExpression = selector.Body as UnaryExpression;
            if (unaryExpression != null && unaryExpression.NodeType == ExpressionType.Convert)
            {
                return ((MemberExpression)unaryExpression.Operand).Member.Name;
            }
            return ((MemberExpression)selector.Body).Member.Name;
        }
    }
}