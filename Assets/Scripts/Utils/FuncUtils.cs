using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal.Commands;
using UnityEngine;

public static class FuncUtils<S> {
    public static Func<S, bool> OR(IEnumerable<Func<S, bool>> funcs) {
        return x => funcs.Any(f => f(x));
    }

    public static Func<S, bool> AND(IEnumerable<Func<S, bool>> funcs) {
        return x => funcs.All(f => f(x));
    }
}

public static class EnumUtils<T> {
    public static T ArgMax(IEnumerable<T> enumerable, Func<T, float> func) {
        float maxVal = Mathf.NegativeInfinity;
        T maxArg = enumerable.First();

        foreach (T t in enumerable) {
            float val = func(t);

            if (val > maxVal) {
                maxArg = t;
                maxVal = val;
            }
        }

        return maxArg;
    }
}