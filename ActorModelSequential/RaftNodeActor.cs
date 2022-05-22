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
    private ICancelable _timerTask;

    private const int heartbeat_periodtimemilisecound = 1000;

    private bool _joinedCluster;
    private ICancelable _heartbeatTask;
    private int _nodesCount;
    private int _nodeRequestResponseCount = 0;
    private bool _heartbeatStarted = false;

    //protected Cluster cluster = Cluster.Get(Context.System);

    public int Id { get; set; }
    public List<IActorRef> RanfNodeList { get; set; }
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
    public RaftNodeActor(int _raftNodeId)
    {
        IActorRef? mediator = DistributedPubSub.Get(Context.System).Mediator;
        raftNodeId = _raftNodeId;
 
        Receive<List<IActorRef>>(rflist =>
        {
            RanfNodeList = rflist;
            Majority = (RanfNodeList.Count + 1) / 2;
            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            //   Log.Error(raftNodeId.ToString());
        //    Console.Write(raftNodeId + " ");
        //    if (raftNodeId==1)
        //    {
        //       // Log.Error("111111111111111111111111111111111111111111111111111111111111111111111111111;");
        //        requestForVote(mediator, raftNodeId);
        //    }
        //    else
        //    {
        ////Log.Error("XXXXXXXXXXXXXXX;");
        //    }
           
        });

        Receive<RequestForVote>(hb =>
        {
            if (Sender != Self)
            {
                Console.Write(".");
            }
            RequestForVotDateTime = hb.datetime;
            mediator.Tell(new Publish("voterequest", new VoteRequest(hb.Term, _raftNodeId)));
        });

        Receive<VoteRequest>(vr =>
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
        });
        Receive<Vote>(v =>
        {
            //Log.Information("{0}", "Receive Vote Message, than Reset Wait timeout");
            //reset
            stopWait();
            startWait();
            //RaftEvents.GotVoteEvent?.Invoke(v.SenderId, v.Term);
            if (term == v.Term)
            {
                Votes++;
            }

            //Log.Information("{0}", $"Got {Votes}/{Majority} votes for term {term} from {v.SenderId}");
            if (Votes >= Majority)
            {
                CurrentLeaderId = raftNodeId;
                Log.Information("{0}", "************************************************************************************"+ CurrentLeaderId + " Selected!  Time : " +(DateTime.Now-RequestForVotDateTime).TotalSeconds);
                Role = Roles.Leader;
              
                startWait();
                if (!_heartbeatStarted)
                {
                    _heartbeatStarted = true;
                    Log.Information("{0}", " Start heartbeat time ");
                    _heartbeatTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                    TimeSpan.FromMilliseconds(heartbeat_periodtimemilisecound), Context.Self, new SendHeartbeat(), ActorRefs.NoSender);
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
           // Console.Write(">");
            // RaftNode.LogIndex = 1;
            mediator.Tell(new Publish("heartbeat", new Heartbeat(term, 1, _raftNodeId)));
        });
        Receive<Heartbeat>(hb =>
        {
            if (Sender != Self)
            {
              //  Console.Write(".");
                //   RaftEvents.HeartbeatEvent?.Invoke(Sender.Path.ToString(), hb);


                //resets selection time
                //otherwise becomes candidate and send request for votes         
                // NodeManager.ResetSelectionTimer();
                _expiredTime = 0;
                //if heartbeat has term equal or bigger than self, then step down
                if (hb.Term >= term)
                {
                    //need to roll back changes and take match leader's log entries
                    if (Role != Roles.Follower)
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

                        // NodeManager.StartSelectionTimer();
                        if (!_selectionStarted)
                        {
                            _selectionStarted = true;
                            _timerTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                            TimeSpan.FromMilliseconds(timeStepMillisecond), Context.Self, new SelectionExpiredTime(), ActorRefs.NoSender);
                        }
                    }

                    if (hb.Term != term)
                    {
                        Log.Information("{0}", $"Changing from term {term} to {hb.Term}");
                        term = hb.Term;
                    }
                }
            }
        });
        Receive<HeartbeatResponse>(hbr =>
        {
            if (Sender != Self)
            {
                //متدش رو پدا نکردم در پروژه قبل
                //  RaftEvents.HeartbeatEventResponse?.Invoke(hbr);
            }
        });
        Receive<RunHeartbeat>(s =>
        {
            if (s.Start)
            {
                if (!_heartbeatStarted)
                {
                    _heartbeatStarted = true;
                    Log.Information("{0}", " Start heartbeat time ");
                    _heartbeatTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                    TimeSpan.FromMilliseconds(heartbeat_periodtimemilisecound), Context.Self, new SendHeartbeat(), ActorRefs.NoSender);
                }
            }
            else
            {
                if (_heartbeatStarted)
                {
                    _heartbeatStarted = false;
                    Log.Information("{0}", "Stop heartbeat time ");
                    _heartbeatTask?.Cancel();
                }
            }
        });
        Receive<StopTimeout>(v =>
        {
            if (_timeStarted)
            {
                Log.Information("{0}", "Wait timeout");
                // RaftEvents.WaitForVoteTimeoutEvent?.Invoke();
                Role = Roles.Follower;
                _expiredTime = 0;
                if (!_selectionStarted)
                {
                    _selectionStarted = true;
                    _timerTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                    TimeSpan.FromMilliseconds(timeStepMillisecond), Context.Self, new SelectionExpiredTime(), ActorRefs.NoSender);
                }
            }
        });

        Receive<SelectionExpiredTime>(t => {

            _expiredTime += timeStepMillisecond;

          //  RaftEvents.SelectionExpiredTimeEvent?.Invoke(_expiredTime);
            Selection_ExpiredTime = _expiredTime;
            //todo mehrshad
            //Console.Write($"({(float)_expiredTime / 1000})");
            if (_expiredTime >= _selectionDuration)
            {
                randomTimeout();
                //RaftEvents.SelectionTimeoutEvent?.Invoke();
                if (Role == Roles.Follower)
                {
                    Role = Roles.Candidate;
                    term++;
                    Votes = 0;
                    Log.Information("{0}", $"Starting selection for term {term}");
                    // NodeManager.StopSelectionTimer();
                    if (_selectionStarted)
                    {
                        _selectionStarted = false;
                        _timerTask?.Cancel();
                    }
                    // NodeManager.RequestForVote(Term);
                    mediator.Tell(new Publish("voterequest", new VoteRequest(term, raftNodeId)));
                    //NodeManager.StartWaitForVote();
                    startWait();
                }
            }
        });

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
    private void requestForVote(IActorRef? mediator, int id)
    {
        term++;
        //Parallel.ForEach(RanfNodeList, node =>
        //{
        //    node.Tell(new RequestForVote(term));
        //});
        mediator.Tell(new Publish("voterequest", new VoteRequest(term, id)));


    }
    protected override void PreStart()
    {
        var mediator = DistributedPubSub.Get(Context.System).Mediator;
        mediator.Tell(new Subscribe("voterequest", Self));
        mediator.Tell(new Subscribe("heartbeat", Self));
    }
    private void startWait()
    {
        if (!_timeStarted)
        {
            _timeStarted = true;
            _timerTask = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(3),
                Context.Self, new StopTimeout(), ActorRefs.NoSender);
        }
    }

    private void stopWait()
    {
        if (_timeStarted)
        {
            _timeStarted = false;
            _timerTask?.Cancel();
        }
    }
}
