
using Serilog; 
using System.Security.Cryptography;
public class RaftNode 
{
    public enum Roles
    {
        Follower = 0,
        Candidate = 1,
        Leader = 2
    }

    public class StopTimeout { }

    private bool _timeStarted;
  
    private const int heartbeat_periodtimemilisecound = 1000;

    private bool _joinedCluster;
    
    private int _nodesCount;
    private int _nodeRequestResponseCount = 0;
    private bool _heartbeatStarted = false;

    //protected Cluster cluster = Cluster.Get(Context.System);

    public int Id { get; set; }
   
    static Roles _role;
    public static Roles Role
    {
        get { return _role; }
        set
        {
            _role = value;
        }
    }
    public static int Term { get; private set; }
    public static int raftNodeId { get; private set; }
    public static int CurrentLeaderId { get; private set; }
    public static int Selection_ExpiredTime { get; set; }
    public static int SelectionDuration { get; set; }
    private int _votedForTerm = 0;

    public static int Votes { get; private set; }
    public static int Majority { get; private set; }
    public static int ProcessId { get; private set; }
    public static DateTime RequestForVotDateTime { get; private set; }

    //election property
    private const int timeStepMillisecond = 50;
    private const int minmunPeriodTimeMillisecond = 5000;
    private const int maximumPeriodTimeMillisecond = 10000;
    private int _selectionDuration = maximumPeriodTimeMillisecond;
    private int _expiredTime = 0;
    private bool _selectionStarted = false;
    public RaftNode(int _raftNodeId)
    { 
        raftNodeId = _raftNodeId;  
    }
    private void randomTimeout()
    {
        byte[] b = new byte[2];
        RandomNumberGenerator.Create().GetBytes(b);
        double rand = Math.Abs((double)BitConverter.ToInt16(b, 0)) / 100000;
        // todo mehrshad
        //Log.Information("{0}", $"selection is now {_selectionDuration}ms");
        _selectionDuration = (int)(rand * (maximumPeriodTimeMillisecond - minmunPeriodTimeMillisecond) + minmunPeriodTimeMillisecond);
        //RaftEvents.SelectionDurationChangedEvent?.Invoke(_selectionDuration);
        SelectionDuration = _selectionDuration;
    }
    public int term { get; set; } = 0;

    public void LeaderElection(List<RaftNode> raftNodes)
    {
        Console.WriteLine("Node with Id: "+this.Id+" Start Request For Vote");
        raftNodes.Where(a => a.Id != this.Id).ToList().ForEach(raftNode => {
            RequestForVote(new VoteRequest(term, this.Id));
        });
    }
    public void RequestForVote(VoteRequest voteRequest)
    { 
        Console.WriteLine("Node with Id: " + voteRequest.SenderId + " Request For Vote from : " +this.Id);
    }
    public void VoteRequest()
    {
        bool vote = false;
        term = vr.Term;
        if (_votedForTerm < vr.Term && Role != Roles.Leader)
        {
            if (vr.SenderId != raftNodeId)
            {
                Log.Information("{0}", $"Vote request from candidate {vr.SenderId} for term {vr.Term}");
            }
            else
            {
                Log.Information("{0}", $"Vote request from self in term {vr.Term}");
            }

            _votedForTerm = vr.Term;

            vote = true;
        }
        else
        {
            //Log.Information("{0}", $"Not voting. {vr.SenderId} asking for term {vr.Term}, last voted for {_votedForTerm} and state is {Role.ToString()}");
            vote = false;
        }
        if (vote)
        {
            Sender.Tell(new Vote(vr.Term, raftNodeId));
        }
    }
}
