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
        public void WithSharedRef_AssignsNullToObjectProp()
        {
            var output = Builder<TestObject>.New.WithSharedRef(o => o.ObjectProp, null).Build();
            output.ObjectProp.Should().BeNull();
        }

        [Test]
        public void WithSharedRef_SharesObjectWhenSetupWithThatIntent()
        {
            var outputs = Builder<TestObject>.New.WithSharedRef(o => o.ObjectProp, new object()).Build(2);
            outputs[0].ObjectProp.Should().BeSameAs(outputs[1].ObjectProp);
        }

        [Test]
        public void WithSharedRef_CanSetAnIEnumerableWithoutInvokingIt()
        {
            counter = 0;
            var builder = Builder<TestObject>.New.WithSharedRef(o => o.IEnumerableProp, Yield_1_2_3_WithTracking());
            counter.Should().Be(0);
            var output = builder.Build();
            counter.Should().Be(0);
            output.IEnumerableProp.Last();
            counter.Should().Be(2);
        }

        private int counter = 0;
        private IEnumerable<int> Yield_1_2_3_WithTracking()
        {
            counter++;
            yield return 1;
            yield return 2;
            yield return 3;
            counter++;
        }

        [Test]
        public void With_HandlesBidirectionalCasts()
        {
            var output = Builder<TestObject>.New.With(o => o.ShortProp, 3).Build();
            output.ShortProp.Should().Be(3);
        }

        [Test]
        public void WithSharedRef_ThrowsClearErrorOnMonodirectionalCasts()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithSharedRef(o => o.SubObjectProp, new object());
            builderSetupAction.Should().Throw<ArgumentException>().WithMessage("Expression of type 'System.Object' cannot be used for assignment to type 'Tests.TestSubObject'");
        }

        [Test]
        public void WithSharedRef_ThrowsClearErrorOnCastsForcedByTypeParams()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithSharedRef<object>(o => o.SubObjectProp, new TestObject());
            builderSetupAction.Should().Throw<ArgumentException>().WithMessage("Expression of type 'System.Object' cannot be used for assignment to type 'Tests.TestSubObject'");
        }
    }
}