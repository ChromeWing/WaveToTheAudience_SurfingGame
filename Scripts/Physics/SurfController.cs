using Godot;
using System;

public class SurfController : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private Gamepad _gamepad;

	public System.Action<Gamepad> OnInput;

	private Gamepad gamepad {get{
		if(_gamepad==null){
			_gamepad = new Gamepad(this);
		}
		return _gamepad;
	}}
	private float boardAngle;

	public class Gamepad{

		private Node parent;

		public Gamepad(Node node){
			parent = node;
		}

		public enum Mode{
			GAMEPAD,
			MOUSE_AND_KEYBOARD
		}

		public Mode currentMode = Mode.MOUSE_AND_KEYBOARD;

		public Vector2 move;
		public Vector2 aim;

		public bool surf;
		public bool pressedThrow;
		public bool pressedTrickDown,pressedTrickLeft,pressedTrickRight,pressedTrickUp; 

		public void Update(){
			switch(currentMode){
				case Mode.MOUSE_AND_KEYBOARD:
				UpdateMouseKeyboard();
				break;
				case Mode.GAMEPAD:
				UpdateGamepad();
				break;
			}
		}

		private void UpdateMouseKeyboard(){
			var pos = parent.GetViewport().GetMousePosition();
			var halfScreen = parent.GetViewport().Size * .5f;
			var screenDist = Math.Min(halfScreen.x,halfScreen.y)*.75f;
			var direction = pos - halfScreen;
			aim = Math.Min(direction.Length(),screenDist)/screenDist * direction.Normalized();
			aim = aim.Normalized();

			move = new Vector2(Input.GetAxis("p1_move_left","p1_move_right"),Input.GetAxis("p1_move_up","p1_move_down"));
			surf = Input.IsActionPressed("p1_action_surf");
			pressedThrow = Input.IsActionJustPressed("p1_action_throw");
			pressedTrickLeft = Input.IsActionJustPressed("p1_trick_left");
			pressedTrickRight = Input.IsActionJustPressed("p1_trick_right");
			pressedTrickUp = Input.IsActionJustPressed("p1_trick_up");
			pressedTrickDown = Input.IsActionJustPressed("p1_trick_down");
		}

		private void UpdateGamepad(){
			aim = new Vector2(Input.GetAxis("p1_aim_left","p1_aim_right"),Input.GetAxis("p1_aim_up","p1_aim_down"));
			move = new Vector2(Input.GetAxis("p1_move_left","p1_move_right"),Input.GetAxis("p1_move_up","p1_move_down"));

			if(move.Length()>.2f){
				var left = -Input.GetActionRawStrength("p1_move_left");
				var right = Input.GetActionRawStrength("p1_move_right");
				var up = -Input.GetActionRawStrength("p1_move_up");
				var down = Input.GetActionRawStrength("p1_move_down");
				move = new Vector2(left+right,up+down);
			}
			aim = move;
			
			surf = Input.IsActionPressed("p1_action_surf");
			pressedThrow = Input.IsActionJustPressed("p1_action_throw");
			
			pressedTrickLeft = Input.IsActionJustPressed("p1_trick_left");
			pressedTrickRight = Input.IsActionJustPressed("p1_trick_right");
			pressedTrickUp = Input.IsActionJustPressed("p1_trick_up");
			pressedTrickDown = Input.IsActionJustPressed("p1_trick_down");
		}

		public void UsingGamepad(){
			currentMode = Mode.GAMEPAD;
		}
		public void UsingMouseKeyboard(){
			currentMode = Mode.MOUSE_AND_KEYBOARD;
		}

	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	public override void _Input(InputEvent @event){
		if(@event is InputEventJoypadButton || @event is InputEventJoypadButton){
			gamepad.UsingGamepad();
		}else if(@event is InputEventMouseButton || @event is InputEventKey){
			gamepad.UsingMouseKeyboard();
		}
		
		gamepad.Update();

		OnInput?.Invoke(gamepad);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		
	}
}
