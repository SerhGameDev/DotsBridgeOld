using Unity.Entities;

namespace DotsBridge
{
    public struct SyncSettings : IComponentData
    {
        public enum Mode
        {
            CopyFromGameObject, 
            CopyToGameObject 
        }

        public Mode SyncMode;
        public bool SyncPosition;
        public bool SyncRotation;
    }
}
