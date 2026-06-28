using Godot;
using System;

public partial class Amos : CharacterBody2D
{
	public const float Speed = 200.0f;
	public const float JumpForce = -300.0f;

	const float Gravity = 1200f;

	const float Acceleration = 5.0f;

	private AnimatedSprite2D _animator;

	const float Friction = 8.0f;

	private string _currentAnim = "Idle";

	public enum PlayerState
	{
		Idle, Walk, Jump, Down
	}

	private PlayerState Status;

	private void PlayAnimation(string animName)
	{
		// Evita reiniciar a animação se ela já estiver tocando
		if (_currentAnim == animName) return;

		if (_animator.SpriteFrames != null && _animator.SpriteFrames.HasAnimation(animName))
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

	public void IdleState()
	{
		Velocity = HandleHorizontalMovement(Velocity, (float)GetPhysicsProcessDeltaTime());
		PlayAnimation("Idle"); // ADICIONADO

		if (Mathf.Abs(Velocity.X) > 1.0f)
		{
			Status = PlayerState.Walk;
		}
	}

	public void WalkState()
	{
		Velocity = HandleHorizontalMovement(Velocity, (float)GetPhysicsProcessDeltaTime());
		PlayAnimation("Walk"); // ADICIONADO

		// Espelha o sprite baseado na direção do movimento
		float inputX = Input.GetAxis("ui_left", "ui_right");
		if (inputX > 0) _animator.FlipH = false;
		else if (inputX < 0) _animator.FlipH = true;

		if (Mathf.Abs(Velocity.X) <= 1.0f)
		{
			Status = PlayerState.Idle;
		}
	}

	public void JumpState()
	{
		Velocity = HandleAirMovement(Velocity, (float)GetPhysicsProcessDeltaTime());
		PlayAnimation("Jump"); // ADICIONADO

		if (Velocity.Y > 0)
			Status = PlayerState.Down;
	}

	public void DownState()
	{
		Velocity = HandleAirMovement(Velocity, (float)GetPhysicsProcessDeltaTime());
		PlayAnimation("Down"); // ADICIONADO (ou "Jump" se não tiver animação de queda)

		if (IsOnFloor())
		{
			Status = Mathf.Abs(Velocity.X) > 1.0f ? PlayerState.Walk : PlayerState.Idle;
		}
	}

	private Vector2 HandleHorizontalMovement(Vector2 vel, float delta)
	{
		float inputX = Input.GetAxis("ui_left", "ui_right");

		if (inputX != 0)
		{
			vel.X = Mathf.Lerp(vel.X, inputX * Speed, Acceleration * delta);
		}
		else
		{
			vel.X = Mathf.Lerp(vel.X, 0f, Friction * delta);
		}

		if (Input.IsActionJustPressed("ui_up") && IsOnFloor())
		{
			vel.Y = JumpForce;
			Status = PlayerState.Jump;
		}

		return vel;
	}

	private Vector2 HandleAirMovement(Vector2 vel, float delta)
	{
		float inputX = Input.GetAxis("ui_left", "ui_right");

		if (inputX != 0)
		{
			vel.X = Mathf.Lerp(vel.X, inputX * Speed, Acceleration * 0.2f * delta);
			_animator.FlipH = inputX < 0; // Espelha no ar também
		}
		else
		{
			vel.X = Mathf.Lerp(vel.X, 0f, Friction * 0.05f * delta);
		}

		return vel;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 vel = Velocity;

		if (!IsOnFloor())
		{
			vel.Y += Gravity * (float)delta;

			// AJUSTE DE SEGURANÇA: Se o jogador cair de uma plataforma sem pular
			if (Status != PlayerState.Jump && Status != PlayerState.Down)
			{
				Status = PlayerState.Down;
			}
		}
		else if (Status != PlayerState.Jump)
		{
			vel.Y = 0;
		}

		Velocity = vel;

		switch (Status)
		{
			case PlayerState.Idle: IdleState(); break;
			case PlayerState.Walk: WalkState(); break;
			case PlayerState.Jump: JumpState(); break;
			case PlayerState.Down: DownState(); break;
		}

		MoveAndSlide();
	}

	public override void _Ready()
	{
		_animator = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		GoToIdleState();
	}
}
