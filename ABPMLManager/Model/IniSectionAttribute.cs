namespace ABPMLManager.Model
{
    public class IniSectionAttribute : Attribute
    {
        public IniSectionAttribute(string sectionName) => SectionName = sectionName;
        public string SectionName { get; }
    }
}
