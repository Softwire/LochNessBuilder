using System.Linq;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    internal class WithSequentialIds_Tests
    {
        [Test]
        public void WithSequentialIds_AssignsValue()
        {
            var output = Builder<TestObject>.New.WithSequentialIds(o => o.IntProp).Build();
            output.IntProp.Should().Be(1);
        }

        [Test]
        public void WithSequentialIds_AssignsSequentialValues()
        {
            var outputs = Builder<TestObject>.New.WithSequentialIds(o => o.IntProp).Build(4);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(1, 2, 3, 4);
        }

        [Test,
         TestCase(1, new[] { 1, 2, 3, 4 }),
         TestCase(0, new[] { 0, 1, 2, 3 }),
         TestCase(5, new[] { 5, 6, 7, 8 }),
         TestCase(-10, new[] { -10,-9, -8, -7 }),
         TestCase(-2, new[] { -2, -1, 0, 1 })
        ]
        public void WithSequentialIds_AssignsSequentialValues_StartingAtInputValue(int startingValue, int[] outputValues)
        {
            var outputs = Builder<TestObject>.New.WithSequentialIds(o => o.IntProp, startingValue).Build(4);
            outputs.Select(o => o.IntProp).Should().BeEquivalentTo(outputValues);
        }

        [Test]
        public void WithSequentialIds_CanAssignToAShortId()
        {
            var outputs = Builder<TestObject>.New.WithSequentialIds(o => o.ShortProp).Build(2);
            outputs[1].ShortProp.Should().Be(2);
        }

        [Test]
        public void WithSequentialIds_CanAssignToALongId()
        {
            var outputs = Builder<TestObject>.New.WithSequentialIds(o => o.LongProp).Build(2);
            outputs[1].LongProp.Should().Be(2);
        }

        [Test]
        public void WithSequentialIds_AssignsCalculatedValue()
        {
            var output = Builder<TestObject>.New.WithSequentialIds(o => o.StringProp, x => $"Hello {x}").Build();
            output.StringProp.Should().Be("Hello 1");
        }

        [Test]
        public void WithSequentialIds_AssignsSequentialCalculatedValues()
        {
            var outputs = Builder<TestObject>.New.WithSequentialIds(o => o.StringProp, x => $"Hello {x*x}").Build(4);
            outputs.Select(o => o.StringProp).Should().BeEquivalentTo("Hello 1", "Hello 4", "Hello 9", "Hello 16");
        }


    }
}