using Npgsql;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace MyORMLibrary
{
    public class ORMContext
    {
        private readonly string _connectionString;

        public ORMContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public T Create<T>(T entity) where T : class, new()
        {
            string tableName = GetTableName<T>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var props = typeof(T).GetProperties()
                        .Where(p => !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
                        .ToArray();

            var columns = string.Join(", ", props.Select(p => $"\"{GetColumnName(p)}\""));
            var parameters = string.Join(", ", props.Select(p => $"@{p.Name}"));

            // Добавляем RETURNING "id", чтобы получить сгенерированный ID
            string sql = $"INSERT INTO \"{tableName}\" ({columns}) VALUES ({parameters}) RETURNING \"id\"";

            using var cmd = new NpgsqlCommand(sql, connection);
            foreach (var prop in props)
            {
                object? value = ConvertValueForDb(prop.GetValue(entity));
                cmd.Parameters.AddWithValue($"@{prop.Name}", value ?? DBNull.Value);
            }

            var result = cmd.ExecuteScalar();
            if (result != null && result != DBNull.Value)
            {
                var idProp = typeof(T).GetProperties()
                    .FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));
                if (idProp != null && idProp.CanWrite)
                {
                    var idValue = Convert.ChangeType(result, idProp.PropertyType);
                    idProp.SetValue(entity, idValue);
                }
            }

            return entity;
        }

        public T ReadById<T>(int id) where T : class, new()
        {
            string tableName = GetTableName<T>();
            string sql = $"SELECT {GetSelectColumns<T>()} FROM \"{tableName}\" WHERE \"id\" = @id";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();


            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapToEntity<T>(reader);

            return null;
        }

        public List<T> ReadByAll<T>() where T : class, new()
        {
            string tableName = GetTableName<T>();
            string sql = $"SELECT {GetSelectColumns<T>()} FROM \"{tableName}\"";

            var result = new List<T>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(MapToEntity<T>(reader));
            }

            return result;
        }

        public void Update<T>(int id, T entity) where T : class, new()
        {
            string tableName = GetTableName<T>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var props = typeof(T).GetProperties()
                        .Where(p => !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
                        .ToArray();

            var setClauses = string.Join(", ", props.Select(p => $"\"{GetColumnName(p)}\" = @{p.Name}"));

            string sql = $"UPDATE \"{tableName}\" SET {setClauses} WHERE \"id\" = @id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            foreach (var prop in props)
            {
                object? value = ConvertValueForDb(prop.GetValue(entity));
                cmd.Parameters.AddWithValue($"@{prop.Name}", value ?? DBNull.Value);
            }

            cmd.ExecuteNonQuery();
        }

        public void Delete<T>(int id) where T : class
        {
            string tableName = GetTableName<T>();
            string sql = $"DELETE FROM \"{tableName}\" WHERE \"id\" = @id";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private string GetTableName<T>()
        {
            string tableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(T).Name + "s";
            if (string.IsNullOrEmpty(tableName))
                return tableName;
            return char.ToUpperInvariant(tableName[0]) + tableName.Substring(1);
        }

        private string GetColumnName(PropertyInfo property)
        {
            // Сначала проверяем атрибут Column
            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
            if (columnAttr != null && !string.IsNullOrEmpty(columnAttr.Name))
            {
                return columnAttr.Name.ToLowerInvariant();
            }

            // Если атрибута нет, используем имя свойства в нижнем регистре
            return property.Name.ToLowerInvariant();
        }

        private string GetSelectColumns<T>()
        {
            var props = typeof(T).GetProperties();
            var columns = props.Select(p => $"\"{GetColumnName(p)}\"");
            return string.Join(", ", columns);
        }

        private T MapToEntity<T>(NpgsqlDataReader reader) where T : new()
        {
            var entity = new T();
            var props = typeof(T).GetProperties()
                .ToDictionary(p => GetColumnName(p).ToLowerInvariant(), p => p);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string colName = reader.GetName(i).ToLowerInvariant();
                if (!props.TryGetValue(colName, out var prop))
                    continue;

                if (reader.IsDBNull(i))
                {
                    if (prop.PropertyType.IsClass || Nullable.GetUnderlyingType(prop.PropertyType) != null)
                        prop.SetValue(entity, null);
                }
                else
                {
                    object value = reader.GetValue(i);

                    if (IsListType(prop.PropertyType))
                    {
                        Type listType = prop.PropertyType;
                        Type elementType = listType.GetGenericArguments()[0];

                        if (value is Array dbArray)
                        {
                            var list = Array.CreateInstance(elementType, dbArray.Length);
                            Array.Copy(dbArray, list, dbArray.Length);
                            var concreteList = typeof(List<>).MakeGenericType(elementType);
                            var listInstance = Activator.CreateInstance(concreteList, new object[] { list });
                            prop.SetValue(entity, listInstance);
                        }
                    }
                    else
                    {
                        prop.SetValue(entity, Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType));
                    }
                }
            }

            return entity;
        }

        private static readonly HashSet<Type> SupportedListTypes = new()
        {
            typeof(string), typeof(int), typeof(long), typeof(bool), typeof(decimal), typeof(DateTime)
        };

        private bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) &&
                   SupportedListTypes.Contains(type.GetGenericArguments()[0]);
        }

        private object? ConvertValueForDb(object? value)
        {
            if (value == null) return null;

            Type valueType = value.GetType();

            if (IsListType(valueType))
            {
                var list = (System.Collections.IList)value;
                Type elementType = valueType.GetGenericArguments()[0];
                Array array = Array.CreateInstance(elementType, list.Count);
                for (int i = 0; i < list.Count; i++)
                    array.SetValue(list[i], i);
                return array;
            }

            return value;
        }

        // =======================
        //     LINQ-подобные методы
        // =======================

        public IEnumerable<T> Where<T>(Expression<Func<T, bool>> predicate, int limit = 0) where T : class, new()
        {
            var (sql, parameters) = BuildSqlQuery(predicate, limit);
            return ExecuteQueryMultiple<T>(sql, parameters);
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            var (sql, parameters) = BuildSqlQuery(predicate);
            return ExecuteQuerySingle<T>(sql, parameters);
        }

        private (string Sql, List<NpgsqlParameter> Parameters) BuildSqlQuery<T>(
            Expression<Func<T, bool>> predicate, int limit = 1)
        {
            var parameters = new List<NpgsqlParameter>();
            string tableName = GetTableName<T>();
            string selectColumns = GetSelectColumns<T>();
            string whereClause = ParseExpression(predicate.Body, parameters);
            string stringLimit = limit == 0 ? "" : $"Limit {limit}";
            string sql = $"SELECT {selectColumns} FROM \"{tableName}\" WHERE {whereClause} {stringLimit}".Trim();
            return (sql, parameters);
        }

        private string ParseExpression(Expression expression, List<NpgsqlParameter> parameters)
        {
            switch (expression)
            {
                case BinaryExpression binary:
                    string left = ParseExpression(binary.Left, parameters);
                    string right = ParseExpression(binary.Right, parameters);

                    if (right == "NULL" && binary.NodeType == ExpressionType.Equal)
                        return $"({left} IS NULL)";
                    if (right == "NULL" && binary.NodeType == ExpressionType.NotEqual)
                        return $"({left} IS NOT NULL)";

                    string op = GetSqlOperator(binary.NodeType);
                    return $"({left} {op} {right})";

                case MemberExpression member when member.Expression is ParameterExpression:
                    // Это поле сущности: x => x.Name
                    var memberProp = ((ParameterExpression)member.Expression).Type.GetProperty(member.Member.Name);
                    string columnName = memberProp != null ? GetColumnName(memberProp) : member.Member.Name.ToLowerInvariant();
                    return $"\"{columnName}\"";

                case MemberExpression member:
                    // Это захваченная переменная или константа — вычисляем её значение
                    object? value = EvaluateExpression(member);
                    var param = CreateParameter(value, parameters);
                    return param.ParameterName;

                case ConstantExpression constant:
                    var p = CreateParameter(constant.Value, parameters);
                    return p.ParameterName;

                case UnaryExpression unary when unary.NodeType == ExpressionType.Not:
                    return $"(NOT {ParseExpression(unary.Operand, parameters)})";

                case MethodCallExpression method:
                    return ParseMethodCall(method, parameters);

                default:
                    throw new NotSupportedException($"Unsupported expression: {expression.NodeType}");
            }
        }

        private string ParseMethodCall(MethodCallExpression method, List<NpgsqlParameter> parameters)
        {
            if (method.Method.DeclaringType == typeof(string))
            {
                string member = ParseExpression(method.Object!, parameters);
                string argument = ParseExpression(method.Arguments[0], parameters);

                return method.Method.Name switch
                {
                    nameof(string.Contains) => $"({member} ILIKE '%' || {argument} || '%')",
                    nameof(string.StartsWith) => $"({member} ILIKE {argument} || '%')",
                    nameof(string.EndsWith) => $"({member} ILIKE '%' || {argument})",
                    _ => throw new NotSupportedException($"Unsupported string method: {method.Method.Name}")
                };
            }

            if (method.Method.Name == nameof(Enumerable.Contains) && method.Arguments.Count == 2)
            {

                var collectionExpr = method.Object;
                var itemExpr = method.Arguments[0];

                if (collectionExpr is MemberExpression memExpr && memExpr.Expression is ParameterExpression)
                {
                    // Это случай: x.Tags.Contains("value")
                    string column = ParseExpression(memExpr, parameters);
                    string value = ParseExpression(itemExpr, parameters);
                    return $"({value} = ANY({column}))";
                }

                // Это случай: list.Contains(x.Property)
                var collection = (IEnumerable<object>)EvaluateExpression(collectionExpr);
                string property = ParseExpression(itemExpr, parameters);


                var paramNames = new List<string>();
                foreach (var item in collection)
                {
                    var p = CreateParameter(item, parameters);
                    paramNames.Add(p.ParameterName);
                }

                string values = string.Join(", ", paramNames);
                return $"({property} IN ({values}))";
            }

            throw new NotSupportedException($"Unsupported method: {method.Method.Name}");
        }


        // Безопасная оценка любого подвыражения (включая замыкания)
        private object? EvaluateExpression(Expression expr)
        {
            var objectExpr = Expression.Convert(expr, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(objectExpr);
            var func = lambda.Compile();
            return func();
        }

        private NpgsqlParameter CreateParameter(object? value, List<NpgsqlParameter> parameters)
        {
            string paramName = $"@p{parameters.Count}";
            var param = new NpgsqlParameter(paramName, value ?? DBNull.Value);
            parameters.Add(param);
            return param;
        }

        private string GetSqlOperator(ExpressionType nodeType) => nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.LessThan => "<",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => throw new NotSupportedException($"Unsupported operator: {nodeType}")
        };

        private T ExecuteQuerySingle<T>(string sql, List<NpgsqlParameter> parameters) where T : class, new()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters.ToArray());
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapToEntity<T>(reader);
            return null;
        }

        private IEnumerable<T> ExecuteQueryMultiple<T>(string sql, List<NpgsqlParameter> parameters) where T : class, new()
        {
            var list = new List<T>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters.ToArray());
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapToEntity<T>(reader));
            return list;
        }
        public List<T> ExecuteQuery<T>(string sql, params NpgsqlParameter[] parameters) where T : class, new()
        {
            var results = new List<T>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand(sql, connection);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(MapToEntity<T>(reader));
            }

            return results;
        }

        public int ExecuteNonQuery(string sql, params NpgsqlParameter[] parameters)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand(sql, connection);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            return cmd.ExecuteNonQuery();
        }

        // Метод для выполнения SQL-запроса и получения одного значения
        public T ExecuteScalar<T>(string sql, params NpgsqlParameter[] parameters)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand(sql, connection);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            var result = cmd.ExecuteScalar();
            if (result == null || result == DBNull.Value)
            {
                return default(T);
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }
    }
}