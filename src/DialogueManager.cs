using Godot;
using System;
using System.Collections.Generic;

public partial class DialogueManager : Control
{
    public static bool isActive;
    public static bool pressed;

    private DialogueBox diagBox;
    private ChoiceBox choiceBox;

    private List<(string, string)> lines = new List<(string, string)>();
    private List<IDialogueEvent> events = new List<IDialogueEvent>();
    private int curLine = -1;

    [Signal]
    public delegate void DialogueCloseEventHandler();


    public override void _Ready()
    {
        diagBox = GetNode<DialogueBox>("DialogueBox");
        choiceBox = GetNode<ChoiceBox>("ChoiceBox");
        // choiceBox.Visible = false;
        // diagBox.Visible = false;
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
                diagBox.QueueText(lines[curLine].Item1, lines[curLine].Item2);
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

    public void LoadLines(string fn, string label, string speaker)
    {
        if (isActive)
            return;

        lines.Clear();
        // Get lines from the file
        var file = FileAccess.Open(fn, FileAccess.ModeFlags.Read);
        string strContent = file.GetAsText();
        file.Close();
        Godot.Collections.Dictionary content = (Godot.Collections.Dictionary) Json.ParseString(strContent);
        Godot.Collections.Array arr = (Godot.Collections.Array) ((Godot.Collections.Dictionary) content[label])["text"];
        // Load into lines array
        foreach (string l in arr)
        {
            lines.Add((speaker, l));
        }
        // Parse any options
        try
        {
            Godot.Collections.Dictionary options = (Godot.Collections.Dictionary) ((Godot.Collections.Dictionary) content[label])["options"];
            foreach (string opt in options.Keys)
            {
                if (opt == "event")
                {
                    Godot.Collections.Dictionary evDict = (Godot.Collections.Dictionary) options[opt];
                    int eventLaunchIndex = Int32.Parse((string) evDict["line"]);
                    string eventType = (string) evDict["type"]; // Doing nothing for now.
                    string eventLabel = (string) evDict["name"];
                    IDialogueEvent ev = null;
                    switch(eventType.ToLower())
                    {
                        case "choice":
                            ev = DialogueEventFactory.CreateChoiceDialogueEvent(eventLaunchIndex, eventLabel, fn, speaker);
                            events.Add(ev);
                            break;

                        // Put other kinds of events here
                    }
                }
            }
        }
        catch { /* Don't load any events. */ }

        // Reset and start
        diagBox.Visible = true;
        isActive = true;
        curLine = 0;
    }

    public void InsertNextLine((string, string) nL)
    {
        lines.Insert(curLine, nL);
        pressed = true;
    }

    private void OnMinigameClose()
    {
        pressed = true;
    }
}
