namespace CodeSync.ExecutionService.Helpers
{
    public static class Judge0LanguageMapper
    {
        // Judge0 CE language IDs
        private static readonly Dictionary<string, int> LanguageMap = new()
        {
            { "python", 71 },
            { "javascript", 63 },
            { "typescript", 74 },
            { "java", 62 },
            { "c", 50 },
            { "cpp", 54 },
            { "csharp", 51 }
        };

        public static int GetLanguageId(string language)
        {
            var key = language.ToLower().Trim();

            if (LanguageMap.TryGetValue(key, out int id))
                return id;

            throw new Exception(
                $"Language '{language}' is not supported");
        }

        public static List<string> GetSupportedLanguages()
            => LanguageMap.Keys.ToList();
    }
}