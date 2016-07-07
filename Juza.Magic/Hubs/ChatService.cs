using Microsoft.AspNet.SignalR.Hubs;

namespace Juza.Magic.Hubs
{
    public interface IChatService
    {
        void OnConnected(int userId, string connectionId);
        void OnDisconnected(int userId, string connectionId);
        void Broadcast(string message, string username, HubConnectionContext clients);
    }

    public class ChatService : IChatService
    {
        public void OnConnected(int userId, string connectionId)
        {
            System.Diagnostics.Debug.WriteLine("Connected: " + connectionId);
        }

        public void OnDisconnected(int userId, string connectionId)
        {
            System.Diagnostics.Debug.WriteLine("Disconnected: " + connectionId);
        }

        public void Broadcast(string message, string username, HubConnectionContext clients)
        {
        }
    }
}