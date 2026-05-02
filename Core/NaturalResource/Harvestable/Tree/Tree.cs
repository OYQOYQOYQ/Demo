using Godot;

namespace Demo.Core.NaturalResource.Harvestable.Tree;

public partial class Tree : Harvestable
{
    private Tween _shakeTween;

    public override void PlayHitShake()
    {
        // 停止之前的摇晃
        _shakeTween?.Kill();

        // 创建摇晃动画
        _shakeTween = CreateTween();
        _shakeTween.SetParallel(true);

        // 左右摇晃
        _shakeTween.TweenProperty(this, "rotation_degrees", -5.0f, 0.05f);
        _shakeTween.TweenProperty(this, "rotation_degrees", 5.0f, 0.05f).SetDelay(0.05f);
        _shakeTween.TweenProperty(this, "rotation_degrees", -3.0f, 0.05f).SetDelay(0.1f);
        _shakeTween.TweenProperty(this, "rotation_degrees", 3.0f, 0.05f).SetDelay(0.15f);
        _shakeTween.TweenProperty(this, "rotation_degrees", 0.0f, 0.05f).SetDelay(0.2f);
    }
}
