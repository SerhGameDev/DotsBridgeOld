using DotsBridge;
using DotsBridge.Modules.Rotation;
using UnityEngine;

namespace DotsBridge
{
    public class InteractionSystem : MonoBehaviour
    {
        private void OnEnable()
        {
            EntityBridge.OnMouseEnter += HandleMouseEnter;
            EntityBridge.OnMouseExit += HandleMouseExit;
        }

        private void OnDisable()
        {
            EntityBridge.OnMouseEnter -= HandleMouseEnter;
            EntityBridge.OnMouseExit -= HandleMouseExit;
        }

        private void HandleMouseEnter(SingleEntity entity)
        {
            entity.EnableMouseRotation();
        }

        private void HandleMouseExit(SingleEntity entity)
        {
            entity.DisableMouseRotation();
        }

        void Update()
        {
            //if (Input.GetMouseButtonDown(0))
            //{

            //    EntityBridge
            //        .InCurrentWorld()
            //        .GetEntityUnderMouse<HoveredTag>()
            //        .ToListEntity()
            //        .Move(Vector3.up, 5f)
            //        .StartMove();
            //}
        }
    }
}