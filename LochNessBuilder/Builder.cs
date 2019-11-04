using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using LochNessBuilder.Extensions;

namespace LochNessBuilder
{
    public sealed class Builder<TInstance> where TInstance : class, new()
    {
        private Builder()
        {
            Blueprint = Enumerable.Empty<Action<TInstance>>();
            PostBuildBlueprint = Enumerable.Empty<Action<TInstance>>();
        }

        private Builder(IEnumerable<Action<TInstance>> blueprint, IEnumerable<Action<TInstance>> postBuildBlueprint)
            : this()
        {
            Blueprint = blueprint;
            PostBuildBlueprint = postBuildBlueprint;
        }

        public static Builder<TInstance> New
        {
            get { return new Builder<TInstance>(); }
        }

        private IEnumerable<Action<TInstance>> Blueprint { get; set; }
        private IEnumerable<Action<TInstance>> PostBuildBlueprint { get; set; }

        public static implicit operator TInstance(Builder<TInstance> source)
        {
            return source.Build();
        }

        public TInstance Build()
        {
            return BuildFromBase(new TInstance());
        }

        public IEnumerable<TInstance> Build(int quantity)
        {
            return quantity.Times(Build);
        }

        public TInstance BuildFromBase(TInstance baseInstance)
        {
            return ApplyBlueprint(baseInstance);
        }

        /// <summary>
        /// Adds an arbitrary action on a TInstance to the builder.
        /// </summary>
        /// <param name="setup">The action to invoke when the builder executes.</param>
        public Builder<TInstance> WithSetup(Action<TInstance> setup)
        {
            return new Builder<TInstance>(Blueprint.Plus(setup), PostBuildBlueprint);
        }

        public Builder<TInstance> WithPostBuildSetup(Action<TInstance> setup)
        {
            return new Builder<TInstance>(Blueprint, PostBuildBlueprint.Plus(setup));
        }

        /// <summary>
        ///     Sets a property on the constructed instance, generated using the default builder for the type of the property.
        /// </summary>
        /// <typeparam name="TProp">The type of the property to set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        public Builder<TInstance> With<TProp>(Expression<Func<TInstance, TProp>> selector) where TProp : class, new()
        {
            var propBuilder = BuilderRegistry.Resolve<TProp>();
            return With(selector, propBuilder.Build());
        }

        /// <summary>
        ///     Sets a collection property on the constructed instance, generating a list using the defact builder for the type of
        ///     the property.
        /// </summary>
        /// <typeparam name="TProp">The type of the property to set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        public Builder<TInstance> With<TProp>(Expression<Func<TInstance, ICollection<TProp>>> selector) where TProp : class, new()
        {
            var propBuilder = BuilderRegistry.Resolve<TProp>();
            return With(selector, 2.Times(propBuilder.Build).ToList());
        }

        /// <summary>
        ///     Sets a property on the constructed instance.
        /// </summary>
        /// <typeparam name="TProp">The type of the property to set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        /// <param name="value">
        ///     The value to assign. Note that this value will be shared between all instances constructed by this
        ///     builder
        /// </param>
        public Builder<TInstance> With<TProp>(Expression<Func<TInstance, TProp>> selector, TProp value)
        {
            var instance = GetInstance();
            var prop = GetProp(selector, instance);
            var val = Expression.Constant(value, typeof(TProp));
            var assign = Expression.Assign(prop, val);

            var result = Expression.Lambda<Action<TInstance>>(assign, instance).Compile();
            return new Builder<TInstance>(Blueprint.Plus(result), PostBuildBlueprint);
        }

        /// <summary>
        ///     Sets a property on the constructed instance.
        /// </summary>
        /// <typeparam name="TProp">The type of the property to set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        /// <param name="values">An enumerable which will be iterated over to obtain a value to assign.</param>
        public Builder<TInstance> With<TProp>(Expression<Func<TInstance, TProp>> selector, IEnumerable<TProp> values)
        {
            var factory = values.Infinite().GetAccessor();
            return WithFactory(selector, factory);
        }

        /// <summary>
        ///     Sets a property of type ICollection on the constructed instance.
        /// </summary>
        /// <typeparam name="TProp">The ICollection type of the property to set.</typeparam>
        /// <param name="selector">A delegate which specifies the property to set.</param>
        /// <param name="values">An enumerable which will be used to construct a Collection to assign to the property.</param>
        public Builder<TInstance> WithCollection<TProp>(Expression<Func<TInstance, ICollection<TProp>>> selector, IEnumerable<TProp> values)
        {
            var collection = new Collection<TProp>(values.ToList());
            return With(selector, collection);
        }

        /// <summary>
        ///     Sets a factory to generate the value of a property.
        /// </summary>
        /// <typeparam name="TProp">The type of the property to set.</typeparam>
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
        ///     Adds an item to a collection on the constructed object, generated using the default builder for the type of the
        ///     collection.
        /// </summary>
        /// <typeparam name="TProp">The type of the object to add.</typeparam>
        /// <param name="selector">A delegate which specifies the target collection.</param>
        public Builder<TInstance> Add<TProp>(Expression<Func<TInstance, ICollection<TProp>>> selector) where TProp : class, new()
        {
            var propBuilder = BuilderRegistry.Resolve<TProp>();
            return Add(selector, propBuilder.Build());
        }

        /// <summary>
        ///     Adds an item to a collection on the constructed object.
        /// </summary>
        /// <typeparam name="TProp">The type of the object to add.</typeparam>
        /// <param name="selector">A delegate which specifies the target collection.</param>
        /// <param name="values">One or more values which will be added to the collection</param>
        public Builder<TInstance> Add<TProp>(Expression<Func<TInstance, ICollection<TProp>>> selector, params TProp[] values) where TProp : class, new()
        {
            var newbuilder = Add(selector, values[0]);
            for (int i = 1; i < values.Length; i++)
            {
                newbuilder = newbuilder.Add(selector, values[i]);
            }
            return newbuilder;
        }

        /// <summary>
        ///     Adds an item to a collection on the constructed object.
        /// </summary>
        /// <typeparam name="TProp">The type of the object to add.</typeparam>
        /// <param name="selector">A delegate which specifies the target collection.</param>
        /// <param name="value">The value which will be added to the collection</param>
        public Builder<TInstance> Add<TProp>(Expression<Func<TInstance, ICollection<TProp>>> selector, TProp value) where TProp : class, new()
        {
            var instance = GetInstance();
            var collectionGetter = typeof(TInstance).GetProperty(selector.GetMemberName()).GetGetMethod();
            var addMethod = typeof(ICollection<TProp>).GetMethod("Add");
            var addParameter = Expression.Constant(value, typeof(TProp));

            var getCollection = Expression.Call(instance, collectionGetter);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            var addItem = Expression.Call(getCollection, addMethod, addParameter);
            var addItemToCollection = Expression.Lambda<Action<TInstance>>(addItem, instance).Compile();

            return new Builder<TInstance>(Blueprint.Plus(addItemToCollection), PostBuildBlueprint);
        }

        private static ParameterExpression GetInstance()
        {
            return Expression.Parameter(typeof(TInstance), typeof(TInstance).FullName);
        }

        private static MemberExpression GetProp<TProp>(Expression<Func<TInstance, TProp>> selector, ParameterExpression instance)
        {
            return Expression.Property(instance, selector.GetMemberName());
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
    }
}
