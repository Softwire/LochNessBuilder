using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LochNessBuilder
{
    /// <summary>
    /// Resolves a builder for a type. Builders decorated with the Builder attribute take precedence, otherwise a generic builder is used.
    /// </summary>
    static class BuilderRegistry
    {
        private static readonly Dictionary<Type, Type> Builders = new Dictionary<Type, Type>();

        static BuilderRegistry()
        {
            var builders = typeof(BuilderRegistry).Assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(BuilderAttribute), false).Any());

            foreach (var builder in builders)
            {
                var newMethod = builder.GetProperty("New", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                var instanceType = newMethod.ReturnType.GetGenericArguments()[0];
                Builders.Add(instanceType, builder);
            }
        }

        public static Builder<TInstance> Resolve<TInstance>() where TInstance : class, new()
        {
            Type builderType;

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (Builders.ContainsKey(typeof(TInstance)))
            {
                builderType = Builders[typeof(TInstance)];
            }
            else
            {
                builderType = typeof(Builder<>).MakeGenericType(typeof(TInstance));
            }

            var newMethod = builderType.GetProperty("New", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
            return newMethod.Invoke(null, null) as Builder<TInstance>;
        }
    }
}