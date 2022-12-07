using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocRfidReader.Model
{
    public class TagItem
    {
        public string? EPC { get; set; }
        public string? DataSet { get; set; }
        public string? ItemNumber { get; set; }
        public string? ItemName1 { get; set; }
        public DateTime? InsertedTimestamp { get; set; }

        public override string ToString()
        {
            return $"{ItemNumber} | {ItemName1}";
        }
    }
}
