using System.ComponentModel;
using Godot;

namespace Demo.Player;

public enum EBehaviorState
{
	Null,
	Hoeing,
	ChopTree,
	Water,
}

public partial class Player : CharacterBody2D
{
	[Export]
	private AnimationTree _animationTree;
	[Export]
	private float MoveSpeed {get; set;} = 100.0f;
	[Export]
	private Sprite2D _sprite;
	[Export]
	private EBehaviorState _behaviorState;

    private Vector2 _direction;
	private Vector2 _lastDirection = Vector2.Down;
	private Vector2 _currentBehaviorDirection;
	private AnimationNodeStateMachinePlayback _stateMachinePlayback;
	private StringName _currentBehaviorState;

	public override void _Ready()
	{
		_stateMachinePlayback = _animationTree.Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();
		_animationTree.AnimationFinished += OnAnimationFinished;
	}

	public override void _PhysicsProcess(double delta)
	{
		CurrentBehaviorState(_behaviorState);

		bool isCurrentlyHoeing = _animationTree.Get($"parameters/conditions/Is{_currentBehaviorState}").AsBool();

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

		bool isHoeing = Input.IsActionJustPressed("Hoeing");
		if (isHoeing && !isCurrentlyHoeing)
		{
            _animationTree.Set("parameters/conditions/IsIdle", false);
			_animationTree.Set("parameters/conditions/IsRunning", false);
			_animationTree.Set($"parameters/conditions/Is{_currentBehaviorState}", true);

			if (_direction != Vector2.Zero)
			{
				_currentBehaviorDirection = _direction;
			}
			else
			{
				_currentBehaviorDirection = _lastDirection;
			}
			_animationTree.Set($"parameters/{_currentBehaviorState}/blend_position", _currentBehaviorDirection);
		}
	}

	public override void _ExitTree()
	{
		_animationTree.AnimationFinished -= OnAnimationFinished;
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (animationName.ToString().Contains(_currentBehaviorState))
		{
            _animationTree.Set($"parameters/conditions/Is{_currentBehaviorState}", false);
            _animationTree.Set("parameters/PlayerIdle/blend_position", _lastDirection);
            _stateMachinePlayback.Travel("PlayerIdle");
        }
	}

	private void CurrentBehaviorState(EBehaviorState behaviorState)
	{
		switch (behaviorState)
		{
			case EBehaviorState.Null:
				break;
			case EBehaviorState.Hoeing:
				_currentBehaviorState = "Hoeing";
				break;
			case EBehaviorState.ChopTree:
				_currentBehaviorState = "ChopTree";
				break;
			case EBehaviorState.Water:
				_currentBehaviorState = "Water";
				break;
		}
	}
}
