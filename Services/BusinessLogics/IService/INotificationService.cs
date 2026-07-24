using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface INotificationService
    {
        Task<GlobalRequestReponse<string>> ComposeNotificationAsync(string notificationType, string channel, Dictionary<string, string> placeholders);
    }
}
