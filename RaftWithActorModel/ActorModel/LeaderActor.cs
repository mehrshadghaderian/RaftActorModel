using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaftWithActorModel.Leader
{
    public class LeaderActor: ReceiveActor
    {
        public LeaderActor()
        {
            Receive<SendAppendEntries>(s => {

            });
        }


    }
}
