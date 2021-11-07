using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    internal class WithBuilder_Tests
    {
        public class TestClass
        {
            public SubObject SubObjectProp { get; set; }
            public List<SubObject> SubList { get; set; }

            public class SubObject
            {
                public int Id { get; set; }
                public string StringProp { get; set; }
            }
        }

        private const string BuilderLabel1 = "BuilderOne";
        private const string BuilderLabel2 = "BuilderTwo";
        private const string BuilderLabel3 = "BuilderThree";
        public class FirstBuilder
        {
            public static Builder<TestClass.SubObject> New => Builder<TestClass.SubObject>.New.With(subObject => subObject.StringProp, BuilderLabel1);
        }

        public class SecondBuilder
        {
            public static Builder<TestClass.SubObject> New => Builder<TestClass.SubObject>.New.With(subObject => subObject.StringProp, BuilderLabel2);
            public static Builder<TestClass.SubObject> WithIds => Builder<TestClass.SubObject>.New.WithSequentialIds(subObject => subObject.Id);
            public static Builder<TestClass.SubObject> WithIdsAndString => WithIds.With(subObject => subObject.StringProp, BuilderLabel3);
        }

        [Test]
        public void WithBuilder_CanUseBuilderToCreateObject()
        {
            var output = Builder<TestClass>.New.WithBuilder(o => o.SubObjectProp, FirstBuilder.New).Build();
            output.SubObjectProp.StringProp.Should().Be(BuilderLabel1);
            output.SubObjectProp.Id.Should().Be(0);
        }

        [Test]
        public void WithBuilder_CanUseADifferentBuilderToCreateObject()
        {
            var output = Builder<TestClass>.New.WithBuilder(o => o.SubObjectProp, SecondBuilder.New).Build();
            output.SubObjectProp.StringProp.Should().Be(BuilderLabel2);
            output.SubObjectProp.Id.Should().Be(0);
        }

        [Test]
        public void WithBuilder_CanUseAParticularBuilderDefinitionOnABuilderClassToCreateObject()
        {
            var output = Builder<TestClass>.New.WithBuilder(o => o.SubObjectProp, SecondBuilder.WithIds).Build();
            output.SubObjectProp.Id.Should().Be(1);
            output.SubObjectProp.StringProp.Should().BeNull();
        }

        [Test]
        public void WithBuilder_CanUseABuilderThatSharesADefinitionFromAnotherBuilderOnThatClassToCreateObject()
        {
            var output = Builder<TestClass>.New.WithBuilder(o => o.SubObjectProp, SecondBuilder.WithIdsAndString).Build();
            output.SubObjectProp.Id.Should().Be(1);
            output.SubObjectProp.StringProp.Should().Be(BuilderLabel3);
        }

        [Test]
        public void WithBuilder_UsesTheSameBuilderRepeatedly()
        {
            var subBuilder = Builder<TestClass.SubObject>.New.WithSequentialIds(o => o.Id);
            var outputs = Builder<TestClass>.New.WithBuilder(o => o.SubObjectProp, subBuilder).Build(2);

            outputs[0].SubObjectProp.Id.Should().Be(1);
            outputs[1].SubObjectProp.Id.Should().Be(2);
        }

        [Test]
        public void WithBuilder_AffectsStateOfACapturedBuilder()
        {
            var subBuilder = Builder<TestClass.SubObject>.New.WithSequentialIds(o => o.Id);
            var subObject1 = subBuilder.Build();
            var outputs2And3 = Builder<TestClass>.New.WithBuilder(o => o.SubObjectProp, subBuilder).Build(2);
            var output4 = Builder<TestClass>.New.WithBuilder(o => o.SubObjectProp, subBuilder).Build();
            var subObject5 = subBuilder.Build();

            subObject1.Id.Should().Be(1);
            outputs2And3[0].SubObjectProp.Id.Should().Be(2);
            outputs2And3[1].SubObjectProp.Id.Should().Be(3);
            output4.SubObjectProp.Id.Should().Be(4);
            subObject5.Id.Should().Be(5);
        }

        [Test]
        public void WithBuilder_CanPopulateAnIEnumerable()
        {
            var output = Builder<TestClass>.New.WithBuilder(o => o.SubList, SecondBuilder.WithIds, 4).Build();
            output.SubList.Should().NotBeNull();
            output.SubList.Should().HaveCount(4);
        }

        [Test]
        public void WithBuilder_UsesTheDefaultCountWhenPopulatingAnIEnumerable()
        {
            var output = Builder<TestClass>.New.WithBuilder(o => o.SubList, SecondBuilder.WithIds).Build();
            output.SubList.Should().NotBeNull();
            output.SubList.Should().HaveCount(3);
        }

        [Test]
        public void WithBuilder_ReusesTheSameBuilderWhenPopulatingIEnumerables()
        {
            var outputs = Builder<TestClass>.New.WithBuilder(o => o.SubList, SecondBuilder.WithIds, 4).Build(3);
            outputs.SelectMany(obj => obj.SubList.Select(sub => sub.Id)).Should().OnlyHaveUniqueItems();
        }
    }
}