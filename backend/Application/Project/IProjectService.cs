using DashboardApi.Application.Project.Response;

namespace DashboardApi.Application.Project
{
    public interface IProjectService
    {
        Task<List<ProjectResponse>> GetAllProjects();

        Task<List<ProjectResponse>> GetProjectsByUser();
    }
}
