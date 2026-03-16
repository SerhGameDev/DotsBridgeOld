using System.Collections.Generic;
using UnityEngine;
namespace UIBridge
{
    // Internal означает, что класс доступен только внутри сборки/плагина UIBridge
    internal class UIFactory
    {
        private readonly Transform _rootContainer;
        private readonly Dictionary<string, View> _prefabs = new Dictionary<string, View>();

        // Словари для хранения пулов объектов
        private readonly Dictionary<string, Queue<View>> _pools = new Dictionary<string, Queue<View>>();

        public UIFactory(List<View> prefabs, Transform rootContainer)
        {
            _rootContainer = rootContainer;

            // Запоминаем префабы по их ID (названиям)
            foreach (var prefab in prefabs)
            {
                _prefabs[prefab.ID] = prefab;
                _pools[prefab.ID] = new Queue<View>();
            }
        }

        public T Get<T>(string id) where T : View
        {
            if (!_pools.ContainsKey(id))
            {
                Debug.LogError($"Префаб с ID {id} не найден в UIRoot!");
                return null;
            }

            View instance;
            if (_pools[id].Count > 0)
            {
                instance = _pools[id].Dequeue();
            }
            else
            {
                // Если пул пуст, создаем новый объект
                instance = Object.Instantiate(_prefabs[id], _rootContainer);
            }

            instance.gameObject.SetActive(true);
            return instance as T;
        }

        public void Release(View view)
        {
            view.gameObject.SetActive(false);
            // Возвращаем объект обратно в пул
            _pools[view.ID].Enqueue(view);
        }

        public void ClearAllPools()
        {
            _pools.Clear();
            _prefabs.Clear();
        }
    }
}