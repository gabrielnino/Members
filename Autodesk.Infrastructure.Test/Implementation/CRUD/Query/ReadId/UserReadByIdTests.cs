using Application.Result;
using Autodesk.Domain;

namespace Autodesk.Infrastructure.Test.Implementation.CRUD.Query.ReadId
{
    public class UserReadByIdTests : TestsBase
    {
        [Fact]
        public async Task GivenExistingUserId_WhenReadingById_ThenReturnsSuccessAndCorrectUser()
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

            var result = await RepoReadById.ReadById(id);
            //Assert Operation result
            Assert.True(result.IsSuccessful);
            Assert.Equal(id, result.Data.Id);
        }

        [Fact]
        public async Task GivenNonexistentUserId_WhenReadingById_ThenReturnsFailureAndNullData()
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

            var result = await RepoReadById.ReadById("Not_found");
            //Assert Operation result
            Assert.False(result.IsSuccessful);
            Assert.Null(result.Data);
        }
    }
}
