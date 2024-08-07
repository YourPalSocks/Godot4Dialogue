using Godot;
using System;
using System.Collections.Generic;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


public interface IDialogueEvent
{
    int lineStart { get; }
}

public struct TransitionEvent : IDialogueEvent
{
    private int line;
    private string label;
    private string filename;

    public int lineStart{ get{return line;} }
    public string Label{ get{return label;} }
    public string Filename { get {return filename;} }
    
    public TransitionEvent(int ls, string lb, string fn)
    {
        line = ls;
        label = lb;
        filename = fn;
    }
}

public struct WaitEvent : IDialogueEvent
{
    private int line;
    private int timeS;

    public int lineStart{ get{return line;} }
    public int TimeSec{ get{return timeS;} }

    public WaitEvent(int ls, int tm)
    {
        line = ls;
        timeS = tm;
    }
}

public struct ChoiceEvent : IDialogueEvent
{
    private int line;
    private string[] choices;
    private string[] results;
    private string filename;

    public int lineStart{ get { return line; } }
    public string[] Choices { get {return choices;} }
    public string[] Results { get {return results;} }
    public string Filename { get {return filename;} }

    public ChoiceEvent(int ls, string[] c, string[] r, string fn)
    {
        choices = c;
        line = ls;
        results = r;
        filename = fn;
    }
}

// Class to create dialogue events without weird overhead
public class DialogueEventFactory
{
    public static ChoiceEvent CreateChoiceDialogueEvent(int lineIndex, string label, string filename)
    {
        // Get choices from the file at the label
        var file = FileAccess.Open(filename, FileAccess.ModeFlags.Read);
        string strContent = file.GetAsText();
        file.Close();
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        Dictionary<object, object> content = deserializer.Deserialize<Dictionary<object, object>>(strContent);

        // Get the choices and results (parallel arrays)
        List<object> choices = (List<object>) ((Dictionary<object, object>) content[label])["choices"];
        List<object> results = (List<object>) ((Dictionary<object, object>) content[label])["results"];

        // Give new properties
        List<string> chL = new List<string>();
        List<string> resL = new List<string>();
        for(int i = 0; i < choices.Count; i++)
        {
            chL.Add((string) choices[i]);
            resL.Add((string) results[i]);
        }
        return new ChoiceEvent(lineIndex, chL.ToArray(), resL.ToArray(), filename);
    }

    public static TransitionEvent CreateTransitionDialogueEvent(int lineIndex, string label, string filename)
    {
        return new TransitionEvent(lineIndex, label, filename);
    }

    public static WaitEvent CreateWaitDialogueEvent(int lineIndex, int timeS)
    {
        return new WaitEvent(lineIndex, timeS);
    }
}
