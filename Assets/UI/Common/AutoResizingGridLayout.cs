using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Common
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class AutoResizingGridLayout : MonoBehaviour
    {
        public int columns = 2;
        public float spacing = 10f;
        public float fixedCellHeight = 120f; // 固定高度

        private RectTransform rectTransform;
        private GridLayoutGroup grid;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            grid = GetComponent<GridLayoutGroup>();
        }

        //void Update()
        //{
        //    UpdateCellSize();
        //}

        protected void OnEnable()
        {
            UpdateCellSize(); // 預先跑一次，避免初始化閃爍
        }

        protected void OnRectTransformDimensionsChange()
        {
            UpdateCellSize(); // 確保在寬度改變時更新 cellSize
        }

        void UpdateCellSize()
        {
            float parentWidth = ((RectTransform)rectTransform.parent).rect.width;

            // 預設欄寬（不含 spacing）
            float totalSpacing = spacing * (columns - 1);
            float availableWidth = rectTransform.rect.width - totalSpacing;
            float cellWidth = availableWidth / columns;

            // 限制不可超出父容器寬度
            float totalWidthWithSpacing = (cellWidth * columns) + totalSpacing;
            if (totalWidthWithSpacing > parentWidth)
            {
                // 壓縮 cellWidth，使剛好不超出父容器
                cellWidth = (parentWidth - totalSpacing) / columns;
            }

            grid.spacing = new Vector2(spacing, spacing);
            grid.cellSize = new Vector2(cellWidth, fixedCellHeight);
        }

    }
}
