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
        static String PrintListInfoAboutKeywordAndReturnChoosenProduct(String htmlHref)
        {
            bool b = false;
            HtmlDocument document = null;
            HtmlWeb web = new HtmlWeb();
            HtmlNodeCollection nodes;
            String parameter = "";
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
                b = true;
                nodes = document.DocumentNode.SelectNodes(".//div[@class='b-product__title']//a");
                int i = 0;
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes) //вывод товаровпо ключевому слову
                    {
                        i++;
                        Console.WriteLine(i + ") " + node.InnerText);
                    }

                    //пользователь выбирает один из списка товаров
                    Console.WriteLine("Выберите товар из списка (напишите номер)");
                    int number = Convert.ToInt32(Console.ReadLine());
                    parameter = nodes[number - 1].Attributes["href"].Value; //вытащили строку вида "goods/237794" где циферки уникальны для каждого товара
                }
                else
                {
                    Console.WriteLine("По вашему запросу ничего не найдено! ");
                    parameter = "";
                }
            }
          
            return parameter;
        }

        static bool isProductInDB(int parameter) //в nosql проверяем наличие товара и печатаем инфу, если есть
        {
            bool b = false;
            using (var db = new LiteDatabase(@"D:\MyData.db"))
            {
                var col = db.GetCollection<MyProduct>("product");
                var results = col.Find(x => x.Id == parameter);
                if (results.Count() != 0)
                {
                    b = true;
                    Console.WriteLine(" Информация найдена в БД!\n");
                    Console.WriteLine(" -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
                    Console.WriteLine(" Информация о товаре (id =" + results.First().Id + ") ");
                    Console.WriteLine(" -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
                    Console.WriteLine(" Название товара - " + results.First().Title);
                    Console.WriteLine(" Цена товара - " + results.First().Price + " руб. ");
                    Console.WriteLine(" Производитель - " + results.First().Author);
                    Console.WriteLine(" Страна производитель - " + results.First().Country);
                }
            }
            return b;
        }

        static MyProduct FindInfoAboutProductWithPasrsingUlmart(String parameter)
        {
            HtmlDocument document = null;
            HtmlWeb web = new HtmlWeb();
            HtmlNode nodeProduct;
            MyProduct p = new MyProduct();

            String htmlHref = "https://www.ulmart.ru" + parameter; //параметр типа "/goods/237794"
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
                HtmlNodeCollection nodesLabelMain = document.DocumentNode.SelectNodes(".//span[@class='b-dotted-line__title']"); //краткая инфа название характеристик
                HtmlNodeCollection nodesLabel = document.DocumentNode.SelectNodes(".//div[@class='b-dotted-line__title']"); //подробная инфа название характеристик (не входит краткая инфа)
                HtmlNodeCollection nodesValue = document.DocumentNode.SelectNodes(".//div[@class='b-dotted-line__right']//div"); //но тут значения сначала краткой части, потмо подробной
                if (nodesLabelMain != null) //бывает и такое - значит, нет инфы о товаре совсем
                {
                    int i = nodesLabelMain.Count; //соотвественно учитваем разницу, нас интересуют значения после краткой характер.
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
                }
            }
            return p;
        }

        static void InsertInMSSQL(MyProduct p) //в nosql проверяем наличие товара и печатаем инфу, если есть
        {
            //ms sql
            ulmartDBEntities1 context = new ulmartDBEntities1();

            //производитель
            Author aMsSql;
            //проверяем есть ли уже в базе производитель, если нет - то и товара тоже нет
            aMsSql = context.Author.Where(x => x.author1 == p.Author).FirstOrDefault();
            if (aMsSql == null) //нет в базе, добавляем производителя узнаем id
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
            //Запис товар
            Author aMsSql2 = context.Author.Single(d => d.id == aMsSql.id);
            aMsSql2.Product.Add(pMsSql);

            context.SaveChanges();

            Console.WriteLine("\n Инорфмация о всех товарах производителя:");
            foreach (Product pr in aMsSql2.Product)
            {
                Console.WriteLine(" - " + pr.title.Replace("  ", "") + " " + pr.price.Replace("  ", "") + " руб.");
            }
        }
        static void InsertInLiteDB(MyProduct p) //в nosql проверяем наличие товара и печатаем инфу, если есть
        {

            using (var db = new LiteDatabase(@"D:\MyData.db"))
            {
                var col = db.GetCollection<MyProduct>("product");
                col.Insert(p); //liteDB
            }
        }


        static void Main(string[] args)
        {
            MyProduct p = new MyProduct(); //  главный товар 

            HtmlWeb web = new HtmlWeb();
            String htmlHref = ""; //храним ссылку назагружаемую страницу

            // ---- 1 часть выводим информацию по запросу пользователя
            Console.WriteLine("Какой товар вас интересует? (введите ключевое слово) ");
            String parameter = Console.ReadLine();
            Console.WriteLine("Пожалуйста подождите ...");
            htmlHref = "https://www.ulmart.ru/search?string=" + parameter;

            parameter = PrintListInfoAboutKeywordAndReturnChoosenProduct(htmlHref);             //информация о выбранном товаре
            if (parameter != "")
            {
                //---- Часть 2. Выводим полную инфу о выборанном товаре

                if (!isProductInDB(Convert.ToInt32(parameter.Replace("/goods/", "")))) //если нет в БД
                {
                    Console.WriteLine("Пожалуйста подождите ...\n");

                    p = FindInfoAboutProductWithPasrsingUlmart(parameter); //тогда парсим сайт

                    if (p.Author != "") //может не быть инфы, просим пользователя не отчаиваться
                    {
                        //запись в обе БД
                        InsertInMSSQL(p); //в обе таблицы
                        InsertInLiteDB(p);
                        Console.WriteLine("\n Информация добавлена в БД!");
                    }
                    else
                    {
                        Console.WriteLine(" Полная информация о товаре отсутсвует! Товар не занесен в базу, попробуйте отправить запрос позже");
                    }
                }
            }

            Console.WriteLine("\n Для выхода нажмите любую клавишу... ");
            Console.ReadKey();
        }
    }
}
