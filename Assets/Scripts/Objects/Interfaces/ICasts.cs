using System.Collections.Generic;
using UnityEngine;

// TODO move the RotatingClockwise code from Character to here

public interface ICasts {
    /// <summary/>
    /// <returns>The transform that indicates the origin position for any <typeparamref name="Castable"/>s</returns>
    public Transform GetAboutTransform();

    /// <summary/>
    /// <returns>The transform that indicates the target position for any <typeparamref name="Castable"/>s</returns>
    public Transform GetTargetTransform();

    /// <summary/>
    /// <remarks>Used to flip the casting direction of <typeparamref name="Castable"/>s</remarks>
    /// <returns>true, if the Caster is rotating clockwise</returns>
    public bool IsRotatingClockwise();

    // status effects
    public int MaimStack { get; set; } // can't use attacks
    public int SilenceStack { get; set; } // can't cast spells

    // Ownership Evaluation
    public int TeamBitMask {get; }
}
