using Godot;
using System;

public partial class ActivateDialogue : Area2D
{
    [Export(PropertyHint.File, "*.yaml,*.yml")]
    private string linesFile;

    [Export]
    private string speaker = "Speaker A";

    [Export]
    private string chunk;

    private DialogueManager dMan;
    bool playerInArea = false;


    public override void _Ready()
    {
        dMan = GetTree().Root.GetChild(0).GetNode<DialogueManager>("%DialogueUI");
        dMan.DialogueClose += _on_dialogue_closed;
        // Connect in/out events
        this.BodyEntered += _on_body_entered;
        this.BodyExited += _on_body_exited;
    }
    
    private void _on_body_entered(Node2D body)
    {
        if(body.GetType() == typeof(PlayerMovement))
            playerInArea = true;
    }

    private void _on_body_exited(Node2D body)
    {
        if(body.GetType() == typeof(PlayerMovement))
            playerInArea = false;
    }
    
    public override void _Process(double delta)
    {
        if(playerInArea && DialogueManager.pressed && !DialogueManager.isActive)
        {
            dMan.LoadLines(linesFile,chunk,speaker);
            this.Monitoring = false;
        }
    }

    private void _on_dialogue_closed()
    {
        this.Monitoring = true;
    }
}
