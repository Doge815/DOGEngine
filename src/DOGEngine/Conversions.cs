namespace DOGEngine;

public static class Conversions
{
    public static BulletSharp.Math.Matrix Convert(this Matrix4 matrix) =>
        Convert<Matrix4, BulletSharp.Math.Matrix>(matrix);

    public static Matrix4 Convert(this BulletSharp.Math.Matrix matrix) =>
        Convert<BulletSharp.Math.Matrix, Matrix4>(matrix);

    public static Vector3 Convert(this BulletSharp.Math.Vector3 vector) =>
        Convert<BulletSharp.Math.Vector3, Vector3>(vector);
    
    public static BulletSharp.Math.Vector3 Convert(this Vector3 vector) =>
        Convert<Vector3, BulletSharp.Math.Vector3>(vector);
    
    private static Tout Convert<Tin, Tout>(this Tin obj)
    {
        unsafe
        {
            return *(Tout*) &obj;
        }
    }
}