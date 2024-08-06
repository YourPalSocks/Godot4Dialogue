using Godot;
using System;
using System.Collections.Generic;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


public interface IDialogueEvent
{
    int lineStart { get; }
}

public struct ChoiceEvent : IDialogueEvent
{
    private string[] choices;
    private string[] results;
    private string filename;
    private int ls;

    public int lineStart{ get { return ls; } }
    public string[] Choices { get {return choices;} }
    public string[] Results { get {return results;} }
    public string Filename { get {return filename;} }

    public ChoiceEvent(int line, string[] c, string[] r, string fn)
    {
        choices = c;
        ls = line;
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
}
