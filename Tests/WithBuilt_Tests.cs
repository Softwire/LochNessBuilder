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
    internal class WithBuilt_Tests
    {
        internal static class UnregisteredSubObjectBuilder
        {
            public static Builder<TestSubObject> One => Builder<TestSubObject>.New.With(subObj => subObj.StringProp, "BuilderOne");
            public static Builder<TestSubObject> WithIds => Builder<TestSubObject>.New.With(subObj => subObj.StringProp, "NewBuilder");
        }

        [BuilderFactory]
        internal static class RegisteredSubObjectBuilder
        {
            public static Builder<TestSubObject> Other => Builder<TestSubObject>.New.With(subObj => subObj.StringProp, "OtherBuilder");
            public static Builder<TestSubObject> New => Builder<TestSubObject>.New.With(subObj => subObj.StringProp, "NewBuilder");
        }

        [Test]
        public void WithBuilder_UsesBuilderToCreateObject()
        {
            var output = Builder<TestObject>.New.WithBuilder(o => o.SubObjectProp, UnregisteredSubObjectBuilder.One).Build();
            output.SubObjectProp.StringProp.Should().Be("BuilderOne");
        }

        [Test]
        public void WithBuilder_CanUseLocallyDefinedBuilder()
        {
            var builder = Builder<TestSubObject>.New.With(o => o.StringProp, "Local");
            var output = Builder<TestObject>.New.WithBuilder(o => o.SubObjectProp, builder).Build();
            output.SubObjectProp.StringProp.Should().Be("Local");
        }

        [Test]
        public void WithBuilder_AffectsStateOfACapturedBuilder()
        {
            var subBuilder = Builder<TestSubObject>.New.WithSequentialIds(o => o.Id).With(o => o.StringProp, "SharedState");
            var subObj1 = subBuilder.Build();
            var outputs2And3 = Builder<TestObject>.New.WithBuilder(o => o.SubObjectProp, subBuilder).Build(2);
            var subObj4 = subBuilder.Build();

            subObj1.Id.Should().Be(1);
            outputs2And3[0].SubObjectProp.Id.Should().Be(2);
            outputs2And3[1].SubObjectProp.Id.Should().Be(3);
            subObj4.Id.Should().Be(4);
        }
    }
}