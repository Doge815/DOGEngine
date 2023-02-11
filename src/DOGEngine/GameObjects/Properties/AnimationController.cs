namespace DOGEngine.GameObjects.Properties;

public struct AnimationState
{
    public double Time { get; }
    public  TransformData Transform { get; }

    public AnimationState(double time, TransformData transform)
    {
        Time = time;
        Transform = transform;
    }
}
public class Animation
{
    private readonly AnimationState[] states;

    public Animation(params AnimationState[] animationStates)
    {
        states = animationStates.OrderBy(x => x.Time).ToArray();
    }

    public Matrix4 GetCurrent(double time)
    {
        if (states.Length == 0) return Matrix4.Identity;
        TransformData start = TransformData.Default;
        TransformData end = TransformData.Default;
        float progress = 0;

        if (time < states[0].Time)
        {
            end = states[0].Transform;
            progress = (float)(time / states[0].Time);
        }
        
        for (int i = 1; i < states.Length; i++)
        {
            if (states[i-1].Time < time && time <= states[i].Time)
            {
                start = states[i - 1].Transform;
                end = states[i].Transform;
                progress = (float)((time-states[i-1].Time)/(states[i].Time- states[i-1].Time));
                break;
            }
        }

        if (time > states.Last().Time) return Matrix4.Identity;

        return TransformData.CreateModelMatrix(
            Vector3.Lerp(start.Scale, end.Scale, progress),
            Vector3.Lerp(start.OrientationOffset, end.OrientationOffset, progress),
            Vector3.Lerp(start.Orientation, end.Orientation, progress),
            Vector3.Lerp(start.Position, end.Position, progress),
            Matrix4.Identity
        );
    }

    public double PlayTime => states.Length == 0 ? 0 : states.Last().Time;
}
public class AnimationController : GameObject, IPostInitializedGameObject
{
    private Transform transform;
    public Animation? Current { get; private set; }
    private Animation? last;
    private double time;
    public bool Running => Current is not null;

    public AnimationController(Animation? animation = null)
    {
        Current = null;
        last = animation;
        transform = null!;
    }

    public void Play(Animation? animation = null)
    {
        last = animation ?? last;
        Current = last;
        time = 0;
    }

    internal void UpdateTransform(double deltaTime)
    {
        if (Current is not null)
        {
            time += deltaTime;
            if (time > Current.PlayTime)
                Current = null;
            else
                transform.Animation = Current.GetCurrent(time);
        }
    } 

    public bool NotInitialized { get; set; } = true;
    public void InitFunc()
    {
        if (Parent.TryGetComponent(out Transform? tf))
            transform = tf!;
    }
}