using System;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class With_Tests
    {
        [Test]
        public void With_AssignsValue()
        {
            var output = Builder<TestObject>.New.With(o => o.IntProp, 3).Build();
            output.IntProp.Should().Be(3);
        }

        [Test]
        public void With_SharesObject()
        {
            var outputs = Builder<TestObject>.New.With(o => o.ObjectProp, new object()).Build(2);
            outputs[0].ObjectProp.Should().BeSameAs(outputs[1].ObjectProp);
        }

        [Test]
        public void With_HandlesBidirectionalCasts()
        {
            var output = Builder<TestObject>.New.With(o => o.ShortProp, 3).Build();
            output.ShortProp.Should().Be(3);
        }

        [Test]
        public void With_ThrowsClearErrorOnMonodirectionalCasts()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.With(o => o.SubObjectProp, new object());
            builderSetupAction.Should().Throw<ArgumentException>().WithMessage("Expression of type 'System.Object' cannot be used for assignment to type 'Tests.TestSubObject'");
        }
    }
}