using System.Diagnostics;
using ABPMLManager.Providers.PAK;
using ABPMLManager.Providers.INI;
using ABPMLManager.Model;
using ABPMLManager.Extensions;
using ABPMLManager.ABPML;

namespace ABPMLManager
{
    public static class MainEntry
    {
        public const string ABPML_INI = "ABPML.ini";
        public const string ENGINE_SURROGATE = "GameWithMods.ini";
        public const string ABPML_SECTION = "[/Engine/ABPML/Public/O_ABPML_Settings.O_ABPML_Settings_C]";
        public const string MANAGER_SECTION = "[ModManager]";

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            var appPath = System.AppContext.BaseDirectory;
            #if DEBUG
            appPath = @"e:\Games\Endling\test\";
            #endif

            string rootPath = PathTools.DirectoryGoUp(appPath).ToString();
            Debug.Assert(appPath != rootPath);

            try
            {   
                var mainIni = new FileInfo(Path.Combine(rootPath, ABPML_INI));
                var ABPMLConf = new ABPMLSettings();
                var ManagerConf = new ManagerSettings();
                var ABPMLini = MakeDefaultConfigIni<ABPMLSettings>(rootPath, ABPML_INI);
                ABPMLini.AppendAnother(MakeDefaultConfigIni<ManagerSettings>(rootPath, ABPML_INI));
                bool isConfigValid = false;

                if (mainIni.Exists && mainIni.Length > 15)
                {
                    ABPMLini = new ConfigIni(rootPath, ABPML_INI);
                    ABPMLini.Populate();

                    if (ABPMLini.TryGetSection(ABPML_SECTION, out List<IniKeyValue>? values)
                        && ReflectionHelper.IniSectionToObject<ABPMLSettings>(values, out ABPMLSettings abpmlInst))
                    {
                        ABPMLConf = abpmlInst;
                    }
                    if (ABPMLini.TryGetSection(MANAGER_SECTION, out values)
                        && ReflectionHelper.IniSectionToObject<ManagerSettings>(values, out ManagerSettings managerInst))
                    {
                        ManagerConf = managerInst;
                        isConfigValid = true;
                    }
                }

                if (!isConfigValid)
                {
                    ShowWarning("No valid ABPML.ini found!", "Manager will generate a default one but keep in mind,"
                                                           + "\nit most likely wont work with your game without proper setup!"
                                                           + "\nConfig path:"
                                                           + $"\n{mainIni}");
                    ABPMLini.SaveWithOverwrite();
                }

                // Validate project structure
                var project = new UEGameFolder(rootPath);

                // Do we need to process PAKs?
                bool mapIsEmpty = string.IsNullOrEmpty(ABPMLConf.GameStartupMap);
                bool pakScanRequired = ABPMLConf.ModScanType == E_ModScanMethod.ModConfPAK || ABPMLConf.ModScanType == E_ModScanMethod.UObjectPAK;

                // Pak logic if we use PAK
                if (mapIsEmpty || pakScanRequired)
                {
                    // Ensure we can proceed with "valid" AES key
                    if (!ManagerConf.IsValid())
                    {
                        string mapStr = mapIsEmpty ? " and GameStartupMap is empty" : string.Empty;
                        FailFastWithMessage("Invalid ModManager config!", $"Mod scantype is set to {ABPMLConf.ModScanType}{mapStr} but some of the "
                                                                        + $"[ModManager] values are invalid:"
                                                                        + $"\nUE version: {ManagerConf.UVersion}"
                                                                        + $"\nAES Key: {ManagerConf.AESKey}");
                    }

                    var pakData = new CUE4ParseReader(project.PakPath, ManagerConf.UVersion, ManagerConf.AESKey!);
                    var result = pakData.TryMount();

                    if (!result)
                    {
                        FailFastWithMessage("Failed to read PAKs", $"PAK reading process returned 0 available files."
                                           + $"This could mean either"
                                           + $" AES key or UEVersion in the {ABPML_INI} is invalid or this is"
                                           + $" older/newer version of the engine");
                    }

                    // Auto-detect map if needed
                    if (mapIsEmpty)
                    {
                        var configPath = $"{project.Project}/Config/DefaultEngine.ini";
                        var defaultEngine = new ConfigIni(configPath, "DefaultEngine.ini");
                        var data = pakData.ReadConfigFile(configPath);
                        if (data?.Length > 0)
                            defaultEngine.ParseData(data);

                        if (defaultEngine.TryGetToken("[/Script/EngineSettings.GameMapsSettings]", "GameDefaultMap", out string value))
                            ABPMLConf.GameStartupMap = value;
                        else
                            FailFastWithMessage("PAK read error:", "GameStartupMap is empty but PAK provider failed to return valid DefaultEngine.ini");
                    }

                    // Populate mods
                    if (pakScanRequired)
                    {
                        var allNames = pakData.GetAllFileNames();
                        if (allNames?.Length < 58)
                            FailFastWithMessage("Failed to read PAKs", "List of assets for the game is either empty or too short (wrong/outdated AES key?)");

                        var modConfData = new ModConfGenerator(allNames!, "ABPML_ModConf_", "/Game/Mods", project.Project);
                        ABPMLConf.ModSOPArray = ABPMLConf?.ModSOPArray ?? new string[modConfData.Assets.Count];

                        for (int i = 0; i < modConfData.Assets.Count; i++)
                            ABPMLConf!.ModSOPArray[i] = modConfData.Assets[i].ObjectPath;
                    }

                    var newABPMLini = new ConfigIni(rootPath, ABPML_INI);
                    var newValues = ReflectionHelper.ObjectToIniSection<ABPMLSettings>(ABPMLConf, out string sectionName);
                    if (newValues != null)
                    {
                        newABPMLini.AddSectionWithValues(sectionName, newValues);
                        ABPMLini = newABPMLini;
                    }
                }

                // Generate temp Engine.ini
                GenerateSurrogate(rootPath, ABPMLini);

                // Launch the game
                var modsIniPath = Path.Combine(rootPath, ENGINE_SURROGATE);
                LaunchGame(project.ExePath, $"ENGINEINI={modsIniPath}");

            }
            catch (Exception ex)
            {
                FailFastWithMessage("Exception error:", ex.Message);
            }
            Environment.Exit(0);
        }


        //
        // Helpers
        //

        private static void GenerateSurrogate(string rootPath, ConfigIni ABPMLini)
        {
            FileInfo engineFile = new FileInfo(Path.Combine(rootPath, ENGINE_SURROGATE));
            if (engineFile.Exists)
                engineFile.IsReadOnly = false;
            var engineIni = new ConfigIni(rootPath, ENGINE_SURROGATE);
            engineIni.AppendAnother(ABPMLini);
            var bootMap = new IniKeyValue("GameDefaultMap", @"/Engine/ABPML/ABPML_BootstrapMap.ABPML_BootstrapMap"); // TODO: make dynamic based on config
            engineIni.AddValueToSection("[/Script/EngineSettings.GameMapsSettings]", bootMap);
            engineIni.SaveWithOverwrite();
            engineFile.IsReadOnly = true;
        }

        private static void LaunchGame(string exePath, string args)
        {
            var procStartInfo = new ProcessStartInfo(exePath);
            procStartInfo.UseShellExecute = false;
            procStartInfo.Arguments = args;
            Process.Start(procStartInfo);
        }

        private static ConfigIni MakeDefaultConfigIni<T>(string dir, string fileName)
        {
            var config = new ConfigIni(dir, fileName);
            var values = ReflectionHelper.ObjectToIniSection<T>(null, out string section);
            if (values != null)
                config.AddSectionWithValues(section, values);
            return config;
        }

        static void FailFastWithMessage(string title, string message)
        {
            MessageBox.Show(message,title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }

        static void ShowWarning(string title, string message)
        {
            MessageBox.Show(message,title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}