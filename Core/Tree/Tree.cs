using Godot;

namespace Demo.Core.Tree;

public partial class Tree : Sprite2D
{
    [Export] private Marker2D WoodGeneratioPoint;
    public int HealthPoints = 6;

    public override void _ExitTree()
    {
        var woodScene = GD.Load<PackedScene>("res://Wood.tscn");
        Node2D wood = (Node2D)woodScene.Instantiate();
        wood.GlobalPosition = WoodGeneratioPoint.GlobalPosition;
        GetParent().CallDeferred("add_child", wood);
    }
}
