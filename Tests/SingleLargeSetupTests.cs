using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LochNessBuilder;
using NUnit.Framework;

namespace Tests
{
    internal class ComplexMonster
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string Colour { get; set; }
        public int Age { get; set; }
        public string[] Sounds { get; set; }
        public List<string> FavouriteFood { get; set; }
        public Lake HomeLake { get; set; }
        public int LakeId { get; set; }
        public Lake HolidayLake { get; set; }
        public Lake CommunityLake { get; set; }
        public Egg Egg { get; set; }
    }

    internal class Lake
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ISet<ComplexMonster> Monsters { get; set; }
    }

    internal class Egg
    {
        public Egg()
        {
            Id = 3;
            Name = "Third";
        }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    internal static class ComplexMonsterBuilder
    {
        public static Builder<ComplexMonster> New
        {
            get
            {
                var rand = new Random();
                var theBiggestLake = new Lake();

                return Builder<ComplexMonster>.New
                    .With(m => m.Nationality, "Scottish")                                               // All monsters will be Scottish.

                    .WithSharedRef(m => m.CommunityLake, theBiggestLake)                                // All monsters will have a reference to the same Lake in this.CommunityLake.

                    .WithSequentialFrom(m => m.Colour, "Green", "Red", "Blue")                          // Monster Colors will be Green, Red, Blue, Green, Red, ...

                    .WithSequentialIds(m => m.Id)                                                       // Ids will be 1, 2, 3, 4, 5....
                                                                                                        // This is identical to ".WithSequentialFrom(m => m.Id, Enumerable.Range(1, int.MaxValue))"

                    .WithSequentialIds(m => m.Address, x => $"Pool {x}", 0)                             // Names will be "Pool 0", "Pool 1", "Pool 2", "Pool 3", "Pool 4", ...
                                                                                                        // This is identical to ".WithSequentialFrom(m => m.Address, Enumerable.Range(0, int.MaxValue).Select(x => $"Pool {x}"))"

                    .WithCreateEnumerableFrom(m => m.Sounds, "Rarrrgggh!", "Screech!", "Wooooosh!")     // All monsters will produce all three of these sounds.
                                                                                                        // This is identical to ".WithCreateEnumerableFrom(m => m.Sounds, new List<string>{"Rarrrgggh!", "Screech!", "Woooooh!"})"
                                                                                                        // and *almost* identical to ".With(m => m.Sounds, new []{"Rarrrgggh!", "Screech!", "Woooooh!"})" (only difference is that the containing array is not shared.)

                    .WithFactory(m => m.FavouriteFood, () => new List<string>())                        // All monsters will get their own, distinct (initially empty) List<> object for food.
                    
                    .WithAddToCollection(m => m.FavouriteFood, "People")                                // All monsters like to eat people, in addition to anything that could have been configured prior to this point.
                                                                                                        // (Obviously the more natural way to achieve that would be to include it in the previous Factory, but we want to demonstrate this .WithAddToCollection method.)

                    .WithFactory(m => m.Age, () => rand.Next(6))                                        // Age might be 2, 4, 1, 4, 6, 3 ...
                    
                    .WithBuilder(m => m.HomeLake, LakeBuilder.New)                                      // All monsters will have this.HomeLake populated with the result of "LakeBuilder.New.Build()", because Lake has a registered BuilderFactory
                    
                    .WithBuilder(m => m.HolidayLake, LakeBuilder.Minimal)                               // All monsters will have this.HolidayLake populated with the result of "LakeBuilder.Minimal.Build()".

                    .WithNew(m => m.Egg)                                                                // All monsters will have this.Egg populated with "new Egg()".
                    
                    .WithPostBuildSetup(IncludeMonsterInHomeLake)                                       // `this.LakeId`, and `this.HomeLake.Monsters` will be updated to honour `this.HomeLake` ... but only at the END of setup. i.e. honouring any later-defined overrides of `this.HomeLake` if configured.
                    
                    .WithCustomSetup(m =>                                                               // Runs this arbitrary logic against the monster. (But these values could be overridden by later Steps.)
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

    internal static class LakeBuilder
    {
        public static Builder<Lake> Minimal
        {
            get
            {
                return Builder<Lake>.New
                    .WithSequentialIds(t => t.Id)
                    .WithSequentialFrom(t => t.Name, Enumerable.Range(1, int.MaxValue).Select(i => $"Lake {i}"))
                    .WithFactory(t => t.Monsters, () => new HashSet<ComplexMonster>());
            }
        }

        public static Builder<Lake> New
        {
            get
            {
                return Minimal
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
    internal class SingleComplexSetupTests
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
            testMonster.Address.Should().Be("Pool 0");
            testMonster.Nationality.Should().Be("Scottish");
            testMonster.Age.Should().BeInRange(0, 6);

            var expectedColour = testMonster.Age <= 5 ? "Green" : "Black";
            testMonster.Colour.Should().Be(expectedColour);

            var expectedSounds = testMonster.Age == 0 ? new[] { "Waaaah!" } : new[] { "Rarrrgggh!", "Screech!", "Wooooosh!" };
            testMonster.Sounds.Should().BeEquivalentTo(expectedSounds);

            testMonster.FavouriteFood.Should().NotBeNull();
            testMonster.FavouriteFood.Should().ContainSingle().Which.Should().Be("People");

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
            testMonster.HomeLake.Monsters.Should().HaveCount(2); //This includes the "extra" monster added in the LakeBuilder.
            testMonster.HomeLake.Monsters.First().Id.Should().Be(0); //This is the "extra" monster added in the LakeBuilder, with only the blank constructor
            testMonster.HomeLake.Monsters.First().HomeLake.Should().Be(testMonster.HomeLake); //This should be achieved by the postSetup wiring-up.
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
            testMonster.FavouriteFood.Should().ContainSingle().Which.Should().Be("People");

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
            testMonster.HomeLake.Monsters.Should().HaveCount(2); //This includes the "extra" monster added in the LakeBuilder.
            testMonster.HomeLake.Monsters.First().Id.Should().Be(0); //This is the "extra" monster added in the LakeBuilder, with only the blank constructor
            testMonster.HomeLake.Monsters.First().HomeLake.Should().Be(testMonster.HomeLake); //This should be achieved by the postSetup wiring-up.
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
            testMonster.Address.Should().Be("Pool 4");

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