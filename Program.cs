using Otaku16;
using Otaku16.Service;
using Otaku16.Tools;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Otaku16.Data;
using Telegram.Bot.Types.ReplyMarkups;


Logger log = new("Main");
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    Debugger.Break();
    log.Panic(e.ExceptionObject);
};
log.Info("正在启动服务");
Hosting.Start();
Hosting.GetService<Handler>();

//记录发送的消息

log.Info("Done! 输入stop终止程序");

while (true)
{
    var input = Console.ReadLine();
    if (input == "stop")
    {
        break;
    }
    else
    {
        log.Info("未知指令: ", input);
    }
}
log.Info("Stopping");




