using UnityEngine;

namespace UIBridge
{
    public static class UI
    {
        private static UIFactory _factory;

        // Вызывается только из UIRoot
        internal static void Initialize(UIFactory factory)
        {
            _factory = factory;
        }

        internal static void Dispose()
        {
            if (_factory != null)
            {
                _factory.ClearAllPools();
                _factory = null;
            }
        }

        // === Публичный API для вызова в коде ===

        public static T Create<T>(string prefabId) where T : View
        {
            if (_factory == null)
            {
                Debug.LogError("UI Bridge не инициализирован! Добавь UIRoot на сцену.");
                return null;
            }
            return _factory.Get<T>(prefabId);
        }

        public static void Release(View view)
        {
            _factory?.Release(view);
        }
    }
}