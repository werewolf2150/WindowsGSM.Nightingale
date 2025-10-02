using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;
using WindowsGSM.GameServer.Engine;
using System.IO;

namespace WindowsGSM.Plugins
{
    public class Nightingale : SteamCMDAgent
    {
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.Nightingale",
            author = "werewolf2150",
            description = "Support du serveur dédié Nightingale pour WindowsGSM",
            version = "1.0",
            url = "https://github.com/werewolf2150/WindowsGSM.Nightingale",
            color = "#ff8c00"
        };

        public override bool loginAnonymous => true;
        public override string AppId => "2965330";

        public Nightingale(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;
        public string Error, Notice;

        public override string StartPath => @"NightingaleServer.exe";
        public string FullName = "Nightingale Dedicated Server";
        public bool AllowsEmbedConsole = false;
        public int PortIncrements = 10;
        public object QueryMethod = new A2S();

        public string Port = "7777";
        public string QueryPort = "27015";
        public string Defaultmap = "Realm1";
        public string Maxplayers = "10";
        public string Additional = "";

        public async void CreateServerCFG()
        {
            // À compléter si nécessaire
        }

        public async Task<Process> Start()
        {
            string exePath = ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);
            if (!File.Exists(exePath))
            {
                Error = $"{Path.GetFileName(exePath)} introuvable ({exePath})";
                return null;
            }

            string param = $"{_serverData.ServerParam}" + (!AllowsEmbedConsole ? " -log" : string.Empty);

            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = exePath,
                    Arguments = param,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            if (AllowsEmbedConsole)
            {
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                var serverConsole = new ServerConsole(_serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;

                try
                {
                    p.Start();
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return null;
                }

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                return p;
            }

            try
            {
                p.Start();
                return p;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null;
            }
        }

        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                ServerConsole.SetMainWindow(p.MainWindowHandle);
                ServerConsole.SendWaitToMainWindow("^c");
            });
            await Task.Delay(20000);
        }
    }
}