using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels.ResponseHelper
{
        // ApiResponseHelper.cs
        public static class ApiResponse
        {
            // ✅ Success Responses
            public static object Success(string message, object data = null)
            {
                return new
                {
                    success = true,
                    msg = message,
                    collection = new { data },
                    code = 200
                };
            }

            public static object Created(string message, object data = null)
            {
                return new
                {
                    success = true,
                    msg = message,
                    collection = new { data },
                    code = 201
                };
            }

            public static object NoContent(string message)
            {
                return new
                {
                    success = true,
                    msg = message,
                    collection = new { data = (object)null },
                    code = 204
                };
            }

            // ✅ Error Responses
            public static object Error(string message, int code, object data = null)
            {
                return new
                {
                    success = false,
                    msg = message,
                    collection = new { data },
                    code = code
                };
            }

            public static object BadRequest(string message, object errors = null)
            {
                return Error(message, 400, errors);
            }

            public static object NotFound(string message)
            {
                return Error(message, 404);
            }

            public static object Unauthorized(string message = "Unauthorized access")
            {
                return Error(message, 401);
            }

            public static object Forbidden(string message = "Access denied")
            {
                return Error(message, 403);
            }

            public static object InternalError(string message = "Internal server error", object errors = null)
            {
                return Error(message, 500);
            }

            // ✅ Special case untuk DataTables/Filter
            public static object Paginated(string message, object paginatedData)
            {
                return new
                {
                    success = true,
                    msg = message,
                    collection = paginatedData, // Langsung pakai object dari service
                    code = 200
                };
            }
        }
    }
