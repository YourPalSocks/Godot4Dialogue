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
    private Timer timer;

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
        timer = GetNode<Timer>("Timer");
        choiceBox.Visible = false;
        diagBox.Visible = false;
    }

    public override void _Process(double delta)
    {
        // Check for press
        if (Input.IsActionJustPressed("Interact"))
            pressed = true;

        // Don't bother if not open, or choices open
        if (!diagBox.Visible || choiceBox.Visible || timer.TimeLeft != 0)
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
                else if (ev.GetType() == typeof(TransitionEvent) && pressed)
                {
                    TransitionEvent t_ev = (TransitionEvent) ev;
                    isActive = false;
                    LoadLines(t_ev.Filename, t_ev.Label);
                    events.Remove(ev);
                    pressed = false;
                    break;
                }
                else if (ev.GetType() == typeof(WaitEvent))
                {
                    WaitEvent w_ev = (WaitEvent) ev;
                    timer.WaitTime = w_ev.TimeSec;
                    timer.Start();
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
            Cleanup();
            EmitSignal(SignalName.DialogueClose);
            pressed = false;
        }
    }

    public void Cleanup()
    {
        diagBox.ClearBox();
        lines.Clear();
        events.Clear();
        diagBox.Visible = false;
        isActive = false;
    }

    public void LoadLines(string fn, string label)
    {
        if (isActive)
            return;

        Cleanup();
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
                if (opt.Contains("event"))
                {
                    Dictionary<object, object> evDict = (Dictionary<object, object>) options[opt];
                    string eventType = (string) evDict["type"]; // Doing nothing for now.
                    int eventLaunchIndex = 0;
                    string eventLabel = "";
                    IDialogueEvent ev = null;
                    switch (eventType.ToLower())
                    {
                        case "choice":
                            eventLaunchIndex = lines.Count; // Choice has to occur on last line
                            eventLabel = (string) evDict["name"];
                            ev = DialogueEventFactory.CreateChoiceDialogueEvent(eventLaunchIndex, eventLabel, fn);
                            events.Add(ev);
                            break;

                        case "transition":
                            eventLaunchIndex = lines.Count;
                            eventLabel = (string) evDict["name"];
                            ev = DialogueEventFactory.CreateTransitionDialogueEvent(eventLaunchIndex, eventLabel, fn);
                            events.Add(ev);
                            break;

                        case "wait":
                            eventLaunchIndex = int.Parse((string) evDict["line"]);
                            int waitTime = int.Parse((string) evDict["time"]);
                            ev = DialogueEventFactory.CreateWaitDialogueEvent(eventLaunchIndex, waitTime);
                            events.Add(ev);
                            break;
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
        diagBox.Visible = true;
        isActive = true;
        curLine = 0;
        EmitSignal(SignalName.DialogueOpen);
    }
}
