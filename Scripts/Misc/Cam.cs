using Godot;
using System;

public class Cam : Camera2D
{
	public static Cam instance;

	public float Left{get{return camLeft;}}
	public float Right{get{return camRight;}}
	public float Top{get{return camTop;}}
	public float Bottom{get{return camBottom;}}

	private float camLeft = 0;
	private float camRight = 0;
	private float camBottom = 0;
	private float camTop = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		instance = this;
	}

	public override void _Process(float delta){
		camLeft = GetCameraLeft();
		camRight = GetCameraRight();
		camTop = GetCameraTop();
		camBottom = GetCameraBottom();
	}

	private float GetCameraLeft(){
		return Cam.instance.GlobalPosition.x-(Cam.instance.GetViewportRect().Size.x+1000);
	}

	private float GetCameraRight(){
		return Cam.instance.GlobalPosition.x+(Cam.instance.GetViewportRect().Size.x+1000);
	}

	private float GetCameraTop(){
		return Cam.instance.GlobalPosition.y-(Cam.instance.GetViewportRect().Size.y+1000);
	}

	private float GetCameraBottom(){
		return Cam.instance.GlobalPosition.y+(Cam.instance.GetViewportRect().Size.y+1000);
	}

}
