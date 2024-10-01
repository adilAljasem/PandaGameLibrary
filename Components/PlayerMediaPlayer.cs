using Microsoft.Xna.Framework.Media;

public class PlayerMediaPlayer
{
    private Song currentSong;
    private bool isRepeating;
    private float volume;
    private MediaState state;

    public MediaState State => state;

    public PlayerMediaPlayer()
    {
        volume = 1.0f;
        state = MediaState.Stopped;
    }

    public void Play(Song song, bool loop = true)
    {
        currentSong = song;
        isRepeating = loop;
        MediaPlayer.Play(song);
        MediaPlayer.IsRepeating = loop;
        MediaPlayer.Volume = volume;
        state = MediaState.Playing;
    }

    public void Pause()
    {
        if (state == MediaState.Playing)
        {
            MediaPlayer.Pause();
            state = MediaState.Paused;
        }
    }

    public void Resume()
    {
        if (state == MediaState.Paused)
        {
            MediaPlayer.Resume();
            state = MediaState.Playing;
        }
    }

    public void Stop()
    {
        MediaPlayer.Stop();
        state = MediaState.Stopped;
    }

    public void SetVolume(float volume)
    {
        this.volume = volume;
        MediaPlayer.Volume = volume;
    }
}
