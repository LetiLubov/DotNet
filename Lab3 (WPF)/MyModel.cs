using HtmlAgilityPack;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf
{
    public class MyModel
    {
        
        static HtmlNodeCollection ParseUlmartByKeyword(String htmlHref) //ИЩЕТ товары по ключевому слову на сайте
        {
            HtmlDocument document = null;
            HtmlWeb web = new HtmlWeb();
            HtmlNodeCollection nodes;
            try
            {
                document = web.Load("https://www.ulmart.ru/search?string=" + htmlHref);
            }
            catch (Exception ex)
            {
             

            }
            //получаем список товаров по запросу 
            if (document != null)
            {
                nodes = document.DocumentNode.SelectNodes(".//div[@class='b-product__title']//a");
                return nodes;
            }
            return null;
        }
        static String GetHrefOnLastProductInList(String htmlHref)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlNodeCollection nodes;
            String parameter = "";
           
            nodes = ParseUlmartByKeyword(htmlHref);//возвращаю инфу по узлам (потребуется для аттребута ссылки на товар, который выберет юзер)
            if (nodes != null)
            {
                int number = nodes.Count() - 1;
                parameter = nodes[number - 1].Attributes["href"].Value; //вытащили строку вида "goods/237794" где циферки уникальны для каждого товара
            }
            else
            {
               
                parameter = "";
            }
            return parameter;
        }

        static MyProduct isProductInDB(int parameter) //в nosql проверяем наличие товара и выдаем инфу, если есть
        {
            MyProduct p = new MyProduct();
            using (var db = new LiteDatabase(@"D:\MyData.db"))
            {
                var col = db.GetCollection<MyProduct>("product");
                var results = col.Find(x => x.Id == parameter);
                if (results.Any())
                {
                    
                    p.Id = results.First().Id;
                    p.Title = results.First().Title;
                    p.Price = results.First().Price ;
                    p.Author = results.First().Author;
                    p.Country = results.First().Country;
                    return p;
                }
            }
            return null;
        }

        static MyProduct FindInfoAboutProductWithPasrsingUlmart(String parameter) //неделима. Получает инфу о товаре. 
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
            }
            if (document != null)
            {
              
                p.Id = Convert.ToInt32(parameter.Replace("/goods/", ""));

                nodeProduct = document.DocumentNode.SelectSingleNode(".//h1[@class='main-h1 main-h1_bold js-reload']");
                p.Title = nodeProduct.InnerText.Replace("\n", "").Replace("  ", "");

                nodeProduct = document.DocumentNode.SelectSingleNode(".//span[@class='b-price__num js-price']");
                String s = nodeProduct.InnerText;//Цена записана очень дико с большим количеством лишних символов
                s = s.Replace("&nbsp;", " ").Replace("\n", "").Replace("  ", "");
                p.Price = s;

                //Далее получение характеристик товара. Они представлены как значения таблицы. 
                // название - значение
                HtmlNodeCollection nodesLabelMain = document.DocumentNode.SelectNodes(".//span[@class='b-dotted-line__title']"); //раздел краткая инфа, название характеристик
                HtmlNodeCollection nodesLabel = document.DocumentNode.SelectNodes(".//div[@class='b-dotted-line__title']"); //подробная инфа, название характеристик (не входит краткая инфа)
                HtmlNodeCollection nodesValue = document.DocumentNode.SelectNodes(".//div[@class='b-dotted-line__right']//div"); //но тут значения сначала краткого раздела, потом подробного
                if (nodesLabelMain != null) //бывает и такое - значит, нет инфы о товаре совсем
                {
                    int i = nodesLabelMain.Count; //соотвественно учитваем разницу, нас интересуют значения после краткого раздела.
                    if (nodesLabelMain != null && nodesLabel != null)
                    {
                        foreach (HtmlNode node in nodesLabel)
                        {
                            if (node.InnerText.Replace("\n", "").Replace("  ", "") == "Производитель")
                            {
                               
                                p.Author = nodesValue[i].InnerText.Replace("\n", "").Replace("  ", "");
                            }
                            if (node.InnerText.Replace("\n", "").Replace("  ", "") == "Страна производства")
                            {
                               
                                p.Country = nodesValue[i].InnerText.Replace("\n", "").Replace("  ", "");
                            }
                            i++;
                        }
                    }
                }
            }
            return p;
        }

       

        static void InsertInLiteDB(MyProduct p) //в nosql проверяем наличие товара и выдаем инфу, если есть
        {

            using (var db = new LiteDatabase(@"D:\MyData.db"))
            {
                var col = db.GetCollection<MyProduct>("product");
                col.Insert(p); //liteDB
            }
        }


        static public MyProduct GetProductByKeyword(String htmlHref)
        {
            MyProduct p = new MyProduct(); //  главный товар 
            HtmlWeb web = new HtmlWeb();
            String parameter = GetHrefOnLastProductInList(htmlHref);
            p = isProductInDB(Convert.ToInt32(parameter.Replace("/goods/", "")));
            if (p != null)
                return p;
            else
                p = FindInfoAboutProductWithPasrsingUlmart(parameter);
            InsertInLiteDB(p);
            return p;
           
        }
    }
}
