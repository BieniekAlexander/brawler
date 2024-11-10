using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class RandomUtils {
    /// <typeparam name="T">The type of thing being chosen</typeparam>
    /// <param name="weights">An ordered set of weights to use for selection; need not total 1.0</param>
    /// <param name="choices">An ordered set of choices to choose from</param>
    /// <returns>A choice</returns>
    public static T Choice<T>(IEnumerable<float> weights, IEnumerable<T> choices) {
        float r = Random.Range(0f, weights.Sum());
        float aggOdd = 0f;

        foreach ((T choice, float odd) in choices.Zip(weights, (a,b)=>(a,b))) {
            aggOdd += odd;
            if (r < aggOdd) {
                return choice;
            }
        }

        return choices.Last();
    }
}