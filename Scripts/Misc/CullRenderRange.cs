using Godot;
using System;

public class CullRenderRange : Node2D
{
	public bool Culled { get{return !visible;} }
	private bool visible = true;

	public bool CameraInRange(float x){
		return x>Cam.instance.Left && x<Cam.instance.Right;
	}

	public bool CameraInRange(float x,float y, float extraSpace=0){
		return x>Cam.instance.Left-extraSpace && x<Cam.instance.Right+extraSpace && y>Cam.instance.Top-extraSpace && y<Cam.instance.Bottom+extraSpace;
	}

	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if(Cam.instance==null){return;}
		visible = CameraInRange(GlobalPosition.x,GlobalPosition.y);
	}
}
