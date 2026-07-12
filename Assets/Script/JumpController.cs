using Godot;
/// Responsável por toda a física do pulo.


public class JumpController
{
    /// Altura máxima do pulo em pixels.
    /// 64 = 4 tiles de 16px.
    public float JumpHeight = 64f;
    /// Tempo até atingir o topo do pulo.
    /// Quanto menor, mais "seco" será o pulo.    
    public float TimeToPeak = 0.35f;
    /// Tempo para cair do topo até o chão.
    /// Menor = queda mais rápida.
    public float TimeToFall = 0.25f;
    /// Tempo após sair da plataforma que ainda é permitido pular.
    public float CoyoteTime = 0.12f;
    /// Se apertar antes de tocar no chão, ele pula automaticamente.    
    public float JumpBuffer = 0.12f;

    // Cáculos Automáticos
    public float JumpVelocity =>
    -(2f * JumpHeight) / TimeToPeak;
    public float GravityUp =>
        2f * JumpHeight / (TimeToPeak * TimeToPeak);

    public float GravityDown =>
        2f * JumpHeight / (TimeToFall * TimeToFall);

    // Timers
    private float _coyoteTimer;
    private float _jumpBufferTimer;

    // Deve ser chamado quando o jogador aperta o botão de pulo.
    public void PressJump()
    {
        _jumpBufferTimer = JumpBuffer;
    }

    // Atualiza toda a física do pulo.
    public Vector2 Update(
        Vector2 velocity,
        bool onFloor,
        bool jumpReleased,
        float delta
    )
    {
        // Updating TImers
        if (onFloor)
            _coyoteTimer = CoyoteTime;
        else
            _coyoteTimer -= delta;

        _jumpBufferTimer -= delta;

        // Exe the jump
        if (_jumpBufferTimer > 0 && _coyoteTimer > 0)
        {
            velocity.Y = JumpVelocity;

            _jumpBufferTimer = 0;
            _coyoteTimer = 0;
        }

        // Buffer Jump
        if (!onFloor)
        {
            if (velocity.Y < 0)
                velocity.Y += GravityUp * delta;
            else
                velocity.Y += GravityDown * delta;

        }

        // Variable Jump
        if (jumpReleased && velocity.Y < 0)
        {
            velocity.Y *= 0.5f;
        }

        return velocity;
    }
}
