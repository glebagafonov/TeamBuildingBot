using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Infrastructure.Helpers
{
    public static class CollectionHelper
    {
        public static void RemoveAll<T>(this IList<T> list, Func<T, bool> predicate) {
            for (int i = 0; i < list.Count; i++) {
                if (predicate(list[i])) {
                    list.RemoveAt(i--);
                }
            }
        }
        
        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate) {
            T element;

            for (int i = 0; i < collection.Count; i++) {
                element = collection.ElementAt(i);
                if (predicate(element)) {
                    collection.Remove(element);
                    i--;
                }
            }
        }
    }
}