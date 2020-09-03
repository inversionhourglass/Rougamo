using Mono.Cecil;
using Mono.Collections.Generic;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    public static class MonoCollectionExtensions
    {
        public static bool TryGet(this Collection<CustomAttributeNamedArgument> arguments, string name, out CustomAttributeNamedArgument? argument)
        {
            foreach (var arg in arguments)
            {
                if(arg.Name == name)
                {
                    argument = arg;
                    return true;
                }
            }
            argument = null;
            return false;
        }
        public static Collection<T> Insert<T>(this Collection<T> collection, int index, IList<T> items)
        {
            for (var i = items.Count - 1; i >= 0; i--)
            {
                collection.Insert(index, items[i]);
            }

            return collection;
        }

        public static Collection<T> InsertBefore<T>(this Collection<T> collection, T targetItem, T item)
        {
            var index = collection.IndexOf(targetItem);
            if (index < 0) throw new System.IndexOutOfRangeException("could not found targetItem from collection");

            collection.Insert(index, item);

            return collection;
        }

        public static Collection<T> InsertBefore<T>(this Collection<T> collection, T targetItem, IList<T> items)
        {
            var index = collection.IndexOf(targetItem);
            if (index < 0) throw new System.IndexOutOfRangeException("could not found targetItem from collection");

            for (int i = items.Count - 1; i >= 0; i--)
            {
                collection.Insert(index, items[i]);
            }

            return collection;
        }

        public static Collection<T> InsertAfter<T>(this Collection<T> collection, T targetItem, T item)
        {
            var index = collection.IndexOf(targetItem);
            if (index < 0) throw new System.IndexOutOfRangeException("could not found targetItem from collection");

            collection.Insert(index + 1, item);

            return collection;
        }

        public static Collection<T> InsertAfter<T>(this Collection<T> collection, T targetItem, IList<T> items)
        {
            var index = collection.IndexOf(targetItem);
            if (index < 0) throw new System.IndexOutOfRangeException("could not found targetItem from collection");

            for (int i = items.Count - 1; i >= 0; i--)
            {
                collection.Insert(index + 1, items[i]);
            }

            return collection;
        }

        public static Collection<T> Add<T>(this Collection<T> collection, IList<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }

            return collection;
        }
    }
}
