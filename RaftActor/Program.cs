
namespace MyNameSpace
{
    using Akka.Actor;
    using Akka.Configuration;
    using Serilog;
    using Akka.Cluster;
    using Akka.Cluster.Tools.PublishSubscribe;
    using RaftWithActorModel.ActorsClasses;
    using System.Diagnostics;

    public  class MyClass
    {

        public static long[] nodeCountList = new long[] { 2, 10, 20, 100, 200, 1000, 2000, 5000, 10000, 50000, 100000, 200000, 500000, 1000000, 2000000, 5000000, 10000000, 20000000, 40000000, 100000000, 200000000, 400000000, 1000000000, 2000000000, 4000000000, 10000000000 };
        public static long nodeCount { get; set; }

        public static void Main()
        {
       
            Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Debug()
  .WriteTo.Console()
  //.WriteTo.File(AppContext.BaseDirectory + "\\logs\\{Date}.log")
  .CreateLogger();
            var hocanConfig = ConfigurationFactory.ParseString(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "hocan.configfile")));
            var config = ConfigurationFactory.ParseString(@"
akka.remote.dot-netty.tcp {
    transport-class = ""Akka.Remote.Transport.DotNetty.DotNettyTransport, Akka.Remote""
    transport-protocol = tcp
    port = 8091
    hostname = ""127.0.0.1""
	stdout-loglevel = ""OFF""
	loglevel = ""OFF""
	log-dead-letters-during-shutdown = off
	log-dead-letters = off
}");
            Console.WriteLine("Select Node Count 0=5:1=10,2=20,3=100,4=200, 5=1000, 6=2000, 7=5000, 8=10000, 9=50000, 10=100000, 11=200000, 12=500000, 13=1000000, 14=2000000,15 =5000000, 16=10000000, 17=20000000,18=400000000,19=1000000000,20=2000000000,21=4000000000,22=10000000000}");
            Console.WriteLine("k1= kill leader *** k2=kill Deputy *** k3=kill Deputy and Leader");
         int arrayindex = Convert.ToInt16(Console.ReadLine());
            nodeCount = nodeCountList[arrayindex];
            Console.WriteLine($"node count = {nodeCount}");
            var system = ActorSystem.Create("raftActorSystem", hocanConfig);
            //Parallel.For(0, nodeCount,
            //               index =>
            //               {
            //                   var actor = system.ActorOf(Props.Create(() => new RaftNodeActor(index)), "rafnode" + index);
            //                  // NodeManager.CreateActor(system.ActorOf<RaftNodeActor>("rafnode" + index));
            //                   NodeManager.CreateActor(actor);
            //               });
            //for (int i = 1; i <= nodeCount; i++)
            //{
            //    var actor = system.ActorOf(Props.Create(() => new RaftNodeActor(i,nodeCount)), "rafnode" + i);
            //    // NodeManager.CreateActor(system.ActorOf<RaftNodeActor>("rafnode" + index));
            //    NodeManager.CreateActor(actor);
            //}
            Random Dice = new Random();
            long randomActorId = Dice.NextInt64(nodeCount);
            ActorPath? ranomActorPath=null;
         
            Parallel.For(0, nodeCount,
             i =>
             {
                 var actor = system.ActorOf(Props.Create(() => new RaftNodeActor(i, nodeCount)), i.ToString());
                 if (randomActorId == i)
                     ranomActorPath = actor.Path;
             });

            ActorSelection? randomActor = system.ActorSelection(ranomActorPath);
            randomActor.Tell(new RequestForVote(1,DateTime.Now,nodeCount));
            string key = Console.ReadLine();
             GetEntry(randomActor); 
        }
        public static void GetEntry(ActorSelection?  randomActor)
        {
            string key = Console.ReadLine();
            if (key == "k1")
            {
                randomActor.Tell(new KillMessage(true, NodeType.Leader));
                GetEntry(randomActor);
            }
            else if (key == "k2")
            {
                randomActor.Tell(new KillMessage(true, NodeType.Deputy));
                GetEntry(randomActor);
            }
            else if (key == "k3")
            {
                randomActor.Tell(new KillMessage(true, NodeType.All));
                GetEntry(randomActor);
            }
            else GetEntry(randomActor);
        }
    }
    
}
