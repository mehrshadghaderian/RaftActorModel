public class Heartbeat
{
    public double Id { get; private set; }
    public int Term { get; private set; }
    public long SenderId { get; private set; }
    public int LogIndex { get; private set; }

    public Heartbeat(int term, int logIndex, long senderId)
    {
        Id = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        SenderId = senderId;
        Term = term;
        LogIndex = logIndex;
    }
}
public class NodeRequest
{
    public int TotalNumbers { get; private set; }
    public int Number { get; private set; }
    public DateTime RequestDateTime { get; private set; }

    public NodeRequest(int number,DateTime requestDateTime)
    {
        RequestDateTime = requestDateTime;
        Number = number;
        TotalNumbers += number;
    }

}
public class SendNodeRequest
{
    public int TotalNumbers { get; private set; }
    public int Number { get; private set; }
    public DateTime RequestDateTime { get; set; }

    public SendNodeRequest(int number,DateTime _requestDateTime,int _totalNumbers)
    {

        Number = number;
        TotalNumbers  = _totalNumbers;
        RequestDateTime = _requestDateTime;
    }

}

public class NodeRequestResponse
{ 
    public DateTime sendTime { get;   set; }

    public NodeRequestResponse(DateTime _sendTime)
    {
        sendTime = _sendTime; 
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
    public NodeRequest? CurrentRequet { get; set; }

    public SendHeartbeatResponse(double heartbeatId, int senderId, string senderPath, int term, int logIndex,  NodeRequest? _currentRequet)
    {
        HeartbeatId = heartbeatId;
        Term = term;
        LogIndex = LogIndex;
        SenderId = senderId;
        SenderPath = senderPath;
        CurrentRequet = _currentRequet;
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
