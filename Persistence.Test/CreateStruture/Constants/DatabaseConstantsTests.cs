using Database = Persistence.CreateStructure.Constants.Database;
namespace Persistence.Test.CreateStructure.Constants
{
    public class DatabaseConstantsTests
    {
        [Fact]
        public void Tables_Users_ShouldBeUsers()
        {
            Assert.Equal("Users", Database.Tables.Users);
        }

        [Fact]
        public void Index_IndexEmail_ShouldBeUniqueUsersEmail()
        {
            Assert.Equal("UC_Users_Email", Database.Index.IndexEmail);
        }
    }
}
