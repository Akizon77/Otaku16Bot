using Otaku16;
using Otaku16.Service;
using System.Diagnostics;

Logger log = new("Main");
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    Debugger.Break();
    log.Panic(e.ExceptionObject);
};
log.Info("正在启动服务");
Hosting.Start();

log.Info("Done! 输入 stop 或发送 Ctrl+C 终止程序");

var bot = Hosting.GetService<Bot>().bot;



#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
Task.Run(() =>
{
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
    Hosting.Stop();
});
#pragma warning restore CS4014 
Hosting.WaitForStop();
log.Info("Stopping");