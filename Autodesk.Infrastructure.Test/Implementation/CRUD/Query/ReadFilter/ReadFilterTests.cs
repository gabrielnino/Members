using Autodesk.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.Infrastructure.Test.Implementation.CRUD.Query.ReadFilter
{
    public class ReadFilterTests : TestsBase
    {
        [Fact]
        public async Task GivenUsersExistMatchingFilter_WhenReadingFilteredPage_ThenReturnsSingleResult()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newUser = new User(id)
            {
                Name = "Alice",
                Email = "alice@email.com",
                Lastname = "Robert"
            };
            await RepoCreate.Create(newUser);
            //Act

            var result = await ReadFilterPage.ReadFilterPage(0, 1, "Alice");
            //Assert Operation result
            Assert.True(result.IsSuccessful);
            Assert.Equal(1, result.Data.ToList().Count());
        }
    }
}
