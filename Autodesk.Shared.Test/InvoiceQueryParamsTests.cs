using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autodesk.Shared.Models;
using Autodesk.Shared.Pagination;
using Xunit;

namespace Autodesk.Shared.Models.Tests
{
    public class InvoiceQueryParamsTests
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
        public void DefaultPageSize_IsPaginationDefault()
        {
            var qp = new InvoiceQueryParams();
            Assert.Equal(PaginationDefaults.DefaultPageSize, qp.PageSize);
        }

        [Fact]
        public void ValidQueryParams_PassesValidation()
        {
            var qp = new InvoiceQueryParams
            {
                InvoiceNumber   = "INV-1001",
                CustomerName    = "Acme Corporation",
                Cursor          = "abc123",
                PageSize        = 20,
                IncludeProducts = true
            };

            var results = Validate(qp);
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(51)]
        [InlineData(100)]
        public void InvoiceNumber_TooLong_FailsValidation(int length)
        {
            var qp = new InvoiceQueryParams
            {
                InvoiceNumber = new string('X', length)
            };

            var results = Validate(qp);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(InvoiceQueryParams.InvoiceNumber)) &&
                r.ErrorMessage!.Contains("cannot exceed 50 characters"));
        }

        [Theory]
        [InlineData(201)]
        [InlineData(500)]
        public void CustomerName_TooLong_FailsValidation(int length)
        {
            var qp = new InvoiceQueryParams
            {
                CustomerName = new string('Y', length)
            };

            var results = Validate(qp);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(InvoiceQueryParams.CustomerName)) &&
                r.ErrorMessage!.Contains("cannot exceed 200 characters"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(PaginationDefaults.MaxPageSize + 1)]
        [InlineData(1000)]
        public void PageSize_OutOfRange_FailsValidation(int pageSize)
        {
            var qp = new InvoiceQueryParams
            {
                PageSize = pageSize
            };

            var results = Validate(qp);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(InvoiceQueryParams.PageSize)) &&
                r.ErrorMessage!.Contains("PageSize must be between 1 and"));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(PaginationDefaults.MaxPageSize)]
        public void PageSize_AtBounds_PassesValidation(int pageSize)
        {
            var qp = new InvoiceQueryParams
            {
                PageSize = pageSize
            };

            var results = Validate(qp);
            Assert.Empty(results);
        }
    }
}
