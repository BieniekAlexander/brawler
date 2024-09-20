using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CastRoutine {
    public CastBase cast;
    public int frequency;
    public int timer;
}

public class CastGenerator : MonoBehaviour
{
    [SerializeField] List<CastRoutine> castRoutines;
    [SerializeField] Transform castTargetPosition;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < castRoutines.Count; i++) {
            CastRoutine cr = castRoutines[i];
            
            if (++cr.timer == cr.frequency) {
                CastBase c = CastBase.Initiate(cr.cast, null, transform, castTargetPosition, false);
                cr.timer = 0;
            }
        }
    }
}
