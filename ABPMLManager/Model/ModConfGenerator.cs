using ABPMLManager.Extensions;

namespace ABPMLManager.Model
{
    internal class ModConfGenerator
    {
        const StringComparison COMPARER = StringComparison.OrdinalIgnoreCase;
        private string ContentModFolder; // ProjectBlood/Content/Mods/
        private string ModFolder; // /Game/Mods
        private string ModConfPrefix; // ABPML_ModConf_

        public List<ModConfAsset> Assets { get; private set; }

        internal record ModConfAsset
        {
            public string UAssetPath { get; init; }
            public string ObjectPath { get; init; }
        }

        public ModConfGenerator(string[] assets, string modconfPrefix, string modDir, string projectName)
        {
            ModFolder = PathTools.StripEndSlash(modDir).ToString();
            ContentModFolder = $"{projectName}/Content/{StripGamePrefix(modDir)}/";
            ModConfPrefix = modconfPrefix;
            Assets = FilterAssets(assets);
        }

        private List<ModConfAsset> FilterAssets(string[] assets)
        {
            var list = new List<ModConfAsset>();
            foreach (var file in assets)
            {
                if (!string.IsNullOrEmpty(file) && file.Length > 15 && file.IndexOf(ContentModFolder, COMPARER) == 0
                                                                    && file.IndexOf(ModConfPrefix, COMPARER) > -1)
                {
                    var spanAsset = GetShortPackageName(file, out ReadOnlySpan<char> justpath);
                    justpath = justpath[ContentModFolder.Length..];
                    var finalPath = $"{ModFolder}/{justpath}{spanAsset}.{spanAsset}"; // Game/Mods/ABPML_ModConf_MyMod.ABPML_ModConf_MyMod.
                    list.Add(new ModConfAsset() { UAssetPath = file, ObjectPath = finalPath });
                }
            }
            return list;
        }

        private ReadOnlySpan<char> GetShortPackageName(string assetPath, out ReadOnlySpan<char> pathWithoutAsset)
        {
            var data = assetPath.AsSpan();
            int dot = -1;
            int slash = data.Length - 1;
            while (slash > 0 && data[slash] != '/')
            {
                if (data[slash] == '.')
                    dot = slash;
                slash--;
            }

            if (dot == -1 || slash == 0)
                throw new Exception($"Invalid asset path for {nameof(GetShortPackageName)}: {assetPath}");

            pathWithoutAsset = data[0..(slash + 1)];
            return data[(slash + 1)..dot];
        }


        // Expecting "Game/Mods" etc
        private string StripGamePrefix(string modDir)
        {
            var span = modDir.AsSpan();
            span = PathTools.StripForwardSlash(span);
            var i = span.IndexOf(@"Game/", StringComparison.OrdinalIgnoreCase);
            var j = span.IndexOf(@"Game\", StringComparison.OrdinalIgnoreCase);
            if (i > -1 || j > -1)
                span = span[(i + 5)..];
            else
                throw new Exception($"Invalid ModDir path supplied to ModDescriptorGenerator: {modDir}");

            span = PathTools.StripEndSlash(span);
            return span.ToString();
        }

    }


}
