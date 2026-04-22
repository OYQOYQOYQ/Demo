using Godot;

namespace Demo.Core.Player;

public enum BehaviorState
{
    Null,
    Hoeing,
    ChopTree,
    Water,
}

public partial class Player : CharacterBody2D
{
    [ExportGroup("Attribute")]
    // 玩家移动属性
    [Export] private float _moveSpeed = 80.0f;
    private Vector2 _direction;
    private Vector2 _lastDirection = Vector2.Down;
    // 玩家行为属性
    [Export] private BehaviorState _currentBehaviorState;
    private StringName _behaviorState;
    private Vector2 _currentBehaviorDirection;

    [ExportGroup("NodeInitialization")]
    // 玩家动画属性
    [Export] private Sprite2D _moveSprite;
    [Export] private Area2D _occlusionDetecion;
    [Export] private AnimationTree _animationTree;
    private AnimationNodeStateMachinePlayback _stateMachinePlayback;

    private Node2D _obj;

    public override void _Ready()
    {
        _stateMachinePlayback = _animationTree.Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();
        _animationTree.AnimationFinished += OnAnimationFinished;
        _occlusionDetecion.BodyEntered += OnBodyEntered;
        _occlusionDetecion.BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        _behaviorState = GetCurrentBehaviorState(_currentBehaviorState);

        bool isCurrentlyHoeing = _animationTree.Get($"parameters/conditions/Is{_currentBehaviorState}").AsBool();

        if (!isCurrentlyHoeing && _moveSprite.Visible)
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
                Velocity = _direction * _moveSpeed;
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
        _occlusionDetecion.BodyEntered -= OnBodyEntered;
        _occlusionDetecion.BodyExited -= OnBodyExited;
    }

    private void OnAnimationFinished(StringName animationName)
    {
        if (animationName.ToString().Contains(_behaviorState))
        {
            _animationTree.Set($"parameters/conditions/Is{_currentBehaviorState}", false);
            _animationTree.Set("parameters/PlayerIdle/blend_position", _lastDirection);
            _stateMachinePlayback.Travel("PlayerIdle");
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if ((bool)body.GetParent().GetMeta("Transparent"))
        {
            _obj = body.GetParent() as Node2D;
            _obj.Modulate = new Color(1, 1, 1, 0.5f);
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if ((bool)body.GetParent().GetMeta("Transparent"))
        {
            _obj = body.GetParent() as Node2D;
            _obj.Modulate = new Color(1, 1, 1, 1);
        }
    }

    private StringName GetCurrentBehaviorState(BehaviorState behaviorState)
    {
        switch (behaviorState)
        {
            case BehaviorState.Null:
                return "";
            case BehaviorState.Hoeing:
                return "Hoeing";
            case BehaviorState.ChopTree:
                return "ChopTree";
            case BehaviorState.Water:
                return "Water";
            default:
                return "";
        }
    }
}
