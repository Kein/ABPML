using ABPMLManager.Model;
using System.Text.RegularExpressions;

namespace ABPMLManager.ABPML
{
    [IniSection(MainEntry.MANAGER_SECTION)]
    public class ManagerSettings
    {
        public UEVersion UVersion = UEVersion.UE4_23;
        public string? AESKey;

        private static readonly Regex match = new Regex(@"^0x[a-zA-Z0-9]{64}$", RegexOptions.Compiled);

        public bool IsValid()
        {
            return UVersion > UEVersion.UE4_22 && AESKey?.Length == 66 && match.IsMatch(AESKey);
        }
    }

    
}
