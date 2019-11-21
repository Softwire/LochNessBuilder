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
    public class WithFactory_Single_Tests
    {
        [Test]
        public void WithFactory_Single_SetsTheProperty()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.IntProp, () => 3).Build();
            output.IntProp.Should().Be(3);
        }

        [Test]
        public void WithFactory_Single_CanOutputADifferentValueEachTime_MultiBuild()
        {
            var outputs = Builder<TestObject>.New.WithFactory(o => o.IntProp, () => DateTime.Now.Ticks).Build(3);
            outputs[2].IntProp.Should().NotBe(outputs[0].IntProp);
        }

        [Test]
        public void WithFactory_Single_CanOutputADifferentValueEachTime_DiscreteBuild()
        {
            var builder = Builder<TestObject>.New.WithFactory(o => o.IntProp, () => DateTime.Now.Ticks);
            var output1 = builder.Build();
            var output2 = builder.Build();
            output2.IntProp.Should().NotBe(output1.IntProp);
        }

        [Test]
        public void WithFactory_Single_CanDirectlySetAnIEnumerableProp_Array()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ArrayProp, () => new int[2]).Build();
            output.ArrayProp.Should().NotBeNull();
            output.ArrayProp.Should().HaveCount(2);
            output.ArrayProp.Should().AllBeEquivalentTo(0);
        }

        [Test]
        public void WithFactory_Single_CanDirectlySetAnIEnumerableProp_List()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ListProp, () => new List<int> { 2, 3, 4 }).Build();
            output.ListProp.Should().NotBeNull();
            output.ListProp.Should().HaveCount(3);
            output.ListProp.First().Should().Be(2);
            output.ListProp.Last().Should().Be(4);
        }

        [Test]
        public void WithFactory_Single_CanDirectlySetAnIEnumerableProp_IEnumerable()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.IEnumerableProp, Yield_1_2_3).Build();
            output.IEnumerableProp.Should().NotBeNull();
            output.IEnumerableProp.Should().HaveCount(3);
            output.IEnumerableProp.First().Should().Be(1);
            output.IEnumerableProp.Last().Should().Be(3);
        }

        private IEnumerable<int> Yield_1_2_3()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }

        [Test]
        public void WithFactory_Single_CanUseAStaticMethod()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.IntProp, StaticSetupMethod).Build();
            output.IntProp.Should().Be(2);
        }
        private static int StaticSetupMethod() => 2;

        [Test]
        public void WithFactory_Single_CanUseANonStaticMethod()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.IntProp, this.NonStaticSetupMethod).Build();
            output.IntProp.Should().Be(3);
        }
        private int NonStaticSetupMethod() => 3;

        [Test]
        public void WithFactory_Single_DoesNotShareTheResultantObject_MultiBuild()
        {
            var outputs = Builder<TestObject>.New.WithFactory(o => o.ObjectProp, () => new object()).Build(2);
            outputs[1].ObjectProp.Should().NotBeSameAs(outputs[0].ObjectProp);
        }

        [Test]
        public void WithFactory_Single_DoesNotShareTheResultantObject_DiscreteBuild()
        {
            var builder = Builder<TestObject>.New.WithFactory(o => o.ObjectProp, () => new object());
            var output1 = builder.Build();
            var output2 = builder.Build();
            output2.ObjectProp.Should().NotBeSameAs(output1.ObjectProp);
        }

        [Test]
        public void WithFactory_Single_HandlesBidirectionalCasts()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ShortProp, () => 3).Build();
            output.ShortProp.Should().Be(3);
        }

        [Test]
        public void WithFactory_Single_ThrowsClearErrorOnMonodirectionalCasts()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithFactory(o => o.SubObjectProp, () => new object());
            builderSetupAction.Should().Throw<ArgumentException>().WithMessage("Expression of type 'System.Object' cannot be used for assignment to type 'Tests.TestSubObject'");
        }

    }
}