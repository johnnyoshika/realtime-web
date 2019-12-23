using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimeWeb.Controllers
{
    public class HomeController : Controller
    {
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
    }
}
