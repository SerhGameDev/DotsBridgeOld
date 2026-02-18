using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    [RequireComponent(typeof(Collider))]
    public abstract class ZoneBase : MonoBehaviour
    {
        public static Dictionary<int, ZoneBase> ActiveZones = new Dictionary<int, ZoneBase>();
        
        public Entity Entity { get; private set; }

        protected virtual void OnEnable()
        {
            ActiveZones[GetInstanceID()] = this;
        }

        protected virtual void OnDisable()
        {
            ActiveZones.Remove(GetInstanceID());
        }

        public virtual void OnZoneStay(Entity intruder) { }

        public class ZoneBaker : Baker<ZoneBase>
        {
            public override void Bake(ZoneBase authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var zone = new ZoneComponent { InstanceID = authoring.GetInstanceID() };
                AddComponent(entity, zone);
                authoring.Entity = entity;
            }
        }
    }

}
