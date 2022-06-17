using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Cluster;
using static Akka.Cluster.ClusterEvent;
using Serilog;
using RaftWithActorModel.ActorsClasses;
using System.Security.Cryptography;
public class RaftNodeActor : ReceiveActor
{
    public enum Roles
    {
        Follower = 0,
        Candidate = 1,
        Leader = 2
    }

    public class StopTimeout { }

    private bool _timeStarted;
    private ICancelable _electionIntervalTask;
    private ICancelable _IamDeputyIntervalTask;
    private ICancelable _changeDeputyIntervalTask;
    const int DeputyHeartbatIntervalTime = 20000;

    private const int heartbeat_periodtimemilisecound = 2000;

    private bool _joinedCluster;
    private ICancelable _heartbeatTask;
    private long _nodesCount;
    private int _nodeRequestResponseCount = 0;
    private bool _heartbeatStarted = false; 

    public int Id { get; set; }
    public List<IActorRef> RanfNodeList { get; set; }
    static Roles _role;
    public  Roles Role
    {
        get { return _role; }
        set
        {
            _role = value;
        }
    }
    public   int Term { get; private set; }
    public long raftNodeId { get; private set; }
    public static long CurrentLeaderId { get; private set; }
    public static string CurrentDeputyId { get; private set; }
    
    public   int Selection_ExpiredTime { get; set; }
    public   int SelectionDuration { get; set; }
    private int _votedForTerm = 0;

    public   int Votes { get; private set; }
    public   long Majority { get; private set; }
    public   int ProcessId { get; private set; }
    public   DateTime RequestForVotDateTime { get; private set; }

    //election property
    private const int timeStepMillisecond = 5000;
    private const int minmunPeriodTimeMillisecond = 50000;
    private const int maximumPeriodTimeMillisecond = 100000;
    private int _selectionDuration = maximumPeriodTimeMillisecond;
    private int _expiredTime = 0;
    private bool _selectionStarted = false;
    const int minElectionTIMEOUT = 90000;
    const int maxElectionTIMEOUT = 100000;

    public System.Timers.Timer electionTimer { get; set; }
    public RaftNodeActor(long _raftNodeId,long nodeCount)
    {
        IActorRef? mediator = DistributedPubSub.Get(Context.System).Mediator;
        raftNodeId = _raftNodeId;
        //Log.Warning("** NodeId : " + _raftNodeId + " * *");
        Majority = (nodeCount + 1) / 2;
        Receive<List<IActorRef>>(rflist =>
        {
            RanfNodeList = rflist;
            Majority = (nodeCount + 1) / 2;
            var mediator = DistributedPubSub.Get(Context.System).Mediator;
 
           
        });

        Receive<RequestForVote>(hb =>
        {

            Votes = 0;
            term = 0;
            CurrentDeputyId = "";
            if (CurrentLeaderId.ToString() != Self.Path.Name)
            {
                term++;
                Role = Roles.Candidate;

                Console.WriteLine("######### Node With Id " + _raftNodeId + " Being Candid as a Leader");
                //if (Sender != Self)
                //{
                //    Console.Write(".");
                //}
                RequestForVotDateTime = DateTime.Now;
                _nodesCount = hb.nodecount;
                mediator.Tell(new Publish("voterequest", new VoteRequest(hb.Term, _raftNodeId)));
            }
           

        });
        Receive<KillMessage>(a => {
           if (a.NodeType== NodeType.Deputy)
            {
                CurrentDeputyId = "";
            }
           else  if(a.FirstRequest)
            {
                CurrentLeaderId = -1;
                Log.Information("{0}", "Receive KillRequest");
                if (CurrentDeputyId != "")
                {
                    Log.Error("Current Deputy {0} Change To leader", CurrentDeputyId); 
                    ActorSelection? deputyActor = Context.ActorSelection("/user/" + CurrentDeputyId); 
                    deputyActor.Tell(new RequestForVote(term, DateTime.Now, _nodesCount));
                }
                mediator.Tell(new Publish("heartbeat", new KillMessage(false,NodeType.Leader))); 
            }
            else
            {
                _heartbeatTask?.Cancel();
            }

        });
        Receive<VoteRequest>(vr =>
        {
            if(Self.Path.Name != Sender.Path.Name)
            {
                Role = Roles.Follower;
                bool vote = false;
                term = vr.Term;
                    if (Role != Roles.Leader)
                    {
                    _votedForTerm = vr.Term;
                    vote = true;
                }
                else
                {
                    
                    vote = false;
                }
                if (vote)
                {
                    Sender.Tell(new Vote(vr.Term, raftNodeId));
                }
            }
    
        });
        Receive<Vote>(v =>
        {
            if (Self.Path.Name != Sender.Path.Name && Self.Path.Name!= CurrentLeaderId.ToString())
            {
                Votes++;
                if(Votes == 1)
                { 
                    CurrentDeputyId = Sender.Path.Name;
                    Log.Error("node With Id: "+ CurrentDeputyId+ " Select As Deputy");
                }
                Console.WriteLine("Candid : " + Self.Path.Name+" has "+ Votes +" Vote");
                // Log.Error($"Receive Vote Message,from {v.SenderId} Votes count: {Votes}/{Majority}");
                //reset
                //comment for report 
                //RaftEvents.GotVoteEvent?.Invoke(v.SenderId, v.Term);
                //if (term == v.Term)
                //{
                //    Votes++;
                //}
              
                //Log.Information("{0}", $"Got {Votes}/{Majority} votes for term {term} from {v.SenderId}");
                if (Votes == Majority)
                {

                    CurrentLeaderId = raftNodeId;
                    Console.WriteLine("{0}", "*******************" + CurrentLeaderId + " Selected!  Time : " + (DateTime.Now - RequestForVotDateTime).TotalSeconds);
                    Role = Roles.Leader;
                     _heartbeatTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                        TimeSpan.FromMilliseconds(heartbeat_periodtimemilisecound), Context.Self, new SendHeartbeat(), ActorRefs.NoSender);
                    _changeDeputyIntervalTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(heartbeat_periodtimemilisecound + 5000), Context.Self, new ChangeDeputy(), Self);

                    //Environment.Exit(0);
                    //comment for Report
                    //startWait();
                    if (!_heartbeatStarted)
                    {
                        _heartbeatStarted = true;
                       // Log.Information("{0}", " Start heartbeat time "); 
                       //_heartbeatTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                       // TimeSpan.FromMilliseconds(heartbeat_periodtimemilisecound), Context.Self, new SendHeartbeat(), ActorRefs.NoSender);
                    }
                }
       
            }

    });

        Receive<SendHeartbeatResponse>(s =>
        {
            var sender = Context.ActorSelection(s.SenderPath);
            sender.Tell(new HeartbeatResponse(s.HeartbeatId, s.Term, s.LogIndex));
            if (s.CurrentRequet != null)
            {
                mediator.Tell(new Publish("heartbeat", new SendNodeRequest(s.CurrentRequet.Number, s.CurrentRequet.RequestDateTime, s.CurrentRequet.TotalNumbers)));
            }
        });

        Receive<SendHeartbeat>(send =>
        {
             Console.Write("> "+CurrentLeaderId.ToString());
            // RaftNode.LogIndex = 1;
            if (Sender != Self)
            {
                mediator.Tell(new Publish("heartbeat", new Heartbeat(term, 1, _raftNodeId)));
            }


        });
        Receive<ChangeDeputy>(hb =>
        {
            CurrentDeputyId = "";
            mediator.Tell(new Publish("selectdeputy", new SelectDeputy()));
        });
        Receive<SelectDeputy>(hb =>
        {
            //  اگر قائم مقامی انتخاب نشده بود درخواست قامئم مقامی ارسال شود
            if (Self != Sender && CurrentDeputyId!="")
            {
                Sender.Tell(new SelectDeputyReplay());
            }
        });
        Receive<SelectDeputyReplay>(hb =>
        {
            if(Self!=Sender)
            {
                if(CurrentDeputyId =="")
                {
                    CurrentDeputyId = Sender.Path.Name;
                    Log.Error("node With Id: " + CurrentDeputyId + "select as Deputy");
                }
            }
        });
        Receive<IamDeputy>(hb =>
        {
            //_changeDeputyIntervalTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(heartbeat_periodtimemilisecound+5000), Context.Self, new ChangeDeputy(), Self);
        });
            Receive<Heartbeat>(hb =>
        {
            if (Sender != Self)
            {
                if(Self.Path.Name== CurrentDeputyId )
                {
                    // Sender.Tell(new IamDeputy());
                    if(_changeDeputyIntervalTask!=null)
                    {
                        _changeDeputyIntervalTask.Cancel();
                    } 
                     _changeDeputyIntervalTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(heartbeat_periodtimemilisecound + 5000), Context.Sender, new ChangeDeputy(), Sender);
                }
                
                _heartbeatTask?.Cancel();
                Random random = new Random();
                if (_electionIntervalTask == null)
                {
                    
      
                    _electionIntervalTask = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(random.Next(timeStepMillisecond + minElectionTIMEOUT, timeStepMillisecond + maxElectionTIMEOUT)), Context.Self, new RequestForVote(hb.Term, DateTime.Now, _nodesCount), ActorRefs.NoSender);
                }
                else { 
                    _electionIntervalTask.Cancel();
                    _electionIntervalTask = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(random.Next(timeStepMillisecond + minElectionTIMEOUT, timeStepMillisecond + maxElectionTIMEOUT)), Context.Self, new RequestForVote(hb.Term, DateTime.Now, _nodesCount), ActorRefs.NoSender);
                }
            
                _expiredTime = 0;
                
                //if heartbeat has term equal or bigger than self, then step down
                if (hb.Term >= term)
                {
                    //need to roll back changes and take match leader's log entries
                    if (Role!= Roles.Leader && Role != Roles.Follower)
                    {
                        Log.Information("{0}", "Stepping down");
                        Role = Roles.Follower;
                        // NodeManager.StopHeartbeat();
                        if (!_heartbeatStarted)
                        {
                            _heartbeatStarted = true;
                            Log.Information("{0}", " Start heartbeat time ");
                            _heartbeatTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                       
                            TimeSpan.FromMilliseconds(heartbeat_periodtimemilisecound), Context.Self, new SendHeartbeat(), ActorRefs.NoSender);
                        }
 
                    }

                    if (hb.Term != term)
                    {
                        Log.Information("{0}", $"Changing from term {term} to {hb.Term}");
                        term = hb.Term;
                    }
                    //else { Log.Information("heartbeat Term : {0}", $"  {hb.Term}"); }
                }
            }
        });
     
        Receive<StopTimeout>(v =>
        {
            if (_timeStarted)
            {
             //   Log.Information("{0}", "Wait timeout");
                // RaftEvents.WaitForVoteTimeoutEvent?.Invoke();
                Role = Roles.Follower;
                _expiredTime = 0;
                if (!_selectionStarted)
                {
                    _selectionStarted = true;
                    _electionIntervalTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                    TimeSpan.FromMilliseconds(timeStepMillisecond), Context.Self, new SelectionExpiredTime(), ActorRefs.NoSender);
                }
            }
        });
         

    } 
 
    public int term { get; set; } = 0;
 
    protected override void PreStart()
    {
        var mediator = DistributedPubSub.Get(Context.System).Mediator;
        mediator.Tell(new Subscribe("voterequest", Self));
        mediator.Tell(new Subscribe("heartbeat", Self));
        mediator.Tell(new Subscribe("selectdeputy", Self)); 
        mediator.Tell(new Subscribe("killmessage", Self));
    }
 
}
