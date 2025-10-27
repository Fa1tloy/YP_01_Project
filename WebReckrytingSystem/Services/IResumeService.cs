// Services/ResumeService.cs
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public interface IResumeService
    {
        ServiceResult<Resume> CreateResume(string userEmail, CreateResumeViewModel model);
        ServiceResult<Resume> UpdateResume(string userEmail, CreateResumeViewModel model);
        Resume? GetUserResume(string userEmail);
    }

}