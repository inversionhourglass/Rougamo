using Mono.Cecil;
using Mono.Collections.Generic;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    public static class MonoCollectionExtensions
    {
        /// <summary>
        /// try get a argument from <paramref name="arguments"/> which argument name equals <paramref name="name"/>
        /// </summary>
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

        public static void Insert<T>(this Collection<T> collection, int index, IList<T> items)
        {
            for (var i = items.Count - 1; i >= 0; i--)
            {
                collection.Insert(index, items[i]);
            }
        }

        /// <summary>
        /// insert <paramref name="item"/> before <paramref name="targetItem"/>
        /// </summary>
        /// <returns><paramref name="item"/></returns>
        public static T InsertBefore<T>(this Collection<T> collection, T targetItem, T item)
        {
            var index = collection.IndexOf(targetItem);
            if (index < 0) throw new System.IndexOutOfRangeException("could not found targetItem from collection");

            collection.Insert(index, item);

            return item;
        }

        /// <summary>
        /// insert <paramref name="items"/> before <paramref name="targetItem"/>
        /// </summary>
        public static void InsertBefore<T>(this Collection<T> collection, T targetItem, IList<T> items)
        {
            var index = collection.IndexOf(targetItem);
            if (index < 0) throw new System.IndexOutOfRangeException("could not found targetItem from collection");

            for (int i = items.Count - 1; i >= 0; i--)
            {
                collection.Insert(index, items[i]);
            }
        }

        /// <summary>
        /// insert <paramref name="item"/> before <paramref name="targetItem"/> or append to tail if <paramref name="targetItem"/> is null
        /// </summary>
        /// <returns><paramref name="item"/></returns>
        public static T InsertBeforeOrAppend<T>(this Collection<T> collection, T? targetItem, T item)
        {
            if (targetItem == null)
            {
                collection.Add(item);
            }
            else
            {
                collection.InsertBefore(targetItem, item);
            }

            return item;
        }

        /// <summary>
        /// insert <paramref name="items"/> before <paramref name="targetItem"/> or append to tail if <paramref name="targetItem"/> is null
        /// </summary>
        public static void InsertBeforeOrAppend<T>(this Collection<T> collection, T? targetItem, IList<T> items)
        {
            if (targetItem == null)
            {
                collection.Add(items);
            }
            else
            {
                collection.InsertBefore(targetItem, items);
            }
        }

        /// <summary>
        /// insert <paramref name="item"/> after <paramref name="targetItem"/>
        /// </summary>
        /// <returns><paramref name="item"/></returns>
        public static T InsertAfter<T>(this Collection<T> collection, T targetItem, T item)
        {
            var index = collection.IndexOf(targetItem);
            if (index < 0) throw new System.IndexOutOfRangeException("could not found targetItem from collection");

            if (++index == collection.Count)
            {
                collection.Add(item);
            }
            else
            {
                collection.Insert(index, item);
            }

            return item;
        }

        /// <summary>
        /// insert <paramref name="items"/> after <paramref name="targetItem"/>
        /// </summary>
        public static void InsertAfter<T>(this Collection<T> collection, T targetItem, IList<T> items)
        {
            var index = collection.IndexOf(targetItem);
            if (index < 0) throw new System.IndexOutOfRangeException("could not found targetItem from collection");

            if (++index == collection.Count)
            {
                collection.Add(items);
            }
            else
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    collection.Insert(index, items[i]);
                }
            }
        }

        /// <summary>
        /// insert <paramref name="item"/> after <paramref name="targetItem"/> or append to tail if <paramref name="targetItem"/> is null
        /// </summary>
        /// <returns><paramref name="item"/></returns>
        public static T InsertAfterOrAppend<T>(this Collection<T> collection, T? targetItem, T item)
        {
            if (targetItem == null)
            {
                collection.Add(item);
            }
            else
            {
                collection.InsertAfter(targetItem, item);
            }

            return item;
        }

        /// <summary>
        /// insert <paramref name="items"/> after <paramref name="targetItem"/> or append to tail if <paramref name="targetItem"/> is null
        /// </summary>
        public static void InsertAfterOrAppend<T>(this Collection<T> collection, T? targetItem, IList<T> items)
        {
            if (targetItem == null)
            {
                collection.Add(items);
            }
            else
            {
                collection.InsertAfter(targetItem, items);
            }
        }

        /// <summary>
        /// add <paramref name="items"/> into <paramref name="collection"/>
        /// </summary>
        public static void Add<T>(this Collection<T> collection, IList<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
