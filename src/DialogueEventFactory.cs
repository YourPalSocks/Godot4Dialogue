using Godot;
using System;
using System.Collections.Generic;

public interface IDialogueEvent
{
    int lineStart { get; }
}

public struct ChoiceEvent : IDialogueEvent
{
    private string speaker;
    private string[] choices;
    private string[] scores;
    private string[] results;
    private int ls;

    public int lineStart{ get { return ls; } }

    public string[] Choices { get {return choices;} }
    public string[] Scores { get {return scores;}}
    public string[] Results { get {return results;} }
    public string Speaker { get{return speaker;} }

    public ChoiceEvent(int line, string sp, string[] c, string[] r, string[] sc)
    {
        choices = c;
        ls = line;
        results = r;
        speaker = sp;
        scores = sc;
    }
}

public struct MinigameEvent : IDialogueEvent
{
    private int ls;
    private int gameLength;
    private int difficulty;
    public int lineStart{ get { return ls; } }
    public int GameLength{ get { return gameLength; } }
    public int Difficulty{ get { return difficulty; } }

    public MinigameEvent(int line, int amt, int diff)
    {
        ls = line;
        gameLength = amt;
        difficulty = diff;
    }
}


// Class to create dialogue events without weird overhead
public class DialogueEventFactory
{
    public static ChoiceEvent CreateChoiceDialogueEvent(int lineIndex, string label, string filename, string speaker)
    {
        // Get choices from the file at the label
        var file = FileAccess.Open(filename, FileAccess.ModeFlags.Read);
        string strContent = file.GetAsText();
        file.Close();
        Godot.Collections.Dictionary content = (Godot.Collections.Dictionary) Json.ParseString(strContent);
        // Get the choices and results (parallel arrays)
        Godot.Collections.Array choices = (Godot.Collections.Array) ((Godot.Collections.Dictionary) content[label])["choices"];
        Godot.Collections.Array results = (Godot.Collections.Array) ((Godot.Collections.Dictionary) content[label])["results"];
        Godot.Collections.Array scores = (Godot.Collections.Array) ((Godot.Collections.Dictionary) content[label])["scores"];

        // Give new properties
        List<string> chL = new List<string>();
        List<string> sor = new List<string>();
        List<string> resL = new List<string>();
        for(int i = 0; i < choices.Count; i++)
        {
            sor.Add((string) scores[i]);
            chL.Add((string) choices[i]);
            resL.Add((string) results[i]);
        }

        return new ChoiceEvent(lineIndex, speaker, chL.ToArray(), resL.ToArray(),sor.ToArray());
    }
}
