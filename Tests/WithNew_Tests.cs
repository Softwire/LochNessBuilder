using System.Collections.Generic;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Tests
{
    [TestFixture]
    internal class WithNew_Tests
    {
        [Test]
        public void WithNew_AssignsNewRawObject()
        {
            var output = Builder<TestObject>.New.WithNew(o => o.ObjectProp).Build();
            output.ObjectProp.Should().NotBeNull();
            output.ObjectProp.Should().BeOfType<object>();
        }

        [Test]
        public void WithNew_AssignsNewList()
        {
            var output = Builder<TestObject>.New.WithNew(o => o.ListProp).Build();
            output.ListProp.Should().NotBeNull();
            output.ListProp.Should().BeOfType<List<int>>();
        }

        [Test]
        public void WithNew_AssignsNewCustomObject()
        {
            var output = Builder<TestObject>.New.WithNew(o => o.SubObjectProp).Build();
            output.SubObjectProp.Should().NotBeNull();
            output.SubObjectProp.Should().BeOfType<TestSubObject>();
        }

        #region Multiple Ctors
        public class DedicatedTestCase_MultipleCtors
        {
            public SubObj Sub { get; set; }

            public class SubObj
            {
                public int X { get; set; }

                public SubObj() : this(3) { }
                public SubObj(int x) { X = x + 4; }
            }
        }

        [Test]
        public void WithNew_CanCopeWithMultipleCtorsExisting()
        {
            var output = Builder<DedicatedTestCase_MultipleCtors>.New.WithNew(o => o.Sub).Build();
            output.Sub.Should().NotBeNull();
            output.Sub.X.Should().Be(7); // Not '0' (if it ignored all constructors) or '4' (if it ran new SubObj(default(int))
        }
        #endregion

        #region Class Hiearchies
        public class DedicatedTestCase_ClassHierarchies
        {
            public ParentSub SubP { get; set; }
            public ChildSub SubC { get; set; }

            public class ParentSub { }
            public class ChildSub : ParentSub { }
        }

        [Test]
        public void WithNew_CanCopeClassHiearchies()
        {
            var output = Builder<DedicatedTestCase_ClassHierarchies>.New.WithNew(o => o.SubP).WithNew(o => o.SubC).Build();
            output.SubP.Should().NotBeNull();
            output.SubP.Should().BeOfType<DedicatedTestCase_ClassHierarchies.ParentSub>();
            output.SubC.Should().NotBeNull();
            output.SubC.Should().BeOfType<DedicatedTestCase_ClassHierarchies.ChildSub>();
        }
        #endregion

        #region Registered Builder
        public class DedicatedTestCase_RegisteredBuilder
        {
            public SubObj Sub { get; set; }

            public class SubObj
            {
                public int X { get; set; }
            }
        }

        [BuilderFactory]
        public class RegisteredBuilder
        {
            public static Builder<DedicatedTestCase_RegisteredBuilder.SubObj> New =>
                Builder<DedicatedTestCase_RegisteredBuilder.SubObj>.New
                    .With(o => o.X, 4);
        }

        [Test]
        public void WithNew_IgnoresRegisteredBuilders()
        {
            var output = Builder<DedicatedTestCase_RegisteredBuilder>.New.WithNew(o => o.Sub).Build();
            output.Sub.Should().NotBeNull();
            output.Sub.X.Should().NotBe(4);
            output.Sub.X.Should().Be(0);
        }
        #endregion
    }
}