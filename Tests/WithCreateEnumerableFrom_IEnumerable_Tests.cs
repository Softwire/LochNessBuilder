using System;
using System.Collections;
using System.Collections.Concurrent;
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
    internal class WithCreateEnumerableFrom_UsingIEnumerable_Tests
    {
        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_SetsAProperty()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ArrayProp, new List<int>{1, 2}).Build();
            output.ArrayProp.Should().NotBeNull();
            output.ArrayProp.Should().BeEquivalentTo(1, 2);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotReuseTheOutputObject()
        {
            var outputs = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ArrayProp, 1, 2).Build(2);
            outputs[1].ArrayProp.Should().NotBeSameAs(outputs[0].ArrayProp);
        }

        #region Does Not Reuse Object for exact Concrete type match
        // We can't commonise these because we want to ensure that nothing odd happens with the Type system,
        // which commonisation could muck around with.
        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfAppropriateTypeIsPassedIn_Array()
        {
            var originalObject = new int[0];
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ArrayProp, originalObject).Build();
            output.ArrayProp.Should().BeOfType(originalObject.GetType());
            output.ArrayProp.Should().NotBeNull();
            output.ArrayProp.Should().NotBeSameAs(originalObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfAppropriateTypeIsPassedIn_List()
        {
            var originalObject = new List<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ListProp, originalObject).Build();
            output.ListProp.Should().BeOfType(originalObject.GetType());
            output.ListProp.Should().NotBeNull();
            output.ListProp.Should().NotBeSameAs(originalObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfAppropriateTypeIsPassedIn_Queue()
        {
            var originalObject = new Queue<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.QueueProp, originalObject).Build();
            output.QueueProp.Should().BeOfType(originalObject.GetType());
            output.QueueProp.Should().NotBeNull();
            output.QueueProp.Should().NotBeSameAs(originalObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfAppropriateTypeIsPassedIn_Stack()
        {
            var originalObject = new Stack<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.StackProp, originalObject).Build();
            output.StackProp.Should().BeOfType(originalObject.GetType());
            output.StackProp.Should().NotBeNull();
            output.StackProp.Should().NotBeSameAs(originalObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfAppropriateTypeIsPassedIn_Set()
        {
            var originalObject = new HashSet<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.HashSetProp, originalObject).Build();
            output.HashSetProp.Should().BeOfType(originalObject.GetType());
            output.HashSetProp.Should().NotBeNull();
            output.HashSetProp.Should().NotBeSameAs(originalObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfAppropriateTypeIsPassedIn_Collection()
        {
            var originalObject = new Collection<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.CollectionProp, originalObject).Build();
            output.CollectionProp.Should().BeOfType(originalObject.GetType());
            output.CollectionProp.Should().NotBeNull();
            output.CollectionProp.Should().NotBeSameAs(originalObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfAppropriateTypeIsPassedIn_ReadOnlyCollection()
        {
            var originalObject = new List<int>().AsReadOnly();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ReadOnlyCollectionProp, originalObject).Build();
            output.ReadOnlyCollectionProp.Should().BeOfType(originalObject.GetType());
            output.ReadOnlyCollectionProp.Should().NotBeNull();
            output.ReadOnlyCollectionProp.Should().NotBeSameAs(originalObject);
        }
        #endregion

        #region Does Not Reuse Object for Interface type match
        // We can't commonise these because we want to ensure that nothing odd happens with the Type system,
        // which commonisation could muck around with.
        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfItSatisfiesThePropInterfaceType_IEnumerabl()
        {
            var originalObject = new int[0];
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IEnumerableProp, originalObject).Build();
            output.IEnumerableProp.Should().NotBeNull();
            output.IEnumerableProp.Should().NotBeSameAs(originalObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfItSatisfiesThePropInterfaceType_IQueryable()
        {
            var originalObject = new Queue<int>().AsQueryable();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IQueryableProp, originalObject).Build();
            output.IQueryableProp.Should().NotBeNull();
            output.IQueryableProp.Should().NotBeSameAs(originalObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfItSatisfiesThePropInterfaceType_IList()
        {
            var originalObject = new List<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IListProp, originalObject).Build();
            output.IListProp.Should().NotBeNull();
            output.IListProp.Should().NotBeSameAs(originalObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfItSatisfiesThePropInterfaceType_ISet()
        {
            var originalObject = new HashSet<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ISetProp, originalObject).Build();
            output.ISetProp.Should().NotBeNull();
            output.ISetProp.Should().NotBeSameAs(originalObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfItSatisfiesThePropInterfaceType_ICollection()
        {
            var originalObject = new Collection<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ICollectionProp, originalObject).Build();
            output.ICollectionProp.Should().NotBeNull();
            output.ICollectionProp.Should().NotBeSameAs(originalObject);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotUseTheInputObjectDirectlyIfItSatisfiesThePropInterfaceType_IReadOnlyCollection()
        {
            var originalObject = new List<int>().AsReadOnly();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IReadOnlyCollectionProp, originalObject).Build();
            output.IReadOnlyCollectionProp.Should().NotBeNull();
            output.IReadOnlyCollectionProp.Should().NotBeSameAs(originalObject);
        }
        #endregion

        #region Can Cross Populate IEnumerables Of a Different Concrete Type
        // We can't commonise these because we want to ensure that nothing odd happens with the Type system,
        // which commonisation could muck around with.
        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_CanCrossPopulateIEnumerablesOfADifferentConcreteType_ListToArray()
        {
            var originalObject = new List<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ArrayProp, originalObject).Build();
            output.ArrayProp.Should().BeOfType<int[]>();
            output.ArrayProp.Should().NotBeNull();
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_CanCrossPopulateIEnumerablesOfADifferentConcreteType_ArrayToQueue()
        {
            var originalObject = new int[0];
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.QueueProp, originalObject).Build();
            output.QueueProp.Should().BeOfType<Queue<int>>();
            output.QueueProp.Should().NotBeNull();
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_CanCrossPopulateIEnumerablesOfADifferentConcreteType_QueueToHashSet()
        {
            var originalObject = new Queue<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.HashSetProp, originalObject).Build();
            output.HashSetProp.Should().BeOfType<HashSet<int>>();
            output.HashSetProp.Should().NotBeNull();
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_CanCrossPopulateIEnumerablesOfADifferentConcreteType_HashSetToStack()
        {
            var originalObject = new HashSet<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.StackProp, originalObject).Build();
            output.StackProp.Should().BeOfType<Stack<int>>();
            output.StackProp.Should().NotBeNull();
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_CanCrossPopulateIEnumerablesOfADifferentConcreteType_StackToCollection()
        {
            var originalObject = new Stack<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.CollectionProp, originalObject).Build();
            output.CollectionProp.Should().BeOfType<Collection<int>>();
            output.CollectionProp.Should().NotBeNull();
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_CanCrossPopulateIEnumerablesOfADifferentConcreteType_CollectionToReadOnlyCollection()
        {
            var originalObject = new Collection<int>();
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ReadOnlyCollectionProp, originalObject).Build();
            output.ReadOnlyCollectionProp.Should().BeOfType<ReadOnlyCollection<int>>();
            output.ReadOnlyCollectionProp.Should().NotBeNull();
        }

        #endregion

        #region Laziness
        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_DoesNotEvaluateIEnumerableOnSetupButDoesEvaluateRepeatedlyOnBuild()
        {
            var originalObject = Yield_1_2_3_WithTracking();
            counter = 0;
            var configuredBuilder = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IEnumerableProp, originalObject);
            counter.Should().Be(0);
            var output1 = configuredBuilder.Build();
            counter.Should().Be(2);
            var output2 = configuredBuilder.Build();
            counter.Should().Be(4);
        }

        private int counter = 0;
        private IEnumerable<int> Yield_1_2_3_WithTracking()
        {
            counter++;
            yield return 1;
            yield return 2;
            yield return 3;
            counter++;
        }
        #endregion

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_PopulatesFromATypeThatIsNotMentionedInTheBuilderCodebase()
        {
            //Just in case there was somethign special about the type being in our list of concrete initialisers!?
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ArrayProp, new ConcurrentBag<int>{1, 2}).Build();
            output.ArrayProp.Should().BeOfType<int[]>();
            output.ArrayProp.Length.Should().Be(2);
            output.ArrayProp.Should().Contain(1); //Order isn't reliable, since Bag is unordered
            output.ArrayProp.Should().Contain(2);
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_ThrowsASensibleErrorIfUnableToPopulateAConcreteIEnumerableProperty()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ConcurrentBagProp, new List<int>());
            builderSetupAction.Should().Throw<NotSupportedException>()
                // Message explains general concept.
                .WithMessage("*From the Int32 values provided, the IEnumerable handler knows how to create*")
                .WithMessage("*Your property type can't be populated by any of those types, and is thus unsupported by this method.*")
                // Message clarifies specific details.
                .WithMessage("*Int32[], List<Int32>, HashSet<Int32>, Queue<Int32>*")
                .WithMessage("*ConcurrentBag`1[System.Int32]*");
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_ThrowsASensibleErrorIfUnableToPopulateAnIEnumerableInterfaceroperty()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.IOrderedEnumerableProp, new List<int>());
            builderSetupAction.Should().Throw<NotSupportedException>()
                // Message explains general concept.
                .WithMessage("*From the Int32 values provided, the IEnumerable handler knows how to create*")
                .WithMessage("*Your property type can't be populated by any of those types, and is thus unsupported by this method.*")
                // Message clarifies specific details.
                .WithMessage("*Int32[], List<Int32>, HashSet<Int32>, Queue<Int32>*")
                .WithMessage("*IOrderedEnumerable`1[System.Int32]*");
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_HandlesPopulatingWithBiggerObjectTypes()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.ObjectListProp, new List<TestSubObject>{new TestSubObject(), new TestSubObject()}).Build();
            output.ObjectListProp.Should().BeOfType<List<object>>();
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_ThrowsClearErrorWhenPopulatingWithSmallerObjectTypes()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.SubObjectListProp, new List<object> { new object(), new object() });
            builderSetupAction.Should().Throw<NotSupportedException>()
                // Message explains general concept.
                .WithMessage("*From the object values provided, the IEnumerable handler knows how to create*")
                .WithMessage("*Your property type can't be populated by any of those types, and is thus unsupported by this method.*")
                // Message clarifies specific details.
                .WithMessage("*object[], List<object>, HashSet<object>, Queue<object>*")
                .WithMessage("*List`1[Tests.TestSubObject]*");
        }

        [Test]
        public void WithCreateEnumerableFrom_UsingIEnumerable_HandlesMixedTypeParams()
        {
            var output = Builder<TestObject>.New.WithCreateEnumerableFrom(o => o.LongListProp, 3, 4L, (short)2).Build();
            output.LongListProp.Should().BeOfType<List<long>>();
        }
    }
}