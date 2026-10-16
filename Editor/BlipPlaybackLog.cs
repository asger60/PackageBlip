using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BlipPlaybackLog : EditorWindow
{
    private enum Tab
    {
        AllSounds,
        Oneshots,
        Looping
    }

    private class LogEntry
    {
        public string Time;
        public Blip Sound;
        public string SoundName;
        public string ParentName;
        public GameObject ParentObject;
        public string EventType;
        public Color Color;
        public bool IsLooping;
        public BlipObjectTracker Tracker;
        public int PlayId;
    }

    private const int ProgressRefreshMs = 100;

    private List<LogEntry> _logEntries = new List<LogEntry>();
    private List<LogEntry> _visibleEntries = new List<LogEntry>();
    private MultiColumnListView _listView;
    private Column _progressColumn;
    private readonly List<ToolbarToggle> _tabToggles = new List<ToolbarToggle>();
    private Tab _currentTab = Tab.AllSounds;

    // Serialized so the ignore list survives domain reloads
    [SerializeField] private List<Blip> _ignoredSounds = new List<Blip>();
    private VisualElement _ignoreBar;

    [MenuItem("FloppyClub/Blip/Playback Log")]
    public static void ShowWindow()
    {
        GetWindow<BlipPlaybackLog>("Blip Playback Log");
    }

    private void OnEnable()
    {
        BlipRuntime.OnPlayEvent += OnPlay;
        BlipRuntime.OnStopEvent += OnStop;
    }

    private void OnDisable()
    {
        BlipRuntime.OnPlayEvent -= OnPlay;
        BlipRuntime.OnStopEvent -= OnStop;
    }

    private void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Toolbar
        VisualElement toolbar = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                backgroundColor = new Color(0.2f, 0.2f, 0.2f),
                paddingBottom = 2,
                paddingTop = 2,
                paddingLeft = 5,
                paddingRight = 5
            }
        };

        AddTabToggle(toolbar, Tab.AllSounds, "All sounds");
        AddTabToggle(toolbar, Tab.Oneshots, "Oneshots");
        AddTabToggle(toolbar, Tab.Looping, "Looping");

        toolbar.Add(new VisualElement { style = { flexGrow = 1 } });

        Button clearButton = new Button(() => {
            _logEntries.Clear();
            RefreshList();
        }) { text = "Clear" };
        toolbar.Add(clearButton);

        root.Add(toolbar);

        // Ignored sounds, each one a button that re-enables it
        _ignoreBar = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                flexWrap = Wrap.Wrap,
                alignItems = Align.Center,
                paddingBottom = 2,
                paddingTop = 2,
                paddingLeft = 5,
                paddingRight = 5
            }
        };
        root.Add(_ignoreBar);
        RefreshIgnoreBar();

        // MultiColumnListView
        _listView = new MultiColumnListView();
        _listView.itemsSource = _visibleEntries;
        _listView.fixedItemHeight = 20;
        _listView.style.flexGrow = 1;

        var columns = _listView.columns;

        // Ignore Column
        var ignoreColumn = new Column { name = "Ignore", title = "", width = 24, resizable = false };
        ignoreColumn.makeCell = () =>
        {
            var button = new Button { text = "\u00d7", tooltip = "Ignore this sound" };
            button.style.width = 16;
            button.style.height = 16;
            button.style.paddingLeft = 0;
            button.style.paddingRight = 0;
            button.clicked += () =>
            {
                if (button.userData is LogEntry entry) IgnoreSound(entry.Sound);
            };
            return button;
        };
        ignoreColumn.bindCell = (element, index) => element.userData = _visibleEntries[index];
        columns.Add(ignoreColumn);

        // Time Column
        var timeColumn = new Column { name = "Time", title = "Time", width = 80 };
        timeColumn.makeCell = () => new Label { style = { paddingLeft = 5 } };
        timeColumn.bindCell = (element, index) => ((Label)element).text = _visibleEntries[index].Time;
        columns.Add(timeColumn);

        // Event Column
        var eventColumn = new Column { name = "Event", title = "Event", width = 60 };
        eventColumn.makeCell = () => new Label { style = { paddingLeft = 5 } };
        eventColumn.bindCell = (element, index) =>
        {
            var label = (Label)element;
            label.text = _visibleEntries[index].EventType;
            label.style.color = _visibleEntries[index].Color;
        };
        columns.Add(eventColumn);

        // Sound Column
        var soundColumn = new Column { name = "Sound", title = "Sound", width = 150 };
        soundColumn.makeCell = () =>
        {
            var label = new Label { style = { paddingLeft = 5 }, tooltip = "Click to ping the sound" };
            // Pointer down instead of click, so the row selection doesn't ping the parent object instead
            label.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (label.userData is LogEntry entry && entry.Sound)
                {
                    EditorGUIUtility.PingObject(entry.Sound);
                    evt.StopPropagation();
                }
            });
            return label;
        };
        soundColumn.bindCell = (element, index) =>
        {
            element.userData = _visibleEntries[index];
            ((Label)element).text = _visibleEntries[index].SoundName;
        };
        columns.Add(soundColumn);

        // Parent Column
        var parentColumn = new Column { name = "Parent", title = "Parent", width = 150 };
        parentColumn.makeCell = () => new Label { style = { paddingLeft = 5 } };
        parentColumn.bindCell = (element, index) => ((Label)element).text = _visibleEntries[index].ParentName;
        columns.Add(parentColumn);

        // Progress Column, only shown for looping sounds
        _progressColumn = new Column { name = "Progress", title = "Progress", width = 200 };
        _progressColumn.makeCell = () => new ProgressBar
        {
            lowValue = 0,
            highValue = 1,
            style = { marginTop = 2, marginBottom = 2, marginLeft = 5, marginRight = 5 }
        };
        _progressColumn.bindCell = (element, index) => BindProgress((ProgressBar)element, _visibleEntries[index]);
        columns.Add(_progressColumn);

        _listView.onSelectionChange += OnSelectionChanged;

        root.Add(_listView);

        root.schedule.Execute(() =>
        {
            if (_currentTab == Tab.Looping) _listView.RefreshItems();
        }).Every(ProgressRefreshMs);

        SetTab(_currentTab);
    }

    private void AddTabToggle(VisualElement toolbar, Tab tab, string text)
    {
        var toggle = new ToolbarToggle { text = text };
        toggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue)
                SetTab(tab);
            else if (_currentTab == tab)
                toggle.SetValueWithoutNotify(true);
        });
        _tabToggles.Add(toggle);
        toolbar.Add(toggle);
    }

    private void SetTab(Tab tab)
    {
        _currentTab = tab;
        for (int i = 0; i < _tabToggles.Count; i++)
        {
            _tabToggles[i].SetValueWithoutNotify(i == (int)tab);
        }

        _progressColumn.visible = tab == Tab.Looping;
        RefreshList();
    }

    private void IgnoreSound(Blip sound)
    {
        if (!sound || _ignoredSounds.Contains(sound)) return;
        _ignoredSounds.Add(sound);
        RefreshIgnoreBar();
        RefreshList();
    }

    private void UnignoreSound(Blip sound)
    {
        _ignoredSounds.Remove(sound);
        RefreshIgnoreBar();
        RefreshList();
    }

    private void RefreshIgnoreBar()
    {
        _ignoredSounds.RemoveAll(sound => !sound);
        _ignoreBar.Clear();
        _ignoreBar.style.display = _ignoredSounds.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        if (_ignoredSounds.Count == 0) return;

        _ignoreBar.Add(new Label("Ignored:") { style = { marginRight = 4 } });
        foreach (var sound in _ignoredSounds)
        {
            _ignoreBar.Add(new Button(() => UnignoreSound(sound))
            {
                text = sound.name,
                tooltip = "Click to show this sound again"
            });
        }
    }

    private bool IsVisible(LogEntry entry)
    {
        if (entry.Sound && _ignoredSounds.Contains(entry.Sound)) return false;

        return _currentTab switch
        {
            Tab.Oneshots => !entry.IsLooping,
            Tab.Looping => entry.IsLooping,
            _ => true
        };
    }

    private void RefreshList()
    {
        _visibleEntries.Clear();
        foreach (var entry in _logEntries)
        {
            if (IsVisible(entry)) _visibleEntries.Add(entry);
        }

        _listView?.Rebuild();
    }

    private static void BindProgress(ProgressBar bar, LogEntry entry)
    {
        BlipObjectTracker tracker = entry.Tracker;
        bool isActive = tracker != null && tracker.Source && !tracker.IsFree && tracker.PlayId == entry.PlayId && tracker.Source.clip;

        bar.style.visibility = isActive ? Visibility.Visible : Visibility.Hidden;
        if (!isActive) return;

        float length = tracker.Source.clip.length;
        float time = tracker.Source.time;
        bar.value = length > 0 ? time / length : 0;
        bar.title = $"{time:0.0} / {length:0.0} s";
    }

    private void OnSelectionChanged(IEnumerable<object> selectedItems)
    {
        foreach (var item in selectedItems)
        {
            if (item is LogEntry entry && entry.ParentObject != null)
            {
                Selection.activeGameObject = entry.ParentObject;
                EditorGUIUtility.PingObject(entry.ParentObject);
                break;
            }
        }
    }

    private void OnPlay(Blip sound, GameObject parent)
    {
        LogEntry entry = AddEntry("Play", sound, parent, new Color(0.4f, 1f, 0.4f));
        entry.Tracker = BlipRuntime.GetNewestTracker(sound, parent);
        entry.PlayId = entry.Tracker?.PlayId ?? -1;
    }

    private void OnStop(Blip sound, GameObject parent)
    {
        AddEntry("Stop", sound, parent, new Color(1f, 0.4f, 0.4f));
    }

    private LogEntry AddEntry(string eventType, Blip sound, GameObject parent, Color color)
    {
        var entry = new LogEntry
        {
            Time = System.DateTime.Now.ToString("HH:mm:ss"),
            EventType = eventType,
            Sound = sound,
            SoundName = sound ? sound.name : "null",
            ParentName = parent ? parent.name : "null",
            ParentObject = parent,
            Color = color,
            IsLooping = sound && sound.GetLooping()
        };
        _logEntries.Insert(0, entry);

        if (IsVisible(entry))
        {
            _visibleEntries.Insert(0, entry);
            _listView?.Rebuild();
        }

        return entry;
    }
}
