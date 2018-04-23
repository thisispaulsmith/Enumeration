using FluentAssertions;
using NetSmith.Enumeration;
using System;
using Xunit;

namespace UnitTests
{
    public class EnumerationTests
    {
        [Fact]
        public void Enumeration_value_should_be_correct()
        {
            TestEnumeration.Test1.Value.Should().Be(0);
            TestEnumeration.Test2.Value.Should().Be(1);
            TestEnumeration.Test3.Value.Should().Be(2);
        }

        [Fact]
        public void Enumeration_text_should_be_correct()
        {
            TestEnumeration.Test1.DisplayName.Should().Be("Test1");
            TestEnumeration.Test2.DisplayName.Should().Be("Test2");
            TestEnumeration.Test3.DisplayName.Should().Be("Test3");
        }

        [Fact]
        public void Enumeration_parse_success()
        {
            byte value = 0;

            var success = Enumeration.TryParse<TestEnumeration>(value, out var result);

            success.Should().Be(true);
            result.Should().Be(TestEnumeration.Test1);
        }

        [Fact]
        public void Enumeration_parse_fail()
        {
            byte value = 10;

            var success = Enumeration.TryParse<TestEnumeration>(value, out var result);

            success.Should().Be(false);
            result.Should().Be(null);
        }
    }
}
