using Otaku16.Data;
using Otaku16.Service;
using Otaku16.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otaku16
{
    public class Handler
    {
        private Logger log = new("Bot");
        TelegramBotClient Bot;
        User Me;
        Config config;
        History history;
        Cache cache;
        public Handler(Bot bot,Config config,Cache cache,History history)
        {
            Bot = bot.bot;Me = bot.Me;
            this.config = config;
            this.history = history;
            this.cache = cache;
            using CancellationTokenSource cts = new();

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
            Bot.StartReceiving(updateHandler: HandleUpdateAsync, pollingErrorHandler: HandlePollingErrorAsync, receiverOptions: receiverOptions, cancellationToken: cts.Token);
            Bot.OnMakingApiRequest += OnSendReq;
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

        async Task HandleCommand(Update update)
        {
            if (update?.Message?.Text is not { } content)
            {
                log.Warn("非指令参数被非法传递");
                return;
            }
            if (update.Message.From is null) return;
            if (update.Message.Chat.Type != ChatType.Private && !content.Contains(Me.Username ?? throw new NullReferenceException()))
                return;
            long chatid = update.Message.Chat.Id;
            long from = update.Message.From.Id;

            if (content.StartsWith("/start"))
            {
                await Bot.SendTextMessageAsync(chatid, $"欢迎使用音乐投稿机器人,如需投稿，请直接发送音乐平台链接或音频文件");
                return;
            }
            if (content.StartsWith("/chatid"))
            {
                await Bot.SendTextMessageAsync(chatid, $"{chatid}");
                return;
            }
            if (content.StartsWith("/user"))
            {
                await Bot.SendTextMessageAsync(chatid, $"{from}");
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
                        await Bot.SendTextMessageAsync(chatid, $"{id} 已经是管理员了");
                    }
                    else
                    {
                        config.data.Admins.Add(id);
                        config.Save();
                        await Bot.SendTextMessageAsync(chatid, $"已添加 {id} 到白名单");
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
                        config.data.Admins.Remove(id);
                        text = $"已从白名单移除 {id}";
                        config.Save();
                    }
                    await Bot.SendTextMessageAsync(chatid, text);
                }
                return;
            }
            if (content.StartsWith("/stop"))
            {
                if (cache.Data.Remove(chatid))
                    await Bot.SendTextMessageAsync(chatid, "已清除正在进行的投稿任务");
                else
                    await Bot.SendTextMessageAsync(chatid, "暂无正在进行的投稿");
                cache.Save();
            }
            if (content.StartsWith("/about"))
            {
                var inline = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("完整更新日志","https://github.com/Akizon77/Otaku16Bot/releases"),
                    },
                    new[]
                    {
                         InlineKeyboardButton.WithUrl("反馈","https://github.com/Akizon77/Otaku16Bot/issues/new"),
                    }
                });
                await Bot.SendTextMessageAsync(chatid, Const.about, replyMarkup: inline, parseMode: ParseMode.Html);
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
                    else if (i == 30)
                    {
                        text += $"等 {posts.Count - i} 条";
                    }
                });
                if (text == "") text = "暂无未审核的稿件";
                await Bot.SendTextMessageAsync(chatid, text, replyToMessageId: update.Message.MessageId, parseMode: ParseMode.Html);
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
                log.Warn($"回调数据为空！ID: {query.Id}, 来自: {Tools.Telegram.GetName(query.From)}");
                return;
            }

            if (data.StartsWith("anonymous"))
            {
                if (!cache.Data.ContainsKey(query.From.Id))
                {
                    await Bot.SendTextMessageAsync(query.From.Id, "暂无进行中的投稿");
                    return;
                }
                var post = cache.Data[query.From.Id];
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
                cache.Data[query.From.Id] = post;
                cache.Save();
                try
                {
                    await Bot.EditMessageTextAsync(query.From.Id, query.Message.MessageId, $"匿名状态当前已选择：{((post.Anonymous??false)?"匿名":"保留来源")}");
                }
                catch (Exception) { }
                await AskToFillInfo(update);
                return;
            }
            if (data.StartsWith("edit"))
            {
                if (!cache.Data.ContainsKey(query.From.Id))
                {
                    await Bot.SendTextMessageAsync(query.From.Id, "暂无进行中的投稿");
                    return;
                }
                var post = cache.Data[query.From.Id];
                switch (data)
                {
                    case "edit/title":
                        post.Title = null;
                        break;

                    case "edit/performer":
                        post.Author = null;
                        break;
                    case "edit/album/null":
                        post.Album = "单曲";
                        await Bot.EditMessageTextAsync(query.From.Id,query.Message.MessageId,"该曲目是单曲，无专辑");
                        break;
                    default:
                        log.Warn(post.UserName, "非法回调参数", data);
                        break;
                }
                cache.Data[query.From.Id] = post;
                cache.Save();
                await AskToFillInfo(update);
                return;
            }
            if (data.StartsWith("tag"))
            {
                if (!cache.Data.ContainsKey(query.From.Id))
                {
                    await Bot.SendTextMessageAsync(query.From.Id, "暂无进行中的投稿");
                    return;
                }
                var post = cache.Data[query.From.Id];
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
                    case "tag/daily":
                        post.Tag = "每日推荐";
                        break;
                    default:
                        log.Warn(post.UserName, "非法回调参数", data);
                        break;
                }
                cache.Data[query.From.Id] = post;
                cache.Save();
                try
                {
                    await Bot.EditMessageTextAsync(query.From.Id, query.Message.MessageId, $"类型当前已选择：{post.Tag}");
                }
                catch (Exception) { }
                await AskToFillInfo(update);
                return;
            }
            if (data.StartsWith("del"))
            {
                if (data == "del/btn")
                {
                    await Bot.EditMessageReplyMarkupAsync(query.Message.Chat.Id, query.Message.MessageId);
                }
            }
            if (data.StartsWith("aduit"))
            {
                if (!config.IsAdmin((update.CallbackQuery.From?.Id) ?? -1))
                {
                    log.Warn($"{Tools.Telegram.GetName(query.From)} 正在尝试审核投稿，非白名单用户已拒绝。");
                    return;
                }
                var args = data.Split('/');
                if (args.Length < 3) return;
                long id = 0;
                long.TryParse(args[1], out id);
                if (!history.Data.ContainsKey(id))
                {
                    log.Warn("无法找到id为", id, "的投稿");
                    return;
                }
                var post = history.Data[id];
                //已经通过了的情况
                if (post.Passed == true)
                {
                    await Bot.SendTextMessageAsync(config.data.Telegram.GroupID, $"{Tools.Telegram.GetName(query.From)} 这个稿件已经通过了", replyToMessageId: post.GroupMessageID);
                    return;
                }
                else if (post.Passed == false)
                {
                    await Bot.SendTextMessageAsync(config.data.Telegram.GroupID, $"{Tools.Telegram.GetName(query.From)} 这个稿件已经被拒绝了", replyToMessageId: post.GroupMessageID);
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
                    post = await Pass(Tools.Telegram.GetName(query.From), post);
                }
                else
                {
                    await Reject(Tools.Telegram.GetName(query.From), post);
                }
                history.Data[id] = post;
                history.Save();
                return;
            }
        }

        private async Task Reject(string from, HistoryTable post)
        {
            await Bot.SendTextMessageAsync(config.data.Telegram.GroupID, $"{from} :" + "已拒绝", replyToMessageId: post.GroupMessageID);
            await Bot.SendTextMessageAsync(post.UserID, $"稿件 {post.Post.Title} 已被管理员拒绝");
            //移除审核群的按钮
            await Bot.EditMessageReplyMarkupAsync(config.data.Telegram.GroupID, post.GroupMessageID);
        }

        /// <summary>
        /// 通过投稿
        /// </summary>
        /// <param name="update"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        private async Task<HistoryTable> Pass(string from, HistoryTable post)
        {
            Message? sent = null;
            string text = post.ToString();
            var t = $"稿件 {post.Post.Title} 已通过";
            //发送到群组
            await Bot.SendTextMessageAsync(config.data.Telegram.GroupID, $"{from} :" + t, replyToMessageId: post.GroupMessageID);
            if (post.Post.FileID is { } fileid)
            {
                sent = await Bot.SendAudioAsync(config.data.Telegram.ChannelID, InputFile.FromFileId(fileid), caption: text);
            }
            else
            {
                sent = await Bot.SendTextMessageAsync(config.data.Telegram.ChannelID, text);
            }
            post.ChannelMessageID = sent.MessageId;
            await Bot.SendTextMessageAsync(post.UserID, $"{t} - {config.data.Telegram.ChannelLink}/{post.ChannelMessageID}");
            //移除审核群的按钮
            await Bot.EditMessageReplyMarkupAsync(config.data.Telegram.GroupID, post.GroupMessageID);
            return post;
        }

        async Task HandleText(Update update)
        {
            if (update.Message is not { } message) return;
            if (update.Message.Text is not { } content) return;
            if (update.Message.From is not { } user) return;
            long chatid = message.Chat.Id;

            if (update.Message.Chat.Type == ChatType.Private && content.StartsWith("http"))
            {
                string text = "欢迎使用链接投稿\n使用 /stop 终止投稿";
                Post post = NewPost(user);
                post.Link = content;
                int id = Netease.GetIdFromUrl(update.Message.Text);
                if(id != 0)
                {
                    post = await Netease.AutoFill(post, id);
                    text += $"\n当前是网易云链接";
                    text += $"{(post.Title is null ? "" : "\n标题: " + post.Title)}";
                    text += $"{(post.Author is null ? "" : "\n艺术家: " + post.Author)}";
                    text += $"{(post.Album is null ? "" : "\n专辑: " + post.Album)}";
                }
                else if (update.Message.Text.StartsWith("https://i.y.qq.com"))
                {
                    post = QQMusic.AutoFill(post, update.Message.Text);
                    text += $"\n当前是QQ音乐分享链接";
                    text += $"{(post.Title is null ? "" : "\n标题: " + post.Title)}";
                    text += $"{(post.Author is null ? "" : "\n艺术家: " + post.Author)}";
                    text += $"{(post.Album is null ? "" : "\n专辑: " + post.Album)}";
                }
                cache.Data[user.Id] = post;
                cache.Save();
                await Bot.SendTextMessageAsync(
                chatId: chatid,
                text: text
                );
                await AskToFillInfo(update);
                return;
            }
            //发送文字消息就是补充信息
            if (!cache.Data.ContainsKey(user.Id) && message.Chat.Type == ChatType.Private)
            {
                string text = "如需投稿，请直接发送平台链接或音频文件";
                await Bot.SendTextMessageAsync(
                 chatId: chatid,
                 text: text
                );
                return;
            }
            else if (message.Chat.Type == ChatType.Private)
            {
                var post = cache.Data[user.Id];
                if (post.Title is null)
                    post.Title = content;
                else if (post.Author is null)
                    post.Author = content;
                else if (post.Album is null)
                    post.Album = content;
                else if (post.Comment is null)
                    post.Comment = content;
                cache.Data[user.Id] = post;
                cache.Save();
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
                var post = history.Data[id];
                await Bot.SendTextMessageAsync(post.UserID, $"来自管理员的消息: {message.Text}");
                await Bot.SendTextMessageAsync(update.Message.Chat.Id, $"消息已转发至投稿人",replyToMessageId:update.Message.MessageId);
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
                var post = history.Data[id];
                //已通过or拒绝
                if (post.Passed != null){
                    await Bot.SendTextMessageAsync(update.Message.Chat.Id,$"稿件 {post.Post.Title} 已经 {(post.Passed == true?"通过":"拒绝")} 了",replyToMessageId:update.Message.MessageId);
                    return;
                }
                //补充稿件的文件id
                post.Post.FileID = audio.FileId;
                history.Data[id] = post;
                history.Save();
                //发送的消息初始化
                Message? sent = null;
                var text = post.ToString();
                //更新状态
                post.Passed = true;
                post = await Pass(Tools.Telegram.GetName(update.Message.From), post);
                history.Data[id] = post;
                history.Save();
                return;
            }
            //私聊里的音频就是新投稿
            if (update.Message.Chat.Type == ChatType.Private && update.Message.From is { } user)
            {
                var post = NewPost( user);
                post.Title = audio.Title;
                post.Author = audio.Performer;
                post.FileID = audio.FileId;
                cache.Data[user.Id] = post;
                cache.Save();
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
                InlineKeyboardMarkup inline;
                if (buttons.Count == 0) 
                {
                     inline = new(new[] { buttons.ToArray() });
                }
                else
                {
                    inline = new(new[] { buttons.ToArray(), new[] { InlineKeyboardButton.WithCallbackData("❌删除按钮", "del/btn") } });
                }
                
                text += $"\n随时可使用 /stop 终止投稿";
                //发送稿件基本信息
                await Bot.SendTextMessageAsync(
                chatId: user.Id,
                text: text,
                replyMarkup: inline,
                parseMode: ParseMode.MarkdownV2
                );
                await AskToFillInfo(update);
            }
        }

        private Post NewPost(User user)
        {
            //初始化投稿信息
            return new()
            {
                Title = null,
                Author = null,
                FileID = null,
                UserName = Tools.Telegram.GetName(user),
                Album = null,
                Link = null,
                Timestamp = TimeStamp.GetNow(),
            };
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
            if (!cache.Data.ContainsKey(user)) return;
            //拿投稿信息
            var post = cache.Data[user];
            string text = "";
            IReplyMarkup inline = new ReplyKeyboardRemove();
            //要求补全
            if (post.Title is null)
                text = "请补充歌曲标题";
            else if (post.Author is null)
                text = "请补充艺术家名";
            else if (post.Album is null) 
            { 
                text = "请补充专辑";
                inline = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("此曲目是单曲","edit/album/null")

                    }
                });
            }
            else if (post.Tag is null)
            {
                if (config.IsOwner((msg?.From?.Id)??0))
                {
                    inline = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("推荐","tag/recommend"),
                            InlineKeyboardButton.WithCallbackData("赠予","tag/gift"),
                            InlineKeyboardButton.WithCallbackData("留言","tag/message")
   
                        },
                        new[]
                        {
                             InlineKeyboardButton.WithCallbackData("每日推荐","tag/daily")
                        }
                    });
                }
                else
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
                }
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
                        InlineKeyboardButton.WithCallbackData("❌匿名","anonymous/true"),
                        InlineKeyboardButton.WithCallbackData("✅保留来源","anonymous/false")
                    }
                });
            }

            //当填的都填完了
            else
            {
                text = "感谢支持，审核结果将在稍后通知";
                await Bot.SendTextMessageAsync(user, text);
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
                    //await Bot.SendTextMessageAsync(user, "预览投稿：\n" + text);
                    sent = await Bot.SendTextMessageAsync(config.data.Telegram.GroupID, text, replyMarkup: inline);
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
                    //await Bot.SendAudioAsync(
                    //    chatId: user,
                    //     InputFile.FromFileId(post.FileID),
                    //    caption: "预览：\n" + text
                    //    );
                    sent = await Bot.SendAudioAsync(
                        chatId: config.data.Telegram.GroupID,
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
                history.Data[post.Timestamp] = new()
                {
                    UserID = user,
                    Post = post,
                    GroupMessageID = (sent?.MessageId) ?? 0,
                    Passed = null,
                    ChannelMessageID = 0,
                };
                history.Save();
                //随后缓存移除本投稿信息
                cache.Data.Remove(user);
                cache.Save();
                return;
            };
            await Bot.SendTextMessageAsync(user, text, replyMarkup: inline);
        }
        //新消息的处理

        //处理TGAPI错误
        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"API Error:{apiRequestException.ErrorCode} - {apiRequestException.Message}",
                _ => exception.ToString()
            };
            Debugger.Break();
            log.Error(ErrorMessage);
            Hosting.Stop();
            return Task.CompletedTask;
        }
    }
}
