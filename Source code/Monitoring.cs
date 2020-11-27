using System;
using System.Net;
using System.Data;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

namespace Telegram_Bot
{
    class Monitoring
    {
        private static bool searchModification = false;

        private static long chatId = 0;

        private static Dictionary<string, short> products = null;

        private static List<bool> isLoaded = null;

        private static List<List<string>> oldListLinks = null;

        private static List<HtmlNode> listProducts = null;

        public Monitoring(long chatId, Dictionary<string, short> products, bool searchModification = false)
        {
            Monitoring.chatId = chatId;

            Monitoring.searchModification = searchModification;

            Monitoring.products = products;

            isLoaded = new List<bool>();
            for (int i = 0; i < products.Count; i++)
                isLoaded.Add(false);

            oldListLinks = new List<List<string>>();
            for (int i = 0; i < products.Count; i++)
                oldListLinks.Add(new List<string>());

            listProducts = new List<HtmlNode>();
        }

        public async void MonitorListings()
        {
            while (Program.users[chatId].IsSearch)
            {
                if (!Program.users[chatId].IsSearch) break;
                for (int countProducts = 0; countProducts < products.Count; countProducts++)
                {
                    if (!Program.users[chatId].IsSearch) break;

                    if ((listProducts = GetListProducts(products.Keys.ToList()[countProducts])) == null) continue;

                    if (listProducts.Count == 0) continue;

                    if (isLoaded[countProducts] == false)
                    {
                        for (int i = 0; i < listProducts.Count; i++)
                            oldListLinks[countProducts].Add(listProducts[i].Descendants("a").FirstOrDefault().GetAttributeValue("href", ""));
                        isLoaded[countProducts] = true;
                        Console.WriteLine(products.Keys.ToList()[countProducts] + " isLoaded");
                    }
                    else
                    {
                        List<string> newListProducts = GetNewListProducts(countProducts);

                        if (newListProducts.Count != 0)
                        {
                            try
                            {
                                string newListings = "";
                                int countListings = 0;

                                for (int i = 0; i < newListProducts.Count; i++)
                                {
                                    for (int j = 0; j < listProducts.Count; j++)
                                    {
                                        if (listProducts[j].Descendants("a").FirstOrDefault().GetAttributeValue("href", "") == newListProducts[i])
                                        {
                                            if (float.Parse(Regex.Match(listProducts[i].Descendants("span")
                                                .Where(node => node.GetAttributeValue("class", "")
                                                .Equals("s-item__price")).FirstOrDefault().InnerText.Trim('\r', '\n', '\t'), @"\d+.\d+")
                                                .Value.Replace(',', '.'), CultureInfo.InvariantCulture) <=
                                                products[products.Keys.ToList()[countProducts]])
                                            {
                                                countListings++;
                                                newListings += newListProducts[i] + "\n";
                                            }
                                            break;
                                        }
                                    }
                                }
                                newListings = newListings.Insert(0, countListings.ToString() + " New listings\n");
                                await Program.botClient.SendTextMessageAsync(chatId, newListings);
                                Thread.Sleep(1000);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
            }
        }

        private static List<HtmlNode> GetListProducts(string nameProduct)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            WebClient html = new WebClient();

            try
            {
                if (!searchModification)
                    htmlDocument.LoadHtml(html.DownloadString("https://www.ebay.com/sch/i.html?_from=R40&_nkw=" +
                        nameProduct.Replace(" ", "+") + "&_sacat=0&_sop=10"));
                else
                    htmlDocument.LoadHtml(html.DownloadString("https://www.ebay.com/sch/i.html?_from=R40&_nkw=" +
                        nameProduct.Replace(" ", "+") + "&_sacat=0&LH_ItemCondition=1000&_sop=10"));

                return htmlDocument.DocumentNode.Descendants("ul").Where(node => node.GetAttributeValue("class", "")
                            .Equals("srp-results srp-list clearfix")).ToList()[0].Descendants("li").Where(node => node.GetAttributeValue("class", "")
                            .Contains("s-item")).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        private static List<string> GetNewListProducts(int countProducts)
        {
            List<string> newListProducts = new List<string>();

            for (int i = 0; i < listProducts.Count; i++)
            {
                if (!oldListLinks[countProducts].Contains(listProducts[i].Descendants("a").FirstOrDefault().GetAttributeValue("href", "")))
                {
                    oldListLinks[countProducts].Insert(0, listProducts[i].Descendants("a").FirstOrDefault().GetAttributeValue("href", ""));
                    oldListLinks[countProducts].RemoveAt(oldListLinks[countProducts].Count - 1);
                    newListProducts.Add(listProducts[i].Descendants("a").FirstOrDefault().GetAttributeValue("href", ""));
                }
                else break;
            }
            return newListProducts;
        }
    }
}
