using System;
using System.Collections.Generic;
using System.Linq;

namespace LochNessBuilder.Extensions
{
    public static class Extensions
    {
        public static IEnumerable<T> Plus<T>(this IEnumerable<T> head, T tail)
        {
            foreach (var item in head)
            {
                yield return item;
            }

            yield return tail;
        }

        public static IEnumerable<T> Plus<T>(this T head, IEnumerable<T> tail)
        {
            yield return head;

            foreach (var item in tail)
            {
                yield return item;
            }
        }

        public static IEnumerable<T> Times<T>(this int number, Func<T> generator)
        {
            return Enumerable.Range(1, number).Select(_ => generator());
        }

        public static IEnumerable<T> LoopInfinitely<T>(this IEnumerable<T> items)
        {
            while (true)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var item in items)
                {
                    yield return item;
                }

                // ReSharper disable once PossibleMultipleEnumeration
                items.GetEnumerator().Reset();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public static Func<T> GetAccessor<T>(this IEnumerable<T> items)
        {
            var enumerator = items.GetEnumerator();

            return () =>
            {
                enumerator.MoveNext();
                return enumerator.Current;
            };
        }
    }
}