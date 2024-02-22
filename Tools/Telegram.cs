using Otaku16.Service;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otaku16.Tools
{
    public class Telegram
    {
        public static string GetName(User user)
        {
            if (user.Username != null) return $"@{user.Username}";
            if (user.FirstName != null && user.LastName != null) return $"{user.FirstName} {user.LastName}";
            if (user.FirstName != null) return $"{user.FirstName}";
            if (user.LastName != null) return $"{user.LastName}";
            return user.Id.ToString();
        }
    }

    public static class Exts
    {
        public static string GetName(this User user)
        {
            if (user.Username != null) return $"@{user.Username}";
            if (user.FirstName != null && user.LastName != null) return $"{user.FirstName} {user.LastName}";
            if (user.FirstName != null) return $"{user.FirstName}";
            if (user.LastName != null) return $"{user.LastName}";
            return user.Id.ToString();
        }

        public async static Task RemoveInlineButton(this Message message) => await Hosting.GetService<Bot>().bot.EditMessageReplyMarkupAsync(message.Chat.Id, message.MessageId,replyMarkup: null);

        public async static Task FastReply(this Message message, string text) => await Hosting.GetService<Bot>().bot.SendTextMessageAsync(message.Chat.Id, text, replyToMessageId: message.MessageId,parseMode: ParseMode.Html);

        public async static Task FastEdit(this Message message, string text) => await Hosting.GetService<Bot>().bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, text, parseMode: ParseMode.Html);

        public async static Task FastEdit(this Message message, InlineKeyboardMarkup reply) => await Hosting.GetService<Bot>().bot.EditMessageReplyMarkupAsync(message.Chat.Id, message.MessageId, replyMarkup:reply);
        public async static Task FastEdit(this Message message, string text, InlineKeyboardMarkup reply) => await Hosting.GetService<Bot>().bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, text,replyMarkup:reply, parseMode: ParseMode.Html);
        public async static Task FastAddString(this Message message,string text)
        {
            string body = "";
            if (message.Caption is  { } caption)
            {
                body = caption + "\n" + text;
                await Hosting.GetService<Bot>().bot.EditMessageCaptionAsync(message.Chat.Id,message.MessageId, body);
            }
            else if (message.Text is { } originText)
            {
                body = originText + "\n" + text;
                await Hosting.GetService<Bot>().bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, body);
            }
            
        }
    }
}