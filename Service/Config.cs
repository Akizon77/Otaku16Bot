using Newtonsoft.Json.Linq;

namespace Otaku16.Service;

/// <summary>
/// 代表程序的配置选项的记录类型。
/// </summary>
public sealed record Options
{
    /// <summary>
    /// 是否开启调试模式。
    /// </summary>
    public bool Debug { get; set; }

    /// <summary>
    /// 是否使用代理。
    /// </summary>
    public bool Proxy { get; set; }

    /// <summary>
    /// SOCKS5代理服务器地址。
    /// </summary>
    public string? Socks5 { get; set; }

    /// <summary>
    /// 主人用户的ID。
    /// </summary>
    public long Owner { get; set; }

    /// <summary>
    /// Telegram相关的配置选项。
    /// </summary>
    public TelegramOptions Telegram { get; set; }

    /// <summary>
    /// 数据库相关的配置选项。
    /// </summary>
    public DatabaseOptions Database { get; set; }

    /// <summary>
    /// 从指定文件路径加载配置选项。
    /// </summary>
    /// <param name="filePath">配置文件的路径。</param>
    /// <returns>加载的配置选项实例。</returns>
    public static Options Load(string filePath)
    {
        try
        {
            // 尝试从文件读取并解析配置
            var str = File.ReadAllText(filePath);
            var jobj = JObject.Parse(str);
            return jobj.ToObject<Options>() ?? new();
        }
        catch (FileNotFoundException)
        {
            // 文件不存在时，创建并保存默认配置
            var opt = new Options()
            {
                Telegram = new(),
                Database = new()
            };
            File.WriteAllText(filePath, JObject.FromObject(opt).ToString());
            return opt;
        }
        catch (Exception)
        {
            // 其他异常直接抛出
            throw;
        }
    }
}

/// <summary>
/// Telegram相关配置选项的记录类型。
/// </summary>
public sealed record TelegramOptions
{
    /// <summary>
    /// Telegram API Token。
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Telegram 频道ID。
    /// </summary>
    public long ChannelID { get; set; }

    /// <summary>
    /// Telegram 群组ID。
    /// </summary>
    public long GroupID { get; set; }

    /// <summary>
    /// Telegram 频道的链接。
    /// </summary>
    public string ChannelLink { get; set; }

    /// <summary>
    /// Telegram 群组的链接。
    /// </summary>
    public string GroupLink { get; set; }
}

/// <summary>
/// 数据库相关配置选项的记录类型。
/// </summary>
public sealed record DatabaseOptions
{
    /// <summary>
    /// 数据库主机地址。
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 数据库端口号。
    /// </summary>
    public uint Port { get; set; }

    /// <summary>
    /// 数据库表名。
    /// </summary>
    public string Table { get; set; }

    /// <summary>
    /// 数据库用户名。
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// 数据库密码。
    /// </summary>
    public string Password { get; set; }
}
public static class OptionsExt
{
    /// <summary>
    /// 保存当前Options对象到options.json文件中。
    /// </summary>
    /// <param name="options">需要保存的Options对象。</param>
    public static void Save(this Options options)
    {
        try
        {
            // 将Options对象转换为JSON对象
            var jo = JObject.FromObject(options);
            // 将JSON对象转换为字符串
            var content = jo.ToString();
            // 写入字符串到文件
            File.WriteAllText("./options.json", content);
        }
        catch (Exception ex)
        {
            // 捕获异常并输出错误信息
            Console.WriteLine($"无法保存配置文件 {ex}");
        }
    }
}