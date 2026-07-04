using DataService.Common;
using Xunit;

namespace Aelbry.Tests
{
    public class ValidateTests
    {
        [Fact]
        public void GetDefaultIntIfDBNull_ReturnsZero_WhenDBNull()
        {
            Assert.Equal(0, Validate.getDefaultIntIfDBNull(DBNull.Value));
        }

        [Fact]
        public void GetDefaultIntIfDBNull_ReturnsValue_WhenNotNull()
        {
            Assert.Equal(42, Validate.getDefaultIntIfDBNull(42));
        }

        [Fact]
        public void GetDefaultStringIfDBNull_ReturnsEmpty_WhenDBNull()
        {
            Assert.Equal(string.Empty, Validate.getDefaultStringIfDBNull(DBNull.Value));
        }

        [Fact]
        public void GetDefaultDateIfDBNull_ReturnsMinValue_WhenDBNull()
        {
            Assert.Equal(DateTime.MinValue, Validate.getDefaultDateIfDBNull(DBNull.Value));
        }

        [Fact]
        public void GetDefaultBoolIfDBNull_ReturnsFalse_WhenDBNull()
        {
            Assert.False(Validate.getDefaultBoolIfDBNull(DBNull.Value));
        }

        [Fact]
        public void GetDefaultBoolIfDBNull_ReturnsTrue_WhenValueIsOne()
        {
            Assert.True(Validate.getDefaultBoolIfDBNull(1));
        }

        [Fact]
        public void GetDefaultIfDBNull_ReturnsTypedDefault_WhenDBNull()
        {
            Assert.Equal(string.Empty, Validate.getDefaultIfDBNull(DBNull.Value, TypeCode.String));
            Assert.Equal(0, Validate.getDefaultIfDBNull(DBNull.Value, TypeCode.Int32));
        }

        [Fact]
        public void GetDefaultIfDBNull_ReturnsOriginalValue_WhenNotNull()
        {
            Assert.Equal("OK", Validate.getDefaultIfDBNull("OK", TypeCode.String));
        }
    }
}
