using Godot;
using System;
using System.Collections.Generic;

public class SurfPlayer : KinematicBody2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	[Export]
	public float swimAccuracy = 20;
	[Export]
	public float grindAccuracy = 110;
	[Export(PropertyHint.Range,"0, 1")]
	public float swimTraction = .8f;
	
	[Export(PropertyHint.Range,"0, 1")]
	public float grindTraction = .2f;
	
	[Export(PropertyHint.Range,"0, 1")]
	public float draggingTraction = .1f;

	[Export(PropertyHint.Range,"0, 500")]
	public float swimBoost = 50;
	
	[Export(PropertyHint.Range,"-500, 500")]
	public float grindBoost = -10;
	
	[Export(PropertyHint.Range,"-1000, 0")]
	public float draggingBoost = -100;

	[Export]
	public float maxSpeed;
	[Export]
	public float maxMovingSpeed;

	[Export]
	public float moveSpeed;
	[Export]
	public float gravity;
	[Export]
	public float buoyancy;

	[Export]
	public float throwDuration = .5f;

	[Export]
	public float throwingSpeed = 300f;

	[Export]
	public float tweenTraversalStateSpeed = 1f;

	private float tweenTraction = .8f;
	private float tweenBoost = 50;

	private Vector2 move;
	private Vector2 aim = new Vector2(1,0);
	private Vector2 surfAim = new Vector2(1,0);

	private Vector2 throwingOrigin = new Vector2(0,0);
	private Vector2 throwingDirection = new Vector2(0,0);

	private List<SurfableZone> occupiedSurfZones = new List<SurfableZone>();

	private bool surf;

	private float angleSpinVel;

	//TODO: create Swimmable script for swimmable surfaces

	public bool SwimReady { get{ return occupiedSurfZones.Count>0; }}

	public bool HasBuoyancy { get{
		foreach(SurfableZone z in occupiedSurfZones){
			if(z.HasBuoyancy){ return true; }
		}
		return false;
	}}

	public bool Throwing { get{ return throwingProgress>0 && throwingProgress<.5f; }}
	public bool Returning { get{ return throwingProgress>=.5f; }}

	public float AimingSwim { get{ return Math.Abs(aim.AngleTo(speed));}}

	public float angleDelta { get{ return Mathf.Clamp(surfAim.AngleTo(aim)*.5f,-(float)Math.PI*.08f,(float)Math.PI*.08f); } }

	private TraversalState moveState;

	private Vector2 speed;

	private SurfController controller;
	private SurfBoard board;

	private float throwingProgress = 0;

	private Tween tweenThrow;

	private TraversalState GetMoveState(){
		if(SwimReady && surf && !Throwing){
			var aimSwim = AimingSwim;
			if(aimSwim<Mathf.Deg2Rad(swimAccuracy)){
				return TraversalState.SWIMMING;
			}else if(aimSwim<Mathf.Deg2Rad(grindAccuracy)){
				return TraversalState.GRINDING;
			}
			return TraversalState.DRAGGING;
		}

		if(SwimReady){
			return TraversalState.SUBMERGED;
		}
		if(TestMove(Transform,new Vector2(0,1))){
			return TraversalState.LAND;
		}
		return TraversalState.AIRBORNE;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		controller = GetNode<SurfController>(new NodePath("./SurfController"));
		board = GetNode<SurfBoard>(new NodePath("./SurfBoard"));
		tweenThrow = GetNode<Tween>(new NodePath("./TweenThrow"));
		controller.OnInput += OnInput;
	}

	public override void _ExitTree()
	{
		controller.OnInput -= OnInput;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if(!Throwing && !Returning){
			board.Holding(surfAim,angleSpinVel,delta);
		}
	}

	public void EnterSurfZone(SurfableZone zone){
		if(occupiedSurfZones.Contains(zone)){return;}
		occupiedSurfZones.Add(zone);
	}
	public void ExitSurfZone(SurfableZone zone){
		if(!occupiedSurfZones.Contains(zone)){return;}
		occupiedSurfZones.Remove(zone);
	}

	public override void _PhysicsProcess(float delta){
		if(!surf){
			if(!HasBuoyancy){
				surfAim = surfAim.Rotated(angleSpinVel);
			}else{
				angleSpinVel *= .92f;
			}
		}else{
			var surfAimLerp = surfAim.LinearInterpolate(aim,delta*15f);
			
			surfAim = surfAimLerp*aim.Length();
		}

		if(Returning){
			board.Returning(GlobalPosition,(throwingProgress-.5f)*2f);
		}


		moveState = GetMoveState();

		if(moveState == TraversalState.LAND || moveState == TraversalState.AIRBORNE || !HasBuoyancy){
			//give it gravity
			speed += Vector2.Down*gravity*delta;
		}else{
			//give it buoyancy
			speed += Vector2.Up*buoyancy*delta;
			if(moveState == TraversalState.SUBMERGED){
				speed *= .92f;
			}
		}


		ProcessSwimming(delta);

		if(speed.Length()>maxSpeed){
			speed = speed.Normalized()*maxSpeed;
		}

		speed = MoveAndSlide(speed);
	}

	private void ProcessSwimming(float delta){
		switch(moveState){
			case TraversalState.SWIMMING:
			tweenBoost = Mathf.Lerp(tweenBoost,swimBoost,delta*tweenTraversalStateSpeed*.5f);
			tweenTraction = Mathf.Lerp(tweenTraction,swimTraction,delta*tweenTraversalStateSpeed*1.4f);
			speed = speed.LinearInterpolate(surfAim.Normalized()*speed.Length()*(1f+tweenBoost/100f),tweenTraction);
			break;
			case TraversalState.GRINDING:
			tweenBoost = Mathf.Lerp(tweenBoost,grindBoost,delta*tweenTraversalStateSpeed*2f);
			tweenTraction = Mathf.Lerp(tweenTraction,grindTraction,delta*tweenTraversalStateSpeed*2f);
			speed = speed.LinearInterpolate(surfAim.Normalized()*speed.Length()*(1f+tweenBoost/100f),tweenTraction);
			break;
			case TraversalState.DRAGGING:
			tweenBoost = Mathf.Lerp(tweenBoost,draggingBoost,delta*tweenTraversalStateSpeed*5f);
			tweenTraction = Mathf.Lerp(tweenTraction,draggingTraction,delta*tweenTraversalStateSpeed*5f);
			speed = speed.LinearInterpolate(surfAim.Normalized()*speed.Length()*(1f+tweenBoost/100f),tweenTraction);
			break;
		}
		
		
	}


	private void Throw(){
		if(tweenThrow.IsActive()){return;}
		GD.Print("Throw()");
		TweenThrow();
	}


	private async void TweenThrow(){
		throwingOrigin = GlobalPosition;
		throwingDirection = aim.Normalized()*throwingSpeed;
		board.Thrown(this,throwingDirection);
		GD.Print("Throw Start!");
		tweenThrow.InterpolateProperty(this,"throwingProgress",0f,1f,throwDuration/2f,Tween.TransitionType.Sine,Tween.EaseType.OutIn);
		tweenThrow.Start();
		await ToSignal(tweenThrow,"tween_completed");
		GD.Print("Throw complete!");
		throwingProgress = 0;
	}


	private void OnInput(SurfController.Gamepad gamepad){
		if(surf && !gamepad.surf){
			angleSpinVel = angleDelta;
		}
		move = gamepad.move;
		surf = gamepad.surf;
		aim = gamepad.aim;
		if(aim.Length()==0){
			aim = surfAim.Normalized();
		}
		aim = aim.Normalized();
		if(gamepad.pressedThrow){
			//Throw();
		}
		
	}

	public enum TraversalState{
		AIRBORNE,
		LAND,
		SUBMERGED,
		SWIMMING,
		GRINDING,
		DRAGGING
	}
}
