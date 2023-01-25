namespace DOGEngine.RenderObjects.Properties;

internal struct TransformData
{
    public static TransformData Default { get; } = new (null);
    private bool modelUpdate;
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

    internal void SetModel(Matrix4 model, bool scale, bool orientation, bool position)
    {
        if(scale)
            _scale = model.ExtractScale();
        if(orientation)
            _orientation = model.ExtractRotation().ToEulerAngles() * 180f/MathF.PI;
        if(position)
            _position = model.ExtractTranslation();
        
        _model = CreateModelMatrix();
        modelUpdate = false;
        
    }
    
    public Matrix4 CreateModelMatrix() => CreateModelMatrix(Scale, OrientationOffset, Orientation, Position);

    public Matrix4 CreateSelectedModelMatrix(bool scale, bool orientationOffset, bool orientation, bool position) =>
        CreateModelMatrix(
            scale ? Scale : Vector3.One,
            orientationOffset ? OrientationOffset : Vector3.Zero,
            orientation ? Orientation : Vector3.Zero,
            position ? Position : Vector3.Zero
        );
    public static Matrix4 CreateModelMatrix(Vector3 Scale, Vector3 OrientationOffset, Vector3 Orientation, Vector3 Position) =>
        Matrix4.CreateScale(Scale)
        * Matrix4.CreateTranslation(OrientationOffset)
        * Matrix4.CreateFromQuaternion(new Quaternion(Orientation * MathF.PI/180f))
        * Matrix4.CreateTranslation(-OrientationOffset)
        * Matrix4.CreateTranslation(Position);

    public TransformData(Vector3? position = null, Vector3? orientation = null, Vector3? scale = null,
        Vector3? orientationOffset = null)
    {
        Position = position ?? Vector3.Zero;
        Orientation = orientation ?? Vector3.Zero;
        Scale = scale ?? Vector3.One;
        OrientationOffset = orientationOffset ?? Vector3.Zero;
        _model = CreateModelMatrix();   
        modelUpdate = false;
    }
}
public class Transform : GameObject
{

    internal TransformData TransformData;
    public Vector3 Position
    {
        get => TransformData.Position;
        set => TransformData.Position = value;
    }
    
    public Vector3  Orientation
    {
        get => TransformData.Orientation;
        set => TransformData.Orientation = value;
    }
    
    public Vector3  Scale
    {
        get => TransformData.Scale;
        set => TransformData.Scale = value;
    }
    
    public Vector3  OrientationOffset
    {
        get => TransformData.OrientationOffset;
        set => TransformData.OrientationOffset = value;
    }
    
    public Matrix4 Model
    {
        get => TransformData.Model;
        set => TransformData.Model = value;
    }
    
    public Transform(Vector3? position = null, Vector3? orientation = null, Vector3? scale = null,
        Vector3? orientationOffset = null)
    {
        TransformData = new TransformData(position, orientation, scale, orientationOffset);
    }

}