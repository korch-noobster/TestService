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
        public int FromCurr { get; set; }
        public int ToCurr { get; set; }
        public string Abbreviation { get; set; }
    }
}
