using DotsBridge.Timeline;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    public static partial class Dots
    {
        /// <summary>
        /// Создать анимацию для группы сущностей.
        /// </summary>
        public static RelativeGroupBuilder Sequence(NativeArray<Entity> entities)
        {
            return new RelativeGroupBuilder(Manager, entities, Allocator.Temp);
        }

        /// <summary>
        /// Создать анимацию для списка сущностей.
        /// </summary>
        public static RelativeGroupBuilder Sequence(NativeList<Entity> entities)
        {
            return new RelativeGroupBuilder(Manager, entities.AsArray(), Allocator.Temp);
        }

        /// <summary>
        /// Создать анимацию для одной сущности (как массив из 1 элемента).
        /// </summary>
        public static RelativeGroupBuilder Sequence(Entity entity)
        {
            var arr = new NativeArray<Entity>(1, Allocator.Temp);
            arr[0] = entity;
            // Builder сам освободит Allocator.Temp массив не надо, 
            // но так как Builder struct, лучше массив освободить снаружи или использовать List
            // Для простоты API вернем Builder, но массив "утечет" до конца кадра (Temp allocator), это норм.
            return new RelativeGroupBuilder(Manager, arr, Allocator.Temp);
        }
    }
}