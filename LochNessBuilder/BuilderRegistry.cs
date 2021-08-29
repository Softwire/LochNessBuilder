using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LochNessBuilder
{
    /// <summary>
    /// Resolves a builder for a type. Builders decorated with the Builder attribute take precedence, otherwise a "generic" builder is used.
    /// </summary>
    static class BuilderRegistry
    {
        private static readonly Dictionary<Type, List<MethodInfo>> BuilderFactoryMethods = new Dictionary<Type, List<MethodInfo>>();

        static BuilderRegistry()
        {
            var builderTypes = IdentifyTypesTaggedAsBuilders();

            foreach (var builderType in builderTypes)
            {
                RegisterMostAppropriateGetter(builderType);
            }
        }

        private static IEnumerable<Type> IdentifyTypesTaggedAsBuilders()
        {
            var allAccessibleAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var possiblyRelevantAssemblies = allAccessibleAssemblies.Where(ass => !ass.FullName.StartsWith("System") && !ass.FullName.StartsWith("Microsoft")).ToList();

            var builderTypes = new List<Type>();
            foreach (var assembly in possiblyRelevantAssemblies)
            {
                try
                {
                    var builderDecoratedTypes =
                        assembly
                            .GetExportedTypes()
                            .Where(t => t.GetCustomAttribute<BuilderFactoryAttribute>() != null)
                            .ToList();
                    builderTypes.AddRange(builderDecoratedTypes);
                }
                catch
                {
                    // Suppress any errors, since this functionality isn't critical, and almost all errors here will
                    // be about assemblies that are in some way innaccessible, and hence weren't interesting assemblies.
                }
            }

            return builderTypes;
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

            if (!propsReturningABuilder.Any())
            {
                throw new NotImplementedException(
                    $"The type '{builderType.FullName}' is marked as a builder, but has no public static getter returning a Builder<>");
            }

            foreach (var builderProp in propsReturningABuilder)
            {
                RegisterPropertyGetterAsBuilderFactory(builderProp);
            }
        }

        private static void RegisterPropertyGetterAsBuilderFactory(PropertyInfo propReturningABuilder)
        {
            var builderGetMethod = propReturningABuilder.GetGetMethod();
            var typeBuilt = builderGetMethod.ReturnType.GetGenericArguments()[0];

            if (!BuilderFactoryMethods.ContainsKey(typeBuilt))
            {
                BuilderFactoryMethods.Add(typeBuilt, new List<MethodInfo>());
            }

            BuilderFactoryMethods[typeBuilt].Add(builderGetMethod);
        }

        public static Builder<TInstance> Resolve<TInstance>() where TInstance : class, new()
        {
            if (!BuilderFactoryMethods.ContainsKey(typeof(TInstance)))
            {
                //Return a blank builder, which will just create `new TInstance()`.
                return Builder<TInstance>.New;
            }

            var availableFactories = BuilderFactoryMethods[typeof(TInstance)];
            if (availableFactories.Count > 1)
            {
                throw new NotSupportedException($"There are multiple BuilderFactories defined for type '{typeof(TInstance).FullName}'. Please use `.WithBuilder()` to specify which one should be used, rather than `WithBuilt()` since it is unable to infer the correct one.");
            }
            var builderFactoryMethod = availableFactories.Single();
            return builderFactoryMethod.Invoke(null, null) as Builder<TInstance>;
        }
    }
}