using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimeWeb.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string message) =>
            await Clients.Others.SendAsync("ShowMessage", $"{Context.ConnectionId}: {message}");

        public override async Task OnConnectedAsync() =>
            await Clients.Others.SendAsync("ShowMessage", $"({Context.ConnectionId} connected!)");

        public override async Task OnDisconnectedAsync(Exception exception) =>
            await Clients.All.SendAsync("ShowMessage", $"({Context.ConnectionId} disconnected. Error: {exception.Message})");
    }
}
