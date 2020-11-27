using System;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram_Bot
{
    public static class Commands
    {
        public static async void GetMenuPossibleCommands(long chatId)
        {
            var contacts = new[]
                {
                    InlineKeyboardButton.WithUrl("VK", AppConfig.VKAddress),
                    InlineKeyboardButton.WithUrl("Telegram", AppConfig.TelegramAddress),
                };

            try
            {
                InlineKeyboardMarkup inlineKeyboard = null;
                if (Program.users[chatId].IsSearch)
                {
                    inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        contacts,
                        new[] { InlineKeyboardButton.WithCallbackData("Pause") }
                    });
                }
                else
                {
                    inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        contacts,
                        new[] { InlineKeyboardButton.WithCallbackData("Start") },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Add item"),
                            InlineKeyboardButton.WithCallbackData("View items")
                        }
                    });
                }
                await Program.botClient.SendTextMessageAsync(chatId, "И что будешь делать?", replyMarkup: inlineKeyboard);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static async void ModificationSelection(long chatId)
        {
            try
            {
                Console.WriteLine("Search modification selection...");

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("All"),
                        InlineKeyboardButton.WithCallbackData("New")
                    }
                });

                await Program.botClient.SendTextMessageAsync(chatId, "Чё ищем?", replyMarkup: inlineKeyboard);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void StartSearch(long chatId, Dictionary<string, short> items, bool searchModification)
        {
            try
            {
                Monitoring proc = new Monitoring(chatId, items, searchModification);

                new Thread(() =>
                {
                    proc.MonitorListings();
                })
                {
                    IsBackground = false,
                    Priority = ThreadPriority.Normal
                }.Start();

                Program.users[chatId].IsSearch = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static async void PauseSearch(string queryId, long chatId)
        {
            try
            {
                if (!Program.users[chatId].IsSearch)
                {
                    await Program.botClient.AnswerCallbackQueryAsync(queryId, "Мы как бэ и не ищем!...");

                    return;
                }

                await Program.botClient.AnswerCallbackQueryAsync(queryId, "Прекратить поиск!");

                Program.users[chatId].IsSearch = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static async void GetListItems(long chatId)
        {
            try
            {
                List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();

                foreach (var product in Program.users[chatId].GetListItems())
                {
                    buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(product.Key + " - price " + product.Value.ToString() + "$") });
                }

                InlineKeyboardMarkup inlineItem = new InlineKeyboardMarkup(buttons.ToArray());

                await Program.botClient.SendTextMessageAsync(chatId, buttons.Count == 0 ? "ПУСТООТАА..." : "Списочек", replyMarkup: inlineItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static async void OnItemClick(long chatId, string product)
        {
            try
            {
                Console.WriteLine("Selecting an action on the pressed element...");

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Edit item"),
                        InlineKeyboardButton.WithCallbackData("Delete item")
                    }
                });

                await Program.botClient.SendTextMessageAsync(chatId, "Ну и чё делаем с этим: \"" + product + "\"?", replyMarkup: inlineKeyboard);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static async void SelectChangedParameter(long chatId)
        {
            try
            {
                Console.WriteLine("A property change operation has been selected...");

                var inlineKeyboard2 = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Name"),
                        InlineKeyboardButton.WithCallbackData("Price")
                    }
                });

                await Program.botClient.SendTextMessageAsync(chatId, "Чё меняем?", replyMarkup: inlineKeyboard2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
