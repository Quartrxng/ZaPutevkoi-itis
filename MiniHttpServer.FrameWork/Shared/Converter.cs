namespace MiniHttpServer.FrameWork.Shared
{
    public static class Converter
    {
        public static object ConvertValue(Type type, string value)
        {
            if (type == typeof(string))
                return value;
            else if (type == typeof(int) || type == typeof(int?))
            {
                if (int.TryParse(value, out var val))
                    return val;
                else return 0;
            }
            if (type == typeof(bool) || type == typeof(bool?))
            {
                if (bool.TryParse(value, out bool b))
                    return b;
                return false;
            }
            return value;
        }
    }
}
