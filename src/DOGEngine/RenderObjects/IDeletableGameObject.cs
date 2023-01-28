namespace DOGEngine.RenderObjects;

public interface IDeletableGameObject
{
    protected bool NotDeleted { get; set; }

    internal sealed void Delete()
    {
        if (NotDeleted) DeleteFunc();
        NotDeleted = false;
    }

    protected void DeleteFunc();
}