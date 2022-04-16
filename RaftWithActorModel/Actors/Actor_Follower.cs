using Akka.Actor; 
using Akka.Cluster.Tools.PublishSubscribe; 

public class Actor_Follower : ReceiveActor
{
    public Actor_Follower()
    {

        Receive<VoteRequest>(vr =>
        {
            bool vote = RaftEvents.VoteRequestEvent?.Invoke(vr) ?? false;
            if (vote)
            {
                Sender.Tell(new Vote(vr.Term, RaftNode.ClusterUid));
            }
        });
    }

    protected override void PreStart()
    {
        var mediator = DistributedPubSub.Get(Context.System).Mediator;
        mediator.Tell(new Subscribe("voterequest", Self));
    }
}