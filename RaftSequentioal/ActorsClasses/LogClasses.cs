public class LogEntry
{
    public string Data { get; set; }
    public bool IsCommited { get; set; }
    public int Index { get; set; }
    public int Term { get; set; }
}
public class LogEntryResponse
{
    public int Index { get; set; }
    public int Term { get; set; }
}