using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

using System.Threading.Tasks;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            HtmlDocument document = null;
            HtmlWeb web = new HtmlWeb();
            String htmlHref = ""; //храним ссылку назагружаемую страницу
            HtmlNode nodeProduct;
            HtmlNodeCollection nodes;

            // ---- 1 часть выводим информацию по запросу пользователя
            Console.WriteLine("Какой товар вас интересует? ");
            String parameter = Console.ReadLine();
            Console.WriteLine("Пожалуйста подождите ...");

            htmlHref = "https://www.ulmart.ru/search?string=" + parameter;

            try
            {
                document = web.Load(htmlHref);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка! Неправильный запрос. ");

            }
            //вывод инфы товаров по запросу 
            if (document != null)
            {
                nodes = document.DocumentNode.SelectNodes(".//div[@class='b-product__title']//a");

                int i = 0;
                if (nodes != null)
                    foreach (HtmlNode node in nodes)
                    {
                        i++;
                        Console.WriteLine(i + ") " + node.InnerText);
                    }
                else
                {
                    Console.WriteLine("По вашему запросу ничего не найдено! ");
                }
                //---- Часть 2. Выводим полную инфу о выборанном товаре
                if (nodes != null)
                {
                    Console.WriteLine("Выберите товар из списка (напишите номер)");
                    int number = Convert.ToInt32(Console.ReadLine());
                    parameter = nodes[number - 1].Attributes["href"].Value; //вытащили строку вида "goods/237794" где циферки уникальны для каждого товара
                    Console.WriteLine("Пожалуйста подождите ...\n");
                    htmlHref = "https://www.ulmart.ru/" + parameter;
                    try
                    {
                        document = web.Load(htmlHref);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка! Невозможно запросить информацию о выбранном товаре. ");
                    }
                    if (document != null)
                    {
                        Console.WriteLine(" -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
                        Console.WriteLine(" Информация о товаре " + parameter + ": ");
                        Console.WriteLine(" -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");

                        nodeProduct = document.DocumentNode.SelectSingleNode(".//h1[@class='main-h1 main-h1_bold js-reload']");
                        Console.WriteLine(" Название товара - " + nodeProduct.InnerText.Replace("\n", "").Replace("  ", ""));

                        nodeProduct = document.DocumentNode.SelectSingleNode(".//span[@class='b-price__num js-price']");
                        String s = nodeProduct.InnerText;//Цена записана очень дико с большим количеством лишних символов
                        s = s.Replace("&nbsp;", " ").Replace("\n", "").Replace("  ", "");
                        Console.WriteLine(" Цена товара - " + s + " руб. ");

                        //Далее вывод характеристик товара. Они представлены как значения таблицы. 
                        // название - значение
                        //Но для отдельных товаров иногда не написана. У них только название и цена.
                        HtmlNodeCollection nodesLabel = document.DocumentNode.SelectNodes(".//span[@class='b-dotted-line__title']");
                        HtmlNodeCollection nodesValue = document.DocumentNode.SelectNodes(".//div[@class='b-dotted-line__right']//div");
                        i = 0;
                        if (nodesLabel != null && nodesValue != null)
                        {
                            Console.WriteLine(" Дополнительная информация: ");
                            foreach (HtmlNode node in nodesLabel)
                            {
                                Console.WriteLine("  - " + node.InnerText + ": " + nodesValue[i].InnerText.Replace("\n", "").Replace("  ", ""));
                                i++;
                            }
                        }
                        else
                        {
                            Console.WriteLine(" Полная информация о товаре отсутсвует!");
                        }
                    }
                }
            }
            Console.WriteLine("\n Для выхода нажмите любую клавишу... ");
            Console.ReadKey();
        }
    }
}
