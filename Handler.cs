using Otaku16.Data;
using Otaku16.Service;
using Otaku16.Tools;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otaku16
{
    public class Handler
    {
        private Logger log = new("Bot");
        private TelegramBotClient Bot;
        private User Me;
        private Config config;
        private History history;
        private Cache cache;

        public Handler(Bot bot, Config config, Cache cache, History history)
        {
            Bot = bot.bot; Me = bot.Me;
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

        private ValueTask OnSendReq(ITelegramBotClient botClient, Telegram.Bot.Args.ApiRequestEventArgs args, CancellationToken cancellationToken = default)
        {
            if (args.Request is Telegram.Bot.Requests.SendMessageRequest smq)
            {
                log.Info($"发送 {smq.ChatId}：", smq.Text);
            }
            else if (args.Request is Telegram.Bot.Requests.SendAudioRequest saq)
            {
                log.Info($"发送 {saq.ChatId}：[Audio] {saq.Caption}");
            }
            else if (args.Request is Telegram.Bot.Requests.EditMessageTextRequest emtr)
            {
                log.Info($"修改消息 {emtr.Text}");
            }
            else if (args.Request is Telegram.Bot.Requests.EditInlineMessageTextRequest eitr)
            {
                log.Info($"修改内联消息 {eitr.Text}");
            }
            else if (args.Request is Telegram.Bot.Requests.EditMessageReplyMarkupRequest emrmr)
            {
                log.Info($"修改内联消息");
            }
            return ValueTask.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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

        private async Task HandleCommand(Update update)
        {
            if (update?.Message is not { } message) return;
            if (message.Text is not { } content)return;
            if (message.From is not { } user) return;
            if (message.Chat.Type != ChatType.Private && !content.Contains(Me.Username!)) return;
            long chatid = message.Chat.Id;
            long from = user.Id;

            if (content.StartsWith("/start"))
            {
               message.FastReply( $"欢迎使用音乐投稿机器人,如需投稿,请直接发送分享链接或音频文件");
                return;
            }
            if (content.StartsWith("/chatid"))
            {
                message.FastReply( $"<code>{chatid}</code>");
                return;
            }
            if (content.StartsWith("/user"))
            {
                message.FastReply( $"<code>{from}</code>");
                return;
            }
            if (content.StartsWith("/add"))
            {
                if (!config.IsOwner(from)) return;
                var args = content.Split(' ');
                if (args.Length >= 2)
                {
                    long id;
                    long.TryParse(args[1], out id);
                    if (config.IsAdmin(id))
                    {
                        config.Save();
                        message.FastReply($"{id} 已经是管理员了");
                    }
                    else
                    {
                        config.data.Admins.Add(id);
                        config.Save();
                        message.FastReply( $"已添加 {id} 到白名单");
                    }
                }
                return;
            }
            if (content.StartsWith("/del"))
            {
                if (!config.IsOwner(from)) return;
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
                    message.FastReply(text);
                }
                return;
            }
            if (content.StartsWith("/stop"))
            {
                if (cache.Data.Remove(chatid))
                    message.FastReply("已清除正在进行的投稿任务");
                else
                    message.FastReply("暂无正在进行的投稿");
                cache.Save();
            }
            if (content.StartsWith("/about"))
            {
                var inline = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("更新日志","https://github.com/Akizon77/Otaku16Bot/releases"),
                         InlineKeyboardButton.WithUrl("反馈","https://github.com/Akizon77/Otaku16Bot/issues/new"),
                    }
                });
                await Bot.SendTextMessageAsync(chatid, Const.about, replyToMessageId: message.MessageId, replyMarkup: inline, parseMode: ParseMode.Html);
                return;
            }
            if (content.StartsWith("/list"))
            {
                if (!config.IsAdmin(from)) return;
                string text = Commands.List.GetPage(0);
                IReplyMarkup replyMarkup = new ReplyKeyboardRemove();
                if (history.GetAllUnaduitPosts().Count > 10)
                {
                    replyMarkup = new InlineKeyboardMarkup(new[]
                    {
                        InlineKeyboardButton.WithCallbackData("▶️ Next Page","cmd/list/page/1"),
                    });
                }
                if (text == "") text = "当前暂无未审核稿件";
                await Bot.SendTextMessageAsync(chatid, text, replyToMessageId: message.MessageId, parseMode: ParseMode.Html,replyMarkup:replyMarkup);
                return;
            }
        }

        private async Task HandleInlineButtonCallback(Update update)
        {
            if (update.CallbackQuery is not { } query) return;
            if (query.Data is not { } data) return;
            if (query.Message is not { } message) return;
            Bot.AnswerCallbackQueryAsync(query.Id);
            var chatid = message.Chat.Id;
            if (data.StartsWith("anonymous"))
            {
                if (!cache.Data.ContainsKey(query.From.Id))
                {
                    message.FastEdit("此投稿已超时或取消");
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
                message.FastEdit($"匿名状态当前已选择：{((post.Anonymous ?? false) ? "匿名" : "保留来源")}");
                await AskToFillInfo(update);
                return;
            }
            if (data.StartsWith("edit"))
            {
                if (!cache.Data.ContainsKey(query.From.Id))
                {
                    message.FastEdit("此投稿已超时或取消");
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
                        message.FastEdit("该曲目是单曲，无专辑");
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
                    message.FastEdit("此投稿已超时或取消");
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
                message.FastEdit($"类型当前已选择：{post.Tag}");
                await AskToFillInfo(update);
                return;
            }
            if (data.StartsWith("del"))
            {
                if (data == "del/btn")
                {
                    message.RemoveInlineButton();
                }
            }
            if (data.StartsWith("aduit"))
            {
                if (!config.IsAdmin((update.CallbackQuery.From?.Id) ?? -1)) return;
                var args = data.Split('/');
                if (args.Length < 3) return;
                long id = 0;
                //异常情况处理
                if (!long.TryParse(args[1], out id))
                {
                    message.FastReply($"无法解析稿件 {args[1]}");
                    return;
                }
                if (!history.Data.ContainsKey(id))
                {
                    message.FastReply($"无法找到稿件 {id}");
                    return;
                }
                var post = history.Data[id];
                //已经通过了的情况
                if (post.Passed is not null)
                {
                    await message.RemoveInlineButton();
                    message.FastReply($"{query.From.GetName()} 这个稿件已经{(post.Passed == true?"通过":"拒绝")}了");
                    return;
                }

                switch (args[2])
                {
                    case "pass":
                        post.Passed = true;
                        post = await Pass(post);
                        message.FastAddString($"此稿件由 {query.From.GetName()} 通过");
                        break;
                    case "reject":
                        post.Passed = false;
                        await Reject(post);
                        message.FastAddString($"此稿件由 {query.From.GetName()} 拒绝");
                        break;
                    case "silentreject":
                        post.Passed = false;
                        message.RemoveInlineButton();
                        message.FastAddString($"此稿件由 {query.From.GetName()} 静默拒绝");
                        break;
                    default:
                        log.Warn(id, "非法回调参数", data);
                        break;
                }
                //通过或发送：通过消息
                history.Data[id] = post;
                history.Save();
                return;
            }
            if (data.StartsWith("post"))
            {
                var user = query.From.Id;
                if (!cache.Data.ContainsKey(user))
                {
                    message.FastEdit("ERR:此稿件已被处理或不存在");
                    return;
                }
                var post = cache.Data[query.From.Id];
                switch (data)
                {
                    case "post/pubdirect":
                        message.FastEdit("稿件将直接发布");

                        Message sent;
                        if (post.FileID is { } fileid)
                        {
                            sent = await Bot.SendAudioAsync(config.data.Telegram.ChannelID, InputFile.FromFileId(fileid), caption: post.ToString());
                        }
                        else
                        {
                            sent = await Bot.SendTextMessageAsync(config.data.Telegram.ChannelID, post.ToString());
                        }
                        history.Data[post.Timestamp] = new()
                        {
                            UserID = user,
                            Post = post,
                            GroupMessageID = 0,
                            Passed = true,
                            ChannelMessageID = sent.MessageId,
                        };
                        history.Save();
                        //随后缓存移除本投稿信息
                        cache.Data.Remove(user);
                        cache.Save();
                        message.FastReply( $"稿件 {post.Title} 已发布 - {config.data.Telegram.ChannelLink}/{sent.MessageId}");
                        break;
                    case "post/aduit":
                        message.FastEdit("稿件将正常审核");
                        await SendForAduit(query.From.Id);
                        break;
                }
            }
            if (data.StartsWith("cmd/list/page"))
            {
                int.TryParse(data.Split('/').Last(), out int page);
                if (page < 0) return;
                while (page * 10 > history.GetAllUnaduitPosts().Count)
                {
                    page--;
                    if (page < 0)
                    {
                        message.FastEdit("暂无未审核消息");
                        return;
                    }
                }
                var body = Commands.List.GetPage(page);
                if (body == "")
                {
                    message.FastEdit("暂无未审核消息");
                    return;
                }
                InlineKeyboardMarkup replyMarkup;
                List<InlineKeyboardButton> buttons = new ();
                if (page > 0) buttons.Add(InlineKeyboardButton.WithCallbackData("◀️ Prev Page", $"cmd/list/page/{page - 1}"));
                if ((page + 1) * 10 < history.GetAllUnaduitPosts().Count) buttons.Add(InlineKeyboardButton.WithCallbackData("▶️ Next Page", $"cmd/list/page/{page + 1}"));
                replyMarkup = new InlineKeyboardMarkup(buttons.ToArray());
                message.FastEdit("当前未审核的稿件有\n"+body, replyMarkup);
            }
        }

        private async Task Reject(HistoryTable post)
        {
            await Bot.SendTextMessageAsync(post.UserID, $"稿件 {post.Post.Title} 已被管理员拒绝");
        }

        /// <summary>
        /// 通过投稿
        /// </summary>
        /// <param name="update"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        private async Task<HistoryTable> Pass(HistoryTable post)
        {
            Message? sent = null;
            string text = post.ToString();
            if (post.Post.FileID is { } fileid)
            {
                sent = await Bot.SendAudioAsync(config.data.Telegram.ChannelID, InputFile.FromFileId(fileid), caption: text);
            }
            else
            {
                sent = await Bot.SendTextMessageAsync(config.data.Telegram.ChannelID, text);
            }
            post.ChannelMessageID = sent.MessageId;
            Bot.SendTextMessageAsync(post.UserID, $"稿件 {post.Post.Title} 已通过 - {config.data.Telegram.ChannelLink}/{post.ChannelMessageID}");
            //移除审核群的按钮
            return post;
        }

        private async Task HandleText(Update update)
        {
            if (update.Message is not { } message) return;
            if (update.Message.Text is not { } content) return;
            if (update.Message.From is not { } user) return;
            long chatid = message.Chat.Id;

            if (message.Chat.Type == ChatType.Private && content.StartsWith("http"))
            {
                string text = "欢迎使用链接投稿";
                Post post = NewPost(user);
                post.Link = content;
                int id = Netease.GetIdFromUrl(message.Text);
                if (id != 0)
                {
                    post = await Netease.AutoFill(post, id);
                    text += $"\n当前是网易云链接";
                    text += $"{(post.Title is null ? "" : "\n标题: " + post.Title)}";
                    text += $"{(post.Author is null ? "" : "\n艺术家: " + post.Author)}";
                    text += $"{(post.Album is null ? "" : "\n专辑: " + post.Album)}";
                }
                else if (message.Text.StartsWith("https://i.y.qq.com"))
                {
                    post = QQMusic.AutoFill(post, message.Text);
                    text += $"\n当前是QQ音乐分享链接";
                    text += $"{(post.Title is null ? "" : "\n标题: " + post.Title)}";
                    text += $"{(post.Author is null ? "" : "\n艺术家: " + post.Author)}";
                    text += $"{(post.Album is null ? "" : "\n专辑: " + post.Album)}";
                }
                text += "\n随时可使用 /stop 终止投稿";
                cache.Data[user.Id] = post;
                cache.Save();
                await message.FastReply(text);
                await AskToFillInfo(update);
                return;
            }
            //发送文字消息就是补充信息
            if (!cache.Data.ContainsKey(user.Id) && message.Chat.Type == ChatType.Private)
            {
                string text = "如需投稿，请直接发送平台链接或音频文件";
                message.FastReply(text);
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
                {
                    post.Album = content;
                    Bot.EditMessageReplyMarkupAsync(message.Chat.Id, message.MessageId - 1);
                }
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
                Bot.SendTextMessageAsync(post.UserID, $"来自管理员的消息: {message.Text}");
                message.FastReply( $"消息已转发至投稿人");
            }
        }

        private async Task HandleAudio(Update update)
        {
            if (update.Message is not { } message)return;
            if (update.Message.Audio is not { } audio) return;
            if (update.Message.From is not { } user) return;
            
            //回复的式群组里的音频文件，那么就是补充文件投稿
            if (message.Chat.Type != ChatType.Private && message.ReplyToMessage is { } replyMessage)
            {
                if (!config.IsAdmin((message.From?.Id) ?? -1))
                {
                    log.Warn($"@{message.From?.Username} 正在尝试补充投稿音频，非白名单用户已拒绝。");
                    return;
                }
                //拿到投稿id
                var id = history.GetIDByMessageID(replyMessage.MessageId);
                if (id == -1) return;
                //通过id拿到稿件信息
                var post = history.Data[id];
                //已通过or拒绝
                if (post.Passed != null)
                {
                    message.FastReply( $"稿件 <a href=\"{config.data.Telegram.GroupLink}/{post.ChannelMessageID}\">{post.Post.Title}</a> 已经 {(post.Passed == true ? "通过" : "拒绝")} 了");
                    return;
                }
                if (post.Post.FileID != null)
                {
                    message.FastReply($"稿件 {post.Post.Title} 已经包含了音频文件");
                    return;
                }
                //补充稿件的文件id
                post.Post.FileID = audio.FileId;
                //更新稿件状态
                post.Passed = true;
                //移除审核按钮
                Bot.EditMessageReplyMarkupAsync(config.data.Telegram.GroupID, post.GroupMessageID);
                //通过稿件
                post = await Pass(post);
                Bot.SendTextMessageAsync(message.Chat.Id,$"{user.GetName()} 使用音频文件通过了稿件",replyToMessageId:post.GroupMessageID);
                history.Data[id] = post;
                history.Save();
                return;
            }
            //私聊里的音频就是新投稿
            if (update.Message.Chat.Type == ChatType.Private)
            {
                var post = NewPost(user);
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
                UserName = user.GetName(),
                Album = null,
                Link = null,
                Timestamp = TimeStamp.GetNow(),
            };
        }

        //查询未补全的信息，并询问
        private async Task AskToFillInfo(Update update)
        {
            var msg = update.Message;
            if (msg is null && update.CallbackQuery is null) return;
            var user = msg?.From?.Id is null ? update.CallbackQuery!.From.Id : msg.Chat.Id;
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
                if (config.IsOwner(user))
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
                if (config.IsOwner(user))
                {
                    text = "您拥有直接投稿权限，是否直接将稿件发布？";
                    text += $"{(post.FileID is null ? "\n提示：当前无音频文件" : "")}";
                    inline = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("✅直接发布","post/pubdirect"),
                            InlineKeyboardButton.WithCallbackData("❌否","post/aduit"),
                        }
                    });
                    Bot.SendTextMessageAsync(user, text, replyMarkup: inline);
                    return;
                }
                text = "感谢支持，审核结果将在稍后通知";
                await Bot.SendTextMessageAsync(user, text);
                text = post.ToString();
                //投稿完成，处理信息

                //转发消息到审核群
                await SendForAduit(user);
                return;
            };
            Bot.SendTextMessageAsync(user, text, replyMarkup: inline);
        }

        private async Task SendForAduit(long user)
        {
            Message? sent = null;
            var post = cache.Data[user];
            IReplyMarkup inline =  new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅通过",$"aduit/{post.Timestamp}/pass"),
                    
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("❌拒绝",$"aduit/{post.Timestamp}/reject"),
                    InlineKeyboardButton.WithCallbackData("🔕静默拒绝",$"aduit/{post.Timestamp}/silentreject"),
                }
            });
            if (post.Link is not null)
            {
                
                sent = await Bot.SendTextMessageAsync(config.data.Telegram.GroupID, post.ToString(), replyMarkup: inline);
            }
            else if (post.FileID is not null)
            {
                sent = await Bot.SendAudioAsync(
                    chatId: config.data.Telegram.GroupID,
                     InputFile.FromFileId(post.FileID),
                    replyMarkup: inline,
                    caption: post.ToString()
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
        }

        //新消息的处理

        //处理TGAPI错误
        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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