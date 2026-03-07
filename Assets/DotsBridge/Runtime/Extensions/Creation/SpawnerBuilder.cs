using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DotsBridge
{
    public struct SpawnerBuilder
    {
        private BridgeRegistry _registry;
        private Entity _prefab;
        private int _count;
        private float3 _position;
        private quaternion _rotation;
        private float _scale;
        private string _idString;

        // НОВОЕ: Храним ID владельца
        private int _ownerId;

        public SpawnerBuilder(BridgeRegistry registry, Entity prefab)
        {
            _registry = registry;
            _prefab = prefab;
            _count = 1;
            _position = float3.zero;
            _rotation = quaternion.identity;
            _scale = 1f;
            _idString = null;
            _ownerId = 0; // 0 означает "нет владельца" (Сервер)
        }

        public SpawnerBuilder SetId(string id) { _idString = id; return this; }
        public SpawnerBuilder SetCount(int count) { _count = count; return this; }
        public SpawnerBuilder SetPosition(Vector3 position) { _position = position; return this; }
        public SpawnerBuilder SetRotation(Quaternion rotation) { _rotation = rotation; return this; }
        public SpawnerBuilder SetScale(float scale) { _scale = scale; return this; }

        // НОВОЕ: Метод для установки владельца
        public SpawnerBuilder SetOwner(int clientId) { _ownerId = clientId; return this; }

        public DotsCommand Spawn(string commandName = "SpawnCommand")
        {
            var command = new DotsCommand(commandName);

            var registry = _registry;
            var prefab = _prefab;
            var count = _count;
            var pos = _position;
            var rot = _rotation;
            var scale = _scale;
            var id = _idString;
            var owner = _ownerId; // Кэшируем для лямбды

            command.SetTargetResolver(() =>
            {
                var manager = registry.Manager;
                if (prefab == Entity.Null) return new EntityBatch(default, manager);

                NativeList<Entity> spawnedEntities = new NativeList<Entity>(count, Allocator.TempJob);
                using (var tempArray = new NativeArray<Entity>(count, Allocator.Temp))
                {
                    manager.Instantiate(prefab, tempArray);
                    spawnedEntities.AddRange(tempArray);
                }

                int idHash = string.IsNullOrEmpty(id) ? 0 : EntityBridge.GetHash(id);
                var transform = LocalTransform.FromPositionRotationScale(pos, rot, scale);

                foreach (var entity in spawnedEntities)
                {
                    manager.SetComponentData(entity, transform);

                    if (idHash != 0)
                        manager.AddComponentData(entity, new BridgeIdentity { Hash = idHash });

                    // НОВОЕ: Если владелец указан, вешаем компонент
                    if (owner != 0)
                        manager.AddComponentData(entity, new BridgeOwner { ClientId = owner });
                }

                return new EntityBatch(spawnedEntities, manager);
            }, true);

            return command;
        }

        public void SpawnAsync()
        {
            if (_prefab == Entity.Null) return;

            Entity requestEntity = _registry.Manager.CreateEntity(typeof(SpawnRequest));
            _registry.Manager.SetComponentData(requestEntity, new SpawnRequest
            {
                Prefab = _prefab,
                Count = _count,
                Position = _position,
                Rotation = _rotation,
                Scale = _scale,
                ID = string.IsNullOrEmpty(_idString) ? 0 : EntityBridge.GetHash(_idString),
                OwnerID = _ownerId // Передаем в запрос
            });
        }
    }
}