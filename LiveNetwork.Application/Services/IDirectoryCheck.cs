namespace LiveNetwork.Application.Services
{
    public interface IDirectoryCheck
    {
        void EnsureDirectoryExists(string path);
    }
}
