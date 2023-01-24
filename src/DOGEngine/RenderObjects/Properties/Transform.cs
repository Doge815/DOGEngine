namespace DOGEngine.RenderObjects.Properties;

public class Transform : GameObject
{
    private bool modelUpdate = true;
    private Vector3 _position;
    private Vector3 _orientation;
    private Vector3 _scale;
    private Vector3 _orientationOffset;
    private Matrix4 _model;

    public Vector3 Position
    {
        get => _position;
        set { _position = value;
            modelUpdate = true;
        }
    }

    public Vector3 Orientation
    {
        get => _orientation;
        set { _orientation = value;
            modelUpdate = true;
        }
    }

    public Vector3 Scale
    {
        get => _scale;
        set { _scale = value;
            modelUpdate = true;
        }
    }

    public Vector3 OrientationOffset
    {
        get => _orientationOffset;
        set { _orientationOffset = value;
            modelUpdate = true;
        }
    }

    public Matrix4 Model
    {
        get
        {
            if (modelUpdate) _model = CreateModelMatrix();
            modelUpdate = false;
            return _model;
        }
        set
        {
            _model = value;
            modelUpdate = false;
            _position = _model.ExtractTranslation();
            _scale = _model.ExtractScale();
            _orientation = _model.ExtractRotation().ToEulerAngles() * 180f/MathF.PI;
            _orientationOffset = Vector3.Zero;
        }
    }

    public Transform(Vector3? position = null, Vector3? orientation = null, Vector3? scale = null,
        Vector3? orientationOffset = null) 
    {
        Position = position ?? Vector3.Zero;
        Orientation = orientation ?? Vector3.Zero;
        Scale = scale ?? Vector3.One;
        OrientationOffset = orientationOffset ?? Vector3.Zero;
    }

    private  Matrix4 CreateModelMatrix() => CreateModelMatrix(Scale, OrientationOffset, Orientation, Position);
    public static Matrix4 CreateModelMatrix(Vector3 Scale, Vector3 OrientationOffset, Vector3 Orientation, Vector3 Position) =>
        Matrix4.CreateScale(Scale)
        * Matrix4.CreateTranslation(OrientationOffset)
        * Matrix4.CreateFromQuaternion(new Quaternion(Orientation * MathF.PI/180f))
        * Matrix4.CreateTranslation(-OrientationOffset)
        * Matrix4.CreateTranslation(Position);
}