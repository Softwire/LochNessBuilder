using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    internal class WithFactory_Enumerable_Tests
    {
        [Test]
        public void WithFactory_Enumerable_SetsAndPopulatesTheProperty()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ListProp, () => 5, 4).Build();
            output.ListProp.Should().NotBeNull();
            output.ListProp.Should().HaveCount(4);
            output.ListProp.Should().AllBeEquivalentTo(5);
        }

        [Test]
        public void WithFactory_Enumerable_UsesTheExpectedDefaultPopulationCount()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ListProp, () => 2).Build();
            output.ListProp.Should().HaveCount(3);
        }

        [Test]
        public void WithFactory_Enumerable_CanPopulateTheListWithVariedValues()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.LongListProp, () => DateTime.Now.Ticks).Build();
            output.LongListProp.Should().HaveCount(3);
            output.LongListProp.Should().OnlyHaveUniqueItems();
            output.LongListProp.Should().BeInAscendingOrder();
        }

        [Test]
        public void WithFactory_Enumerable_PopulatesTheWholeFirstListBeforeStartingTheNext_WhenMultiBuilding()
        {
            var outputs = Builder<TestObject>.New.WithFactory(o => o.LongListProp, () => DateTime.Now.Ticks).Build(2);
            var result = outputs[0].LongListProp.Concat(outputs[1].LongListProp).ToList();
            result.Should().HaveCount(6);
            result.Should().OnlyHaveUniqueItems();
            result.Should().BeInAscendingOrder();
        }

        [Test]
        public void WithFactory_Enumerable_DoesNotShareTheContentsOfTheList()
        {
            var outputs = Builder<TestObject>.New.WithFactory(o => o.ObjectListProp, () => new object()).Build(2);
            outputs.SelectMany(o => o.ObjectListProp).Should().OnlyHaveUniqueItems();
        }

        [Test]
        public void WithFactory_Enumerable_DoesNotShareTheListItself()
        {
            var outputs = Builder<TestObject>.New.WithFactory(o => o.ObjectListProp, () => new object()).Build(2);
            outputs.Select(o => o.ObjectListProp).Should().OnlyHaveUniqueItems();
        }

        [Test]
        public void WithFactory_Enumerable_CanSetAnIEnumerableProp_Array()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ArrayProp, () => new int[2]).Build();
            output.ArrayProp.Should().NotBeNull();
            output.ArrayProp.Should().HaveCount(2);
            output.ArrayProp.Should().AllBeEquivalentTo(0);
        }

        [Test]
        public void WithFactory_Enumerable_CanSetAnIEnumerableProp_List()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ListProp, () => new List<int> { 2, 3, 4 }).Build();
            output.ListProp.Should().NotBeNull();
            output.ListProp.Should().HaveCount(3);
            output.ListProp.First().Should().Be(2);
            output.ListProp.Last().Should().Be(4);
        }

        [Test]
        public void WithFactory_Enumerable_CanSetAnIEnumerableProp_IEnumerable()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.IEnumerableProp, Yield_1_2_3).Build();
            output.IEnumerableProp.Should().NotBeNull();
            output.IEnumerableProp.Should().HaveCount(3);
            output.IEnumerableProp.First().Should().Be(1);
            output.IEnumerableProp.Last().Should().Be(3);
        }

        private IEnumerable<int> Yield_1_2_3()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }

        [Test]
        public void WithFactory_Enumerable_RequiresExplicitCastsForPopulatingASmallerTypeCollectionWithALargerType()
        {
            Builder<TestObject>.New.WithFactory(o => o.ShortListProp, () => (short)3).Build(); //Won't compile without '(short)' cast.
        }

        [Test]
        public void WithFactory_Enumerable_HandlesBidirectionalCastsFrom()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.LongListProp, () => 3).Build();
            output.LongListProp.Should().NotBeNull();
        }

        [Test]
        public void WithFactory_Enumerable_ThrowsClearErrorOnMonodirectionalCasts()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithFactory(o => o.SubObjectListProp, () => new object());
            builderSetupAction.Should().Throw<NotSupportedException>()
                .WithMessage("*From the object values provided, the IEnumerable handler knows how to create object[], List<object>, HashSet<object>, Queue<object>, Collection<object>, ReadOnlyCollection<object>, or IQueryable<object>. Your property type can't be populated by any of those types, and is thus unsupported by this method. Please use a standard .With() call.*")
                .WithMessage("*List`1[Tests.TestSubObject]*");
        }
    }
}