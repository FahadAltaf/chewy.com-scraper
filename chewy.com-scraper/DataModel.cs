using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chewy.com_scraper
{
    public class DataModel
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string Description { get; set; }
        public List<Categories> Categories { get; set; } = new List<Categories>();
    }

    public class Categories
    {
        [Index(0)]
        public string Name { get; set; }
        [Index(1)]
        public string Url { get; set; }
    }
}
