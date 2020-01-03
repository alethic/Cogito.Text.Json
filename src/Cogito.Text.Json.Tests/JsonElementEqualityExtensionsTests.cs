using System.Text.Json;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cogito.Text.Json.Tests
{

    [TestClass]
    public class JsonElementEqualityExtensionsTests
    {

        [TestMethod]
        public void Can_compare_empty_object()
        {
            var a = JsonDocument.Parse(@"{}").RootElement;
            var b = JsonDocument.Parse(@"{}").RootElement;
            a.DeepEquals(b).Should().BeTrue();
            a.GetDeepHashCode().Should().Be(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Can_compare_string()
        {
            var a = JsonDocument.Parse(@"""foo""").RootElement;
            var b = JsonDocument.Parse(@"""foo""").RootElement;
            a.DeepEquals(b).Should().BeTrue();
            a.GetDeepHashCode().Should().Be(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Can_compare_number()
        {
            var a = JsonDocument.Parse(@"1").RootElement;
            var b = JsonDocument.Parse(@"1").RootElement;
            a.DeepEquals(b).Should().BeTrue();
            a.GetDeepHashCode().Should().Be(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Can_compare_boolean()
        {
            var a = JsonDocument.Parse(@"true").RootElement;
            var b = JsonDocument.Parse(@"true").RootElement;
            a.DeepEquals(b).Should().BeTrue();
            a.GetDeepHashCode().Should().Be(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Can_compare_decimal_to_integer()
        {
            var a = JsonDocument.Parse(@"0.0").RootElement;
            var b = JsonDocument.Parse(@"0").RootElement;
            a.DeepEquals(b).Should().BeTrue();
            a.GetDeepHashCode().Should().Be(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Should_fail_comparing_decimal_to_integer()
        {
            var a = JsonDocument.Parse(@"0.1").RootElement;
            var b = JsonDocument.Parse(@"0").RootElement;
            a.DeepEquals(b).Should().BeFalse();
            a.GetDeepHashCode().Should().NotBe(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Should_fail_comparing_number_to_string()
        {
            var a = JsonDocument.Parse(@"0").RootElement;
            var b = JsonDocument.Parse(@"""hi""").RootElement;
            a.DeepEquals(b).Should().BeFalse();
            a.GetDeepHashCode().Should().NotBe(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Should_fail_comparing_number_to_array()
        {
            var a = JsonDocument.Parse(@"0").RootElement;
            var b = JsonDocument.Parse(@"[]").RootElement;
            a.DeepEquals(b).Should().BeFalse();
            a.GetDeepHashCode().Should().NotBe(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Should_fail_comparing_number_to_object()
        {
            var a = JsonDocument.Parse(@"0").RootElement;
            var b = JsonDocument.Parse(@"{}").RootElement;
            a.DeepEquals(b).Should().BeFalse();
            a.GetDeepHashCode().Should().NotBe(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Can_compare_array_items()
        {
            var a = JsonDocument.Parse(@"[1,2,3]").RootElement;
            var b = JsonDocument.Parse(@"[1,2,3]").RootElement;
            a.DeepEquals(b).Should().BeTrue();
            a.GetDeepHashCode().Should().Be(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Should_fail_comparing_unequal_arrays()
        {
            var a = JsonDocument.Parse(@"[1,2,3]").RootElement;
            var b = JsonDocument.Parse(@"[1,2,4]").RootElement;
            a.DeepEquals(b).Should().BeFalse();
            a.GetDeepHashCode().Should().NotBe(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Should_fail_comparing_unequal_arrays_of_string()
        {
            var a = JsonDocument.Parse(@"[1,2,3]").RootElement;
            var b = JsonDocument.Parse(@"[1,2,""hi""]").RootElement;
            a.DeepEquals(b).Should().BeFalse();
            a.GetDeepHashCode().Should().NotBe(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Can_compare_object_with_properties()
        {
            var a = JsonDocument.Parse(@"{ ""Foo"": 1 }").RootElement;
            var b = JsonDocument.Parse(@"{ ""Foo"": 1 }").RootElement;
            a.DeepEquals(b).Should().BeTrue();
            a.GetDeepHashCode().Should().Be(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Can_compare_object_with_two_properties()
        {
            var a = JsonDocument.Parse(@"{ ""Foo"": 1, ""Bar"": 2 }").RootElement;
            var b = JsonDocument.Parse(@"{ ""Foo"": 1, ""Bar"": 2 }").RootElement;
            a.DeepEquals(b).Should().BeTrue();
            a.GetDeepHashCode().Should().Be(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Can_compare_object_with_two_properties_out_of_order()
        {
            var a = JsonDocument.Parse(@"{ ""Foo"": 1, ""Bar"": 2 }").RootElement;
            var b = JsonDocument.Parse(@"{ ""Bar"": 2, ""Foo"": 1 }").RootElement;
            a.DeepEquals(b).Should().BeTrue();
            a.GetDeepHashCode().Should().Be(b.GetDeepHashCode());
        }

        [TestMethod]
        public void Should_fail_comparing_two_objects_with_out_of_order_properties_that_are_not_equal()
        {
            var a = JsonDocument.Parse(@"{ ""Foo"": 1, ""Bar"": 2 }").RootElement;
            var b = JsonDocument.Parse(@"{ ""Bar"": 2, ""Foo"": 2 }").RootElement;
            a.DeepEquals(b).Should().BeFalse();
            a.GetDeepHashCode().Should().NotBe(b.GetDeepHashCode());
        }

    }

}
