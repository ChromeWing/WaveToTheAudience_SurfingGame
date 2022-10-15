using Godot;

public class TrickPerformer : Node2D{
	
	[Export]
	public float boostTrickDuration = .7f;

	[Export]
	public float trickBoostSpeed = 500;

	private SurfPlayer parent;

	private Tween boostTrickTween;

	private float boostTrickProgress;
	private bool jumped;


	public override void _Ready()
	{
		parent = this.GetParent() as SurfPlayer;
		boostTrickTween = GetNode(new NodePath("./BoostTween")) as Tween;
		parent.promptedTrick += OnPromptedTrick;
		parent.startedSurfing += OnStartedSurfing;
	}

	public override void _ExitTree()
	{
		parent.promptedTrick -= OnPromptedTrick;
	}



	private void OnPromptedTrick(TrickType type){
		if(!parent.Airborne){return;}
		if(boostTrickTween.IsActive()){return;}
		switch(type){
			case TrickType.LEFT:
			PerformBoostTrick(Vector2.Left*trickBoostSpeed);
			break;
			case TrickType.RIGHT:
			PerformBoostTrick(Vector2.Right*trickBoostSpeed);
			break;
			case TrickType.UP:
			if(jumped){break;}
			jumped = true;
			PerformBoostTrick(Vector2.Up*trickBoostSpeed*2f);
			break;
			case TrickType.DOWN:
			PerformBoostTrick(Vector2.Down*trickBoostSpeed*.5f);
			break;
		}
	}

	private void OnStartedSurfing(){
		jumped = false;
	}

	private void PerformBoostTrick(Vector2 force){
		parent.PushPlayer(force);
		GD.Print("BoostTrick()");
		BoostTrickRoutine();
	}

	private async void BoostTrickRoutine(){
		boostTrickTween.InterpolateProperty(this,"boostTrickProgress",0f,1f,boostTrickDuration,Tween.TransitionType.Sine,Tween.EaseType.OutIn);
		boostTrickTween.Start();
		await ToSignal(boostTrickTween,"tween_completed");
	}

}