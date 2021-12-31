# LochNessBuilder

LochNessBuilder is a C# Test Object Builder library to automate and commonise creation of objects for tests.
Support all modern versions of .NET Framework, Core and Standard. See ['.NET Version targets'](#net-version-targets) below for more details.

The intended/recommended usage, is that you define one (or a small number) of default builders for each major objects in your domain.
Then, in individual tests or test classes, you use that default builder and apply further configurations **only** to those additional properties that you *actively* care about in those specific tests.
The default builders are responsible for setting up sensible default general values for those objects, and configuring relationships with any of the other major objects that a given test object might be linked to - all the stuff that needs to be present for a test to run smoothly, but isn't **actually** what the test cares about.
i.e. We imagine that you *won't* be creating a new builder for each test, or even for each test class.

**Note that `Builder`s are Immutable, so each configuration call creates a _new_ `Builder`, leaving the existing one untouched. (as opposed to modifying the existing Builder)**

If you want to add your own custom build methods, say `WithCommonComplexSetupStepThatNeedsToBeSpecifiedInLotsOfTheTestsBasedOnX(x)`, then you can define those as extension methods against `Builder<YourDomainObject>`.

Note that v4.0 has made a lot of superficial changes to the API. See below for details.

## Migration Guide

### **Do not use v2.0.0**

This version was released inadvertantly and has been deprecated and de-listed from nuget.org
Versions `2.1` and `4.0` are available, and the latter should be used if possible.

* If you wanted "Unchanged `v1.x` API, but on .NET Standard", then use `v2.1`.
* If you wanted the updated API, then use `v4.0` (Compatible with any remotely modern .NET version), and see migration notes below.

### Migration from v1.0 to v2.1

Added Support for .NET Standard. See ['Use in a .Net Framework project'](#use-in-a-net-framework-project) below.

No API changes. No C# Code Migration required.

MIT License was added.

### Migration from v2.1 to v4.0

Lots of method/type names have changed between `v2.1` and `v4.0`, but there's very little _functionality_ change, so it should be very easy to migrate:

* The `With(m => m.SubObject)` method call, intended to either auto-find any registered Builder or to use the default constructor to create the needed object, has been entirely removed, as has the `[Builder]` attribute that it used.
  * In the former case, where you intended the use of the default constructor (i.e. *without* a Builder defined and registered), there is now an explicit `WithNew()` method.
  * The latter case, where you intended that the registered Builder would be found and used, there is now an explicit `WithBuilder(m => m.SubObject, TheRegisteredBuilder.New)` method, where `TheRegisteredBuilder` was the type that was previously decorated with the `[Builder]` attribute
* The implicit cast from `Builder<T>` to `T` has been removed. Replace it with calling `.Build()` on the builder.
* The `With(m => m.SubObject, someExistingObject)` method call, intended re-use the same `someExistingObject` on every built object, is now `WithSharedRef(m => m.SubObject, someExistingObject)`.
* The `With(m => m.Prop, value)` method call, is now constrained to primitives and other value Types (specifically `where TProp : struct`) to enforce the use of `WithSharedRef` where that's intended.
* The `With(m => m.SingleString, "a", "b", "c" )` method call, intended to loop over values, is now `WithSequentialFrom(m => m.SingleString, "a", "b", "c" )`.
* `WithCollection(...)` is now `WithCreateEnumerableFrom(...)`.
* `Add(...)` is now `WithAddToCollection(...)`.
* `WithSetup(...)` is now `WithCustomSetup(...)`.
* `Build(n)` method is now Eager, returning the result in a `List`, rather than a Lazy `IEnumerable`.

The broad changes to the API were to avoid reliance on Type-based differences in method overload, thereby adding clarity and allowing for more possible behaviours, improving discoverability of the available options.

Additionally, extended .NET version support all the way back to Framework 4.0.
All versions of .NET Core and .NET Standard were already supported, as was .NET 5

### Migration via v3.0-alpha, or v2.0.0

A pre-release v3.0 was released to Nuget to support anyone that had used the erroneously published `v2.0` (see above)
Both those versions were a mid-way state between `v2.1` and `v4.0`.
If you used either of those versions and need specific details of the API changes between that and `v4.0`, then please see [GH Issue 12](https://github.com/Softwire/LochNessBuilder/issues/12).

## Use in a .Net Framework project

As of `v2.1`, this library has been migrated to .NET Standard, to enable use in .NET Core projects.

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

public static class MonsterBuilder
{
    public static Builder<Monster> Default
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
Monster testMonster = MonsterBuilder.Default.Build();
// testMonster has Id = 1, Colour = "Green", Age = 3.
```

To build multiple `Monster`s

```csharp
List<Monster> testMonsters = MonsterBuilder.Default.Build(5);
// testMonsters has 5 Monsters, with Ids of respectively 1, 2, 3, 4, 5, but all have the same Colour and Age.
// Each monster is fully built before the next monster is started.
```

To build multiple `Monster`s, at different points in time, but retaining any stateful properties of the `Builder` (e.g. Id Sequences)

```csharp
Builder<Monster> monsterBuilder = MonsterBuilder.Default;
Monster earlyMonster = monsterBuilder.Build();         // earlyMonster has Id=1
// Do some testing stuff.
// ...
// then later
// ...
Monster lateMonster = monsterBuilder.Build();          // lateMonster has Id=2
```

To build a Monster, but override a particular property that has previously been configured

```csharp
Monster youngMonsters = MonsterBuilder.Default.With(t => t.Age, 1).Build(4);
// youngMonsters will have Id 1-4, and be "Green" but will now have Age = 1, despite the configuration defined in the initial MonsterBuilder.
// Note that the original assignment from the original Builder has still *run*; we've simply overwritten the value later.
```

### Setup Methods

Please examine the XML docs in your IDE for full details. However, in simplified form, we have the following methods.

**Note that `Builder`s are immutable, and thus every one of these methods will produce a _new_ `Builder`, leaving the existing one untouched.**

* ##### `With()`
  * Sets a property to a value.
* ##### `WithSharedRef()`
  * Sets a ReferenceType property with the given object, assinging the same object ref to all output objects.
* ##### `WithFactory()`
  * Provide a factory method, to set a property to a newly created value each time.
* ##### `WithSequentialFrom()`
  * Provide multiple values, and the builder will cycle through them in order, for each new object built.
* ##### `WithSequentialIds()`
  * Sets a numeric property with increasing numbers, from 1 to int.MaxValue.
  * Can also provide a lambda to build some other object from the sequential Ids, instead.
  * Can override the starting value, if needed.
* ##### `WithCreateEnumerableFrom()`
  * Provide multiple values (either as a params, or by passing in an `IEnumerable<T>`), and the builder will create a suitable container and put them all onto each new object
  * Note that a new `IEnumerable<T>` will be created for each newly built object.
  * All of the most common .NET `IEnumerable` types are supported, and a clear error message is provided if not. (In which case just use a more explicit `WithFactory()` call.)
* ##### `WithBuilder()`
  * Like `WithFactory`, but the factory is specifically the `.Build()` method of the provided `Builder`.
* ##### _`IEnumerable` variations of `WithFactory()`_
  * If the property you are trying to set implements `IEnumerable<T>` then there are some additional overloads of the above 3 variations on `WithFactory()`. They allow you to provide Factory/Buildrs that simply build `Ts`, rather than having to build the whole `IEnumerable<T>`. The details of the `IEnumerable<>` portion will then get worked out for you. The resultant `IEnumerable<T>` will contain 3 elements by default, or you can specify how many `T`s should be built and put into the IEnumerable, if wanted.
* ##### `WithAddToCollection()`
  * Add the given value to the existing ICollection property.
  * This assumes that an earlier setup method (or possibly the object constructor) has initialised the ICollection beforehand.
* ##### `WithPreBuildSetup()`
  * Do an arbitrary action to the object being created, but do it *before* all the other setup actions that have been, or will be, defined on the builder.
* ##### `WithCustomSetup()`
  * Do an arbitrary action to the object being created, to set it up.
* ##### `WithPostBuildSetup()`
  * Do an arbitrary action to the object being created, but do it *after* all the other setup actions (even after other setup methods are called after this one).

See end of this README for some further notes of usage and behaviour interactions.

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
        public Lake CommunityLake { get; set; }
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
        public Egg()
        {
            Id = 3;
            Name = "Third";
        }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public static class MonsterBuilder
    {
        public static Builder<Monster> Default
        {
            get
            {
                var rand = new Random();
                var theBiggestLake = new Lake();

                return Builder<Monster>.New
                    .With(m => m.Nationality, "Scottish")                                               // All monsters will be Scottish.

                    .WithSharedRef(m => m.CommunityLake, theBiggestLake)                                // All monsters will have a reference to the same Lake in this.CommunityLake.

                    .WithSequentialFrom(m => m.Colour, "Green", "Red", "Blue")                          // Monster Colors will be Green, Red, Blue, Green, Red, ...

                    .WithSequentialIds(m => m.Id)                                                       // Ids will be 1, 2, 3, 4, 5....
                                                                                                        // This is identical to ".WithSequentialFrom(m => m.Id, Enumerable.Range(1, int.MaxValue))"

                    .WithSequentialIds(m => m.Address, x => $"Pool {x}", 0)                             // Names will be "Pool 0", "Pool 1", "Pool 2", "Pool 3", "Pool 4", ...
                                                                                                        // This is identical to ".WithSequentialFrom(m => m.Address, Enumerable.Range(0, int.MaxValue).Select(x => $"Pool {x}"))"

                    .WithCreateEnumerableFrom(m => m.Sounds, "Rarrrgggh!", "Screech!", "Wooooosh!")     // All monsters will produce all three of these sounds.
                    // Above is always identical to ".WithCreateEnumerableFrom(m => m.Sounds, new List<string>{"Rarrrgggh!", "Screech!", "Woooooh!"})"
                    // And also identical to ".WithFactory(m => m.Sounds, () => new List<string>{"Rarrrgggh!", "Screech!", "Woooooh!"})"   (given that `Sounds` is a `List<string>`)
                    // The method will create whatever manner of IEnumerable is appropriate for the type of the property being set. All of the most common .NET `IEnumerable` types are supported.

                    .WithFactory(m => m.FavouriteFood, () => new List<string>())                        // All monsters will get their own, distinct (initially empty) List<> object for food.
                    
                    .WithAddToCollection(m => m.FavouriteFood, "People")                                // All monsters like to eat people, in addition to anything that could have been configured prior to this point.
                                                                                                        // (Obviously the more natural way to achieve that would be to include it in the previous Factory, but we want to demonstrate this .WithAddToCollection method.)

                    .WithFactory(m => m.Age, () => rand.Next(6))                                        // Age might be 2, 4, 1, 4, 6, 3 ...
                    
                    .WithBuilder(m => m.HomeLake, LakeBuilder.Default)                                  // All monsters will have this.HomeLake populated with the result of "LakeBuilder.Default.Build()"
                    
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

    public static class LakeBuilder
    {
        public static Builder<Lake> Minimal
        {
            get
            {
                return Builder<Lake>.New
                    .WithSequentialIds(t => t.Id)
                    .WithSequentialFrom(t => t.Name, Enumerable.Range(1, int.MaxValue).Select(i => $"Lake {i}"))
                    .WithFactory(t => t.Monsters, () => new HashSet<Monster>());
            }
        }

        public static Builder<Lake> Default
        {
            get
            {
                return Minimal
                    .WithAddToCollection(t => t.Monsters, new Monster())
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

### Notable usages and features

Some notes on common situations and easy mistakes to make...

* The standard `.With()` method (and also `.WithSequentialFrom()` when it loops) will share the provided value(s) with all objects that get Built. As a result it is constrained to only those types which are passed-by-value.
  * If you're setting an object property then you must use either `.WithSharedRef()` or `.WithFactory()` depending on whether you want different built outputs to be sharing the same child object, or to have separate freshly-made ones.
* The `.WithCreateEnumerableFrom()` should only be used if you want to make use of its ability to create most kinds of enumerable for you.
  * It will create distinct `IEnumerable` objects for each object built, even if you gave it an appropriate `IEnumerable` yourself - the built object will get a new `IEnumerable` collection.
  * If you want to specifically create your `IEnumerable`, then you should either use `.WithSharedRef()`, or `.WithFactory()` depending on whether you intend the enumerable to be shared or not.
* Note the `.WithSequentialIds()` method, which will likely be useful for any Id-based properties.
  * All it actually does is call `.WithSequentialFrom(<propSelector>, Enumerable.Range(1, int.MaxValue))`, but it's a lot more readable!
* You can use the `.WithBuilder()` methods to setup complex sub-properties, for which you've already defined `Builder`s.
* In general, the various setup actions are executed against each object being built in the order in which they are configured on the Builder, and ALL actions are performed, even if they would be overridden by later setup actions.
  * Thus if you configure `.With(m => m.Prop, val)` multiple times on the builder, then the resultant object will have the last value that was configured, but would have triggered any setter code associated with `m.Prop` repeatedly.
* If you have a database object with properties representing DB relations, where there is a FK int property AND a FK object property (and possibly also a collection property on the other end of the relationship), then you probably want to use `.WithPostBuildSetup()` to ensure that everything gets suitably wired up at the end, to account for later modifications applied to the Builder.
* `Builder` objects are immutable; each configuration method call derives a *new* `Builder` object, leaving the original one unchanged.
  * Note that the resultant new `Builder` is *not* completely separate, in that it stills retains any shared internal state that was defined on the original.
  * e.g. If you have a parent `Builder` that uses `.WithSequentialIds()`, and derive a further `Builder` from it, then calls to either `Builder` will increment the 'shared' next Id value.

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
        .WithCustomSetup(m =>
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

## .NET Version targets

The csproj builds a package that targets .NET Framework 4.0, 4.5, 4.5.1, 4.6.1, and .NET Standard 2.0
Thus this should be usable in projects for built in .NET Framework 4.0 onwards, .NET Core 2.0 onwards, .NET Standard 2.0 onwards, and .NET 5
It is actively tested against project running under the following .NET Versions:

* .NET 5
* .NET Core 3.1
* .NET Core 2.1
* .NET Standard 2.1
* .NET Standard 2.0
* .NET Framework 4.8
* .NET Framework 4.5.2
* .NET Framework 4.5
* .NET Framework 4.0

## TODOs

* Add support for Builders for objects with no default constructor.

## Creating a new Nuget Package (Dev Notes)

With .Net Standard, you no longer have to use a nuspec file since all the package information is added to the csproj file. You will, however, need VS >= 2017

Process for releasing a new NugetPackage:

* Update the `LochNessBuilder.csproj` with any new information. This can also be done in the package tab of the project properties:
  * Identify the appropriate new semantic version number. See here if you're unsure: <https://semver.org/>
  * Update the version numbers. Note that `PackageVersion` needs to be manually updated in the `.csproj` file not the VS UI, as the VS UI seems to be buggy.
  * Write some appropriate PackageReleaseNotes in the package data.
* Update this README
  * It will then be automatically used by NuGet, as well as being displayed in GitHub.
* Create the new package.
  * Clean and Rebuild the solution, which will automatically package everything up for you.
    * The newly created package will be dropped in the `<root>\nuget` folder.
  * Delete the previous version which will be sitting beside the newly produced version.
    * Note that a clean copy of the previous version should already have been kept, in `<root>\nuget\ArchivedPackages`, when it was released. Double check that it's there and if not, look through git to recreate it.
* Deploy to Nuget!
  * Log into nuget with an account that is part of the [LochNess nuget Organisation](https://www.nuget.org/organization/LochNess)
    * _Get an existing member of the org to add you if not already included._
  * Go to the `Upload Package` Page ([link](https://www.nuget.org/packages/manage/upload))
  * Browse to the newly packaged file, and select it.
  * Nuget should auto-parse all the meta data and display it. Briefly review it.
* Test that the new package is available on Nuget, and that updating your project still works after updating it to use the new version.
* Archive the package
  * Having released a new version to Nuget, copy the binary pacakge that you released into `<root>\nuget\ArchivedPackages`, for posterity.
  * Do this now, to ensure that we have a clean version of the package, not one overwritten with later dev work!
* Publicise the release in whatever manner you deem appropriate. At very least, the people in the Authors tag probably want to know!
