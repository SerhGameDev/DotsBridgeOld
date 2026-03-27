using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

namespace DotsBridge.Modules.Network
{
    public enum Role
    {
        ServerClient = 0, // Host
        Server = 1,       // Dedicated Server
        Client = 2,       // Pure Client
    }

    public static class DotsNetworkManager
    {
        public static Role CurrentRole { get; private set; }

        public static void StartServer(ushort port)
        {
            CurrentRole = Role.Server;
            
            // 1. Look for an existing server world
            var serverWorld = GetExistingWorld(WorldFlags.GameServer);

            // 2. Create one manually if Auto-Bootstrap is disabled
            if (serverWorld == null)
            {
                serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            }

            World.DefaultGameObjectInjectionWorld = serverWorld;

            // FIX: Use NetworkStreamRequestListen instead of querying the driver directly
            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(port);
            var listenEntity = serverWorld.EntityManager.CreateEntity();
            serverWorld.EntityManager.AddComponentData(listenEntity, new NetworkStreamRequestListen { Endpoint = endpoint });

            Debug.Log($"[DotsNetworkManager] Server initialized to listen on port {port}");
        }

        public static void ConnectClient(string ip, ushort port)
        {
            if (CurrentRole != Role.ServerClient)
                CurrentRole = Role.Client;

            // 1. Look for an existing client world
            var clientWorld = GetExistingWorld(WorldFlags.GameClient);

            if (clientWorld == null)
            {
                clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            }

            if (CurrentRole == Role.Client)
                World.DefaultGameObjectInjectionWorld = clientWorld;

            var endpoint = NetworkEndpoint.Parse(ip, port);

            // FIX: Use NetworkStreamRequestConnect instead of querying the driver directly
            var connectEntity = clientWorld.EntityManager.CreateEntity();
            clientWorld.EntityManager.AddComponentData(connectEntity, new NetworkStreamRequestConnect { Endpoint = endpoint });

            Debug.Log($"[DotsNetworkManager] Client initiated connection request to {ip}:{port}");
        }

        public static void StartHost(string ip, ushort port)
        {
            CurrentRole = Role.ServerClient;
            StartServer(port);
            ConnectClient(ip, port);
        }
        
        // Helper method to find existing worlds
        private static World GetExistingWorld(WorldFlags flag)
        {
            foreach (var world in World.All)
            {
                if ((world.Flags & flag) != 0) return world;
            }
            return null;
        }
    }
}