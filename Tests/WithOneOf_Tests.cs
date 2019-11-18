using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class WithOneOf_Tests
    {
        [Test]
        public void WithOneOf_CyclesThroughAvailableValues()
        {
            var outputs = Builder<TestObject>.New.WithOneOf(o => o.IntProp, 4, 3, 2, 1).Build(2);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3);
        }

        [Test]
        public void WithOneOf_LoopsOverAvailableValuesWhenNeeded()
        {
            var outputs = Builder<TestObject>.New.WithOneOf(o => o.IntProp, 4, 3).Build(4);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3, 4, 3);
        }

        [Test]
        public void WithOneOf_GivesSameValueGivenOnlyOneValue()
        {
            var outputs = Builder<TestObject>.New.WithOneOf(o => o.IntProp, 4).Build(4);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 4, 4, 4);
        }

        [Test]
        public void WithOneOf_AcceptsAnyFormOfIEnumerable_Array()
        {
            var enumerable = new[] { 4, 3, 2 };
            var outputs = Builder<TestObject>.New.WithOneOf(o => o.IntProp, enumerable).Build(5);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3, 2, 4, 3);
        }

        [Test]
        public void WithOneOf_AcceptsAnyFormOfIEnumerable_List()
        {
            var enumerable = new List<int> { 4, 3, 2 };
            var outputs = Builder<TestObject>.New.WithOneOf(o => o.IntProp, enumerable).Build(5);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3, 2, 4, 3);
        }

        [Test]
        public void WithOneOf_AcceptsAnyFormOfIEnumerable_Queue()
        {
            var enumerable = new Queue<int>( new[] { 4, 3, 2 });
            var outputs = Builder<TestObject>.New.WithOneOf(o => o.IntProp, enumerable).Build(5);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3, 2, 4, 3);
        }

        [Test]
        public void WithOneOf_AcceptsAnyFormOfIEnumerable_Select()
        {
            var enumerable = new[] { 3, 2, 1 }.Select(x => x+1);
            var outputs = Builder<TestObject>.New.WithOneOf(o => o.IntProp, enumerable).Build(5);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3, 2, 4, 3);
        }

        [Test]
        public void WithOneOf_AcceptsAnyFormOfIEnumerable_Yield()
        {
            var enumerable = Yield_4_3_2_TrackingProgress();
            var outputs = Builder<TestObject>.New.WithOneOf(o => o.IntProp, enumerable).Build(5);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3, 2, 4, 3);
        }

        [Test]
        public void WithOneOf_SharesTheGivenObjects()
        {
            var outputs = Builder<TestObject>.New.WithOneOf(o => o.ObjectProp, new object(), new object()).Build(5).ToList();
            outputs[2].ObjectProp.Should().BeSameAs(outputs[0].ObjectProp);
            outputs[4].ObjectProp.Should().BeSameAs(outputs[0].ObjectProp);
            outputs[3].ObjectProp.Should().BeSameAs(outputs[1].ObjectProp);
        }

        private int counter = 0;
        private IEnumerable<int> Yield_4_3_2_TrackingProgress()
        {
            counter++;
            yield return 4;
            counter++;
            yield return 3;
            counter++;
            yield return 2;
        }

        [Test]
        public void WithOneOf_OnlyInvokesTheEnumerableLazily()
        {
            counter = 0;
            var enumerable = Yield_4_3_2_TrackingProgress();
            var builder = Builder<TestObject>.New.WithOneOf(o => o.IntProp, enumerable);
            counter.Should().Be(0);
            builder.Build();
            counter.Should().Be(1);
            builder.Build();
            counter.Should().Be(2);
            builder.Build(1).ToList();
            counter.Should().Be(3);
            builder.Build();
            counter.Should().Be(4);
            builder.Build(3).ToList();
            counter.Should().Be(7);
            builder.Build();
            counter.Should().Be(8);
        }
    }
}