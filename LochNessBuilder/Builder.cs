using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using LochNessBuilder.Extensions;

namespace LochNessBuilder
{
    /// <summary>
    /// This builder will be given a series of steps, before being asked to build an
    /// instance of TInstance.
    /// It will 'new()' up an object, and then apply each step to that object, in order.  
    /// </summary>
    /// <typeparam name="TInstance">The Type to be constructed</typeparam>
    public sealed class Builder<TInstance> where TInstance : class, new()
    {
        private Builder(IEnumerable<Action<TInstance>> blueprint = null, IEnumerable<Action<TInstance>> postBuildBlueprint = null)
        {
            Blueprint = blueprint ?? Enumerable.Empty<Action<TInstance>>();
            PostBuildBlueprint = postBuildBlueprint ?? Enumerable.Empty<Action<TInstance>>();
        }

        /// <summary>
        /// Returns a blank builder for the type, with no steps configured.
        /// </summary>
        public static Builder<TInstance> New => new Builder<TInstance>();

        private IEnumerable<Action<TInstance>> Blueprint { get; }
        private IEnumerable<Action<TInstance>> PostBuildBlueprint { get; }
        private int _numberOfElementsToAddToNewIEnumerable = 3;

        /// <summary>
        /// Construct a TInstance and apply all configured steps.
        /// </summary>
        public TInstance Build()
        {
            return BuildFromBase(new TInstance());
        }

        /// <summary>
        /// Construct multiple TInstances, applying all configured steps to each one in turn.
        /// (All steps are apply to the first TInstance, before the 2nd one is constructed.)
        /// </summary>
        public IEnumerable<TInstance> Build(int quantity)
        {
            return quantity.Times(Build);
        }

        /// <summary>
        /// Take an existing object, and apply all of the setup steps to that object.
        /// </summary>
        public TInstance BuildFromBase(TInstance baseInstance)
        {
            return ApplyBlueprint(baseInstance);
        }

        private TInstance ApplyBlueprint(TInstance result)
        {
            foreach (var action in Blueprint)
            {
                action(result);
            }

            foreach (var action in PostBuildBlueprint)
            {
                action(result);
            }

            return result;
        }

        #region Arbitrary Action Setups
        /// <summary>
        /// Perform an arbitrary action on the TInstance, immediately after `new()`ing it up, *before* any other steps are applied.
        /// </summary>
        public Builder<TInstance> WithPreBuildSetup(Action<TInstance> setup)
        {
            return new Builder<TInstance>(setup.Plus(Blueprint), PostBuildBlueprint);
        }

        /// <summary>
        /// Performs an arbitrary action on the TInstance, in order with other steps defined.
        /// </summary>
        public Builder<TInstance> WithSetup(Action<TInstance> setup)
        {
            return new Builder<TInstance>(Blueprint.Plus(setup), PostBuildBlueprint);
        }

        /// <summary>
        /// Performs an arbitrary action on the TInstance, *after* all other steps are complete (even steps added to the builder later).
        /// </summary>
        public Builder<TInstance> WithPostBuildSetup(Action<TInstance> setup)
        {
            return new Builder<TInstance>(Blueprint, PostBuildBlueprint.Plus(setup));
        }
        #endregion

        #region With
        /// <summary>
        /// Sets a property on the TInstance.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        /// <param name="value">
        /// The value to assign.<br/>
        /// Note that this value will be shared between all instances constructed by this builder
        /// </param>
        public Builder<TInstance> With<TProp>(Expression<Func<TInstance, TProp>> selector, TProp value)
        {
            return With(selector, value, typeof(TProp));
        }

        private Builder<TInstance> With<TProp>(Expression<Func<TInstance, TProp>> selector, TProp value, Type explicitValueType)
        {
            var instance = GetInstance();
            var prop = GetProp(selector, instance);
            var val = Expression.Constant(value, explicitValueType);
            var assign = Expression.Assign(prop, val);

            var result = Expression.Lambda<Action<TInstance>>(assign, instance).Compile();
            return new Builder<TInstance>(Blueprint.Plus(result), PostBuildBlueprint);
        }
        #endregion

        #region WithOneOf
        /// <summary>
        /// Sets a property on the TInstance, based of a set of available values, looping if necessary.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        /// <param name="values">An IEnumerable which will be iterated over to obtain single values to assign.</param>
        public Builder<TInstance> WithOneOf<TProp>(Expression<Func<TInstance, TProp>> selector, IEnumerable<TProp> values)
        {
            var factory = values.LoopInfinitely().GetAccessor();
            return WithFactory(selector, factory);
        }

        /// <summary>
        /// Sets a property on the TInstance, based of a set of available values, looping if necessary.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the IEnumerable property to set.</param>
        /// <param name="values">One or more values which will be looped over to obtain single values to assign.</param>
        public Builder<TInstance> WithOneOf<TProp>(Expression<Func<TInstance, TProp>> selector, params TProp[] values)
        {
            return WithOneOf(selector, (IEnumerable<TProp>)values);
        }
        #endregion

        #region WithEnumerable
        /// <summary>
        /// Sets a property of type IEnumerable on the constructed instance.
        /// Builds a relevant concrete object from the provided IEnumerable, to satisfy the property.
        /// </summary>
        /// <typeparam name="TProp">The type of the objects inside the IEnumerable property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the IEnumerable property to set.</param>
        /// <param name="values">An IEnumerable which will be used to construct an appropriate concrete IEnumerable to assign to the property.</param>
        public Builder<TInstance> WithEnumerable<TProp>(Expression<Func<TInstance, IEnumerable<TProp>>> selector, IEnumerable<TProp> values)
        {
            var propType = GetDeclaredTypeOfIEnumerableProp(selector);
            var valuesInTypedIEnumerable = GetIEnumerableAsAppropriateType(values, propType);
            return With(selector, valuesInTypedIEnumerable, propType);
        }

        /// <summary>
        /// Sets a property of type IEnumerable on the constructed instance.
        /// </summary>
        /// <typeparam name="TProp">The type of the objects inside the IEnumerable property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the IEnumerable property to set.</param>
        /// <param name="values">One or more values which will be used to construct an appropriate concrete IEnumerable to assign to the property.</param>
        public Builder<TInstance> WithEnumerable<TProp>(Expression<Func<TInstance, IEnumerable<TProp>>> selector, params TProp[] values)
        {
            return WithEnumerable(selector, (IEnumerable<TProp>) values);
        }

        private IEnumerable<TProp> GetIEnumerableAsAppropriateType<TProp>(IEnumerable<TProp> values, Type targetType)
        {
            var concreteInitialisers = new Dictionary<Type, Func<IEnumerable<TProp>, IEnumerable<TProp>>>
            {
                { typeof(IEnumerable<TProp>), (vals) => vals },
                { typeof(IQueryable<TProp>), (vals) => vals.AsQueryable() },
                { typeof(TProp[]), (vals) => vals.ToArray() },
                { typeof(List<TProp>), (vals) => vals.ToList() },
                { typeof(HashSet<TProp>), (vals) => new HashSet<TProp>(vals) },
                { typeof(Queue<TProp>), (vals) => new Queue<TProp>(vals) },
                { typeof(Collection<TProp>), (vals) => new Collection<TProp>(vals.ToList()) },
                { typeof(ReadOnlyCollection<TProp>), (vals) => vals.ToList().AsReadOnly() },
            };

            foreach (var concreteType in concreteInitialisers.Keys)
            {
                if (targetType.IsAssignableFrom(concreteType))
                {
                    var valuesInRelevantConcreteType = concreteInitialisers[concreteType](values);
                    return valuesInRelevantConcreteType;
                }
            }
            
            throw EnumerableTypeNotSupportedException(targetType);
        }

        private Exception EnumerableTypeNotSupportedException(Type propType)
        {
            throw new NotSupportedException("The IEnumerable handler knows how to create Array, List<>, HashSet<>, Queue<>, Collection<>, ReadOnlyCollection<>, or IQueryable<>. Your property type can't be populated by any of those types, and is thus unsupported by this method. Please use a standard .With() call. PropertyType was :" + propType.ToString());
        }
        #endregion

        #region WithFactory
        /// <summary>
        /// Sets a property on the TInstance, using the output from a factory method invoked at Build time.
        /// As a result each TInstance built, will get a different value for the Property.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        /// <param name="value">A factory method to generate an value for each constructed instance.</param>
        public Builder<TInstance> WithFactory<TProp>(Expression<Func<TInstance, TProp>> selector, Func<TProp> value)
        {
            var instance = GetInstance();
            var prop = GetProp(selector, instance);
            Expression<Func<TProp>> valueInvoker = () => value();

            var setExpression = Expression.Assign(prop, Expression.Invoke(valueInvoker));
            var setLambda = Expression.Lambda<Action<TInstance>>(setExpression, instance).Compile();

            return new Builder<TInstance>(Blueprint.Plus(setLambda), PostBuildBlueprint);
        }

        /// <summary>
        /// Sets an IEnumerable property on the TInstance, using the output from a factory method invoked at Build time.
        /// As a result each TInstance built, will get a different value for the Property.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        /// <param name="value">A factory method to generate an value for each constructed instance.</param>
        public Builder<TInstance> WithFactory<TProp>(Expression<Func<TInstance, IEnumerable<TProp>>> selector, Func<TProp> valueFactory)
        {
            var propType = GetDeclaredTypeOfIEnumerableProp(selector);

            return WithFactory(selector, () =>
            {
                var elements = _numberOfElementsToAddToNewIEnumerable.Times(valueFactory);
                var typedElementsObject = GetIEnumerableAsAppropriateType(elements, propType);
                return typedElementsObject;
            });
        }
        #endregion

        #region WithBuilt/Builder
        /// <summary>
        /// Sets a property on the TInstance, with a value built by the registered builder for the type of the property (or just new object() if nothing registered)
        /// </summary>
        /// <typeparam name="TProp">The type of the property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        public Builder<TInstance> WithBuilt<TProp>(Expression<Func<TInstance, TProp>> selector) where TProp : class, new()
        {
            return WithDeferredResolveBuilder(selector, BuilderRegistry.Resolve<TProp>);
        }

        /// <summary>
        /// Sets an IEnumerable property on the TInstance, with values built by the registered builder for the type inside the property (or just new object() if nothing registered)
        /// </summary>
        /// <typeparam name="TProp">The type of the objects inside the IEnumerable property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        public Builder<TInstance> WithBuilt<TProp>(Expression<Func<TInstance, IEnumerable<TProp>>> selector) where TProp : class, new()
        {
            return WithDeferredResolveBuilder(selector, BuilderRegistry.Resolve<TProp>);
        }

        /// <summary>
        /// Sets a property on the TInstance, with a value built by the provided builder.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        /// <param name="builder">The builder object to use to create the value.</param>
        public Builder<TInstance> WithBuilder<TProp>(Expression<Func<TInstance, TProp>> selector, Builder<TProp> builder) where TProp : class, new()
        {
            return WithDeferredResolveBuilder(selector, () => builder);
        }

        /// <summary>
        /// Sets an IEnumerable property on the TInstance, with values built by the provided builder
        /// </summary>
        /// <typeparam name="TProp">The type of the objects inside the IEnumerable property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        /// <param name="builder">The builder object to use to create the values.</param>
        public Builder<TInstance> WithBuilder<TProp>(Expression<Func<TInstance, IEnumerable<TProp>>> selector, Builder<TProp> builder) where TProp : class, new()
        {
            return WithDeferredResolveBuilder(selector, () => builder);
        }

        private Func<TProp> ProduceObjectFactoryThatLazilyResolvesBuilderFactory<TProp>(Func<Builder<TProp>> builderFactory) where TProp : class, new()
        {
            Builder<TProp> capturedBuilder = null;
            Func<TProp> actionToResolveBuilderOnceAndThenUseItRepeatedly = () =>
            {
                if (capturedBuilder == null)
                {
                    capturedBuilder = builderFactory();
                }

                var element = capturedBuilder.Build();
                return element;
            };

            return actionToResolveBuilderOnceAndThenUseItRepeatedly;
        }

        private Builder<TInstance> WithDeferredResolveBuilder<TProp>(Expression<Func<TInstance, TProp>> selector, Func<Builder<TProp>> builderFactory) where TProp : class, new()
        {
            return WithFactory(selector, ProduceObjectFactoryThatLazilyResolvesBuilderFactory(builderFactory));
        }

        private Builder<TInstance> WithDeferredResolveBuilder<TProp>(Expression<Func<TInstance, IEnumerable<TProp>>> selector, Func<Builder<TProp>> builderFactory) where TProp : class, new()
        {
            return WithFactory(selector, ProduceObjectFactoryThatLazilyResolvesBuilderFactory(builderFactory));
        }
        #endregion

        #region WithSequentialIds
        /// <summary>
        /// Sets an integer property on the TInstance, with sequential values starting from 1.
        /// </summary>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        public Builder<TInstance> WithSequentialIds(Expression<Func<TInstance, int>> selector)
        {
            return WithOneOf(selector, Enumerable.Range(1, int.MaxValue));
        }
        #endregion

        #region Add
        /// <summary>
        /// Adds an item to an ICollection on the TInstance.
        /// </summary>
        /// <typeparam name="TProp">The type of the objects inside the ICollection.</typeparam>
        /// <param name="selector">A delegate which specifies the target ICollection.</param>
        /// <param name="values">One or more values which will be added to the ICollection</param>
        public Builder<TInstance> Add<TProp>(Expression<Func<TInstance, ICollection<TProp>>> selector, params TProp[] values)
        {
            var builder = this;
            foreach (var value in values)
            {
                builder = builder.Add(selector, value);
            }
            return builder;
        }

        /// <summary>
        /// Adds an item to an ICollection on the TInstance.
        /// </summary>
        /// <typeparam name="TProp">The type of the objects inside the ICollection.</typeparam>
        /// <param name="selector">A delegate which specifies the target ICollection.</param>
        /// <param name="value">The value which will be added to the ICollection</param>
        public Builder<TInstance> Add<TProp>(Expression<Func<TInstance, ICollection<TProp>>> selector, TProp value)
        {
            var instance = GetInstance();
            var collectionGetter = typeof(TInstance).GetProperty(selector.GetMemberName()).GetGetMethod();
            var addMethod = typeof(ICollection<TProp>).GetMethod("Add");
            var addParameter = Expression.Constant(value, typeof(TProp));

            var getCollection = Expression.Call(instance, collectionGetter);
            var addItem = Expression.Call(getCollection, addMethod, addParameter);
            var addItemToCollection = Expression.Lambda<Action<TInstance>>(addItem, instance).Compile();

            return new Builder<TInstance>(Blueprint.Plus(addItemToCollection), PostBuildBlueprint);
        }
        #endregion

        #region Object reflection helpers
        private static ParameterExpression GetInstance()
        {
            return Expression.Parameter(typeof(TInstance), typeof(TInstance).FullName);
        }

        /// <remarks>
        /// Note that the tempting refactor to invoke GetInstance() directly inside here won't work.
        /// This is because of how expression-invocation work, which means that we'll often need
        /// to have the same ParameterExperssion object here as we do outside this method. 
        /// </remarks>
        private static MemberExpression GetProp<TProp>(Expression<Func<TInstance, TProp>> selector, ParameterExpression instance)
        {
            return Expression.Property(instance, selector.GetMemberName());
        }

        /// <remarks>
        /// We can't use `selector.ReturnType` as it would always return IEnumerable, even if the
        /// declared type of the property is some specific implementation of IEnumerable.
        /// </remarks>
        private static Type GetDeclaredTypeOfIEnumerableProp<TProp>(Expression<Func<TInstance, IEnumerable<TProp>>> selector)
        {
            var instance = GetInstance();
            var prop = GetProp(selector, instance);
            return prop.Type;
        }
        #endregion
    }
}
