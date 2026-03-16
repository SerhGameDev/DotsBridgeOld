using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector; // Используем Odin для красивого инспектора

namespace UIBridge
{
    public class UIRoot : MonoBehaviour
    {
        [Title("UI Configuration")]
        [Tooltip("Корневой объект для всего UI на сцене")]
        [Required] public Transform CanvasContainer;

        [TableList(ShowIndexLabels = true)]
        [Tooltip("Список всех доступных префабов интерфейса")]
        public List<View> UIPrefabs = new List<View>();

        private UIFactory _internalFactory;

        private void Awake()
        {
            // 1. Создаем внутреннюю фабрику (с пулами) и передаем ей префабы
            _internalFactory = new UIFactory(UIPrefabs, CanvasContainer);

            // 2. Инжектим фабрику в статический класс UI
            UI.Initialize(_internalFactory);
        }

        private void OnDestroy()
        {
            // Обязательно очищаем статику при выгрузке сцены, 
            // чтобы пулы и ссылки уничтожились вместе с объектами
            UI.Dispose();
        }
    }
}