using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TestLineNotifyAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TestLineNotifyAPI.Controllers
{
    [Route("api/[controller]")]
    public class StickersController : Controller
    {
        // GET: api/Stickers
        /// <summary>取得貼圖清單</summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetStickers() => new JsonResult(FetchStickers());

        /// <summary>貼圖清單</summary>
        /// <returns></returns>
        private static IEnumerable<StickerModel> FetchStickers()
        {
            const string filePath = "Data/sticker_list.json";
            var fileJson = System.IO.File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<IEnumerable<StickerModel>>(fileJson);
        }
    }
}
