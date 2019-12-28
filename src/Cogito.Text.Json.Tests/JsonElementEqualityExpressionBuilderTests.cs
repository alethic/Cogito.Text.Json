using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cogito.Text.Json.Tests
{

    [TestClass]
    public class JsonElementEqualityExpressionBuilderTests
    {

        static JsonDocument FromObject(object o)
        {
            return JsonDocument.Parse(JsonSerializer.Serialize(o));
        }


        [TestMethod]
        public void Should_evaluate_fairly_large_object_as_true()
        {
            using (var d = JsonDocument.Parse(File.ReadAllText(Path.Combine(Directory.GetParent(typeof(JsonElementEqualityExpressionBuilderTests).Assembly.Location).FullName, "efm.json"))))
            {
                var o = d.RootElement;

                var b = new JsonElementEqualityExpressionBuilder();
                var e = b.Build(o);
                var m = e.Compile();
                m.Invoke(o).Should().BeTrue();
            }
        }

        [TestMethod]
        public void Should_evaluate_fairly_large_object_as_false()
        {
            using (var d = JsonDocument.Parse(File.ReadAllText(Path.Combine(Directory.GetParent(typeof(JsonElementEqualityExpressionBuilderTests).Assembly.Location).FullName, "efm.json"))))
            {
                var o = d.RootElement;
                var b = new JsonElementEqualityExpressionBuilder();
                var e = b.Build(o);
                var m = e.Compile();

                var z = new MemoryStream();
                var s = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(Path.Combine(Directory.GetParent(typeof(JsonElementEqualityExpressionBuilderTests).Assembly.Location).FullName, "efm.json")));
                s["Next"] = 123;
                using (var w = new Utf8JsonWriter(z))
                    JsonSerializer.Serialize(w, s);
                var l = Encoding.UTF8.GetString(z.ToArray());

                using (var d2 = JsonDocument.Parse(l))
                {
                    var o2 = d2.RootElement;
                    m.Invoke(o2).Should().BeFalse();
                }
            }
        }

        [TestMethod]
        public void Should_evaluate_simple_object_as_false()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(new { Foo = "Bar" }));
            var m = e.Compile();
            m.Invoke(FromObject(new { Foo = "Foo" }).RootElement).Should().BeFalse();
        }

        [TestMethod]
        public void Should_evaluate_simple_object_as_true()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(new { Foo = "Bar" }));
            var m = e.Compile();
            m.Invoke(FromObject(new { Foo = "Bar" }).RootElement).Should().BeTrue();
        }

        [TestMethod]
        public void Should_evaluate_object_with_two_properties_as_true()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(new { Foo = "Bar", Joe = 123 }));
            var m = e.Compile();
            m.Invoke(FromObject(new { Foo = "Bar", Joe = 123 }).RootElement).Should().BeTrue();
        }

        [TestMethod]
        public void Should_evaluate_missing_property_as_false()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(new { Foo = "Bar" }));
            var m = e.Compile();
            m.Invoke(FromObject(new { Asd = "Bar" }).RootElement).Should().BeFalse();
        }

        [TestMethod]
        public void Should_evaluate_extra_property_as_false()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(new { Foo = "Bar" }));
            var m = e.Compile();
            m.Invoke(FromObject(new { Foo = "Bar", Joe = 1 }).RootElement).Should().BeFalse();
        }

        [TestMethod]
        public void Should_compare_booleans()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(true));
            var m = e.Compile();
            m.Invoke(FromObject(true).RootElement).Should().BeTrue();
        }

        [TestMethod]
        public void Should_compare_booleans_as_false()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(true));
            var m = e.Compile();
            m.Invoke(FromObject(false).RootElement).Should().BeFalse();
        }

        [TestMethod]
        public void Should_compare_strings()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject("TEST"));
            var m = e.Compile();
            m.Invoke(FromObject("TEST").RootElement).Should().BeTrue();
        }

        [TestMethod]
        public void Should_compare_strings_as_false()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject("TEST"));
            var m = e.Compile();
            m.Invoke(FromObject("TEST2").RootElement).Should().BeFalse();
        }

        [TestMethod]
        public void Should_compare_empty_arrays()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(new object[0]));
            var m = e.Compile();
            m.Invoke(FromObject(new object[0]).RootElement).Should().BeTrue();
        }

        [TestMethod]
        public void Should_compare_arrays_with_one_value()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(new[] { true }));
            var m = e.Compile();
            m.Invoke(FromObject(new[] { true }).RootElement).Should().BeTrue();
        }

        [TestMethod]
        public void Should_compare_arrays_with_one_unequal_value()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(new[] { true }));
            var m = e.Compile();
            m.Invoke(FromObject(new[] { false }).RootElement).Should().BeFalse();
        }

        [TestMethod]
        public void Should_compare_arrays_with_unequal_size()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(new[] { true }));
            var m = e.Compile();
            m.Invoke(FromObject(new[] { true, true }).RootElement).Should().BeFalse();
        }

        [TestMethod]
        public void Can_compare_against_null()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(new { foo = 12 }));
            var m = e.Compile();
            m.Invoke(FromObject(null).RootElement).Should().BeFalse();
        }

        [TestMethod]
        public void Can_compare_against_badly_typed_properties()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(new { foo = 12 }));
            var m = e.Compile();
            m.Invoke(FromObject(new { foo = false }).RootElement).Should().BeFalse();
        }

        [TestMethod]
        public void Should_compare_integers_as_equal_if_difference_is_decimal_only_ltr()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(0));
            var m = e.Compile();
            m.Invoke(FromObject(0.0).RootElement).Should().BeTrue();
        }

        [TestMethod]
        public void Should_compare_integers_as_equal_if_difference_is_decimal_only()
        {
            var b = new JsonElementEqualityExpressionBuilder();
            var e = b.Build(FromObject(0.0));
            var m = e.Compile();
            m.Invoke(FromObject(0).RootElement).Should().BeTrue();
        }

    }

}
