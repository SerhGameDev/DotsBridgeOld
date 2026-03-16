using UnityEngine;
using UnityEngine.UIElements;

namespace SimPLS
{
    public class Connection : VisualElement
    {
        public Port PortA { get; private set; } // Откуда (Output)
        public Port PortB { get; private set; } // Куда (Input) - может быть null, если провод еще тянется

        private Vector2 tempMouseEndPos;
        private VisualElement workspace;

        public Connection(Port portA, VisualElement workspaceContainer)
        {
            PortA = portA;
            workspace = workspaceContainer;
            
            generateVisualContent += OnGenerateVisualContent;
            pickingMode = PickingMode.Ignore; 
            style.position = Position.Absolute;
            style.top = 0; style.left = 0; style.right = 0; style.bottom = 0;
        }

        // Вызывается во время перетаскивания (второго порта еще нет)
        public void UpdateTempEndPosition(Vector2 mousePos)
        {
            tempMouseEndPos = mousePos;
            MarkDirtyRepaint();
        }
// Вызывается, когда мы отрываем провод от входа
        public void DetachPortB()
        {
            if (PortB != null)
            {
                // Отписываемся от движения старой ноды
                PortB.ParentNode.OnNodeMoved -= OnNodeMovedHandler; 
                PortB = null;
            }
        }

        // Вспомогательный метод для подписки (чтобы можно было отписаться)
        private void OnNodeMovedHandler(Node node)
        {
            MarkDirtyRepaint();
        }

        // Обнови CompleteConnection, чтобы использовать именованный обработчик:
        public void CompleteConnection(Port portB)
        {
            PortB = portB;
            PortA.ParentNode.OnNodeMoved += OnNodeMovedHandler;
            PortB.ParentNode.OnNodeMoved += OnNodeMovedHandler;
            MarkDirtyRepaint();
        }
        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;
            if (painter == null || workspace == null) return;

            // Вычисляем начальную точку в координатах Workspace
            Vector2 startPos = workspace.WorldToLocal(PortA.VisualConnector.worldBound.center);
            
            // Вычисляем конечную точку (либо второй порт, либо мышка)
            Vector2 endPos = PortB != null 
                ? workspace.WorldToLocal(PortB.VisualConnector.worldBound.center) 
                : tempMouseEndPos;

            painter.strokeColor = new Color(0.8f, 0.8f, 0.2f, 1f); 
            painter.lineWidth = 4f;
            painter.lineCap = LineCap.Round;

            painter.BeginPath();
            painter.MoveTo(startPos);
            
            float distance = Mathf.Abs(endPos.x - startPos.x);
            float controlOffset = Mathf.Max(distance * 0.5f, 40f); 
            
            Vector2 control1 = startPos + Vector2.right * controlOffset;
            Vector2 control2 = endPos + Vector2.left * controlOffset;

            painter.BezierCurveTo(control1, control2, endPos);
            painter.Stroke();
        }
        
    }
}