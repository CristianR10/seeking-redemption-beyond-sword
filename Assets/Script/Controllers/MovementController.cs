using Godot;

public class MovementController
{
	private const float Speed = 200.0f;
	private const float Acceleration = 5.0f;
	private const float Friction = 8.0f;

	public Vector2 HandleGroundMovement(Vector2 velocity, float delta)
	{
		float inputX = GetInputDirection();

		if (inputX != 0)
		{
			velocity.X = Mathf.Lerp(
				velocity.X,
				inputX * Speed,
				Acceleration * delta
			);
		}
		else
		{
			velocity.X = Mathf.Lerp(
				velocity.X,
				0f,
				Friction * delta
			);
		}

		return velocity;
	}

	public Vector2 HandleAirMovement(Vector2 velocity, float delta)
	{
		float inputX = GetInputDirection();

		if (inputX != 0)
		{
			velocity.X = Mathf.Lerp(
				velocity.X,
				inputX * Speed,
				Acceleration * 0.2f * delta
			);
		}
		else
		{
			velocity.X = Mathf.Lerp(
				velocity.X,
				0f,
				Friction * 0.05f * delta
			);
		}

		return velocity;
	}

	public float GetInputDirection()
	{
		return Input.GetAxis("ui_left", "ui_right");
	}
}