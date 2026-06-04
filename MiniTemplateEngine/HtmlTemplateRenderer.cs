using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniTemplateEngine
{
    public class HtmlTemplateRenderer : IHtmlTemplateRenderer
    {
        public string RenderFromFile(string filePath, object dataModel)
        {
            string html = File.ReadAllText(filePath);
            return RenderFromString(html, dataModel);
        }

        public string RenderFromString(string htmlTemplate, object dataModel)
        {
            Console.WriteLine("=== START RENDER ===");
            var result = htmlTemplate;
            result = ProcessFor(result, dataModel);
            Console.WriteLine($"After ProcessFor: {result.Length} chars");
            result = ProcessForeach(result, dataModel);
            Console.WriteLine($"After ProcessForeach: {result.Length} chars");
            result = ProcessIfElse(result, dataModel);
            Console.WriteLine($"After ProcessIfElse: {result.Length} chars");
            result = ProcessVariables(result, dataModel);
            Console.WriteLine($"After ProcessVariables: {result.Length} chars");
            result = Regex.Replace(result, @"\n\s*\n", "\n");
            Console.WriteLine("=== END RENDER ===");
            return result;
        }

        public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
        {
            var result = RenderFromFile(inputFilePath, dataModel);
            File.WriteAllText(outputFilePath, result);
            return result;
        }

        private string ProcessFor(string template, object dataModel)
        {
            var result = new StringBuilder();
            int pos = 0;

            while (true)
            {
                int startIndex = template.IndexOf("$for", pos, StringComparison.OrdinalIgnoreCase);
                if (startIndex == -1)
                {
                    result.Append(template.Substring(pos));
                    break;
                }

                result.Append(template.Substring(pos, startIndex - pos));

                int endCondition = template.IndexOf(')', startIndex);
                if (endCondition == -1) break;

                string header = template.Substring(startIndex, endCondition - startIndex + 1);

                // Поддерживаем синтаксис: for(int i = 0; i < hotel.Stars; i++)
                var headerMatch = Regex.Match(header,
                    @"\$for\s*\(\s*int\s+(\w+)\s*=\s*(\d+)\s*;\s*\1\s*([<>=!]+)\s*([^;]+)\s*;\s*\1\s*(\+\+|--)\s*\)",
                    RegexOptions.IgnoreCase);

                if (!headerMatch.Success)
                {
                    // Если не нашли match, пропускаем этот блок
                    result.Append(header);
                    pos = endCondition + 1;
                    continue;
                }

                string varName = headerMatch.Groups[1].Value.Trim();
                string startValue = headerMatch.Groups[2].Value.Trim();
                string conditionOperator = headerMatch.Groups[3].Value.Trim();
                string conditionValuePath = headerMatch.Groups[4].Value.Trim();
                string incrementOperator = headerMatch.Groups[5].Value.Trim();

                int bodyStart = endCondition + 1;
                int searchPos = bodyStart;
                int nested = 0;
                int bodyEnd = -1;

                // Ищем конец блока for ($endfor)
                while (searchPos < template.Length)
                {
                    int nextFor = template.IndexOf("$for", searchPos, StringComparison.OrdinalIgnoreCase);
                    int nextEndfor = template.IndexOf("$endfor", searchPos, StringComparison.OrdinalIgnoreCase);

                    if (nextEndfor == -1) break;

                    if (nextFor != -1 && nextFor < nextEndfor)
                    {
                        nested++;
                        searchPos = nextFor + 1;
                    }
                    else
                    {
                        if (nested == 0)
                        {
                            bodyEnd = nextEndfor;
                            break;
                        }
                        nested--;
                        searchPos = nextEndfor + 1;
                    }
                }

                if (bodyEnd == -1) break;

                string loopBody = template.Substring(bodyStart, bodyEnd - bodyStart);

                // Получаем максимальное значение для условия из текущего контекста
                var maxValueObj = GetValueByPath(dataModel, conditionValuePath);
                if (maxValueObj == null || !int.TryParse(maxValueObj.ToString(), out int maxValue))
                {
                    // Если не получилось получить число, пропускаем цикл но оставляем содержимое
                    result.Append(loopBody);
                    pos = bodyEnd + "$endfor".Length;
                    continue;
                }

                int start = int.Parse(startValue);

                // Создаем контекст для цикла, наследуя текущий dataModel
                // ВАЖНО: не создаем новый словарь, а используем существующий контекст
                var loopContext = dataModel;

                // Выполняем цикл
                StringBuilder loopResult = new StringBuilder();
                for (int i = start; EvaluateForCondition(i, conditionOperator, maxValue); i = ExecuteIncrement(i, incrementOperator))
                {
                    // Создаем временный контекст с переменной цикла
                    var tempContext = new Dictionary<string, object>();

                    // Копируем существующий контекст
                    if (dataModel is IDictionary<string, object> dictParent)
                    {
                        foreach (var kv in dictParent)
                            tempContext[kv.Key] = kv.Value;
                    }
                    else
                    {
                        var props = dataModel.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var prop in props)
                            tempContext[prop.Name] = prop.GetValue(dataModel);
                    }

                    // Добавляем переменную цикла
                    tempContext[varName] = i;

                    // Обрабатываем тело цикла с временным контекстом
                    var itemResult = ProcessFor(loopBody, tempContext);
                    itemResult = ProcessForeach(itemResult, tempContext);
                    itemResult = ProcessIfElse(itemResult, tempContext);
                    itemResult = ProcessVariables(itemResult, tempContext);

                    loopResult.Append(itemResult);
                }

                result.Append(loopResult);
                pos = bodyEnd + "$endfor".Length;
            }

            return result.ToString();
        }

        private bool EvaluateForCondition(int currentValue, string conditionOperator, int maxValue)
        {
            return conditionOperator switch
            {
                "<" => currentValue < maxValue,
                "<=" => currentValue <= maxValue,
                ">" => currentValue > maxValue,
                ">=" => currentValue >= maxValue,
                "==" => currentValue == maxValue,
                "!=" => currentValue != maxValue,
                _ => false
            };
        }

        private int ExecuteIncrement(int currentValue, string incrementOperator)
        {
            return incrementOperator switch
            {
                "++" => currentValue + 1,
                "--" => currentValue - 1,
                _ => currentValue
            };
        }

        private string ProcessForeach(string template, object dataModel)
        {
            var result = new StringBuilder();
            int pos = 0;

            while (true)
            {
                int startIndex = template.IndexOf("$foreach", pos, StringComparison.OrdinalIgnoreCase);
                if (startIndex == -1)
                {
                    result.Append(template.Substring(pos));
                    break;
                }

                result.Append(template.Substring(pos, startIndex - pos));

                int endCondition = template.IndexOf(')', startIndex);
                if (endCondition == -1) break;

                string header = template.Substring(startIndex, endCondition - startIndex + 1);
                var headerMatch = Regex.Match(header, @"\$foreach\s*\(\s*var\s+(\w+)\s+in\s+([^)]+)\s*\)", RegexOptions.IgnoreCase);
                if (!headerMatch.Success) break;

                string itemName = headerMatch.Groups[1].Value.Trim();
                string collectionPath = headerMatch.Groups[2].Value.Trim();

                int bodyStart = endCondition + 1;
                int searchPos = bodyStart;
                int nested = 0;
                int bodyEnd = -1;

                while (searchPos < template.Length)
                {
                    int nextForeach = template.IndexOf("$foreach", searchPos, StringComparison.OrdinalIgnoreCase);
                    int nextEndfor = template.IndexOf("$endfor", searchPos, StringComparison.OrdinalIgnoreCase);

                    if (nextEndfor == -1) break;

                    if (nextForeach != -1 && nextForeach < nextEndfor)
                    {
                        nested++;
                        searchPos = nextForeach + 1;
                    }
                    else
                    {
                        if (nested == 0)
                        {
                            bodyEnd = nextEndfor;
                            break;
                        }
                        nested--;
                        searchPos = nextEndfor + 1;
                    }
                }

                if (bodyEnd == -1) break;

                string loopBody = template.Substring(bodyStart, bodyEnd - bodyStart);

                var collection = GetValueByPath(dataModel, collectionPath) as System.Collections.IEnumerable;
                if (collection != null)
                {
                    foreach (var item in collection)
                    {
                        var loopContext = new Dictionary<string, object>();
                        if (dataModel is IDictionary<string, object> dictParent)
                        {
                            foreach (var kv in dictParent)
                                loopContext[kv.Key] = kv.Value;
                        }
                        else
                        {
                            var props = dataModel.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                            foreach (var prop in props)
                                loopContext[prop.Name] = prop.GetValue(dataModel);
                        }
                        loopContext[itemName] = item;

                        var itemResult = ProcessForeach(loopBody, loopContext);
                        itemResult = ProcessIfElse(itemResult, loopContext);
                        itemResult = ProcessVariables(itemResult, loopContext);

                        result.Append(itemResult);
                    }
                }

                pos = bodyEnd + "$endfor".Length;
            }

            return result.ToString();
        }

        private string ProcessIfElse(string template, object dataModel)
        {
            var result = new StringBuilder();
            int pos = 0;

            while (true)
            {
                int startIndex = template.IndexOf("$if", pos, StringComparison.OrdinalIgnoreCase);
                if (startIndex == -1)
                {
                    result.Append(template.Substring(pos));
                    break;
                }

                result.Append(template.Substring(pos, startIndex - pos));

                int endCondition = template.IndexOf(')', startIndex);
                if (endCondition == -1) break;

                string header = template.Substring(startIndex, endCondition - startIndex + 1);
                var headerMatch = Regex.Match(header, @"\$if\s*\(([^)]*)\)", RegexOptions.IgnoreCase);
                if (!headerMatch.Success) break;

                string condition = headerMatch.Groups[1].Value.Trim();

                int bodyStart = endCondition + 1;
                int searchPos = bodyStart;
                int nested = 0;
                int elseIndex = -1;
                int bodyEnd = -1;

                while (searchPos < template.Length)
                {
                    int nextIf = template.IndexOf("$if", searchPos, StringComparison.OrdinalIgnoreCase);
                    int nextElse = template.IndexOf("$else", searchPos, StringComparison.OrdinalIgnoreCase);
                    int nextEndif = template.IndexOf("$endif", searchPos, StringComparison.OrdinalIgnoreCase);

                    if (nextEndif == -1) break;

                    if (nextIf != -1 && nextIf < nextEndif && (nextElse == -1 || nextIf < nextElse))
                    {
                        nested++;
                        searchPos = nextIf + 1;
                    }
                    else if (nextElse != -1 && nextElse < nextEndif && nested == 0 && elseIndex == -1)
                    {
                        elseIndex = nextElse;
                        searchPos = nextElse + 1;
                    }
                    else
                    {
                        if (nested == 0)
                        {
                            bodyEnd = nextEndif;
                            break;
                        }
                        nested--;
                        searchPos = nextEndif + 1;
                    }
                }

                if (bodyEnd == -1) break;

                string truePart, falsePart = string.Empty;
                if (elseIndex != -1)
                {
                    truePart = template.Substring(bodyStart, elseIndex - bodyStart);
                    falsePart = template.Substring(elseIndex + "$else".Length, bodyEnd - (elseIndex + "$else".Length));
                }
                else
                {
                    truePart = template.Substring(bodyStart, bodyEnd - bodyStart);
                }

                bool isTrue = EvaluateCondition(dataModel, condition);
                string chosenPart = isTrue ? truePart : falsePart;

                string processed = ProcessIfElse(chosenPart, dataModel);
                processed = ProcessVariables(processed, dataModel);

                result.Append(processed);
                pos = bodyEnd + "$endif".Length;
            }

            return result.ToString();
        }

        private bool EvaluateCondition(object dataModel, string condition)
        {
            var parts = condition.Split(new[] { "==" }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                var propName = parts[0].Trim();
                var expectedValue = parts[1].Trim().Trim('"');
                var actualValue = GetValueByPath(dataModel, propName)?.ToString();
                return actualValue == expectedValue;
            }

            var condValue = GetValueByPath(dataModel, condition);
            bool isTrue = false;
            if (condValue is bool boolValue)
                isTrue = boolValue;
            else if (condValue != null)
                isTrue = true;

            return isTrue;
        }

        private string ProcessVariables(string template, object dataModel)
        {
            // Изменил регулярное выражение, чтобы оно учитывало квадратные скобки
            var varRegex = new Regex(@"\$\{([A-Za-z_][A-Za-z0-9_\.\[\]]*)\}");

            return varRegex.Replace(template, match =>
            {
                string path = match.Groups[1].Value;
                var value = GetValueByPath(dataModel, path);
                return value?.ToString() ?? string.Empty;
            });
        }

        public static object? GetValueByPath(object obj, string path)
        {
            if (obj == null || string.IsNullOrEmpty(path))
                return null;

            // Простая замена квадратных скобок на точки для обработки
            // и создание массива индексов
            var modifiedPath = Regex.Replace(path, @"\[([0-9]+)\]", ".$1");
            var parts = modifiedPath.Split('.');
            object? current = obj;

            for (int i = 0; i < parts.Length; i++)
            {
                if (current == null)
                    return null;

                string part = parts[i];

                // Проверяем, является ли текущая часть числовым индексом
                if (int.TryParse(part, out int index))
                {
                    if (current is IList list && index >= 0 && index < list.Count)
                    {
                        current = list[index];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    // Это обычное свойство
                    if (current is IDictionary<string, object> dict)
                    {
                        dict.TryGetValue(part, out current);
                    }
                    else
                    {
                        var type = current.GetType();
                        var prop = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (prop != null)
                            current = prop.GetValue(current);
                        else
                        {
                            var field = type.GetField(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                            if (field != null)
                                current = field.GetValue(current);
                            else
                                return null;
                        }
                    }
                }
            }

            return current;
        }
    }
}