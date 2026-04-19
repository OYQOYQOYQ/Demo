using Godot;

namespace Demo.Player;

public partial class Player : CharacterBody2D
{
	[Export]
	private AnimationTree _animationTree;
	[Export]
	private float MoveSpeed {get; set;} = 100.0f;

	private Vector2 _direction;
	private Vector2 _lastDirection = Vector2.Down;

	public override void _PhysicsProcess(double delta)
	{
#if DEBUG
		GD.Print(_direction);
#endif
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
}
