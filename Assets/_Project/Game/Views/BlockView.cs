using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace SlidingBlock.Game
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BlockView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _targetColor = Color.red;
        [SerializeField, Min(0f)] private float _padding = 0.1f;
        [SerializeField, Min(0f)] private float _moveDuration = 0.15f;

        public void SetTarget(bool isTarget)
        {
            _spriteRenderer.color = isTarget ? _targetColor : _normalColor;
        }

        public void SetBounds(Vector3 center, Vector2 size)
        {
            transform.position = center;
            transform.localScale = new Vector3(size.x - _padding, size.y - _padding, 1f);
        }

        public void SetPosition(Vector3 center)
        {
            transform.position = center;
        }

        public UniTask AnimateTo(Vector3 center)
        {
            return LMotion.Create(transform.position, center, _moveDuration)
                .WithEase(Ease.OutCubic)
                .BindToPosition(transform)
                .ToUniTask(destroyCancellationToken);
        }
    }
}
