using DOGEngine.GameObjects.Properties;
using DOGEngine.GameObjects;

namespace DOGEngine.GameObjects;

public class GameObject
{
    private readonly Dictionary<Type, GameObject> children;
    public IReadOnlyCollection<GameObject> Children => children.Values;

    public T GetComponent<T>() where T : GameObject
    {
        if (children.TryGetValue(typeof(T), out var gameObject))
            return gameObject as T ?? throw new InvalidOperationException("Internal engine error");
        throw new ArgumentException("Can't find component");
    }

    public bool TryGetComponent<T>(out T? gameObject) where T : GameObject
    {
        if (children.TryGetValue(typeof(T), out GameObject? obj))
        {
            gameObject = obj as T ?? throw new InvalidOperationException("Internal engine error");
            return true;
        }

        gameObject = null;
        return false;
    }

    public virtual IEnumerable<T> GetAllInChildren<T>() where T : GameObject
    {
        if (this is T o) yield return o;
        
        foreach (GameObject child in Children)
        {
            foreach (var childComponent in child.GetAllInChildren<T>())
                yield return childComponent;
        }
    }

    public virtual IEnumerable<GameObject> GetAllWithName(string name)
    {
        
        foreach (GameObject child in Children)
        foreach (var childComponent in child.GetAllWithName(name))
            yield return childComponent;

        if (TryGetComponent(out Name? objName) && objName!.ObjName == name) yield return this;
    }

    public void AddComponent(GameObject gameObject)
    {
        if (!TryAddComponent(gameObject)) throw new ArgumentException("Can't add game object");
    }

    public bool TryAddComponent(GameObject gameObject)
    {
        if (!children.TryAdd(gameObject.GetType(), gameObject)) return false;
        gameObject.Parent = this;
        gameObject.initializeChildren = initializeChildren;
        if(initializeChildren && gameObject is IPostInitializedGameObject initialize) initialize.Initialize();
        return true;
    }

    public bool TryRemoveComponent(GameObject gameObject, bool delete = true)
    {
        if (!children.Keys.Contains(gameObject.GetType())) return false;
        if(delete) gameObject.deleteWithChildren();
        children.Remove(gameObject.GetType());
        return true;
    }

    public void RemoveComponent(GameObject gameObject, bool delete = true)
    {
        if (!TryRemoveComponent(gameObject, delete)) throw new ArgumentException("Can't remove game object");
    }

    internal  virtual void deleteWithChildren()
    {
        foreach (var child in Children)
            child.deleteWithChildren();
        if (this is IDeletableGameObject delete) delete.Delete();
    }

    public GameObject Parent { get; internal set; }

    public GameObject Root => Parent == this ? this : Parent.Root;

    internal bool initializeChildren { get; set; }

    public void InitializeAll()
    {
        foreach (var obj in GetAllInChildren<GameObject>())
        {
            obj.initializeChildren = true;
            if (obj is IPostInitializedGameObject initialize)
                initialize.Initialize();
        }
    }
    public GameObject(params GameObject[] newChildren)
    {
        Parent = this;
        children = new Dictionary<Type, GameObject>();
        foreach (var child in newChildren)
        {
            if (!children.TryAdd(child.GetType(), child)) throw new ArgumentException("Can't add component");
            child.Parent = this;
            child.initializeChildren = initializeChildren;
        }
    }
}