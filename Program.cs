using Otaku16;
using Otaku16.Tools;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

const string version = "1.1.6c";
const string updateLog = $"\n" +
    $"更新日志：\n" +
    $"修复了 - 未审核数量超30报告的数据不准确" + 
    $"修复了 - 初次运行不生成配置文件";
const string about = $"音乐投稿机器人 by @AkizonChan - {version} {updateLog}";



#region 初始化

Logger log = new("Main");
Config config = Config.GetConfig();
Dictionary<long, Post> cache = Cache.GetCache().Caches;
History history = History.GetHistory();
//全局处理异常

//登录、初始化
TelegramBotClient bot = Bot.GetBot();
var me = await bot.GetMeAsync();
using CancellationTokenSource cts = new();
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    log.Panic(e.ExceptionObject);
};
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = new UpdateType[]
    {
        UpdateType.Unknown,
        UpdateType.Message,
        UpdateType.CallbackQuery,
        UpdateType.InlineQuery,
        UpdateType.ChosenInlineResult,
        UpdateType.ChannelPost,
    }
};
//开始接受消息
bot.StartReceiving(updateHandler: HandleUpdateAsync, pollingErrorHandler: HandlePollingErrorAsync, receiverOptions: receiverOptions, cancellationToken: cts.Token);
bot.OnMakingApiRequest += OnSendReq;
//记录发送的消息

//修改机器人指令
await bot.SetMyCommandsAsync(new[]
{
    new BotCommand()
    {
        Command = "about",
        Description = "关于"
    },
    new BotCommand()
    {
        Command = "user",
        Description = "获取自身用户ID"
    },
});
await bot.SetMyCommandsAsync(new[]
{   
    new BotCommand()
    {
        Command = "about",
        Description = "关于"
    },
    new BotCommand()
    {
        Command = "user",
        Description = "获取自身用户ID"
    },
     new BotCommand()
    {
        Command = "list",
        Description = "获取所有未审核的稿件",
    }
},new BotCommandScopeChat() { ChatId = config.Telegram.GroupID});
log.Info("已更新自身指令列表");

log.Info("Done! 输入stop终止程序");
TestCode();
while (true)
{
    var input = Console.ReadLine();
    if (input == "stop")
    {
        cts.Cancel();
        break;
    }
    else
    {
        log.Info("未知指令: ", input);
    }
}
log.Info("Stopping");

#endregion 初始化

void TestCode()
{
}

ValueTask OnSendReq(ITelegramBotClient botClient, Telegram.Bot.Args.ApiRequestEventArgs args, CancellationToken cancellationToken = default)
{
    if (args.Request is Telegram.Bot.Requests.SendMessageRequest smq)
    {
        log.Info($"发送 {smq.ChatId}：", smq.Text);
    }
    else if (args.Request is Telegram.Bot.Requests.SendAudioRequest saq)
    {
        log.Info($"发送 {saq.ChatId}：[Audio] {saq.Performer ?? "未知作曲家"} -  {saq.Title ?? "未知标题"} / {saq.Caption}");
    }
    return ValueTask.CompletedTask;
}

async Task HandleCommand(Update update)
{
    if (update?.Message?.Text is not { } content)
    {
        log.Warn("非指令参数被非法传递");
        return;
    }
    if (update.Message.From is null) return;
    if (update.Message.Chat.Type != ChatType.Private && !content.Contains(me.Username ?? throw new NullReferenceException()))
        return;
    long chatid = update.Message.Chat.Id;
    long from = update.Message.From.Id;

    if (content.StartsWith("/start"))
    {
        await bot.SendTextMessageAsync(chatid, $"欢迎使用音乐投稿机器人,如需投稿，请直接发送音乐平台链接或音频文件");
        return;
    }
    if (content.StartsWith("/chatid"))
    {
        await bot.SendTextMessageAsync(chatid, $"{chatid}");
        return;
    }
    if (content.StartsWith("/user"))
    {
        await bot.SendTextMessageAsync(chatid, $"{from}");
        return;
    }
    if (content.StartsWith("/add"))
    {
        if (!config.IsOwner(from))
        {
            log.Warn($"@{update.Message.From.Username} 尝试添加白名单，已拒绝。");
            return;
        }
        var args = content.Split(' ');
        if (args.Length >= 2)
        {
            long id;
            long.TryParse(args[1], out id);
            if (config.IsAdmin(id))
            {
                config.Save();
                await bot.SendTextMessageAsync(chatid, $"{id} 已经是管理员了");
            }
            else
            {
                config.Admins.Add(id);
                config.Save();
                await bot.SendTextMessageAsync(chatid, $"已添加 {id} 到白名单");
            }
        }
        return;
    }
    if (content.StartsWith("/del"))
    {
        if (!config.IsOwner(from))
        {
            log.Warn($"@{update.Message.From.Username} 尝试删除白名单，已拒绝。");
            return;
        }
        var args = content.Split(' ');
        if (args.Length >= 2)
        {
            long id;
            long.TryParse(args[1], out id);
            var text = "";
            if (!config.IsAdmin(id)) text = $"{id} 本身就不在白名单内";
            else
            {
                config.Admins.Remove(id);
                text = $"已从白名单移除 {id}";
                config.Save();
            }
            await bot.SendTextMessageAsync(chatid, text);
        }
        return;
    }
    if (content.StartsWith("/stop"))
    {
        if (cache.Remove(chatid))
            await bot.SendTextMessageAsync(chatid, "已清除正在进行的投稿任务");
        else
            await bot.SendTextMessageAsync(chatid, "暂无正在进行的投稿");
        Cache.Save();
    }
    if (content.StartsWith("/about"))
    {
        var inline = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithUrl("反馈","https://github.com/Akizon77/Otaku16Bot/issues/new"),
            }
        });
        await bot.SendTextMessageAsync(chatid, about, replyMarkup: inline);
        return;
    }
    if (content.StartsWith("/list"))
    {
        if (!config.IsAdmin(from))
        {
            return;
        }
        var posts = history.GetAllUnaduitPosts();
        string text = "";
        int i = 0;
        posts.ForEach(x =>
        {
            if (i < 30)
            {
                text += $"<a href=\"https://t.me/c/2056899506/{x.GroupMessageID}\">{x.Post.Title}</a>\n";
                i++;
            }
            else if(i == 30)
            {
                text += $"等 {posts.Count - i} 条";
            }
        });
        if (text == "") text = "暂无未审核的稿件";
        await bot.SendTextMessageAsync(chatid, text, replyToMessageId:update.Message.MessageId,parseMode:ParseMode.Html);
        return;
    }
}
async Task HandleInlineButtonCallback(Update update)
{
    if (update.CallbackQuery is not { } query)
    {
        log.Warn($"非Query回调事件被导航至HandleInlineCallback，栈堆:{new StackTrace()}");
        return;
    }
    if (query.Data is not { } data)
    {
        log.Warn($"回调数据为空！ID: {query.Id}, 投稿人: @{query.From.Username}");
        return;
    }

    if (data.StartsWith("anonymous"))
    {
        if (!cache.ContainsKey(query.From.Id))
        {
            await bot.SendTextMessageAsync(query.From.Id, "暂无进行中的投稿");
            return;
        }
        var post = cache[query.From.Id];
        switch (query.Data)
        {
            case "anonymous/true":
                post.Anonymous = true;
                break;

            case "anonymous/false":
                post.Anonymous = false;
                break;

            default:
                log.Warn("未知的Callback事件 ", query.Data);
                break;
        }
        cache[query.From.Id] = post;
        Cache.Save();
        await AskToFillInfo(update);
        return;
    }
    if (data.StartsWith("edit"))
    {
        if (!cache.ContainsKey(query.From.Id))
        {
            await bot.SendTextMessageAsync(query.From.Id, "暂无进行中的投稿");
            return;
        }
        var post = cache[query.From.Id];
        switch (data)
        {
            case "edit/title":
                post.Title = null;
                break;

            case "edit/performer":
                post.Author = null;
                break;

            default:
                log.Warn(post.UserName, "非法回调参数", data);
                break;
        }
        cache[query.From.Id] = post;
        Cache.Save();
        await AskToFillInfo(update);
        return;
    }
    if (data.StartsWith("tag"))
    {
        if (!cache.ContainsKey(query.From.Id))
        {
            await bot.SendTextMessageAsync(query.From.Id, "暂无进行中的投稿");
            return;
        }
        var post = cache[query.From.Id];
        switch (data)
        {
            case "tag/recommend":
                post.Tag = "推荐";
                break;

            case "tag/gift":
                post.Tag = "赠予";
                break;

            case "tag/message":
                post.Tag = "留言";
                break;

            default:
                log.Warn(post.UserName, "非法回调参数", data);
                break;
        }
        cache[query.From.Id] = post;
        Cache.Save();
        await AskToFillInfo(update);
        return;
    }
    if (data.StartsWith("aduit"))
    {
        if (!config.IsAdmin((update.CallbackQuery.From?.Id) ?? -1))
        {
            log.Warn($"@{update.CallbackQuery?.From?.Username} 正在尝试审核投稿，非白名单用户已拒绝。");
            return;
        }
        var args = data.Split('/');
        if (args.Length < 3) return;
        long id = 0;
        long.TryParse(args[1], out id);
        if (!history.His.ContainsKey(id))
        {
            log.Warn("无法找到id为", id, "的投稿");
            return;
        }
        var post = history.His[id];
        //已经通过了的情况
        if (post.Passed == true)
        {
            await bot.SendTextMessageAsync(config.Telegram.GroupID, $"@{update.CallbackQuery?.From?.Username} 这个稿件已经通过了", replyToMessageId: post.GroupMessageID);
            return;
        }
        else if (post.Passed == false)
        {
            await bot.SendTextMessageAsync(config.Telegram.GroupID, $"@{update.CallbackQuery?.From?.Username} 这个稿件已经被拒绝了", replyToMessageId: post.GroupMessageID);
            return;
        }
        switch (args[2])
        {
            case "pass":
                post.Passed = true;
                break;

            case "reject":
                post.Passed = false;
                break;

            default:
                log.Warn(id, "非法回调参数", data);
                break;
        }
        //通过或发送：通过消息
        if (post.Passed == true)
        {
            Message? sent = null;
            string text = post.ToString();
            var t = $"稿件 {post.Post.Title} 已通过";
            //发送到群组
            await bot.SendTextMessageAsync(config.Telegram.GroupID, $"@{update.CallbackQuery.From?.Username} :" + t, replyToMessageId: post.GroupMessageID);
            if (post.Post.FileID is { } fileid)
            {
                sent = await bot.SendAudioAsync(config.Telegram.ChannelID, InputFile.FromFileId(fileid), caption: text);
            }
            else
            {
                sent = await bot.SendTextMessageAsync(config.Telegram.ChannelID, text);
            }
            post.ChannelMessageID = sent.MessageId;
            await bot.SendTextMessageAsync(post.UserID, $"{t} - {config.Telegram.ChannelLink}/{post.ChannelMessageID}");
        }
        else
        {
            await bot.SendTextMessageAsync(config.Telegram.GroupID, $"@{update.CallbackQuery.From?.Username} :" + "已拒绝", replyToMessageId: post.GroupMessageID);
            await bot.SendTextMessageAsync(post.UserID, $"稿件 {post.Post.Title} 已被管理员拒绝");
        }
        //移除审核群的按钮
        await bot.EditMessageReplyMarkupAsync(config.Telegram.GroupID,post.GroupMessageID);
        history.His[id] = post;
        history.Save();
        return;
    }
}
async Task HandleText(Update update)
{
    if (update.Message is not { } message) return;
    if (update.Message.Text is not { } content) return;
    if (update.Message.From is not { } user) return;
    long chatid = message.Chat.Id;

    if (update.Message.Chat.Type == ChatType.Private &&content.StartsWith("http"))
    {
        cache[user.Id] = new()
        {
            Title = null,
            Author = null,
            FileID = null,
            UserName = $"@{user.Username}",
            Album = null,
            Link = content,
            Timestamp = TimeStamp.GetNow(),
        };
        Cache.Save();
        string text = "欢迎使用链接投稿\n使用 /stop 终止投稿";
        await bot.SendTextMessageAsync(
        chatId: chatid,
        text: text
        );
        await AskToFillInfo(update);
        return;
    }
    //发送文字消息就是补充信息
    if (!cache.ContainsKey(user.Id) && message.Chat.Type == ChatType.Private)
    {
        string text = "如需投稿，请直接发送平台链接或音频文件";
        await bot.SendTextMessageAsync(
         chatId: chatid,
         text: text
        );
        return;
    }
    else if (message.Chat.Type == ChatType.Private)
    {
        var post = cache[user.Id];
        if (post.Title is null)
            post.Title = content;
        else if (post.Author is null)
            post.Author = content;
        else if (post.Album is null)
            post.Album = content;
        else if (post.Comment is null)
            post.Comment = content;
        cache[user.Id] = post;
        Cache.Save();
        await AskToFillInfo(update);
        return;
    }
    //不是私聊 那就是转发消息
    else if (message.ReplyToMessage is { } origin)
    {
        if (!config.IsAdmin((update.Message.From?.Id) ?? -1))
        {
            log.Warn($"@{update.Message.From?.Username} 正在尝试转发管理员消息，非白名单用户已拒绝。");
            return;
        }
        long id = history.GetIDByMessageID(origin.MessageId);
        if (id == -1) return;
        var post = history.His[id];
        await bot.SendTextMessageAsync(post.UserID, $"来自管理员的消息: {message.Text}");
    }
}
async Task HandleAudio(Update update)
{
    if (update.Message?.Audio is not { } audio)
    {
        return;
    }
    //回复的式群组里的音频文件，那么就是补充文件投稿
    if (update.Message.Chat.Type != ChatType.Private && update.Message.ReplyToMessage is { } replyMessage)
    {
        if (!config.IsAdmin((update.Message.From?.Id) ?? -1))
        {
            log.Warn($"@{update.Message.From?.Username} 正在尝试补充投稿音频，非白名单用户已拒绝。");
            return;
        }
        //拿到投稿id
        var id = history.GetIDByMessageID(replyMessage.MessageId);
        if (id == -1) return;
        //通过id拿到稿件信息
        var post = history.His[id];
        //补充稿件的文件id
        post.Post.FileID = audio.FileId;
        history.His[id] = post;
        history.Save();
        //发送的消息初始化
        Message? sent = null;
        var text = post.ToString();
        //更新状态
        post.Passed = true;
        var t = $"稿件 {post.Post.Title} 已通过";
        //发送到群组已通过的信息
        await bot.SendTextMessageAsync(config.Telegram.GroupID, $"@{update.Message.From?.Username} :" + t, replyToMessageId: post.GroupMessageID);
        //发送文件到Channel
        if (post.Post.FileID is { } fileid)
            sent = await bot.SendAudioAsync(config.Telegram.ChannelID, InputFile.FromFileId(fileid), caption: text);
        else
            sent = await bot.SendTextMessageAsync(config.Telegram.ChannelID, text);
        post.ChannelMessageID = sent.MessageId;
        //告知歌曲名稿件已通过
        await bot.SendTextMessageAsync(post.UserID, $"{t} - {config.Telegram.ChannelLink}/{post.ChannelMessageID}");
        history.His[id] = post;
        history.Save();
        return;
    }
    //私聊里的音频就是新投稿
    if (update.Message.Chat.Type == ChatType.Private && update.Message.From is { } user)
    {
        //初始化投稿信息
        cache[user.Id] = new()
        {
            Title = audio.Title,
            Author = audio.Performer,
            FileID = audio.FileId,
            UserName = $"@{user.Username}",
            Album = null,
            Link = null,
            Timestamp = TimeStamp.GetNow(),
        };
        Cache.Save();
        string text = "欢迎使用音频文件投稿";
        //修改信息的提示
        List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
        if (audio.Title is not null)
        {
            text += $"\n标题: `{audio.Title}` ";
            buttons.Add(InlineKeyboardButton.WithCallbackData("修改标题", "edit/title"));
        }
        if (audio.Performer is not null)
        {
            text += $"\n艺术家名: `{audio.Performer}` ";
            buttons.Add(InlineKeyboardButton.WithCallbackData("修改艺术家名", "edit/performer"));
        }
        InlineKeyboardMarkup inline = new(new[] { buttons.ToArray() });
        text += $"\n随时可使用 /stop 终止投稿";
        //发送稿件基本信息
        await bot.SendTextMessageAsync(
        chatId: user.Id,
        text: text,
        replyMarkup: inline,
        parseMode: ParseMode.MarkdownV2
        );
        await AskToFillInfo(update);
    }
}
//查询未补全的信息，并询问
async Task AskToFillInfo(Update update)
{
    #region 拿chatid等

    var msg = update.Message;
    if (msg is null && update.CallbackQuery is null) return;
#pragma warning disable CS8602 // 解引用可能出现空引用。
    var user = msg?.From?.Id is null ? update.CallbackQuery.From.Id : msg.Chat.Id;
#pragma warning restore CS8602 // 解引用可能出现空引用。

    #endregion 拿chatid等

    //如没有发起投稿，则退出
    if (!cache.ContainsKey(user)) return;
    //拿投稿信息
    var post = cache[user];
    string text = "";
    IReplyMarkup inline = new ReplyKeyboardRemove();
    //要求补全
    if (post.Title is null)
        text = "请补充歌曲标题";
    else if (post.Author is null)
        text = "请补充艺术家名";
    else if (post.Album is null)
        text = "请补充专辑，如无专辑请回复单曲";
    else if (post.Tag is null)
    {
        inline = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("推荐","tag/recommend"),
                InlineKeyboardButton.WithCallbackData("赠予","tag/gift"),
                InlineKeyboardButton.WithCallbackData("留言","tag/message")
            }
        });
        text = "请补充投稿类型";
    }
    else if (post.Comment is null)
        text = "请补充附言";
    else if (post.Anonymous is null)
    {
        text = "是否匿名投稿？";
        //Inline 按钮
        inline = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("×匿名","anonymous/true"),
                InlineKeyboardButton.WithCallbackData("√保留来源","anonymous/false")
            }
        });
    }

    //当填的都填完了
    else
    {
        text = "完成投稿！将在审核后通过";
        await bot.SendTextMessageAsync(user, text);
        //转发给审核
        text = post.ToString();
        //投稿完成，处理信息

        //转发消息到审核群
        Message? sent = null;
        if (post.Link is not null)
        {
            inline = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("通过",$"aduit/{post.Timestamp}/pass"),
                    InlineKeyboardButton.WithCallbackData("拒绝",$"aduit/{post.Timestamp}/reject"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("回复音频文件通过",$"reserved"),
                }
            });
            await bot.SendTextMessageAsync(user, "预览投稿：\n" + text);
            sent = await bot.SendTextMessageAsync(config.Telegram.GroupID, text, replyMarkup: inline);
        }
        else if (post.FileID is not null)
        {
            inline = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("通过",$"aduit/{post.Timestamp}/pass"),
                    InlineKeyboardButton.WithCallbackData("拒绝",$"aduit/{post.Timestamp}/reject"),
                }
            });
            await bot.SendAudioAsync(
             chatId: user,
             InputFile.FromFileId(post.FileID),
             caption: "预览：\n" + text
             );
            sent = await bot.SendAudioAsync(
             chatId: config.Telegram.GroupID,
             InputFile.FromFileId(post.FileID),
             replyMarkup: inline,
             caption: text
             );
        }
        else
        {
            log.Warn("无法转发消息");
        }

        //保存到历史投稿
        history.His[post.Timestamp] = new()
        {
            UserID = user,
            Post = post,
            GroupMessageID = (sent?.MessageId) ?? 0,
            Passed = null,
            ChannelMessageID = 0,
        };
        history.Save();
        //随后缓存移除本投稿信息
        cache.Remove(user);
        Cache.Save();
        return;
    };
    await bot.SendTextMessageAsync(user, text, replyMarkup: inline);
}
//新消息的处理
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    //打印消息日志
    log.Message(update);
    //处理点击按钮的事件
    if (update.CallbackQuery is not null)
    {
        await HandleInlineButtonCallback(update);
        return;
    }
    else if (update.Message?.Text is { } text)
    {
        if (text.StartsWith('/'))
            await HandleCommand(update);
        else
            await HandleText(update);
        return;
    }
    else if (update.Message?.Audio is not null)
    {
        await HandleAudio(update);
        return;
    }
    var msg = update.Message;
}
//处理TGAPI错误
Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"API Error:{apiRequestException.ErrorCode} - {apiRequestException.Message}",
        _ => exception.ToString()
    };
    log.Panic("与Telegram通信时发生错误:", ErrorMessage);
    return Task.CompletedTask;
}