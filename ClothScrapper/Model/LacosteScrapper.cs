using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ClothScrapper.Model
{
    internal class LacosteScrapper : API.Scrapper
    {
        public LacosteScrapper() : base("https://www.lacoste.com")
        {

        }

        public override void SetCategoriesUrl()
        {
            List<string> mainPages = new List<string>
            {
                "femme",
                "homme"
            };

            foreach (var mainPage in mainPages)
            {
                _driver.Navigate().GoToUrl($"{_baseUrl}/fr/lacoste/{mainPage}/vetements/");

                var categories = _driver.FindElements(By.ClassName("plp-category-link"));

                foreach (var category in categories.Skip(1))
                {
                    var href = category.GetAttribute("href");
                    _categoriesUrl.Add(href);
                    Console.WriteLine(href);
                }
            }
        }

        public override async Task ScrapCategory(string url)
        {
            _driver.Navigate().GoToUrl(url);
            Thread.Sleep(5000);

            if (!_driver.PageSource.Contains("Hors-jeu"))
            {
                var articles = _driver.FindElements(By.XPath("//div[@data-image]"));
                foreach (var article in articles)
                {
                    try
                    {
                        _js.ExecuteScript("arguments[0].scrollIntoView(false);", article);

                        await SetSingleCloth(article, url);
                    }
                    catch (Exception e)
                    {
                    }
                }

                if (_currentPage < _pageToScrapPerCategory)
                {
                    _currentPage++;
                    await ScrapCategory($"{_currentCategoryUrl}?page={_currentPage}");
                }
            }
        }

        public override async Task SetSingleCloth(IWebElement article, string currentCategory)
        {
            var parent = article.FindElement(By.XPath("../../.."));

            string img = parent.FindElement(By.TagName("img")).GetAttribute("src").Replace("_20", "_24");

            string price = parent.FindElement(By.ClassName("sales-price"))
                                            .Text.Replace(" €", "")
                                            .Replace(".", ",")
                                            .Trim();

            var cloth = new Cloth
            {
                Image = img.Split("/").Last(x => x != "").Split("?")[0],
                ImageUrl = img,
                Brand = "Lacoste",
                Price = Convert.ToDouble(price),
                Category = $"{(_currentCategoryUrl.Contains("femme") ? "femme" : "homme")}-{_currentCategoryUrl.Split("/").Last(x => x != "")}"
            };

            _cloths.Add(cloth);
            _dbHelper.AddCloth(cloth);

            await _fileHelper.DownloadImageAsync(cloth.Image, $"{cloth.Brand}/{cloth.Category}", new Uri(cloth.ImageUrl));
        }
    }
}
