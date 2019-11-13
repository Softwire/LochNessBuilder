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

            foreach (var builderType in builders)
            {
                RegisterMostAppropriateGetter(builderType);
            }
        }

        private static void RegisterMostAppropriateGetter(Type builderType)
        {
            var publicStaticGetProperties =
                builderType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty);
            var propsReturningABuilder = publicStaticGetProperties
                .Where(prop =>
                    prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(Builder<>))
                .ToList();

            switch (propsReturningABuilder.Count)
            {
                case 0:
                    throw new NotImplementedException(
                        $"The type {builderType} is marked as a builder, but has no public static getter returning a Builder<>");
                case 1:
                    RegisterPropertyGetterAsBuilderFactory(propsReturningABuilder.Single());
                    break;
                default:
                    // There are more than one candidate Prop. Pick one, use it, ignore the others.
                    var propsCalledNew = propsReturningABuilder.Where(prop => prop.Name == "New").ToList();
                    var propToUse = propsCalledNew.FirstOrDefault() ?? propsReturningABuilder.First();
                    RegisterPropertyGetterAsBuilderFactory(propToUse);
                    break;
            }
        }

        private static void RegisterPropertyGetterAsBuilderFactory(PropertyInfo propReturningABuilder)
        {
            var builderGetMethod = propReturningABuilder.GetGetMethod();
            var typeBuilt = builderGetMethod.ReturnType.GetGenericArguments()[0];
            BuilderFactoryMethods.Add(typeBuilt, builderGetMethod);
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