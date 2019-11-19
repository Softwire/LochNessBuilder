using System;
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
        public void WithFactory_SetsTheProperty()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.IntProp, () => 3).Build();
            output.IntProp.Should().Be(3);
        }

        [Test]
        public void WithFactory_CanOutputADifferentValueEachTime()
        {
            var outputs = Builder<TestObject>.New.WithFactory(o => o.IntProp, () => DateTime.Now.Ticks).Build(3);
            outputs[2].IntProp.Should().NotBe(outputs[0].IntProp);
        }

        [Test]
        public void WithFactory_CanUseAStaticMethod()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.IntProp, StaticSetupMethod).Build();
            output.IntProp.Should().Be(2);
        }
        private static int StaticSetupMethod() => 2;

        [Test]
        public void WithFactory_CanUseANonStaticMethod()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.IntProp, this.NonStaticSetupMethod).Build();
            output.IntProp.Should().Be(3);
        }
        private int NonStaticSetupMethod() => 3;

        [Test]
        public void WithFactory_DoesNotReUseTheResultantObject()
        {
            var outputs = Builder<TestObject>.New.WithFactory(o => o.ObjectProp, () => new object()).Build(2);
            outputs[1].ObjectProp.Should().NotBeSameAs(outputs[0].ObjectProp);
        }

        [Test]
        public void WithFactory_HandlesBidirectionalCasts()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ShortProp, () => 3).Build();
            output.ShortProp.Should().Be(3);
        }

        [Test]
        public void WithFactory_ThrowsClearErrorOnMonodirectionalCasts()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithFactory(o => o.SubObjectProp, () => new object());
            builderSetupAction.Should().Throw<ArgumentException>().WithMessage("Expression of type 'System.Object' cannot be used for assignment to type 'Tests.TestSubObject'");
        }

    }
}