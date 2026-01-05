using System.Collections.Generic;

namespace Data.ViewModels
{
    public class ResponseCollection<T>
    {
        public bool Success { get; set; } = true;
        public string Msg { get; set; } = "OK";

        // wrapper utama
        public CollectionWrapper<T>? Collection { get; set; }

        // ⬇️ PASTI muncul setelah collection
        public int Code { get; set; } = 200;

        public static ResponseCollection<T> Ok(
            IEnumerable<T> data,
            string message = "OK",
            int code = 200)
        {
            return new ResponseCollection<T>
            {
                Success = true,
                Msg = message,
                Code = code,
                Collection = new CollectionWrapper<T>
                {
                    Data = data
                }
            };
        }

        public static ResponseCollection<T> Error(
            string message,
            int code = 500)
        {
            return new ResponseCollection<T>
            {
                Success = false,
                Msg = message,
                Code = code,
                Collection = null
            };
        }
    }

    public class CollectionWrapper<T>
    {
        public IEnumerable<T>? Data { get; set; }
    }

    // ================= SINGLE =================

    public class ResponseSingle<T>
    {
        public bool Success { get; set; } = true;
        public string Msg { get; set; } = "OK";

        // wrapper utama
        public SingleWrapper<T>? Collection { get; set; }

        // ⬇️ sudah benar posisinya
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
