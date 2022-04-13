using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using RaftWithActorModel.Nodes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaftWithActorModel.Logs
{
    class SendStatus { };
    class SendTerminate { };

    public class StatusBroadcastActor:ReceiveActor
    {
        private const int BROADCAST_INTERVAL_MS = 500;
        private ICancelable _statusBroacast;

        public StatusBroadcastActor()
        {
            var mediator = DistributedPubSub.Get(Context.System).Mediator;

            Receive<SendStatus>(s =>
            {
                var ns = new NodeStatus(RaftNode.Term, RaftNode.ClusterUid)
                {
                    ElectionElapsed = RaftNode.ElectionElapsed,
                    ElectionDuration = RaftNode.ElectionDuration,
                    IsLeader = (RaftNode.Role == RaftNode.Roles.Leader),
                    Role = RaftNode.Role.ToString(),
                    ProcessId = RaftNode.ProcessId,
                    Votes = RaftNode.Votes,
                    Majority = RaftNode.Majority
                };

                mediator.Tell(new Publish("nodestatus", ns));
            });

            Receive<SendTerminate>(s =>
            {
                var ns = new NodeStatus(RaftNode.Term, RaftNode.ClusterUid)
                {
                    ElectionElapsed = RaftNode.ElectionElapsed,
                    ElectionDuration = RaftNode.ElectionDuration,
                    IsLeader = (RaftNode.Role == RaftNode.Roles.Leader),
                    Role = RaftNode.Role.ToString(),
                    ProcessId = RaftNode.ProcessId,
                    Votes = RaftNode.Votes,
                    Majority = RaftNode.Majority,
                    Terminated = true
                };
                mediator.Tell(new Publish("nodestatus", ns));
            });
        }

        protected override void PreStart()
        {
            Log.Information("{0}", "Status Broadcast Started");
            _statusBroacast = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
           TimeSpan.FromMilliseconds(BROADCAST_INTERVAL_MS), Context.Self, new SendStatus(), ActorRefs.NoSender);
        }
    }
}