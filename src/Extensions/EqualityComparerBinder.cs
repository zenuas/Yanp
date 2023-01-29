using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Extensions;

public class EqualityComparerBinder<T> : IEqualityComparer<T>
{
    public new required Func<T?, T?, bool> Equals;

    bool IEqualityComparer<T>.Equals(T? x, T? y) => Equals(x, y);
    int IEqualityComparer<T>.GetHashCode([DisallowNull] T obj) => obj.GetHashCode();
}
