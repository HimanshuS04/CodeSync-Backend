using CodeSync.ExecutionService.Services;

namespace CodeSync.ExecutionService.Interfaces
{
    public interface IJudge0Client
    {
        Task<Judge0Result> SubmitAndWait(
            int languageId,
            string sourceCode,
            string? stdin);
    }
}