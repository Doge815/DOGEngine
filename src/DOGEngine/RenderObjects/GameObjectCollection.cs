namespace DOGEngine.RenderObjects;

public class GameObjectCollection : GameObject
{
    private readonly List<GameObject> collection;
    public IReadOnlyCollection<GameObject> Collection => collection.AsReadOnly();
    
    public GameObjectCollection()
    {
        collection = new List<GameObject>();
    }

    public void CollectionAddComponents(params GameObject[] children)
    {
        foreach (var child in children)
        {
            collection.Add(child);
            child.Parent = this;
            child.initializeChildren = initializeChildren;
        }
        if(initializeChildren)
            foreach (var child in children)
                if(child is IPostInitializedGameObject initialize) 
                    initialize.Initialize();
    }


    public override IEnumerable<T> GetAllInChildren<T>()
    {
        if (this is T o) yield return o;
        
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