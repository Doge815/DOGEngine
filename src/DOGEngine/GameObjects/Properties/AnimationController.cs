namespace DOGEngine.GameObjects.Properties;

public struct AnimationState
{
    public double Time { get; }
    public Matrix4 State { get; }
}
public class Animation
{
    private AnimationState[] states;

    public Animation(params AnimationState[] animationStates)
    {
        states = animationStates.OrderBy(x => x.Time).ToArray();
    }

    public Matrix4 GetCurrent(double time)
    {
        if (states.Length == 0) return Matrix4.Identity;
        Matrix4 start = Matrix4.Identity;
        Matrix4 last = Matrix4.Identity;
        double diff = 1;
        double delta = 0;

        if (time < states[0].Time)
        {
            last = states[0].State;
            diff = states[0].Time;
            delta = time;
        }
        
        for (int i = 1; i < states.Length; i++)
        {
            if (states[i-1].Time < time && time < states[i].Time)
            {
                
            }
        }
        set:
    }
}
public class AnimationController : GameObject
{
    private Animation? current;
}