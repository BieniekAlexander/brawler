using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// https://weeklyhow.com/how-to-make-a-health-bar-in-unity/#google_vignette
/// </summary>
public class HPBarVisuals : MonoBehaviour {
    [HideInInspector] public Character Character;
    private Slider healthBar;

    public void Initialize(Character _character) {
        Character = _character;
        healthBar.maxValue = Character.HPMax;
        healthBar.value = Character.HP;
    }

    private void Awake() {
        healthBar = GetComponent<Slider>();
    }

    public void Update() {
        // TODO this happens every frame and will be slow - make this a callback instead
        if (Character==null) {
            Destroy(gameObject);
        } else {
            healthBar.value = Character.HP;
        }
    }
}