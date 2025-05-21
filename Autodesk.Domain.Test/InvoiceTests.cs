using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autodesk.Domain;
using Domain;
using Xunit;

namespace Autodesk.Domain.Tests
{
    public class InvoiceTests
    {
        /// <summary>
        /// Validates all DataAnnotation rules on the given model.
        /// </summary>
        private static IList<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Constructor_SetsIdAndDefaultProducts()
        {
            var id = Guid.NewGuid().ToString();
            var invoice = new Invoice(id);

            Assert.Equal(id, invoice.Id);
            Assert.NotNull(invoice.Products);
            Assert.Empty(invoice.Products);
        }

        [Fact]
        public void ValidInvoice_PassesValidation()
        {
            var invoice = new Invoice(Guid.NewGuid().ToString())
            {
                InvoiceNumber  = "INV-2025-0001",
                InvoiceDate    = DateTime.UtcNow,
                CustomerName   = "Contoso Ltd.",
                TotalAmount    = 123.45m
            };

            var results = Validate(invoice);
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(null, "Invoice number is required.")]
        [InlineData("", "Invoice number is required.")]
        public void MissingInvoiceNumber_FailsRequired(string? invoiceNumber, string expectedMessage)
        {
            var invoice = new Invoice(Guid.NewGuid().ToString())
            {
                InvoiceNumber = invoiceNumber,
                InvoiceDate   = DateTime.UtcNow,
                CustomerName  = "Foo",
                TotalAmount   = 0m
            };

            var results = Validate(invoice);
            Assert.Contains(results, r => r.ErrorMessage == expectedMessage);
        }

        [Theory]
        [InlineData("X", "Customer name must be at least 3 characters long.")]
        [InlineData(null, "Customer name is required.")]
        [InlineData("", "Customer name is required.")]
        public void InvalidCustomerName_FailsLengthOrRequired(string? name, string expectedMessage)
        {
            var invoice = new Invoice(Guid.NewGuid().ToString())
            {
                InvoiceNumber = "INV1",
                InvoiceDate   = DateTime.UtcNow,
                CustomerName  = name,
                TotalAmount   = 0m
            };

            var results = Validate(invoice);
            Assert.Contains(results, r => r.ErrorMessage == expectedMessage);
        }

        [Fact]
        public void CustomerName_TooLong_FailsMaxLength()
        {
            var longName = new string('C', 201);
            var invoice = new Invoice(Guid.NewGuid().ToString())
            {
                InvoiceNumber = "INV2",
                InvoiceDate   = DateTime.UtcNow,
                CustomerName  = longName,
                TotalAmount   = 0m
            };

            var results = Validate(invoice);
            Assert.Contains(results, r =>
                r.ErrorMessage == "Customer name must be maximum 200 characters long.");
        }

        [Fact]
        public void InvoiceNumber_TooLong_FailsStringLength()
        {
            var longInvoiceNumber = new string('I', 51);
            var invoice = new Invoice(Guid.NewGuid().ToString())
            {
                InvoiceNumber = longInvoiceNumber,
                InvoiceDate   = DateTime.UtcNow,
                CustomerName  = "Valid Name",
                TotalAmount   = 0m
            };

            var results = Validate(invoice);
            Assert.Contains(results, r =>
                r.ErrorMessage == "Invoice number cannot exceed 50 characters.");
        }

        [Fact]
        public void NegativeTotalAmount_FailsRange()
        {
            var invoice = new Invoice(Guid.NewGuid().ToString())
            {
                InvoiceNumber = "INV-NEG",
                InvoiceDate   = DateTime.UtcNow,
                CustomerName  = "Valid",
                TotalAmount   = -1m
            };

            var results = Validate(invoice);
            Assert.Contains(results, r =>
                r.ErrorMessage == "Total amount must be non-negative.");
        }
    }
}
