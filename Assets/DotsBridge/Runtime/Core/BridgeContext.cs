using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    /// <summary>
    /// Структура-помощник, которая знает, в каком мире мы сейчас работаем.
    /// </summary>
    public readonly struct BridgeContext
    {
        public readonly BridgeRegistry State;

        public BridgeContext(BridgeRegistry state)
        {
            State = state;
        }
    }
}