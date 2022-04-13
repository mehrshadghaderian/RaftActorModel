using System;
using System.Collections.Generic;
using System.Text;

namespace RaftWithActorModel.Candidates
{
    public class StartWaitForVote
    {
        public bool Start { get; private set; }
        public StartWaitForVote(bool start)
        {
            Start = start;
        }
    }
}
