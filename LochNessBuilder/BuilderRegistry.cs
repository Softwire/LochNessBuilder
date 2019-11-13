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
        private static readonly Dictionary<Type, MethodInfo> BuilderFactoryMethods = new Dictionary<Type, MethodInfo>();

        static BuilderRegistry()
        {
            var builders = typeof(BuilderRegistry).Assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(BuilderAttribute), false).Any());

            foreach (var builder in builders)
            {
                var newMethod = builder.GetProperty("New", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                var instanceType = newMethod.ReturnType.GetGenericArguments()[0];
                BuilderFactoryMethods.Add(instanceType, newMethod);
            }
        }

        public static Builder<TInstance> Resolve<TInstance>() where TInstance : class, new()
        {
            if (!BuilderFactoryMethods.ContainsKey(typeof(TInstance)))
            {
                //Return a blank builder, which will just create `new TInstance()`.
                return Builder<TInstance>.New;
            }

            var builderFactoryMethod = BuilderFactoryMethods[typeof(TInstance)];
            return builderFactoryMethod.Invoke(null, null) as Builder<TInstance>;
        }
    }
}