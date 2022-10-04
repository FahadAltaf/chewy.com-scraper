using chewy.com_scraper;
using CsvHelper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using System.Globalization;
using System.Web;

List<DataModel> entries = new List<DataModel>();

List<Categories> categories = new List<Categories>();
using (var reader = new StreamReader("file.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    categories = csv.GetRecords<Categories>().ToList();
}

int pages = (categories.Count + 1 - 1) / 1;
List<Task> tasks = new List<Task>();
for (int count = 1; count <= pages; ++count)
{
    int index = count - 1;
    var data = categories.Skip(index * 1).Take(1).ToList();

    Task newTask = Task.Factory.StartNew(() => ScrapeEntries(data));
    tasks.Add(newTask);

    if (count % 10 == 0 || count == pages)
    {
        foreach (Task task in tasks)
        {
            while (!task.IsCompleted)
            { }
        }
    }
}

File.WriteAllText("Result.json", JsonConvert.SerializeObject(entries));
Console.WriteLine("Completed");
Console.ReadKey();

void ScrapeEntries(List<Categories> data)
{
    using (var driver = new ChromeDriver())
    {
        foreach (var cat in data)
        {
            Console.WriteLine("Processing " + cat.Url);
            DataModel model = new DataModel { Name = cat.Name, URL = cat.Url };
            try
            {
                driver.Navigate().GoToUrl(cat.Url);

                Thread.Sleep(5000);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                try
                {
                    var nodes = doc.DocumentNode.SelectNodes("//div").Where(x => x.Attributes.FirstOrDefault(y => y.Name == "class") != null);
                    if (nodes != null)
                    {
                        model.Description = HttpUtility.HtmlDecode(nodes.FirstOrDefault(x => x.Attributes["class"].Value.Contains("kib-typography-paragraph2 SupportingText_content__")).InnerText);
                    }
                }
                catch { }

                try
                {
                    var categoriesNode = doc.DocumentNode.SelectNodes("//a[@class='CategoryEntry_categoryLabel__20yXW']");
                    if (categoriesNode != null)
                    {
                        List<Categories> categories = new List<Categories>();
                        foreach (var item in categoriesNode)
                        {
                            var category = HttpUtility.HtmlDecode(item.InnerText);
                            var link = "https://www.chewy.com" + item.Attributes["href"].Value;
                            categories.Add(new Categories { Name = category, Url = link });
                        }
                        model.Categories = categories;
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {

            }
            entries.Add(model);
        }
    }
}
