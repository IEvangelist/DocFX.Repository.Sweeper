﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocFX.Repository.Extensions
{
    // https://blogs.msdn.microsoft.com/pfxteam/2012/03/05/implementing-a-simple-foreachasync-part-2/
    public static class EnumerableExtensions
    {
        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body) =>
            Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(dop)
                select Task.Run(async delegate
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            await body(partition.Current);
                        }
                    }
                }));

        public static void For<T>(this T[] array, Action<T> action) => array.For((item, _) => action(item));

        public static void For<T>(this T[] array, Action<T, int> action)
        {
            for (var i = 0; i < array.Length; ++i)
            {
                action(array[i], i);
            }
        }
    }
}