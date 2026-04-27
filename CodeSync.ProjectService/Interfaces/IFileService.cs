using CodeSync.ProjectService.DTOs;

namespace CodeSync.ProjectService.Interfaces
{
    public interface IFileService
    {
        Task<FileResponseDto> CreateFileAsync(
            Guid userId, CreateFileDto dto);
        Task<FileResponseDto> GetFileByIdAsync(Guid fileId);
        Task<List<FileResponseDto>> GetFilesByProjectAsync(
            Guid projectId);
        Task<FileResponseDto> UpdateContentAsync(
            Guid userId, UpdateFileContentDto dto);
        Task<FileResponseDto> RenameFileAsync(
            Guid userId, RenameFileDto dto);
        Task DeleteFileAsync(Guid userId, Guid fileId);
        Task RestoreFileAsync(Guid userId, Guid fileId);
        Task<List<FileTreeItemDto>> GetFileTreeAsync(
            Guid projectId);
    }
}