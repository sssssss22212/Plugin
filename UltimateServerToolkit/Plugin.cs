using System;
using System.IO;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using UltimateServerToolkit.Modules.AntiRdm;
using UltimateServerToolkit.Modules.Display;
using UltimateServerToolkit.Modules.Karma;
using UltimateServerToolkit.Modules.Notes;
using UltimateServerToolkit.Modules.Profiles;
using UltimateServerToolkit.Modules.RoundLog;
using UltimateServerToolkit.Modules.Whitelist;

namespace UltimateServerToolkit
{
    public sealed class UstPlugin : Plugin<Config>
    {
        public static UstPlugin Instance { get; private set; }

        public override string Name => "UltimateServerToolkit";
        public override string Description => "Хардкорный РП-набор: профили, карма, анти-RDM, вайтлист, заметки, GM-инструменты, лог раунда.";
        public override string Author => "sssssss22212";
        public override Version Version => new Version(1, 0, 0, 0);
        public override Version RequiredApiVersion => new Version(LabApiProperties.CompiledVersion);

        public ProfileStore ProfileStore { get; private set; }
        public ProfileManager Profiles { get; private set; }
        public KarmaService Karma { get; private set; }
        public AntiRdmService AntiRdm { get; private set; }
        public WhitelistService Whitelist { get; private set; }
        public NoteService Notes { get; private set; }
        public RoundLogService RoundLog { get; private set; }
        public DisplayNameUpdater DisplayNames { get; private set; }

        private EventsHandler _events;

        public string DataDirectory { get; private set; }

        public override void Enable()
        {
            if (!Config.IsEnabled)
            {
                Logger.Info("[UST] Выключен в конфиге.");
                return;
            }

            Instance = this;

            DataDirectory = Path.Combine(
                Path.GetDirectoryName(FilePath) ?? Environment.CurrentDirectory,
                "UltimateServerToolkit-Data");
            Directory.CreateDirectory(DataDirectory);

            ProfileStore = new ProfileStore(DataDirectory);
            Profiles = new ProfileManager(this, ProfileStore);
            DisplayNames = new DisplayNameUpdater(this);
            Karma = new KarmaService(this);
            AntiRdm = new AntiRdmService(this);
            Whitelist = new WhitelistService(this);
            Notes = new NoteService(this);
            RoundLog = new RoundLogService(this, DataDirectory);

            _events = new EventsHandler(this);
            _events.Register();

            Logger.Info($"[UST] {Name} v{Version} включён. Данные: {DataDirectory}");
        }

        public override void Disable()
        {
            try
            {
                _events?.Unregister();
                ProfileStore?.SaveAll();
                RoundLog?.Flush();
            }
            catch (Exception e)
            {
                Logger.Error($"[UST] Ошибка при выключении: {e}");
            }

            Logger.Info("[UST] Выключен.");
            Instance = null;
        }
    }
}
