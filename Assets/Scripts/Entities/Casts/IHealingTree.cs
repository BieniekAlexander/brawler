using System.Collections.Generic;
using UnityEngine.Assertions;

internal interface IHealingTree<T> {
    public void AddChild(T newChild);

    public void PropagateChildren();
}