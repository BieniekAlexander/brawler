using Unity.VisualScripting;
using UnityEngine;

public class ProjectileNuke : Projectile
{
    private Material _material;
    
    public void Start()
    {
        base.Start();
        _material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        _material.color = new Color(((float)frame)/duration, 0f, 0f, 1f);
    }
}
