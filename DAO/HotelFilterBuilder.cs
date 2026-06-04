using ModelsLibrary;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAO
{
    public class HotelFilterBuilder : IHotelFilterBuilder
    {
        public (string Sql, List<NpgsqlParameter> Parameters) BuildFilterQuery(HotelFilter filter)
        {
            var parameters = new List<NpgsqlParameter>();
            var whereConditions = new List<string>();
            int paramIndex = 0;

            string selectClause = @"
                SELECT ""id"", ""name"", ""description"", ""city"", ""country"", ""stars"", ""rating"", 
                       ""about_hotel"", ""meal"", ""note"", ""image_names"", ""card""
                FROM ""Hotels""";

            switch (filter.Type?.ToLower())
            {
                case "country":
                    ApplyCountryFilters(filter, whereConditions, parameters, ref paramIndex);
                    break;
                case "city":
                    ApplyCityFilters(filter, whereConditions, parameters, ref paramIndex);
                    break;
                case "hotel":
                    ApplyHotelFilters(filter, whereConditions, parameters, ref paramIndex);
                    break;
                default:
                    whereConditions.Add("1 = 0");
                    break;
            }

            ApplyCommonFilters(filter, whereConditions, parameters, ref paramIndex);

            if (filter.Services != null && filter.Services.Count > 0 && !(filter.Services.Count == 1 && filter.Services[0] == 0))
            {
                ApplyServiceFilters(filter, whereConditions, parameters, ref paramIndex);
            }
            string whereClause = whereConditions.Any() ? $"WHERE {string.Join(" AND ", whereConditions)}" : "";
            string orderClause = "ORDER BY \"rating\" DESC";
            string limitClause = $"LIMIT {filter.Limit} OFFSET {filter.StartValue}";

            string sql = $"{selectClause} {whereClause} {orderClause} {limitClause}".Trim();

            return (sql, parameters);
        }

        private void ApplyCountryFilters(HotelFilter filter, List<string> conditions, List<NpgsqlParameter> parameters, ref int paramIndex)
        {
            if (!string.IsNullOrEmpty(filter.Name))
            {
                conditions.Add($"\"country\" ILIKE @p{paramIndex}");
                parameters.Add(new NpgsqlParameter($"@p{paramIndex}", $"%{filter.Name}%"));
                paramIndex++;
            }
        }

        private void ApplyCityFilters(HotelFilter filter, List<string> conditions, List<NpgsqlParameter> parameters, ref int paramIndex)
        {
            if (!string.IsNullOrEmpty(filter.Name))
            {
                conditions.Add($"\"city\" ILIKE @p{paramIndex}");
                parameters.Add(new NpgsqlParameter($"@p{paramIndex}", $"%{filter.Name}%"));
                paramIndex++;
            }

            if (!string.IsNullOrEmpty(filter.Detail))
            {
                conditions.Add($"\"country\" ILIKE @p{paramIndex}");
                parameters.Add(new NpgsqlParameter($"@p{paramIndex}", $"%{filter.Detail}%"));
                paramIndex++;
            }
        }

        private void ApplyHotelFilters(HotelFilter filter, List<string> conditions, List<NpgsqlParameter> parameters, ref int paramIndex)
        {
            if (!string.IsNullOrEmpty(filter.Name))
            {
                conditions.Add($"\"name\" ILIKE @p{paramIndex}");
                parameters.Add(new NpgsqlParameter($"@p{paramIndex}", $"%{filter.Name}%"));
                paramIndex++;
            }

            if (!string.IsNullOrEmpty(filter.Detail))
            {
                var parts = filter.Detail.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length >= 2)
                {
                    conditions.Add($"\"city\" ILIKE @p{paramIndex} AND \"country\" ILIKE @p{paramIndex + 1}");
                    parameters.Add(new NpgsqlParameter($"@p{paramIndex}", $"%{parts[0]}%"));
                    parameters.Add(new NpgsqlParameter($"@p{paramIndex + 1}", $"%{parts[1]}%"));
                    paramIndex += 2;
                }
            }
        }

        private void ApplyCommonFilters(HotelFilter filter, List<string> conditions, List<NpgsqlParameter> parameters, ref int paramIndex)
        {
            if (filter.Stars > 0)
            {
                conditions.Add($"\"stars\" >= @p{paramIndex}");
                parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.Stars));
                paramIndex++;
            }

            if (filter.Rating > 0)
            {
                conditions.Add($"\"rating\" >= @p{paramIndex}");
                parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.Rating));
                paramIndex++;
            }

            if (!string.IsNullOrEmpty(filter.Meal) && filter.Meal != "Любой")
            {
                conditions.Add($"\"meal\" ILIKE @p{paramIndex}");
                parameters.Add(new NpgsqlParameter($"@p{paramIndex}", $"%{filter.Meal}%"));
                paramIndex++;
            }
        }


        private void ApplyServiceFilters(HotelFilter filter, List<string> conditions, List<NpgsqlParameter> parameters, ref int paramIndex)
        {
            var servicePlaceholders = string.Join(",", filter.Services.Select((_, i) => $"@service{i}"));

            conditions.Add($@"
                ""id"" IN (
                    SELECT ""hotel_id""
                    FROM ""Hotel_Services""
                    WHERE ""service_id"" IN ({servicePlaceholders})
                    GROUP BY ""hotel_id""
                    HAVING COUNT(DISTINCT ""service_id"") = {filter.Services.Count}
                )");

            for (int i = 0; i < filter.Services.Count; i++)
            {
                parameters.Add(new NpgsqlParameter($"@service{i}", filter.Services[i]));
            }
        }
    }
}