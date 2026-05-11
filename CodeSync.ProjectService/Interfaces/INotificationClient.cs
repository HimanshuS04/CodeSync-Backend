namespace CodeSync.ProjectService.Interfaces
{
    public interface INotificationClient
    {
        Task CreateAsync(object payload);
        Task<HttpResponseMessage> GetAsync(string url);
    }
}