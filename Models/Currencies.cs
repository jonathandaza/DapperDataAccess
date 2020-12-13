using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infraestructure.Api.ModelAnalizer;

namespace Models
{
    [EntityInfo("adCurrency")]
    public class Currencies
    {
        [EntityInfo("curId", false, true)]
        public int Id { get; set; }

        [EntityInfo("curCode", true, false)]
        public string Code { get; set; }

        [EntityInfo("curName")]
        public string Name { get; set; }
    }
}
