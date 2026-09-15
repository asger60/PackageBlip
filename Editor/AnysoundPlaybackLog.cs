using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AnysoundPlaybackLog : EditorWindow
{
    private class LogEntry
    {
        public string Time;
        public string SoundName;
        public string ParentName;
        public GameObject ParentObject;
        public string EventType;
        public Color Color;
    }

    private List<LogEntry> _logEntries = new List<LogEntry>();
    private MultiColumnListView _listView;

    [MenuItem("Window/Anysound/Playback Log")]
    public static void ShowWindow()
    {
        GetWindow<AnysoundPlaybackLog>("Anysound Playback Log");
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

        Button clearButton = new Button(() => {
            _logEntries.Clear();
            _listView.Rebuild();
        }) { text = "Clear" };
        toolbar.Add(clearButton);

        root.Add(toolbar);

        // MultiColumnListView
        _listView = new MultiColumnListView();
        _listView.itemsSource = _logEntries;
        _listView.fixedItemHeight = 20;
        _listView.style.flexGrow = 1;

        var columns = _listView.columns;

        // Time Column
        var timeColumn = new Column { name = "Time", title = "Time", width = 80 };
        timeColumn.makeCell = () => new Label { style = { paddingLeft = 5 } };
        timeColumn.bindCell = (element, index) => ((Label)element).text = _logEntries[index].Time;
        columns.Add(timeColumn);

        // Event Column
        var eventColumn = new Column { name = "Event", title = "Event", width = 60 };
        eventColumn.makeCell = () => new Label { style = { paddingLeft = 5 } };
        eventColumn.bindCell = (element, index) =>
        {
            var label = (Label)element;
            label.text = _logEntries[index].EventType;
            label.style.color = _logEntries[index].Color;
        };
        columns.Add(eventColumn);

        // Sound Column
        var soundColumn = new Column { name = "Sound", title = "Sound", width = 150 };
        soundColumn.makeCell = () => new Label { style = { paddingLeft = 5 } };
        soundColumn.bindCell = (element, index) => ((Label)element).text = _logEntries[index].SoundName;
        columns.Add(soundColumn);

        // Parent Column
        var parentColumn = new Column { name = "Parent", title = "Parent", width = 150 };
        parentColumn.makeCell = () => new Label { style = { paddingLeft = 5 } };
        parentColumn.bindCell = (element, index) => ((Label)element).text = _logEntries[index].ParentName;
        columns.Add(parentColumn);

        _listView.onSelectionChange += OnSelectionChanged;

        root.Add(_listView);
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
        AddEntry("Play", sound, parent, new Color(0.4f, 1f, 0.4f));
    }

    private void OnStop(Blip sound, GameObject parent)
    {
        AddEntry("Stop", sound, parent, new Color(1f, 0.4f, 0.4f));
    }

    private void AddEntry(string eventType, Blip sound, GameObject parent, Color color)
    {
        _logEntries.Insert(0, new LogEntry
        {
            Time = System.DateTime.Now.ToString("HH:mm:ss"),
            EventType = eventType,
            SoundName = sound ? sound.name : "null",
            ParentName = parent ? parent.name : "null",
            ParentObject = parent,
            Color = color
        });

        if (_listView != null)
        {
            _listView.Rebuild();
        }
    }
}
