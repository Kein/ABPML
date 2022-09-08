namespace ABPMLManager.Model
{
    internal record UEGameFolder
    {
        /*
        [DllImport("version.dll")]
        static extern int GetFileVersionInfoSize(string fileName, [Out]IntPtr dummy);
        [DllImport("version.dll")]
        static extern bool GetFileVersionInfoExA(int dwFlags, string fileName, int dwHandle, int dwLen, [Out]IntPtr verData);
        [DllImport("version.dll")]
        static extern bool VerQueryValueA(IntPtr vBlockPtr, string queryKey, [Out] IntPtr PtrToData, [Out] IntPtr DataSize);
        */

        const int MAX_BOOSTRAP_LEN = 1 * 1024 * 1024; // bytes
        const string CONTENT_SUFFX = @"Content\Paks\";
        const string MODCONF_SUFFIX = @"modconf\";

        public string Project { get; set; }
        public string RootPath { get; init; }
        public string ExePath { get; init; }
        public string PakPath { get; init; }
        public string ModConfPath { get; init; }

        public UEGameFolder(string project, string rootPath, string exePath, string pakPath, string modConfPath)
        {
            Project = project;
            RootPath = rootPath;
            ExePath = exePath;
            PakPath = pakPath;
            ModConfPath = modConfPath;
        }

        public UEGameFolder(string rootPath)
        {
            if (!EnsureUEFolderStructure(rootPath, out string exePath, out string projectName))
                throw new Exception($"Unable to validate UE game root dir: {rootPath}");

            Project = projectName;
            RootPath = rootPath;
            ExePath = exePath;
            PakPath = Path.Combine(rootPath, projectName, CONTENT_SUFFX);
            ModConfPath = Path.Combine(PakPath, MODCONF_SUFFIX);;
        }

        private bool EnsureUEFolderStructure(string rotPath, out string exePath, out string projectName)
        {
            exePath = string.Empty;
            projectName = string.Empty;
            if (!Directory.Exists(rotPath))
                return false;

            var dirPath = new DirectoryInfo(rotPath);
            foreach (FileInfo file in dirPath.GetFiles())
            {
                if (file.Name.EndsWith(".exe") && file.Length < MAX_BOOSTRAP_LEN && ValidateUEBinary(file))
                {
                    exePath = file.FullName;
                    break;
                }
            }

            foreach (var dir in dirPath.GetDirectories())
            {
                //dir.FullName = D:\someDir\MyDir
                var pakFolder = Path.Combine(dir.FullName, CONTENT_SUFFX);
                var binFolder = Path.Combine(dir.FullName, @"Binaries\");
                if (dir.Name != "Engine" && Directory.Exists(pakFolder) && Directory.Exists(binFolder))
                {
                    projectName = dir.Name;
                    break;
                }
            }
            return exePath != string.Empty && projectName != string.Empty;
        }

        // TODO: fix this to do better in reading binary data
        private bool ValidateUEBinary(FileInfo file)
        {
            // "Epic Games, Inc."
            // "BootstrapPackage"
            // Should be enough for now to identify bostrap binary
            byte[] epicStr = {0x45,0x00,0x70,0x00,0x69,0x00,0x63,0x00,0x20,0x00,0x47,0x00,0x61,0x00,0x6D,0x00,0x65,
                              0x00,0x73,0x00,0x2C,0x00,0x20,0x00,0x49,0x00,0x6E,0x00,0x63,0x00,0x2E,0x00,0x00,0x00};
            byte[] bootStr = {0x42,0x00,0x6F,0x00,0x6F,0x00,0x74,0x00,0x73,0x00,0x74,0x00,0x72,0x00,0x61,0x00,0x70,
                              0x00,0x50,0x00,0x61,0x00,0x63,0x00,0x6B,0x00,0x61,0x00,0x67,0x00,0x65,0x00,0x64,0x00};

            var rawData = File.ReadAllBytes(file.FullName).AsSpan();
            var fileLen = rawData.Length;
            var c = 0;
            bool epicMatch = false, bootMatch = false;
            while (fileLen - c >= 34 && (!epicMatch || !bootMatch))
            {
                var buff = rawData[c..(c+34)];
                if (!epicMatch && buff.SequenceEqual(epicStr))
                    epicMatch = true;

                if (!bootMatch && buff.SequenceEqual(bootStr))
                    bootMatch = true;

                c++;
            }

            /*
            var vSize = GetFileVersionInfoSize(file.FullName, IntPtr.Zero);
            IntPtr data = IntPtr.Zero;
            IntPtr vData = Marshal.AllocHGlobal(vSize);
            GetFileVersionInfoExA(2, file.FullName, 0, vSize, vData);
            IntPtr hglobal = Marshal.AllocHGlobal(vSize);
            IntPtr test = Marshal.AllocHGlobal(8);
            IntPtr test2 = Marshal.AllocHGlobal(8);
            VerQueryValueA(vData, @"\\StringFileInfo\\040904b0\\CompanyName", test, test2);
            */

            return epicMatch && bootMatch;
        }
    }
}
