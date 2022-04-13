using Akka.Configuration;
using System;
using System.Linq;
using System.IO;
using Akka.Actor;
using Serilog;
using log4net.Config;
using System.Reflection;
using log4net;

//string path = AppContext.BaseDirectory; 
//Log.Logger = new LoggerConfiguration()
//       .MinimumLevel.Debug()
//       .WriteTo.LiterateConsole()
//       .WriteTo.RollingFile(path + "\\logs\\{Date}.log")
//       .CreateLogger();
var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
var _log4net = log4net.LogManager.GetLogger(typeof(Program));
_log4net.Info("Hello Logging World");
var actorSystemName = "raftActorSystem"; 
string ip = args[0];
string port = args[1];

var hocanConfig = ConfigurationFactory.ParseString(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "hocan.configfile")).Replace("$ip", ip).Replace("$port", port));
 

var clusterConfig = hocanConfig.GetConfig("cluster");
if (clusterConfig != null)
{
    actorSystemName = clusterConfig.GetString("actorsystemname", actorSystemName);
}

var selfAddress = string.Format("akka.tcp://{0}@{1}:{2}", actorSystemName, ip, port);
var seedsNode = hocanConfig.GetStringList("akka.cluster.seed-nodes");
if (!seedsNode.Contains(selfAddress))
{
    seedsNode.Add(selfAddress);
}
//Log.Information("seed addresses are {0}", string.Join(",", string.Join(",", seedsNode.Select(s => "\"" + s + "\""))));
_log4net.Info("seed addresses are {0}" + string.Join(",", string.Join(",", seedsNode.Select(s => "\"" + s + "\""))));

//var injectedClusterConfigString = "akka.cluster.seed-nodes = [" + string.Join(",", seedsNode.Select(s => "\"" + s + "\"")) + "]";

//var finalConfig = ConfigurationFactory.ParseString(injectedClusterConfigString)
//    .WithFallback(hocanConfig);

//var system = ActorSystem.Create(actorSystemName, finalConfig);
ActorSystem.Create(actorSystemName, ConfigurationFactory.ParseString("akka.cluster.seed-nodes = [" + string.Join(",", seedsNode.Select(s => "\"" + s + "\"")) + "]")
    .WithFallback(hocanConfig));

Console.ReadLine();