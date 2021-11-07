using System;
using System.Collections.Generic;

namespace LochNessBuilder.Extensions
{
    internal static class Extensions
    {
        internal static IEnumerable<T> Plus<T>(this IEnumerable<T> head, T tail)
        {
            foreach (var item in head)
            {
                yield return item;
            }

            yield return tail;
        }

        internal static IEnumerable<T> Plus<T>(this T head, IEnumerable<T> tail)
        {
            yield return head;

            foreach (var item in tail)
            {
                yield return item;
            }
        }

        internal static IEnumerable<T> Times<T>(this int number, Func<T> generator)
        {
            for (int i = 0; i < number; i++)
            {
                yield return generator();
            }
        }

        internal static IEnumerable<T> LoopInfinitely<T>(this IEnumerable<T> items)
        {
            while (true)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var item in items)
                {
                    yield return item;
                }

                // ReSharper disable once PossibleMultipleEnumeration
                try
                {
                    items.GetEnumerator().Reset();
                }
                catch (Exception)
                {
                    // Some IEnumerables will need this. Others won't.
                    // Some that don't need it will actively complain about this.
                    // So ignore any errors thrown here.
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        internal static Func<T> GetAccessor<T>(this IEnumerable<T> items)
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