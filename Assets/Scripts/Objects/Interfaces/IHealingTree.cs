using System.Collections.Generic;

internal interface IHealingTree<T> {
    public T Parent { get; set; }
    public List<T> Children { get; }
    public void PropagateChildren();
}