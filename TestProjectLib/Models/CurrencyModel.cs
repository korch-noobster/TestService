using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProjectLib.Models
{
    public class CurrencyModel
    {
        public int Id { get; set; }
        public int fromCurr { get; set; }
        public int toCurr { get; set; }
        public string Abbreviation { get; set; }
    }
}
