namespace DOGEngine.RenderObjects;

public interface IPostInitializedGameObject
{
    protected bool NotInitialized { get; set; }

    internal sealed void Initialize()
    {
        if (NotInitialized) InitFunc();
        NotInitialized = false;
    }

    protected void InitFunc();
}