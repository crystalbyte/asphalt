#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace Crystalbyte.Asphalt {
    internal static class CollectionExtensions {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action) {
            foreach (var item in collection) {
                action(item);
            }
        }

        public static void ForEach(this IEnumerable collection, Action<object> action) {
            foreach (var item in collection) {
                action(item);
            }
        }

        public static void AddRange(this IList collection, IEnumerable range) {
            range.ForEach(x => collection.Add(x));
        }

        public static void AddRange<T>(this Collection<T> collection, IEnumerable<T> range) {
            range.ForEach(collection.Add);
        }

        public static void AddRange<T>(this Collection<T> collection, Func<IEnumerable<T>> iter) {
            collection.AddRange(iter());
        }

        public static void AddRange<T>(this IList<T> collection, IEnumerable<T> range) {
            range.ForEach(collection.Add);
        }

        public static List<T> Sample<T>(this IList<T> collection, int count) {
            var distance = collection.Count / count;
            var indices = Enumerable.Range(0, collection.Count).Where(x => x % distance == 0);
            return indices.Select(x => collection[x]).ToList();
        }
    }
}