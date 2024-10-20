using UnityEngine;

public class Armor : Cast {
    [SerializeField] public int amount;

    public override bool AppliesTo(GameObject go) => go.GetComponent<Character>()!=null;

    public int TakeDamage(int damage) {
        amount -= damage;
        return Mathf.Min(amount, 0);
    }
}