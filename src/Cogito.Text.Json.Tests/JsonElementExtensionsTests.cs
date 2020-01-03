using System.Text.Json;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cogito.Text.Json.Tests
{

    [TestClass]
    public class JsonElementExtensionsTests
    {

        [TestMethod]
        public void Can_count_empty_object()
        {
            var d = JsonDocument.Parse(@"{ }");
            d.RootElement.GetPropertyCount().Should().Be(0);
        }

        [TestMethod]
        public void Can_count_single_object_property()
        {
            var d = JsonDocument.Parse(@"{ ""Foo"": ""Bar"" }");
            d.RootElement.GetPropertyCount().Should().Be(1);
        }

        [TestMethod]
        public void Can_count_two_object_property()
        {
            var d = JsonDocument.Parse(@"{ ""Foo"": ""Bar"", ""Foo2"": ""Bar2"" }");
            d.RootElement.GetPropertyCount().Should().Be(2);
        }

        [TestMethod]
        [ExpectedException(typeof(JsonException))]
        public void Should_fail_counting_array()
        {
            var d = JsonDocument.Parse(@"[ ""Foo"" ]");
            d.RootElement.GetPropertyCount();
        }

        [TestMethod]
        [ExpectedException(typeof(JsonException))]
        public void Should_fail_counting_string()
        {
            var d = JsonDocument.Parse(@"""Foo""");
            d.RootElement.GetPropertyCount();
        }

        [TestMethod]
        [ExpectedException(typeof(JsonException))]
        public void Should_fail_counting_number()
        {
            var d = JsonDocument.Parse(@"1");
            d.RootElement.GetPropertyCount();
        }

        [TestMethod]
        [ExpectedException(typeof(JsonException))]
        public void Should_fail_counting_null()
        {
            var d = JsonDocument.Parse(@"null");
            d.RootElement.GetPropertyCount();
        }

    }

}
