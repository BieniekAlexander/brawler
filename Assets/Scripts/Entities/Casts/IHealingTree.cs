internal interface IHealingTree<T> {
    public void AddChild(T newChild);

    public void PropagateChildren();
}