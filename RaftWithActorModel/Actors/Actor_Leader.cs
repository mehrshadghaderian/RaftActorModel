using Akka.Actor;
public class Actor_Leader : ReceiveActor
{
    public Actor_Leader()
    {
        Receive<SendAppendEntries>(s => {
        });
    }


}