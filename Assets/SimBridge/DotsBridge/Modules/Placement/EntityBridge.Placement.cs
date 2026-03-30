using Unity.Entities;
using Unity.Mathematics;
using System;
using DotsBridge.Placement;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        public static event Action<SingleEntity> OnPlacementSuccess;
        public static event Action<SingleEntity> OnPlacementFailed;
        public static event Action<SingleEntity> OnDuctDrawingStarted;

        public enum PlacementStatus
        {
            Success,
            DrawingStarted,
            Invalid,
            NoGhostFound
        }

        /// <summary>
        /// Берет уже существующий на сцене объект и переводит его в режим размещения.
        /// </summary>
        public static void BeginPlacement(this ListEntity entity, float defaultSnapStep = 0.1f)
        {    
            if (entity.Count <= 0) // Исправлено с < 0 на <= 0
                return;
            
            entity.AddComponent(new GhostTag { IsValid = true });
            
            if (!entity.HasComponent<PlaceableTag>())
                entity.AddComponent<PlaceableTag>();

            if (!entity.HasComponent<GridSnapSettings>())
                entity.AddComponent(new GridSnapSettings { IsEnabled = true, Step = defaultSnapStep });

            if (!entity.HasComponent<PlacementPivot>())
                entity.AddComponent(new PlacementPivot { Value = float3.zero });
        }    

        public static DotsCommand BeginPlacement(this DotsCommand command, float defaultSnapStep = 0.1f) 
            => command.Do(entity => BeginPlacement(entity, defaultSnapStep));

        public static void BeginPlacement(this SingleEntity entity, float defaultSnapStep = 0.1f) 
            => BeginPlacement(entity.ToListEntity(), defaultSnapStep);

        /// <summary>
        /// Завершает размещение ТЕКУЩЕГО активного призрака.
        /// </summary>
        public static PlacementStatus CompleteCurrentPlacement()
        {
            var bridge = ClientBridge.World();
            if (bridge == null) return PlacementStatus.NoGhostFound;

            var ghostBatch = bridge.GetFromContainer<GhostTag>();
            if (ghostBatch.Count == 0)
            {
                ghostBatch.Dispose();
                return PlacementStatus.NoGhostFound;
            }

            var entity = new SingleEntity(ghostBatch.Entities[0], bridge);
            var ghostTag = entity.GetComponent<GhostTag>();
            var status = PlacementStatus.Invalid;

            if (!ghostTag.IsValid)
            {
                OnPlacementFailed?.Invoke(entity);
            }
            else if (entity.HasComponent<StretchableDuct>())
            {
                var duct = entity.GetComponent<StretchableDuct>();
                if (!duct.IsDrawing)
                {
                    duct.IsDrawing = true;
                    entity.SetComponent(duct);
                    OnDuctDrawingStarted?.Invoke(entity);
                    status = PlacementStatus.DrawingStarted;
                }
                else
                {
                    FinalizePlacement(bridge, entity);
                    status = PlacementStatus.Success;
                }
            }
            else
            {
                FinalizePlacement(bridge, entity);
                status = PlacementStatus.Success;
            }

            ghostBatch.Dispose();
            return status;
        }

        private static void FinalizePlacement(this BridgeWorld bridge, SingleEntity entity)
        {
            bridge.Manager.RemoveComponent<GhostTag>(entity.Entity);
            if (entity.HasComponent<BridgeIdentity>())
            {
                entity.SetComponent(new BridgeIdentity { Hash = 0 }); 
            }
            OnPlacementSuccess?.Invoke(entity);
        }

        /// <summary>
        /// Отменяет размещение (уничтожает текущего призрака).
        /// </summary>
        public static void CancelCurrentPlacement()
        {
            var bridge = ClientBridge.World();
            if (bridge == null) return;

            using var ghostBatch = bridge.GetFromContainer<GhostTag>();
            ghostBatch.Destroy(); 
        }

        public static bool IsCurrentPlacementValid()
        {
            var bridge = ClientBridge.World();
            if (bridge == null) return false;

            using var ghostBatch = bridge.GetFromContainer<GhostTag>();
            if (ghostBatch.Count == 0) return false;

            var entity = new SingleEntity(ghostBatch.Entities[0], bridge);
            return entity.GetComponent<GhostTag>().IsValid;
        }

        public static bool ToggleCurrentGridSnap(this BridgeWorld world)
        {
            using var ghostBatch = world.GetFromContainer<GhostTag>();
            if (ghostBatch.Count == 0) return false;

            var entity = new SingleEntity(ghostBatch.Entities[0], world);
            if (!entity.HasComponent<GridSnapSettings>()) return false;

            var snap = entity.GetComponent<GridSnapSettings>();
            snap.IsEnabled = !snap.IsEnabled;
            entity.SetComponent(snap);
            return snap.IsEnabled;
        }
        
        public static bool HasActiveGhost(this BridgeWorld world)
        {
            using var ghostBatch = world.GetFromContainer<GhostTag>();
            return ghostBatch.Count > 0;
        }
    }
}