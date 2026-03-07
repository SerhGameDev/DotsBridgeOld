using Unity.Mathematics;
using Unity.NetCode;

namespace DotsBridge
{
    public struct OopEventRpc : IRpcCommand
    {
        public int EventHash;    
        public int IntValue;
        public float FloatValue;
        public float3 VectorValue;
    }
}