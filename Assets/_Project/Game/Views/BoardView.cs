using System.Collections.Generic;
using UnityEngine;

namespace SlidingBlock.Game
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _background;
        [SerializeField] private Transform _blockViewsParent;
        [SerializeField] private BlockView _blockViewPrefab;
        [SerializeField, Min(0f)] private float _margin = 0.2f;

        private readonly List<BlockView> _blockViews = new List<BlockView>();

        public void SetSize(Vector2 boardWorldSize)
        {
            _background.transform.localScale = new Vector3(boardWorldSize.x + _margin * 2f, boardWorldSize.y + _margin * 2f, 1f);
        }

        public BlockView CreateBlockView()
        {
            BlockView blockView = Instantiate(_blockViewPrefab, _blockViewsParent);
            _blockViews.Add(blockView);
            return blockView;
        }

        public void Clear()
        {
            foreach (BlockView blockView in _blockViews)
            {
                Destroy(blockView.gameObject);
            }
            _blockViews.Clear();
        }
    }
}
