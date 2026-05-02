using Godot;

namespace Demo.Core.NaturalResource.Harvestable;

public partial class Harvestable: Node2D
{
    [Export] public PackedScene HarvestScene;
    public virtual int MaxHarvestCount { get; set; } = 6;  // 默认需要被工具开采几次

    public override void _ExitTree() 
    { 
        Node2D harvestScene = (Node2D)HarvestScene.Instantiate();
        harvestScene.GlobalPosition = GlobalPosition;
        GetParent().CallDeferred("add_child", harvestScene);
    }

    public virtual void Playing() 
    {
        MaxHarvestCount--;
        PlayHitShake();

        if (MaxHarvestCount <= 0)
        {
            QueueFree();
        }
    }

    public virtual void PlayHitShake() 
    { 

    }
}
