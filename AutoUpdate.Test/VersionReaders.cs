using AutoUpdate.Provider;
using System;
using Xunit;


namespace AutoUpdate.Test
{
    public class VersionReaders
    {

        [Theory]
        [InlineData("2")]
        [InlineData("2.0")]
        [InlineData("2.0-beta")]
        [InlineData("2.0-beta.1")]
        [InlineData("2.0.0")]
        [InlineData("2...1")]
        [InlineData("  \r\n  1.2.3.4")]
        [InlineData("  \r\n  1")]
        [InlineData("  \r\n  2...1")]
        [InlineData("  \r\n  1\t. 2.")]
        public void StringVersionReaderTest(string ver)
        {
            var reader = new StringToVersionReader();
            var version = reader.GetVersion(ver);
        }



    }
}
