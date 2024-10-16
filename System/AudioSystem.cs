using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using PandaGameLibrary.System;
using System.Diagnostics;

namespace PandaGameLibrary.Audio;

public class AudioSystem
{
    public Dictionary<string, SoundEffect> soundEffects;
    public Dictionary<string, Song> songs;

    internal List<AudioZone> audioZones { get; private set; }
    public float AudioVolume { get; set; }
    public float MusicVolume { get; set; }

    public Song previousSong;
    private Song currentSong;
    private float targetVolume = 1f;
    private float fadeSpeed = 2005.5f; // Adjust the speed of fade in/out
    private bool isFadingIn = false;
    private bool isFadingOut = false;
    private bool IsFadingRunning = false;
    private bool WasPlayerInside = false;
    private string songName;

    public void RemoveAudioZone(AudioZone audioZone)
    {
        audioZones.Remove(audioZone);
    }

    //public void PlaySong(string name)
    //{
    //    //targetVolume = 1f;
    //    if (songs.ContainsKey(name))
    //    {
    //        PlaySong(name);
    //        //isFadingIn = true;
    //        //Console.WriteLine("fade in called");
    //        WasPlayerInside = true;
    //    }
    //}
    private object lockObject = new object();

    public void StopCurrentSongSavePrevious()
    {
        if (MediaPlayer.Queue.ActiveSong != null)
        {
            previousSong = MediaPlayer.Queue.ActiveSong;
            MediaPlayer.Stop();
            //PlaySong(song);
            //isFadingIn = true;
            //Console.WriteLine("fade in called");
            //WasPlayerInside = true;
        }

        //MediaPlayer.Stop();
        //if (WasPlayerInside && previousSong != null) MediaPlayer.Play(previousSong);
        //previousSong = MediaPlayer.Queue.ActiveSong;
        ////isFadingOut = true;
        ////Console.WriteLine("fade out called");
        //WasPlayerInside = false;
    }

    //private void UpdateFade(GameTime gameTime)
    //{
    //    if (isFadingOut )
    //    {
    //        MediaPlayer.Volume -= MathHelper.Lerp(MediaPlayer.Volume, 0, 1);

    //        if (MediaPlayer.Volume <= 0)
    //        {
    //            StopSong();
    //            MediaPlayer.Volume = 0f;
    //            isFadingOut = false;

    //        }
    //    }
    //    //if (isFadingIn )
    //    //{
    //    //    MediaPlayer.Volume += 0.6f * (float)gameTime.ElapsedGameTime.TotalSeconds;
    //    //    Console.WriteLine(MediaPlayer.Volume);
    //    //    if (MediaPlayer.Volume >= targetVolume)
    //    //    {
    //    //        MediaPlayer.Volume = targetVolume;
    //    //        isFadingIn = false;

    //    //    }
    //    //}

    //}


    public AudioSystem()
    {
        soundEffects = new Dictionary<string, SoundEffect>();
        songs = new Dictionary<string, Song>();

        audioZones = new List<AudioZone>();

    }
    // Add an audio zone
    internal void AddAudioZoneSong(AudioZone zone, string path, float radius, string songName)
    {
        audioZones.Add(zone);
        //zone.InitializeSongAudioZone(path, radius, songName);

        LoadSong(songName, path);
    }

    internal void AddAudioZoneSoundEffect(AudioZone zone, string path, string SoundEffectName)
    {
        audioZones.Add(zone);
        //zone.InitializeSoundEffectAudioZone(path, radius, SoundEffectName);

        LoadSoundEffect(SoundEffectName, path);
    }

    //Load a sound effect from file
    public void LoadSoundEffect(string name, string filePath)
    {
        SoundEffect soundEffect = PandaCore.Instance.Game.Content.Load<SoundEffect>(filePath);
        soundEffects[name] = soundEffect;
    }

    // Load a song from file
    public void LoadSong(string name, string filePath)
    {
        Song song = PandaCore.Instance.Game.Content.Load<Song>(filePath);
        songs[name] = song;
        MediaPlayer.Volume = MusicVolume;

    }

    // Play a sound effect
    public void PlaySoundEffect(string name)
    {
        if (soundEffects.ContainsKey(name))
        {
            soundEffects[name].Play(AudioVolume, 0.0f, 0.0f);
        }
    }

    // Play a song
    public void PlaySong(string name, bool loop = true)
    {
        if (name != null && songs.ContainsKey(name))
        {
            songName = name;

            try
            {
                MediaPlayer.Stop();
                MediaPlayer.Play(songs[name]);
                MediaPlayer.IsRepeating = loop;
            }
            catch (Exception ex)
            {
                // Log the details
                Debug.WriteLine($"Error playing song '{name}': {ex.Message}");
                // You can also handle this error gracefully, like notifying the player or attempting a recovery.
            }
        }
    }


    // Pause the currently playing song
    public void PauseSong()
    {
        if (MediaPlayer.State == MediaState.Playing)
        {
            MediaPlayer.Pause();
        }
    }

    // Resume the paused song
    public void ResumeSong()
    {
        if (MediaPlayer.State == MediaState.Paused)
        {
            MediaPlayer.Resume();
        }
    }

    // Stop the currently playing song
    public void StopSong()
    {
        MediaPlayer.Stop();
    }

    //// Set the volume for sound effects
    public void SetSoundEffectVolume(float volume)
    {
        AudioVolume = MathHelper.Clamp(volume, 0.0f, 1.0f);
        AudioVolume = AudioVolume;
    }

    // Set the volume for music
    public void SetMusicVolume(float volume)
    {
        MusicVolume = MathHelper.Clamp(volume, 0.0f, 1.0f);
        MediaPlayer.Volume = MusicVolume;
    }

    // Get the volume for sound effects
    public float GetSoundEffectVolume()
    {
        return AudioVolume;
    }

    // Get the volume for music
    public float GetMusicVolume()
    {
        return MusicVolume;
    }

    public void Update(GameTime gameTime)
    {
        //UpdateFade(gameTime);


        foreach (var audioZone in audioZones)
        {
            //// this for non player auido zones
            //if (audioZone.IsPlayerInSide && audioZone.IsSong)
            //{
            //    if (!audioZone.IsPlaying)
            //    {
            //        if (!string.IsNullOrEmpty(audioZone.SongName) && songs.ContainsKey(audioZone.SongName))
            //        {
            //            PlaySong(audioZone.SongName);
            //        }
            //        audioZone.IsPlaying = true;
            //    }
            //    if (audioZone.IsPlaying && audioZone.IsDynamicAudio)
            //    {
            //        MediaPlayer.Volume = MathHelper.Clamp(audioZone.GetVolumeBasedOnDistance, 0, Core.Instance.AudioSystem.MusicVolume );

            //    }
            //}

            // this for player auido zones
            if (audioZone.Islocal)
            {
                foreach (var otherZone in audioZones)
                {

                    if (otherZone.IsPlayerInSide && otherZone.IsSong)
                    {
                        if (!otherZone.IsPlaying)
                        {
                            if (!string.IsNullOrEmpty(otherZone.SongName) && songs.ContainsKey(otherZone.SongName))
                            {
                                PlaySong(otherZone.SongName);
                            }
                            otherZone.IsPlaying = true;
                        }
                        if (otherZone.IsPlaying && otherZone.IsDynamicAudio)
                        {
                            MediaPlayer.Volume = MathHelper.Clamp(otherZone.GetVolumeBasedOnDistance, 0, PandaCore.Instance.AudioSystem.MusicVolume);

                        }
                    }


                    if (otherZone != null && otherZone != audioZone)
                    {
                        otherZone.ZoneAudioVolume = otherZone.VolumeBasedOnDistance(audioZone.Collider.Center);
                      
                    }
                }
            }
        }
    }


}
