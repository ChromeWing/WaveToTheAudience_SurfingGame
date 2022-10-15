using Godot;
using System;

public class SurfBoard : KinematicBody2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	[Export]
	private float holdDistance = 5;

	private SurfPlayer owner;

	private Vector2 speed = new Vector2(0,0);

	private float cachedSpin = 0;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public override void _PhysicsProcess(float delta){
		if(owner!=null && owner!=GetParent()){
			Rotation += cachedSpin;
			MoveAndCollide(speed);
		}
	}


	public void Holding(Vector2 aim,float spin,float delta){
		if(owner!=null && owner!=GetParent()){
			GetParent().RemoveChild(this);
			owner.AddChild(this);
			owner = null;
		}
		cachedSpin = spin;
		Rotation = Mathf.LerpAngle(Rotation,aim.Angle(),delta*100f);
		Position = Position.LinearInterpolate(aim.Rotated((float)Math.PI/2f).Normalized()*holdDistance,delta*100f);
	}

	public void Thrown(SurfPlayer owner_,Vector2 launchDirection){
		owner = owner_;
		owner.RemoveChild(this);
		owner.GetParent().AddChild(this);
		GlobalPosition = owner_.GlobalPosition;
		speed = launchDirection;
	}

	public void Returning(Vector2 target,float progress){
		GlobalPosition = GlobalPosition.LinearInterpolate(target,progress);
	}


}
