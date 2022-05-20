using Akka.Actor;
using Akka.Configuration;
using Serilog;
using Akka.Cluster;


Log.Logger = new LoggerConfiguration()
       .MinimumLevel.Error()
       .WriteTo.Console()
       //.WriteTo.File(AppContext.BaseDirectory + "\\logs\\{Date}.log")
       .CreateLogger();
var hocanConfig = ConfigurationFactory.ParseString(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "hocan.configfile")));

List<RaftNode> raftNodes = new List<RaftNode>();
Parallel.For(0,3,
                   index =>
                   {
                       //using (var system = ActorSystem.Create("raftActorSystem", hocanConfig))
                       //{
                       var system = ActorSystem.Create("raftActorSystem", hocanConfig);
                           var cluster = Cluster.Get(system);
                           int uid = cluster.SelfUniqueAddress.Uid;
                           RaftNode? node = new RaftNode(uid);
                           var pId = node.GetProcessId();
                           NodeManager.CreateActorLeader(system.ActorOf<Actor_Leader>("leader"));
                           NodeManager.CreateActorFollower(system.ActorOf<Actor_Follower>("follower"));
                           NodeManager.CreateActorCandidate(system.ActorOf<Actor_Candidate>("candidate"));
                           NodeManager.CreateActorSelection(system.ActorOf<Actor_Selection>("selectionTerm"));
                           NodeManager.CreateActorHeartbeat(system.ActorOf<Actor_Heartbeat>("heartbeat"));
                           Log.Information("Enter 'quit' to exit Actor");
                           raftNodes.Add(node);

                           //for (int i = 0; i < 5; i++)
                           //{ 
                           //}

                       //}
                   });
//int milliseconds = 10000;
//Thread.Sleep(milliseconds);
////var leaderNode=raftNodes.Where(a => a.getRoles() == RaftNode.Roles.Leader).FirstOrDefault();

//var leaderNode = raftNodes.FirstOrDefault();
//int tt = Convert.ToInt32(Console.ReadLine());
//leaderNode.SendRequest(new NodeRequest(tt, DateTime.Now));
 Console.ReadLine();



