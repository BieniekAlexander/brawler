using UnityEngine;

public class Armor : Effect {
    [SerializeField] public int amount;

    public override bool CanEffect(MonoBehaviour _target) {
        return (_target is Character);
    }

    public int TakeDamage(int damage) {
        amount -= damage;
        return Mathf.Min(amount, 0);
    }
}