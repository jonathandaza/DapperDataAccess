using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Api.ModelAnalizer
{
    public class BooleanConversion: Attribute
    {
        public string TrueValue { get; internal set; }

        public string FalseValue { get; internal set; }

        public BooleanConversion(string trueValue, string falseValue)
        {
            TrueValue = trueValue;

            FalseValue = falseValue;
        }
    }
}
