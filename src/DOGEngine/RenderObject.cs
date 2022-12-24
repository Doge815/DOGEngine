using OpenTK.Graphics.ES20;
using OpenTK.Mathematics;

namespace DOGEngine;

public abstract class RenderObject
{
    public Vector3 Position;
    public Vector3 Rotation;

    protected RenderObject(Vector3? position, Vector3? rotation)
    {
        Shader = null!;
        Position = position ?? Vector3.Zero;
        Rotation = rotation ?? Vector3.Zero;
    }

    public virtual Shader Shader { get; }

    public abstract void OnLoad();
    public abstract void Draw(Matrix4 view, Matrix4 projection);

    protected void interpretVertexData(string attribute, int size, VertexAttribPointerType type, int stride, int offset)
    {
        int index = Shader!.GetAttributeLocation(attribute);
        if (index != -1)
        {
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(index, size, VertexAttribPointerType.Float, false, stride, offset);
        }
    }

    protected void interpretVertexDataFloat(ShaderAttribute attribute, int stride)
    {
        interpretVertexData(attribute.AttributeName, attribute.Size, VertexAttribPointerType.Float,
            stride * sizeof(float),
            attribute.Offset * sizeof(float));
    }
}