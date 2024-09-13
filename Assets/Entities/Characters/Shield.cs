using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static CharacterControls;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Shield : MonoBehaviour {
    public int DamageTier { get; set; } = 2;
    public Collider Collider { get; set; }
    public Material Material { get; set; }
    
    // public float - TODO should I tie the shield to a resource?

    private void Awake() {
        Collider = GetComponent<Collider>();
        Material = GetComponent<Renderer>().material;
    }

    private void Update() {
        HandleVisuals();
    }

    private void HandleVisuals() {
        /*
         * Bolt Indicator
         */
        // duration
        if (DamageTier==2)
            Material.color=Color.blue;
        else
            Material.color=Color.magenta;
    }
}
