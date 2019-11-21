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
    public class WithFactory_Enumerable_Tests
    {
        [Test]
        public void WithFactory_Enumerable_CanSetAnIEnumerableProp_Array()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ArrayProp, () => new int[2]).Build();
            output.ArrayProp.Should().NotBeNull();
            output.ArrayProp.Should().HaveCount(2);
            output.ArrayProp.Should().AllBeEquivalentTo(0);
        }

        [Test]
        public void WithFactory_Enumerable_CanSetAnIEnumerableProp_List()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ListProp, () => new List<int> { 2, 3, 4 }).Build();
            output.ListProp.Should().NotBeNull();
            output.ListProp.Should().HaveCount(3);
            output.ListProp.First().Should().Be(2);
            output.ListProp.Last().Should().Be(4);
        }

        [Test]
        public void WithFactory_Enumerable_CanSetAnIEnumerableProp_IEnumerable()
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
        public void WithFactory_Enumerable_ThrowsClearErrorOnMonodirectionalCasts()
        {
            Action builderSetupAction = () => Builder<TestObject>.New.WithFactory(o => o.SubObjectListProp, () => new object());
            builderSetupAction.Should().Throw<ArgumentException>().WithMessage("Expression of type '*[System.Object]' cannot be used for assignment to type '*[Tests.TestSubObject]'");
        }

    }
}