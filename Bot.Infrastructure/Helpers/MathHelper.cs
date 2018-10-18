using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Infrastructure.Helpers
{
    public static class MathHelper
    {
        public static long Factorial(long number)
        {
            return number <= 1 ? 1 : number * Factorial(number);
        }

        public static long Combination(long n, long k)
        {
            double sum = 0;
            for (long i = 0; i < k; i++)
            {
                sum += Math.Log10(n - i);
                sum -= Math.Log10(i + 1);
            }

            return (long) Math.Pow(10, sum);
        }

        public static IEnumerable<IEnumerable<T>> Combinations<T>(List<T> elements, int k)
        {
            //не работает с 2
            return k == 0
                ? new[] {new T[0]}
                : elements.SelectMany((e, i) =>
                    Combinations(elements.Skip(i + 1).ToList(), (k - 1)).Select(c => (new[] {e}).Concat(c)));
        }
    }
}