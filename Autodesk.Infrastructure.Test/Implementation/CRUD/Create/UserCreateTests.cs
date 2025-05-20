using Autodesk.Domain;
using Persistence.Repositories;

namespace Autodesk.Infrastructure.Test.Implementation.CRUD.Create
{
    public class UserCreateTests: TestsBase
    {
        [Fact]
        public async Task GivenNewUserWithUniqueEmail_WhenCreating_ThenReturnsSuccess()
        {
            //Arrange
            var newUser = new User(Guid.NewGuid().ToString())
            {
                Name = "Alice",
                Email = "alice@email.com",
                Lastname = "Robert"
            };

            //Act
            var result = await RepoCreate.CreateUserAsync(newUser);

            //Assert Operation result
            Assert.True(result.IsSuccessful);
        }
    }
}
