public class RequestForVote  
{
    public int Term { get; private set; }
    public long nodecount { get; private set; }
    public DateTime datetime { get; private set; }
    public RequestForVote  (int term,DateTime _datetime, long _nodecount)
    {
        Term = term;
        datetime = _datetime;
        nodecount = _nodecount;
    }
}
public enum NodeType
{
    Follower = 0,
    Candidate = 1,
    Leader = 2,
    Deputy=3
}
public class KillMessage
{
    public bool FirstRequest { get; private set; } = true;
    public NodeType NodeType { get; private set; } = NodeType.Leader;

    public KillMessage(bool? firstRequest, NodeType? nodeType)
    {
        FirstRequest = firstRequest.HasValue ? firstRequest.Value : true;
        NodeType = nodeType.HasValue? nodeType.Value:NodeType.Leader;
    }
}
public class IamDeputy { }
public class ChangeDeputy { }
public class SelectDeputyReplay { }
public class SelectDeputy { }



public class StartWaitForVote
{
    public bool Start { get; private set; }
    public StartWaitForVote(bool start)
    {
        Start = start;
    }
}
public class Vote
{
    public int Term { get; private set; }
    public long SenderId { get; private set; }
    public Vote(int term, long clusterUniqueId)
    {
        Term = term;
        SenderId = clusterUniqueId;
    }
}
public class VoteRequest
{
    public int Term { get; private set; }
    public long SenderId { get; private set; }
    public VoteRequest(int term, long clusterUniqueId)
    {
        SenderId = clusterUniqueId;
        Term = term;
    }
}