using System.Linq;
using System;
using System.Runtime.CompilerServices;

namespace ApiApplication.Helper
{
    public class RandomValue
    {
        public static int GetRandom(int value)
        {
            Random _rand = new Random();
            var result = _rand.Next(0, value);
            return result;
        }
    }
}
