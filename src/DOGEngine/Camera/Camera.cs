namespace DOGEngine.Camera;

public class Camera
{
    public Vector3 Position { get; set; }
    public float Yaw { get; set; }
    private float pitch;
    public float Pitch
    {
        get => pitch;
        set => pitch = MathHelper.Clamp(value, -89f, 89f);
    }

    public int Width { get; set; }
    public int Height { get; set; }
    public float AspectRatio => (float)Width / Height;
    public float FovY { get; set; } = 90;
    private float pitchRad => MathHelper.DegreesToRadians(Pitch);
    private float yawRad => MathHelper.DegreesToRadians(Yaw);
    private float fovRad => MathHelper.DegreesToRadians(FovY);

    public Vector3 Front =>
        Vector3.Normalize( new Vector3(MathF.Cos(pitchRad) * MathF.Cos(yawRad), 
            MathF.Sin(pitchRad),
            MathF.Cos(pitchRad) * MathF.Sin(yawRad)));

    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
    public Vector3 Up => Vector3.Normalize(Vector3.Cross(Right, Front));

    public Matrix4 ViewMatrix
        => Matrix4.LookAt(Position, Position + Front, Up);

    public Matrix4 ProjectionMatrix
        => Matrix4.CreatePerspectiveFieldOfView(fovRad, AspectRatio, 0.01f, 100f);
}