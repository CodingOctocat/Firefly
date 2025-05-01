using System;

namespace Firefly.Helpers;

public static class Jitter
{
    public static int Next(int v, double offset)
    {
        offset = ((Random.Shared.NextDouble() * 2) - 1) * (v * offset);

        return (int)Math.Round(v + offset);
    }
}
