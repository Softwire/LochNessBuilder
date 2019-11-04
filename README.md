# LochNessBuilder
A C# Builder library to automate creation of objects for tests

## Use in a .Net Framework project

As of version 2.0, this library has been migrated to .Net standard, to enable use in .Net core projects.

To continue consuming this library from a .Net Framework project, the following reference will need to be added to the `.csproj` file:

`<Reference Include="netstandard" />`

### Examples
The builder for a number of `Monster`s:
```
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
```
// To build a single Monster
Monster testMonster = MonsterBuilder.New.Build();
// testMonster has Id = 1, Name = "Name 1"
```
```
// To build an enumerable of Monsters
IEnumerable<Monster> testMonsters = MonsterBuilder.New.Build(5);
// testMonsters has 5 Monsters, with Ids of respectively 1, 2, 3, 4, 5 and matching Names
```
```
// To build an enumerable of Monsters, each with a fixed Name
IEnumerable<Monster> nameMonsters = MonsterBuilder.New.WithPostBuildSetup(t => t.Name, "Constant").Build(4);
// nameMonsters have Ids 1-4 but all have name "Constant"
```
```
// To build an enumerable of Monsters, each with a name matching the previous monster's Id
string prevName = "0";
IEnumerable<Monster> seqMonsters = MonsterBuilder.New
    .WithSetup(t => t.Name = prevName)
    .WithPostBuildSetup(t => prevName = t.Id.ToString()
    .Build(3);
// seqMonsters have Ids 1, 2, 3 and Names "0", "1", "2"
```
Sometimes you may need a stateful Builder - for example, if there are constraints on uniqueness of certain properties. 
```
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
