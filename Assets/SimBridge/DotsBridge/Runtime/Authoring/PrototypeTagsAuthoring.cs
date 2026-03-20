using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Entities;

namespace DotsBridge
{
    public class PrototypeTagsAuthoring : SerializedMonoBehaviour
    {
        [Title("Entity Tags")]
        [InfoBox("Выберите теги (структуры), которые будут добавлены на эту сущность при запекании.")]
        
        // ExcludeExistingValuesInList убирает из дропдауна теги, которые вы уже добавили
        [ValueDropdown("GetAvailableTags", ExcludeExistingValuesInList = true)]
        public List<Type> TagsToBake = new List<Type>();

        // Метод, который Odin вызывает для построения списка
        private IEnumerable<Type> GetAvailableTags()
        {
            var targetInterface = typeof(IPrototypeTag);
            
            // Ищем все структуры (ValueType), реализующие наш интерфейс
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsValueType && targetInterface.IsAssignableFrom(type));
        }
    }
    public class PrototypeTagsBaker : Baker<PrototypeTagsAuthoring>
    {
        public override void Bake(PrototypeTagsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            if (authoring.TagsToBake == null || authoring.TagsToBake.Count == 0) 
                return;

            foreach (var type in authoring.TagsToBake)
            {
                if (type != null)
                {
                    // Добавляем структуру на сущность по её типу.
                    // Важно: Поля внутри структуры (если они есть) получат значения по умолчанию (0, false и т.д.)
                    AddComponent(entity, new ComponentType(type));
                }
            }
        }
    }
    

    public interface IPrototypeTag : IComponentData { }

    public struct IsObjectTag : IPrototypeTag { }
    public struct BossTag : IPrototypeTag { }
    public struct StunnedTag : IPrototypeTag { }
    public struct IsActiveTag : IPrototypeTag { }        // Объект активен/включен
    public struct IsDisabledTag : IPrototypeTag { }      // Объект выключен (логически)
    public struct IsInitializedTag : IPrototypeTag { }   // Прошел ли объект через систему инициализации

    // --- БОЕВАЯ СИСТЕМА ---
    public struct IsAliveTag : IPrototypeTag { }         // Живой
    public struct IsDeadTag : IPrototypeTag { }          // Мертвый (для запуска анимации смерти/удаления)
    public struct IsInvulnerableTag : IPrototypeTag { }  // Неуязвимый
    public struct IsTargetableTag : IPrototypeTag { }    // Можно ли выбрать как цель
    public struct IsInCombatTag : IPrototypeTag { }      // В бою

    // --- ФРАКЦИИ / ОТНОШЕНИЯ ---
    public struct PlayerTag : IPrototypeTag { }          // Игрок
    public struct EnemyTag : IPrototypeTag { }           // Враг
    public struct FriendlyTag : IPrototypeTag { }        // Союзник
    public struct NeutralTag : IPrototypeTag { }         // Нейтральный объект

    // --- ПЕРЕМЕЩЕНИЕ ---
    public struct IsGroundedTag : IPrototypeTag { }      // На земле
    public struct IsFallingTag : IPrototypeTag { }       // Падает
    public struct IsMovingTag : IPrototypeTag { }        // В движении
    public struct IsJumpingTag : IPrototypeTag { }       // В прыжке
    public struct IsStaticTag : IPrototypeTag { }        // Неподвижный объект (препятствие)

    // --- ЭФФЕКТЫ (DEBUFFS / STATES) ---
    public struct IsStunnedTag : IPrototypeTag { }       // Оглушен
    public struct IsFrozenTag : IPrototypeTag { }        // Заморожен
    public struct IsBurningTag : IPrototypeTag { }       // Горит
    public struct IsSlowedTag : IPrototypeTag { }        // Замедлен
    public struct IsSilencedTag : IPrototypeTag { }      // Не может колдовать/использовать навыки

    // --- AI И ЛОГИКА ---
    public struct IsIdleTag : IPrototypeTag { }          // Бездельничает
    public struct IsChasingTag : IPrototypeTag { }       // Преследует цель
    public struct IsPatrollingTag : IPrototypeTag { }    // Патрулирует
    public struct IsFleeingTag : IPrototypeTag { }       // Убегает в страхе
    public struct HasAggroTag : IPrototypeTag { }        // Агрессивен к кому-то

    // --- ВЗАИМОДЕЙСТВИЕ И UI ---
    public struct IsInteractableTag : IPrototypeTag { }  // Можно взаимодействовать (рычаг, сундук)
    public struct IsSelectedTag : IPrototypeTag { }      // Выбран игроком
    public struct IsHoveredTag : IPrototypeTag { }       // Мышка наведена на объект
    public struct IsVisibleTag : IPrototypeTag { }       // Видим ли камерой (для оптимизации)

    // --- ТРИГГЕРЫ ---
    public struct IsTriggerTag : IPrototypeTag { }       // Зона-триггер
    public struct IsOccupiedTag : IPrototypeTag { }      // Точка/клетка занята
}
