using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace LizardTCP
{
    internal class Program
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();
        public static RulesClass[] Rules = Array.Empty<RulesClass>();

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing logger...");

            #region NLog Initializator

            var config = new NLog.Config.LoggingConfiguration();
            LogManager.Configuration = new LoggingConfiguration();
            const string LayoutFile = @"[${date:format=yyyy-MM-dd HH\:mm\:ss}] [${logger}/${uppercase: ${level}}] [THREAD: ${threadid}] >> ${message} ${exception: format=ToString}";
            var consoleTarget = new ColoredConsoleTarget("Console Target")
            {
                Layout = @"${counter}|[${date:format=yyyy-MM-dd HH\:mm\:ss}] [${logger}/${uppercase: ${level}}] [THREAD: ${threadid}] >> ${message} ${exception: format=ToString}"
            };

            var logfile = new FileTarget();

            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");

            // Rules for mapping loggers to targets
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, consoleTarget);

            logfile.CreateDirs = true;
            logfile.FileName = $"logs{Path.DirectorySeparatorChar}lastlog.log";
            logfile.AutoFlush = true;
            logfile.LineEnding = LineEndingMode.CRLF;
            logfile.Layout = LayoutFile;
            logfile.FileNameKind = FilePathKind.Absolute;
            logfile.ConcurrentWrites = false;
            logfile.KeepFileOpen = true;

            // Apply config
            NLog.LogManager.Configuration = config;

            #endregion NLog Initializator

            await InitializeConfigsAsync();

            Logger.Info("Starting LizardTCP...");
            Dictionary<string, Thread> RulesTasksDict = new Dictionary<string, Thread>();
            foreach (var _rule in Rules)
            {
                var t = new Thread(() => Task.Factory.StartNew(() => new Proxy.TcpForwarderSlim().Start(
                    new IPEndPoint(IPAddress.Parse(_rule.bindIP), _rule.bindPort),
                    new IPEndPoint(IPAddress.Parse(_rule.ruleIP), _rule.rulePort))).ConfigureAwait(false));
                t.Start();
                RulesTasksDict.Add(_rule.ruleName, t);
                Logger.Info("Activated: " + _rule.ruleName);
            }
            Logger.Info("Task created!");
            while (true)
            {
            }
        }

        public static async Task InitializeConfigsAsync()
        {
            Logger.Info("Reading config...");
            SettingsClass Settings = JsonConvert.DeserializeObject<SettingsClass>(await File.ReadAllTextAsync("settings.json"));
            Logger.Info("Loading rules...");
            JArray RulesContainer = (JArray)JsonConvert.DeserializeObject(await File.ReadAllTextAsync("rules.json"));
            int RulesCount = 0;
            for (; RulesCount < RulesContainer.Count; ++RulesCount)
            {
                RulesClass _rule = JsonConvert.DeserializeObject<RulesClass>(RulesContainer[RulesCount].ToString());
                Misc.AppendRule(_rule);
            }

            Logger.Info($"Loaded: {RulesCount} rules!");
        }
    }
}