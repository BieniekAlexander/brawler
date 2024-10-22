using UnityEngine;

public class EffectImmaterial: Effect {
    private int originalLayer;

    protected override void OnInitialize() {
        About.GetComponent<ICollidable>().ImmaterialStack++;
        originalLayer = About.gameObject.layer;
        About.gameObject.layer = LayerMask.NameToLayer("Immaterial");
        // TODO can I just add to the immaterial stack and ignore the Layer switch?
        // proramming it that way would mean I don't have to keep track of the actual stack of layer changes
        // the current implementation logically errs if mutliple immaterials are applied
    }

    override protected void OnDestruction() {
        if (About != null){
            About.gameObject.layer = originalLayer;
            About.GetComponent<ICollidable>().ImmaterialStack--;
        }
    }

    override public bool AppliesTo(GameObject go) => go.GetComponent<ICollidable>() != null;
}    