using Godot;
using System;
using System.Threading.Tasks;

public partial class DialogueBox : Control
{
    private Label dialogueText;
    private Label speakerText;

    // Typing Stats 
    private static bool istyping = false;
    public static bool isTyping { get { return istyping; } }
    private int curChar = 0;
    private double t = 0;
    private double typeRate = 0.05f;

    // Storing what line we're on
    private string curLine = "";


    public override void _Ready()
    {
        dialogueText = GetNode<Label>("%Text");
        speakerText = GetNode<Label>("%Speaker");
    }

    public override void _Process(double delta)
    {
        t += delta;
        if (this.Visible && istyping)
        {
            // Check for interrupt
            if (DialogueManager.pressed)
            {
                DialogueManager.pressed = false;
                dialogueText.Text = curLine;
                istyping = false;
                return;
            }
            
            // Type
            if(t >= typeRate)
            {
                t = 0;
                dialogueText.Text = dialogueText.Text + curLine[curChar++];
            }
            // Check if done
            if(curChar >= curLine.Length)
                istyping = false;
        }
    }

    public void QueueText(string s, string d)
    {
        if (istyping)
            return;

        // Reset
        curLine = d;
        // TypeText();
        curChar = 0;
        speakerText.Text = s;
        dialogueText.Text = "";
        istyping = true; 
    }
}
