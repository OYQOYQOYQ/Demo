using Godot;

namespace Demo.Player;

public partial class Player : CharacterBody2D
{
	[Export]
	private AnimationTree _animationTree;
	[Export]
	private float MoveSpeed {get; set;} = 100.0f;
	[Export]
	private Sprite2D _sprite;
    [Export]
    private Sprite2D _sprite2;

    private Vector2 _direction;
	private Vector2 _lastDirection = Vector2.Down;
	private Vector2 _hoeingDirection;
	private AnimationNodeStateMachinePlayback _stateMachinePlayback;

	public override void _Ready()
	{
		_stateMachinePlayback = _animationTree.Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();
		_animationTree.AnimationFinished += OnAnimationFinished;
	}

	public override void _PhysicsProcess(double delta)
	{
		bool isCurrentlyHoeing = _animationTree.Get("parameters/conditions/IsHoeing").AsBool();

        if (isCurrentlyHoeing) 
		{
            Velocity = Vector2.Zero;
            MoveAndSlide();
            return;
        }

        if (!isCurrentlyHoeing && _sprite.Visible)
		{
			_direction = Input.GetVector("LeftMove", "RightMove", "ForwardMove", "BackMove");
			if (_direction != Vector2.Zero)
			{
				if (_direction != _lastDirection)
				{
					_lastDirection = _direction;
				}
				_animationTree.Set("parameters/conditions/IsIdle", false);
				_animationTree.Set("parameters/conditions/IsRunning", true);
				_animationTree.Set("parameters/PlayerRun/blend_position", _direction);
				Velocity = _direction * MoveSpeed;
			}
			else
			{
				Velocity = Vector2.Zero;
				_animationTree.Set("parameters/conditions/IsIdle", true);
				_animationTree.Set("parameters/conditions/IsRunning", false);
				_animationTree.Set("parameters/PlayerIdle/blend_position", _lastDirection);
			}
			MoveAndSlide();
		}

        bool IsHoeing = Input.IsActionJustPressed("Hoeing");
		if (IsHoeing && !isCurrentlyHoeing)
		{
            _animationTree.Set("parameters/conditions/IsIdle", false);
			_animationTree.Set("parameters/conditions/IsRunning", false);
			_animationTree.Set("parameters/conditions/IsHoeing", true);

			if (_direction != Vector2.Zero)
			{
				_hoeingDirection = _direction;
			}
			else
			{
				_hoeingDirection = _lastDirection;
			}
			_animationTree.Set("parameters/Hoeing/blend_position", _hoeingDirection);
		}
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (animationName.ToString().Contains("Hoeing"))
		{
            _animationTree.Set("parameters/conditions/IsHoeing", false);
			_animationTree.Set("parameters/conditions/IsIdle", true);
            _animationTree.Set("parameters/PlayerIdle/blend_position", _lastDirection);
		}
	}
}
