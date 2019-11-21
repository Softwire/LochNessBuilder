using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    public class ComplexMonster
    {
        public int Id { get; set; }
        public string Nationality { get; set; }
        public string Colour { get; set; }
        public int Age { get; set; }
        public string[] Sounds { get; set; }
        public List<string> FavouriteFood { get; set; }
        public Lake HomeLake { get; set; }
        public int LakeId { get; set; }
        public Lake HolidayLake { get; set; }
        public Egg Egg { get; set; }
    }

    public class Lake
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ISet<ComplexMonster> Monsters { get; set; }
    }

    public class Egg
    {
        public Egg()
        {
            Id = 3;
            Name = "Third";
        }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [BuilderFactory]
    public static class ComplexMonsterBuilder
    {
        public static Builder<ComplexMonster> New
        {
            get
            {
                var rand = new Random();
                return Builder<ComplexMonster>.New
                    .WithSequentialIds(m => m.Id)                                           // Ids will be 1, 2, 3, 4, 5....
                                                                                            // Above is identical to ".With(t => t.Id, Enumerable.Range(1, int.MaxValue))"
                    
                    .With(m => m.Nationality, "Scottish")                                   // All monsters will be Scottish....
                    
                    .WithSequentialFrom(m => m.Colour, "Green", "Red", "Blue")                       // Monster Colors will be Green, Red, Blue, Green, Red, ...

                    .WithEnumerable(m => m.Sounds, "Rarrrgggh!", "Screech!", "Wooooosh!")   // All monsters will produce all three of these sounds.
                                                                                            // Above is identical to ".WithEnumerable(m => m.Sounds, new List<string>{"Rarrrgggh!", "Screech!", "Woooooh!"})"
                                                                                            // and *almost* identical to ".With(m => m.Sounds, new []{"Rarrrgggh!", "Screech!", "Woooooh!"})" (only difference is that the containing array is not shared.)

                    .WithFactory(m => m.FavouriteFood, () => new List<string>())            // All monsters will get their own, distinct (initially empty) List<> object for food.
                    
                    .WithFactory(m => m.Age, () => rand.Next(6))                            // Age might be 2, 4, 1, 4, 6, 3 ...
                    
                    .WithBuilder(m => m.HolidayLake, MinimalLakeBuilder.New)                // All monsters will have this.HomeLake populated with the result of "LakeBuilder.Minimal.Build()".
                    
                    .WithBuilt(m => m.HomeLake)                                             // All monsters will have this.HomeLake populated with the result of "LakeBuilder.New.Build()", because Lake has a registered BuilderFactory
                    
                    .WithBuilt(m => m.Egg)                                                  // All monsters will have this.Egg populated with "new Egg()", because no builder has been registered for Eggs.
                    
                    .WithPostBuildSetup(IncludeMonsterInHomeLake)                           // `this.LakeId`, and `this.HomeLake.Monsters` will be updated to honour `this.HomeLake` ... but only at the END of setup. i.e. honouring any later-defined overrides of `this.HomeLake` if configured.
                    
                    .WithSetup(m =>                                                         // Runs this arbitrary logic against the monster. (But these values could be overridden by later Steps.)
                        {
                            if (m.Age > 5)
                            {
                                m.Colour = "Black";
                            }

                            if (m.Age < 1)
                            {
                                m.Sounds = new[] { "Waaaah!" };
                            }
                        })
                    ;
            }
        }

        private static void IncludeMonsterInHomeLake(ComplexMonster thisMonster)
        {
            if (thisMonster.HomeLake == null)
            {
                return;
            }

            var lake = thisMonster.HomeLake;
            thisMonster.LakeId = lake.Id;

            if (!lake.Monsters.Contains(thisMonster))
            {
                lake.Monsters.Add(thisMonster);
            }
        }
    }

    public static class MinimalLakeBuilder
    {
        public static Builder<Lake> New
        {
            get
            {
                return Builder<Lake>.New
                    .WithSequentialIds(t => t.Id)
                    .WithSequentialFrom(t => t.Name, Enumerable.Range(1, int.MaxValue).Select(i => $"Lake {i}"))
                    .WithFactory(t => t.Monsters, () => new HashSet<ComplexMonster>());
            }
        }
    }

    [BuilderFactory]
    public static class LakeBuilder
    {
        public static Builder<Lake> New
        {
            get
            {
                return MinimalLakeBuilder.New
                    .WithAddToCollection(t => t.Monsters, new ComplexMonster())
                    .WithPostBuildSetup(TieAllMonstersToLake);
            }
        }

        private static void TieAllMonstersToLake(Lake thisLake)
        {
            foreach (var ownedMonster in thisLake.Monsters)
            {
                ownedMonster.HomeLake = thisLake;
                ownedMonster.LakeId = thisLake.Id;
            }
        }
    }

    [TestFixture]
    public class SingleComplexSetupTests
    {
        [Test]
        public void CanBuildOne()
        {
            var testMonster = ComplexMonsterBuilder.New.Build();

            testMonster.Id.Should().Be(1);
        }

        [Test]
        public void CanBuildMultiple()
        {
            var testMonsters = ComplexMonsterBuilder.New.Build(5);

            testMonsters.Should().HaveCount(5);

            testMonsters.First().Id.Should().Be(1);
            testMonsters.First().Colour.Should().Be("Green");

            testMonsters.Last().Id.Should().Be(5);
            testMonsters.Last().Colour.Should().Be("Red");
        }

        [Test, Repeat(20)]
        public void AllStepsHaveExecuted()
        {
            var testMonster = ComplexMonsterBuilder.New.Build();

            testMonster.Id.Should().Be(1);
            testMonster.Nationality.Should().Be("Scottish");
            testMonster.Age.Should().BeInRange(0, 6);

            var expectedColour = testMonster.Age <= 5 ? "Green" : "Black";
            testMonster.Colour.Should().Be(expectedColour);

            var expectedSounds = testMonster.Age == 0 ? new[] { "Waaaah!" } : new[] { "Rarrrgggh!", "Screech!", "Wooooosh!" };
            testMonster.Sounds.Should().BeEquivalentTo(expectedSounds);

            testMonster.FavouriteFood.Should().NotBeNull();
            testMonster.FavouriteFood.Should().BeEmpty();

            testMonster.HolidayLake.Should().NotBeNull();
            testMonster.HolidayLake.Id.Should().Be(1);
            testMonster.HolidayLake.Name.Should().Be("Lake 1");
            testMonster.HolidayLake.Monsters.Should().BeOfType<HashSet<ComplexMonster>>();
            testMonster.HolidayLake.Monsters.Should().BeEmpty();

            testMonster.HomeLake.Should().NotBeNull();
            testMonster.HomeLake.Id.Should().Be(1);
            testMonster.HomeLake.Name.Should().Be("Lake 1");
            testMonster.HomeLake.Monsters.Should().BeOfType<HashSet<ComplexMonster>>();

            testMonster.LakeId.Should().Be(1);
            testMonster.HomeLake.Monsters.Should().HaveCount(2);
            testMonster.HomeLake.Monsters.Should().Contain(testMonster);

            testMonster.Egg.Should().NotBeNull();
            testMonster.Egg.Id.Should().Be(3);
            testMonster.Egg.Name.Should().Be("Third");
        }

        [Test, Repeat(20)]
        public void ConstantStepsAreConstant()
        {
            var testMonster = ComplexMonsterBuilder.New.Build(5).Last();

            //testMonster.Id.Should().Be(1);
            testMonster.Nationality.Should().Be("Scottish");
            testMonster.Age.Should().BeInRange(0, 6);

            //var expectedColour = testMonster.Age <= 5 ? "Green" : "Black";
            //testMonster.Colour.Should().Be(expectedColour);

            var expectedSounds = testMonster.Age == 0 ? new[] { "Waaaah!" } : new[] { "Rarrrgggh!", "Screech!", "Wooooosh!" };
            testMonster.Sounds.Should().BeEquivalentTo(expectedSounds);

            testMonster.FavouriteFood.Should().NotBeNull();
            testMonster.FavouriteFood.Should().BeEmpty();

            testMonster.HolidayLake.Should().NotBeNull();
            //testMonster.HolidayLake.Id.Should().Be(1);
            //testMonster.HolidayLake.Name.Should().Be("Lake 1");
            testMonster.HolidayLake.Monsters.Should().BeOfType<HashSet<ComplexMonster>>();
            testMonster.HolidayLake.Monsters.Should().BeEmpty();

            testMonster.HomeLake.Should().NotBeNull();
            //testMonster.HomeLake.Id.Should().Be(1);
            //testMonster.HomeLake.Name.Should().Be("Lake 1");
            testMonster.HomeLake.Monsters.Should().BeOfType<HashSet<ComplexMonster>>();

            //testMonster.LakeId.Should().Be(1);
            testMonster.HomeLake.Monsters.Should().HaveCount(2);
            testMonster.HomeLake.Monsters.Should().Contain(testMonster);

            testMonster.Egg.Should().NotBeNull();
            testMonster.Egg.Id.Should().Be(3);
            testMonster.Egg.Name.Should().Be("Third");
        }

        [Test, Repeat(20)]
        public void VariableStepsVary()
        {
            var testMonster = ComplexMonsterBuilder.New.Build(5).Last();

            testMonster.Id.Should().Be(5);

            var expectedColour = testMonster.Age <= 5 ? "Red" : "Black";
            testMonster.Colour.Should().Be(expectedColour);

            testMonster.HolidayLake.Id.Should().Be(5);
            testMonster.HolidayLake.Name.Should().Be("Lake 5");

            testMonster.HomeLake.Id.Should().Be(5);
            testMonster.HomeLake.Name.Should().Be("Lake 5");
            testMonster.LakeId.Should().Be(5);
        }

        [Test]
        public void IndependentBuildersSensibly()
        {
            var builder1 = ComplexMonsterBuilder.New;
            var builder2 = ComplexMonsterBuilder.New;

            builder1.Build().Id.Should().Be(1);
            builder2.Build().Id.Should().Be(1);
            builder2.Build().Id.Should().Be(2);
            builder2.Build().Id.Should().Be(3);
            builder1.Build().Id.Should().Be(2);
        }
    }
}