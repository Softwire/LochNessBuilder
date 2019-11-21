using System;
using System.Collections;
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
        private const int NumberOfElementsToAddToNewIEnumerable = 3;

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
        public List<TInstance> Build(int quantity)
        {
            //We reify this collection, rather than leaving it lazy, to ensure that any side-effects have happened.
            return quantity.Times(Build).ToList();
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

        /*
         * Map of Dependencies.
         *
         * With() : Explicitly implemented
         *
         * WithFactory() : Explicitly implemented
         *  called by WithBuilt/Builder()
         *  called by WithEnumerable()
         *  called by WithOneOf()
         *    called by WithSequentialIds()
         *
         * WithAddToCollection() : Explicitly implemented
         */

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
            var val = Expression.Constant(value, explicitValueType);
            var settingLamda = CreateAssignmentLamdaFromPropExpressionAndValueExpression(selector, val);

            return new Builder<TInstance>(Blueprint.Plus(settingLamda), PostBuildBlueprint);
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
        /// Builds a relevant concrete object from the provided params, to satisfy the property.
        /// </summary>
        /// <typeparam name="TProp">The type of the objects inside the IEnumerable property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the IEnumerable property to set.</param>
        /// <param name="values">One or more values which will be used to construct an appropriate concrete IEnumerable to assign to the property.</param>
        public Builder<TInstance> WithEnumerable<TProp>(Expression<Func<TInstance, IEnumerable<TProp>>> selector, params TProp[] values)
        {
            var propType = GetDeclaredTypeOfIEnumerableProp(selector);
            
            // We want to do this once, up-front, to verify that we can create an appropriate type.
            // So that if we *cant* then we get the error thrown immediately, rather than later.
            var proofOfConcept = GetIEnumerableAsAppropriateType(values, propType);

            return WithFactory(selector, () => GetIEnumerableAsAppropriateType(values, propType), propType);
        }

        /// <summary>
        /// Given an IEnumerable of values, and a TargetType that implements IEnumerable[T],
        /// creates a concrete implementation of IEnumerable which satisfies TargetType.
        /// </summary>
        /// <remarks>
        /// We achieve this by having a bunch of Types that we *know* how to make, and then
        /// checking whether any of these would satisfy the TargetType.
        /// e.g. if TargetType is ISet[T] then a List[T] is no good, by HashSet[T] will be
        /// suitable, so construct that.
        /// </remarks>
        /// <typeparam name="TProp">The type of the objects inside the IEnumerables being handled.</typeparam>
        private IEnumerable<TProp> GetIEnumerableAsAppropriateType<TProp>(IEnumerable<TProp> values, Type targetType)
        {
            var concreteInitialisers = new Dictionary<Type, Func<IEnumerable<TProp>, IEnumerable<TProp>>>
            {
                { typeof(IEnumerable<TProp>), (vals) => vals.ToList() },
                { typeof(IQueryable<TProp>), (vals) => vals.ToList().AsQueryable() },
                { typeof(List<TProp>), (vals) => vals.ToList() },
                { typeof(TProp[]), (vals) => vals.ToArray() },
                { typeof(HashSet<TProp>), (vals) => new HashSet<TProp>(vals) },
                { typeof(Queue<TProp>), (vals) => new Queue<TProp>(vals) },
                { typeof(Stack<TProp>), (vals) => new Stack<TProp>(vals) },
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

            var T = typeof(TProp).Name;
            throw new NotSupportedException($"From the {T} values provided, the IEnumerable handler knows how to create {T}[], List<{T}>, HashSet<{T}>, Queue<{T}>, Collection<{T}>, ReadOnlyCollection<{T}>, or IQueryable<{T}>. Your property type can't be populated by any of those types, and is thus unsupported by this method. Please use a standard .With() call. PropertyType that was to populated was: " + targetType.ToString());
        }
        #endregion

        #region WithFactory
        /// <summary>
        /// Sets a property on the TInstance, using the output from a factory method invoked at Build time.
        /// As a result each TInstance built, will get a different value for the Property.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        /// <param name="valueFactory">A factory method to generate an value for each constructed instance.</param>
        public Builder<TInstance> WithFactory<TProp>(Expression<Func<TInstance, TProp>> selector, Func<TProp> valueFactory)
        {
            return WithFactory(selector, valueFactory, null);
        }

        private Builder<TInstance> WithFactory<TProp>(Expression<Func<TInstance, TProp>> selector, Func<TProp> valueFactory, Type explicitValueType)
        {
            Expression<Func<TProp>> valueInvoker = () => valueFactory();
            var settingLamda = CreateAssignmentLamdaFromPropExpressionAndValueExpression(selector, valueInvoker.Body, explicitValueType);

            return new Builder<TInstance>(Blueprint.Plus(settingLamda), PostBuildBlueprint);
        }

        /// <summary>
        /// Sets an IEnumerable property on the TInstance, using the output from a factory method invoked at Build time.
        /// As a result each TInstance built, will get a different value for the Property.
        /// </summary>
        /// <typeparam name="TProp">The type of the property being set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        /// <param name="valueFactory">A factory method to generate an value for each constructed instance.</param>
        /// <param name="numberOfValues">How many values should be generated and put into the IEnumerable.</param>
        public Builder<TInstance> WithFactory<TProp>(
            Expression<Func<TInstance, IEnumerable<TProp>>> selector,
            Func<TProp> valueFactory,
            int numberOfValues = NumberOfElementsToAddToNewIEnumerable)
        {
            var propType = GetDeclaredTypeOfIEnumerableProp(selector);

            return WithFactory(selector, () => {
                var elements = numberOfValues.Times(valueFactory);
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
        /// <param name="numberOfValues">How many values should be generated and put into the IEnumerable.</param>
        public Builder<TInstance> WithBuilt<TProp>(
            Expression<Func<TInstance, IEnumerable<TProp>>> selector,
            int numberOfValues = NumberOfElementsToAddToNewIEnumerable)
            where TProp : class, new()
        {
            return WithDeferredResolveBuilder(selector, BuilderRegistry.Resolve<TProp>, numberOfValues);
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
        /// <param name="numberOfValues">How many values should be generated and put into the IEnumerable.</param>
        public Builder<TInstance> WithBuilder<TProp>(
            Expression<Func<TInstance, IEnumerable<TProp>>> selector,
            Builder<TProp> builder,
            int numberOfValues = NumberOfElementsToAddToNewIEnumerable)
            where TProp : class, new()
        {
            return WithDeferredResolveBuilder(selector, () => builder, numberOfValues);
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

        private Builder<TInstance> WithDeferredResolveBuilder<TProp>(
            Expression<Func<TInstance, TProp>> selector,
            Func<Builder<TProp>> builderFactory)
            where TProp : class, new()
        {
            return WithFactory(selector, ProduceObjectFactoryThatLazilyResolvesBuilderFactory(builderFactory));
        }

        private Builder<TInstance> WithDeferredResolveBuilder<TProp>(
            Expression<Func<TInstance, IEnumerable<TProp>>> selector,
            Func<Builder<TProp>> builderFactory,
            int numberOfValues = NumberOfElementsToAddToNewIEnumerable)
            where TProp : class, new()
        {
            return WithFactory(selector, ProduceObjectFactoryThatLazilyResolvesBuilderFactory(builderFactory), numberOfValues);
        }
        #endregion

        #region WithSequentialIds
        /// <summary>
        /// Sets a short, int or long property on the TInstance, with sequential values starting from 1.
        /// </summary>
        /// <remarks>
        /// Defining it as populating a long will transparently support all three types.
        /// Technically it could overflow a short, but that will be obvious, and is very unlikely to happen.
        /// </remarks>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        public Builder<TInstance> WithSequentialIds(Expression<Func<TInstance, long>> selector)
        {
            return WithOneOf(selector, Enumerable.Range(1, int.MaxValue).Select(intVal => (long)intVal));
        }
        #endregion

        #region WithAddToCollection
        /// <summary>
        /// Adds an item to an ICollection on the TInstance.
        /// </summary>
        /// <typeparam name="TProp">The type of the objects inside the ICollection.</typeparam>
        /// <param name="selector">A delegate which specifies the target ICollection.</param>
        /// <param name="values">One or more values which will be added to the ICollection</param>
        public Builder<TInstance> WithAddToCollection<TProp>(Expression<Func<TInstance, ICollection<TProp>>> selector, params TProp[] values)
        {
            var builder = this;
            foreach (var value in values)
            {
                builder = builder.WithAddToCollection(selector, value);
            }
            return builder;
        }

        /// <summary>
        /// Adds an item to an ICollection on the TInstance.
        /// </summary>
        /// <typeparam name="TProp">The type of the objects inside the ICollection.</typeparam>
        /// <param name="selector">A delegate which specifies the target ICollection.</param>
        /// <param name="value">The value which will be added to the ICollection</param>
        public Builder<TInstance> WithAddToCollection<TProp>(Expression<Func<TInstance, ICollection<TProp>>> selector, TProp value)
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

        private static Action<TInstance> CreateAssignmentLamdaFromPropExpressionAndValueExpression<TProp>(
            Expression<Func<TInstance, TProp>> propSelector,
            Expression valueExpression,
            Type valueTypeOverride = null)
        {
            var instance = GetInstance();
            var prop = GetProp(propSelector, instance);

            if (valueTypeOverride != null)
            {
                valueExpression = Expression.Convert(valueExpression, valueTypeOverride);
            }
            else if (propSelector.IsUnaryConversion())
            {
                // In this case TProp doesn't actually represent the type of the Property, so we need to cast the 
                // value to the correct type, and assign the result of that cast.
                // Note that this is the opposite of the cast that the compiler has inferred, and thus the reverse cast isn't guaranteed to work!
                // But it should work in every reasonable use-case and will error in an understandable way.
                // See the Notes on IsUnaryConversion for further details.
                valueExpression = Expression.Convert(valueExpression, prop.Type);
            }

            var assign = Expression.Assign(prop, valueExpression);

            return Expression.Lambda<Action<TInstance>>(assign, instance).Compile();
        }
        #endregion
    }
}
