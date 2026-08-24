using Godot;

public partial class Amos : CharacterBody2D
{
	public const float Speed = 200.0f;

	private AnimatedSprite2D _animator;

	private readonly MovementController _movement = new();
	private readonly JumpController _jump = new();
	private readonly WallController _wall = new();

	private string _currentAnim = "Idle";

	public enum PlayerState
	{
		Idle,
		Walk,
		Jump,
		Down,
		Wall,
		WallTop
	}

	private PlayerState Status;

	private void PlayAnimation(string animName)
	{
		if (_currentAnim == animName)
			return;

		if (_animator.SpriteFrames != null &&
			_animator.SpriteFrames.HasAnimation(animName))
		{
			_animator.Play(animName);

			_currentAnim = animName;
		}
		else
		{
			GD.Print(
				$"Animação '{animName}' Não encontrada no AnimatedSprite2D"
			);
		}
	}

	public void GoToIdleState()
	{
		Status = PlayerState.Idle;

		PlayAnimation("Idle");
	}

	private void IdleState(float delta)
	{
		Velocity =
			_movement.HandleGroundMovement(
				Velocity,
				delta
			);

		PlayAnimation("Idle");

		if (Mathf.Abs(Velocity.X) > 1.0f)
		{
			Status = PlayerState.Walk;
		}
	}

	private void WalkState(float delta)
	{
		Velocity =
			_movement.HandleGroundMovement(
				Velocity,
				delta
			);

		PlayAnimation("Walk");

		float inputX =
			_movement.GetInputDirection();

		if (inputX != 0)
		{
			_animator.FlipH =
				inputX < 0;
		}

		if (Mathf.Abs(Velocity.X) <= 1.0f)
		{
			Status = PlayerState.Idle;
		}
	}

	private void JumpState(float delta)
	{
		Velocity =
			_movement.HandleAirMovement(
				Velocity,
				delta
			);

		PlayAnimation("Jump");

		if (Velocity.Y > 0)
		{
			Status = PlayerState.Down;
		}
	}

	private void DownState(float delta)
	{
		Velocity =
			_movement.HandleAirMovement(
				Velocity,
				delta
			);

		PlayAnimation("Down");

		if (IsOnFloor())
		{
			Status =
				Mathf.Abs(Velocity.X) > 1.0f
					? PlayerState.Walk
					: PlayerState.Idle;
		}
	}

	private void WallState(float delta)
	{
		/*
         * Se chegou ao topo, muda para WallTop.
         */
		if (_wall.IsAtTop)
		{
			Status = PlayerState.WallTop;

			return;
		}

		/*
         * Movimento normal da parede.
         */
		Velocity =
			_wall.HandleWallMovement(
				Velocity
			);

		PlayAnimation("Wall");

		_animator.FlipH =
			_wall.ShouldFlipSprite();

		/*
         * Se perdeu a parede,
         * volta para o estado de queda.
         */
		if (!_wall.IsAttached)
		{
			Status = PlayerState.Down;

			return;
		}

		/*
         * Salto normal da parede.
         */
		if (Input.IsActionJustPressed("Jump_Space"))
		{
			Velocity =
				_wall.GetWallJumpVelocity();

			_animator.FlipH =
				Velocity.X < 0;

			Status = PlayerState.Jump;
		}
	}

	private void WallTopState(float delta)
	{
		PlayAnimation("WallTop");

		/*
		 * Fica completamente parado.
		 *
		 * Mesmo segurando UI_UP.
		 */
		Velocity = Vector2.Zero;

		/*
		 * Só sai daqui quando apertar Jump_Space.
		 */
		if (_wall.ShouldTopJump())
		{
			Velocity = _wall.GetTopJumpVelocity();

			/*
			 * A direção visual acompanha o input
			 * usado no salto.
			 */
			if (Velocity.X < 0)
			{
				_animator.FlipH = true;
			}
			else if (Velocity.X > 0)
			{
				_animator.FlipH = false;
			}

			Status = PlayerState.Jump;
		}
	}

	private void UpdateState()
	{
		/*
         * Topo da parede tem prioridade.
         */
		if (_wall.IsAtTop)
		{
			Status = PlayerState.WallTop;

			return;
		}

		/*
         * Parede.
         */
		if (_wall.IsAttached)
		{
			Status = PlayerState.Wall;

			return;
		}

		/*
         * Chão.
         */
		if (IsOnFloor())
		{
			Status =
				Mathf.Abs(Velocity.X) > 1.0f
					? PlayerState.Walk
					: PlayerState.Idle;

			return;
		}

		/*
         * Ar.
         */
		if (Velocity.Y < 0)
		{
			Status = PlayerState.Jump;
		}
		else
		{
			Status = PlayerState.Down;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		_wall.Update(this, dt);

		/*
		 * Chegou ao topo.
		 */
		if (_wall.IsAtTop)
		{
			Status = PlayerState.WallTop;

			WallTopState(dt);

			MoveAndSlide();

			return;
		}

		/*
		 * Está escalando.
		 */
		if (_wall.IsAttached)
		{
			Status = PlayerState.Wall;

			WallState(dt);

			MoveAndSlide();

			return;
		}

		/*
		 * Movimento normal.
		 */
		if (Input.IsActionJustPressed("Jump_Space"))
		{
			_jump.PressJump();
		}

		Vector2 vel = Velocity;

		vel = _jump.Update(
			vel,
			IsOnFloor(),
			Input.IsActionJustReleased("Jump_Space"),
			dt
		);

		Velocity = vel;

		UpdateState();

		switch (Status)
		{
			case PlayerState.Idle:
				IdleState(dt);
				break;

			case PlayerState.Walk:
				WalkState(dt);
				break;

			case PlayerState.Jump:
				JumpState(dt);
				break;

			case PlayerState.Down:
				DownState(dt);
				break;
		}

		MoveAndSlide();
	}

	public override void _Ready()
	{
		_animator =
			GetNode<AnimatedSprite2D>(
				"AnimatedSprite2D"
			);

		RayCast2D leftRay =
			GetNode<RayCast2D>(
				"LeftWallDetected"
			);

		RayCast2D rightRay =
			GetNode<RayCast2D>(
				"RightWallDetected"
			);

		RayCast2D topRayRight =
			GetNode<RayCast2D>(
				"TopRayRight"
			);

		RayCast2D topRayLeft =
			GetNode<RayCast2D>(
				"TopRayLeft"
			);

		_wall.Initialize(
			leftRay,
			rightRay,
			topRayRight,
			topRayLeft
		);

		GoToIdleState();
	}
}