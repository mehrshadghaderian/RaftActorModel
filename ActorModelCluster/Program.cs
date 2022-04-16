using Akka.Configuration;
using Akka.Actor;
using log4net.Config;
using System.Reflection;
using log4net;

var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
var _log4net = log4net.LogManager.GetLogger(typeof(Program)); 
if(args.Length == 0)
{
    
    string[] newarg = { "localhost", "5050"};
    args = newarg;
}
var hocanConfig = ConfigurationFactory.ParseString(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "hocan.configfile")).Replace("$ip", args[0]).Replace("$port", args[1]));
var actorSystemName = "raftActorSystem";
actorSystemName = hocanConfig.GetConfig("cluster").GetString("actorsystemname", actorSystemName);
 
string? selfAddress = string.Format("akka.tcp://{0}@{1}:{2}", "raftActorSystem", (args.Length == 0 ? "localhost": args[0]), (args.Length == 0  ? "5050" : args[1]));
IList<string>? seedsNode = hocanConfig.GetStringList("akka.cluster.seed-nodes");
seedsNode.Add(string.Format("akka.tcp://{0}@{1}:{2}", "raftActorSystem", args[0], args[1]));
_log4net.Info("seed addresses are {0}" + string.Join(",", string.Join(",", seedsNode.Select(s => "\"" + s + "\""))));
ActorSystem.Create("raftActorSystem", ConfigurationFactory.ParseString("akka.cluster.seed-nodes = [" + string.Join(",", seedsNode.Select(s => "\"" + s + "\"")) + "]").WithFallback(hocanConfig));
Console.ReadLine();