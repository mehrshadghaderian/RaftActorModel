
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
    private List<RaftNode> raftnodeList;

    //protected Cluster cluster = Cluster.Get(Context.System);

   
    static Roles _role;
    public  Roles Role
    {
        get { return _role; }
        set
        {
            _role = value;
        }
    }
    public  int Term { get; private set; }
    public  int raftNodeId { get; private set; } 
    public int Id { get; set; }
    public  int CurrentLeaderId { get; private set; }
    public  int Selection_ExpiredTime { get; set; }
    public  int SelectionDuration { get; set; }
    private int _votedForTerm = 0;

    public  int Votes { get; private set; }
    public  int Majority { get; private set; }
    public  int ProcessId { get; private set; }
    public  DateTime RequestForVotDateTime { get; private set; }

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
        Id = _raftNodeId;
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

    public void LeaderElection(int electionTerm)
    {
        //Console.WriteLine("Node with Id: "+raftNodeId+" Start Request For Vote"); 
        raftnodeList.Where(a => a.Id != this.Id).ToList().ForEach(raftNode => {
            raftNode.RequestForVote(new VoteRequest(electionTerm, raftNodeId,DateTime.Now));
        });
    }
    public void SetRaftNoedList(List<RaftNode> _raftnodeList )
    { 
        this.raftnodeList = _raftnodeList;
        this.Majority = (raftnodeList.Count + 1) / 2; 
    }
    public void RequestForVote(VoteRequest voteRequest)
    { 
        //Console.WriteLine("Node with Id: " + voteRequest.SenderId + " Request For Vote from : " +this.Id);
        bool vote = false;
        this.term = voteRequest.Term;
        if (_votedForTerm < voteRequest.Term && Role != Roles.Leader)
        {
            if (voteRequest.SenderId != this.Id)
            {
                //Log.Information("{0}", $"Vote request from candidate {voteRequest.SenderId} for term {voteRequest.Term}");
            }
            else
            {
                //Log.Information("{0}", $"Vote request from self in term {voteRequest.Term}");
            }

            this._votedForTerm = voteRequest.Term;

            vote = true;
        }
        else
        {
            //Log.Information("{0}", $"Not voting. {vr.SenderId} asking for term {vr.Term}, last voted for {_votedForTerm} and state is {Role.ToString()}");
            vote = false;
        }
        if (vote)
        {
            raftnodeList.Where(a => a.Id == voteRequest.SenderId).FirstOrDefault().Vote(new Vote(voteRequest.Term, this.Id));
 
        }
    }
    public void Vote(Vote vote)
    { 
        //if (this.term == vote.Term)
        //{
        //    Votes++;
        //}
        Votes++;
        //Log.Information("{0}", $"Got {Votes}/{Majority} votes for term {term} from {v.SenderId}");
        if (Votes >= Majority)
        {
            if (Role!=Roles.Leader)
            {
                CurrentLeaderId = raftNodeId;
                Log.Information("{0}", "************************************************************************************" + CurrentLeaderId + " Selected!  Time : " + (DateTime.Now - RequestForVotDateTime).TotalSeconds);
                Role = Roles.Leader;
                if (!_heartbeatStarted)
                {
                    _heartbeatStarted = true;
                    Log.Information("{0}", " Start heartbeat time ");
                    //_heartbeatTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                    //TimeSpan.FromMilliseconds(heartbeat_periodtimemilisecound), Context.Self, new SendHeartbeat(), ActorRefs.NoSender);
                }
            }
            else
            {
                Log.Information("Ignore Vote becuse node is leader");
            }
       
        }
    }
    public   Task PeriodicFooAsync(Action action,TimeSpan interval, CancellationToken cancellationToken)
    {
        while (true)
        { 
              action();
              Task.Delay(interval, cancellationToken);
        }
    }
 
}
