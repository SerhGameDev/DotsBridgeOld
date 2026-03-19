using DotsBridge.Placement;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public class PlacementSettingsAuthoring : MonoBehaviour
    {
        public GameObject GhostPrefab;

    }
    public class PlacementSettingsBaker : Baker<PlacementSettingsAuthoring>
    {
        public override void Bake(PlacementSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new PlacementSettings
            {
                GhostPrefab = GetEntity(authoring.GhostPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}