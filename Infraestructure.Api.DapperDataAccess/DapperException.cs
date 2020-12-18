using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Api.DapperDataAccess
{
    public class DapperDataAccessException : Exception
    {
        public DapperDataAccessException()
        {

        }

        public DapperDataAccessException(string message)
            : base(message)
        {

        }

        public DapperDataAccessException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
