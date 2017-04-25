using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestLineNotifyAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TestLineNotifyAPI.Controllers
{
    [Route("api/[controller]")]
    public class LineNotifyController : Controller
    {
        private readonly string _notifyUrl;

        public LineNotifyController()
        {
            _notifyUrl = "https://notify-api.line.me/api/notify";
        }

        // GET: api/LineNotify/SendMessage?target=PoyChang&message=HelloWorld
        /// <summary>傳送文字訊息</summary>
        /// <param name="token">令牌</param>
        /// <param name="message">訊息</param>
        [HttpGet]
        [Route("SendMessage")]
        public async Task<IActionResult> SendMessage(string token, string message)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_notifyUrl);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("message", message)
                });

                await client.PostAsync("", form);
            }

            return new EmptyResult();
        }

        // POST: api/LineNotify/SendMessage
        /// <summary>傳送文字訊息</summary>
        /// <param name="msg">訊息</param>
        [HttpPost]
        [Route("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody]MessageModel msg)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_notifyUrl);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + msg.Token);

                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("message", msg.Message)
                });

                await client.PostAsync("", form);
            }

            return new EmptyResult();
        }
    }
}
