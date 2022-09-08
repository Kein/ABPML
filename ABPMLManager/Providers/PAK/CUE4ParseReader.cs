using ABPMLManager.Model;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using System.Text;

namespace ABPMLManager.Providers.PAK
{
    public class CUE4ParseReader : AbstractPakReader
    {
        private DefaultFileProvider? Provider;
        private int mounts;

        public CUE4ParseReader(string pakPath, UEVersion version, string AESKey) : base(pakPath, version, AESKey)
        {
        }

        public override bool TryMount()
        {
            bool result;
            try
            {
                var versions = new VersionContainer(TranslateUEVersion());
                Provider = new DefaultFileProvider(PakPath, SearchOption.AllDirectories, true, versions);
                Provider.Initialize();  
                Provider.SubmitKey(new FGuid(), new FAesKey(AESKey));
                mounts = Provider.Mount();
                result = Provider.Files?.Count > 0;
            }
            catch (Exception)
            {
                result = false;
                throw;
            }
            return result;
        }

        public string[] ReadConfigFile(string pakPath)
        {
            string[] result = null;
            if (Provider != null)
            {
                if (Provider.TryCreateReader(pakPath, out var archive) && archive != null && archive.Length > 50 && archive.Length < 5_000_000)
                {
                    string content = Encoding.UTF8.GetString(archive.ReadBytes((int)archive.Length));
                    result = content.Split('\n');
                }
            }
            return result;
        }

        // engine/content/functions/engine_materialfunctions02/utility/debugfloat2values.uasset
        // game/Player/somefolder/clock.uasset
        public override string[]? GetAllFileNames()
        {
            string[]? result = null;
            if (Provider != null && Provider.Files?.Count > 0)
                result = Provider.Files.Keys.ToArray();

            return result;
        }

        private EGame TranslateUEVersion() => Version switch
        {
            UEVersion.UE4_0 => EGame.GAME_UE4_0,
            UEVersion.UE4_1 => EGame.GAME_UE4_1,
            UEVersion.UE4_2 => EGame.GAME_UE4_2,
            UEVersion.UE4_3 => EGame.GAME_UE4_3,
            UEVersion.UE4_4 => EGame.GAME_UE4_4,
            UEVersion.UE4_5 => EGame.GAME_UE4_5,
            UEVersion.UE4_6 => EGame.GAME_UE4_6,
            UEVersion.UE4_7 => EGame.GAME_UE4_7,
            UEVersion.UE4_8 => EGame.GAME_UE4_8,
            UEVersion.UE4_9 => EGame.GAME_UE4_9,
            UEVersion.UE4_10 => EGame.GAME_UE4_10,
            UEVersion.UE4_11 => EGame.GAME_UE4_11,
            UEVersion.UE4_12 => EGame.GAME_UE4_12,
            UEVersion.UE4_13 => EGame.GAME_UE4_13,
            UEVersion.UE4_14 => EGame.GAME_UE4_14,
            UEVersion.UE4_15 => EGame.GAME_UE4_15,
            UEVersion.UE4_16 => EGame.GAME_UE4_16,
            UEVersion.UE4_17 => EGame.GAME_UE4_17,
            UEVersion.UE4_18 => EGame.GAME_UE4_18,
            UEVersion.UE4_19 => EGame.GAME_UE4_19,
            UEVersion.UE4_20 => EGame.GAME_UE4_20,
            UEVersion.UE4_21 => EGame.GAME_UE4_21,
            UEVersion.UE4_22 => EGame.GAME_UE4_22,
            UEVersion.UE4_23 => EGame.GAME_UE4_23,
            UEVersion.UE4_24 => EGame.GAME_UE4_24,
            UEVersion.UE4_25 => EGame.GAME_UE4_25,
            UEVersion.UE4_26 => EGame.GAME_UE4_26,
            UEVersion.UE4_27 => EGame.GAME_UE4_27,
            UEVersion.UE5_0 => EGame.GAME_UE5_0,
            UEVersion.UE5_1 => EGame.GAME_UE5_1,
            _ => EGame.GAME_UE5_LATEST,
        };
    }
}
