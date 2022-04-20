using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe; 
using Akka.Cluster;
using static Akka.Cluster.ClusterEvent; 
using Serilog;

public class Actor_Heartbeat : ReceiveActor
{
    private const int heartbeat_periodtimemilisecound = 1000;

    private bool _joinedCluster;
    private ICancelable _heartbeatTask;
    private int _nodesCount;
    private bool _heartbeatStarted = false;

    protected Cluster cluster = Cluster.Get(Context.System);

    public Actor_Heartbeat()
    {
        var mediator = DistributedPubSub.Get(Context.System).Mediator;

        Receive<Heartbeat>(hb =>
        {
            if (Sender != Self)
            {
                Console.Write(".");
                RaftEvents.HeartbeatEvent?.Invoke(Sender.Path.ToString(), hb);
            }
        });

        Receive<SendHeartbeatResponse>(s =>
        {
            var sender = Context.ActorSelection(s.SenderPath);
            sender.Tell(new HeartbeatResponse(s.HeartbeatId, s.Term, s.LogIndex));
        });

        Receive<SendHeartbeat>(send =>
        {
            Console.Write(">");
            mediator.Tell(new Publish("heartbeat", new Heartbeat(RaftNode.Term, RaftNode.LogIndex, RaftNode.ClusterUid)));
        });

        Receive<HeartbeatResponse>(hbr =>
        {
            if (Sender != Self)
            {
                RaftEvents.HeartbeatEventResponse?.Invoke(hbr);
            }
        });

        Receive<MemberStatusChange>(_ =>
        {
            var selfStatus = cluster.State.Members.Where(m => m.UniqueAddress.Uid == cluster.SelfUniqueAddress.Uid).FirstOrDefault()?.Status ?? MemberStatus.Down;
            if (!_joinedCluster && selfStatus == MemberStatus.Up)
            {
                _joinedCluster = true;
                RaftEvents.JoinedClusterEvent?.Invoke();
            }

            var nodes = cluster.State.Members.Where(m => (m.Status == MemberStatus.Joining
                || m.Status == MemberStatus.Up) && m.Roles.Contains("heartbeat"));

            int nodesCount = nodes.Count();
            if (_nodesCount != nodesCount)
            {
                _nodesCount = nodesCount;
                Log.Information("{0}", $"{nodesCount} nodes in  this cluster.");

                foreach (var m in nodes)
                {
                    Log.Information("{0}", $"Nodes {m.UniqueAddress.Uid} with roles {string.Join(",", m.Roles)} is {m.Status}");
                }

                RaftEvents.NodeChangedEvent?.Invoke(_nodesCount);
            }
        });

        Receive<RunHeartbeat>(s => {
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
    }

    protected override void PreStart()
    {
        cluster.Subscribe(Self, ClusterEvent.InitialStateAsEvents,
            new[] { typeof(ClusterEvent.IMemberEvent) });

        var mediator = DistributedPubSub.Get(Context.System).Mediator;
        mediator.Tell(new Subscribe("heartbeat", Self));

    }

    protected override void PostStop()
    {
        _heartbeatTask?.Cancel();
        cluster.Unsubscribe(Self);
    }
}
