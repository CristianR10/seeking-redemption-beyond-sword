using Godot;

public partial class AreaSoul : Area2D
{
	private AnimatedSprite2D _animator;
	private AnimationController _animation;

	public enum SoulState
	{
		IdleSoul
	}

	private SoulState StatusSoul;

	public override void _Ready()
	{
		_animator =
			GetNode<AnimatedSprite2D>(
				"AnimatedSprite2D"
			);

		_animation =
			new AnimationController(_animator);

		StatusSoul = SoulState.IdleSoul;

		_animation.Play("IdleSoul");

		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Amos)
		{
			QueueFree();
		}
	}
}