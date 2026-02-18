using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public class GlobalPrefabsAuthoring : MonoBehaviour
    {
        [Tooltip("Перетащи сюда все префабы, которые нужны в DOTS")]
        public List<GameObject> Prefabs = new List<GameObject>();

        public class Baker : Baker<GlobalPrefabsAuthoring>
        {
            public override void Bake(GlobalPrefabsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new PrefabManagerTag());

                var buffer = AddBuffer<PrefabLink>(entity);

                foreach (var go in authoring.Prefabs)
                {
                    if (go == null) continue;

                    var prefabEntity = GetEntity(go, TransformUsageFlags.Dynamic);

                    buffer.Add(new PrefabLink
                    {
                        IDHash = Dots.GetHash(go.name),
                        PrefabEntity = prefabEntity
                    });
                }
            }
        }
    }
}