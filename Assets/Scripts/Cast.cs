using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Collections;
using TreeEditor;

[Serializable]
public struct HitEvent
{
    public List<Hit> hits;
    public int startFrame;
}

public class Cast : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public HitEvent[] hitEvents;
    private int frame = 0;
    private int duration = -1;

    private Transform origin = null;
    private Vector3 initialPosition = new();
    private Quaternion initialRotation = new();

    public void Initialize(Transform _origin)
    {
        origin = _origin;
        foreach (HitEvent hitEvent in hitEvents) {
            foreach (Hit hit in hitEvent.hits)
            {
                Assert.IsNotNull(hit);
                duration = Mathf.Max(duration, hitEvent.startFrame + hit.duration);
            }
        }
    }

    public void Initialize(Vector3 _initialPosition, Quaternion _initialRotation)
    {
        initialPosition = _initialPosition;
        initialRotation = _initialRotation;

        foreach (HitEvent hitEvent in hitEvents)
        {
            foreach (Hit hit in hitEvent.hits)
            {
                Assert.IsNotNull(hit);
                duration = Mathf.Max(duration, hitEvent.startFrame + hit.duration);
            }
        }
    }

    void FixedUpdate()
    {
        foreach (HitEvent hitEvent in hitEvents) {
            if (hitEvent.startFrame == frame) {
                foreach (Hit hit in hitEvent.hits)
                {
                    Hit h = Instantiate(hit);

                    if (origin != null) {
                        h.Initialize(origin);
                    } else {
                        h.Initialize(initialPosition, initialRotation);
                    }

                    h.HandleMove();
                    h.HandleHitCollisions();
                }
            }
        }

        if (frame++ >= duration)
            Destroy(gameObject);
    }
}
