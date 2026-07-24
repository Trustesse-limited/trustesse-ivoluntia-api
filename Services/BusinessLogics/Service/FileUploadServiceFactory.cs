using Microsoft.Extensions.DependencyInjection;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class FileUploadServiceFactory : IFileUploadServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public FileUploadServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IFileUploadService GetFileUploadService()
        {
            var provider = FileUploadServiceProvider.Cloudinary.ToString();

            return provider switch
            {
                "Cloudinary" => _serviceProvider.GetRequiredService<CloudinaryService>(),
            };
        }
    }
}
