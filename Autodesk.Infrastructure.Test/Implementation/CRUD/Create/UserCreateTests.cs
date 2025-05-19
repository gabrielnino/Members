using Autodesk.Domain;

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
            var result = await RepoCreate.CreateEntity(newUser);

            //Assert Operation result
            Assert.True(result.IsSuccessful);
            Assert.Equal(1, Ctx.Users.Count());
        }

        [Fact]
        public async Task GivenExistingUserWithSameEmail_WhenCreating_ThenReturnsFailure()
        {
            // Arrange

            // Seed an existing user with the target email
            var existingUser = new User(Guid.NewGuid().ToString())
            {
                Name = "Bob",
                Email = "bob@example.com",
                Lastname = "Smith"
            };
            Ctx.Users.Add(existingUser);
            await Ctx.SaveChangesAsync();


            // Create another user with the same email
            var newUser = new User(Guid.NewGuid().ToString())
            {
                Name = "Robert",
                Email = "bob@example.com",
                Lastname = "Johnson"
            };

            // Act
            var result = await RepoCreate.CreateEntity(newUser);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.False(string.IsNullOrWhiteSpace(result.Message));

            // Ensure only the original user remains
            Assert.Equal(1, Ctx.Users.Count());
        }
    }
}
