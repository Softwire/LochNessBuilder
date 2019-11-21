using System;
using System.Linq.Expressions;

namespace LochNessBuilder.Extensions
{
    internal static class ExpressionExtensions
    {
        /// <remarks>See block comment below.</remarks>
        internal static MemberExpression GetMemberExpression<TInstance, TProp>(this Expression<Func<TInstance, TProp>> selector)
        {
            switch (selector?.Body)
            {
                case null:
                    throw new ArgumentNullException(nameof(selector), "Either the expression, or its .Body were null");
                case MemberExpression m:
                    return m;
                case UnaryExpression u when u.Operand is MemberExpression m:
                    return m;
                default:
                    throw new NotSupportedException("The expression passed into the Builder was not a simple member expression. Please pass props ONLY in the form `buildTarget => buildTarget.PropertyToSet` . Expression Type was: " + selector.Type.ToString());
            }
        }

        /* ========================
         * ==  Unary Conversions ==
         * ========================
         *
         * Most calls looks something like this:
         *     `.With(obj => obj.IntProp, 2)`
         *
         * But some calls might look like this:
         *     `.With(obj => obj.ShortProp, 2019)`
         * where the prop type and the argument are not the same, but have implicit casts available.
         *
         * Unfortunately, then compiler turns this into:
         *     `.With<int>(obj => (int)(obj.ShortProp), 2019)`
         * i.e. rather than cast the value, it assumes an implicit cast on the selector expression.
         * This is the scenario that we're trying to handle with these two methods.
         * One to extract the `obj.ShortProp` out in either case, and one to explicitly detect the casting case
         * as we need to know that the `TProp` that we think we have isn't actually correct.
         * (i.e. `obj.ShortProp` is a `short`, not the `int` that we have in `TProp`.)
         *
         * In most *realistic* situations, we will be fine to convert the value, rather than the Property.
         * But it is entirely possible to write code that compiles, but which can't execute:
         *     `.With(obj => obj.ChildTypeProp, new ParentType())`
         * Like in the obj.ShortProp example, this will compile, as it can be interpretted as:
         *     `.With<ParentType>(obj => (ParentType)(obj.ChildTypeProp), new ParentType())`
         * but it cannot possibly succeed, as it is not possible to assign a `new ParentType()` to `obj.ChildTypeProp`.
         * The relevant ArgumentException will be thrown, on the `.With()` call, and the user should be able to fix the mis-usages.
         */

        /// <remarks>See block comment above.</remarks>
        internal static bool IsUnaryConversion<TInstance, TProp>(this Expression<Func<TInstance, TProp>> selector)
        {
            switch (selector?.Body)
            {
                case null:
                    return false;
                case MemberExpression m:
                    return false;
                case UnaryExpression u when u.NodeType == ExpressionType.Convert && u.Operand is MemberExpression m:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Simple helper to fetch the name of the Member being invoked.
        /// </summary>
        internal static string GetMemberName<TInstance, TProp>(this Expression<Func<TInstance, TProp>> selector)
        {
            return GetMemberExpression(selector).Member.Name;
        }
    }
}