using System.Collections.Generic;

namespace Data.ViewModels
{
    public class ResponseCollection<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "OK";
        public IEnumerable<T>? Data { get; set; }
        public int? TotalRecords { get; set; }
        public int? FilteredRecords { get; set; }

        public static ResponseCollection<T> Ok(IEnumerable<T> data, string message = "OK")
        {
            return new ResponseCollection<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ResponseCollection<T> Error(string message)
        {
            return new ResponseCollection<T>
            {
                Success = false,
                Message = message,
                Data = null
            };
        }

    }
    public class ResponseSingle<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
            public T? Data { get; set; }
            public int Code { get; set; }

            public static ResponseSingle<T> Ok(T data, string message = "Success", int code = 200) =>
                new ResponseSingle<T> { Success = true, Message = message, Data = data, Code = code };

            public static ResponseSingle<T> Error(string message, int code = 500) =>
                new ResponseSingle<T> { Success = false, Message = message, Code = code };
        }
}
