using System;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public struct HitEvent
{
    public Hit hit;
    public int startFrame;
}

public class Cast : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public HitEvent[] hitEvents;
    private int frame = 0;
    private int duration = -1;
    private Transform origin = null;

    public void Initialize(Transform _origin)
    {
        origin = _origin;

        foreach (HitEvent hitEvent in hitEvents) {
            Assert.IsNotNull(hitEvent.hit);
            duration = Mathf.Max(duration, hitEvent.startFrame + hitEvent.hit.duration);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (HitEvent hitEvent in hitEvents) {
            if (hitEvent.startFrame == frame) {
                Hit hit = Instantiate(
                    hitEvent.hit                    
                );

                hit.Initialize(origin);
                hit.transform.position = origin.position + hitEvent.hit.offset;
            }
        }

        if (frame++ == duration)
            Destroy(gameObject);
    }
}
