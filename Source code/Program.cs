using System;
using System.Collections.Generic;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Telegram_Bot
{
    class Program
    {
        public static TelegramBotClient botClient;

        public static Dictionary<long, SpecificUserDatabaseAndRequests> users;

        static void Main(string[] args)
        {
            users = new Dictionary<long, SpecificUserDatabaseAndRequests>();

            string[] chatIDs = System.IO.File.ReadAllLines("chatIDs.txt");

            for(int i = 0; i < chatIDs.Length; i++)
                users.Add(long.Parse(chatIDs[i]), new SpecificUserDatabaseAndRequests(long.Parse(chatIDs[i])));

            botClient = Bot.Get();
            botClient.OnMessage += BotOnMessageReceived;
            botClient.OnCallbackQuery += BotOnCallbackQueryReceived;
            botClient.StartReceiving();

            Console.WriteLine($"Bot id: {botClient.GetMeAsync().Result.Id}. Bot name: {botClient.GetMeAsync().Result.FirstName}");

            Console.ReadKey();
        }

        private static async void BotOnCallbackQueryReceived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            string buttonText = e.CallbackQuery.Data;

            try
            {
                switch (buttonText)
                {
                    case "Start":
                        if(users[e.CallbackQuery.Message.Chat.Id].IsSearch) 
                            await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, $"Да мы ищем уже!");
                        else
                        {
                            Commands.ModificationSelection(e.CallbackQuery.Message.Chat.Id);

                            await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Ну... выбирайте!");
                            await botClient.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId);
                        }
                        users[e.CallbackQuery.Message.Chat.Id].ResetRequest();
                        break;

                    case "All":
                        Commands.StartSearch(e.CallbackQuery.Message.Chat.Id, users[e.CallbackQuery.Message.Chat.Id].GetListItems(), false);

                        await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Ищем всё...");
                        await botClient.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId);

                        Commands.GetMenuPossibleCommands(e.CallbackQuery.Message.Chat.Id);

                        users[e.CallbackQuery.Message.Chat.Id].ResetRequest();
                        break;

                    case "New":
                        Commands.StartSearch(e.CallbackQuery.Message.Chat.Id, users[e.CallbackQuery.Message.Chat.Id].GetListItems(), true);

                        await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Ищем только новое...");
                        await botClient.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId);

                        Commands.GetMenuPossibleCommands(e.CallbackQuery.Message.Chat.Id);
                       
                        users[e.CallbackQuery.Message.Chat.Id].ResetRequest();
                        break;

                    case "Pause":
                        Commands.PauseSearch(e.CallbackQuery.Id, e.CallbackQuery.Message.Chat.Id);

                        await botClient.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId);

                        Commands.GetMenuPossibleCommands(e.CallbackQuery.Message.Chat.Id);

                        users[e.CallbackQuery.Message.Chat.Id].ResetRequest();
                        break;

                    case "View items":
                        Commands.GetListItems(e.CallbackQuery.Message.Chat.Id);

                        await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Лови список");

                        users[e.CallbackQuery.Message.Chat.Id].ResetRequest();
                        break;

                    case "Add item":
                        users[e.CallbackQuery.Message.Chat.Id].FlagRequestAddItemName = true;
                        users[e.CallbackQuery.Message.Chat.Id].FlagRequestAddItemPrice = true;

                        await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Нас и так много");
                        await botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Ну и что за имячко?");
                        await botClient.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId);

                        break;

                    case "Delete item":
                        if (users[e.CallbackQuery.Message.Chat.Id].DeleteItem(users[e.CallbackQuery.Message.Chat.Id].NameElementClick) != 1)
                        {
                            await botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Хммм... не получилось");
                            Console.WriteLine("Something went wrong!");
                            break;
                        }

                        await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Ну как бы, продукт удален...");
                        await botClient.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId);

                        users[e.CallbackQuery.Message.Chat.Id].ResetRequest();
                        break;

                    case "Edit item":
                        Commands.SelectChangedParameter(e.CallbackQuery.Message.Chat.Id);

                        await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Значит меняем");
                        await botClient.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId);

                        break;

                    case "Name":
                        users[e.CallbackQuery.Message.Chat.Id].FlagRequestEditItemName = true;

                        await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Название то зачем?");
                        await botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Вводи новое название, только красивое");
                        await botClient.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId);

                        break;

                    case "Price":
                        users[e.CallbackQuery.Message.Chat.Id].FlagRequestEditItemPrice = true;

                        await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Согласен, она слишком большая");
                        await botClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Ну вводи новую цену");
                        await botClient.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId);

                        break;

                    default:
                        await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Тык...");
                        if (users[e.CallbackQuery.Message.Chat.Id].GetListItems().ContainsKey(buttonText.Substring(0, buttonText.IndexOf(" - price"))))
                        {
                            users[e.CallbackQuery.Message.Chat.Id].NameElementClick = buttonText.Substring(0, buttonText.IndexOf(" - price"));
                            users[e.CallbackQuery.Message.Chat.Id].PriceElementClick = Convert.ToInt16(buttonText.Substring(buttonText.IndexOf(" - price") + 9, buttonText.Length - buttonText.IndexOf(" - price") - 10));

                            Console.WriteLine("An item was selected: " + users[e.CallbackQuery.Message.Chat.Id].NameElementClick + " price: " + users[e.CallbackQuery.Message.Chat.Id].PriceElementClick.ToString());

                            Commands.OnItemClick(e.CallbackQuery.Message.Chat.Id, buttonText);

                            await botClient.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static async void BotOnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != MessageType.Text) return;
            Console.WriteLine($"Recived text message '{message.Text}' in chat '{e.Message.Chat.Id}' with name {message.From.FirstName} {message.From.LastName}");

            switch (message.Text)
            {
                case "/menu":
                    if(!users.ContainsKey(message.Chat.Id))
                    {
                        System.IO.File.WriteAllLines("users/products" + message.Chat.Id.ToString() + ".txt", new List<string>());
                        System.IO.File.WriteAllLines("users/prices" + message.Chat.Id.ToString() + ".txt", new List<string>());

                        List<string> chatIDs = new List<string>();
                        foreach (var user in users)
                            chatIDs.Add(user.Key.ToString());
                        chatIDs.Add(message.Chat.Id.ToString());
                        System.IO.File.WriteAllLines("chatIDs.txt", chatIDs);

                        users.Add(message.Chat.Id, new SpecificUserDatabaseAndRequests(message.Chat.Id));
                    }

                    if(users[message.Chat.Id].FlagRequestEditItemName || users[message.Chat.Id].FlagRequestEditItemPrice ||
                        users[message.Chat.Id].FlagRequestAddItemName || users[message.Chat.Id].FlagRequestAddItemPrice)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Это ж команда, ... давай по новой");
                        break;
                    }

                    Commands.GetMenuPossibleCommands(message.Chat.Id);

                    users[message.Chat.Id].ResetRequest();
                    break;

                default:
                    if (users[message.Chat.Id].FlagRequestEditItemName && users[message.Chat.Id].NameElementClick != null && users[message.Chat.Id].PriceElementClick != 0)
                    {
                        if (users[message.Chat.Id].ChangedItemName(message.Text, users[message.Chat.Id].NameElementClick) == 3)
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Хммм... не получилось");
                        else await botClient.SendTextMessageAsync(message.Chat.Id, "Ееее... поменяли");
                        users[message.Chat.Id].ResetRequest();
                    }

                    if (users[message.Chat.Id].FlagRequestEditItemPrice && users[message.Chat.Id].NameElementClick != null && users[message.Chat.Id].PriceElementClick != 0)
                    {
                        if (users[message.Chat.Id].ChangedItemPrice(Convert.ToInt16(message.Text), users[message.Chat.Id].NameElementClick) == 3)
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Хммм... не получилось");
                        else await botClient.SendTextMessageAsync(message.Chat.Id, "Ееее... поменяли");
                        users[message.Chat.Id].ResetRequest();
                    }

                    if (users[message.Chat.Id].FlagRequestAddItemName || users[message.Chat.Id].FlagRequestAddItemPrice)
                    {
                        if (users[message.Chat.Id].FlagRequestAddItemName && users[message.Chat.Id].FlagRequestAddItemPrice)
                        {
                            users[message.Chat.Id].InputItemName = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat.Id, "И сколько это стоит?");
                            users[message.Chat.Id].FlagRequestAddItemName = false;
                            return;
                        }
                        else
                        {
                            users[message.Chat.Id].InputItemPrice = Convert.ToInt16(message.Text);
                            if(users[message.Chat.Id].AddItem(users[message.Chat.Id].InputItemName, users[message.Chat.Id].InputItemPrice) != 1)
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Хммм... не получилось");
                            else await botClient.SendTextMessageAsync(message.Chat.Id, "У нас пополнение");
                            users[message.Chat.Id].ResetRequest();
                            return;
                        }
                    }
                    break;
            }
        }
    }
}