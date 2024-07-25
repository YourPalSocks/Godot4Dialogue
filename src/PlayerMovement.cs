using System;
using Godot;

public partial class PlayerMovement : CharacterBody2D 
{
  private float speed = 225;


  public override void _Process(double delta) 
  {
    Vector2 vel = Vector2.Zero;

    if (DialogueManager.isActive)
      return;

    if(Input.IsKeyLabelPressed(Key.W))
      vel = new Vector2(vel.X, -speed);
    else if(Input.IsKeyLabelPressed(Key.S))
      vel = new Vector2(vel.X, speed);

    if(Input.IsKeyLabelPressed(Key.A))
      vel = new Vector2(-speed, vel.Y);
    else if(Input.IsKeyLabelPressed(Key.D))
      vel = new Vector2(speed, vel.Y);
    
    // Apply to controller
    this.Velocity = vel;
    this.MoveAndSlide();
  }
}
