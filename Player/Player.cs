using System.ComponentModel;
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

    private Vector2 _direction;
	private Vector2 _lastDirection = Vector2.Down;
	private Vector2 _hoeingDirection;
	private AnimationNodeStateMachinePlayback _stateMachinePlayback;
	private int _hoeingFrameCount = 0;
	private double _hoeingStartTime = 0;
	private const double MAX_HOEING_DURATION = 0.53; // 最大允许耕地时间（秒）

	public override void _Ready()
	{
		_stateMachinePlayback = _animationTree.Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();
		_animationTree.AnimationFinished += OnAnimationFinished;
	}

	public override void _PhysicsProcess(double delta)
	{
		bool isCurrentlyHoeing = _animationTree.Get("parameters/conditions/IsHoeing").AsBool();

        // 耕地状态检测和卡住恢复
        if (isCurrentlyHoeing) 
		{
			_hoeingFrameCount++;
			
			// 基于时间的检测（更准确）
			double currentTime = Time.GetTicksMsec();
			double hoeingDuration = (currentTime - _hoeingStartTime) / 1000.0;
			
			// 如果耕地时间超过最大允许时间，强制退出
			if (hoeingDuration > MAX_HOEING_DURATION)
			{
				GD.Print($"耕地动画卡住，强制退出。持续时间: {hoeingDuration:F2}秒，帧数: {_hoeingFrameCount}");
				ForceExitHoeingState();
			}
			// 基于帧数的备用检测
			else if (_hoeingFrameCount > 180) // 约3秒（60fps × 3）
			{
				GD.Print($"耕地动画可能卡住。帧数: {_hoeingFrameCount}");
				ForceExitHoeingState();
			}
        }

        if (!isCurrentlyHoeing && _sprite.Visible)
		{
			// 重置耕地状态计数器
			_hoeingFrameCount = 0;
			_hoeingStartTime = 0;
			_animationTree.Set("parameters/conditions/IsHoeing", false);
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
			// 记录耕地开始时间
			_hoeingStartTime = Time.GetTicksMsec();
			_hoeingFrameCount = 0;
			
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

	public override void _ExitTree()
	{
		_animationTree.AnimationFinished -= OnAnimationFinished;
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (animationName.ToString().Contains("Hoeing"))
		{
			// 正常完成耕地动画
			double hoeingDuration = (Time.GetTicksMsec() - _hoeingStartTime) / 1000.0;
			GD.Print($"耕地动画正常完成。持续时间: {hoeingDuration:F2}秒，帧数: {_hoeingFrameCount}");
			
            _animationTree.Set("parameters/conditions/IsHoeing", false);
			_animationTree.Set("parameters/conditions/IsIdle", true);
            _animationTree.Set("parameters/PlayerIdle/blend_position", _lastDirection);
		}
	}

	private void ForceExitHoeingState()
	{
		// 强制退出耕地状态
		_animationTree.Set("parameters/conditions/IsHoeing", false);
		_animationTree.Set("parameters/conditions/IsIdle", true);
		_hoeingFrameCount = 0;
		_hoeingStartTime = 0;
		
		// 添加额外恢复逻辑
		CallDeferred(MethodName.ResetAnimationState);
	}

	private void ResetAnimationState()
	{
		// 确保动画状态完全重置
		_animationTree.Active = false;
		_animationTree.Active = true;
	}
}
