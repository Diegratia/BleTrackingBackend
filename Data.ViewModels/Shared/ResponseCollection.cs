using System.Collections.Generic;

namespace Data.ViewModels
{
    public class ResponseCollection<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "OK";
        public CollectionContent<T> Collection { get; set; } = new();

        public static ResponseCollection<T> Ok(IEnumerable<T> data, string message = "OK")
        {
            return new ResponseCollection<T>
            {
                Success = true,
                Message = message,
                Collection = new CollectionContent<T>
                {
                    Data = data
                }
            };
        }

        public static ResponseCollection<T> Error(string message)
        {
            return new ResponseCollection<T>
            {
                Success = false,
                Message = message,
                Collection = new CollectionContent<T>
                {
                    Data = new List<T>()
                }
            };
        }
    }

    public class CollectionContent<T>
    {
        public IEnumerable<T>? Data { get; set; }
        public int? TotalRecords { get; set; }
        public int? FilteredRecords { get; set; }
    }
}
