namespace ABPMLManager.Providers.INI
{
    public class ConfigIni
    {
        private string? ConfigName;
        private string? DirectoryPath;

        public Dictionary<string, List<IniKeyValue>> Sections { get; private set; } = new();
        public List<string> Comments { get; private set; } = new();

        public ConfigIni() { }

        public ConfigIni(string dirPath, string fileName)
        {
            ConfigName = fileName;
            DirectoryPath = dirPath;
        }

        public void AddValueToSection(string section, IniKeyValue? value)
        {
            if (!Sections.ContainsKey(section))
                Sections[section] = new List<IniKeyValue>();

            if (value != null)
                Sections[section].Add(value);
        }

        public void AddSectionWithValues(string section, List<IniKeyValue>? values)
        {
            if (!Sections.ContainsKey(section))
                Sections[section] = new List<IniKeyValue>();

            var valref = Sections[section];
            if (values != null)
                foreach (var iniKV in values)
                    valref.Add(iniKV);
        }

        public bool TryGetSection(string sectionName, out List<IniKeyValue>? values)
        {
            return Sections.TryGetValue(sectionName, out values) && values != null && values.Count > 0;
        }

        public void AppendAnother(ConfigIni? config)
        {
            if (config != null)
                foreach (var section in config.Sections)
                    AddSectionWithValues(section.Key, section.Value);
        }

        public void Populate()
        {
            if (string.IsNullOrEmpty(ConfigName) || string.IsNullOrEmpty(DirectoryPath))
                throw new NullReferenceException($"{nameof(ConfigName)} or {nameof(DirectoryPath)} is empty for ConfigIni!");

            var filePath = Path.Combine(DirectoryPath, ConfigName);
            var lines = File.ReadAllLines(filePath);
            ParseData(lines);
        }

        public void PopulateDirectFromFile(string filePath)
        {
            ConfigName = Path.GetFileName(filePath);
            DirectoryPath = Path.GetDirectoryName(filePath);
            var lines = File.ReadAllLines(filePath);
            ParseData(lines);
        }

        public void AppendSections(ConfigIni other)
        {
            foreach (var KV in other.Sections)
                if (!Sections.ContainsKey(KV.Key))
                    Sections.Add(KV.Key, KV.Value);
        }

        // Returns first occurence of the key
        public bool TryGetToken(string section, string propName, out string value)
        {
            bool result = false;
            value = string.Empty;
            if (Sections.TryGetValue(section, out List<IniKeyValue>? KV))
            {
                if (KV != null)
                {
                    foreach (var entry in KV)
                    {
                        if (string.Equals(entry.Key, propName, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(entry.Value))
                        {
                            result = true;
                            value = entry.Value;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public void ParseData(string[] memory)
        {
            var lines = memory;
            string secName = string.Empty;
            //int bogusLines = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var currLine = lines[i].AsSpan().Trim();
                if (currLine.IsEmpty || currLine.Length < 2)
                    continue;

                var delim = currLine.IndexOf('=');
                if (currLine[0] == '[' && currLine[^1] == ']' && currLine.Length > 3)
                {
                    secName = currLine.ToString();
                    if (!Sections.ContainsKey(secName))
                        Sections.Add(secName, new List<IniKeyValue>());
                }
                else if (currLine[0] == ';' && currLine.Length > 1)
                {
                    Comments.Add(currLine.ToString());
                }
                else if (delim > 1)
                {
                    var key = currLine[0..delim]; // all before '='
                    var value = currLine[(delim + 1)..]; // all after '='
                    // Axe empty entries
                    if (!key.IsEmpty && key.Length > 1 && !value.IsEmpty && value.Length > 1)
                        Sections[secName].Add(new IniKeyValue(key.Trim().ToString(), value.Trim().ToString()));
                }
            }
        }

        public void SaveWithOverwrite()
        {
            if (string.IsNullOrEmpty(ConfigName) || string.IsNullOrEmpty(DirectoryPath))
                throw new NullReferenceException($"{nameof(ConfigName)} or {nameof(DirectoryPath)} is empty for ConfigIni!");

            var filePath = Path.Combine(DirectoryPath, ConfigName);
            WriteDirectToFile(filePath);
        }

        public void WriteDirectToFile(string path)
        {
            using (TextWriter tw = File.CreateText(path))
            {
                foreach (var section in Sections)
                {
                    var tokens = section.Value;
                    if (tokens?.Count < 1)
                        continue;
                    tw.WriteLine(section.Key);
                    if (tokens != null)
                        foreach (var item in tokens)
                            tw.WriteLine($"{item.Key}={item.Value}");
                        tw.Write(Environment.NewLine);
                }
            }
        }

    }

    public class IniKeyValue : IEquatable<IniKeyValue>
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public IniKeyValue(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public bool Equals(IniKeyValue? other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (this.Key == other.Key && this.Value == other.Value)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            var k = Key?.GetHashCode() ?? 0;
            var v = Value?.GetHashCode() ?? 0;
            return v + k;
        }
    }
}
