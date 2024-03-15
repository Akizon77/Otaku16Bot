using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Otaku16.Repos;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otaku16.Service
{
    public class Hosting
    {
        private static readonly IHost _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(c =>
            {

            }).ConfigureServices((c, s) =>
            {
                s.AddHostedService<ApplicationHostService>();
                s.AddSingleton<Bot>();
                s.AddSingleton<Cache>();
                s.AddSingleton<History>();
                s.AddSingleton<Handler>();
                s.AddSingleton<PostRepo>();
                s.AddSingleton<UserRepo>();
                //注册Options
                s.AddSingleton<Options>(services =>
                {
                    return Options.Load("./options.json");
                });
                //注册数据库
                s.AddSingleton<ISqlSugarClient>(ser =>
                {
                    Options options = ser.GetService<Options>()!;
                    var builder = new MySqlConnectionStringBuilder()
                    {
                        Server = options.Database.Host,
                        Port = options.Database.Port,
                        Database = options.Database.Table,
                        UserID = options.Database.Username,
                        Password = options.Database.Password,
                        CharacterSet = "utf8mb4"
                    };
                    var cf = new ConnectionConfig()
                    {
                        ConnectionString = builder.ToString(),
                        DbType = DbType.MySql,
                        IsAutoCloseConnection = true,
                    };
                    return new SqlSugarClient(cf, db =>
                    {
                        var logger = new Logger("SQL");
                        db.Aop.OnLogExecuting = (sql, pars) =>
                        {
                            //var param = db.GetConnectionScope(0).Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value));
                            foreach (var par in pars)
                            {
                                sql = sql.Replace(par.ParameterName, par?.Value?.ToString() ?? "NULL");
                            }
                            if (db.Ado.SqlExecutionTime.TotalMilliseconds >= 0)
                                logger.Info($"执行时间: {db.Ado.SqlExecutionTime.TotalMilliseconds} ms | {sql}");
                        };
                        db.Aop.OnError = (e) => Console.WriteLine("执行SQL出错：" + e.ToString());
                    });
                });
            }).Build();
        /// <summary>
        /// 获取指定服务的实例。
        /// </summary>
        /// <typeparam name="T">服务的类型，必须是类类型。</typeparam>
        /// <returns>返回T类型的实例。如果未找到该服务，则抛出ArgumentNullException。</returns>
        public static T GetService<T>()
            where T : class
        {
            // 尝试从_host.Services获取T类型的服务实例
            return _host.Services.GetService(typeof(T)) as T ?? throw new ArgumentNullException($"{typeof(T)} 未注册");
        }
        public static void Start()
        {
            _host.Start();
        }
        public static void Stop()
        {
            _host.StopAsync();
        }
        public static void WaitForStop()
        {
            _host.WaitForShutdown();
        }
    }
}
