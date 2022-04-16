public class Heartbeat
{
    public double Id { get; private set; }
    public int Term { get; private set; }
    public int SenderId { get; private set; }
    public int LogIndex { get; private set; }

    public Heartbeat(int term, int logIndex, int senderId)
    {
        Id = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        SenderId = senderId;
        Term = term;
        LogIndex = logIndex;
    }
}
public class HeartbeatResponse
{
    public double Id { get; set; }
    public int LogIndex { get; set; }
    public int Term { get; set; }

    public HeartbeatResponse(double id, int term, int logIndex)
    {
        Id = id;
        Term = term;
        LogIndex = logIndex;
    }
}
public class SendHeartbeat
{
}
public class SendHeartbeatResponse
{
    public double HeartbeatId { get; private set; }
    public int Term { get; private set; }
    public int LogIndex { get; private set; }
    public int SenderId { get; private set; }
    public string SenderPath { get; private set; }

    public SendHeartbeatResponse(double heartbeatId, int senderId, string senderPath, int term, int logIndex)
    {
        HeartbeatId = heartbeatId;
        Term = term;
        LogIndex = LogIndex;
        SenderId = senderId;
        SenderPath = senderPath;
    }
}
public class RunHeartbeat
{
    public bool Start { get; private set; }
    public RunHeartbeat(bool start)
    {
        Start = start;
    }
}
