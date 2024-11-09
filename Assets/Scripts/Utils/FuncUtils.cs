using System;
using System.Collections.Generic;
using System.Linq;

public static class FuncUtils<S> {
    public static Func<S, bool> OR(IEnumerable<Func<S, bool>> funcs) {
        return x => funcs.Any(f => f(x));
    }

    public static Func<S, bool> AND(IEnumerable<Func<S, bool>> funcs) {
        return x => funcs.All(f => f(x));
    }
}