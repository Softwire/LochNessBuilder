# LochNessBuilder
A C# Builder library to automate creation of objects for tests

### Examples
The builder for a number of `Widget`s:
```
using System.Linq;
using LochNessBuilder;
  [Builder]
  public static class WidgetBuilder
  {
      public static Builder<Widget> New
      {
          get
          {
              return Builder<Widget>.New
                  .With(t => t.Id, Enumerable.Range(1, int.MaxValue))
                  .With(t => t.Name, Enumerable.Range(1, int.MaxValue).Select(i => $"Unit {i}"));
          }
      }
  }
  
  public class Widget
  {
    public int Id { get; set; }
    public string Name { get; set; }
  }
```
How you would then build a number of `Widget`s for use in tests (using types in place of `var` for clarity):
```
// To build a single Widget
Widget testWidget = WidgetBuilder.New.Build();
// testWidget has Id = 1, Name = "Unit 1"
```
```
// To build an enumerable of Widgets
IEnumerable<Widget> testWidgets = WidgetBuilder.New.Build(5);
// testWidgets has 5 Widgets, with Ids of respectively 1, 2, 3, 4, 5 and matching Names
```
```
// To build an enumerable of Widgets, each with a fixed Name
IEnumerable<Widget> nameWidgets = WidgetBuilder.New.WithPostBuildSetup(t => t.Name, "Constant").Build(4);
// nameWidgets have Ids 1-4 but all have name "Constant"
```
```
// To build an enumerable of Widgets, each with a name matching the previous widget's Id
string prevName = "0";
IEnumerable<Widget> seqWidgets = WidgetBuilder.New
    .WithSetup(t => t.Name = prevName)
    .WithPostBuildSetup(t => prevName = t.Id.ToString()
    .Build(3);
// seqWidgets have Ids 1, 2, 3 and Names "0", "1", "2"
```
Sometimes you may need a stateful Builder - for example, if there are constraints on uniqueness of certain properties. 
```
// In the enclosing class
private Builder<Widget> widgetBuilder = WidgetBuilder.New;
// Then use this form in methods instead
Widget testWidget = widgetBuilder.Build();
```
### Variations on the Builder
The builder can also be customised to build things differently. You can use factories (`withFactory`), setup (`withSetup`), post-build setup (`withPostBuildSetup`), collections (`withCollection`)

## TODOs
* Update with more detailed documentation for Builder
* Better examples
