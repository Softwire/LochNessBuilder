using System;
using System.Collections.Generic;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    internal class WithAddToCollection_Tests
    {
        [Test]
        public void WithAddToCollection_AddsAValueToACollection()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ListProp, () => new List<int>()).WithAddToCollection(o => o.ListProp, 3).Build();
            output.ListProp.Should().ContainSingle().Which.Should().Be(3);
        }

        [Test]
        public void WithAddToCollection_AddsMultipleValueToACollection()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ListProp, () => new List<int>()).WithAddToCollection(o => o.ListProp, 3, 4, 5).Build();
            output.ListProp.Should().HaveCount(3);
            output.ListProp.Should().BeEquivalentTo(3,4,5);
        }

        [Test]
        public void WithAddToCollection_AddsAValueToPreviouslyPopulatedCollection()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ListProp, () => new List<int>{5, 4}).WithAddToCollection(o => o.ListProp, 3, 5).Build();
            output.ListProp.Should().HaveCount(4);
            output.ListProp.Should().BeEquivalentTo(5, 4, 3, 5);
        }

        [Test]
        public void WithAddToCollection_GivesSuitableErrorIfCollectionUninitialised()
        {
            var builder = Builder<TestObject>.New.WithAddToCollection(o => o.ListProp, 3);
            Action buildAction = () => builder.Build();
            buildAction.Should()
                .Throw<Exception>()
                .WithMessage("Error occurred when attempting to '.Add' to the property 'ListProp'.")
                .WithInnerException<NullReferenceException>();
        }

        [Test]
        public void WithAddToCollection_GivesSuitableErrorIfCollectionIsNotAddable()
        {
            var builder = Builder<TestObject>.New.WithFactory(o => o.ArrayProp, () => new int[0]).WithAddToCollection(o => o.ArrayProp, 3);
            Action buildAction = () => builder.Build();

            buildAction.Should()
                .Throw<Exception>()
                    .WithMessage("Error occurred when attempting to '.Add' to the property 'ArrayProp'.")
                .WithInnerException<NotSupportedException>()
                    .WithMessage("Collection was of a fixed size.");
        }

        [Test]
        public void WithAddToCollection_CanAddAnIntToALongList()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.LongListProp, () => new List<long>()).WithAddToCollection(o => o.LongListProp, 3).Build();
            output.LongListProp.Should().ContainSingle().Which.Should().Be(3);
        }

        [Test]
        public void WithSequentialIds_ForcesACastToAddAnIntToAShortList()
        {
            var output = Builder<TestObject>.New.WithFactory(o => o.ShortListProp, () => new List<short>()).WithAddToCollection<short>(o => o.ShortListProp, 3).Build(); //Won't compile without the <short> cast
            output.ShortListProp.Should().ContainSingle().Which.Should().Be(3);
        }
    }
}