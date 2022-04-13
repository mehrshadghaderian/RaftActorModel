using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using RaftWithActorModel.Candidates;
using RaftWithActorModel.Nodes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaftWithActorModel.Followers
{
    public class FollowerActor:ReceiveActor
    {
        public FollowerActor()
        {

            Receive<VoteRequest>(vr =>
            {
                bool vote = RaftEvents.OnVoteRequest?.Invoke(vr) ?? false;
                if (vote)
                {
                    Sender.Tell(new Vote(vr.Term,RaftNode.ClusterUid));
                }
            });
        }

        protected override void PreStart()
        {
            var mediator = DistributedPubSub.Get(Context.System).Mediator;
            mediator.Tell(new Subscribe("voterequest", Self));
        }
    }
}