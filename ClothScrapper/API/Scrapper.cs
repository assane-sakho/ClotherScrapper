using ClothScrapper.Helper;
using ClothScrapper.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace ClothScrapper.API
{
    abstract class Scrapper
    {
        #region PROPERTIES
        internal IWebDriver _driver;
        internal IJavaScriptExecutor _js;
        internal List<Cloth> _cloths;
        internal readonly DbHelper _dbHelper;
        internal readonly FileHelper _fileHelper;
        internal readonly int _pageToScrapPerCategory;
        internal readonly string _baseUrl;
        internal string _currentCategoryUrl;
        internal List<string> _categoriesUrl;
        internal int _currentPage;
        #endregion

        public Scrapper(string baseUrl, int pageToScrapPerCategory = 30)
        {
            _baseUrl = baseUrl;
            _pageToScrapPerCategory = pageToScrapPerCategory;

            _dbHelper = DbHelper.GetInstance();
            _fileHelper = FileHelper.GetInstance();
            _cloths = new List<Cloth>();
            _categoriesUrl = new List<string>();

            ChromeOptions chromeOptions = new ChromeOptions();
            //ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            //chromeDriverService.HideCommandPromptWindow = true;
            //chromeDriverService.SuppressInitialDiagnosticInformation = true;

            //chromeOptions.AddArgument("headless");
            //chromeOptions.AddArgument("--silent");
            //chromeOptions.AddArgument("log-level=3");
            chromeOptions.AddArgument("--start-maximized");

            _driver = new ChromeDriver(chromeOptions);

            _js = (IJavaScriptExecutor)_driver;

            _currentPage = 1;
            _currentCategoryUrl = String.Empty;
        }

        /// <summary>
        /// Get all the categories to visit
        /// </summary>
        abstract public void SetCategoriesUrl();

        abstract public Task ScrapCategory(string url);

        abstract public Task SetSingleCloth(IWebElement article, string currentCategory);

        public async Task Start()
        {
            SetCategoriesUrl();

            foreach (var categoryUrl in _categoriesUrl)
            {
                _currentCategoryUrl = categoryUrl;
                await ScrapCategory(_currentCategoryUrl);
            }

            Stop();
        }

        private void Stop()
        {
            _driver.Quit();
        }
    }
}
