using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TestLineNotifyAPI.Controllers
{
    [Route("api/[controller]")]
    public class AuthorizeController : Controller
    {
        private readonly string _authorizeUrl;
        private readonly string _clientId;
        private readonly string _redirectUri;
        private readonly string _state;

        /// <summary>建構式</summary>
        public AuthorizeController()
        {
            _authorizeUrl = "https://notify-bot.line.me/oauth/authorize";
            //TODO: 請先填入 Line Notify 服務的識別碼
            _clientId = "[YOUR_CLIENT_ID]";
            _redirectUri = "[YOUR_CALLBACK_URL]";
            //TODO: 可透過 State 避免 CSRF 攻擊
            _state = "NO_STATE";
        }

        // GET: api/Authorize
        /// <summary>設定與 Lind Notify 連動</summary>
        [HttpGet]
        public IActionResult GetAuthorize()
        {
            var uri = Uri.EscapeUriString(
                _authorizeUrl + "?" +
                "response_type=code" +
                "&client_id=" + _clientId +
                "&redirect_uri=" + _redirectUri +
                "&scope=notify" +
                "&state=" + _state
            );
            Response?.Redirect(uri);

            return new EmptyResult();
        }
    }
}
