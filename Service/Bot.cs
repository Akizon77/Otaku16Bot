using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Otaku16.Service
{
    public class Bot
    {
        private readonly Logger log = new Logger("Bot");
        public readonly TelegramBotClient bot;
        public readonly User Me;
        private readonly Options Opt;

        public Bot(Options config)
        {
            Opt = config;
            log.Info($"正在登录Bot");
            log.Debug($"Bot登录私钥: {Opt.Telegram.Token}");
            if (Opt.Proxy)
            {
                log.Info($"使用代理 {Opt.Socks5}");
                WebProxy proxy = new(Opt.Socks5);
                HttpClient httpClient = new(
                    new SocketsHttpHandler { Proxy = proxy, UseProxy = true }
                );
                bot = new TelegramBotClient(Opt.Telegram.Token, httpClient);
            }
            else
            {
                bot = new TelegramBotClient(Opt.Telegram.Token);
            }
            Me = bot.GetMeAsync().Result;
            log.Info($"已成功作为 @{Me.Username} 登录");
            bot.SetMyCommandsAsync(new[]
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
            }).GetAwaiter().GetResult();
            bot.SetMyCommandsAsync(new[]
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
                },
                new BotCommand()
                {
                    Command = "echo",
                    Description = "传话",
                },
                new BotCommand()
                {
                    Command = "add",
                    Description = "添加管理员",
                },
                new BotCommand()
                {
                    Command = "del",
                    Description = "移除管理员",
                },
                new BotCommand()
                {
                    Command = "admins",
                    Description = "列出所有管理员",
                }
            }, new BotCommandScopeChat() { ChatId = Opt.Telegram.GroupID }).GetAwaiter().GetResult();
            log.Info("已更新自身指令列表");
        }
    }
}