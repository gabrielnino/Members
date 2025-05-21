using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autodesk.Domain;
using Domain;
using Xunit;

namespace Autodesk.Domain.Tests
{
    public class ProductTests
    {
        /// <summary>
        /// Runs all DataAnnotation validations on the given model.
        /// </summary>
        private static IList<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Constructor_SetsId()
        {
            var id = Guid.NewGuid().ToString();
            var product = new Product(id);

            Assert.Equal(id, product.Id);
        }

        [Fact]
        public void ValidProduct_PassesValidation()
        {
            var product = new Product(Guid.NewGuid().ToString())
            {
                Name        = "Widget",
                Description = "A useful widget.",
                Price       = 9.99m
            };

            var results = Validate(product);
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(null, "Product name is required.")]
        [InlineData("", "Product name is required.")]
        public void MissingName_FailsRequired(string? name, string expectedMessage)
        {
            var product = new Product(Guid.NewGuid().ToString())
            {
                Name        = name,
                Description = "Desc",
                Price       = 1.00m
            };

            var results = Validate(product);
            Assert.Contains(results, r => r.ErrorMessage == expectedMessage);
        }

        [Theory]
        [InlineData("A", "Product name must be at least 2 characters long.")]
        [InlineData("B", "Product name must be at least 2 characters long.")]
        public void Name_TooShort_FailsMinLength(string name, string expectedMessage)
        {
            var product = new Product(Guid.NewGuid().ToString())
            {
                Name        = name,
                Description = "Desc",
                Price       = 1.00m
            };

            var results = Validate(product);
            Assert.Contains(results, r => r.ErrorMessage == expectedMessage);
        }

        [Fact]
        public void Name_TooLong_FailsMaxLength()
        {
            var longName = new string('X', 101);
            var product = new Product(Guid.NewGuid().ToString())
            {
                Name        = longName,
                Description = "Desc",
                Price       = 1.00m
            };

            var results = Validate(product);
            Assert.Contains(results, r =>
                r.ErrorMessage == "Product name must be maximum 100 characters long.");
        }

        [Fact]
        public void Description_TooLong_FailsMaxLength()
        {
            var longDesc = new string('D', 501);
            var product = new Product(Guid.NewGuid().ToString())
            {
                Name        = "Valid Name",
                Description = longDesc,
                Price       = 1.00m
            };

            var results = Validate(product);
            Assert.Contains(results, r =>
                r.ErrorMessage == "Description cannot exceed 500 characters.");
        }

        [Theory]
        [InlineData(0.00)]
        [InlineData(0.009)]
        [InlineData(-5.00)]
        public void Price_LessThanMin_FailsRange(decimal price)
        {
            var product = new Product(Guid.NewGuid().ToString())
            {
                Name        = "Valid",
                Description = "Desc",
                Price       = price
            };

            var results = Validate(product);
            Assert.Contains(results, r =>
                r.ErrorMessage == "Price must be greater than zero.");
        }

        [Fact]
        public void Price_ExactlyMin_PassesValidation()
        {
            var product = new Product(Guid.NewGuid().ToString())
            {
                Name        = "Valid",
                Description = "Desc",
                Price       = 0.01m
            };

            var results = Validate(product);
            Assert.Empty(results);
        }
    }
}
