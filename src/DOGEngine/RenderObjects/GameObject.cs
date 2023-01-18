using DOGEngine.RenderObjects.Properties;
using DOGEngine.Texture;

namespace DOGEngine.RenderObjects;

public interface IPostInitializedGameObject
{
    protected bool NotInitialized { get; set; }

    internal sealed void Initialize()
    {
        if (NotInitialized) InitFunc();
        NotInitialized = false;
    }

    public void InitFunc();
}
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
        foreach (GameObject child in Children)
        foreach (var childComponent in child.GetAllInChildren<T>())
            yield return childComponent;

        if (GetType() == typeof(T)) yield return this as T ?? throw new SystemException("Oof");
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
        if (!children.TryAdd(gameObject.GetType(), gameObject)) throw new ArgumentException("Can't add component");
        gameObject.Parent = this;
        if(gameObject is IPostInitializedGameObject initialize) initialize.Initialize();
    }

    public bool TryAddComponent(GameObject gameObject)
    {
        bool success = children.TryAdd(gameObject.GetType(), gameObject);
        if(success && gameObject is IPostInitializedGameObject initialize) initialize.Initialize();
        return success;
    }

    public GameObject Parent { get; internal set; }

    public GameObject(params GameObject[] newChildren)
    {
        Parent = this;
        children = new Dictionary<Type, GameObject>();
        foreach (var child in newChildren)
        {
            if (!children.TryAdd(child.GetType(), child)) throw new ArgumentException("Can't add component");
            child.Parent = this;
        }
        foreach (var child in newChildren)
        {
            if(child is IPostInitializedGameObject initialize)
                initialize.Initialize();
        }
    }
}