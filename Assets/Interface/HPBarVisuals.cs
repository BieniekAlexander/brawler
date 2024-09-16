using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// https://weeklyhow.com/how-to-make-a-health-bar-in-unity/#google_vignette
/// </summary>
public class HPBarVisuals : MonoBehaviour {
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