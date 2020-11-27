using System;
using Telegram.Bot;

namespace Telegram_Bot
{
    public static class Bot
    {
        private static TelegramBotClient client;

        public static TelegramBotClient Get()
        {
            if (client != null)
            {
                return client;
            }

            client = new TelegramBotClient(AppConfig.Key)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            return client;
        }
    }
}
