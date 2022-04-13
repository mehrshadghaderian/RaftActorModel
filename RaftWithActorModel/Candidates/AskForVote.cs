using System;
using System.Collections.Generic;
using System.Text;

namespace RaftWithActorModel.Candidates
{
    public class AskForVote
    {
        public int Term { get; private set; }
        public AskForVote(int term)
        {
            Term = term;
        }
    }
}