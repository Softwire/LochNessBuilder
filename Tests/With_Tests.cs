using System;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    internal class With_Tests
    {
        [Test]
        public void With_AssignsValue()
        {
            var output = Builder<TestObject>.New.With(o => o.IntProp, 3).Build();
            output.IntProp.Should().Be(3);
        }

        [Test]
        public void With_AssignsValuesToNullableProp()
        {
            var output = Builder<TestObject>.New.With(o => o.NullableIntProp, 3).Build();
            output.NullableIntProp.Should().Be(3);
        }

        [Test]
        public void With_AssignsNullToNullableProp()
        {
            var output = Builder<TestObject>.New.With(o => o.NullableIntProp, null).Build();
            output.NullableIntProp.Should().BeNull();
        }

        [Test]
        public void With_AssignsValueToStringProps()
        {
            var output = Builder<TestObject>.New.With(o => o.StringProp, "Hello").Build();
            output.StringProp.Should().Be("Hello");
        }

        [Test]
        public void With_AssignsNullToStringProps()
        {
            var output = Builder<TestObject>.New.With(o => o.StringProp, null).Build();
            output.StringProp.Should().BeNull();
        }

        [Test]
        public void With_AssignsNullToObjectProp()
        {
            var output = Builder<TestObject>.New.WithSharedRef(o => o.ObjectProp, null).Build();
            output.ObjectProp.Should().BeNull();
        }

        [Test]
        public void With_SharesObjectWhenSetupWithThatIntent()
        {
            var outputs = Builder<TestObject>.New.WithSharedRef(o => o.ObjectProp, new object()).Build(2);
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
            Action builderSetupAction = () => Builder<TestObject>.New.WithSharedRef(o => o.SubObjectProp, new object());
            builderSetupAction.Should().Throw<ArgumentException>().WithMessage("Expression of type 'System.Object' cannot be used for assignment to type 'Tests.TestSubObject'");
        }

        [Test]
        public void With_ThrowsClearErrorOnCastsForcedByTypeParams()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithSharedRef<object>(o => o.SubObjectProp, new TestObject());
            builderSetupAction.Should().Throw<ArgumentException>().WithMessage("Expression of type 'System.Object' cannot be used for assignment to type 'Tests.TestSubObject'");
        }
    }
}