
namespace MyNameSpace
{
    using RaftActorSimple;
    using Serilog; 
    public class MyClass
    {
        public static long[] nodeCountList = new long[] { 5,10, 20, 100, 200, 1000, 2000, 5000, 10000, 50000, 100000, 200000, 500000, 1000000, 2000000, 5000000, 10000000, 20000000,40000000,100000000,200000000,400000000,1000000000,2000000000,4000000000,10000000000};
        public static long nodeCount { get; set; } 
     

        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Debug()
  .WriteTo.Console()
  //.WriteTo.File(AppContext.BaseDirectory + "\\logs\\{Date}.log")
  .CreateLogger();
            Console.WriteLine("Select Node Count 0=5:1=10,2=20,3=100,4=200, 5=1000, 6=2000, 7=5000, 8=10000, 9=50000, 10=100000, 11=200000, 12=500000, 13=1000000, 14=2000000,15 =5000000, 16=10000000, 17=20000000,18=400000000,19=1000000000,20=2000000000,21=4000000000,22=10000000000}");
            int arrayindex=Convert.ToInt16(Console.ReadLine());
            nodeCount = nodeCountList[arrayindex];
            Console.WriteLine($"node count = {nodeCount}");
            List<RaftNode> raftnodes = new List<RaftNode>();
       
            for (int i = 1; i <= nodeCount; i++)
            {
                RaftNode raftNode =new RaftNode(i);
                raftnodes.Add(raftNode);
            }
            raftnodes.Shuffle();
            RaftNode randomRaftNode  = raftnodes.First();
            DateTime starttime = DateTime.Now;
            int electionTerm = 1;
            raftnodes.ForEach(node => {
                node.SetRaftNoedList(raftnodes);
            }); 
            randomRaftNode.LeaderElection(electionTerm); 
            Console.WriteLine((DateTime.Now-starttime).TotalSeconds);
            Console.ReadLine();
        }
    }

}
