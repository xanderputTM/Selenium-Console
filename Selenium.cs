// See https://aka.ms/new-console-template for more information
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Collections.ObjectModel;
using System.Text.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        List<Dictionary<String, String>> dataYoutube = ScrapeYoutube();
        writeToJson(dataYoutube, "youtube-data.json");

        List<Dictionary<String, String>> dataICTJobs = ScrapeICTJobs();
        writeToJson(dataICTJobs, "ictjobs-data.json");

        Dictionary<String, String> dataWikipedia = ScrapeWikipedia();
        writeToJson(dataWikipedia, "wikipedia-data.json");
    }

    private static List<Dictionary<String, String>> ScrapeYoutube()
    {
        ChromeOptions chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("start-maximized");
        chromeOptions.AddArgument("headless");
        chromeOptions.AddArgument("log-level=3");

        IWebDriver driver = new ChromeDriver(chromeOptions);
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        driver.Navigate().GoToUrl("https://www.youtube.com/");

        String XPathCookies = "//*[@id=\"content\"]/div[2]/div[6]/div[1]/ytd-button-renderer[1]/yt-button-shape/button";
        IWebElement cookiesButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(XPathCookies)));
        cookiesButton.Click();

        Console.WriteLine("Enter search term:");
        driver.Manage().Window.Minimize();
        string searchTerm = Console.ReadLine();
        driver.Manage().Window.Maximize();


        String XPathSearchBar = "//input[@id=\"search\"]";
        IWebElement searchBar = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(XPathSearchBar)));
        searchBar.Click();
        searchBar.SendKeys(searchTerm);
        searchBar.Submit();

        // Open filters
        String XPathFilter = "//*[@id=\"container\"]/ytd-toggle-button-renderer";
        IWebElement filterButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(XPathFilter)));
        filterButton.Click();

        // Sort by recent
        String XPathRecent = "//*[@id=\"collapse-content\"]/ytd-search-filter-group-renderer[5]/ytd-search-filter-renderer[2]";
        IWebElement sortByRecentButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(XPathRecent)));
        sortByRecentButton.Click();
        filterButton.Click();

        List <Dictionary<String, String>> data = new List<Dictionary<String, String>>();

        ReadOnlyCollection<IWebElement> titles = driver.FindElements(By.XPath("//*[@id=\"video-title\"]/yt-formatted-string"));
        ReadOnlyCollection<IWebElement> channels = driver.FindElements(By.XPath("//*[@id=\"channel-name\"]/div/div/yt-formatted-string/a"));
        ReadOnlyCollection<IWebElement> views = driver.FindElements(By.XPath("/html/body/ytd-app/div[1]/ytd-page-manager/ytd-search/div[1]/ytd-two-column-search-results-renderer/div[2]/div/ytd-section-list-renderer/div[2]/ytd-item-section-renderer/div[3]/ytd-video-renderer/div[1]/div/div[1]/ytd-video-meta-block/div[1]/div[2]/span[1]"));
        ReadOnlyCollection<IWebElement> uploadDate = driver.FindElements(By.XPath("/html/body/ytd-app/div[1]/ytd-page-manager/ytd-search/div[1]/ytd-two-column-search-results-renderer/div[2]/div/ytd-section-list-renderer/div[2]/ytd-item-section-renderer/div[3]/ytd-video-renderer/div[1]/div/div[1]/ytd-video-meta-block/div[1]/div[2]/span[2]"));
        ReadOnlyCollection<IWebElement> links = driver.FindElements(By.XPath("//a[@id=\"video-title\"]"));
        for (int i = 0; i < 5; i++)
        {
            Dictionary<String, String> video = new Dictionary<String, String>();
            Console.WriteLine($"\nVideo {i + 1}");
            Console.WriteLine($"Title: {titles[i].Text}");
            Console.WriteLine($"Channel: {channels[i].Text}");
            Console.WriteLine($"Views: {views[i].Text}");
            Console.WriteLine($"Uploaded: {uploadDate[i].Text}");
            Console.WriteLine($"URL: {links[i].GetAttribute("href")}");
            video.Add("id", $"{i + 1}");
            video.Add("title", titles[i].Text);
            video.Add("channel", channels[i].Text);
            video.Add("views", views[i].Text);
            video.Add("uploadDate", uploadDate[i].Text);
            video.Add("url", links[i].GetAttribute("href"));
            data.Add(video);
        }

        return data;
    }

    private static List<Dictionary<String, String>> ScrapeICTJobs()
    {
        ChromeOptions chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("start-maximized");
        chromeOptions.AddArgument("headless");
        chromeOptions.AddArgument("log-level=3");

        IWebDriver driver = new ChromeDriver(chromeOptions);
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        IJavaScriptExecutor js = (IJavaScriptExecutor) driver;

        driver.Navigate().GoToUrl("https://www.ictjob.be/");


        driver.Manage().Window.Minimize();
        Console.WriteLine("Enter search term:");
        string searchTerm = Console.ReadLine();
        driver.Manage().Window.Maximize();


        String XPathSearchBar = "//input[@id=\"keywords-input\"]";
        IWebElement searchBar = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(XPathSearchBar)));
        searchBar.Click();
        searchBar.SendKeys(searchTerm);
        searchBar.Submit();

        String XPathRecent = "//*[@id=\"search-result\"]/div[1]/div[2]/div/div[2]/span[2]";
        IWebElement sortByRecentButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(XPathRecent)));
        js.ExecuteScript("arguments[0].scrollIntoView();", sortByRecentButton);
        sortByRecentButton.Click();

        while (driver.FindElement(By.Id("search-result-body")).GetCssValue("opacity") == "0.5") {
            Console.WriteLine("Waiting for sorting...");
            Thread.Sleep(250);
        }

        IWebElement searchResults = driver.FindElement(By.Id("search-result-body"));
        ReadOnlyCollection<IWebElement> jobs = searchResults.FindElements(By.XPath(".//div/ul/li/span[@class=\"job-info\"]"));

        List<Dictionary<String, String>> data = new List<Dictionary<String, String>>();


        for (int i = 0; i < 5; i++)
        {
            IWebElement job = jobs.ElementAt(i);
            String id = $"{i + 1}";
            String title = job.FindElement(By.XPath(".//h2[@class=\"job-title\"]")).Text;
            String company = job.FindElement(By.ClassName("job-company")).Text;
            String location = job.FindElement(By.XPath(".//*[@class=\"job-location\"]/span/span")).Text;
            String keywords = job.FindElement(By.ClassName("job-keywords")).Text;
            String url = job.FindElement(By.TagName("a")).GetAttribute("href");

            Console.WriteLine("");
            Console.WriteLine($"Job {i + 1}");
            Console.WriteLine($"Title: {title}");
            Console.WriteLine($"Company: {company}");
            Console.WriteLine($"Location: {location}");
            Console.WriteLine($"Keywords: {keywords}");
            Console.WriteLine($"URL: {url}");

            Dictionary<String, String> video = new Dictionary<String, String>();
            video.Add("id", id);
            video.Add("title", title);
            video.Add("company", company);
            video.Add("location", location);
            video.Add("keywords", keywords);
            video.Add("url", url);
            data.Add(video);
        }

        return data;
    }

    private static Dictionary<String, String> ScrapeWikipedia() { 
        ChromeOptions chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("start-maximized");
        chromeOptions.AddArgument("headless");
        chromeOptions.AddArgument("log-level=3");

        IWebDriver driver = new ChromeDriver(chromeOptions);
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

        driver.Navigate().GoToUrl("https://www.wikipedia.org/");


        driver.Manage().Window.Minimize();
        Console.WriteLine("Enter search term:");
        string searchTerm = Console.ReadLine();
        driver.Manage().Window.Maximize();


        IWebElement searchBar = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("searchInput")));
        searchBar.Click();
        searchBar.SendKeys(searchTerm);
        searchBar.Submit();

        IWebElement infobox = driver.FindElement(By.ClassName("infobox"));
        IWebElement infoboxTitle = infobox.FindElement(By.ClassName("infobox-above"));
        Console.WriteLine($"\n{infoboxTitle.Text}");

        Dictionary<String, String> data = new Dictionary<String, String>();

        ReadOnlyCollection<IWebElement> infoboxRows = infobox.FindElements(By.TagName("tr"));
        foreach (IWebElement row in infoboxRows) {
            Dictionary<String, String> video = new Dictionary<String, String>();

            ReadOnlyCollection<IWebElement> headers = row.FindElements(By.ClassName("infobox-header"));
            IWebElement header;

            if (headers.Count > 0) {
                header = headers.First();
                Console.WriteLine($"\n{header.Text}");
            }

            try {
                IWebElement label = row.FindElement(By.ClassName("infobox-label"));
                IWebElement infoboxData = row.FindElement(By.ClassName("infobox-data"));
                
                Console.WriteLine($"{label.Text}:\n{infoboxData.Text}\n");
                data.Add(label.Text, infoboxData.Text);
            }
            catch { }

        }

        return data;

    }


    private static void writeToJson(object? data, String filename) {
        var options = new JsonSerializerOptions() {
            WriteIndented = true
        };

        string jsonString = JsonSerializer.Serialize(data, options); 
        File.WriteAllText(@$"C:\Users\Xander\source\repos\Selenium\files\{filename}", jsonString);
    }
}