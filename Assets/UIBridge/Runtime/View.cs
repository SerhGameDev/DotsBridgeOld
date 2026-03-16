using UnityEngine;

namespace UIBridge
{
    // === 1. Базовые абстракции визуала ===

    // View - это MonoBehaviour, так как он физически висит на префабе
    public abstract class View : MonoBehaviour
    {
        public string ID;
        // Место для базовых анимаций (например, через DOTween)
        public virtual void Show() => gameObject.SetActive(true);
        public virtual void Hide() => gameObject.SetActive(false);
    }
}