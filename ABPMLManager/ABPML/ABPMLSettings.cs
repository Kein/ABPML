using ABPMLManager.Model;

namespace ABPMLManager.ABPML
{
    [IniSection(MainEntry.ABPML_SECTION)]
    public record ABPMLSettings
    {
        public bool bSpawnConsole = true;
        public bool bModSpawnEnabled = true;
        public bool bRemoveFailedOnSpawn = true;
        public bool bEnableAutoTravel = true;
        public bool bEnableLevelEvents;
        public bool bRestoreOriginalDefaultMap;
        public bool bUseARForGameMap;
        public E_ModSpawnMethod ModSpawnType = E_ModSpawnMethod.EngineEOF;
        public E_ActorSpawnType WorkerSpawnType = E_ActorSpawnType.DeferredSummon;
        public E_ModScanMethod ModScanType = E_ModScanMethod.DataAssetAuto;
        public E_UIMode UIReturnMode = E_UIMode.Game;
        public float TickResolution = 0.008f;
        public float TravelDelay = 0.0f;
        public float LevelEventsCheckFreq = 1f;
        public int StreamedLevelsThreshold = 3;
        public string? GameStartupMap;
        public string ModPrefix = "ABPML_Mod_";
        public string ModConfPrefix = "ABPML_ModConf_";
        public string MountDir = "/Engine/ABPML/";
        public string ModPackagePath = "/Game/Mods";
        public string[]? ModSOPArray;
    }
}
