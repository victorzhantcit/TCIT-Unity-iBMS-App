using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Common
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class AutoResizingGridLayout : MonoBehaviour
    {
        public int columns = 2;
        public float spacing = 10f;
        public float fixedCellHeight = 120f; // �T�w����

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
            UpdateCellSize(); // �w���]�@���A�קK��l�ư{�{
        }

        protected void OnRectTransformDimensionsChange()
        {
            UpdateCellSize(); // �T�O�b�e�ק��ܮɧ�s cellSize
        }

        void UpdateCellSize()
        {
            float parentWidth = ((RectTransform)rectTransform.parent).rect.width;

            // �w�]��e�]���t spacing�^
            float totalSpacing = spacing * (columns - 1);
            float availableWidth = rectTransform.rect.width - totalSpacing;
            float cellWidth = availableWidth / columns;

            // ����i�W�X���e���e��
            float totalWidthWithSpacing = (cellWidth * columns) + totalSpacing;
            if (totalWidthWithSpacing > parentWidth)
            {
                // ���Y cellWidth�A�ϭ�n���W�X���e��
                cellWidth = (parentWidth - totalSpacing) / columns;
            }

            grid.spacing = new Vector2(spacing, spacing);
            grid.cellSize = new Vector2(cellWidth, fixedCellHeight);
        }

    }
}
