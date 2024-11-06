using UnityEngine;

public class Shield : MonoBehaviour {
    public Transform Transform { get { return transform; } }
    public int ParryWindow { get; private set; }
    private int _parryWindowMax = 30;
    
    private void FixedUpdate() {
        ParryWindow--;
    }

    private void OnEnable() {
        ParryWindow = _parryWindowMax;
    }
}
