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
            var out0 = outputs[0].LongListProp;
            var out1 = outputs[1].LongListProp;
            out0.Should().HaveCount(3);
            out1.Should().HaveCount(3);
            out0.Should().OnlyHaveUniqueItems();
            out1.Should().OnlyHaveUniqueItems();
            out0.Max().Should().BeLessThan(out1.Min());
        }

        [Test]
        public void WithFactory_Single_DoesNotInvokeTheFactoryMethodUntilAskedToBuild()
        {
            bool hasRun = false;
            Func<int> setup = () => { hasRun = true; return 3; };
            var builder = Builder<TestObject>.New.WithFactory(o => o.ListProp, setup);
            hasRun.Should().BeFalse();
            var output = builder.Build();
            hasRun.Should().BeTrue();
            output.ListProp.First().Should().Be(3);
        }

        [Test]
        public void WithFactory_Single_DoesInvokeTheFactoryMethodToBuildAnIEnumerable()
        {
            bool hasRun = false;
            Func<int> setup = () => { hasRun = true; return 3; };
            var builder = Builder<TestObject>.New.WithFactory(o => o.IEnumerableProp, setup);
            hasRun.Should().BeFalse();
            var output = builder.Build();
            hasRun.Should().BeTrue();

            hasRun = false;
            output.IEnumerableProp.First().Should().Be(3);
            hasRun.Should().BeFalse();
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
            outputs[0].ObjectListProp.Should().NotBeSameAs(outputs[1].ObjectListProp);
        }

        [Test]
        public void WithFactory_Enumerable_CanSetAnIEnumerableProp_Array()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ArrayProp, () => 0).Build();
            output.ArrayProp.Should().NotBeNull();
            output.ArrayProp.Should().HaveCount(3);
            output.ArrayProp.Should().AllBeEquivalentTo(0);
        }

        [Test]
        public void WithFactory_Enumerable_CanSetAnIEnumerableProp_List()
        {
            var counter = 2;
            Func<int> setup = () => counter++; //returns the value it had prior to incrementing ... hence 2, 3, 4.
            var output = Builder<TestObject>.New.WithFactory(o => o.ListProp, setup).Build();
            output.ListProp.Should().NotBeNull();
            output.ListProp.Should().HaveCount(3);
            output.ListProp.First().Should().Be(2);
            output.ListProp.Last().Should().Be(4);
        }

        [Test]
        public void WithFactory_Enumerable_CanSetAnIEnumerableProp_IEnumerable()
        {
            var counter = 1;
            Func<int> setup = () => counter++; //returns the value it had prior to incrementing ... hence 2, 3, 4.
            var output = Builder<TestObject>.New.WithFactory(o => o.IEnumerableProp, setup, 4).Build();
            output.IEnumerableProp.Should().NotBeNull();
            output.IEnumerableProp.Should().HaveCount(4);
            output.IEnumerableProp.First().Should().Be(1);
            output.IEnumerableProp.Last().Should().Be(4);
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
                // Message explains general concept.
                .WithMessage("*From the object values provided, the IEnumerable handler knows how to create*")
                .WithMessage("*Your property type can't be populated by any of those types, and is thus unsupported by this method.*")
                // Message clarifies specific details.
                .WithMessage("*object[], List<object>, HashSet<object>, Queue<object>*")
                .WithMessage("*List`1[Tests.TestSubObject]*");
        }
    }
}