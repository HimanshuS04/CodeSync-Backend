namespace CodeSync.CollabService.OT
{
    public class OTOperation
    {
        public string Type { get; set; } = string.Empty;
        // INSERT or DELETE
        public int Position { get; set; }
        public string Text { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public long Timestamp { get; set; }
    }
}