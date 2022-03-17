using System;
using Sample.Exceptions;
using Xunit;

namespace Sample.Tests
{
    public class GuardTests
    {
        [Fact]
        public void ThrowIfNullTest()
        {
            object testObject = new object();

            var sameObject = Guard.ThrowIfNull(testObject, nameof(testObject));

            Assert.Equal(testObject, sameObject);
        }

        [Fact]
        public void ThrowIfNullThrowsTest()
        {
            object testObject = null;

            Assert.Throws<ArgumentNullException>(() => Guard.ThrowIfNull(testObject, nameof(testObject)));
        }

        [Fact]
        public void ThrowIfNullOrEmptyTest()
        {
            string testString = "hello world";

            var sameString = Guard.ThrowIfNullOrEmpty(testString, nameof(testString));

            Assert.Equal(testString, sameString);
        }

        [Fact]
        public void ThrowIfNullOrEmptyThrowsTest()
        {
            string nullString = null;
            string emptyString = string.Empty;

            Assert.Throws<ArgumentNullException>(() => Guard.ThrowIfNullOrEmpty(nullString, nameof(nullString)));
            Assert.Throws<ArgumentNullException>(() => Guard.ThrowIfNullOrEmpty(emptyString, nameof(emptyString)));
        }
    }
}
