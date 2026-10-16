using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlipObjectTracker
{
    private GameObject _parent;
    private AudioSource _source;
    private Blip _blip;

    private bool _isFree;
    public bool IsFree => _isFree;
    public AudioSource Source => _source;
    public GameObject Parent => _parent;
    public Blip Blip => _blip;
    public float PlayTime => _playTime;
    float _playTime;

    private static int _nextPlayId;
    // Identifies a single playback, since trackers are reused for other sounds
    public int PlayId { get; private set; }

    private bool _isFadingVolume;

    private Action _onStopped;
    private float _startTimer = -1;
    private float _stopTimer = -1;
    private float _schedulePlayDelay;
    private float _scheduleStopDelay;


    struct Fade
    {
        public float timer;
        public float initialValue;
        public float targetValue;
        public float duration;

        public Fade(float initialValue, float targetValue, float duration)
        {
            timer = 0;
            this.initialValue = initialValue;
            this.targetValue = targetValue;
            this.duration = duration;
        }
    }

    private Fade _fade;
    // Multiplied onto the volume, so fades follow the same volume curve and live parameter changes
    private float _fadeGain = 1;
    private float _parameter = 1;

    public BlipObjectTracker(AudioSource source)
    {
        _source = source;
        _source.spatialBlend = 1;
        _source.spread = 10;
        _source.dopplerLevel = 0;
        _isFree = true;
        _startTimer = -1;
        _stopTimer = -1;
    }

    public void Update()
    {
        if (_isFree) return;
        if (!_parent)
        {
            _source.Stop();
            _isFree = true;
            return;
        }

        if (!_source.isPlaying && _startTimer < 0)
        {
            _isFree = true;
            return;
        }

        if (_startTimer >= 0)
        {
            _startTimer += BlipRuntime.DeltaTime;
        }

        if (_startTimer > _schedulePlayDelay)
        {
            _startTimer = -1;
            _source.Play();
            if (_blip.GetPlaySettings().UseFade)
            {
                _fade = new Fade(0, 1, _blip.GetPlaySettings().fadeDuration);
                _isFadingVolume = true;
            }
        }

        if (_stopTimer >= 0)
        {
            _stopTimer += BlipRuntime.DeltaTime;
        }

        if (_stopTimer > _scheduleStopDelay)
        {
            _stopTimer = -1;
            if (!_blip.GetStopSettings().UseFade)
            {
                DoStop();
                return;
            }

            _fade = new Fade(_fadeGain, 0, _blip.GetStopSettings().fadeDuration);
            _isFadingVolume = true;
        }


        if (_blip.Is2D)
        {
            _source.panStereo = BlipRuntime.GetSound2DPan(_parent);
        }

        if (_isFadingVolume)
        {
            _fade.timer += BlipRuntime.DeltaTime;
            _fadeGain = Mathf.Lerp(_fade.initialValue, _fade.targetValue, _fade.timer / _fade.duration);

            if (_fade.timer > _fade.duration)
            {
                _isFadingVolume = false;
                if (_fade.targetValue == 0)
                {
                    DoStop();
                    return;
                }
            }
        }

        HandleVolumeAndPitch();

        if (_source.spatialBlend > 0)
        {
            _source.transform.position = _parent.transform.position;
        }
    }

    public void Play(Blip sound, GameObject parentObject, AudioClip specificCLip = null)
    {
        _blip = sound;
        _parent = parentObject;
        _isFree = false;
        PlayId = ++_nextPlayId;
        _source.clip = specificCLip ?? sound.GetAudioClip();

        _fadeGain = sound.GetPlaySettings().UseFade ? 0 : 1;
        _source.volume = Mathf.Pow(sound.GetVolumeValue(), 2) * _fadeGain;
        _source.pitch = sound.GetPitchValue();
        _source.loop = sound.GetLooping();
        var positionSettings = sound.GetSoundPositionSettings();
        _source.spatialBlend = positionSettings.Spatialize ? 1 : 0;
        _source.spatialize = positionSettings.Spatialize;
        _source.minDistance = positionSettings.MinDistance;
        _source.maxDistance = positionSettings.MaxDistance;
        _source.panStereo = positionSettings.GetPan(parentObject);
        _startTimer = 0;
        _stopTimer = -1;
        _isFadingVolume = false;
        _schedulePlayDelay = sound.GetPlaySettings().delay;
        _playTime = Time.time + _schedulePlayDelay;
    }

    public float GetPlaybackPercent()
    {
        if (_isFree)
            return 100;

        if (!_source.clip)
            return 100;

        return (_source.time / _source.clip.length) * 100f;
    }

    void DoStop()
    {
        _blip = null;
        _parent = null;
        _source.Stop();
        _isFree = true;
        _onStopped?.Invoke();
        _startTimer = -1;
    }

    public void Stop(Action onStopped = null)
    {
        _onStopped = onStopped;
        _stopTimer = 0;
        _scheduleStopDelay = _blip.GetStopSettings().delay;
    }

    public void SetParameter(float value)
    {
        _parameter = value;
        HandleVolumeAndPitch();
    }

    void HandleVolumeAndPitch()
    {
        _source.pitch = Mathf.Max(_blip.GetPitchValue(), 0.1f);
        _source.volume = Mathf.Pow(_blip.GetVolumeValue(), 2) * _fadeGain;
    }
}