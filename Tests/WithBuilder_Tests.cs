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
            public SubObj Sub { get; set; }
            public List<SubObj> SubList { get; set; }

            public class SubObj
            {
                public int Id { get; set; }
                public string StringProp { get; set; }
            }
        }

        public class FirstBuilder
        {
            public static Builder<TestClass.SubObj> New => Builder<TestClass.SubObj>.New.With(subObj => subObj.StringProp, "BuilderOne");
        }

        public class SecondBuilder
        {
            public static Builder<TestClass.SubObj> New => Builder<TestClass.SubObj>.New.With(subObj => subObj.StringProp, "BuilderTwo");
            public static Builder<TestClass.SubObj> WithIds => Builder<TestClass.SubObj>.New.WithSequentialIds(subObj => subObj.Id);
        }

        [Test]
        public void WithBuilder_CanUseBuilderToCreateObject()
        {
            var output = Builder<TestClass>.New.WithBuilder(o => o.Sub, FirstBuilder.New).Build();
            output.Sub.StringProp.Should().Be("BuilderOne");
        }

        [Test]
        public void WithBuilder_CanUseADifferentBuilderToCreateObject()
        {
            var output = Builder<TestClass>.New.WithBuilder(o => o.Sub, SecondBuilder.New).Build();
            output.Sub.StringProp.Should().Be("BuilderTwo");
        }

        [Test]
        public void WithBuilder_CanUseLocallyDefinedBuilder()
        {
            var builder = Builder<TestClass.SubObj>.New.With(o => o.StringProp, "Local");

            var output = Builder<TestClass>.New.WithBuilder(o => o.Sub, builder).Build();
            output.Sub.StringProp.Should().Be("Local");
        }

        [Test]
        public void WithBuilder_UsesTheSameBuilderRepeatedly()
        {
            var subBuilder = Builder<TestClass.SubObj>.New.WithSequentialIds(o => o.Id);
            var outputs = Builder<TestClass>.New.WithBuilder(o => o.Sub, subBuilder).Build(2);

            outputs[0].Sub.Id.Should().Be(1);
            outputs[1].Sub.Id.Should().Be(2);
        }

        [Test]
        public void WithBuilder_AffectsExternalStateOfACapturedBuilder()
        {
            var subBuilder = Builder<TestClass.SubObj>.New.WithSequentialIds(o => o.Id);
            var subObj1 = subBuilder.Build();
            var outputs2And3 = Builder<TestClass>.New.WithBuilder(o => o.Sub, subBuilder).Build(2);
            var output4 = Builder<TestClass>.New.WithBuilder(o => o.Sub, subBuilder).Build();
            var subObj5 = subBuilder.Build();

            subObj1.Id.Should().Be(1);
            outputs2And3[0].Sub.Id.Should().Be(2);
            outputs2And3[1].Sub.Id.Should().Be(3);
            output4.Sub.Id.Should().Be(4);
            subObj5.Id.Should().Be(5);
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