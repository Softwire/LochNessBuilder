using System;
using System.Linq;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class WithFactory_Tests
    {
        [Test]
        public void WithFactory_Works()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.IntProp, () => 3).Build();
            output.IntProp.Should().Be(3);
        }

        [Test]
        public void WithFactory_DoesNotReUseTheResultantObject()
        {
            var outputs = Builder<TestObject>.New.WithFactory(o => o.ObjectProp, () => new object()).Build(2);
            outputs[0].ObjectProp.Should().NotBeSameAs(outputs[1].ObjectProp);
        }

        [Test]
        public void With_HandlesBidirectionalCasts()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ShortProp, () => 3).Build();
            output.ShortProp.Should().Be(3);
        }

        [Test]
        public void With_ThrowsClearErrorOnMonodirectionalCasts()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithFactory(o => o.SubObjectProp, () => new object());
            builderSetupAction.Should().Throw<ArgumentException>().WithMessage("Expression of type 'System.Object' cannot be used for assignment to type 'Tests.TestSubObject'");
        }

    }
}