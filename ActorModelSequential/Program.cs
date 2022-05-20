
namespace MyNameSpace
{
    using Akka.Actor;
    using Akka.Configuration;
    using Serilog;
    using Akka.Cluster;
    using Akka.Cluster.Tools.PublishSubscribe;
    using RaftWithActorModel.ActorsClasses;

    public  class MyClass
    {
  
        public static int nodeCount { get; set; } = 20000;


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
}");
            var system = ActorSystem.Create("raftActorSystem", hocanConfig);
            //Parallel.For(0, nodeCount,
            //               index =>
            //               {
            //                   var actor = system.ActorOf(Props.Create(() => new RaftNodeActor(index)), "rafnode" + index);
            //                  // NodeManager.CreateActor(system.ActorOf<RaftNodeActor>("rafnode" + index));
            //                   NodeManager.CreateActor(actor);
            //               });
            for (int i = 1; i <= nodeCount; i++)
            {
                var actor = system.ActorOf(Props.Create(() => new RaftNodeActor(i)), "rafnode" + i);
                // NodeManager.CreateActor(system.ActorOf<RaftNodeActor>("rafnode" + index));
                NodeManager.CreateActor(actor);
            }
            // intrduce Actors Together 
            //Parallel.ForEach(NodeManager.GetActorList(), actor =>
            //{
            //    actor.Tell(NodeManager.GetActorList());
            //    //actor.Tell(new Welecome(1,""));
            //}  );
            //foreach (var actor in NodeManager.GetActorList())
            //{
            //    actor.Tell(new RequestForVote(1));
            //}
            NodeManager.GetActorList().FirstOrDefault().Tell(new RequestForVote(1));
            Console.ReadLine();
        }
    }
    
}
