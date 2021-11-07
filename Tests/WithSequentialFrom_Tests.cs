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
    internal class WithSequentialFrom_Tests
    {
        [Test]
        public void WithSequentialFrom_CyclesThroughAvailableValues()
        {
            var outputs = Builder<TestObject>.New.WithSequentialFrom(o => o.IntProp, 4, 3, 2, 1).Build(2);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3);
        }

        [Test]
        public void WithSequentialFrom_LoopsOverAvailableValuesWhenNeeded()
        {
            var outputs = Builder<TestObject>.New.WithSequentialFrom(o => o.IntProp, 4, 3).Build(4);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3, 4, 3);
        }

        [Test]
        public void WithSequentialFrom_GivesSameValueGivenOnlyOneValue()
        {
            var outputs = Builder<TestObject>.New.WithSequentialFrom(o => o.IntProp, 4).Build(4);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 4, 4, 4);
        }

        #region AcceptsAnyFormOfIEnumerable
        [Test]
        public void WithSequentialFrom_AcceptsAnyFormOfIEnumerable_Array()
        {
            var enumerable = new[] { 4, 3, 2 };
            var outputs = Builder<TestObject>.New.WithSequentialFrom(o => o.IntProp, enumerable).Build(5);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3, 2, 4, 3);
        }

        [Test]
        public void WithSequentialFrom_AcceptsAnyFormOfIEnumerable_List()
        {
            var enumerable = new List<int> { 4, 3, 2 };
            var outputs = Builder<TestObject>.New.WithSequentialFrom(o => o.IntProp, enumerable).Build(5);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3, 2, 4, 3);
        }

        [Test]
        public void WithSequentialFrom_AcceptsAnyFormOfIEnumerable_Queue()
        {
            var enumerable = new Queue<int>( new[] { 4, 3, 2 });
            var outputs = Builder<TestObject>.New.WithSequentialFrom(o => o.IntProp, enumerable).Build(5);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3, 2, 4, 3);
        }

        [Test]
        public void WithSequentialFrom_AcceptsAnyFormOfIEnumerable_Select()
        {
            var enumerable = new[] { 3, 2, 1 }.Select(x => x+1);
            var outputs = Builder<TestObject>.New.WithSequentialFrom(o => o.IntProp, enumerable).Build(5);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3, 2, 4, 3);
        }

        [Test]
        public void WithSequentialFrom_AcceptsAnyFormOfIEnumerable_Yield()
        {
            var enumerable = Yield_4_3_2_TrackingProgress();
            var outputs = Builder<TestObject>.New.WithSequentialFrom(o => o.IntProp, enumerable).Build(5);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(4, 3, 2, 4, 3);
        }
        #endregion

        [Test]
        public void WithSequentialFrom_SharesTheGivenObjects()
        {
            var outputs = Builder<TestObject>.New.WithSequentialFrom(o => o.ObjectProp, new object(), new object()).Build(5);
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
        public void WithSequentialFrom_OnlyInvokesTheEnumerableLazily()
        {
            counter = 0;
            var enumerable = Yield_4_3_2_TrackingProgress();
            var builder = Builder<TestObject>.New.WithSequentialFrom(o => o.IntProp, enumerable);
            counter.Should().Be(0);
            builder.Build();
            counter.Should().Be(1);
            builder.Build();
            counter.Should().Be(2);
            builder.Build(1);
            counter.Should().Be(3);
            builder.Build();
            counter.Should().Be(4);
            builder.Build(3);
            counter.Should().Be(7);
            builder.Build();
            counter.Should().Be(8);
        }

        [Test]
        public void WithSequentialFrom_HandlesBidirectionalCasts()
        {
            var output = Builder<TestObject>.New.WithSequentialFrom(o => o.ShortProp, 3, 4).Build();
            output.ShortProp.Should().Be(3);
        }

        [Test]
        public void WithSequentialFrom_HandlesMixedTypeParams()
        {
            var output = Builder<TestObject>.New.WithSequentialFrom(o => o.ShortProp, 3, 4L, (short)2).Build();
            output.ShortProp.Should().Be(3);
        }

        [Test]
        public void WithSequentialFrom_ThrowsClearErrorOnMonodirectionalCasts()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithSequentialFrom(o => o.SubObjectProp, new object(), new object());
            builderSetupAction.Should().Throw<ArgumentException>().WithMessage("Expression of type 'System.Object' cannot be used for assignment to type 'Tests.TestSubObject'");
        }
    }
}