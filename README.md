# LochNessBuilder

A C# Builder library to automate and commonise creation of objects for tests.
You can define a basic builder that wires up default values, which all tests can then use as-is, or add further customisations for specific tests.

Note that v3.0 has made a lot of superficial changes to the API. See below for details

## Use in a .Net Framework project

As of version 2.0, this library has been migrated to .NETStandard, to enable use in .NETCore projects.

To continue consuming this library from a .NET Framework project, the following reference will need to be added to the `.csproj` file:

`<Reference Include="netstandard" />`

## Basic Usage

A basic builder for `Monster`s:

```csharp
using System.Collections.Generic;
using LochNessBuilder;

public class Monster
{
    public int Id { get; set; }
    public string Colour { get; set; }
    public int Age { get; set; }
    public List<string> Sounds { get; set; }
}

[BuilderFactory]
public static class MonsterBuilder
{
    public static Builder<Monster> New
    {
        get
        {
            return Builder<Monster>.New
                .WithSequentialIds(t => t.Id)
                .With(m => m.Colour, "Green")
                .With(m => m.Age, 3)
                .WithFactory(m => m.Sounds, () => new List<string>());
        }
    }
}
```

To build a single `Monster`

```csharp
Monster testMonster = MonsterBuilder.New.Build();
// testMonster has Id = 1, Colour = "Green", Age = 3.
```

To build multiple `Monster`s

```csharp
IEnumerable<Monster> testMonsters = MonsterBuilder.New.Build(5);
// testMonsters has 5 Monsters, with Ids of respectively 1, 2, 3, 4, 5, but all have the same Colour and Age.
```

To build multiple `Monster`s, at different points in time, but retaining any stateful properties of the `Builder` (e.g. Id Sequences)

```csharp
Builder<Monster> monsterBuilder = MonsterBuilder.New;
Monster earlyMonster = monsterBuilder.Build();         // earlyMonster has Id=2
// Do some testing stuff.
// ...
// then later
// ...
Monster lateMonster = monsterBuilder.Build();          // lateMonster has Id=2
```

To build a Monster, but override a particular property that has previously been configured

```csharp
Monster youngMonsters = MonsterBuilder.New.With(t => t.Age, 1).Build(4);
// youngMonsters will have Id 1-4, and be "Green" but will now have Age = 1, despite the configuration defined in the initiail MonsterBuilder.
// Note that the original assignment from the original Builder has still *run*; we've simply overwritten the value later.
```

### Notable usages and features

There are further docs down below, but some particular notes on common situations and easy mistakes to make...

* The standard `With()` will share the provided argument with all objects that get Built. If you're setting an object then you don't probably don't want that; you probably want to use `.WithFactory()`.
* Note the `.WithSequentialIds()` method, which will likely be useful for any Id-based properties.
  * All it does is call `.WithOneOf(<propSelector>, Enumerable.Range(1, int.MaxValue))`.
* If you have a database object with properties representing DB relations, where there is a FK int property AND a FK object property (and possibly also a collection property on the other end of the relationship), then you probably want to use `.WithPostBuildSetup()` to ensure that everything gets suitably wired up at the end, to account for later modifications applied to the Builder.
* You can use the `.WithBuilt/Builder()` methods to setup complex sub-properties, for which you've already defined `Builder`s.
  * The "default" builders for `T`s, are ones which are `public static getters` on classes tagged with `[BuilderFactory]`.
  * So if you want to define multiple `Builder<T>` properties, then you won't be able to use `.WithBuilt()` and should use `.WithBuilder()` instead.
  * Equally if you're not using "default" builders, then there's no need to include the `[BuilderFactory]` attribute.
  * If no defined `Builder` is found, then the "default" `Builder` is just one that calls `new()` on its target.
* `Builder` objects are immutable; each method call derives a *new* `Builder` object, leaving the original one unchanged.
  * Note that the resultant new `Builder` is *not* completely separate, in that it stills retains any shared internal state that was defined on the original.
  * e.g. If you have a parent `Builder` that uses `.WithSequentialIds()`, and derive a further `Builder` from it, then calls to either `Builder` will increment the 'shared' next Id value.

### Setup Methods

Please examine the XML docs for full details. However, in simplified form, we have:

* ##### With()
  * Set a property to a value.
* ##### WithOneOf()
  * Provide multiple values, and the builder will cycle through them in order, for each new object built.
* ##### WithSequentialIds()
  * Sets a numeric property with increasing numbers, from 1 to int.MaxValue.
* ##### WithEnumerable()
  * Provide multiple values, and the builder will create the relevant container and put them all onto each new object
* ##### WithFactory()
  * Provide a factory method, to set a property to a newly created value each time.
* ##### WithBuilder()
  * Like `WithFactory`, but the factory is specifically the `.Build()` method of the provided `Builder`.
* ##### WithBuilt()
  * Like `WithBuilder`, but the appropriate `Builder<T>` is found automatically from any classes tagged with `BuilderFactory`, so you don't need to provide any second argument at all ... it just works it all out.
* ##### _`IEnumerable` variations of `WithFactory()`_
  * The above 3 variations on `WithFactory()` have overloads which simplify the work if the property in question implements `IEnumerable<T>`. Rather than defining a Factory that builds the whole `IEnumerable<T>`, so provide one which simply builds `T`s and the `IEnumerable<>` portion will get worked out for you. The resultant `IEnumerable<T>` will contain 3 elements.
* ##### Add()
  * Add the value to the existing ICollection property.
  * This assumes that an earlier setup method (or possibly the object constructor) has initialised the ICollection beforehand.
* ##### WithPreBuildSetup()
  * Do an arbitrary action to the object being created, but do it *before* all the other things.
* ##### WithSetup()
  * Do an arbitrary action to the object being created.
* ##### WithPostBuildSetup()
  * Do an arbitrary action to the object being created, but do it *after* all the other things (even in other setup methods are called after this one).

### Full Example

```csharp
   //An example of all the available methods:
    public class Monster
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

        public ISet<Monster> Monsters { get; set; }
    }

    public class Egg
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [BuilderFactory]
    public static class MonsterBuilder
    {
        public static Builder<Monster> New
        {
            get
            {
                var rand = new Random();
                return Builder<Monster>.New
                    .WithSequentialIds(m => m.Id)                                           // Ids will be 1, 2, 3, 4, 5....
                    .With(m => m.Nationality, "Scottish")                                           // Ids will be 1, 2, 3, 4, 5....
                    // Above is identical to ".With(t => t.Id, Enumerable.Range(1, int.MaxValue))"

                    .WithOneOf(m => m.Colour, "Green", "Red", "Blue")                       // Monster Colors will be Green, Red, Blue, Green, Red, ...

                    .WithEnumerable(m => m.Sounds, "Rarrrgggh!", "Screech!", "Wooooosh!")   // All monsters will produce all three of these sounds.
                    // Above is identical to ".WithEnumerable(m => m.Sounds, new List<string>{"Rarrrgggh!", "Screech!", "Woooooh!"})"
                    // Above is *almost* identical to ".With(m => m.Sounds, new []{"Rarrrgggh!", "Screech!", "Woooooh!"})" (only difference is that the containing array is not shared.)

                    .WithFactory(m => m.FavouriteFood, () => new List<string>())            // All monsters will get their own, distinct (initially empty) List<> object for food.
                    .WithFactory(m => m.Age, () => rand.Next(4))                            // Age might be 2, 4, 1, 4, 3 ...
                    .WithBuilder(m => m.HolidayLake, LakeBuilder.Minimal)                   // All monsters will have this.HomeLake populated with the result of "LakeBuilder.Minimal.Build()".
                    .WithBuilt(m => m.HomeLake)                                             // All monsters will have this.HomeLake populated with the result of "LakeBuilder.New.Build()", because Lake has a registered Builder (and 'New' is used preferentially)
                    .WithBuilt(m => m.Egg)                                                  // All monsters will have this.Egg populated with "new Egg()", because no builder has been registered for Eggs.
                    .WithPostBuildSetup(IncludeMonsterInHomeLake)                           // `this.LakeId`, and `this.HomeLake.Monsters` will be updated to honour `this.HomeLake` ... but only at the END of setup. i.e. honouring any later-defined overrides of `this.HomeLake` if configured.
                    .WithSetup(m =>                                                         // Runs this arbitrary logic against the monster. (But these values could be overridden by later Steps.)
                        {
                            if (m.Age > 3)
                            {
                                m.Colour = "Red";
                            }

                            if (m.Age < 2)
                            {
                                m.Sounds = new[] {"Waaaah!"};
                            }
                        })
                    ;
            }
        }

        private static void IncludeMonsterInHomeLake(Monster thisMonster)
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

    [BuilderFactory]
    public static class LakeBuilder
    {
        public static Builder<Lake> Minimal
        {
            get
            {
                return Builder<Lake>.New
                    .WithSequentialIds(t => t.Id)
                    .WithOneOf(t => t.Name, Enumerable.Range(1, int.MaxValue).Select(i => $"Name {i}"))
                    .WithFactory(t => t.Monsters, () => new HashSet<Monster>());
            }
        }

        public static Builder<Lake> New
        {
            get
            {
                return Minimal
                    .Add(t => t.Monsters, new Monster())
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
```

### Sequence of function calls

When constructing multiple objects (i.e. when calling `Build(int count)`) the Builder will apply all configured steps to each object in turn, before constructing the next blank object.
This is worth noting, in case you have some edge case where you want to maintain some sort of state between built objects. For example ...

```csharp
// To build an bunch of Monsters, each with an age matching the *previous* monster's Id
[Test]
public void WeirdStatefulBuilder()
{
    int previousId = 0;
    var monsters = Builder<Monster>.New
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
```

Note that such usages are stretching this library beyond the bounds of what it is intended for. You're likely better off building all the monsters up-front, and then updating them after the fact!

### Migration to v3.0

Lots of method/type names have changed between v2.0 and v3.0, but there's very little _functionality_ change, so it should be very easy to migrate:

* The `[Builder]` attribute is now called `[BuilderFactory]`, to better represent what it is doing.
* `With(m => m.SubObject)` intended to auto-find any existing builders, is now `WithBuilt(m => m.SubObject)`.
* `With(m => m.SingleString, "a", "b", "c" )` intended to loop over values, is now `WithOneOf(m => m.SingleString, "a", "b", "c" )`.
* `WithCollection(...)` is now `WithEnumerable(...)`.
* The implicit cast from `Builder<T>` to `T` has been removed. Replace it with calling `.Build()` on the builder.

## TODOs

* Unit Tests
* Add support for Builders for objects with no default constructor.

## Creating a new Nuget Package (Dev Notes)

With .Net Standard, you no longer have to use a nuspec file since all the package information is added to the csproj file. You will, however, need VS >= 2017

Process for releasing a new NugetPackage:

* Update the `LochNessBuilder.csproj` with any new information. This can also be done in the package tab of the project properties:
  * Identify the appropriate new semantic version number. See here if you're unsure: <https://semver.org/>
  * Update the version numbers. Note that `PackageVersion` needs to be manually updated in the `.csproj` file not the VS UI, as the VS UI seems to be buggy.
  * Write some appropriate PackageReleaseNotes in the package data.
* Update the READMEs
  * Update the `.\README.md` file in general, and if appropriate add a "Migration" section to it.
  * Update the `.\nugetREADME.md` file, with an appropriate selection from them main `README.md` file.
* Create the new package.
  * Clean and Rebuild the solution, which will automatically package everything up for you.
    * The newly created package will be dropped in the `<root>\nuget` folder.
  * Delete the previous version which will be sitting beside the newly produced version.
    * Note that a clean copy of the previous version will already have been kept, in `<root>\nuget\ArchivedPackages`, when it was released. Double check that it's there and if not, look through git to recreate it.
* Deploy to Nuget!
  * Log into nuget with an account that is part of the LochNess organisation (Fet an existing member of the org to add you if not already included)
  * Go to the `Upload Package` Page ([link](https://www.nuget.org/packages/manage/upload))
  * Browse to the newly packaged file, and select it.
    * Don't worry about the \<license\> warning. VS doesn't seem to properly support the new version of the tag yet, so we're stuck using the old version.
    * Feel free to fix this if you know how!
  * Nuget should auto-parse all the meta data and display it. Briefly review it.
  * Scroll to the bottom of the page, where Nuget asks for any documentation. Upload the `.\nugetREADME.md` file.
* Test that the new package is available on Nuget, and that updating your project still works after updating it to use the new version.
* Archive the package
  * Having released a new version to Nuget, copy the binary pacakge that you released into `<root>\nuget\ArchivedPackages`, for posterity.
  * Do this now, to ensure that we have a clean version of the package, not one overwritten with later dev work!
* Publicise the release in whatever manner you deem appropriate. At very least, the people in the Authors tag probably want to know!
