using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectBurning: StatusEffectBase {
    [SerializeField] int damage;

    public override void Initialize(Character _character) {
        base.Initialize(_character);
    }

    public override void Tick() {    
        target.TakeDamage(damage, Vector3.zero, 0);
    }
}
