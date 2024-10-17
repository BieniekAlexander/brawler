using UnityEngine;

public class Armor : Castable {
    [SerializeField] public int amount;

    public override bool AppliesTo(MonoBehaviour mono) {
        return mono is Character;
    }

    public int TakeDamage(int damage) {
        amount -= damage;
        return Mathf.Min(amount, 0);
    }
}