using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LochNessBuilder;
// ReSharper disable InconsistentNaming

namespace Tests
{
    public class TestObject
    {
        public int Id { get; set; }
        public int IntProp { get; set; }
        public int? NullableIntProp { get; set; }
        public short ShortProp { get; set; }
        public long LongProp { get; set; }
        public string StringProp { get; set; }
        public object ObjectProp { get; set; }

        public int[] ArrayProp { get; set; }
        public List<int> ListProp { get; set; }
        public IList<int> IListProp { get; set; }
        public Collection<int> CollectionProp { get; set; }
        public ICollection<int> ICollectionProp { get; set; }
        public Queue<int> QueueProp { get; set;}
        public Stack<int> StackProp { get; set; }
        public IEnumerable<int> IEnumerableProp { get; set; }
        public IQueryable<int> IQueryableProp { get; set; }
        public HashSet<int> HashSetProp { get; set; }
        public ISet<int> ISetProp { get; set; }
        public ReadOnlyCollection<int> ReadOnlyCollectionProp { get; set; }
        public IReadOnlyCollection<int> IReadOnlyCollectionProp { get; set; }

        public List<short> ShortListProp { get; set; }
        public List<long> LongListProp { get; set; }
        public List<object> ObjectListProp { get; set; }
        public List<TestSubObject> SubObjectListProp { get; set; }

        //Intended not to be supported.
        public ConcurrentBag<int> ConcurrentBagProp { get; set; }
        public IOrderedEnumerable<int> IOrderedEnumerableProp { get; set; }

        public TestSubObject SubObjectProp { get; set; }
        public int SubObjectRef { get; set; }
    }

    public class TestSubObject
    {
        public int Id { get; set; }
        public string StringProp { get; set; }

        public TestObject AssociatedTestObject { get; set; }
    }

    [BuilderFactory]
    public static class RegisteredSubObjectBuilder
    {
        public static Builder<TestSubObject> Other => Builder<TestSubObject>.New.With(subObj => subObj.StringProp, "OtherBuilder");
        public static Builder<TestSubObject> New => Builder<TestSubObject>.New.With(subObj => subObj.StringProp, "NewBuilder");
    }
}