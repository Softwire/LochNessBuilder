using System;
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
            public SubObject SubObjectProp { get; set; }

            public class SubObject
            {
                public int X { get; set; }

                public SubObject() : this(3) { }
                public SubObject(int x) { X = x + 4; }
            }
        }

        [Test]
        public void WithNew_UsesParameterlessConstructor_WhenMultipleContructorsExist()
        {
            var output = Builder<DedicatedTestCase_MultipleCtors>.New.WithNew(o => o.SubObjectProp).Build();
            output.SubObjectProp.Should().NotBeNull();
            output.SubObjectProp.X.Should().Be(7); // Not '0' (if it ignored all constructors) or '4' (if it ran new SubObject(default(int))
        }

        public class DedicatedTestCase_MultipleCtorsButNoneParameterless
        {
            public SubObject SubObjectProp { get; set; }

            public class SubObject
            {
                public int X { get; set; }

                public SubObject(int x) { X = x + 4; }
                public SubObject(string y) { }
            }
        }

        [Test]
        public void WithNew_EnforcesAtCompileTimeThatAParameterlessConstructorMustExist()
        {
            //Builder<DedicatedTestCase_MultipleCtorsButNoneParameterless>.New.WithNew(o => o.SubObjectProp);
            Assert.Pass("Because the line above does not compile, stating that DedicatedTestCase_MultipleCtorsButNoneParameterless.SubObjectProp must have a public parameterless constructor.");
        }
        #endregion

        #region Class Hiearchies
        public class DedicatedTestCase_ClassHierarchies
        {
            public ParentSub SubObjectParent { get; set; }
            public ChildSub SubObjectChild { get; set; }

            public class ParentSub { }
            public class ChildSub : ParentSub { }
        }

        [Test]
        public void WithNew_CanCopeClassHiearchies()
        {
            var output = Builder<DedicatedTestCase_ClassHierarchies>.New.WithNew(o => o.SubObjectParent).WithNew(o => o.SubObjectChild).Build();
            output.SubObjectParent.Should().NotBeNull();
            output.SubObjectParent.Should().BeOfType<DedicatedTestCase_ClassHierarchies.ParentSub>();
            output.SubObjectChild.Should().NotBeNull();
            output.SubObjectChild.Should().BeOfType<DedicatedTestCase_ClassHierarchies.ChildSub>();
        }
        #endregion

        #region With Defined Builder
        public class DedicatedTestCase_WithDefinedBuilder
        {
            public SubObject SubObjectProp { get; set; }

            public class SubObject
            {
                public int X { get; set; }
            }
        }

        public class TestBuilder
        {
            public static Builder<DedicatedTestCase_WithDefinedBuilder.SubObject> New =>
                Builder<DedicatedTestCase_WithDefinedBuilder.SubObject>.New
                    .With(o => o.X, 4);
        }

        [Test]
        public void WithNew_IgnoresAvailableBuilders()
        {
            var output = Builder<DedicatedTestCase_WithDefinedBuilder>.New.WithNew(o => o.SubObjectProp).Build();
            output.SubObjectProp.Should().NotBeNull();
            output.SubObjectProp.X.Should().NotBe(4);
            output.SubObjectProp.X.Should().Be(0);
        }
        #endregion
    }
}