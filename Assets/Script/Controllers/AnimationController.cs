using Godot;

public class AnimationController
{
    private readonly AnimatedSprite2D _animator;
    private string _currentAnim = "";

    public AnimationController(AnimatedSprite2D animator)
    {
        _animator = animator;
    }

    public void Play(string animName)
    {
        if (_currentAnim == animName)
            return;

        if (_animator.SpriteFrames == null)
        {
            GD.Print("AnimatedSprite2D não possui SpriteFrames.");
            return;
        }

        if (!_animator.SpriteFrames.HasAnimation(animName))
        {
            GD.Print(
                $"Animação '{animName}' não encontrada no AnimatedSprite2D."
            );

            return;
        }

        _animator.Play(animName);
        _currentAnim = animName;
    }

    public void FlipH(bool flip)
    {
        _animator.FlipH = flip;
    }
}