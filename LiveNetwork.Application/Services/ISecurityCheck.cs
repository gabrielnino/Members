namespace LiveNetwork.Application.Services
{
    public interface ISecurityCheck
    {
        bool IsSecurityCheck();
        Task TryStartPuzzle();
        Task HandleSecurityPage();
        Task HandleUnexpectedPage();
    }
}
