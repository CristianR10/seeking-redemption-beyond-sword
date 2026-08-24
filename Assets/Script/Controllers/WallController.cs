using Godot;

public class WallController
{
    private const float WallClimbSpeed = 100.0f;
    private const float WallJumpForceX = 500.0f;
    private const float WallJumpForceY = -500.0f;
    private const float WallRayDisableTimeAfterTopJump = 0.50f;

    private RayCast2D _leftRay;
    private RayCast2D _rightRay;

    private RayCast2D _topRayRight;
    private RayCast2D _topRayLeft;

    public bool IsAttached { get; private set; }
    public bool IsAtTop { get; private set; }
    public int WallDirection { get; private set; }

    private float _wallRayDisableTimer;

    public void Initialize(
        RayCast2D leftRay,
        RayCast2D rightRay,
        RayCast2D topRayRight,
        RayCast2D topRayLeft)
    {
        _leftRay = leftRay;
        _rightRay = rightRay;

        _topRayRight = topRayRight;
        _topRayLeft = topRayLeft;

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

        if (_topRayRight == null)
        {
            throw new System.ArgumentNullException(
                nameof(topRayRight),
                "TopWallDetectedRight não foi encontrado."
            );
        }

        if (_topRayLeft == null)
        {
            throw new System.ArgumentNullException(
                nameof(topRayLeft),
                "TopWallDetectedLeft não foi encontrado."
            );
        }
    }

    public void Update(CharacterBody2D player, float delta)
    {
        if (_wallRayDisableTimer > 0.0f)
        {
            _wallRayDisableTimer -= delta;
            return;
        }

        if (IsAtTop)
        {
            /*
            * Mesmo estando no topo, o jogador pode escolher
            * descer novamente pela parede.
            *
            * Como o RayCast lateral ainda está colidindo,
            * podemos continuar agarrados à parede.
            */
            if (Input.IsActionPressed("ui_down"))
            {
                IsAtTop = false;
            }
            else
            {
                return;
            }
        }

        if (player.IsOnFloor())
        {
            Release();
            return;
        }

        bool wallLeft = _leftRay.IsColliding();
        bool wallRight = _rightRay.IsColliding();

        if (IsAttached)
        {
            HandleClimbing();
            return;
        }

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

    private void HandleClimbing()
    {
        if (Input.IsActionPressed("ui_up"))
        {
            /*
             * Se estiver escalando pela esquerda,
             * utiliza o TopRayLeft.
             *
             * Se estiver escalando pela direita,
             * utiliza o TopRayRight.
             */
            RayCast2D topRay =
                WallDirection == -1
                    ? _topRayLeft
                    : _topRayRight;

            if (topRay.IsColliding())
            {
                return;
            }

            /*
             * O TopRay deixou de detectar a parede/plataforma.
             * Chegamos ao topo.
             */
            IsAtTop = true;

            return;
        }

        bool stillOnWall =
            WallDirection == -1
                ? _leftRay.IsColliding()
                : _rightRay.IsColliding();

        if (stillOnWall)
        {
            return;
        }

        Release();
    }

    private void Attach(int direction)
    {
        IsAttached = true;
        IsAtTop = false;
        WallDirection = direction;
    }

    public Vector2 HandleWallMovement(Vector2 velocity)
    {
        if (!IsAttached)
        {
            return velocity;
        }

        if (IsAtTop)
        {
            /*
             * No topo, UI_UP não faz o personagem voar.
             *
             * Porém, UI_DOWN libera a descida.
             */
            if (Input.IsActionPressed("ui_down"))
            {
                IsAtTop = false;
                velocity.Y = WallClimbSpeed;
            }
            else
            {
                return Vector2.Zero;
            }
        }
        else if (Input.IsActionPressed("ui_up"))
        {
            velocity.Y = -WallClimbSpeed;
        }
        else if (Input.IsActionPressed("ui_down"))
        {
            velocity.Y = WallClimbSpeed;
        }
        else
        {
            velocity.Y = 0.0f;
        }

        velocity.X = 0.0f;

        return velocity;
    }

    public bool ShouldTopJump()
    {
        return IsAtTop &&
               Input.IsActionJustPressed("Jump_Space");
    }

    public Vector2 GetTopJumpVelocity()
    {
        float inputX = Input.GetAxis("ui_left", "ui_right");

        float jumpX = 0.0f;

        if (inputX < 0.0f)
        {
            jumpX = -WallJumpForceX;
        }
        else if (inputX > 0.0f)
        {
            jumpX = WallJumpForceX;
        }

        Vector2 velocity = new Vector2(
            jumpX,
            WallJumpForceY
        );

        Release();

        _wallRayDisableTimer =
            WallRayDisableTimeAfterTopJump;

        return velocity;
    }

    public Vector2 GetWallJumpVelocity()
    {
        float inputX =
            Input.GetAxis("ui_left", "ui_right");

        if (!Mathf.IsZeroApprox(inputX))
        {
            Vector2 velocity = new Vector2(
                Mathf.Sign(inputX) * WallJumpForceX,
                WallJumpForceY
            );

            Release();

            return velocity;
        }

        int direction =
            WallDirection == -1
                ? 1
                : -1;

        Vector2 wallJumpVelocity =
            new Vector2(
                direction * WallJumpForceX,
                WallJumpForceY
            );

        Release();

        return wallJumpVelocity;
    }

    public void Release()
    {
        IsAttached = false;
        IsAtTop = false;
        WallDirection = 0;
    }

    public bool ShouldFlipSprite()
    {
        return WallDirection < 0;
    }
}