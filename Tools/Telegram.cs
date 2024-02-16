using Telegram.Bot.Types;

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
}