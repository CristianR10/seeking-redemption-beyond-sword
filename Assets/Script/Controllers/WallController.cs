using Godot;

public class WallController
{
    private const float WallClimbSpeed = 100.0f;

    private const float WallJumpForceX = 250.0f;
    private const float WallJumpForceY = -300.0f;

    // Velocidade da transição para o topo.
    private const float TopTransitionSpeed = 100.0f;

    // Distância que o personagem precisa avançar
    // para entrar na plataforma.
    private const float TopForwardDistance = 16.0f;

    private RayCast2D _leftRay;
    private RayCast2D _rightRay;
    private RayCast2D _topRay;

    public bool IsAttached { get; private set; }

    public bool IsAtTop { get; private set; }

    // Indica que o personagem está executando
    // a transição da parede para a plataforma.
    public bool IsTransitioningTop { get; private set; }

    // -1 = parede à esquerda
    //  1 = parede à direita
    //  0 = nenhuma parede
    public int WallDirection { get; private set; }

    private Vector2 _topStartPosition;

    private Vector2 _topTargetPosition;

    public void Initialize(
        RayCast2D leftRay,
        RayCast2D rightRay,
        RayCast2D topRay
    )
    {
        _leftRay = leftRay;
        _rightRay = rightRay;
        _topRay = topRay;

        if (_leftRay == null)
        {
            throw new System.ArgumentNullException(
                nameof(leftRay),
                "LeftWallDetected não foi encontrado."
            );
        }

        if (_rightRay == null)
        {
            throw new System.ArgumentNullException(
                nameof(rightRay),
                "RightWallDetected não foi encontrado."
            );
        }

        if (_topRay == null)
        {
            throw new System.ArgumentNullException(
                nameof(topRay),
                "TopWallDetected não foi encontrado."
            );
        }
    }

    public void Update(CharacterBody2D player)
    {
        // ============================================================
        // TRANSIÇÃO DO TOPO
        // ============================================================

        // Se já estamos fazendo a transição da quina,
        // NÃO fazemos mais nenhuma detecção da parede.
        //
        // Isso é muito importante.
        //
        // Os RayCast esquerdo/direito podem continuar
        // detectando ou deixar de detectar a parede.
        // Durante a transição isso não importa.
        if (IsAtTop)
        {
            return;
        }

        // ============================================================
        // CHÃO
        // ============================================================

        // No chão, não podemos ficar grudados na parede.
        if (player.IsOnFloor())
        {
            Release();
            return;
        }

        bool wallLeft = _leftRay.IsColliding();
        bool wallRight = _rightRay.IsColliding();

        // ============================================================
        // JÁ ESTÁ GRUDADO
        // ============================================================

        if (IsAttached)
        {
            // --------------------------------------------------------
            // PRIMEIRO verificamos o topo.
            // --------------------------------------------------------
            //
            // Isso precisa acontecer ANTES de verificar
            // se o RayCast lateral ainda está colidindo.
            //
            // Na quina é normal o RayCast lateral perder
            // a colisão.
            //
            if (Input.IsActionPressed("ui_up") &&
                _topRay.IsColliding())
            {
                IsAtTop = true;

                // NÃO damos Release().
                // NÃO entramos em Down.
                // NÃO zeramos a velocidade aqui.
                return;
            }

            // --------------------------------------------------------
            // PAREDE
            // --------------------------------------------------------

            bool stillOnWall =
                WallDirection == -1
                    ? wallLeft
                    : wallRight;

            if (stillOnWall)
            {
                // Continua normalmente grudado.
                return;
            }

            // --------------------------------------------------------
            // RAYCAST PERDEU A PAREDE
            // --------------------------------------------------------
            //
            // NÃO soltamos imediatamente.
            //
            // Isso evita o efeito:
            //
            // Wall -> Down -> Wall -> Down
            //
            // causado pela pequena diferença entre o collider
            // e o RayCast.
            //
            // Se o jogador estiver tentando subir e o TopRay
            // ainda não encontrou a quina, mantemos a parede.
            if (Input.IsActionPressed("ui_up"))
            {
                return;
            }

            // Se estiver descendo, também permitimos que
            // o movimento continue até chegar ao chão.
            if (Input.IsActionPressed("ui_down"))
            {
                return;
            }

            // Sem intenção de subir/descer e sem parede,
            // agora sim podemos soltar.
            Release();

            return;
        }

        // ============================================================
        // AINDA NÃO ESTÁ GRUDADO
        // ============================================================

        if (wallLeft)
        {
            Attach(-1);
            return;
        }

        if (wallRight)
        {
            Attach(1);
        }
    }
    private void Attach(int direction)
    {
        IsAttached = true;
        IsAtTop = false;
        WallDirection = direction;
    }

    // ============================================================
    // TOP TRANSITION
    // ============================================================

    private void StartTopTransition(CharacterBody2D player)
    {
        IsAtTop = true;
        IsTransitioningTop = true;

        _topStartPosition = player.GlobalPosition;

        // A posição alvo será construída gradualmente.
        //
        // Primeiro subimos aproximadamente a altura
        // necessária para passar da quina.
        float characterHeight = 16.0f;

        _topTargetPosition = player.GlobalPosition;

        _topTargetPosition.Y -= characterHeight;

        // Depois avançamos para dentro da plataforma.
        _topTargetPosition.X +=
            WallDirection * TopForwardDistance;
    }

    public Vector2 HandleWallMovement(Vector2 velocity)
    {
        if (!IsAttached)
            return velocity;

        // Durante a transição do topo,
        // o WallMovement normal não deve interferir.
        if (IsTransitioningTop)
            return Vector2.Zero;

        if (Input.IsActionPressed("ui_up"))
        {
            // Sobe a parede.
            velocity.Y = -WallClimbSpeed;
        }
        else if (Input.IsActionPressed("ui_down"))
        {
            // Desce a parede.
            velocity.Y = WallClimbSpeed;
        }
        else
        {
            // Sem input: fica literalmente parado na parede.
            velocity.Y = 0.0f;
        }

        // Enquanto está grudado,
        // não existe movimento horizontal.
        velocity.X = 0.0f;

        return velocity;
    }

    public Vector2 HandleTopMovement(
        CharacterBody2D player,
        float delta
    )
    {
        if (!IsTransitioningTop)
            return Vector2.Zero;

        Vector2 current = player.GlobalPosition;

        Vector2 direction =
            current.DirectionTo(_topTargetPosition);

        float distance =
            current.DistanceTo(_topTargetPosition);

        // Chegamos ao destino.
        if (distance <= 1.0f)
        {
            player.GlobalPosition = _topTargetPosition;

            IsTransitioningTop = false;
            IsAtTop = false;
            IsAttached = false;
            WallDirection = 0;

            return Vector2.Zero;
        }

        return direction * TopTransitionSpeed;
    }

    public bool IsTopTransitionFinished()
    {
        return !IsTransitioningTop;
    }

    public Vector2 GetWallJumpVelocity()
    {
        int jumpDirection = -WallDirection;

        Release();

        return new Vector2(
            jumpDirection * WallJumpForceX,
            WallJumpForceY
        );
    }

    public void Release()
    {
        IsAttached = false;
        IsAtTop = false;
        IsTransitioningTop = false;
        WallDirection = 0;
    }

    public bool ShouldFlipSprite()
    {
        return WallDirection < 0;
    }
}