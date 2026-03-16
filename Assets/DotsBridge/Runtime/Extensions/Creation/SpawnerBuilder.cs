using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

namespace DotsBridge
{
    public struct SpawnerBuilder
    {
        private BridgeWorld _world;
        private Entity _prefab;
        private int _count;
        private float3 _position;
        private quaternion _rotation;
        private float _scale;
        private string _idString;

        private int _ownerId;

        public SpawnerBuilder(BridgeWorld world, Entity prefab)
        {
            _world = world;
            _prefab = prefab;
            _count = 1;
            _position = float3.zero;
            _rotation = quaternion.identity;
            _scale = 1f;
            _idString = null;
            _ownerId = 0; 
        }

        public SpawnerBuilder SetId(string id) { _idString = id; return this; }
        public SpawnerBuilder SetCount(int count) { _count = count; return this; }
        public SpawnerBuilder SetPosition(Vector3 position) { _position = position; return this; }
        public SpawnerBuilder SetRotation(Quaternion rotation) { _rotation = rotation; return this; }
        public SpawnerBuilder SetScale(float scale) { _scale = scale; return this; }

        public SpawnerBuilder SetOwner(int clientId) { _ownerId = clientId; return this; }
     
        public DotsCommand Spawn(string commandName = "SpawnCommand")
        {
            var command = new DotsCommand(commandName);

            var world = _world;
            var prefab = _prefab;
            var count = _count;
            var position = _position;
            var rotation = _rotation;
            var scale = _scale;
            var id = _idString;
            var owner = _ownerId;

            command.SetTargetResolver(() =>
            {
                var localTramsforrm = new LocalTransform()
                {
                    Position = position,
                    Rotation = rotation,
                    Scale = scale,
                };

                int idHash = string.IsNullOrEmpty(id) ? 0 : EntityBridge.GetHash(id);

                var spawnedEntities = new ListEntity(world);
                spawnedEntities.Instantiate(prefab, count);
                spawnedEntities.AddComponent(localTramsforrm);
                spawnedEntities.AddComponent(new BridgeIdentity { Hash = idHash });
                spawnedEntities.AddComponent(new BridgeOwner { ClientId = idHash });
                return new ListEntity(world);
            }, true);

            return command;
        }

        public void SpawnAsync()
        {
            if (_prefab == Entity.Null) return;

            SingleEntity.CreateEmpty(_world).AddComponent( new SpawnRequest
            {
                Prefab = _prefab,
                Count = _count,
                Position = _position,
                Rotation = _rotation,
                Scale = _scale,
                ID = string.IsNullOrEmpty(_idString) ? 0 : EntityBridge.GetHash(_idString),
                OwnerID = _ownerId
            });
        }
    }
}