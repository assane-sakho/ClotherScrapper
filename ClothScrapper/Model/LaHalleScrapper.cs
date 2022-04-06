using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ClothScrapper.Model
{
    internal class LaHalleScrapper : API.Scrapper
    {
        private HashSet<IWebElement> _oldElements;

        public LaHalleScrapper() : base("https://www.lahalle.com/", 1)
        {
            _oldElements = new HashSet<IWebElement>();
        }

        public override void SetCategoriesUrl()
        {
            List<string> mainPages = new List<string>
            {
                "vetements-femme-cf_010000",
                "vetements-homme"
            };

            foreach (var mainPage in mainPages)
            {
                _driver.Navigate().GoToUrl($"{_baseUrl}/{mainPage}");

                var categories = _driver.FindElement(By.Id("category-level-1"))
                                        .FindElements(By.ClassName("category-element"));

                foreach (var category in categories.Skip(5))
                {
                    var href = category.FindElement(By.TagName("a"))
                                        .GetAttribute("href");
                    _categoriesUrl.Add(href);
                }
            }
        }

        public override async Task ScrapCategory(string url)
        {
            if(_currentPage == 1)
            {
                _driver.Navigate().GoToUrl(url);
                Thread.Sleep(5000);
                var cookieButton = _driver.FindElements(By.Id("popin_tc_privacy_button_2"));

                if (cookieButton.Any())
                    cookieButton.FirstOrDefault().Click();
            }

            while (true)
            {
                var elements = _driver.FindElements(By.ClassName("product-name")).Where(x => !_oldElements.Contains(x)).ToList();

                foreach (var element in elements)
                {
                    _oldElements.Add(element);
                    try
                    {
                        _js.ExecuteScript("arguments[0].scrollIntoView(false);", element);

                        await SetSingleCloth(element.FindElement(By.XPath("../..")), url);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("error: " + e.Message);
                    }
                }

                var nextButton = _driver.FindElements(By.ClassName("js-product-list-insert"));
                if (nextButton.Any())
                {
                    nextButton.First().Click();
                    _currentPage++;
                    Thread.Sleep(15000);
                    await ScrapCategory("");
                }
                else
                {
                    break;
                }
            }
        }

        public override async Task SetSingleCloth(IWebElement article, string currentCategory)
        {
            string img = article.FindElement(By.TagName("img")).GetAttribute("src").Replace("-b-", "-e-");

            string brand = article.FindElement(By.ClassName("product-name"))
                                  .FindElement(By.TagName("span")).Text;

            string price = article.FindElement(By.ClassName("product-sales-price"))
                            .Text
                            .Replace("€", ",");

            if(price == String.Empty || brand == String.Empty)
            {
                article = article.FindElement(By.XPath(".."));

                brand = article.FindElement(By.XPath("..")).FindElement(By.ClassName("product-name"))
                                  .FindElement(By.TagName("span")).Text;

                price = article.FindElement(By.ClassName("product-sales-price"))
                            .Text
                            .Replace("€", ",");
            }

            var cloth = new Cloth
            {
                Image = img.Split("/").Last(x => x != "").Split("?").First().Split("-").Last(),
                ImageUrl = img,
                Brand = brand,
                Price = Convert.ToDouble(price),
                Category = _currentCategoryUrl.Split("/").Last(x => x != "").Replace("-scf_010900", "")
            };

            _cloths.Add(cloth);
            _dbHelper.AddCloth(cloth);

            try
            {
                await _fileHelper.DownloadImageAsync(cloth.Image, $"LaHalle/{cloth.Category}", new Uri(cloth.ImageUrl));
            }
            catch (Exception)  //Image file doesn't exist
            {
                await _fileHelper.DownloadImageAsync(cloth.Image, $"LaHalle/{cloth.Category}", new Uri(cloth.ImageUrl.Replace("-e-", "-b-")));
            }
        }
    }
}
