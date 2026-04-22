namespace CodeSync.ProjectService.Helpers
{
    public static class LanguageHelper
    {
        public static string DetectLanguage(string fileName)
        {
            var ext = System.IO.Path.GetExtension(fileName)
                .ToLower();

            return ext switch
            {
                ".py" => "python",
                ".js" => "javascript",
                ".ts" => "typescript",
                ".java" => "java",
                ".cs" => "csharp",
                ".c" => "c",
                ".cpp" => "cpp",
                ".go" => "go",
                ".rs" => "rust",
                ".rb" => "ruby",
                ".php" => "php",
                ".html" => "html",
                ".css" => "css",
                ".json" => "json",
                ".xml" => "xml",
                ".md" => "markdown",
                ".sql" => "sql",
                ".sh" => "shell",
                ".yaml" or ".yml" => "yaml",
                _ => "plaintext"
            };
        }
    }
}