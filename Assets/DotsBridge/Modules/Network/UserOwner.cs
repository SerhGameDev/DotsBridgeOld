#if DOTSBRIDGE_NETCODE
using Unity.Entities;
using Unity.NetCode;

namespace DotsBridge.Modules.Network
{
    // Компонент, указывающий, какому игроку принадлежит объект
    public struct UserOwner : IComponentData
    {
        public int NetworkId;
    }
    // Компонент-призрак. Он останется на сущности даже когда Netcode её удалит!
    public struct UserCleanupState : ICleanupComponentData
    {
        public int NetworkId;
    }
}
#endif