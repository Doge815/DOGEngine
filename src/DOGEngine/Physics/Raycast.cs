using DOGEngine.RenderObjects;
using DOGEngine.RenderObjects.Properties;
using DOGEngine.Shader;

namespace DOGEngine.Physics;

internal record struct Triangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Collider owner);

public static class Raycast
{
    public static GameObject? CastRay(this GameObject scene, Vector3 origin, Vector3 direction)
    {
        ReaderWriterLockSlim threadLock = new ReaderWriterLockSlim();
        float closest = float.MaxValue;
        GameObject? obj = null;

        void CheckTriangle(Triangle tri)
        {
            if (MoellerTrumbore(origin, direction, tri, out var intersection))
            {
                float distance = Vector3.Distance(intersection!.Value, origin);
                threadLock.EnterUpgradeableReadLock();
                if (distance < closest)
                {
                    threadLock.EnterWriteLock();
                    if (distance < closest)
                    {
                        closest = distance;
                        obj = tri.owner;
                    }
                    threadLock.ExitWriteLock();
                }
                threadLock.ExitUpgradeableReadLock();
            }

        }
        
        Parallel.ForEach(scene.GetAllTriangles(), CheckTriangle);
        
        return obj;
    }

    internal static IEnumerable<Triangle> GetAllTriangles(this GameObject scene, bool translate = true)
    {
        foreach (Collider collider in scene.GetAllInChildren<Collider>())
        {
            var model = Mesh.GetModel(collider.Parent);
            var vertexData = collider.ColliderVertexData;
            for (int i = 0; i < vertexData.Length; i += 9)
            {
                var vertex1 = new Vector4(vertexData[i + 0], vertexData[i + 1], vertexData[i + 2], 1);
                var vertex2 = new Vector4(vertexData[i + 3], vertexData[i + 4], vertexData[i + 5], 1);
                var vertex3 = new Vector4(vertexData[i + 6], vertexData[i + 7], vertexData[i + 8], 1);
                if (!translate) yield return new Triangle(vertex1.Xyz, vertex2.Xyz, vertex3.Xyz, collider);
                else
                {
                    yield return new Triangle((vertex1 * model).Xyz, (vertex2 * model).Xyz, (vertex3 * model).Xyz, collider);
                }
                
            }
        }
    }

    private static bool MoellerTrumbore(Vector3 origin, Vector3 direction, Triangle triangle, out Vector3? intersection)
    {
        intersection = null;
        const float epsilon = 0.000001f;
        var edge1 = triangle.vertex2 - triangle.vertex1;
        var edge2 = triangle.vertex3 - triangle.vertex1;
        var h = Vector3.Cross(direction, edge2);
        var a = Vector3.Dot(edge1, h);

        if (a > -epsilon && a < epsilon)
            return false;

        var f = 1 / a;
        var s = origin - triangle.vertex1;
        var u = f * Vector3.Dot(s, h);
        if (u < 0 || u > 1)
            return false;
        var q = Vector3.Cross(s, edge1);
        var v = f * Vector3.Dot(direction, q);
        if (v < 0 || u + v > 1)
            return false;
        float t = f * Vector3.Dot(edge2, q);
        if (t > epsilon)
        {
            intersection = origin + direction * t;
            return true;
        }

        return false;
    }
}