using Otaku16.Service;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otaku16.Tools
{
    public class Telegram
    {
        /// <summary>
        /// 获取用户的显示名称。
        /// </summary>
        /// <param name="user">包含用户信息的对象。</param>
        /// <returns>根据可用信息返回用户的名称表示。优先级从高到低为：用户名、姓和名、姓、名、用户ID。</returns>
        public static string GetName(User user)
        {
            // 如果用户名不为空，返回带@的用户名
            if (user.Username != null) return $"@{user.Username}";
            // 如果同时有姓和名，返回拼接的全名
            if (user.FirstName != null && user.LastName != null) return $"{user.FirstName} {user.LastName}";
            // 如果只有姓，返回姓
            if (user.FirstName != null) return $"{user.FirstName}";
            // 如果只有名，返回名
            if (user.LastName != null) return $"{user.LastName}";
            // 如果所有字段都为空，返回用户ID
            return user.Id.ToString();
        }

        /// <summary>
        /// 根据聊天ID获取聊天名称。
        /// </summary>
        /// <param name="chatid">聊天的ID，如果为null则返回"Null"。</param>
        /// <returns>返回聊天的名称。优先使用用户名，其次是名和姓，然后是标题，最后是ID。</returns>
        public static string GetName(long? chatid, bool HTML = true)
        {
            var name = $"{chatid}";
            // 如果chatid为null，直接返回"Null"
            if (chatid is null) return "Null";
            Chat c;
            try
            {
                // 尝试从服务中获取聊天信息
                c = Hosting.GetService<Bot>().bot.GetChatAsync(chatid).Result;
            }
            catch
            {
                // 如果获取失败，返回chatid的字符串形式
                return name;
            }
            if (HTML)
            {
                // 尝试使用名和姓作为聊天名称
                if (c.FirstName != null && c.LastName != null) return $"<a href=\"tg://user?id={chatid}\" >{c.FirstName.HTMLEscape()} {c.LastName.HTMLEscape()}</a>";
                // 如果只有名或只有姓，分别使用它们作为聊天名称
                if (c.FirstName != null) return $"<a href=\"tg://user?id={chatid}\" >{c.FirstName.HTMLEscape()}</a>";
                if (c.LastName != null) return $"<a href=\"tg://user?id={chatid}\" >{c.LastName.HTMLEscape()}</a>";
                // 使用用户名作为聊天名称
                if (c.Username != null) return $"<a href=\"tg://user?id={chatid}\" >{c.Username.HTMLEscape()}</a>";
                // 如果上述都不存在，使用聊天标题作为名称
                if (c.Title != null) return $"{c.Title.HTMLEscape()}";
                // 最后，如果所有信息都不存在，使用聊天ID作为名称
                return c.Id.ToString();
            }
            else
            {
                // 尝试使用名和姓作为聊天名称
                if (c.FirstName != null && c.LastName != null) return $"{c.FirstName.HTMLEscape()} {c.LastName.HTMLEscape()}";
                // 如果只有名或只有姓，分别使用它们作为聊天名称
                if (c.FirstName != null) return $"{c.FirstName.HTMLEscape()}";
                if (c.LastName != null) return $"{c.LastName.HTMLEscape()}";
                // 使用用户名作为聊天名称
                if (c.Username != null) return $"{c.Username.HTMLEscape()}";
                // 如果上述都不存在，使用聊天标题作为名称
                if (c.Title != null) return $"{c.Title.HTMLEscape()}";
                // 最后，如果所有信息都不存在，使用聊天ID作为名称
                return c.Id.ToString();
            }
            
        }
    }

    public static class Exts
    {
        /// <summary>
        /// 获取用户的显示名称。
        /// </summary>
        /// <param name="user">包含用户信息的对象。</param>
        /// <returns>根据可用信息返回用户的名称表示。优先级从高到低为：用户名、姓和名、姓、名、用户ID。</returns>
        public static string GetName(this User user,bool HTML = true)
        {
            return Tools.Telegram.GetName(user.Id,HTML);
        }

        /// <summary>
        /// 移除消息中的内联按钮。
        /// </summary>
        /// <param name="message">要移除按钮的消息对象。</param>
        /// <returns>一个任务对象，表示异步操作的完成。当操作完成时，任务将完成。</returns>
        public static async Task RemoveInlineButton(this Message message) => await Hosting.GetService<Bot>().bot.EditMessageReplyMarkupAsync(message.Chat.Id, message.MessageId, replyMarkup: null);

        /// <summary>
        /// 快速回复消息
        /// </summary>
        /// <param name="message">要快速回复的消息对象。</param>
        /// <param name="text">回复内容</param>
        /// <returns>一个任务对象，表示异步操作的完成。当操作完成时，任务将完成。</returns>
        public static async Task FastReply(this Message message, string text) => await Hosting.GetService<Bot>().bot.SendTextMessageAsync(message.Chat.Id, text, replyToMessageId: message.MessageId, parseMode: ParseMode.Html);

        /// <summary>
        /// 快速编辑消息
        /// </summary>
        /// <param name="message">要快速回复的消息对象。</param>
        /// <param name="text">编辑内容</param>
        /// <returns>一个任务对象，表示异步操作的完成。当操作完成时，任务将完成。</returns>
        public static async Task FastEdit(this Message message, string text) => await Hosting.GetService<Bot>().bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, text, parseMode: ParseMode.Html);

        /// <summary>
        /// 快速编辑消息
        /// </summary>
        /// <param name="message">要快速回复的消息对象。</param>
        /// <param name="reply">内联消息</param>
        /// <returns>一个任务对象，表示异步操作的完成。当操作完成时，任务将完成。</returns>
        public static async Task FastEdit(this Message message, InlineKeyboardMarkup reply) => await Hosting.GetService<Bot>().bot.EditMessageReplyMarkupAsync(message.Chat.Id, message.MessageId, replyMarkup: reply);

        /// <summary>
        /// 快速编辑消息
        /// </summary>
        /// <param name="message">要快速回复的消息对象。</param>
        /// <param name="text">编辑内容</param>
        /// <param name="reply">内联消息</param>
        /// <returns>一个任务对象，表示异步操作的完成。当操作完成时，任务将完成。</returns>
        public static async Task FastEdit(this Message message, string text, InlineKeyboardMarkup reply) => await Hosting.GetService<Bot>().bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, text, replyMarkup: reply, parseMode: ParseMode.Html);

        /// <summary>
        /// 快速向消息中添加文本。
        /// 如果原始消息有Caption，则在Caption后添加文本；
        /// 如果原始消息是文本消息，则在文本后添加文本。
        /// </summary>
        /// <param name="message">需要编辑的消息对象。</param>
        /// <param name="text">要添加的文本内容。</param>
        public static async Task FastAddString(this Message message, string text)
        {
            string body = "";
            // 如果消息有标题，则在标题后添加文本，并更新消息的标题
            if (message.Caption is { } caption)
            {
                body = caption + "\n" + text;
                await Hosting.GetService<Bot>().bot.EditMessageCaptionAsync(message.Chat.Id, message.MessageId, body, parseMode: ParseMode.Html);
            }
            // 如果消息是文本消息，则在文本后添加文本，并更新消息的文本内容
            else if (message.Text is { } originText)
            {
                body = originText + "\n" + text;
                await Hosting.GetService<Bot>().bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, body, parseMode: ParseMode.Html);
            }
        }
    }
}