using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealtimeWeb.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimeWeb.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(IHubContext<ChatHub> chatHub)
        {
            ChatHub = chatHub;
        }

        IHubContext<ChatHub> ChatHub { get; }

        public IActionResult Index() => View();

        [HttpGet("sse")]
        public IActionResult ServerSentEvents() => View();


        [HttpGet("sse/data")]
        public async Task ServerSentEventsData()
        {
            Response.ContentType = "text/event-stream";
            for (int i = 0; i < 5; i++)
            {
                if (i == 3 && new Random().Next(5) < 4)
                    throw new InvalidOperationException(); // Break the connection. Client should auto-establish connection.

                var data = JsonSerializer.Serialize(new { loop = i+1, at = DateTime.Now.ToLongTimeString() });
                await Response.WriteAsync($"data: {data}\n\n");
                await Response.Body.FlushAsync();
                Thread.Sleep(2000);
            }

            Response.Body.Close();
        }

        [HttpGet("websocket")]
        public IActionResult WebSocket() => View();

        [HttpGet("websocket/data")]
        public async Task WebSocketData()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await SendEvents(ws);
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        async Task SendEvents(WebSocket ws)
        {
            for (int i = 0; i < 5; i++)
            {
                if (i == 3 && new Random().Next(5) < 4)
                    throw new InvalidOperationException(); // Break the connection. Client should auto-establish connection.

                var data = JsonSerializer.Serialize(new { loop = i + 1, at = DateTime.Now.ToLongTimeString() });
                await ws.SendAsync(buffer: new ArraySegment<byte>(
                    array: Encoding.ASCII.GetBytes(data),
                    offset: 0,
                    count: data.Length),
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None);

                Thread.Sleep(2000);
            }
        }


        [HttpGet("signalr")]
        public IActionResult SignalR() => View();

        [HttpGet("/signalr/test")]
        public async Task SignalRSendTestMessage() =>
            await ChatHub.Clients.All.SendAsync("Test", $"Test message from server at {DateTime.Now.ToLongTimeString()}");
    }
}
