using UnityEngine;

public class Armor : Effect { // TODO maybe IDamageable?
    [SerializeField] public int AP;

    override public bool AppliesTo(GameObject go) => go.GetComponent<Character>()!=null;

    public int TakeDamage(in int damage) {
        int takenDamage = IDamageable.GetTakenDamage(damage, AP);
        AP -= takenDamage;
        return damage-takenDamage;
    }

    override protected void OnInitialize() {
        base.OnInitialize();
        About.gameObject.GetComponent<Character>().Armors.Insert(0, this);
    }

    override protected void Tick() {
        if (AP <= 0) {
            Destroy(gameObject);
        }
    }

    override protected void OnDestruction() {
        if (About != null) {
            About.gameObject.GetComponent<Character>().Armors.Remove(this);
        }
    }
}