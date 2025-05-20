using Autodesk.Domain;

namespace Autodesk.Infrastructure.Test.Implementation.CRUD.Delete
{
    public class UserDeleteTests : TestsBase
    {
        [Fact]
        public async Task GivenExistingUserId_WhenDeleting_ThenReturnsSuccess()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newUser = new User(id)
            {
                Name = "Alice",
                Email = "alice@email.com",
                Lastname = "Robert"
            };
            await RepoCreate.CreateEntity(newUser);
            //Act

            var result = await RepoDelete.DeleteEntity(id);
            //Assert Operation result
            //Assert.True(result.IsSuccessful);
            //Assert.True(result.Data);
        }

        [Fact]
        public async Task GivenNonexistentUserId_WhenDeleting_ThenReturnsFailure()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newUser = new User(id)
            {
                Name = "Alice",
                Email = "alice@email.com",
                Lastname = "Robert"
            };
            await RepoCreate.CreateEntity(newUser);
            //Act

            var result = await RepoDelete.DeleteEntity("NOT_ID");
            //Assert Operation result
            Assert.False(result.IsSuccessful);
            Assert.False(result.Data);
        }
    }
}
