using Godot;
using System;

public class SurfableZone : Area2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	public enum Type{
		WATER,
		CLOUD
	}

	public bool HasBuoyancy { get{ return type==Type.WATER; }}

	[Export]
	private Type type;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	private void _on_Area2D_body_entered(object body)
	{
		if(body is SurfPlayer){
			(body as SurfPlayer).EnterSurfZone(this);
		}
	}


	private void _on_Area2D_body_exited(object body)
	{
		if(body is SurfPlayer){
			(body as SurfPlayer).ExitSurfZone(this);
		}
	}

	
}


	
