using System.Diagnostics;
using Telegram.Bot.Types;

namespace Otaku16.Tools
{
    public class Logger
    {
        private string Model { get; set; } = "Main";

        /// <summary>
        /// 实例化新的 Logger
        /// </summary>
        /// <param name="model">日志模块</param>
        public Logger(string model)
        {
            Model = model;
        }

        private void Log(string level, params object?[] v)
        {
            //先获取现在的时间 要记录日志发生的时间方便排查错误
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //先打印出时间
            Console.Write($"[{time}]");
            //再打印出是谁触发的日志 和日志的等级
            //同时进行颜色处理，为了方便，也为了快速定位
            //正常的日志就是默认颜色
            //警告的就是黄色
            //发生错误的是红色
            //调试的是蓝色、
            //这几种颜色符合直觉、用其他颜色也当然没问题
            switch (level)
            {
                case "DEBUG":
                    Console.ForegroundColor = ConsoleColor.Blue;
                    PrintLevel();
                    break;

                case "PANIC":
                case "ERROR":
                    Console.ForegroundColor = ConsoleColor.Red;
                    PrintLevel();
                    break;

                case "WARN":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    PrintLevel();
                    break;

                default:
                    PrintLevel();
                    break;
            }
            //切换回默认
            Console.ResetColor();
            //抽象成函数
            void PrintLevel() => Console.Write($"[{Model}/{level}]");
            //有可能直接 Info(null)，这可能导致 NullReference 异常
            if (v is null) Console.WriteLine("Null");
            else
                foreach (var obj in v)
                {
                    Console.Write(obj ?? "Null");
                }
            Console.WriteLine();

            //PANIC 需要终止程序
            if (level == "PANIC")
            {
                StackTrace stack = new StackTrace();
                Console.WriteLine(stack);
                Environment.Exit(-1);
            }
                
        }

        /// <summary>
        /// 打印 Info 级别的日志
        /// </summary>
        /// <param name="v">日志内容</param>
        public void Info(params object?[] v)
        {
            Log("INFO", v);
        }

        /// <summary>
        /// 打印 Warn 级别的日志
        /// </summary>
        /// <param name="v">日志内容</param>
        public void Warn(params object?[] v)
        {
            Log("WARN", v);
        }

        /// <summary>
        /// 打印 Error 级别的日志
        /// </summary>
        /// <param name="v">日志内容</param>
        public void Error(params object?[] v)
        {
            Log("ERROR", v);
        }

        /// <summary>
        /// 打印 Debug 级别的日志
        /// </summary>
        /// <param name="v">日志内容</param>
        public void Debug(params object?[] v)
        {
            if (!Config.GetConfig().Debug) return;
            Log("DEBUG", v);
        }

        /// <summary>
        /// 打印 Panic 级别的日志,并终止程序
        /// </summary>
        /// <param name="v">日志内容</param>
        public void Panic(params object?[] v)
        {
            Log("PANIC", v);
        }
        public void Message(Update update)
        {
            if (update.CallbackQuery is { } query)
            {
                Debug($"收到 {query.From.Username??query.From.Id.ToString()} 的回调: {query.Data}");
            }
            else if (update.Message is { } message)
            {
                if (message.From is null)
                {
                    Warn("Sender is null");
                    return;
                }
                var groupperfix = message.Chat.Title is null ? "" : $"[{message.Chat.Title}]";
                var perfix = $"{groupperfix}{message.From.FirstName} {message.From.LastName}(@{message.From.Username}): ";
                //纯文本信息
                if (message.Text is { } text)
                    Info($"{perfix}{text}");
                if (message.Audio is { } audio)
                    Info($"{perfix}[音频]{audio.Performer ?? "未知作曲家"} - {audio.Title ?? "未知曲名"} / {audio.FileSize / 1024.0 / 1024.0:.00}MB");
            }
            else if (update.ChannelPost is { } post)
            {
                Info("有新的频道消息");
            }
            else
            {
                Warn("未知消息");
            }
           
        }
    }
}