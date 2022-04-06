using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClothScrapper.Model
{
    internal class ZalandoScrapper : API.Scrapper
    {
        public ZalandoScrapper() : base("https://www.zalando.fr")
        {

        }

        public override void SetCategoriesUrl()
        {
            List<string> mainPages = new List<string>
            {
                "femme",
                "homme",
                "enfant"
            };

            foreach (var mainPage in mainPages)
            {
                _driver.Navigate().GoToUrl($"{_baseUrl}/mode-{mainPage}");

                IWebElement categories = _driver.FindElement(By.CssSelector("ul[role='tree']"));
                var lis = categories.FindElements(By.TagName("li")).Skip(1);

                foreach (var li in lis)
                {
                    var href = li.FindElement(By.XPath("a")).GetAttribute("href");
                    _categoriesUrl.Add(href);
                    Console.WriteLine(href);
                }
            }
        }

        public override async Task ScrapCategory(string url)
        {
            _driver.Navigate().GoToUrl(url);

            try
            {
                if (!_driver.PageSource.Contains("Oops, il y a comme un hic"))
                {
                    var articles = _driver.FindElements(By.TagName("article"));
                    foreach (var article in articles)
                    {
                        try
                        {
                            _js.ExecuteScript("arguments[0].scrollIntoView(false);", article);
                            if (article.Text.Contains("€"))
                            {
                                SetSingleCloth(article, url);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }

                    if (_currentPage < _pageToScrapPerCategory)
                    {
                        _currentPage++;
                        await ScrapCategory($"{_categorieUrl}?p={_currentPage}");
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public override async Task SetSingleCloth(IWebElement article, string currentCategory)
        {
            var brandName = article.FindElement(By.TagName("header"))
                                    .FindElement(By.TagName("span")).Text;
            string pp = article.Text;
            string price = Regex.Replace(article.Text.Split("\n").FirstOrDefault(x => x.Contains("€")), "[A-Za-z-À-ÖØ-öø-ÿЀ]", "")
                                                                .Replace(" €", "")
                                                                .Replace(",", ".")
                                                                .Trim();
            var cloth = new Cloth
            {
                Brand = brandName,
                Price = Convert.ToDouble(price),
                Category = currentCategory.Replace(_baseUrl, "").Replace("/", "")
            };

            var subDiv = article.FindElement(By.XPath("//div[4]"));

            var images = article.FindElements(By.CssSelector("img[draggable='false']"));

            foreach (var image in images)
            {
                string src = image.GetAttribute("src");
                if (src.Contains("packshot"))
                {
                    cloth.Image = src.Split("?")[0];
                    break;
                }
            }

            if (!String.IsNullOrEmpty(cloth.Image))
            {
                _cloths.Add(cloth);
                _dbHelper.AddCloth(cloth);

                string imageName = cloth.Image.Split("/").Last();

                await _fileHelper.DownloadImageAsync(imageName, cloth.Category, new Uri(cloth.Image));
            }
        }
    }
}
