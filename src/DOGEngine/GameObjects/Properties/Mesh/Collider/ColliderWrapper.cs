using BulletSharp;

namespace DOGEngine.GameObjects.Properties.Mesh.Collider;

public interface IColliderWrapper
{
    public Matrix4? Transform { get; }
    public CollisionShape GetShape(Vector3 scale);

    public static CollisionShape Combine(Vector3 scale, params IColliderWrapper[] collider)
    {
        if (collider is [{ Transform: null }])
        {
            var col = collider[0];
            if (col.Transform is null) return col.GetShape(scale);
            else
            {
                var shape = new CompoundShape(true, 1);
                shape.AddChildShape(col.Transform!.Value.Convert(), col.GetShape(scale));
                return shape;
            }
        }
        else
        {
            var shape = new CompoundShape(true, collider.Length);
            foreach (var col in collider)
                shape.AddChildShape((col.Transform ?? Matrix4.Identity).Convert(), col.GetShape(scale));
            return shape;
        }
    }
}

public class CubeCollider : IColliderWrapper
{
    public Matrix4? Transform { get; }
    public CubeCollider(Vector3? position = null, Vector3? orientation = null)
    {
        if (position is not null || orientation is not null)
        {
            Transform = Matrix4.Identity;
            if (position is not null)
                Transform *= Matrix4.CreateTranslation(position.Value);
            if (orientation is not null)
                Transform *= Matrix4.CreateFromQuaternion(new Quaternion(orientation.Value * MathF.PI / 180f));
        }
        else
            Transform = null;
    }

    public CollisionShape GetShape(Vector3 scale) {
        return new BoxShape((scale * 0.5f).Convert());
    }
}

public class CapsuleCollider : IColliderWrapper
{
    public Matrix4? Transform { get; }
    private readonly float _radius;
    private readonly float _height;
    public CapsuleCollider(float radius, float height, Vector3? position = null, Vector3? orientation = null)
    {
        _radius = radius;
        _height = height;
        if (position is not null || orientation is not null)
        {
            Transform = Matrix4.Identity;
            if (position is not null)
                Transform *= Matrix4.CreateTranslation(position.Value);
            if (orientation is not null)
                Transform *= Matrix4.CreateFromQuaternion(new Quaternion(orientation.Value * MathF.PI / 180f));
        }
        else
            Transform = null;
    }

    public CollisionShape GetShape(Vector3 scale) {
        return new CapsuleShape(_radius, _height);
    }
}

public class MeshCollider : IColliderWrapper
{
    public Matrix4? Transform { get; }
    private readonly float[] vertices;
    public MeshCollider(float[] vertexData, Vector3? position = null, Vector3? orientation = null)
    {
        vertices = vertexData;
        if (position is not null || orientation is not null)
        {
            Transform = Matrix4.Identity;
            if (position is not null)
                Transform *= Matrix4.CreateTranslation(position.Value);
            if (orientation is not null)
                Transform *= Matrix4.CreateFromQuaternion(new Quaternion(orientation.Value * MathF.PI / 180f));
        }
        else
            Transform = null;
    }
    public CollisionShape GetShape(Vector3 scale) {
        var mesh = new BulletSharp.TriangleMesh();
        for (int i = 0; i < vertices.Length; i+=9)
        {
            mesh.AddTriangle(
                new BulletSharp.Math.Vector3(vertices[i+0],vertices[i+1],vertices[i+2]) * scale.Convert(),
                new BulletSharp.Math.Vector3(vertices[i+3],vertices[i+4],vertices[i+5]) * scale.Convert(),
                new BulletSharp.Math.Vector3(vertices[i+6],vertices[i+7],vertices[i+8]) * scale.Convert()
            );
        }

        return new ConvexTriangleMeshShape(mesh);
    }
}