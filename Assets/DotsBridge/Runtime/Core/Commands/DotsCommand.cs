using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DotsBridge
{
    public interface ICommandAction
    {
        void Execute(ListEntity batch);
    }
    public interface ITargetResolver
    {
        ListEntity Resolve(BridgeWorld bridge);
        bool RequiresDispose { get; }
    }

    public partial class DotsCommand
    {
        public readonly string Name;
        private Func<ListEntity> _targetResolver;
        private ITargetResolver _resolver;
        private bool _requiresDispose;

        public readonly List<ICommandAction> ListActions = new List<ICommandAction>();

        public DotsCommand(string name) => Name = name;

        /// <summary>
        /// Вариант 1: Максимальная производительность.
        /// Передайте готовый класс-экшен. Идеально для кэшированных команд.
        /// </summary>
        public DotsCommand Do(ICommandAction customAction)
        {
            if (customAction != null) ListActions.Add(customAction);
            return this;
        }

        private class DelegateAction : ICommandAction
        {
            public Action<ListEntity> Action;
            public void Execute(ListEntity batch) => Action?.Invoke(batch);
        }

        /// <summary>
        /// Вариант 2: Максимальное удобство (Лямбда-выражения).
        /// ВНИМАНИЕ: Создает замыкания и нагружает GC при сборке команды!
        /// Идеально для UI-кнопок и разовых ивентов.
        /// </summary>
        public DotsCommand Do(Action<ListEntity> customAction)
        {
            if (customAction != null)
            {
                // Оборачиваем делегат в наш стандартный интерфейс
                ListActions.Add(new DelegateAction { Action = customAction });
            }
            return this;
        }


        private class AddComponentAction<T> : ICommandAction where T : unmanaged, IComponentData
        {
            public void Execute(ListEntity batch) => batch.AddComponent<T>();
        }

        public DotsCommand AddComponent<T>() where T : unmanaged, IComponentData
        {
            ListActions.Add(new AddComponentAction<T>());
            return this;
        }

        private class SetComponentAction<T> : ICommandAction where T : unmanaged, IComponentData
        {
            public T Data;
            public void Execute(ListEntity batch) => batch.TrySetComponent(Data);
        }

        public DotsCommand SetComponent<T>(T data) where T : unmanaged, IComponentData
        {
            ListActions.Add(new SetComponentAction<T> { Data = data });
            return this;
        }

        private class RemoveComponentAction<T> : ICommandAction where T : unmanaged, IComponentData
        {
            public void Execute(ListEntity batch) => batch.RemoveComponent<T>();
        }

        public DotsCommand RemoveComponent<T>() where T : unmanaged, IComponentData
        {
            ListActions.Add(new RemoveComponentAction<T>());
            return this;
        }

        private class LogAction : ICommandAction
        {
            public string Prefix;
            public string CommandName;
            public void Execute(ListEntity batch) => Debug.Log($"[{CommandName}] {Prefix} {batch.Count}");
        }

        public DotsCommand LogCount(string prefix = "Command executed on entities:")
        {
            ListActions.Add(new LogAction { Prefix = prefix, CommandName = Name });
            return this;
        }


        public DotsCommand SetTargetResolver(Func<ListEntity> resolver, bool requiresDispose = true)
        {
            _targetResolver = resolver;
            _requiresDispose = requiresDispose;
            return this;
        }

        public void Execute()
        {
            if (_targetResolver == null) return;

            ListEntity batch = _targetResolver.Invoke();

            if (!batch.Entities.IsCreated || batch.Count == 0)
            {
                if (_requiresDispose && batch.Entities.IsCreated) batch.Dispose();
                return;
            }

            for (int i = 0; i < ListActions.Count; i++)
            {
                ListActions[i].Execute(batch);
            }

            if (_requiresDispose) batch.Dispose();
        }

        public void Register() => EntityBridge.RegisterCommandInternal(this);
        public void RegisterAndExecute()
        {
            Register();
            Execute();
        }
    }
    /// <summary>
    /// Резолвер для получения сущностей из именованной группы BridgeWorld.
    /// </summary>
    internal class ContainerResolver : ITargetResolver
    {
        public string GroupName;
        // Мы НЕ удаляем список, так как он принадлежит реестру BridgeWorld
        public bool RequiresDispose => false;

        public ListEntity Resolve(BridgeWorld bridge)
            => bridge.GetEntitiesFromContainer(GroupName);
    }

    /// <summary>
    /// Резолвер для динамического поиска сущностей по компонентам.
    /// </summary>
    internal class QueryResolver : ITargetResolver
    {
        public ComponentType[] ComponentTypes;
        // Мы ОБЯЗАТЕЛЬНО удаляем этот список, так как он создается на лету (Temp)
        public bool RequiresDispose => true;

        public ListEntity Resolve(BridgeWorld bridge)
            => bridge.GetEntitiesFromContainer(ComponentTypes);
    }

    // =========================================================
    // ВНУТРЕННИЕ ЭКШЕНЫ (ICommandAction)
    // =========================================================

    // Эти классы нужны для реализации методов AddFromContainer внутри команды

    internal class AddContainerByNameAction : ICommandAction
    {
        public string GroupName;
        public void Execute(ListEntity batch) => batch.AddEntitiesFromContainer(GroupName);
    }

    internal class AddContainerByQueryAction : ICommandAction
    {
        public ComponentType[] Types;
        public void Execute(ListEntity batch) => batch.AddEntitiesFromQuery(Types);
    }

    public partial class DotsCommand
    {
        // =========================================================
        // ПЕРВИЧНЫЙ ВЫБОР (RESOLVERS)
        // =========================================================

        /// <summary>
        /// Выбирает сущности из именованной группы BridgeWorld.
        /// </summary>
        public DotsCommand FromContainer(string groupName)
        {
            _resolver = new ContainerResolver { GroupName = groupName };
            return this;
        }

        /// <summary>
        /// Динамически собирает сущности по массиву компонентов.
        /// </summary>
        public DotsCommand FromContainer(params ComponentType[] componentTypes)
        {
            _resolver = new QueryResolver { ComponentTypes = componentTypes };
            return this;
        }


        public DotsCommand FromContainer<T1>() where T1 : struct, IComponentData
            => FromContainer(typeof(T1));

        public DotsCommand FromContainer<T1, T2>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData
            => FromContainer(typeof(T1), typeof(T2));

        public DotsCommand FromContainer<T1, T2, T3>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData
            => FromContainer(typeof(T1), typeof(T2), typeof(T3));

        public DotsCommand FromContainer<T1, T2, T3, T4>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData where T4 : struct, IComponentData
            => FromContainer(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        public DotsCommand FromContainer<T1, T2, T3, T4, T5>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData where T4 : struct, IComponentData where T5 : struct, IComponentData
            => FromContainer(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

        public DotsCommand FromContainer<T1, T2, T3, T4, T5, T6>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData where T4 : struct, IComponentData where T5 : struct, IComponentData where T6 : struct, IComponentData
            => FromContainer(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }
    public partial class DotsCommand
    {
        // =========================================================
        // ДОБАВЛЕНИЕ ИЗ КОНТЕЙНЕРОВ (СТРОКОВЫЕ ГРУППЫ)
        // =========================================================

        private class AddContainerByNameAction : ICommandAction
        {
            public string GroupName;
            public void Execute(ListEntity batch) => batch.AddEntitiesFromContainer(GroupName);
        }

        /// <summary>
        /// Добавляет в текущую выборку команды сущности из другой именованной группы.
        /// </summary>
        public DotsCommand AddFromContainer(string groupName)
        {
            ListActions.Add(new AddContainerByNameAction { GroupName = groupName });
            return this;
        }

        // =========================================================
        // ДОБАВЛЕНИЕ ПО КОМПОНЕНТАМ (ДИНАМИЧЕСКИЕ ЗАПРОСЫ)
        // =========================================================

        private class AddContainerByQueryAction : ICommandAction
        {
            public ComponentType[] Types;
            public void Execute(ListEntity batch) => batch.AddEntitiesFromQuery(Types);
        }

        /// <summary>
        /// Добавляет в текущую выборку сущности, найденные по массиву компонентов.
        /// </summary>
        public DotsCommand AddFromContainer(params ComponentType[] componentTypes)
        {
            ListActions.Add(new AddContainerByQueryAction { Types = componentTypes });
            return this;
        }

        // --- СИНТАКСИЧЕСКИЙ САХАР ДЛЯ ДОБАВЛЕНИЯ (Generic до 6 элементов) ---

        public DotsCommand AddFromContainer<T1>() where T1 : struct, IComponentData
            => AddFromContainer(typeof(T1));

        public DotsCommand AddFromContainer<T1, T2>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData
            => AddFromContainer(typeof(T1), typeof(T2));

        public DotsCommand AddFromContainer<T1, T2, T3>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData
            => AddFromContainer(typeof(T1), typeof(T2), typeof(T3));

        public DotsCommand AddFromContainer<T1, T2, T3, T4>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData where T4 : struct, IComponentData
            => AddFromContainer(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        public DotsCommand AddFromContainer<T1, T2, T3, T4, T5>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData where T4 : struct, IComponentData where T5 : struct, IComponentData
            => AddFromContainer(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

        public DotsCommand AddFromContainer<T1, T2, T3, T4, T5, T6>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData where T4 : struct, IComponentData where T5 : struct, IComponentData where T6 : struct, IComponentData
            => AddFromContainer(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }
    public partial class DotsCommand
    {
        // --- ВНУТРЕННИЕ ЭКШЕНЫ ---

        private class AddDeathEventAction : ICommandAction
        {
            public void Execute(ListEntity batch) => batch.AddDeathEvent();
        }

        private class TriggerDeathAction : ICommandAction
        {
            public void Execute(ListEntity batch) => batch.TriggerDeath();
        }

        private class SetSpawnOnDeathAction : ICommandAction
        {
            public Entity Prefab;
            public void Execute(ListEntity batch) => batch.SetSpawnOnDeath(Prefab);
        }

        private class SubscribeOnDeathAction : ICommandAction
        {
            public Action<Entity> Callback;
            public void Execute(ListEntity batch) => batch.SubscribeOnDeath(Callback);
        }

        // --- ПУБЛИЧНЫЕ МЕТОДЫ КОМАНД ---

        public DotsCommand AddDeathEvent()
            => Do(new AddDeathEventAction());

        public DotsCommand TriggerDeath()
            => Do(new TriggerDeathAction());

        public DotsCommand SetSpawnOnDeath(Entity prefab)
            => Do(new SetSpawnOnDeathAction { Prefab = prefab });

        /// <summary>
        /// Подписывает действие на смерть. Реестр определится автоматически при Execute().
        /// </summary>
        public DotsCommand SubscribeOnDeath(Action<Entity> onDeathAction)
            => Do(new SubscribeOnDeathAction { Callback = onDeathAction });
    }
}