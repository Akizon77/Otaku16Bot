using Otaku16;
using Otaku16.Repos;
using Otaku16.Service;
using SqlSugar;
using System.Diagnostics;
using Telegram.Bot;

Logger log = new("Main");
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    //Debugger.Break();
    log.Panic(e.ExceptionObject);
};
log.Info("正在启动服务");
Hosting.Start();

log.Info("Done! 输入 stop 或发送 Ctrl+C 终止程序");

var pr = Hosting.GetService<PostRepo>();
var ur = Hosting.GetService<UserRepo>();
pr.InitHeader();
ur.InitHeader();
await ur.CopyNew().InsertOrUpdateAsync(new Otaku16.Model.User()
{
    Id = Hosting.GetService<Options>().Owner,
    Permission = Otaku16.Model.Permissions.Owner
});
var posts = pr.Queryable().ToList();
log.Info("加载了 ",posts.Count," 条投稿");
Hosting.GetService<Bot>();

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