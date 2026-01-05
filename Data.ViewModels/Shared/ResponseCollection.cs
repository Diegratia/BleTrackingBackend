using System.Collections.Generic;

namespace Data.ViewModels
{
        public class ResponseCollection<T>
    {
        public bool Success { get; set; } = true;
        public string Msg { get; set; } = "OK";

        // wrapper utama
        public CollectionWrapper<T>? Collection { get; set; }

        public static ResponseCollection<T> Ok(
            IEnumerable<T> data,
            string message = "OK")
        {
            return new ResponseCollection<T>
            {
                Success = true,
                Msg = message,
                Collection = new CollectionWrapper<T>
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
                Msg = message,
                Collection = null
            };
        }
    }

    public class CollectionWrapper<T>
    {
        public IEnumerable<T>? Data { get; set; }
    }
    
        public class ResponseSingle<T>
    {
        public bool Success { get; set; } = true;
        public string Msg { get; set; } = "OK";

        // wrapper utama (sama konsep dengan collection)
        public SingleWrapper<T>? Collection { get; set; }

        public int Code { get; set; } = 200;

        public static ResponseSingle<T> Ok(
            T data,
            string message = "Success",
            int code = 200)
        {
            return new ResponseSingle<T>
            {
                Success = true,
                Msg = message,
                Code = code,
                Collection = new SingleWrapper<T>
                {
                    Data = data
                }
            };
        }

        public static ResponseSingle<T> Error(
            string message,
            int code = 500)
        {
            return new ResponseSingle<T>
            {
                Success = false,
                Msg = message,
                Code = code,
                Collection = null
            };
        }
    }

    public class SingleWrapper<T>
    {
        public T? Data { get; set; }
    }
}
