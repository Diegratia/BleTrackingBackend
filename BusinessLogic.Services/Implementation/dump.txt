using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Data.ViewModels;

namespace BusinessLogic.Services.Implementation
{
    public class GenericDataTableService<TModel, TDto> where TModel : class where TDto : class
    {
        private readonly IQueryable<TModel> _query;
        private readonly IMapper _mapper;
        private readonly string[] _searchableColumns;
        private readonly string[] _validSortColumns;

        public GenericDataTableService(
            IQueryable<TModel> query,
            IMapper mapper,
            string[] searchableColumns,
            string[] validSortColumns)
        {
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _searchableColumns = searchableColumns ?? throw new ArgumentNullException(nameof(searchableColumns));
            _validSortColumns = validSortColumns ?? throw new ArgumentNullException(nameof(validSortColumns));
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            // Validasi input
            if (request.Length < 1)
                throw new ArgumentException("Length must be greater than or equal to 1.");
            if (request.Start < 0)
                throw new ArgumentException("Start cannot be negative.");
            if (string.IsNullOrEmpty(request.SortColumn) || !_validSortColumns.Contains(request.SortColumn))
                request.SortColumn = _validSortColumns.First();
            if (string.IsNullOrEmpty(request.SortDir) || !new[] { "asc", "desc" }.Contains(request.SortDir.ToLower()))
                request.SortDir = "asc";

            // Ambil query dasar
            var query = _query;

            // Hitung total record sebelum filter
            var totalRecords = await query.CountAsync();

            // Terapkan pencarian global
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                var predicates = _searchableColumns
                    .Select(col => $"{col}.ToLower().Contains(@0)") // Gunakan Contains untuk kompatibilitas
                    .Aggregate((current, next) => $"{current} || {next}");
                query = query.Where(predicates, search);
            }

            // Hitung total record setelah filter
            var filteredRecords = await query.CountAsync();

            // Terapkan pengurutan
            var sortDirection = request.SortDir.ToLower() == "asc" ? "ascending" : "descending";
            query = query.OrderBy($"{request.SortColumn} {sortDirection}");

            // Terapkan paging
            query = query.Skip(request.Start).Take(request.Length);

            // Ambil data
            var data = await query.ToListAsync();
            var dtos = _mapper.Map<IEnumerable<TDto>>(data);

            return new
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = dtos
            };
        }
    }
}