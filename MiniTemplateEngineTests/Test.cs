using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using MiniTemplateEngine;

namespace MiniTemplateEngine.Tests
{
    [TestClass]
    public class HtmlTemplateRendererTests
    {
        private IHtmlTemplateRenderer _renderer;

        [TestInitialize]
        public void Setup()
        {
            _renderer = new HtmlTemplateRenderer();
        }

        [TestMethod]
        public void RenderFromString_WithNestedModelAndList_RenderedCorrectly()
        {
            // Arrange
            var template = @"
<html>
<head><title>${Hotel.Name}</title></head>
<body>
    <h1>${Hotel.Name}</h1>
    <p>Location: ${Hotel.Location}</p>
    <p>Header Photo: ${HeaderPhoto}</p>
    <p>Gallery Main Photo: ${GalleryPhotos.Url}</p>
    <div>
        $foreach(var amenity in Amenities)
        <div class=""amenity"">
            <h3>${amenity.Title}</h3>
            $foreach(var item in amenity.Items)
            <span>${item}</span>
            $endfor
        </div>
        $endfor
    </div>
</body>
</html>";

            var model = new HotelPageModel
            {
                Hotel = new HotelInfo
                {
                    Name = "Grand Hotel",
                    Location = "Paris"
                },
                HeaderPhoto = "header.jpg",
                GalleryPhotos = new HotelImage
                {
                    Url = "gallery/main.jpg"
                },
                Amenities = new List<AmenitySection>
                {
                    new AmenitySection
                    {
                        Title = "Room Amenities",
                        Items = new List<string> { "WiFi", "TV", "Mini Bar" }
                    },
                    new AmenitySection
                    {
                        Title = "Hotel Amenities",
                        Items = new List<string> { "Pool", "Gym", "Spa" }
                    }
                }
            };

            // Act
            var result = _renderer.RenderFromString(template, model);

            // Assert
            Assert.IsTrue(result.Contains("<h1>Grand Hotel</h1>"));
            Assert.IsTrue(result.Contains("<p>Location: Paris</p>"));
            Assert.IsTrue(result.Contains("<p>Header Photo: header.jpg</p>"));
            Assert.IsTrue(result.Contains("<p>Gallery Main Photo: gallery/main.jpg</p>"));
            Assert.IsTrue(result.Contains("<h3>Room Amenities</h3>"));
            Assert.IsTrue(result.Contains("<h3>Hotel Amenities</h3>"));
            Assert.IsTrue(result.Contains("<span>WiFi</span>"));
            Assert.IsTrue(result.Contains("<span>TV</span>"));
            Assert.IsTrue(result.Contains("<span>Pool</span>"));
        }
    }

    public class HotelPageModel
    {
        public HotelInfo Hotel { get; set; }
        public string HeaderPhoto { get; set; }
        public HotelImage GalleryPhotos { get; set; }
        public List<AmenitySection> Amenities { get; set; }
    }

    public class HotelInfo
    {
        public string Name { get; set; }
        public string Location { get; set; }
    }

    public class HotelImage
    {
        public string Url { get; set; }
    }

    public class AmenitySection
    {
        public string Title { get; set; }
        public List<string> Items { get; set; }
    }
}
