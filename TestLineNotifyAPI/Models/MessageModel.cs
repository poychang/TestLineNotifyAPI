namespace TestLineNotifyAPI.Models
{
    /// <summary>訊息</summary>
    public class MessageModel
    {
        /// <summary>令牌</summary>
        public string Token { get; set; }
        /// <summary>文字訊息</summary>
        public string Message { get; set; }
        /// <summary>貼圖包識別碼</summary>
        public string StickerPackageId { get; set; }
        /// <summary>貼圖識別碼</summary>
        public string StickerId { get; set; }
        /// <summary>圖片檔案路徑。限 jpg, png 檔</summary>
        public string FileUri { get; set; }
        /// <summary>圖片檔案名稱</summary>
        public string Filename { get; set; }
    }
}
