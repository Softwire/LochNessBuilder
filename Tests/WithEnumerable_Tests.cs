using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class WithCreateEnumerableFrom_Tests
    {
        [Test]
        public void WithCreateEnumerableFrom_SetsAProperty()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ArrayProp, 1, 2).Build();
            output.ArrayProp.Should().NotBeNull();
            output.ArrayProp.Should().BeEquivalentTo(1, 2);
        }

        [Test]
        public void WithCreateEnumerableFrom_DoesNotReuseTheOutputObject()
        {
            var outputs = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ArrayProp, 1, 2).Build(2);
            outputs[1].ArrayProp.Should().NotBeSameAs(outputs[0].ArrayProp);
        }

        [Test]
        public void WithCreateEnumerableFrom_DoesNotUseTheInputObjectDirectlyIfArrayIsPassedIn()
        {
            var originalArrayObject = new[] { 1, 2 };
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ArrayProp, originalArrayObject).Build();
            output.ArrayProp.Should().NotBeSameAs(originalArrayObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_DoesNotUseTheInputObjectDirectlyIfArrayIsPassedIn_EvenForIEnumerableProps()
        {
            var originalArrayObject = new[] { 1, 2 };
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IEnumerableProp, originalArrayObject).Build();
            output.IEnumerableProp.Should().NotBeSameAs(originalArrayObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_DoesNotUseTheInputObjectDirectlyIfArrayIsPassedIn_EvenForIQueryableProps()
        {
            var originalArrayObject = new[] { 1, 2 };
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IQueryableProp, originalArrayObject).Build();
            output.IEnumerableProp.Should().NotBeSameAs(originalArrayObject);
        }

        #region PopulatesMostFormsOfConcreteIEnumerable
        [Test]
        public void WithCreateEnumerableFrom_PopulatesMostFormsOfConcreteIEnumerable_Array()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ArrayProp, 1, 2).Build();
            output.ArrayProp.Should().BeOfType<int[]>();
            output.ArrayProp.Should().BeEquivalentTo(1, 2);
            output.ArrayProp[0].Should().Be(1);
            output.ArrayProp[1].Should().Be(2);
            output.ArrayProp.Length.Should().Be(2);
        }

        [Test]
        public void WithCreateEnumerableFrom_PopulatesMostFormsOfConcreteIEnumerable_List()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ListProp, 1, 2).Build();
            output.ListProp.Should().BeOfType<List<int>>();
            output.ListProp.Should().BeEquivalentTo(1, 2);
            output.ListProp[0].Should().Be(1);
            output.ListProp[1].Should().Be(2);
            output.ListProp.Should().HaveCount(2);
        }

        [Test]
        public void WithCreateEnumerableFrom_PopulatesMostFormsOfConcreteIEnumerable_Queue()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.QueueProp, 1, 2).Build();
            output.QueueProp.Should().BeOfType<Queue<int>>();
            output.QueueProp.Should().BeEquivalentTo(1, 2);
            output.QueueProp.Dequeue().Should().Be(1);
            output.QueueProp.Dequeue().Should().Be(2);
            output.QueueProp.Should().BeEmpty();
        }

        [Test]
        public void WithCreateEnumerableFrom_PopulatesMostFormsOfConcreteIEnumerable_Stack()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.StackProp, 1, 2).Build();
            output.StackProp.Should().BeOfType<Stack<int>>();
            output.StackProp.Should().BeEquivalentTo(1, 2);
            output.StackProp.Pop().Should().Be(2);
            output.StackProp.Pop().Should().Be(1);
            output.StackProp.Should().BeEmpty();
        }

        [Test]
        public void WithCreateEnumerableFrom_PopulatesMostFormsOfConcreteIEnumerable_HashSet()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.HashSetProp, 1, 2).Build();
            output.HashSetProp.Should().BeOfType<HashSet<int>>();
            output.HashSetProp.Should().BeEquivalentTo(1, 2);
            output.HashSetProp.Should().Contain(1);
            output.HashSetProp.Should().Contain(2);
            output.HashSetProp.Should().HaveCount(2);
        }

        [Test]
        public void WithCreateEnumerableFrom_PopulatesMostFormsOfConcreteIEnumerable_Collection()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.CollectionProp, 1, 2).Build();
            output.CollectionProp.Should().BeOfType<Collection<int>>();
            output.CollectionProp.Should().BeEquivalentTo(1, 2);
            output.CollectionProp.Should().HaveCount(2);
            output.CollectionProp.Should().Contain(1);
            output.CollectionProp.Should().Contain(2);
        }

        [Test]
        public void WithCreateEnumerableFrom_PopulatesMostFormsOfConcreteIEnumerable_ReadOnlyCollection()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ReadOnlyCollectionProp, 1, 2).Build();
            output.ReadOnlyCollectionProp.Should().BeOfType<ReadOnlyCollection<int>>();
            output.ReadOnlyCollectionProp.Should().BeEquivalentTo(1, 2);
            output.ReadOnlyCollectionProp.Should().HaveCount(2);
            output.ReadOnlyCollectionProp.Should().Contain(1);
            output.ReadOnlyCollectionProp.Should().Contain(2);
        }
        #endregion

        [Test]
        public void WithCreateEnumerableFrom_PopulatesAPureIEnumerable()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IEnumerableProp, 1, 2).Build();
            output.IEnumerableProp.Should().BeEquivalentTo(1, 2);
            output.IEnumerableProp.First().Should().Be(1);
            output.IEnumerableProp.Last().Should().Be(2);
            output.IEnumerableProp.Count().Should().Be(2);
        }

        [Test]
        public void WithCreateEnumerableFrom_PopulatesMostInterfacesDerivingFromIEnumerable_IQueryable()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IQueryableProp, 1, 2).Build();
            output.IQueryableProp.Should().BeEquivalentTo(1, 2);
            output.IQueryableProp.First().Should().Be(1);
            output.IQueryableProp.Last().Should().Be(2);
            output.IQueryableProp.Count().Should().Be(2);
        }

        #region PopulatesMostInterfacesDerivingFromIEnumerable
        [Test]
        public void WithCreateEnumerableFrom_PopulatesMostInterfacesDerivingFromIEnumerable_ICollection()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ICollectionProp, 1, 2).Build();
            output.ICollectionProp.Should().BeOfType<List<int>>();
            output.ICollectionProp.Should().BeEquivalentTo(1, 2);
            output.ICollectionProp.Should().HaveCount(2);
            output.ICollectionProp.Should().Contain(1);
            output.ICollectionProp.Should().Contain(2);
        }

        [Test]
        public void WithCreateEnumerableFrom_PopulatesMostInterfacesDerivingFromIEnumerable_IList()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IListProp, 1, 2).Build();
            output.IListProp.Should().BeOfType<List<int>>();
            output.IListProp.Should().BeEquivalentTo(1, 2);
            output.IListProp[0].Should().Be(1);
            output.IListProp[1].Should().Be(2);
            output.IListProp.Should().HaveCount(2);
        }

        [Test]
        public void WithCreateEnumerableFrom_PopulatesMostInterfacesDerivingFromIEnumerable_ISet()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ISetProp, 1, 2).Build();
            output.ISetProp.Should().BeOfType<HashSet<int>>();
            output.ISetProp.Should().BeEquivalentTo(1, 2);
            output.ISetProp.Should().Contain(1);
            output.ISetProp.Should().Contain(2);
            output.ISetProp.Should().HaveCount(2);
        }

        [Test]
        public void WithCreateEnumerableFrom_PopulatesMostInterfacesDerivingFromIEnumerable_IReadOnlyCollection()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IReadOnlyCollectionProp, 1, 2).Build();
            output.IReadOnlyCollectionProp.Should().BeOfType<List<int>>();
            output.IReadOnlyCollectionProp.Should().BeEquivalentTo(1, 2);
            output.IReadOnlyCollectionProp.Should().HaveCount(2);
            output.IReadOnlyCollectionProp.Should().Contain(1);
            output.IReadOnlyCollectionProp.Should().Contain(2);
        }
        #endregion

        [Test]
        public void WithCreateEnumerableFrom_ThrowsASensibleErrorIfUnableToPopulateAConcreteIEnumerableProperty()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ConcurrentBagProp, 1, 2);
            builderSetupAction.Should().Throw<NotSupportedException>()
                .WithMessage("*From the Int32 values provided, the IEnumerable handler knows how to create Int32[], List<Int32>, HashSet<Int32>, Queue<Int32>, Collection<Int32>, ReadOnlyCollection<Int32>, or IQueryable<Int32>. Your property type can't be populated by any of those types, and is thus unsupported by this method. Please use a standard .With() call.*")
                .WithMessage("*ConcurrentBag`1[System.Int32]*");
        }

        [Test]
        public void WithCreateEnumerableFrom_ThrowsASensibleErrorIfUnableToPopulateAnIEnumerableInterfaceroperty()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IOrderedEnumerableProp, 1, 2);
            builderSetupAction.Should().Throw<NotSupportedException>()
                .WithMessage("*From the Int32 values provided, the IEnumerable handler knows how to create Int32[], List<Int32>, HashSet<Int32>, Queue<Int32>, Collection<Int32>, ReadOnlyCollection<Int32>, or IQueryable<Int32>. Your property type can't be populated by any of those types, and is thus unsupported by this method. Please use a standard .With() call.*")
                .WithMessage("*IOrderedEnumerable`1[System.Int32]*");
        }

        [Test]
        public void WithCreateEnumerableFrom_HandlesPopulatingWithBiggerNumericTypes()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.LongListProp, 1, 2).Build();
            output.LongListProp.Should().BeOfType<List<long>>();
        }

        [Test]
        public void WithCreateEnumerableFrom_HandlesPopulatingWithBiggerObjectTypes()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ObjectListProp, new TestSubObject(), new TestSubObject()).Build();
            output.ObjectListProp.Should().BeOfType<List<object>>();
        }

        [Test]
        public void WithCreateEnumerableFrom_ForcesCastingToSmallerNumericTypes()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom<short>(o => o.ShortListProp, 1, 2).Build(); //This won't compile without the '<short>' Method TypeParam, or explicit '(short)' casts.
            output.ShortListProp.Should().BeOfType<List<short>>();
        }

        [Test]
        public void WithCreateEnumerableFrom_ThrowsClearErrorWhenPopulatingWithSmallerObjectTypes()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.SubObjectListProp, new object(), new object());
            builderSetupAction.Should().Throw<NotSupportedException>()
                .WithMessage("*From the object values provided, the IEnumerable handler knows how to create object[], List<object>, HashSet<object>, Queue<object>, Collection<object>, ReadOnlyCollection<object>, or IQueryable<object>. Your property type can't be populated by any of those types, and is thus unsupported by this method. Please use a standard .With() call.*")
                .WithMessage("*List`1[Tests.TestSubObject]*");
        }

        [Test]
        public void WithCreateEnumerableFrom_HandlesMixedTypeParams()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.LongListProp, 3, 4l, (short)2).Build();
            output.LongListProp.Should().BeOfType<List<long>>();
        }
    }
}