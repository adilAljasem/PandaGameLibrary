using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using PandaGameLibrary.Components;
using PandaGameLibrary.System;

namespace PandaGameLibrary.Audio;

public class AudioZone : Component
{
    public float Radius { get; set; } // For circular zones
    public string SoundEffectName { get; private set; }
    public string SongName { get; private set; }
    public bool IsPlaying { get; set; }
    public ColliderComponent Collider { get; set; }
    public bool IsSong { get; private set; }
    public bool IsDynamicAudio { get; private set; }
    public float GetVolumeBasedOnDistance { get; set; }
    public bool IsPlayerInSide { get; set; }

    //public Dictionary<string, SoundEffect> soundEffects;
    //private Song Song { get; set; }
    public SoundEffectInstance currentPlayingSoundEffect;

    public bool Islocal { get; set; }

    public float ZoneAudioVolume { get; set; }
    //public float ZoneMusicVolume { get; set; }

    //public void PlaySong(string name, bool loop = true)
    //{
    //    if (Song != null)
    //    {
    //        MediaPlayer.Play(Song);
    //        MediaPlayer.IsRepeating = loop;

    //    }
    //}
    //public void SetMusicVolume(float volume)
    //{
    //    ZoneMusicVolume = MathHelper.Clamp(volume, 0.0f, 1.0f);
    //    MediaPlayer.Volume = ZoneMusicVolume;
    //}
    public void SetSoundEffectVolume(float volume)
    {
        ZoneAudioVolume = MathHelper.Clamp(volume, 0.0f, 1.0f);
        ZoneAudioVolume = ZoneAudioVolume * 100;
    }
    public void PlaySoundEffect(string name)
    {
        if (name != null && PandaCore.Instance.AudioSystem.soundEffects.ContainsKey(name))
        {
            float volume = !Islocal ?
                MathHelper.Clamp(ZoneAudioVolume, 0, PandaCore.Instance.AudioSystem.AudioVolume) :
                MathHelper.Clamp(1, 0, PandaCore.Instance.AudioSystem.AudioVolume);

            // Create the instance first
            currentPlayingSoundEffect = PandaCore.Instance.AudioSystem.soundEffects[name].CreateInstance();

            // Set the volume and play
            currentPlayingSoundEffect.Volume = volume;
            currentPlayingSoundEffect.Play();
        }
    }
    public void StopCurrentSoundEffect(string name)
    {
        if (name != null && PandaCore.Instance.AudioSystem.soundEffects.ContainsKey(name))
        {
            currentPlayingSoundEffect?.Stop();
        }
    }
    //internal void LoadSong(string name, string filePath)
    //{
    //    Song = Core.Instance.contentManager.Load<Song>(filePath);
    //    MediaPlayer.Volume = MusicVolume / 100f;

    //}
    // Load a sound effect from file
    //internal void LoadSoundEffect(string name, string filePath)
    //{
    //    SoundEffect soundEffect = Core.Instance.contentManager.Load<SoundEffect>(filePath);
    //    soundEffects[name] = soundEffect;
    //}
    public void InitializeSongAudioZone(string path, float radius, string songName, bool IsDynamic)
    {
        this.IsDynamicAudio = IsDynamic;
        Radius = radius;
        Collider.Radius = radius;
        Collider.Center = gameObject.Transform.Position;
        SongName = songName;
        IsPlaying = false;
        IsSong = true;

        //LoadSong(songName, path);
        PandaCore.Instance.AudioSystem.AddAudioZoneSong(this, path, radius, songName);

        Collider.OnEnterCollision += OnEnter;
        Collider.OnExitCollision += OnExit;
        Collider.OnCollision += OnCollide;
    }

    public void InitializeSoundEffectAudioZone(string path, float radius, string soundEffect, bool IsDynamic)
    {
        this.IsDynamicAudio = IsDynamic;


        Radius = radius;
        Collider.Radius = radius;
        Collider.Center = gameObject.Transform.Position;
        SoundEffectName = soundEffect;
        IsPlaying = false;
        IsSong = false;

        //LoadSoundEffect(soundEffect, path);
        PandaCore.Instance.AudioSystem.AddAudioZoneSoundEffect(this, path, soundEffect);
        Console.WriteLine(PandaCore.Instance.AudioSystem.audioZones.Count);
        Collider.OnEnterCollision += OnEnter;
        Collider.OnExitCollision += OnExit;
        Collider.OnCollision += OnCollide;

    }

    //public void PlaySoundEffect(string name)
    //{
    //    Core.Instance.AudioSystem.PlaySoundEffect(name);
    //}


    public override void Awake()
    {
        //soundEffects = new Dictionary<string, SoundEffect>();
        Collider = gameObject.AddComponent<ColliderComponent>();
        Collider.Transparent = true;
        //PandaCore.Instance.AudioSystem.audioZones.Add(this);
    }

    public override void Update(GameTime gameTime)
    {
        Collider.Center = gameObject.Transform.Position;
        if (currentPlayingSoundEffect != null && currentPlayingSoundEffect.State == SoundState.Playing)
        {
            float volume = !Islocal ?
    MathHelper.Clamp(ZoneAudioVolume, 0, PandaCore.Instance.AudioSystem.AudioVolume) :
    MathHelper.Clamp(1, 0, PandaCore.Instance.AudioSystem.AudioVolume);

            currentPlayingSoundEffect.Volume = MathHelper.Clamp(volume, 0, PandaCore.Instance.AudioSystem.AudioVolume);
        }
        //if (currentPlayingSoundEffect != null)
        //{
        //    currentPlayingSoundEffect.Volume  = ZoneAudioVolume;
        //    Console.WriteLine(currentPlayingSoundEffect.Volume);
        //}



        //foreach (var audiozone in Core.Instance.AudioSystem.audioZones)
        //{
        //    if (audiozone != null && audiozone.Owner == Owner)
        //    {
        //        continue;
        //    }
        //    if (audiozone != null && Islocal)
        //    {
        //        audiozone.AudioVolume = VolumeBasedOnDistance(audiozone.Collider.Center);
        //    }
        //}

    }

    public void OnCollide(GameObject other)
    {

        //if (other != null && gameObject.Tag == "player")
        //{
        //    var otherAudioZone = other.GetComponent<AudioZone>();

        //    if (Islocal && otherAudioZone != null && otherAudioZone.IsDynamicAudio)
        //    {
        //        GetVolumeBasedOnDistance = VolumeBasedOnDistance(other.Transform.Position);

        //    }
        //}

    }

    public void OnEnter(GameObject other)
    {
        // this method is from non player auido zone to player auido zone
        //if (other != null && gameObject.Tag == "player")
        //{
        //    var OtherAudioZone = other.GetComponent<AudioZone>();
        //    if (OtherAudioZone != null && Islocal)
        //    {
        //        OtherAudioZone.IsPlayerInSide = true;

        //        if (OtherAudioZone.IsDynamicAudio)
        //        {
        //            GetVolumeBasedOnDistance = VolumeBasedOnDistance(other.Transform.Position);

        //            Core.Instance.AudioSystem.FadeOutCurrentSong();
        //            Core.Instance.AudioSystem.FadeInSong(SongName);
        //        }
        //        else if (OtherAudioZone.IsSong)
        //        {
        //            Core.Instance.AudioSystem.FadeOutCurrentSong();
        //            Core.Instance.AudioSystem.FadeInSong(SongName);
        //            IsPlaying = true;
        //        }
        //    }

        //}

    }

    public void OnExit(GameObject other)
    {
        // this method is from non player auido zone to player auido zone

        //if (other != null && gameObject.Tag == "player")
        //{
        //    var OtherAudioZone = other.GetComponent<AudioZone>();
        //    if (OtherAudioZone != null && Islocal)
        //    {

        //        IsPlayerInSide = false;

        //        if (OtherAudioZone.IsDynamicAudio)
        //        {
        //            GetVolumeBasedOnDistance = VolumeBasedOnDistance(other.Transform.Position);

        //            Core.Instance.AudioSystem.FadeOutCurrentSong();
        //            MediaPlayer.Volume = Core.Instance.AudioSystem.MusicVolume;


        //        }
        //        else if (OtherAudioZone.IsPlaying && OtherAudioZone.IsSong)
        //        {
        //            Core.Instance.AudioSystem.FadeOutCurrentSong();
        //            OtherAudioZone.IsPlaying = false;
        //        }
        //    }

        //}
    }


    public float VolumeBasedOnDistance(Vector2 playerPosition)
    {
        float distance = Vector2.Distance(Collider.Center, playerPosition);
        float volume = 1.0f - MathHelper.Clamp(distance / Radius, 0.0f, 1.0f);
        return volume;
    }
}
