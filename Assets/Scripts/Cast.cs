using System;
using UnityEngine;

[Serializable]
public struct HitEvent
{
    public Hit hit;
    public float startTime;
}

public class Cast : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public HitEvent[] hits;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
