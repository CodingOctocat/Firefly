using System;
using System.Collections.Generic;

namespace Firefly.Models;

public struct FireSystem : IComparable<FireSystem>, IComparable
{
    private static readonly Dictionary<string, int> _fireSystems = [];

    private static int _order;

    public string Name { get; }

    public int Order { get; }

    public FireSystem(string name)
    {
        Name = name;

        if (_fireSystems.TryGetValue(name, out int value))
        {
            Order = value;
        }
        else
        {
            _fireSystems.Add(name, _order);
            Order = _order;
            _order++;
        }
    }

    public static implicit operator FireSystem(string fireSystem)
    {
        return new FireSystem(fireSystem);
    }

    public static implicit operator string(FireSystem fireSystem)
    {
        return fireSystem.Name;
    }

    public static bool operator !=(FireSystem left, FireSystem right)
    {
        return !(left == right);
    }

    public static bool operator <(FireSystem left, FireSystem right)
    {
        return left.Order < right.Order;
    }

    public static bool operator <=(FireSystem left, FireSystem right)
    {
        return left.Order <= right.Order;
    }

    public static bool operator ==(FireSystem left, FireSystem right)
    {
        return left.Equals(right);
    }

    public static bool operator >(FireSystem left, FireSystem right)
    {
        return left.Order > right.Order;
    }

    public static bool operator >=(FireSystem left, FireSystem right)
    {
        return left.Order >= right.Order;
    }

    public static void ResetOrder()
    {
        _order = 0;
        _fireSystems.Clear();
    }

    public readonly int CompareTo(FireSystem other)
    {
        return Order.CompareTo(other.Order);
    }

    public readonly int CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        return CompareTo((FireSystem)obj);
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is FireSystem other)
        {
            return Order == other.Order;
        }

        return false;
    }

    public override readonly int GetHashCode()
    {
        return Order.GetHashCode();
    }

    public override readonly string ToString()
    {
        return Name;
    }
}
