using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    public class SimpleMonster
    {
        public int Id { get; set; }
        public string Colour { get; set; }
        public int Age { get; set; }
        public List<string> Sounds { get; set; }
    }

    [BuilderFactory]
    public static class SimpleMonsterBuilder
    {
        public static Builder<SimpleMonster> New
        {
            get
            {
                return Builder<SimpleMonster>.New
                    .WithSequentialIds(t => t.Id)
                    .With(m => m.Colour, "Green")
                    .With(m => m.Age, 3)
                    .WithFactory(m => m.Sounds, () => new List<string>());
            }
        }
    }

    [TestFixture]
    public class BasicBuildingTests
    {
        [Test]
        public void BasicBuild()
        {
            var testMonster = SimpleMonsterBuilder.New.Build();

            testMonster.Id.Should().Be(1);
            testMonster.Colour.Should().Be("Green");
        }

        [Test]
        public void SequentialBuild()
        {
            var builder = SimpleMonsterBuilder.New;
            var monster1 = builder.Build();
            var monster2 = builder.Build();

            monster1.Id.Should().Be(1);
            monster1.Colour.Should().Be("Green");

            monster2.Id.Should().Be(2);
            monster2.Colour.Should().Be("Green");

            monster1.Sounds.Should().NotBeSameAs(monster2.Sounds);
        }

        [Test]
        public void MultiBuild()
        {
            var testMonsters = SimpleMonsterBuilder.New.Build(5).ToList();

            testMonsters.Should().HaveCount(5);

            testMonsters.First().Id.Should().Be(1);
            testMonsters.First().Colour.Should().Be("Green");

            testMonsters.Last().Id.Should().Be(5);
            testMonsters.Last().Colour.Should().Be("Green");
        }

        [Test]
        public void OverriddenPropertyConfig()
        {
            var testMonsters = SimpleMonsterBuilder.New.With(t => t.Colour, "Purple").Build(2).ToList();

            testMonsters.First().Id.Should().Be(1);
            testMonsters.First().Colour.Should().Be("Purple");

            testMonsters.Last().Id.Should().Be(2);
            testMonsters.Last().Colour.Should().Be("Purple");
        }

        [Test]
        public void IndependentBuilders()
        {
            var builder1 = SimpleMonsterBuilder.New;
            var builder2 = SimpleMonsterBuilder.New;

            builder1.Build().Id.Should().Be(1);
            builder2.Build().Id.Should().Be(1);
            builder2.Build().Id.Should().Be(2);
            builder2.Build().Id.Should().Be(3);
            builder1.Build().Id.Should().Be(2);
        }

        [Test]
        public void AddingConfigurationReturnsANewBuilder()
        {
            var builder1 = SimpleMonsterBuilder.New;
            var builder2 = builder1.With(t => t.Id, -1);

            builder1.Build().Id.Should().Be(1);
            builder1.Build().Id.Should().Be(2);
            builder2.Build().Id.Should().Be(-1);
            builder2.Build().Id.Should().Be(-1);
        }

        [Test]
        public void MultipleBuildsResolveInSequenceNotInParallel()
        {
            int previousId = 0;
            var monsters = Builder<SimpleMonster>.New
                .WithSequentialIds(m => m.Id)
                .WithSetup(m =>
                {
                    m.Age = previousId;
                    previousId = m.Id;
                })
                .Build(3).ToList();

            monsters.Select(m => m.Id).Should().BeEquivalentTo(1, 2, 3);
            monsters.Select(m => m.Age).Should().BeEquivalentTo(0, 1, 2);
        }
    }
}