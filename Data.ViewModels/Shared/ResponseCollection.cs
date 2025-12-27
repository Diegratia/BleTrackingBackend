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
}
