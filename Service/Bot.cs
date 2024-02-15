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
        private readonly Config.Data Config;

        public Bot(Config config)
        {
            Config = config.data;
            log.Info($"正在登录Bot");
            log.Debug($"Bot登录私钥: {Config.Telegram.Token}");
            if (Config.Proxy)
            {
                log.Info($"使用代理 {Config.Socks5}");
                WebProxy proxy = new(Config.Socks5);
                HttpClient httpClient = new(
                    new SocketsHttpHandler { Proxy = proxy, UseProxy = true }
                );
                bot = new TelegramBotClient(Config.Telegram.Token, httpClient);
            }
            else
            {
                bot = new TelegramBotClient(Config.Telegram.Token);
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
                }
            }, new BotCommandScopeChat() { ChatId = Config.Telegram.GroupID }).GetAwaiter().GetResult();
            log.Info("已更新自身指令列表");
            }
    }
}