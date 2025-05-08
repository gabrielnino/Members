namespace Autodesk.Application.UseCases.CRUD.User.Query
{
    public interface IUserQuery : IUserReadFilter, IUserReadFilterCount, IUserReadFilterPage, IUserReadById
    {
    }
}
