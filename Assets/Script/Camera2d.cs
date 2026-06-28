using Godot;
using System;

public partial class Camera2d : Camera2D
{
	public Node2D Target;
	string Amos = "Amos";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetTarget();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Target != null)
		{
			GlobalPosition = Target.GlobalPosition;
		}
	}


	public void GetTarget()
	{
		var players = GetTree().GetNodesInGroup(Amos);

		if (players.Count > 0)
		{
			Target = (Node2D)players[0];			
		}
		else
		{
			GD.PrintErr("Aviso: Nenhum nó foi encontrado no grupo");
			return;
		}
	}
}
