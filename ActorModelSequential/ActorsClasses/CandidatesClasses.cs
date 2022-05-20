public class RequestForVote  
{
    public int Term { get; private set; }
    public RequestForVote  (int term)
    {
        Term = term;
    }
}
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
    public int SenderId { get; private set; }
    public Vote(int term, int clusterUniqueId)
    {
        Term = term;
        SenderId = clusterUniqueId;
    }
}
public class VoteRequest
{
    public int Term { get; private set; }
    public int SenderId { get; private set; }
    public VoteRequest(int term, int clusterUniqueId)
    {
        SenderId = clusterUniqueId;
        Term = term;
    }
}