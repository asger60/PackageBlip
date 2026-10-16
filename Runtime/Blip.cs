using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Blip", menuName = "FloppyClub/Blip", order = 1)]
public class Blip : ScriptableObject
{
    [SerializeField] private AudioClip[] audioClips;

    public AudioClip[] AudioClips
    {
        get => audioClips;
        set => audioClips = value;
    }

    public enum ClipSelectMode
    {
        Random,
        Sequential,
        All
    }

    [SerializeField] private ClipSelectMode clipSelectMode;
    public ClipSelectMode GetClipSelectMode() => clipSelectMode;

    private enum PlayMode
    {
        OneShot,
        Looping
    }

    [SerializeField] private PlayMode playMode;

    [Range(1, 10)] [SerializeField] private int numberOfPlayingInstances = 10;
    public int NumberOfPlayingInstances => numberOfPlayingInstances;

    [Serializable]
    public struct SoundPositionMode
    {
        public enum SoundPositionType
        {
            WorldSpace,
            ScreenSpace,
            None
        }

        public SoundPositionType soundPositionType;
        public float maxDistance;
        public float minDistance;

        public SoundPositionMode(SoundPositionType soundPositionMode, float maxDistance)
        {
            soundPositionType = soundPositionMode;
            this.maxDistance = maxDistance;
            minDistance = 10;
        }
    }


    [SerializeField] private SoundPositionMode soundPositionMode;


    [Serializable]
    public struct ControlledValue
    {
        [Range(0, 1f)] public float value;
        [SerializeField] private ControlSource control;
        
        private float _currentValue;

        public float GetValue(float externalValue)
        {
            if (control.ExternalControl == ControlSource.ExternalControlTypes.FollowGameParameter)
                return control.GetControlValue(externalValue);
            if (control.ExternalControl == ControlSource.ExternalControlTypes.RandomEachPlay)
                return _currentValue;

            return value;
        }


        public bool IsExternallyControlled => control.ExternalControl == ControlSource.ExternalControlTypes.FollowGameParameter;

        public ControlSource.ExternalControlTypes ExternalControl => control.ExternalControl;

        // True once a random value has been rolled by a play
        public bool HasCurrentValue { get; private set; }

        // The value currently in effect, used by the editor to show what the external control produces
        public float GetDisplayValue(float externalValue)
        {
            switch (control.ExternalControl)
            {
                case ControlSource.ExternalControlTypes.RandomEachPlay:
                    return _currentValue;
                case ControlSource.ExternalControlTypes.FollowGameParameter:
                    return control.GetControlValue(externalValue);
                default:
                    return value;
            }
        }

        public void Init(float initialValue)
        {
            value = initialValue;
            control.Init();
        }

        public void OnPlay(float externalValue)
        {
            _currentValue = control.GetControlValue(externalValue);
            HasCurrentValue = true;
        }
    }

    [Serializable]
    public struct ControlSource
    {
        public enum ExternalControlTypes
        {
            None,
            RandomEachPlay,
            FollowGameParameter
        }

        [FormerlySerializedAs("type")] [FormerlySerializedAs("sourceType")] [SerializeField] private ExternalControlTypes externalControl;
        [SerializeField] private float valueMin;
        [SerializeField] private float valueMax;


        public float GetControlValue(float inputValue)
        {
            switch (externalControl)
            {
                case ExternalControlTypes.None:
                    return inputValue;
                case ExternalControlTypes.RandomEachPlay:
                    float rangeMin = valueMin;
                    float rangeMax = valueMax;
                    return Random.Range(rangeMin, rangeMax);
                case ExternalControlTypes.FollowGameParameter:
                    return Mathf.Lerp(valueMin, valueMax, inputValue);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ExternalControlTypes ExternalControl => externalControl;

        public void Init()
        {
            externalControl = ExternalControlTypes.RandomEachPlay;
            valueMin = 0;
            valueMax = 1;
        }
    }


    [SerializeField] private ControlledValue volume;
    [SerializeField] private ControlledValue pitch;


    private int _currentPlayIndex;


    [SerializeField] ActivationSettings playSettings;
    [SerializeField] ActivationSettings stopSettings;


    [Serializable]
    public struct ActivationSettings
    {
        [Range(0, 5f)] public float delay;
        [Range(0, 5f)] public float fadeDuration;
        public bool UseFade => fadeDuration > 0;
    }

    public struct SoundPositionSettings
    {
        private SoundPositionMode.SoundPositionType _positionMode;
        public readonly bool Spatialize;
        public readonly float MaxDistance;
        public readonly float MinDistance;


        public SoundPositionSettings(SoundPositionMode positionMode) : this()
        {
            MaxDistance = positionMode.maxDistance;
            MinDistance = positionMode.minDistance;
            _positionMode = positionMode.soundPositionType;
            Spatialize = _positionMode == SoundPositionMode.SoundPositionType.WorldSpace;
        }

        public float GetPan(GameObject gameObject)
        {
            switch (_positionMode)
            {
                case SoundPositionMode.SoundPositionType.WorldSpace:
                    return 0;
                case SoundPositionMode.SoundPositionType.ScreenSpace:
                    return BlipRuntime.GetSound2DPan(gameObject);
                case SoundPositionMode.SoundPositionType.None:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public bool ExternalPitchControl => pitch.IsExternallyControlled;
    public bool ExternalVolumeControl => volume.IsExternallyControlled;

    public bool Is2D => soundPositionMode.soundPositionType == SoundPositionMode.SoundPositionType.ScreenSpace;

    public SoundPositionMode.SoundPositionType PositionType => soundPositionMode.soundPositionType;

    public float GetPitchValue()
    {
        return pitch.GetValue(_externalParameterValue);
    }

    public ControlledValue GetVolumeControl() => volume;


    public float GetVolumeValue()
    {
        return volume.GetValue(_externalParameterValue);
    }

    public AudioClip GetAudioClip()
    {
        if (audioClips == null || audioClips.Length == 0) return null;

        switch (clipSelectMode)
        {
            case ClipSelectMode.Random:
                return audioClips[Random.Range(0, audioClips.Length)];

            case ClipSelectMode.Sequential:
                _currentPlayIndex++;
                return audioClips[(int)Mathf.Repeat(_currentPlayIndex, audioClips.Length)];

            case ClipSelectMode.All:
                return audioClips[0];

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public bool GetLooping()
    {
        return playMode == PlayMode.Looping;
    }

    public SoundPositionSettings GetSoundPositionSettings()
    {
        return new SoundPositionSettings(soundPositionMode);
    }

    public ActivationSettings GetStopSettings() => stopSettings;
    public ActivationSettings GetPlaySettings() => playSettings;

    public Blip()
    {
        volume.value = 1;
        pitch.value = 1;
        soundPositionMode = new SoundPositionMode(SoundPositionMode.SoundPositionType.None, 100);
        audioClips = new AudioClip[1];
        audioClips[0] = BlipRuntime.DebugClip;
        pitch.Init(1);
        volume.Init(0.8f);
        playSettings.fadeDuration = 0.2f;
        stopSettings.fadeDuration = 0.2f;
    }

    private float _externalParameterValue;
    public float ParameterValue => _externalParameterValue;

    public void RegisterPlay()
    {
        volume.OnPlay(_externalParameterValue);
        pitch.OnPlay(_externalParameterValue);
    }

    public static void Play(Blip sound, GameObject parentObject = null) => BlipRuntime.Play(sound, parentObject);
    public static void Stop(Blip sound, GameObject parentObject = null) => BlipRuntime.Stop(sound, parentObject);

    public void Play(GameObject parentObject = null) => BlipRuntime.Play(this, parentObject);
    public void Stop(GameObject parentObject = null) => BlipRuntime.Stop(this, parentObject);

    public void SetParameter(GameObject parentObject, float value)
    {
        _externalParameterValue = value;
        BlipRuntime.SetParameter(this, parentObject, value);
    }
}