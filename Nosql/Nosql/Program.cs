using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using LiteDB;

namespace Nosql
{
    class Program
    {
        static void Main(string[] args)
        {
            MyProduct p = new MyProduct();

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

                    using (var db = new LiteDatabase(@"D:\MyData.db"))
                    {
                        var col = db.GetCollection<MyProduct>("product");
                        var results = col.Find(x => x.Id == Convert.ToInt32(parameter.Replace("/goods/", "")));
                        if (results.Count() != 0)
                        {
                            Console.WriteLine(" Информация найдена в БД!\n");
                            Console.WriteLine(" -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
                            Console.WriteLine(" Информация о товаре (id =" + results.First().Id + ") ");
                            Console.WriteLine(" -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
                            Console.WriteLine(" Название товара - " + results.First().Title);
                            Console.WriteLine(" Цена товара - " + results.First().Price + " руб. ");
                            Console.WriteLine(" Производитель - " + results.First().Author);
                            Console.WriteLine(" Страна производитель - " + results.First().Country);
                        }
                        else
                        {
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
                                p.Id = Convert.ToInt32(parameter.Replace("/goods/", ""));
                                nodeProduct = document.DocumentNode.SelectSingleNode(".//h1[@class='main-h1 main-h1_bold js-reload']");
                                Console.WriteLine(" Название товара - " + nodeProduct.InnerText.Replace("\n", "").Replace("  ", ""));
                                p.Title = nodeProduct.InnerText.Replace("\n", "").Replace("  ", "");
                                nodeProduct = document.DocumentNode.SelectSingleNode(".//span[@class='b-price__num js-price']");
                                String s = nodeProduct.InnerText;//Цена записана очень дико с большим количеством лишних символов
                                s = s.Replace("&nbsp;", " ").Replace("\n", "").Replace("  ", "");
                                Console.WriteLine(" Цена товара - " + s + " руб. ");
                                p.Price = s;
                                //Далее вывод характеристик товара. Они представлены как значения таблицы. 
                                // название - значение
                                //Но для отдельных товаров иногда не написана. У них только название и цена.
                                HtmlNodeCollection nodesLabelMain = document.DocumentNode.SelectNodes(".//span[@class='b-dotted-line__title']");
                                HtmlNodeCollection nodesLabel = document.DocumentNode.SelectNodes(".//div[@class='b-dotted-line__title']");
                                HtmlNodeCollection nodesValue = document.DocumentNode.SelectNodes(".//div[@class='b-dotted-line__right']//div");
                                if (nodesLabelMain != null)
                                {
                                    i = nodesLabelMain.Count;
                                    if (nodesLabelMain != null && nodesLabel != null)
                                    {
                                        foreach (HtmlNode node in nodesLabel)
                                        {
                                            //Console.WriteLine("  - " + node.InnerText.Replace("\n", "").Replace("  ", "") + ": " + nodesValue[i].InnerText.Replace("\n", "").Replace("  ", ""));
                                            if (node.InnerText.Replace("\n", "").Replace("  ", "") == "Производитель")
                                            {
                                                Console.WriteLine("  - " + node.InnerText.Replace("\n", "").Replace("  ", "") + ": " + nodesValue[i].InnerText.Replace("\n", "").Replace("  ", ""));

                                                p.Author = nodesValue[i].InnerText.Replace("\n", "").Replace("  ", "");
                                            }
                                            if (node.InnerText.Replace("\n", "").Replace("  ", "") == "Страна производства")
                                            {
                                                Console.WriteLine("  - " + node.InnerText.Replace("\n", "").Replace("  ", "") + ": " + nodesValue[i].InnerText.Replace("\n", "").Replace("  ", ""));

                                                p.Country = nodesValue[i].InnerText.Replace("\n", "").Replace("  ", "");
                                            }
                                            i++;
                                        }
                                    }
                                    //ms sql
                                    ulmartDBEntities1 context = new ulmartDBEntities1();

                                    //производитель
                                    Author aMsSql;
                                    //проверяем есть ли уже в базе
                                    aMsSql = context.Author.Where(x => x.author1 == p.Author).FirstOrDefault();
                                    if (aMsSql == null) //нет в базе, добавляем узнаем id
                                    {
                                        aMsSql = new Author();
                                        aMsSql.author1 = p.Author;
                                        try
                                        {
                                            aMsSql.id = context.Author.OrderByDescending(u => u.id).FirstOrDefault().id + 1;
                                        }
                                        catch
                                        {
                                            aMsSql.id = 0;
                                        }
                                        context.Author.Add(aMsSql);
                                    }
                                    //товар
                                    Product pMsSql = new Product();
                                    pMsSql.title = p.Title;
                                    pMsSql.country = p.Country;
                                    pMsSql.price = p.Price;
                                    pMsSql.id = p.Id;
                                    pMsSql.author = aMsSql.id;
                                    Author aMsSql2 = context.Author.Single(d => d.id == aMsSql.id);
                                    aMsSql2.Product.Add(pMsSql);

                                    context.SaveChanges();

                                    col.Insert(p); //liteDB
                                    Console.WriteLine("\n Информация добавлена в БД!");

                                    Console.WriteLine("\n Инорфмация о всех товарах производителя:");
                                    foreach (Product pr in aMsSql2.Product)
                                    {
                                        Console.WriteLine(" - " + pr.title.Replace("  ", "") + " " + pr.price.Replace("  ", "") + " руб.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(" Полная информация о товаре отсутсвует! Товар не занесен в базу, попробуйте отправить запрос позже");
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("\n Для выхода нажмите любую клавишу... ");
            Console.ReadKey();
        }
    }
}
