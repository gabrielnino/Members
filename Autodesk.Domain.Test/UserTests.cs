using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autodesk.Domain;
using Xunit;

namespace Autodesk.Domain.Tests
{
    public class UserTests
    {
        /// <summary>
        /// Runs DataAnnotation validation on the model.
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
            var user = new User(id);

            Assert.Equal(id, user.Id);
        }

        [Fact]
        public void ValidUser_PassesValidation()
        {
            var user = new User(Guid.NewGuid().ToString())
            {
                Name      = "Alice",
                Lastname  = "Johnson",
                Email     = "alice.johnson@example.com"
            };

            var results = Validate(user);
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingName_FailsRequired(string? name)
        {
            var user = new User(Guid.NewGuid().ToString())
            {
                Name      = name,
                Lastname  = "Doe",
                Email     = "jane.doe@example.com"
            };

            var results = Validate(user);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(User.Name)) &&
                r.ErrorMessage == "Name is required.");
        }

        [Theory]
        [InlineData("Al")]
        public void Name_TooShort_FailsMinLength(string name)
        {
            var user = new User(Guid.NewGuid().ToString())
            {
                Name      = name,
                Lastname  = "Doe",
                Email     = "jane.doe@example.com"
            };

            var results = Validate(user);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(User.Name)) &&
                r.ErrorMessage == "Name must be at least 3 characters long.");
        }

        [Fact]
        public void Name_TooLong_FailsMaxLength()
        {
            var longName = new string('N', 101);
            var user = new User(Guid.NewGuid().ToString())
            {
                Name      = longName,
                Lastname  = "Doe",
                Email     = "jane.doe@example.com"
            };

            var results = Validate(user);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(User.Name)) &&
                r.ErrorMessage == "Name must be maximun 100 characters long.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingLastname_FailsRequired(string? lastname)
        {
            var user = new User(Guid.NewGuid().ToString())
            {
                Name      = "Jane",
                Lastname  = lastname,
                Email     = "jane.doe@example.com"
            };

            var results = Validate(user);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(User.Lastname)) &&
                r.ErrorMessage == "Lastname is required.");
        }

        [Theory]
        [InlineData("Do")]
        public void Lastname_TooShort_FailsMinLength(string lastname)
        {
            var user = new User(Guid.NewGuid().ToString())
            {
                Name      = "Jane",
                Lastname  = lastname,
                Email     = "jane.doe@example.com"
            };

            var results = Validate(user);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(User.Lastname)) &&
                r.ErrorMessage == "Lastname must be at least 3 characters long.");
        }

        [Fact]
        public void Lastname_TooLong_FailsMaxLength()
        {
            var longLastname = new string('L', 101);
            var user = new User(Guid.NewGuid().ToString())
            {
                Name      = "Jane",
                Lastname  = longLastname,
                Email     = "jane.doe@example.com"
            };

            var results = Validate(user);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(User.Lastname)) &&
                r.ErrorMessage == "Lastname must be maximun 100 characters long.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingEmail_FailsRequired(string? email)
        {
            var user = new User(Guid.NewGuid().ToString())
            {
                Name      = "Jane",
                Lastname  = "Doe",
                Email     = email
            };

            var results = Validate(user);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(User.Email)) &&
                r.ErrorMessage == "Email is required.");
        }

        [Theory]
        [InlineData("a@b")]
        [InlineData("user@")]
        [InlineData("user@@domain.com")]
        public void InvalidEmail_FailsRegex(string email)
        {
            var user = new User(Guid.NewGuid().ToString())
            {
                Name      = "Jane",
                Lastname  = "Doe",
                Email     = email
            };

            var results = Validate(user);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(User.Email)) &&
                r.ErrorMessage == "Email must be a valid email address.");
        }

        [Fact]
        public void Email_TooShort_FailsMinLength()
        {
            var user = new User(Guid.NewGuid().ToString())
            {
                Name      = "Jane",
                Lastname  = "Doe",
                Email     = "a@"
            };

            var results = Validate(user);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(User.Email)) &&
                r.ErrorMessage == "Email must be at least 3 characters long.");
        }

        [Fact]
        public void Email_TooLong_FailsMaxLength()
        {
            var localPart = new string('x', 95);
            var email = $"{localPart}@d.com"; // total 100+ chars
            var user = new User(Guid.NewGuid().ToString())
            {
                Name      = "Jane",
                Lastname  = "Doe",
                Email     = email
            };

            var results = Validate(user);
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(User.Email)) &&
                r.ErrorMessage == "Email must be maximun 100 characters long.");
        }

        [Fact]
        public void ValidEmailAtBounds_PassesValidation()
        {
            var localPart = new string('a', 94);
            var email = $"{localPart}@x.com"; // exactly 100 chars
            var user = new User(Guid.NewGuid().ToString())
            {
                Name      = "Jane",
                Lastname  = "Doe",
                Email     = email
            };

            var results = Validate(user);
            Assert.Empty(results);
        }
    }
}
