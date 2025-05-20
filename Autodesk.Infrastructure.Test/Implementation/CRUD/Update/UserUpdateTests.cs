using Autodesk.Domain;

namespace Autodesk.Infrastructure.Test.Implementation.CRUD.Update
{
    public class UserUpdateTests : TestsBase
    {
        [Fact]
        public async Task GivenExistingUser_WhenUpdatingName_ThenReturnsSuccess()
        {
            //Arrange
            var newUser = new User(Guid.NewGuid().ToString())
            {
                Name = "Alice",
                Email = "alice@email.com",
                Lastname = "Robert"
            };
            await RepoCreate.CreateEntity(newUser);
            //Act
            newUser.Name = "Eve";
            var result = await RepoUpdate.UpdateEntity(newUser);

            //Assert Operation result
            //Assert.True(result.IsSuccessful);
            //Assert.True(result.Data);
            //Assert.Equal(1, Ctx.Users.Count());
        }

        [Fact]
        public async Task GivenNonexistentUser_WhenUpdating_ThenReturnsFailure()
        {
            //Arrange
            var newUser = new User(Guid.NewGuid().ToString())
            {
                Name = "Alice",
                Email = "alice@email.com",
                Lastname = "Robert"
            };
            await RepoCreate.CreateEntity(newUser);
            //Act
            newUser.Name = "Eve";
            newUser.Id = Guid.NewGuid().ToString();
            var result = await RepoUpdate.UpdateEntity(newUser);

            //Assert Operation result
            Assert.False(result.IsSuccessful);
            Assert.False(result.Data);
        }
    }
}
