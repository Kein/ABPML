namespace ABPMLManager.ABPML
{
    public enum E_ActorSpawnType
    {
        Direct,
        DeferredSummon,
        NextTimerTick
    }

    public enum E_ModScanMethod
    {
        DataAssetAuto,
        ModConfPAK,
        ModConfManual,
        UObjectAuto,
        UObjectPAK,
        UObjectManual
    }

    public enum E_ModSpawnMethod
    {
        UWorldChange,
        EngineEOF,
        DeferredEOF
    }

    public enum E_UIMode
    {
        GameAndUI,
        Game,
        UIOnly
    }

}
