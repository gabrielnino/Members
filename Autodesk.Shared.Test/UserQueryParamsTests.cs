using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autodesk.Shared.Models;
using Autodesk.Shared.Pagination;
using Xunit;

namespace Autodesk.Shared.Models.Tests
{
    public class UserQueryParamsTests
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
        public void DefaultPageSize_IsPaginationDefault()
        {
            var qp = new UserQueryParams();
            Assert.Equal(PaginationDefaults.DefaultPageSize, qp.PageSize);
        }

        [Fact]
        public void ValidQueryParams_PassesValidation()
        {
            var qp = new UserQueryParams
            {
                Id       = Guid.NewGuid().ToString(),
                Name     = "Alice Smith",
                PageSize = 10,
                Cursor   = "cursor123"
            };

            var results = Validate(qp);
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(101)]
        [InlineData(200)]
        public void Name_TooLong_FailsValidation(int length)
        {
            var qp = new UserQueryParams
            {
                Name = new string('Z', length)
            };

            var results = Validate(qp);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(UserQueryParams.Name)) &&
                r.ErrorMessage!.Contains("cannot exceed 100 characters"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(PaginationDefaults.MaxPageSize + 1)]
        [InlineData(1000)]
        public void PageSize_OutOfRange_FailsValidation(int pageSize)
        {
            var qp = new UserQueryParams
            {
                PageSize = pageSize
            };

            var results = Validate(qp);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(UserQueryParams.PageSize)) &&
                r.ErrorMessage!.Contains("PageSize must be between 1 and"));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(PaginationDefaults.MaxPageSize)]
        public void PageSize_AtBounds_PassesValidation(int pageSize)
        {
            var qp = new UserQueryParams
            {
                PageSize = pageSize
            };

            var results = Validate(qp);
            Assert.Empty(results);
        }
    }
}
