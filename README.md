# 實作 Line Notify 通知服務 (2) 搭配 ASP.NET Web API

完整部落格文章連結：[https://poychang.github.io/line-notify-2-use-web-api/](https://poychang.github.io/line-notify-2-use-web-api/)

----------

做過上一篇[實作 Line Notify 通知服務 (1)](https://poychang.github.io/line-notify-1-basic/)後，雖然作法有些複雜，但對 Line Notify 的連動的操作以及發訊息的方法有了認識，這篇打算將複雜封裝，做成一隻隻的 API 服務，讓使用上變得簡單。

>我使用的開發工具是 Visual Studio 2017，使用 ASP.NET Core 專案範本。

首先在 Visual Studio 新增專案，選擇 `ASP.NET Core Web 應用程式（.NET Core）`。

![](http://i.imgur.com/jSd0wfd.png)

選擇 `Web API` 範本。

![](http://i.imgur.com/ag0s05I.png)

## 連動 Line Notify

參考 Commit：[加入設定與 Line Notify 連動的控制器](https://github.com/poychang/TestLineNotifyAPI/commit/9fe3771fbac9df2a6252be5ab48a51e4b4e558ca)

第一步就是要進行 Line Notify 的帳號連動，建立一個 `AuthorizeController` 控制器，來處理和連動授權相關的方法。

這裡主要是轉址到特定 Line Notify 的連動頁面，並將我們建立的服務識別碼（Client ID）及相關設定送過去，這樣使用者就可以連動到我們指定的服務。

這段有兩個地方須修改成對應服務的設定值：

* `_clientId = "[YOUR_CLIENT_ID]";`
* `_redirectUri = "[YOUR_CALLBACK_URL]";`

## 設定 Callback 並取得發送訊息的 Token

參考 Commit：[新增對應的 Callback 和取得使用者 Access Token 的功能](https://github.com/poychang/TestLineNotifyAPI/commit/779784e2560c72d9c2818f4b017c221e7e91988d)

使用者設定玩連動後，會產生一組 Authorize Code 並回傳給指定的 Callback 位置，這裡就是建立 Callback 的位置，並透過 `FetchToken` 這個方法，來取得發送訊息的 Token。

```csharp
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
```

在取到 Token 後，隨即透過 URL 參數的方式，傳回指定的前端頁面 `_successUri`（這個位置會是下一篇的前端網頁）。

這段有兩個地方須修改成對應服務的設定值：

* `_clientSecret = "[YOUR_CLIENT_SECERT]";`
* `_successUri = "[http://XXX.XXX.XXX/LineNotifyPage]";`

## 發送文字訊息

參考 Commit：[加入傳訊訊息的控制器，增加傳送文字訊息的方法](https://github.com/poychang/TestLineNotifyAPI/commit/24441d9b2dcc93bdd7f8166a6e0e842becd33da3)

取到 Token 後，我們就可以借此來發送訊息，這裡在建立一支 `LineNotifyController` 控制器來處理發送訊息的大小事了。

發送文字訊息有兩個主要參數：

1. `token` 令牌
2. `message` 文字訊息

接到這兩個參數後，我們就可以透過 `https://notify-api.line.me/api/notify` Line Notify 的 API 服務來發訊訊息。

```csharp
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
```

這裡要注意的是，Token 必須設定在 HTTP Header 中的 `Authorization` 中才能通過授權驗證。

## 發送進階訊息

參考 Commit：

* [加入傳送訊息之模型](https://github.com/poychang/TestLineNotifyAPI/commit/9cb8fc8c6d7e8240ce5d57eda36dfdbd3fade9a4)
* [增加使用 POST 及訊息模型來傳送文字訊息的方法](https://github.com/poychang/TestLineNotifyAPI/commit/6c08aa6232c8775d4e34e79b3f4a578178c79bbb)

Line Notify 還提供發送貼圖以及圖片的服務，而在實作這兩個服務的時候，所需要的參數比較多，所以我這裡先建了一個傳送訊息用的模型，用來接收所有相關資訊，這部分可參考[官方文件](https://notify-bot.line.me/doc/en/)中 Notification 段落的說明。

有了訊息模型，順手增加了一個訊息模型搭配 POST 方法來傳送訊息的方法，這樣在前端就可以使用 JSON Object 來傳資料，然後交由 Web API 做 Data Binding，操作上會方便很多。

改完傳送文字訊息的方法，我們還可以做下面兩個：傳送官方貼圖、傳送圖片檔案。

### 傳送官方貼圖

參考 Commit：[增加傳送官方貼圖的方法](https://github.com/poychang/TestLineNotifyAPI/commit/e07ce6d7b3d36b90154c4f0745bc94d09ebe7690)

發送官方貼圖有四個主要參數：

1. `Token` 令牌
2. `Message` 文字訊息
3. `stickerPackageId` 貼圖包識別碼
4. `stickerId` 貼圖識別碼

貼圖包和貼圖的識別碼可以參考這個 [sticker_list.pdf](https://devdocs.line.me/files/sticker_list.pdf)。

### 傳送圖片檔案

參考 Commit：[增加傳送圖片檔案的方法](https://github.com/poychang/TestLineNotifyAPI/commit/4fba8778df70b1742a6c7abe1b99551d9ccf7ed7)

發送圖片檔案有四個主要參數：

1. `Token` 令牌
2. `Message` 文字訊息
3. `FileUri` 圖片檔案路徑
4. `Filename` 圖片檔案名稱

你會發現傳送訊息的三種方法都長得很像，不外乎就是透過 `HttpClient` 去傳送參數給 Line Notify API 服務，只要 Token 是合法的，參數符合特定條件，就 OK 了。

而且每一個傳送訊息的方法，都一定要有文字訊息，沒有這個了話，就會回傳下列訊息，告訴你 400 錯誤，還會貼心地和你說文字訊息不能是空的唷～

```json
{
  "status": 400,
  "message": "message: may not be empty"
}
```

## 取得貼圖資訊

貼圖人人愛用，但如果每次要發送貼圖都要找一次 [sticker_list.pdf](https://devdocs.line.me/files/sticker_list.pdf) 這個檔案了話，那就累了，如果有更簡單的方式那就好了

因此我整理了一份 [JSON 檔](https://github.com/poychang/TestLineNotifyAPI/blob/master/TestLineNotifyAPI/Data/sticker_list.json)，裡面包含各貼圖包的識別碼及相關資訊，這裡我也實作一隻取得貼圖及清單的控制器 `StickersController`。

### 取得貼圖清單

參考 Commit：

* [加入貼圖清單](https://github.com/poychang/TestLineNotifyAPI/commit/ac053db51cb9e7df54dcc4b9659d054f158bdab3)
* [加入貼圖控制器及模型，增加取得貼圖清單的方法](https://github.com/poychang/TestLineNotifyAPI/commit/8550cec7f8d922e38d54e65e23f32e5d0ecfebbb)

因為清單已經整理好了，這裡只需要建立一個讀取 JSON 檔案的方法，然後將資料回傳給前端，就搞定了。

### 取得貼圖圖示

參考 Commit：

* [加入貼圖 png 檔](https://github.com/poychang/TestLineNotifyAPI/commit/4a1169f83383b04aab5c199b22ab941b99452381)
* [增加取得貼圖檔的方法](https://github.com/poychang/TestLineNotifyAPI/commit/28715c3bbab5eb8341211802120bde6d47994de9)

如果只是傳回 JSON 清單，看著識別碼是沒有感覺的，能再提供貼圖圖片了話，就更清楚知道這個貼圖識別碼是啥了。

首先，要先取得所有貼圖圖示，這其實很簡單，只要安裝 Line 桌面版的應用程式，然後在聊天視窗中有執行過下載貼圖的動作，你就會在這個資料夾中 `C:\Users\[UserAccount]\AppData\Local\LINE\Data\Sticker` 看到所有的貼圖圖示了，而且這裡會依照貼圖包識別碼來做分類，把我們需要的貼圖複製一份下來即可。

接著在實作回傳貼圖的方法時，要注意的是檔案存取的路徑，但因為整個貼圖的資料夾結構，其實是依照識別碼做分類的，資料夾是貼圖包識別碼 `stickerPackageId`，然後檔案名稱也就是貼圖識別碼 `stickerId`，所以可以很輕鬆的取到正確路徑。

接者回傳時使用 `FileContentResult` 型別，然後給他讀出來的檔案及對應的 `Content-Type` 即可。

如果有介紹不清楚的地方，歡迎到我的[部落格](https://poychang.github.io/line-notify-2-use-web-api/)留言討論：)

----------

參考資料：

* [LINE Notify API Document](https://notify-bot.line.me/doc/en/)