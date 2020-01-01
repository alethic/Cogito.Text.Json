using System;
using System.Text.Json;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cogito.Text.Json.Tests
{

    [TestClass]
    public class JsonPointerTests
    {

        [TestMethod]
        public void Should_execute_spec_tests()
        {
            var d = JsonDocument.Parse(@"
{
      ""foo"": [""bar"", ""baz""],
      """": 0,
      ""a/b"": 1,
      ""c%d"": 2,
      ""e^f"": 3,
      ""g|h"": 4,
      ""i\\j"": 5,
      ""k\""l"": 6,
      "" "": 7,
      ""m~n"": 8
}");
            d.RootElement.SelectByPointer("").Value.Should().Be(d.RootElement);
            d.RootElement.SelectByPointer("/foo").Value.Should().Be(d.RootElement.GetProperty("foo"));
            d.RootElement.SelectByPointer("/foo/0").Value.GetString().Should().Be("bar");
            d.RootElement.SelectByPointer("/").Value.GetInt32().Should().Be(0);
            d.RootElement.SelectByPointer("/a~1b").Value.GetInt32().Should().Be(1);
            d.RootElement.SelectByPointer("/c%d").Value.GetInt32().Should().Be(2);
            d.RootElement.SelectByPointer("/e^f").Value.GetInt32().Should().Be(3);
            d.RootElement.SelectByPointer("/g|h").Value.GetInt32().Should().Be(4);
            d.RootElement.SelectByPointer("/i\\j").Value.GetInt32().Should().Be(5);
            d.RootElement.SelectByPointer("/k\"l").Value.GetInt32().Should().Be(6);
            d.RootElement.SelectByPointer("/ ").Value.GetInt32().Should().Be(7);
            d.RootElement.SelectByPointer("/m~0n").Value.GetInt32().Should().Be(8);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Should_throw_on_length_of_bad_segment()
        {
            var p = new JsonPointer("");
            p.GetSegmentLength(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Should_throw_on_length_of_bad_second_segment()
        {
            var p = new JsonPointer("/");
            p.GetSegmentLength(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_throw_on_length_of_non_slash_character()
        {
            var p = new JsonPointer("/a");
            p.GetSegmentLength(1);
        }

        [TestMethod]
        public void Should_return_length_of_empty_segment_as_1()
        {
            var p = new JsonPointer("/");
            p.GetSegmentLength(0).Should().Be(1);
        }

        [TestMethod]
        public void Should_return_length_of_small_segment_as_2()
        {
            var p = new JsonPointer("/a");
            p.GetSegmentLength(0).Should().Be(2);
        }

        [TestMethod]
        public void Should_return_length_of_small_segment_with_following_empty_as_2()
        {
            var p = new JsonPointer("/a/");
            p.GetSegmentLength(0).Should().Be(2);
        }

        [TestMethod]
        public void Should_return_length_of_second_empty_segment_as_1()
        {
            var p = new JsonPointer("//");
            p.GetSegmentLength(0).Should().Be(1);
            p.GetSegmentLength(1).Should().Be(1);
        }

        [TestMethod]
        public void Should_retrieve_single_segment_for_whole_document()
        {
            var z = new JsonPointer("").ToList();
            z.Should().HaveCount(0);
        }

        [TestMethod]
        public void Should_retrieve_slash_for_empty_first_segment()
        {
            var z = new JsonPointer("/").ToList();
            z.Should().HaveCount(1);
            z[0].ToString().Should().Be("/");
        }

        [TestMethod]
        public void Should_retrieve_slash_for_empty_second_segment()
        {
            var z = new JsonPointer("//").ToList();
            z.Should().HaveCount(2);
            z[1].ToString().Should().Be("/");
        }

        [TestMethod]
        public void Should_retrieve_string_for_segment_followed_by_empty_segment()
        {
            var z = new JsonPointer("/a/").ToList();
            z.Should().HaveCount(2);
            z[0].ToString().Should().Be("/a");
            z[1].ToString().Should().Be("/");
            z.Invoking(i => i[2]).Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void Should_retrieve_string_for_longer_first_segment()
        {
            var z = new JsonPointer("/aLKJFLKSJDFLKSJDLFKJSDLKFJ").ToList();
            z.Should().HaveCount(1);
            z[0].ToString().Should().Be("/aLKJFLKSJDFLKSJDLFKJSDLKFJ");
            z.Invoking(i => i[2]).Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void Should_return_root()
        {
            var d = JsonDocument.Parse(@"{ ""prop"": ""val"" }");
            var e = d.RootElement.SelectByPointer("");
            e.Should().Be(d.RootElement);
        }

        [TestMethod]
        public void Should_return_property()
        {
            var d = JsonDocument.Parse(@"{ ""prop"": ""val"" }");
            var e = d.RootElement.SelectByPointer("/prop");
            e.Should().Be(d.RootElement.GetProperty("prop"));
        }

        [TestMethod]
        public void Should_return_nested_property()
        {
            var d = JsonDocument.Parse(@"{ ""prop"": { ""prop2"": ""val"" } }");
            var e = d.RootElement.SelectByPointer("/prop/prop2");
            e.Should().Be(d.RootElement.GetProperty("prop").GetProperty("prop2"));
        }

        [TestMethod]
        public void Should_return_null_for_missing_property()
        {
            var d = JsonDocument.Parse(@"{ ""prop"": ""val"" }");
            var e = d.RootElement.SelectByPointer("/prop2");
            e.Should().BeNull();
        }

        [TestMethod]
        public void Should_return_null_for_nested_missing_property()
        {
            var d = JsonDocument.Parse(@"{ ""prop"": { ""prop2"": ""val"" } }");
            var e = d.RootElement.SelectByPointer("/prop/prop3");
            e.Should().BeNull();
        }

        [TestMethod]
        public void Should_return_null_for_nested_property_inside_missing()
        {
            var d = JsonDocument.Parse(@"{ ""prop"": { ""prop2"": ""val"" } }");
            var e = d.RootElement.SelectByPointer("/prop3/prop2");
            e.Should().BeNull();
        }

        [TestMethod]
        public void Should_return_item()
        {
            var d = JsonDocument.Parse(@"[ ""val"" ]");
            var e = d.RootElement.SelectByPointer("/0");
            e.Should().Be(d.RootElement[0]);
        }

        [TestMethod]
        public void Should_return_property_inside_array()
        {
            var d = JsonDocument.Parse(@"[ { ""prop"": ""val"" } ]");
            var e = d.RootElement.SelectByPointer("/0/prop");
            e.Should().Be(d.RootElement[0].GetProperty("prop"));
        }

        [TestMethod]
        public void Should_return_array_inside_property()
        {
            var d = JsonDocument.Parse(@"{ ""prop"": [ ""val"" ] }");
            var e = d.RootElement.SelectByPointer("/prop/0");
            e.Should().Be(d.RootElement.GetProperty("prop")[0]);
        }

        [TestMethod]
        public void Should_return_array_null_againt_object()
        {
            var d = JsonDocument.Parse(@"{ ""prop"": [ ""val"" ] }");
            var e = d.RootElement.SelectByPointer("/0");
            e.Should().BeNull();
        }

        [TestMethod]
        public void Should_return_array_null_againt_string()
        {
            var d = JsonDocument.Parse(@"""val""");
            var e = d.RootElement.SelectByPointer("/0");
            e.Should().BeNull();
        }

        [TestMethod]
        public void Should_get_first_segment()
        {
            new JsonPointer("/").FirstSegment.ToString().Should().Be("/");
        }

        [TestMethod]
        public void Should_get_next_segment()
        {
            new JsonPointer("/a/b").FirstSegment.Next.ToString().Should().Be("/b");
        }

        [TestMethod]
        public void Should_return_null_if_no_first()
        {
            (new JsonPointer("").FirstSegment == JsonPointerSegment.Null).Should().BeTrue();
        }

        [TestMethod]
        public void Should_return_count_for_root()
        {
            new JsonPointer("").Count.Should().Be(0);
        }

        [TestMethod]
        public void Should_return_count_for_empty_value()
        {
            new JsonPointer("/").Count.Should().Be(1);
        }

        [TestMethod]
        public void Should_return_count_for_three()
        {
            new JsonPointer("/asd/asd/asd").Count.Should().Be(3);
        }

    }

}
