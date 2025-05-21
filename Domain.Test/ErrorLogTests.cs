using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain;
using Xunit;

namespace Domain.Tests
{
    public class ErrorLogTests
    {
        /// <summary>
        /// Helper to run all DataAnnotation validations on an object.
        /// </summary>
        private static IList<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Constructor_SetsTimestampToNow()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var log = new ErrorLog(Guid.NewGuid().ToString())
            {
                Level          = "Error",
                Message        = "Something went wrong",
                ExceptionType  = "System.Exception",
                StackTrace     = "at Foo.Bar()",
                Context        = "{}"
            };

            var after = DateTime.UtcNow;

            // Assert: Timestamp is between before and after
            Assert.InRange(log.Timestamp, before, after);
        }

        [Fact]
        public void ValidErrorLog_PassesValidation()
        {
            var log = new ErrorLog(Guid.NewGuid().ToString())
            {
                Level          = "Warning",
                Message        = "A minor issue",
                ExceptionType  = "System.InvalidOperationException",
                StackTrace     = "at Test.Main()",
                Context        = "{\"key\":\"value\"}"
            };

            var results = Validate(log);
            Assert.Empty(results);
        }

        [Theory]
        [InlineData("", "Level is required.")]   // Required fails
        [InlineData("Err", "Level must be at least 3 characters long.")] // MinLength fails
        public void InvalidLevel_FailsValidation(string invalidLevel, string expectedMessage)
        {
            var log = new ErrorLog(Guid.NewGuid().ToString())
            {
                Level          = invalidLevel,
                Message        = "Valid message",
                ExceptionType  = "System.Exception",
                StackTrace     = "stack",
                Context        = "ctx"
            };

            var results = Validate(log);
            Assert.Contains(results, r => r.ErrorMessage == expectedMessage);
        }

        [Theory]
        [InlineData("", "Message is required.")]
        [InlineData("Msg", "Message must be at least 3 characters long.")]
        public void InvalidMessage_FailsValidation(string invalidMessage, string expectedMessage)
        {
            var log = new ErrorLog(Guid.NewGuid().ToString())
            {
                Level          = "Error",
                Message        = invalidMessage,
                ExceptionType  = "System.Exception",
                StackTrace     = "stack",
                Context        = "ctx"
            };

            var results = Validate(log);
            Assert.Contains(results, r => r.ErrorMessage == expectedMessage);
        }

        [Fact]
        public void TooLongFields_FailMaxLengthValidation()
        {
            var longText = new string('x', 151);
            var log = new ErrorLog(Guid.NewGuid().ToString())
            {
                Level          = longText,
                Message        = longText,
                ExceptionType  = longText,
                StackTrace     = longText,
                Context        = longText
            };

            var results = Validate(log);

            // All of these properties have a MaxLength of 150
            Assert.Equal(5, results.Count);
            Assert.All(results, r =>
                Assert.Contains("be maximun 150 characters long", r.ErrorMessage));
        }
    }
}
