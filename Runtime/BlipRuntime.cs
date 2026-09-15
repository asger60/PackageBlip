using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class BlipRuntime : MonoBehaviour
{
    private static BlipRuntime _instance;

    private static BlipRuntime Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindAnyObjectByType<BlipRuntime>();
            }

            if (!_instance)
            {
                //_instance = new GameObject("AnysoundRuntime").AddComponent<AnysoundRuntime>();
                Debug.LogWarning("No Blip found.");
                return null;
            }


            return _instance;
        }
    }

    private AudioSource[] _sources;
    private List<BlipObjectTracker> _trackers = new();
    [Range(1, 200)] [SerializeField] private int voices = 100;
    private Camera _camera;
    private bool _isInit;
    private double _prevTime;
    public static bool ShowExtendedSettings;
    public static AudioClip DebugClip;

    public static float DeltaTime
    {
        get
        {
            if (Application.isPlaying)
            {
                return Time.deltaTime;
            }
#if UNITY_EDITOR
            return (float)(EditorApplication.timeSinceStartup - Instance._prevTime);
#else
            return 0;
#endif
        }
    }

    private void Start()
    {
        _isInit = false;
        Init();
    }

    public static void Init() => Instance?.DoInit();

    void DoInit()
    {
        if (_isInit) return;
        foreach (var audioSource in GetComponentsInChildren<AudioSource>())
        {
            if (audioSource.gameObject == gameObject) continue;
            DestroyImmediate(audioSource.gameObject);
        }

        _camera = Camera.main;
        _trackers = new List<BlipObjectTracker>(voices);
        for (int i = 0; i < voices; i++)
        {
            var sourceObject = new GameObject("AnysoundSource");
            var source = sourceObject.AddComponent<AudioSource>();
            sourceObject.transform.SetParent(transform);
            _trackers.Add(new BlipObjectTracker(source));
        }
#if UNITY_EDITOR
        DebugClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Packages/com.floppyclub.blip/Runtime/Resources/DebugPling.wav");
#endif
        _isInit = true;
    }

    private void OnValidate()
    {
        _isInit = false;
    }

#if UNITY_EDITOR
    static BlipRuntime()
    {
        EditorApplication.update += EditorUpdate;
    }

    public static Action<Blip, GameObject> OnPlayEvent;
    public static Action<Blip, GameObject> OnStopEvent;

    static void EditorUpdate()
    {
        if (Application.isPlaying) return;
        if (Instance != null)
        {
            Instance.Update();
        }
    }
#endif

    public static void StartPreview(Blip sound)
    {
        if (!Instance._isInit) Init();
        Instance.GetFreeTracker()?.Play(sound, Instance.gameObject);
    }

    public static void StopPreview(Blip sound, Action onStopped = null)
    {
        if (!Instance._isInit) Init();
        foreach (var tracker in Instance.GetTrackers(sound, Instance.gameObject))
        {
            tracker.Stop(onStopped);
        }
    }

    public static void Play(Blip sound, GameObject gameObject) => Instance?.DoPlay(sound, gameObject);


    public static void Stop(Blip sound, GameObject gameObject) => Instance?.DoStop(sound, gameObject);


    public static void SetParameter(Blip sound, GameObject parentObject, float value) => Instance?.DoSetParameter(sound, parentObject, value);


    public static void SetPreviewParameter(float value)
    {
        foreach (var tracker in Instance.GetTrackers(Instance.gameObject))
        {
            tracker.SetParameter(value);
        }
    }

    void DoSetParameter(Blip sound, GameObject parentObject, float value)
    {
        var trackers = GetTrackers(sound, parentObject);
        foreach (var tracker in trackers)
        {
            tracker.SetParameter(value);
        }
    }


    public static bool IsPreviewing(Blip sound)
    {
        if (!Instance._isInit) Init();
        return Instance.GetTrackers(sound, Instance.gameObject).Length > 0;
    }


    void DoPlay(Blip sound, GameObject parentObject)
    {
        Init();
        if (!parentObject)
        {
            Debug.LogWarning("#Anysound#Trying to play with no parent object.. Assigning default.");
            parentObject = gameObject;
        }

        if (!sound)
        {
            Debug.LogWarning("#Anysound#Trying to play null sound " + parentObject.transform.name, parentObject.transform);
            return;
        }

        if (!_isInit) Init();

        GetFreeTracker()?.Play(sound, parentObject);
#if UNITY_EDITOR
        OnPlayEvent?.Invoke(sound, parentObject);
#endif
    }

    void DoStop(Blip sound, GameObject parentObject)
    {
        if (!_isInit) Init();
        if (!parentObject)
        {
            Debug.LogWarning("#Anysound#Trying to stop with no parent object..");
            return;
        }

        if (!sound)
        {
            Debug.LogWarning("#Anysound#Trying to stop null sound " + parentObject.transform.name, parentObject.transform);
            return;
        }

        foreach (var tracker in GetTrackers(sound, parentObject))
        {
            tracker.Stop();
        }
#if UNITY_EDITOR
        OnStopEvent?.Invoke(sound, parentObject);
#endif
    }

    BlipObjectTracker GetFreeTracker()
    {
        foreach (var tracker in _trackers)
        {
            if (tracker.IsFree) return tracker;
        }

        float bestPercent = 0;
        BlipObjectTracker furthestTracker = null;
        foreach (var tracker in _trackers)
        {
            var thisPercent = tracker.GetPlaybackPercent();
            if (thisPercent > bestPercent)
            {
                bestPercent = thisPercent;
                furthestTracker = tracker;
            }
        }

        return furthestTracker;
    }


    BlipObjectTracker[] GetTrackers(GameObject parentObject)
    {
        List<BlipObjectTracker> trackers = new List<BlipObjectTracker>();
        foreach (var tracker in _trackers)
        {
            if (tracker.Parent == parentObject) trackers.Add(tracker);
        }

        return trackers.ToArray();
    }

    BlipObjectTracker[] GetTrackers(Blip sound, GameObject parentObject)
    {
        List<BlipObjectTracker> trackers = new List<BlipObjectTracker>();
        foreach (var tracker in _trackers)
        {
            if (tracker.Blip == sound && tracker.Parent == parentObject) trackers.Add(tracker);
        }

        return trackers.ToArray();
    }


    private void Update()
    {
        foreach (var tracker in _trackers)
        {
            if (tracker.IsFree) continue;
            tracker.Update();
        }
#if UNITY_EDITOR
        _prevTime = EditorApplication.timeSinceStartup;
#endif
    }

    public static float GetSound2DPan(GameObject gameObject)
    {
        if (!Instance._camera) Instance._camera = Camera.main;
        if (Instance._camera)
        {
            var pos = Instance._camera.WorldToViewportPoint(gameObject.transform.position);
            pos.x -= 0.5f;
            pos.x *= 2;
            return pos.x;
        }

        return 0;
    }
}