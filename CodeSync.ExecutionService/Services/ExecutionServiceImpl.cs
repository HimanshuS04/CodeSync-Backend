using CodeSync.ExecutionService.DTOs;
using CodeSync.ExecutionService.Helpers;
using CodeSync.ExecutionService.Interfaces;
using CodeSync.ExecutionService.Models;

namespace CodeSync.ExecutionService.Services
{
    public class ExecutionServiceImpl : IExecutionService
    {
        private readonly IExecutionRepository _repo;
        private readonly Judge0Client _judge0;

        public ExecutionServiceImpl(
            IExecutionRepository repo,
            Judge0Client judge0)
        {
            _repo = repo;
            _judge0 = judge0;
        }

        public async Task<ExecutionResultDto> RunCodeAsync(
            Guid userId, RunCodeDto dto)
        {
            // Get Judge0 language ID
            var langId = Judge0LanguageMapper
                .GetLanguageId(dto.Language);

            // Create job record
            var job = new ExecutionJob
            {
                ProjectId = dto.ProjectId,
                FileId = dto.FileId,
                UserId = userId,
                Language = dto.Language,
                SourceCode = dto.SourceCode,
                Stdin = dto.Stdin,
                Status = "RUNNING"
            };
            await _repo.CreateAsync(job);

            try
            {
                // Submit to Judge0 and wait
                var result = await _judge0.SubmitAndWait(
                    langId, dto.SourceCode, dto.Stdin);

                // Map status
                job.Status = result.StatusId switch
                {
                    3 => "COMPLETED",
                    5 => "TIMED_OUT",
                    6 => "COMPILATION_ERROR",
                    _ => result.StatusId > 3
                        ? "FAILED" : "RUNNING"
                };

                job.Stdout = result.Stdout;
                job.Stderr = result.Stderr;
                job.CompileOutput = result.CompileOutput;

                if (result.Time != null &&
                    double.TryParse(result.Time, out var time))
                    job.ExecutionTimeMs = (int)(time * 1000);

                job.MemoryUsedKb = result.Memory;

                await _repo.UpdateAsync(job);
            }
            catch (Exception ex)
            {
                job.Status = "FAILED";
                job.Stderr = ex.Message;
                await _repo.UpdateAsync(job);
            }

            return MapToDto(job);
        }

        public async Task<ExecutionResultDto> GetResultAsync(
            int id)
        {
            var job = await _repo.FindByIdAsync(id)
                ?? throw new Exception("Execution not found");
            return MapToDto(job);
        }

        public async Task<List<ExecutionResultDto>>
            GetByProjectAsync(Guid projectId)
        {
            var jobs = await _repo
                .FindByProjectAsync(projectId);
            return jobs.Select(MapToDto).ToList();
        }
        public async Task<object> GetStatsAsync()
        {
            var total = await _repo.CountAllAsync();
            var completed = await _repo.CountByStatusAsync("COMPLETED");
            var failed = await _repo.CountByStatusAsync("FAILED");
            return new
            {
                totalExecutions = total,
                completedExecutions = completed,
                failedExecutions = failed
            };
        }

        private static ExecutionResultDto MapToDto(
            ExecutionJob j) => new()
        {
            Id = j.Id,
            Language = j.Language,
            Status = j.Status,
            Stdout = j.Stdout,
            Stderr = j.Stderr,
            CompileOutput = j.CompileOutput,
            ExecutionTimeMs = j.ExecutionTimeMs,
            MemoryUsedKb = j.MemoryUsedKb,
            CreatedAt = j.CreatedAt
        };
    }
}