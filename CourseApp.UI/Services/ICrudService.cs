using CourseApp.UI.Models;

namespace CourseApp.UI.Services
{
    public interface ICrudService
    {
        Task<PaginatedResponse<TResponse>> GetAllPaginated<TResponse>(string path, int page);
        Task<TResponse> Get<TResponse>(string path);
        Task<CreateResponse> Create<TRequest>(TRequest request, string path);
        Task Update<TRequest>(TRequest request, string path);
        Task Delete(string path);

        Task<CreateResponse> CreateFromForm<TRequest>(TRequest request, string path);
        Task UpdateFromForm<TRequest>(TRequest request, string path);
    }
}
