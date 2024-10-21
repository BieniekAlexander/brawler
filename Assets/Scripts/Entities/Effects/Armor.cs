using UnityEngine;

public class Armor : Effect {
    [SerializeField] public int amount;

    override public bool AppliesTo(GameObject go) => go.GetComponent<Character>()!=null;

    public int TakeDamage(int damage) {
        amount -= damage;
        return Mathf.Min(amount, 0);
    }
}