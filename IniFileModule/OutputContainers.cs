using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IniFileModule
{
    public class IniElement
    {
        public virtual string Path { get; set; }
    }

    public class IniFile:IniElement
    {

        public List<IniFileSection> Sections { get; set; }

        public IniFile()
        {
            this.Sections = new List<IniFileSection>();
        }
    }

    public class IniFileSection:IniElement
    {
        public string SectionName { get; set; }

        public Dictionary<string,string> Values { get; set; }

        public IniFileSection()
        {
            Values = new Dictionary<string, string>();
        }
    }
}
