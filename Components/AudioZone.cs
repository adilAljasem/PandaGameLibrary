using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using PandaGameLibrary.Components;
using PandaGameLibrary.System;

namespace PandaGameLibrary.Audio;
/// <summary>
/// AudioZone Is a Component that makes sound play in specific areas within the game
/// </summary>
public class AudioZone : Component
{
    private float Radius { get; set; }
    private Vector2 _playerPosition;
    public string SongName { get; private set; }
    public bool IsPlaying { get; set; }
    public ColliderComponent Collider { get; private set; }
    public bool IsDynamicAudio { get; private set; }
    public float ZoneAudioVolume { get; set; } = 1.0f;
    public float ZoneMusicVolume { get; set; } = 1.0f;
    public bool IsLocal { get; set; }

    private Dictionary<string, List<SoundEffectInstance>> currentPlayingSoundEffects = new Dictionary<string, List<SoundEffectInstance>>();
    private List<string> soundEffectNames = new List<string>();

    public override void Awake()
    {
        Collider = gameObject.AddComponent<ColliderComponent>();
        Collider.Transparent = true;
        Collider.Color = Color.Blue;
    }

    public override void Update(GameTime gameTime)
    {
        Collider.Center = gameObject.Transform.Position;
        UpdateAudioVolume();
        CleanUpFinishedSoundEffects();
    }

    public void InitializeAudioZone(string path, float radius, string audioName, bool isSong, bool isDynamic)
    {
        Radius = radius;
        Collider.Radius = radius;
        IsDynamicAudio = isDynamic;

        if (isSong)
        {
            SongName = audioName;
            PandaCore.Instance.AudioSystem.songs[audioName] = PandaCore.Instance.Game.Content.Load<Song>(path);
        }
        else
        {
            AddSoundEffect(path, audioName);
        }

        Collider.OnEnterCollision += OnEnter;
        Collider.OnExitCollision += OnExit;
        Collider.OnCollision += OnCollision;
    }

    public void AddSoundEffect(string path, string soundEffectName)
    {
        if (!soundEffectNames.Contains(soundEffectName))
        {
            soundEffectNames.Add(soundEffectName);
            PandaCore.Instance.AudioSystem.soundEffects[soundEffectName] = PandaCore.Instance.Game.Content.Load<SoundEffect>(path);
        }
    }

    public void PlayAudio()
    {
        if (!string.IsNullOrEmpty(SongName))
        {
            PandaCore.Instance.AudioSystem.PlaySong(SongName, false);
        }

        foreach (var soundEffectName in soundEffectNames)
        {
            PlaySoundEffect(soundEffectName);
        }

        IsPlaying = true;
    }

    public void PlaySoundEffect(string soundEffectName)
    {
        if (string.IsNullOrEmpty(soundEffectName) || !PandaCore.Instance.AudioSystem.soundEffects.TryGetValue(soundEffectName, out SoundEffect soundEffect))
        {
            return;
        }

        var instance = soundEffect.CreateInstance();
        instance.IsLooped = false;
        instance.Volume = IsLocal ? PandaCore.Instance.AudioSystem.AudioVolume : ZoneAudioVolume * PandaCore.Instance.AudioSystem.AudioVolume;
        instance.Play();

        if (!currentPlayingSoundEffects.TryGetValue(soundEffectName, out List<SoundEffectInstance> instances))
        {
            instances = new List<SoundEffectInstance>();
            currentPlayingSoundEffects[soundEffectName] = instances;
        }
        instances.Add(instance);
    }

    public void StopAudio()
    {
        if (!string.IsNullOrEmpty(SongName))
        {
            PandaCore.Instance.AudioSystem.StopSong();
        }

        StopAllSoundEffects();
        IsPlaying = false;
    }

    public void StopAllSoundEffects()
    {
        foreach (var instances in currentPlayingSoundEffects.Values)
        {
            foreach (var instance in instances)
            {
                instance.Stop();
            }
            instances.Clear();
        }
    }

    public void StopSoundEffect(string soundEffectName)
    {
        if (currentPlayingSoundEffects.TryGetValue(soundEffectName, out List<SoundEffectInstance> instances))
        {
            foreach (var instance in instances)
            {
                instance.Stop();
            }
            instances.Clear();
        }
    }

    private void UpdateAudioVolume()
    {
        float volumeMultiplier = IsDynamicAudio ? CalculateVolumeBasedOnDistance() : 1f;
        float finalVolume = ZoneAudioVolume * volumeMultiplier;

        if (!string.IsNullOrEmpty(SongName))
        {
            ZoneMusicVolume = IsLocal ? PandaCore.Instance.AudioSystem.MusicVolume : finalVolume * PandaCore.Instance.AudioSystem.MusicVolume;
            MediaPlayer.Volume = ZoneMusicVolume;
        }

        foreach (var instances in currentPlayingSoundEffects.Values)
        {
            if (instances == null) continue;
            foreach (var instance in instances)
            {
                instance.Volume = IsLocal ? PandaCore.Instance.AudioSystem.AudioVolume : finalVolume * PandaCore.Instance.AudioSystem.AudioVolume;
            }
        }
    }

    private void CleanUpFinishedSoundEffects()
    {
        foreach (var kvp in currentPlayingSoundEffects)
        {
            kvp.Value.RemoveAll(instance => instance.State == SoundState.Stopped);
        }
    }

    private float CalculateVolumeBasedOnDistance()
    {
        Vector2 playerPosition = _playerPosition;
        float distance = Vector2.Distance(Collider.Center, playerPosition);
        return 1.0f - MathHelper.Clamp(distance / Radius, 0.0f, 1.0f);
    }

    private void OnEnter(GameObject other)
    {
        // Implementation for when an object enters the audio zone
    }

    private void OnCollision(GameObject other)
    {
        if (other.Tag == "player")
        {
            var AudioZonePlayerGameObject = other.Children.Find(w => w.Tag == "AudioZone Player");
            var playerAudioZone = AudioZonePlayerGameObject?.GetComponent<AudioZone>();
            if (playerAudioZone != null && playerAudioZone.IsLocal) _playerPosition = other.Transform.Position;
        }
    }

    private void OnExit(GameObject other)
    {
        // Implementation for when an object exits the audio zone
    }
}

