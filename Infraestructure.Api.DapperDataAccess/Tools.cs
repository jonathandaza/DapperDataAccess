using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Api.DapperDataAccess
{
    public static class Tools
    {
        public static List<string> exeptionToArray(string message)
        {
            return message.Split(new string[] { "\r\n" }, 100, System.StringSplitOptions.RemoveEmptyEntries).ToList<string>();
        }
    }
}
