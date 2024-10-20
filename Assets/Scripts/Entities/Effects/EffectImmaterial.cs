using UnityEngine;

public class EffectImmaterial: Cast {
    private int originalLayer;

    protected override void OnInitialize() {
        About.GetComponent<ICollidable>().ImmaterialStack++;
        originalLayer = About.gameObject.layer;
        About.gameObject.layer = LayerMask.NameToLayer("Immaterial");
        // TODO can I just add to the immaterial stack and ignore the Layer switch?
        // proramming it that way would mean I don't have to keep track of the actual stack of layer changes
        // the current implementation logically errs if mutliple immaterials are applied
    }

    protected void OnDestroy() {
        if (About != null){
            About.gameObject.layer = originalLayer;
            About.GetComponent<ICollidable>().ImmaterialStack--;
        }
    }

    public override bool AppliesTo(MonoBehaviour mono) {
        return mono is ICollidable;
    }
}    