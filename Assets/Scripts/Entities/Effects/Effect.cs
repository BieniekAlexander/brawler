using UnityEngine;

public class Effect : Cast {
    public virtual bool AppliesTo(GameObject go) => false;
}
