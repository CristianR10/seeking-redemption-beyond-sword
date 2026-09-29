using Godot;
using System.Linq;

public partial class GlobalBars : Control
{
	private ProgressBar _progressBar;

	private int _totalSouls;
	private float _soulValue;

	[Export]
	private float DrainSpeed = 0.5f;

	public override void _Ready()
	{
		AddToGroup("global_bars");

		_progressBar = GetNode<ProgressBar>("ProgressBar");

		InitializeSouls();

		_progressBar.Value = 100;
	}

	public override void _Process(double delta)
	{
		if (_progressBar.Value > 0)
		{
			_progressBar.Value -= DrainSpeed * (float)delta;

			if (_progressBar.Value < 0)
			{
				_progressBar.Value = 0;
			}
		}
	}

	private void InitializeSouls()
	{
		AreaSoul[] souls = GetTree()
			.GetNodesInGroup("area_soul")
			.OfType<AreaSoul>()
			.ToArray();

		_totalSouls = souls.Length;

		if (_totalSouls > 0)
		{
			_soulValue = 100.0f / _totalSouls;
		}
		else
		{
			_soulValue = 0;
		}

		GD.Print("Total de almas: ", _totalSouls);
		GD.Print("Valor de cada alma: ", _soulValue);
	}

	public void SoulCollected()
	{
		_progressBar.Value += _soulValue;

		if (_progressBar.Value > 100)
		{
			_progressBar.Value = 100;
		}

		GD.Print("Alma coletada!");
		GD.Print("Valor da barra: ", _progressBar.Value);
	}
}