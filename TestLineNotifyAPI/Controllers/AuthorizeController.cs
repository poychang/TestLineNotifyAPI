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
        private readonly string _tokenUrl;

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly string _state;
        private readonly string _successUri;

        /// <summary>建構式</summary>
        public AuthorizeController()
        {
            _authorizeUrl = "https://notify-bot.line.me/oauth/authorize";
            _tokenUrl = "https://notify-bot.line.me/oauth/token";
            //TODO: 請先填入 Line Notify 服務的識別碼及密鑰等資訊
            _clientId = "[YOUR_CLIENT_ID]";
            _clientSecret = "[YOUR_CLIENT_SECERT]";
            _redirectUri = "[YOUR_CALLBACK_URL]";
            //TODO: 可透過 State 避免 CSRF 攻擊
            _state = "NO_STATE";
            //TODO: 成功後轉跳之頁面
            _successUri = "[http://XXX.XXX.XXX/LineNotifyPage]";
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

        // GET: api/Authorize/Callback
        /// <summary>取得使用者 code</summary>
        /// <param name="code">用來取得 Access Tokens 的 Authorize Code</param>
        /// <param name="state">驗證用。避免 CSRF 攻擊</param>
        /// <param name="error">錯誤訊息</param>
        /// <param name="errorDescription">錯誤描述</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Callback")]
        public async Task<IActionResult> GetCallback(
            [FromQuery]string code,
            [FromQuery]string state,
            [FromQuery]string error,
            [FromQuery][JsonProperty("error_description")]string errorDescription)
        {
            if (!string.IsNullOrEmpty(error))
                return new JsonResult(new
                {
                    error,
                    state,
                    errorDescription
                });

            Response.Redirect(_successUri + "?token=" + await FetchToken(code));

            return new EmptyResult();
        }

        /// <summary>取得使用者 Token</summary>
        /// <param name="code">用來取得 Access Tokens 的 Authorize Code</param>
        /// <returns></returns>
        private async Task<string> FetchToken(string code)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 60);
                client.BaseAddress = new Uri(_tokenUrl);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", _redirectUri),
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret)
                });
                var response = await client.PostAsync("", content);
                var data = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<JObject>(data)["access_token"].ToString();
            }
        }
    }
}
