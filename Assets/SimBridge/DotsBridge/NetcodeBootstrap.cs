using Unity.NetCode;
using Unity.Entities;

[UnityEngine.Scripting.Preserve]
public class NetcodeBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        // 1. Создаем пустую заглушку. Unity 6 требует, чтобы DefaultGameObjectInjectionWorld 
        // не был null при возврате true, иначе игра падает с AssertionException.
        var emptyWorld = new World(defaultWorldName);
        World.DefaultGameObjectInjectionWorld = emptyWorld;

        // 2. ВАЖНО: Мы НЕ ВЫЗЫВАЕМ здесь CreateServerWorld или CreateClientWorld!
        // Вся работа по созданию миров и загрузке сцен делегирована твоему MonoBehaviourBridge.
        
        return true; 
    }
}