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
        public string fromCurr { get; set; }
        public string toCurr { get; set; }
    }
}
