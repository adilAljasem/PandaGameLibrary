using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace PandaGameLibrary.Audio;
public class AudioSystem
{
    public Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
    public Dictionary<string, Song> songs = new Dictionary<string, Song>();

    public float AudioVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 1.0f;

    private Song currentSong;
    private ContentManager content;

    public void Initialize(ContentManager contentManager)
    {
        content = contentManager;
    }
    public void LoadSoundEffect(string name, string path)
    {
        if (!soundEffects.ContainsKey(name))
        {
            soundEffects[name] = content.Load<SoundEffect>(path);
        }
    }

    public void LoadSong(string name, string path)
    {
        if (!songs.ContainsKey(name))
        {
            songs[name] = content.Load<Song>(path);
        }
    }

    public void PlaySoundEffect(string name, bool isGlobal = true, float volume = 1.0f)
    {
        if (soundEffects.TryGetValue(name, out SoundEffect effect))
        {
            effect.Play(volume * AudioVolume, 0.0f, 0.0f);
        }
    }

    public void PlaySong(string name, bool isGlobal = true, bool repeat = true)
    {
        if (songs.TryGetValue(name, out Song song))
        {
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = repeat;
            MediaPlayer.Volume = MusicVolume;
            currentSong = song;
        }
    }

    public void StopSong()
    {
        MediaPlayer.Stop();
    }

    public void SetAudioVolume(float volume)
    {
        AudioVolume = MathHelper.Clamp(volume, 0.0f, 1.0f);
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = MathHelper.Clamp(volume, 0.0f, 1.0f);
        MediaPlayer.Volume = MusicVolume;
    }

    // Methods for fading in/out songs
    public void FadeInSong(string name, float duration = 1.0f)
    {
        // Implement fade-in logic
    }

    public void FadeOutCurrentSong(float duration = 1.0f)
    {
        // Implement fade-out logic
    }

    public void Update(GameTime gameTime)
    {

    }

}



