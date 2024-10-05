using System.Collections.Generic;
using UnityEngine;

// TODO move the RotatingClockwise code from Character to here

public interface ICasts {
    /// <summary/>
    /// <returns>The transform that indicates the origin position for any <typeparamref name="Castable"/>s</returns>
    public Transform GetOriginTransform();

    /// <summary/>
    /// <returns>The transform that indicates the target position for any <typeparamref name="Castable"/>s</returns>
    public Transform GetTargetTransform();

    /// <summary/>
    /// <remarks>Used to flip the casting direction of <typeparamref name="Castable"/>s</remarks>
    /// <returns>true, if the Caster is rotating clockwise</returns>
    public bool IsRotatingClockwise();

    /// <summary>
    /// Get a list of all of the things that the ICasts has casted
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Castable> ActiveCastables { get; }

    // status effects
    public int MaimStack { get; set; } // can't use attacks
    public int SilenceStack { get; set; } // can't cast spells
}
