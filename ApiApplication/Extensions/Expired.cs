﻿using System;
using System.Runtime.CompilerServices;

namespace ApiApplication.Extensions
{
    public static class Expired
    {
        public static bool HasExpired(this DateTime time, int minutes = 10)
        {
            return DateTime.UtcNow - time > TimeSpan.FromMinutes(minutes);
        }
    }
}
