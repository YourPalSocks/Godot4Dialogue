using Godot;
using System;
using System.Collections.Generic;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


public partial class DialogueManager : Control
{
    public static bool isActive;
    public static bool pressed;

    private DialogueBox diagBox;
    private ChoiceBox choiceBox;

    private List<string> lines = new List<string>();
    private List<IDialogueEvent> events = new List<IDialogueEvent>();
    private int curLine = -1;

    [Signal]
    public delegate void DialogueOpenEventHandler();

    [Signal]
    public delegate void DialogueCloseEventHandler();


    public override void _Ready()
    {
        diagBox = GetNode<DialogueBox>("DialogueBox");
        choiceBox = GetNode<ChoiceBox>("ChoiceBox");
        choiceBox.Visible = false;
        diagBox.Visible = false;
    }

    public override void _Process(double delta)
    {
        // Check for press
        if (Input.IsActionJustPressed("Interact"))
            pressed = true;

        // Don't bother if not open, or choices open
        if (!diagBox.Visible || choiceBox.Visible)
            return;

        // Check events
        foreach (IDialogueEvent ev in events)
        {
            if (curLine == ev.lineStart)
            {
                // Figure out the type
                if (ev.GetType() == typeof(ChoiceEvent))
                {
                    choiceBox.LaunchChoiceEvent((ChoiceEvent) ev);
                    events.Remove(ev);
                    break;
                }
            }
        }

        // Check if we have a line to give
        if (curLine < lines.Count)
        {
            if (curLine == 0 || (pressed && !DialogueBox.isTyping))
            {
                diagBox.QueueText(lines[curLine]);
                curLine++;
            }
            pressed = false;
        }
        else if (pressed && !DialogueBox.isTyping)
        {
            // Cleanup and disable
            lines.Clear();
            events.Clear();
            diagBox.Visible = false;
            isActive = false;
            EmitSignal(SignalName.DialogueClose);
            pressed = false;
        }
    }

    public void LoadLines(string fn, string label)
    {
        if (isActive)
            return;

        lines.Clear();
        // Get lines from the file
        var file = FileAccess.Open(fn, FileAccess.ModeFlags.Read);
        string strContent = file.GetAsText();
        file.Close();
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        Dictionary<object, object> content = deserializer.Deserialize<Dictionary<object, object>>(strContent);
        List<object> arr = (List<object>) ((Dictionary<object, object>) content[label])["text"];
        // Load into lines array
        foreach (string l in arr)
        {
            lines.Add(l);
        }
        // Parse any options
        try
        {
            Dictionary<object, object> options = (Dictionary<object, object>) ((Dictionary<object, object>) content[label])["options"];
            foreach (string opt in options.Keys)
            {
                if (opt == "event")
                {
                    Dictionary<object, object> evDict = (Dictionary<object, object>) options[opt];
                    int eventLaunchIndex = lines.Count; // Choice has to occur on last line
                    string eventType = (string) evDict["type"]; // Doing nothing for now.
                    string eventLabel = (string) evDict["name"];
                    switch (eventType.ToLower())
                    {
                        case "choice":
                            IDialogueEvent ev = DialogueEventFactory.CreateChoiceDialogueEvent(eventLaunchIndex, eventLabel, fn);
                            events.Add(ev);
                            break;
                        // Put other kinds of events here
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Hide message about options being missing, assume its deliberate
            if (!e.Message.Contains("given key 'options'"))
                GD.PrintErr($"Event could not be parsed: {e.Message}");
        }

        // Reset and start
        diagBox.ClearBox();
        diagBox.Visible = true;
        isActive = true;
        curLine = 0;
        EmitSignal(SignalName.DialogueOpen);
    }

    public void InsertNextLine(string nL)
    {
        lines.Insert(curLine, nL);
        pressed = true;
    }

    private void OnMinigameClose()
    {
        pressed = true;
    }
}
