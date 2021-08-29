# LochNessBuilder

A C# Builder library to automate creation of objects for tests

## Migration Guide

### **Do not use v2.0.0**

This version was released inadvertantly and has been deprecated and de-listed from nuget.org

Versions `2.1` and `3.0-alpha` are available

* If you wanted "`v1.x` but on .NET Standard", then use v2.1.
* If you really wanted "the version that was released as v2.0.0" then use v3.0-alpha, though this is not release-ready and should only be used if you've already integrated against the inadvertant v2.0.0 release.

### Migration from v1.0 to v2.1

No API changes. No Migration required.
MIT License was added.

## Use in a .Net Framework project

As of version 2.0, this library has been migrated to .Net standard, to enable use in .Net core projects.

To continue consuming this library from a .Net Framework project, the following reference will need to be added to the `.csproj` file:

`<Reference Include="netstandard" />`

### Examples

The builder for a number of `Monster`s:

```csharp
using System.Linq;
using LochNessBuilder;

[Builder]
public static class MonsterBuilder
{
    public static Builder<Monster> New
    {
        get
        {
            return Builder<Monster>.New
                .With(t => t.Id, Enumerable.Range(1, int.MaxValue))
                .With(t => t.Name, Enumerable.Range(1, int.MaxValue).Select(i => $"Name {i}"));
        }
    }
}
  
public class Monster
{
  public int Id { get; set; }
  public string Name { get; set; }
}
```

How you would then build a number of `Monster`s for use in tests (using types in place of `var` for clarity):

```csharp
// To build a single Monster
Monster testMonster = MonsterBuilder.New.Build();
// testMonster has Id = 1, Name = "Name 1"
```

```csharp
// To build an enumerable of Monsters
IEnumerable<Monster> testMonsters = MonsterBuilder.New.Build(5);
// testMonsters has 5 Monsters, with Ids of respectively 1, 2, 3, 4, 5 and matching Names
```

```csharp
// To build an enumerable of Monsters, each with a fixed Name
IEnumerable<Monster> nameMonsters = MonsterBuilder.New.WithPostBuildSetup(t => t.Name, "Constant").Build(4);
// nameMonsters have Ids 1-4 but all have name "Constant"
```

```csharp
// To build an enumerable of Monsters, each with a name matching the previous monster's Id
string prevName = "0";
IEnumerable<Monster> seqMonsters = MonsterBuilder.New
    .WithSetup(t => t.Name = prevName)
    .WithPostBuildSetup(t => prevName = t.Id.ToString()
    .Build(3);
// seqMonsters have Ids 1, 2, 3 and Names "0", "1", "2"
```

Sometimes you may need a stateful Builder - for example, if there are constraints on uniqueness of certain properties.

```csharp
// In the enclosing class
private Builder<Monster> monsterBuilder = MonsterBuilder.New;
// Then use this form in methods instead
Monster testMonster = monsterBuilder.Build();
```

### Variations on the Builder

The builder can also be customised to build things differently. You can use factories (`withFactory`), setup (`withSetup`), post-build setup (`withPostBuildSetup`), collections (`withCollection`)

## TODOs

* Update with more detailed documentation for Builder
* Better examples
