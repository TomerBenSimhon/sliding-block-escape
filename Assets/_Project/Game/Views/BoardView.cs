using System.Collections.Generic;
using UnityEngine;

namespace SlidingBlock.Game
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _background;
        [SerializeField] private Transform _blocksParent;
        [SerializeField] private BlockView _blockPrefab;
        [SerializeField, Min(0f)] private float _margin = 0.2f;

        private readonly List<BlockView> _blocks = new List<BlockView>();

        public void SetBoardSize(Vector2 worldSize)
        {
            _background.transform.localScale = new Vector3(worldSize.x + _margin * 2f, worldSize.y + _margin * 2f, 1f);
        }

        public BlockView CreateBlock()
        {
            BlockView blockView = Instantiate(_blockPrefab, _blocksParent);
            _blocks.Add(blockView);
            return blockView;
        }

        public void Clear()
        {
            foreach (BlockView blockView in _blocks)
            {
                Destroy(blockView.gameObject);
            }
            _blocks.Clear();
        }
    }
}
