using ModelsLibrary;
using Npgsql;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DAO
{
    public interface IHotelFilterBuilder
    {
        (string Sql, List<NpgsqlParameter> Parameters) BuildFilterQuery(HotelFilter filter);
    }
}