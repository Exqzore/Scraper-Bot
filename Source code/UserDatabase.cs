using System;
using System.Collections.Generic;

namespace Telegram_Bot
{
    class UserDatabase
    {
        protected long _chatId;

        private Dictionary<string, short> Items = null;

        public UserDatabase(long chatId)
        {
            _chatId = chatId;

            bool isContain = false;

            string[] сhatIDs = System.IO.File.ReadAllLines("chatIDs.txt");

            for (int i = 0; i < сhatIDs.Length; i++)
            {
                if (сhatIDs[i] == _chatId.ToString())
                {
                    isContain = true;
                }
            }

            Items = new Dictionary<string, short>();

            if (isContain)
            {
                string[] productsPrice = System.IO.File.ReadAllLines("users/prices" + _chatId.ToString() + ".txt");
                string[] productsName = System.IO.File.ReadAllLines("users/products" + _chatId.ToString() + ".txt");

                for (int i = 0; i < productsName.Length; i++)
                    Items.Add(productsName[i], productsPrice[i] == "-" ? short.MaxValue : Convert.ToInt16(productsPrice[i]));
            }
        }

        public int AddItem(string itemName, short itemPrice)
        {
            if (!Items.ContainsKey(itemName))
            {
                Items.Add(itemName, itemPrice);
                Console.WriteLine("Product added");

                UpdateData();

                return 1;
            }
            else
            {
                Console.WriteLine("This product already exists");
                return 2;
            }
        }

        public int DeleteItem(string itemName)
        {
            if (Items.Remove(itemName))
            {
                Console.WriteLine("Product removed from the list");

                UpdateData();

                return 1;
            }
            else
            {
                Console.WriteLine("Something went wrong!");
                return 2;
            }
        }

        public int ChangedItemName(string newItemName, string oldItemName)
        {
            try
            {
                short priceEditItem = Items[oldItemName];
                Items.Remove(oldItemName);
                Items.Add(newItemName, priceEditItem);

                UpdateData();

                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 3;
            }
        }

        public int ChangedItemPrice(short newItemPrice, string itemName)
        {
            try
            {
                Items[itemName] = newItemPrice;

                UpdateData();

                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 3;
            }
        }

        public Dictionary<string, short> GetListItems()
        {
            return Items;
        }

        private void UpdateData()
        {
            List<string> tempItemPrices = new List<string>();
            List<string> tempItemNames = new List<string>();

            foreach (var item in Items)
            {
                tempItemNames.Add(item.Key);
                tempItemPrices.Add(item.Value.ToString());
            }

            System.IO.File.WriteAllLines("users/products" + _chatId.ToString() + ".txt", tempItemNames);
            System.IO.File.WriteAllLines("users/prices" + _chatId.ToString() + ".txt", tempItemPrices);
        }
    }
}
