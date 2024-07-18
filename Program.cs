using Otaku16;
using Otaku16.Model;
using Otaku16.Repos;
using Otaku16.Service;
using SqlSugar;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
Logger log = new("Main");
log.Info(Const.version);
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

await ur.CopyNew().InsertOrUpdateAsync(new Otaku16.Model.User()
{
    Id = Hosting.GetService<Options>().Owner,
    Permission = Otaku16.Model.Permissions.Owner
});
var posts = pr.Queryable().ToList();
log.Info("加载了 ",posts.Count," 条投稿");
var bot = Hosting.GetService<Bot>().bot;
var Opt = Hosting.GetService<Options>();
var Handler = Hosting.GetService<Handler>();

#region 调试
/**
var text = $"<a href=\"tg://user?id={Opt.Owner}\" >{Otaku16.Tools.Telegram.GetName(Opt.Owner)}</a>";
bot.SendTextMessageAsync(Opt.Telegram.GroupID, text,parseMode:Telegram.Bot.Types.Enums.ParseMode.Html);
**/
#endregion



/**导入所有投稿
//posts.ForEach(post =>
//{
//    if (post.Passed == null)
//    {
//        Message? sent = null;
//        IReplyMarkup inline = new InlineKeyboardMarkup(new[]
//        {
//                new[]
//                {
//                    InlineKeyboardButton.WithCallbackData("✅通过",$"aduit/{post.Id}/pass"),
//                },
//                new[]
//                {
//                    InlineKeyboardButton.WithCallbackData("❌拒绝",$"aduit/{post.Id}/reject"),
//                    InlineKeyboardButton.WithCallbackData("🔕静默拒绝",$"aduit/{post.Id}/silentreject"),
//                }
//            });
//            sent =  bot.SendTextMessageAsync(Opt.Telegram.GroupID, post.ToString(), replyMarkup: inline).Result;
//        post.GroupMessageID = sent!.MessageId;
//        pr.Update(post);

//        Thread.Sleep(2000);
//    }
//});

//Console.ReadLine();
**/

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