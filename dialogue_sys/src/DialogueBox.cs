using Godot;
using System;

public enum TypeSpeed : int
{
    FAST = 2,
    SLOW = 10,
}

public partial class DialogueBox : Control
{
    [ExportGroup("")]
    [Export]
    private TypeSpeed typeSpeed = TypeSpeed.FAST;

    [Export]
    private bool canSkip = true;
    
    private Label dialogueText;
    private Label speakerText;

    // Typing Stats 
    private static bool istyping = false;
    public static bool isTyping { get { return istyping; } }
    private int curChar = 0;
    private double t = 0;
    private double tsR = 0;

    // Storing what line we're on
    private string curLine = "";


    public override void _Ready()
    {
        dialogueText = GetNode<Label>("%Text");
        speakerText = GetNode<Label>("%Speaker");
        tsR = (double)typeSpeed / 100;

        dialogueText.Text = "";
        speakerText.Text = "";
    }

    public override void _Process(double delta)
    {
        t += delta;
        if (this.Visible && istyping)
        {
            // Check for interrupt
            if (DialogueManager.pressed && canSkip)
            {
                DialogueManager.pressed = false;
                dialogueText.Text = curLine;
                istyping = false;
                return;
            }
            
            // Type
            if(t >= tsR)
            {
                t = 0;
                dialogueText.Text += curLine[curChar++];
            }
            // Check if done
            if(curChar >= curLine.Length)
                istyping = false;
        }
    }

    public void QueueText(string d)
    {
        if (istyping)
            return;

        ClearBox();
        // Find speaker based off colon
        string[] line = d.Split("::");
        if (line.Length == 1)
            curLine = line[0].Trim();
        else // Assume format 'speaker:line'
        {
            speakerText.Text = line[0].Trim();
            curLine = line[1].Trim();
        }

        // Reset
        curChar = 0;
        istyping = true; 
    }

    public void ClearBox()
    {
        dialogueText.Text = "";
        speakerText.Text = "";
    }
}
