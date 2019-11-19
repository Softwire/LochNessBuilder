using System.Linq;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class WithSequentialIds_Tests
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
    }
}