using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour {
    [SerializeField] public Character character;
    private Slider healthBar;
    
    private void Start() {
        healthBar = GetComponent<Slider>();
        healthBar.maxValue = Character.HPMax;
        healthBar.value = character.HP;
    }

    public void Update() {
        // TODO this happens every frame and will be slow - make this a callback instead
        healthBar.value = character.HP;
    }
}