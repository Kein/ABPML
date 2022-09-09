# ABPML
Blueprint-based mod-loader for Unreal Engine. It attempts to be as generic as possible for easy set-up that does not require replacing any in-game assets for bootstrap. Mostly aimed at indie-games that do not attempt to change engine code.

ABPML strives to provides:
* automatic mod discovery
* both Widget and Actor based mods
* interface for mods to interact with Manager
* interface for Manager to interact with mods
* some basic-events via EventManager like PersistentLevel changed, new streamed/instanced level loaded and visible, etc.
* persistent mods (widgets)
* persistent data for mods to store
* ini-based config for mods, if desired
* mod spawn rules

## Downloads

**4.23** 
https://drive.google.com/file/d/1FUzMeHjtCZ9IKXamnLvyPKmkUUfYohDj/view?usp=drivesdk

**4.26** 
https://drive.google.com/file/d/1gTM9HJC8dBM8KJhD6LUJWa20uniUELWh/view?usp=drivesdk

**4.27**
https://drive.google.com/file/d/160NMZyTtZ8Frc4YwqWZcoRND0IEA2c4u/view?usp=drivesdk

## Where is the "sources"
The raw uassets will be added to `BinaryData` when the spaghetto inside is no longer vomit-inducing.

## Compatibility

**UE4.22 and below** - supported, but requires backport.

**UE4.23 - UE 5.1** - supported

[`NM_Standalone` builds only! `NM_client` functionality will be limited or broken for obvious reasons]

ModManager only requires so that the game read and mounts custom PAKs or IOstore files properly. As long as this is valid it should work.
Usual restrictions apply when the game has a custom changes made to Unreal Engine, this mostly means majority of Chinese games (absolutely paranoid)  or Big Publisher games.

## Usage/Installation

### Unattended setup 
ABPML supports unattended configuration when all required options baked into modmanager's PAK file. These usually expected to be made by community for specific game as a base modmanager for mods to use for end-user. In simple terms, it is a legacy drop-in `_P` PAK file.
Unattended mode only works for type of game that allow asset auto-discovery at runtime.

### ModManager tool/Manual
Requires some user interaction, mainly just launching the game via ModManager and some INI-file editing *in case pre-made correct one aren't provided*. The tool will automatically detect some of the settings for generic ABPML PAKs (see ***"Downloads"*** above), provided basic configuration options like `UVersion` and `AESKey` are supplied.
This mode also used for manual (as in - manual by MM tool) PAK-based mods detection, when automatic discovery is confirmed to failing at runtime for specific game.

### Installation
#### Manual/ModManager
1. Copy *appropriate to your game version* `ABPML_UE4_2x_P.pak` into `<your_game_path>\GameName\Content\Paks\`
2. If this is not an unattend-installation, copy default `Config/ABPML.ini` in this repo into `<your_game_path>`
3. Edit defaults with correct values for your game, mostly `UVersion` and `AESKey`
4. Download ModManager release
5. Extract into `<your_game_path>\ModManager`
6. Run `Modmanager.exe`
7. (Optional) If `ModConfPAK` mod discovery method is used, game **ALWAYS** must be started via `Modmanager.exe`


#### Unattended

1. Copy ABPML pak file provided by modding community  into `<your_game_path>\GameName\Content\Paks\`
2. That's all.

### Runtime Usage

Mods spawned automatically according to their config rules.

**Shift+F2** to summon basic UI (WIP)

**=** to close UI (or used button)

**crn.quality 1** - use this console command if game has input locked to UI (requires `bSpawnConsole=True` if supported)

### Creating compatible mods
1. Find your Unreal Engine installation folder (call it `engine_root\`)
2. Copy folder `Engine\` from `BinaryData\` folder in this repo into your `engine_root\`. There will be no overwrites.
3. Create basic project for your game or start existing
4. In the `Content Browser`, on the bottom right, click `View options`, select `Show Engine Content` and disable Plugin/C++ if you dont need it (to avoid clutter)
5. Now, toggle asset tree view on the left and navigate to `Engine Content/ABPML`. right click and select `Validate`, then repeat and select `Resave all`
6. Go back to your projects `Content/` folder.
7. Create folder `Mods/`, inside that folder create some folder for your mod to keep organized, it can be a short name of your mod. For example `Mods/InfiniteAmmo/`
8. Create new `Data Asset` asset (in Miscellaneous on right click) in this folder, as base/parent select `PDA_ABPML_ModConfig`
9. Name it as `ABPML_ModConf_<your_mod_short_name>` where last bit (without `<>` is mod's short name)
10. Create your Actor or UserWidget Blueprint that will be your mod. Name it as `ABPML_Mod_<your_mod_short_name>`
11. Open your `ABPML_ModConf` DataAsset, and in `ModToLoad` select your mod object `ABPML_Mod_***`
12. Fill the rest of the settings, save.
13. Edit your mod to your hearts content.
14. Use pak-chunks or manual PAKing to pak your mods content with full path (ie. you need to pack `Mods/` folder structure)
15. Drop created PAK into game's `Paks` folder. Depending on game' setup, `_P` prefix might be needed, otherwise, use `pakchunk` when available.
16. That's all

### Naming rules

Although automatic mod discovery system does not require any strict naming, because it cannot be relied upon in 100% of cases, strict naming required for forward and backward compatibility.
Rules as of now:
*  Prefix for mod config: **ABPML_ModConf_**
*  Prefix for main mod object: **ABPML_Mod_**


## Features

### Mod discovery types
ABPML supports (or plans to) few types of mod loading:
* ***DataAssetAuto*** - as long as ModConf asset is inherited and named correctly, and placed into correct mods folder (`ModPackagePath`) it will discover and load them automatically.
* ***ModConfPAK*** - if previous method fails, ModManager tool will scan PAK files, using current PAK provider, detect mods and build a list of `ModConf` to process at runtime.
* ***ModConfManual*** - mod authors specify the correct mod object path manually when they make the mod and `modconf.ini` for it. These are gathered and processed at runtime by ABPML.
* ***UObjectAuto*** - currently not used. Reserved for when DataAsset mode fails.
* ***UObjectPAK*** - see above
* ***UObjectManual***  - see above

### Events

**[OnWorldObjectChanged]**

Fires when underlying UWorld object has changed i.e. when old World is destroyed and new on loaded. This method is default fallback when others aren't reliable.

**[OnActorEndPlay]**

Most useful for persistent UserWidget mods to get EndOfPlay events, so they dont have to fabricate their own dummy actor.

**[OnEngineEOFNewWorld]**

When available, if runtime restrictions allow, fired right at the end of Engine's Tick, just after new UWorld was created but hasnt ticked yet.

**[OnLevelRemovedFromWorld]**
**[OnLevelAddedToWorld]**
**[OnMultiLevelLoad]**

Fired when new level is added to World's level list, loaded and visible. Tracking these could be expensive, so the option is disabled by default. Resolution check: 1 second (configurable)

### ModConfig

**ModToLoad**

SoftClassObjectReference to your mod.

**ShortName**

Short name of your mod, used in Manager UI

**LongName**

Unused for now, but worth filling in.

**Author**
**Description**

Shows in pop-up window.

**OnlySpawnOnLevel**

Level name (short) that is used to check against when spawning your mod, i.e. if you wan to spawn only on `TitleMap` level, specify this string. Leave empty or `Any` to respawn on every level.

**Version**

We use default Engine's `Intvector` struct as a semver alternative. Long story short, UserDefinedStruct absolutely unreliable so we have to improvise. `X` - major, `Y` - minor, `Z` - changeset/patch

**bIsPersistent**

Defines if the mod is persistent. This mean it will be spawned **only once** and added to KeepAlive list to survive PersistentLevel reloads. **This only applies to UserWidget-based mods!** Actor ALWAYS will be destroyed, there is **NO** bypassing it via BP. ABPML will ignore Actors.

**bOneShot**

Defines if mod is spawned only once in lifetime.

**bDisabledByDefault**

Disabled spawn by default (i.e. when you only want to spawn it via UI)

**bIsAsyncLoad**

Reserved for future use.

### Manager's config (ABPML config)

todo
```
[/Engine/ABPML/Public/O_ABPML_Settings.O_ABPML_Settings_C]
bSpawnConsole=True
bModSpawnEnabled=True
bRemoveFailedOnSpawn=True
bEnableAutoTravel=False
bEnableLevelEvents=False
bRestoreOriginaltDefaulMap=False
bUseARForGameMap=False
ModSpawnType=EngineEOF
WorkerSpawnType=DeferredSummon
ModScanType=DataAssetAuto
UIReturnMode=Game
TickResolution=0.008
TravelDelay=0
LevelEventsCheckFreq=1
StreamedLevelsThreshold=3
ModPrefix=ABPML_Mod_
ModConfPrefix=ABPML_ModConf_
MountDir=/Engine/ABPML/
ModPackagePath=/Game/Mods

[ModManager]
UVersion=UE4_27
AESKey=0x*************************************************
```
