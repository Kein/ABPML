using ABPMLManager.Model;

namespace ABPMLManager.Providers.PAK
{
    public abstract class AbstractPakReader
    {
        protected readonly string PakPath;
        protected readonly UEVersion Version;
        protected readonly string AESKey;

        public AbstractPakReader(string pakPath, UEVersion version, string aESKey)
        {
            PakPath = pakPath;
            Version = version;
            AESKey = aESKey;
        }

        public abstract bool TryMount();
        public abstract string[]? GetAllFileNames();

    }
}
