using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Api.DapperDataAccess
{
    public class Result
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public IEnumerable<string> Errors { get; private set; }

        public Result(bool success)
        {
            Success = success;
        }

        public Result(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public Result(bool success, string message, IEnumerable<string> errors)
        {
            Success = success;
            Message = message;
            Errors = errors;
        }

        public Result(bool success, IEnumerable<string> errors)
        {
            Success = success;
            Errors = errors;
        }


    }

    public class Result<T> : Result 
    {
        public T Data { get; private set; }

        public Result(bool success, string message, IEnumerable<string> errors)
            : base(success, message, errors)
        {

        }

        public Result(bool success, IEnumerable<string> errors)
            : base(success, errors)
        {

        }


        public Result(bool success, string message)
            : base(success, message)
        {

        }

        public Result(bool success, string message, T data)
            : base(success, message)
        {
            Data = data;
        }

        public Result(bool success, T data)
            : base(success)
        {
            Data = data;
        }

    }
}
