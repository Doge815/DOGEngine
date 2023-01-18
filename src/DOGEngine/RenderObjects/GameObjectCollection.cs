namespace DOGEngine.RenderObjects;

public class GameObjectCollection : GameObject
{
    private readonly List<GameObject> collection;
    public IReadOnlyCollection<GameObject> Collection => collection.AsReadOnly();
    
    public GameObjectCollection(params GameObject[] newChildren)
    {
        collection = new List<GameObject>();
        CollectionAddComponents(newChildren);
    }

    public void CollectionAddComponent(GameObject child)
    {
        collection.Add(child);
        child.Parent = this;
        if(child is IPostInitializedGameObject initialize)
            initialize.Initialize();
    }

    public void CollectionAddComponents(params GameObject[] children)
    {
        foreach (var child in children)
        {
            collection.Add(child);
            child.Parent = this;
        }
        foreach (var child in children)
            if(child is IPostInitializedGameObject initialize)
                initialize.Initialize();
    }


    public override IEnumerable<T> GetAllInChildren<T>()
    {
        foreach (var child in base.GetAllInChildren<T>())
            yield return child;

        foreach (GameObject child in collection)
        foreach (T childComponent in child.GetAllInChildren<T>())
            yield return childComponent;
    }
    public override IEnumerable<GameObject> GetAllWithName(string name)
    {
        foreach (var child in base.GetAllWithName(name))
            yield return child;

        foreach (GameObject child in collection)
        foreach (GameObject childComponent in child.GetAllWithName(name))
            yield return childComponent;
    }
}