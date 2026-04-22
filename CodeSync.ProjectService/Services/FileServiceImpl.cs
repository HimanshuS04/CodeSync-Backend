using CodeSync.ProjectService.DTOs;
using CodeSync.ProjectService.Helpers;
using CodeSync.ProjectService.Interfaces;
using CodeSync.ProjectService.Models;

namespace CodeSync.ProjectService.Services
{
    public class FileServiceImpl : IFileService
    {
        private readonly IFileRepository _fileRepo;
        private readonly IProjectRepository _projectRepo;

        public FileServiceImpl(
            IFileRepository fileRepo,
            IProjectRepository projectRepo)
        {
            _fileRepo = fileRepo;
            _projectRepo = projectRepo;
        }

        public async Task<FileResponseDto> CreateFileAsync(
            Guid userId, CreateFileDto dto)
        {
            // Check if user is owner or editor
            await ValidateAccess(dto.ProjectId, userId);

            if (await _fileRepo.ExistsAsync(
                dto.ProjectId, dto.Path))
                throw new Exception(
                    "File already exists at this path");

            var file = new CodeFile
            {
                ProjectId = dto.ProjectId,
                Name = dto.Name,
                Path = dto.Path,
                Language = LanguageHelper
                    .DetectLanguage(dto.Name),
                Content = dto.Content,
                Size = System.Text.Encoding.UTF8
                    .GetByteCount(dto.Content),
                CreatedById = userId,
                LastEditedBy = userId
            };

            await _fileRepo.CreateAsync(file);
            return MapToDto(file);
        }

        public async Task<FileResponseDto> GetFileByIdAsync(
            Guid fileId)
        {
            var file = await _fileRepo.FindByIdAsync(fileId)
                ?? throw new Exception("File not found");
            return MapToDto(file);
        }

        public async Task<List<FileResponseDto>>
            GetFilesByProjectAsync(Guid projectId)
        {
            var files = await _fileRepo
                .FindByProjectIdAsync(projectId);
            return files.Select(MapToDto).ToList();
        }

        public async Task<FileResponseDto> UpdateContentAsync(
            Guid userId, UpdateFileContentDto dto)
        {
            var file = await _fileRepo.FindByIdAsync(dto.FileId)
                ?? throw new Exception("File not found");

            // Check if user is owner or editor
            await ValidateAccess(file.ProjectId, userId);

            file.Content = dto.Content;
            file.Size = System.Text.Encoding.UTF8
                .GetByteCount(dto.Content);
            file.LastEditedBy = userId;

            await _fileRepo.UpdateAsync(file);
            return MapToDto(file);
        }

        public async Task<FileResponseDto> RenameFileAsync(
            Guid userId, RenameFileDto dto)
        {
            var file = await _fileRepo.FindByIdAsync(dto.FileId)
                ?? throw new Exception("File not found");

            // Check if user is owner or editor
            await ValidateAccess(file.ProjectId, userId);

            var directory = System.IO.Path
                .GetDirectoryName(file.Path) ?? "";
            var newPath = string.IsNullOrEmpty(directory)
                ? dto.NewName
                : $"{directory}/{dto.NewName}";

            if (await _fileRepo.ExistsAsync(
                file.ProjectId, newPath))
                throw new Exception(
                    "File with this name already exists");

            file.Name = dto.NewName;
            file.Path = newPath;
            file.Language = LanguageHelper
                .DetectLanguage(dto.NewName);
            file.LastEditedBy = userId;

            await _fileRepo.UpdateAsync(file);
            return MapToDto(file);
        }

        public async Task DeleteFileAsync(
            Guid userId, Guid fileId)
        {
            var file = await _fileRepo.FindByIdAsync(fileId)
                ?? throw new Exception("File not found");

            // Check if user is owner or editor
            await ValidateAccess(file.ProjectId, userId);

            file.IsDeleted = true;
            file.LastEditedBy = userId;
            await _fileRepo.UpdateAsync(file);
        }

        public async Task RestoreFileAsync(
            Guid userId, Guid fileId)
        {
            var file = await _fileRepo.FindByIdAsync(fileId)
                ?? throw new Exception("File not found");

            // Only owner can restore
            var project = await _projectRepo
                .FindByIdAsync(file.ProjectId)
                ?? throw new Exception("Project not found");

            if (project.OwnerId != userId)
                throw new Exception(
                    "Only owner can restore files");

            file.IsDeleted = false;
            file.LastEditedBy = userId;
            await _fileRepo.UpdateAsync(file);
        }

        public async Task<List<FileTreeItemDto>>
            GetFileTreeAsync(Guid projectId)
        {
            var files = await _fileRepo
                .FindByProjectIdAsync(projectId);
            return BuildTree(files);
        }

        // Authorization helper
        private async Task ValidateAccess(
            Guid projectId, Guid userId)
        {
            var project = await _projectRepo
                .FindByIdAsync(projectId)
                ?? throw new Exception("Project not found");

            // Owner always has access
            if (project.OwnerId == userId) return;

            // Check if user is a member
            var member = await _projectRepo
                .FindMemberAsync(projectId, userId);

            if (member == null)
                throw new Exception(
                    "You don't have access to this project");

            // Only OWNER and EDITOR can modify files
            if (member.Role != "OWNER"
                && member.Role != "EDITOR")
                throw new Exception(
                    "You don't have edit permission");
        }

        private static List<FileTreeItemDto> BuildTree(
            List<CodeFile> files)
        {
            var root = new List<FileTreeItemDto>();

            foreach (var file in files)
            {
                var parts = file.Path.Split('/');
                var current = root;

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    var folder = current.FirstOrDefault(
                        f => f.Name == parts[i]
                        && f.Type == "folder");

                    if (folder == null)
                    {
                        folder = new FileTreeItemDto
                        {
                            Name = parts[i],
                            Path = string.Join("/",
                                parts.Take(i + 1)),
                            Type = "folder"
                        };
                        current.Add(folder);
                    }

                    current = folder.Children;
                }

                current.Add(new FileTreeItemDto
                {
                    FileId = file.FileId,
                    Name = file.Name,
                    Path = file.Path,
                    Type = "file",
                    Language = file.Language
                });
            }

            SortTree(root);
            return root;
        }

        private static void SortTree(
            List<FileTreeItemDto> items)
        {
            items.Sort((a, b) =>
            {
                if (a.Type != b.Type)
                    return a.Type == "folder" ? -1 : 1;
                return string.Compare(
                    a.Name, b.Name,
                    StringComparison.OrdinalIgnoreCase);
            });

            foreach (var item in items)
            {
                if (item.Children.Count > 0)
                    SortTree(item.Children);
            }
        }

        private static FileResponseDto MapToDto(
            CodeFile f) => new()
        {
            FileId = f.FileId,
            ProjectId = f.ProjectId,
            Name = f.Name,
            Path = f.Path,
            Language = f.Language,
            Content = f.Content,
            Size = f.Size,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt
        };
    }
}