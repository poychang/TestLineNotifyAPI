using System.Collections.Generic;
using System.Linq;
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

        // GET: api/Stickers/1/12
        /// <summary>取得貼圖</summary>
        /// <param name="stickerPackageId">貼圖包識別碼</param>
        /// <param name="stickerId">貼圖識別碼</param>
        /// <returns>貼圖</returns>
        [HttpGet]
        [Route("{stickerPackageId}/{stickerId}")]
        public IActionResult GetSticker(string stickerPackageId, string stickerId)
        {
            var filename = FetchStickers().First(p => p.Stkpkgid == stickerPackageId && p.Stkid == stickerId).Img;
            var image = System.IO.File.ReadAllBytes("Data/" + stickerPackageId + "/" + filename);
            return new FileContentResult(image, "image/png");
        }

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
