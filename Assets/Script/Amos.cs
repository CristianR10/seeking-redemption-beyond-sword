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
		// Evita reiniciar a animação se ela já estiver tocando
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
			GD.Print($"Animação '{animName}' Não encontrada no AnimatedSprite2D");
		}
	}

	public void GoToIdleState()
	{
		Status = PlayerState.Idle;
		PlayAnimation("Idle");
	}

	private void IdleState(float delta)
	{
		Velocity = _movement.HandleGroundMovement(
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
		Velocity = _movement.HandleGroundMovement(
			Velocity,
			delta
		);

		PlayAnimation("Walk");

		// Espelha o sprite baseado na direção do movimento
		float inputX = _movement.GetInputDirection();

		if (inputX != 0)
		{
			_animator.FlipH = inputX < 0;
		}

		if (Mathf.Abs(Velocity.X) <= 1.0f)
		{
			Status = PlayerState.Idle;
		}
	}

	private void JumpState(float delta)
	{
		Velocity = _movement.HandleAirMovement(
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
		Velocity = _movement.HandleAirMovement(
			Velocity,
			delta
		);

		PlayAnimation("Down");

		if (IsOnFloor())
		{
			Status = Mathf.Abs(Velocity.X) > 1.0f
				? PlayerState.Walk
				: PlayerState.Idle;
		}
	}

	private void WallState(float delta)
	{
		// Chegou ao topo.
		if (_wall.IsAtTop)
		{
			Status = PlayerState.WallTop;
			return;
		}

		Velocity = _wall.HandleWallMovement(
			Velocity
		);

		PlayAnimation("Wall");

		// Olha para a parede.
		_animator.FlipH = _wall.ShouldFlipSprite();

		if (!_wall.IsAttached)
		{
			Status = PlayerState.Down;
			return;
		}

		// Wall Jump.
		if (Input.IsActionJustPressed("Jump_Space"))
		{
			Velocity = _wall.GetWallJumpVelocity();

			// Olha para o lado para onde está pulando.
			_animator.FlipH = Velocity.X < 0;

			Status = PlayerState.Jump;
		}
	}

	private void WallTopState(float delta)
	{
		PlayAnimation("WallTop");

		// Durante a transição, o personagem sobe
		// e passa pela quina suavemente.
		Velocity = _wall.HandleTopMovement(
			this,
			delta
		);

		// Olha para a direção da plataforma.
		_animator.FlipH = _wall.ShouldFlipSprite();

		if (_wall.IsTopTransitionFinished())
		{
			Velocity = Vector2.Zero;

			Status = PlayerState.Idle;
		}
	}

	private void UpdateState()
	{
		// Se chegou ao topo da parede, o estado WallTop tem prioridade.
		if (_wall.IsAtTop)
		{
			Status = PlayerState.WallTop;
			return;
		}

		// Se estiver grudado na parede, o estado Wall tem prioridade.
		if (_wall.IsAttached)
		{
			Status = PlayerState.Wall;
			return;
		}

		if (IsOnFloor())
		{
			Status = Mathf.Abs(Velocity.X) > 1.0f
				? PlayerState.Walk
				: PlayerState.Idle;

			return;
		}

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

		// Atualiza o controlador de parede antes
		// de decidir o estado.
		_wall.Update(this);

		// ============================================================
		// WALL TOP TRANSITION
		// ============================================================

		if (_wall.IsTransitioningTop)
		{
			Status = PlayerState.WallTop;

			WallTopState(dt);

			MoveAndSlide();

			return;
		}

		// ============================================================
		// WALL
		// ============================================================

		if (_wall.IsAttached)
		{
			Status = PlayerState.Wall;

			WallState(dt);

			MoveAndSlide();

			return;
		}

		// ============================================================
		// NORMAL MOVEMENT
		// ============================================================

		// Informa ao JumpController que o jogador pressionou
		// o botão de pulo.
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
		_animator = GetNode<AnimatedSprite2D>(
			"AnimatedSprite2D"
		);

		RayCast2D leftRay =
			GetNode<RayCast2D>("LeftWallDetected");

		RayCast2D rightRay =
			GetNode<RayCast2D>("RightWallDetected");

		RayCast2D topRay =
			GetNode<RayCast2D>("TopWallDetected");

		_wall.Initialize(
			leftRay,
			rightRay,
			topRay
		);

		GoToIdleState();
	}
}