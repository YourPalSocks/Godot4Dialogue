using Godot;
using System;

public partial class ChoiceBox : Control
{
    private DialogueManager dMan;
    private VBoxContainer choices;
    private ChoiceEvent curEv;

    [Export(PropertyHint.File, "*.tres")]
    private string highlightloc;
    private StyleBox highlight;

    private int selectedChoice = 0;

    public override void _Ready()
    {
        choices = GetNode<VBoxContainer>("%ChoiceStack");
        dMan = GetParent<DialogueManager>();
        highlight = GD.Load<StyleBox>(highlightloc);
    }

    public override void _Process(double delta)
    {
        if (!this.Visible || choices.GetChildCount() == 0)
            return;

        // Check input
        if(Input.IsActionJustPressed("Choice_Up"))
        {
            selectedChoice--;
            if (selectedChoice < 0)
                selectedChoice = choices.GetChildCount() - 1;
            HighlightCurrentChoice();
        }
        else if (Input.IsActionJustPressed("Choice_Down"))
        {
            selectedChoice++;
            if(selectedChoice >= choices.GetChildCount())
                selectedChoice = 0;
            HighlightCurrentChoice();
        }

        if (DialogueManager.pressed && !DialogueBox.isTyping)
        {
            // Send result to Dialogue and close
            // TODO, this is a really hacky override
            DialogueManager.isActive = false;
            dMan.LoadLines(curEv.Filename , curEv.Results[selectedChoice]);
            this.Visible = false;
            DialogueManager.pressed = false;
        }
    }

    public void LaunchChoiceEvent(ChoiceEvent ev)
    {
        this.Visible = true;
        curEv = ev;
        // Remove previous choices
        foreach (Control c in choices.GetChildren())
            c.QueueFree();

        // Load choices into VBox
        int num = 0;
        foreach(string c in ev.Choices)
        {
            // Create new text object
            Label l = new Label
            {
                Text = c,
                HorizontalAlignment = HorizontalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsVertical = SizeFlags.ExpandFill,
            };
            l.AddThemeColorOverride("font_color", Colors.Black);
            // Add highlight if first
            if(num == 0)
                l.AddThemeStyleboxOverride("normal", highlight);
            // Add to VBox
            choices.AddChild(l);
            num++;
        }
        // TODO, Dynamic Scaling based on # of choices
        selectedChoice = 0;
    }

    private void HighlightCurrentChoice()
    {
        for(int i = 0; i < choices.GetChildCount(); i++)
        {
            Control opt = choices.GetChild<Control>(i);
            if (i == selectedChoice)
            {
                opt.AddThemeStyleboxOverride("normal", highlight);
            }
            else
            {
                opt.RemoveThemeStyleboxOverride("normal");
            }
        }
    }
}
