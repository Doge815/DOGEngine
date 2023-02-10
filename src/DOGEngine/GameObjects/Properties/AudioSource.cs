using DOGEngine.GameObjects;
using DOGEngine.GameObjects.Properties;
using NetCoreAudio;

namespace DOGEngine.GameObjects.Properties;

public class AudioSource : GameObject
{
    private AudioFile? last;
    private  List<Player> currentPlayers;
    public IReadOnlyList<Player> Current => currentPlayers;

    public AudioSource(AudioFile? file = null)
    {
        currentPlayers = new List<Player>();
        last = file;
    }

    public void Play(AudioFile? file = null)
    {
        last = file ?? last;
        if (last is not null)
        {
            var player = new Player();
            player.Play(last);
            currentPlayers.Add(player);
        }
    }

    public void UpdateDirections(Vector3 position)
    {
        float distance = 0;
        if(Parent.TryGetComponent(out Transform? transform))
            distance = Vector3.Distance(position, transform!.Position);

        //float volume = MathF.Min(MathF.Max((100 - MathF.Pow(distance, 3) / 100), 0), 100);
        float volume = MathF.Min(MathF.Max((MathF.Pow(distance, -2)* 1000), 0), 100);

        currentPlayers = currentPlayers.Where(x => x.Playing).ToList();

        foreach (Player player in currentPlayers)
        {
            player.SetVolume((byte)volume);
        }
    }
}

public class AudioFile
{
    public string File { get; }
    public AudioFile(string file) => File = file;
    public static implicit operator String(AudioFile af) => af.File;
    public static implicit operator AudioFile(string f) => new AudioFile(f);

}