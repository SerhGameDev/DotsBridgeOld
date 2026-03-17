using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public class WorkspaceGrid : VisualElement
    {
        private const float GridSpacing = 20f;
        private readonly Color _lineColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        public WorkspaceGrid()
        {
            // Сетка должна растягиваться на весь экран и лежать на самом дне (position Absolute)
            style.flexGrow = 1;
            style.position = Position.Absolute;
            style.left = 0; style.top = 0; style.right = 0; style.bottom = 0;
            
            // Подписываемся на событие перерисовки контента
            generateVisualContent += OnGenerateVisualContent;
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            var rect = contentRect;
            var painter = mgc.painter2D; // 2D API для рисования линий
            
            painter.strokeColor = _lineColor;
            painter.lineWidth = 1f;

            // Рисуем вертикальные линии
            for (float x = 0; x < rect.width; x += GridSpacing)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(x, 0));
                painter.LineTo(new Vector2(x, rect.height));
                painter.Stroke();
            }

            // Рисуем горизонтальные линии
            for (float y = 0; y < rect.height; y += GridSpacing)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(0, y));
                painter.LineTo(new Vector2(rect.width, y));
                painter.Stroke();
            }
        }
    }
}