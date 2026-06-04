using MiniTemplateEngine;

namespace MiniTemplateEngineTests
{
    [TestClass]
    public sealed class HtmlTemplateRendererTests
    {
        [TestMethod]
        public void RenderFromString_When_Return()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHtml = "<h1> Привет, ${Name} </h1>";
            var model = new { Name = "Тимерхан" };
            string expectedString = "<h1> Привет, Тимерхан </h1>";
            var result = testee.RenderFromString(templateHtml, model);
            Assert.AreEqual(expectedString, result);
        }

        [TestMethod]
        public void RenderFromString_WhenDoubleReplace_ReturnCorrectString()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHtml = "<h1> Привет, ${Name} </h1><p> Привет, ${Name} </p>";
            var model = new { Name = "Тимерхан" };
            string expectedString = "<h1> Привет, Тимерхан </h1><p> Привет, Тимерхан </p>";
            var result = testee.RenderFromString(templateHtml, model);
            Assert.AreEqual(expectedString, result);
        }

        [TestMethod]
        public void RenderFromString_WhenTwoProperties_ReturnCorrectString()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHtml = "<h1> Привет, ${Name} </h1><p> Привет, ${Email} </p>";
            var model = new { Name = "Тимерхан", Email = "test@test.ru" };
            string expectedString = "<h1> Привет, Тимерхан </h1><p> Привет, test@test.ru </p>";
            var result = testee.RenderFromString(templateHtml, model);
            Assert.AreEqual(expectedString, result);
        }

        [TestMethod]
        public void RenderFromString_WhenSubProperties_ReturnCorrectString()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHtml = "<h1> Привет, ${Name} </h1><p> группа: ${Group.Name} </p>";
            var model = new
            {
                Name = "Тимерхан",
                Group = new { Id = 1, Name = "11-409" }
            };
            string expectedString = "<h1> Привет, Тимерхан </h1><p> группа: 11-409 </p>";
            var result = testee.RenderFromString(templateHtml, model);
            Assert.AreEqual(expectedString, result);
        }

        [TestMethod]
        public void RenderFromString_WhenIf_ReturnCorrectString()
        {
            var testee = new HtmlTemplateRenderer();
            string templateHtml = "<h1>$if(Name == \"Дима\")<p>Привет, Дима</p>$endif</h1>";
            var model = new { Name = "Дима" };
            string expectedString = "<h1><p>Привет, Дима</p></h1>";
            var result = testee.RenderFromString(templateHtml, model);
            Assert.AreEqual(expectedString, result);
        }

        [TestMethod]
        public void RenderFromString_WhenIfElseTrue_ReturnsTrueBlock()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "$if(IsActive)<p>Active</p>$else<p>Inactive</p>$endif";
            var model = new { IsActive = true };
            string expected = "<p>Active</p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenIfElseFalse_ReturnsElseBlock()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "$if(IsActive)<p>Active</p>$else<p>Inactive</p>$endif";
            var model = new { IsActive = false };
            string expected = "<p>Inactive</p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenForeach_ReturnsListItems()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "<ul>$foreach(var u in Users)<li>${u.Name}</li>$endfor</ul>";
            var model = new { Users = new[] { new { Name = "Alice" }, new { Name = "Bob" } } };
            string expected = "<ul><li>Alice</li><li>Bob</li></ul>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenEmptyCollection_ReturnsEmpty()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "<ul>$foreach(var u in Users)<li>${u.Name}</li>$endfor</ul>";
            var model = new { Users = Array.Empty<object>() };
            string expected = "<ul></ul>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenNestedForeach_ReturnsNestedItems()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "$foreach(var g in Groups)<h2>${g.Name}</h2>$foreach(var u in g.Users)<p>${u.Name}</p>$endfor$endfor";
            var model = new
            {
                Groups = new[]
                {
                    new { Name = "Group1", Users = new[] { new { Name = "Alice" }, new { Name = "Bob" } } },
                    new { Name = "Group2", Users = new[] { new { Name = "Charlie" } } }
                }
            };
            string expected = "<h2>Group1</h2><p>Alice</p><p>Bob</p><h2>Group2</h2><p>Charlie</p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenVariableNotExists_ReturnsEmpty()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "<p>${Missing}</p>";
            var model = new { Name = "Test" };
            string expected = "<p></p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenNestedPropertyNotExists_ReturnsEmpty()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "<p>${User.Name}</p>";
            var model = new { };
            string expected = "<p></p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenIfElseWithoutElse_ReturnsEmptyOnFalse()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "$if(IsActive)<p>Active</p>$endif";
            var model = new { IsActive = false };
            string expected = "";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromFile_WhenFileExists_ReturnsRenderedString()
        {
            var testee = new HtmlTemplateRenderer();
            string path = "test.html";
            File.WriteAllText(path, "<h1>${Name}</h1>");
            var model = new { Name = "Alice" };
            string expected = "<h1>Alice</h1>";
            var result = testee.RenderFromFile(path, model);
            File.Delete(path);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderToFile_CreatesFileWithRenderedContent()
        {
            var testee = new HtmlTemplateRenderer();
            string inputPath = "input.html";
            string outputPath = "output.html";
            File.WriteAllText(inputPath, "<h1>${Name}</h1>");
            var model = new { Name = "Bob" };
            testee.RenderToFile(inputPath, outputPath, model);
            string content = File.ReadAllText(outputPath);
            File.Delete(inputPath);
            File.Delete(outputPath);
            Assert.AreEqual("<h1>Bob</h1>", content);
        }

        [TestMethod]
        public void RenderFromString_WhenForeachWithIfElse_ReturnsCorrectResult()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "$foreach(var u in Users)$if(u.IsActive)<p>${u.Name}</p>$endif$endfor";
            var model = new
            {
                Users = new[]
                {
                    new { Name = "Alice", IsActive = true },
                    new { Name = "Bob", IsActive = false }
                }
            };
            string expected = "<p>Alice</p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenMultipleVariablesAndForeach_ReturnsCorrect()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "<h1>${Title}</h1>$foreach(var u in Users)<p>${u.Name} - ${u.Email}</p>$endfor";
            var model = new
            {
                Title = "Users",
                Users = new[]
                {
                    new { Name = "Alice", Email = "a@test.com" },
                    new { Name = "Bob", Email = "b@test.com" }
                }
            };
            string expected = "<h1>Users</h1><p>Alice - a@test.com</p><p>Bob - b@test.com</p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenForeachDictionary_ReturnsCorrect()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "$foreach(var item in Items)<p>${item.Value}</p>$endfor";
            var model = new
            {
                Items = new[]
                {
                    new Dictionary<string, object> { { "Value", "One" } },
                    new Dictionary<string, object> { { "Value", "Two" } }
                }
            };
            string expected = "<p>One</p><p>Two</p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        // --- ДОПОЛНИТЕЛЬНЫЕ ТЕСТЫ ---

        [TestMethod]
        public void RenderFromString_WhenNestedIfInsideIf_ReturnsInnerBlock()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "$if(IsAdmin)$if(IsSuper)<p>Super Admin</p>$else<p>Admin</p>$endif$else<p>User</p>$endif";
            var model = new { IsAdmin = true, IsSuper = true };
            string expected = "<p>Super Admin</p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenNestedIfInsideForeach_ReturnsConditionalBlocks()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "$foreach(var u in Users)$if(u.IsAdmin)<p>${u.Name} (Admin)</p>$else<p>${u.Name}</p>$endif$endfor";
            var model = new
            {
                Users = new[]
                {
                    new { Name = "Alice", IsAdmin = true },
                    new { Name = "Bob", IsAdmin = false }
                }
            };
            string expected = "<p>Alice (Admin)</p><p>Bob</p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenIfInsideNestedForeach_ReturnsExpected()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "$foreach(var g in Groups)$foreach(var u in g.Users)$if(u.Active)<p>${g.Name}-${u.Name}</p>$endif$endfor$endfor";
            var model = new
            {
                Groups = new[]
                {
                    new { Name = "Group1", Users = new[] { new { Name = "Alice", Active = true }, new { Name = "Bob", Active = false } } },
                    new { Name = "Group2", Users = new[] { new { Name = "Charlie", Active = true } } }
                }
            };
            string expected = "<p>Group1-Alice</p><p>Group2-Charlie</p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenConditionUsesBoolProperty_ReturnsTrueBlock()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "$if(Enabled)<p>ON</p>$else<p>OFF</p>$endif";
            var model = new { Enabled = true };
            string expected = "<p>ON</p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenConditionValueNotEqual_ReturnsElseBlock()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "$if(Role==\"Admin\")<p>Admin</p>$else<p>User</p>$endif";
            var model = new { Role = "User" };
            string expected = "<p>User</p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RenderFromString_WhenWhitespaceAndExtraLines_RemovedProperly()
        {
            var testee = new HtmlTemplateRenderer();
            string template = "<p>${Name}</p>\n\n<p>${Email}</p>";
            var model = new { Name = "Test", Email = "mail@test.com" };
            string expected = "<p>Test</p>\n<p>mail@test.com</p>";
            var result = testee.RenderFromString(template, model);
            Assert.AreEqual(expected, result);
        }
    }
}
