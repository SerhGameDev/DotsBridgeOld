using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace SimElectric
{
    [RequireComponent(typeof(WireAuthoring))] 
    public class RelayContactAuthoring : MonoBehaviour
    {
        [Required("Link to the coil that controls this contact")]
        public RelayCoilAuthoring targetCoil;

        public ContactType contactType = ContactType.NormallyOpen;

        public class Baker : Baker<RelayContactAuthoring>
        {
            public override void Bake(RelayContactAuthoring authoring)
            {
                if (authoring.targetCoil == null) return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RelayContact
                {
                    TargetCoil = GetEntity(authoring.targetCoil, TransformUsageFlags.Dynamic),
                    Type = authoring.contactType
                });
            }
        }
    }
}