using System.Collections.Generic;
using System;
using System.Linq;

namespace ApiApplication.Extensions
{
    public static class Grouping
    {

        /// <summary>
        /// https://stackoverflow.com/questions/20469416/linq-to-find-series-of-consecutive-numbers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> GroupWhile<T>(this IEnumerable<T> seq, Func<T, T, bool> condition)
        {
            T prev = seq.First();
            List<T> list = new List<T>() { prev };

            foreach (T item in seq.Skip(1))
            {
                if (condition(prev, item) == false)
                {
                    yield return list;
                    list = new List<T>();
                }
                list.Add(item);
                prev = item;
            }

            yield return list;
        }

    }
}
