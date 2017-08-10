using System.Management.Automation;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace IniFileModule
{
    [Cmdlet(VerbsCommon.Get, "IniFile")]
    [OutputType(typeof(IniFile))]
    public class GetIniFile : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public System.IO.FileInfo[] Files { get; set; }

        protected override void BeginProcessing()
        {
            WriteVerbose($"Get-IniFile Begin ParameterSetName={ParameterSetName}...");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            WriteVerbose("Get-IniFile Process...");
            foreach (var item in Files)
            {
                if (!item.Exists)
                {
                    WriteWarning($"{item.FullName} does not exist!");
                }
                else
                {
                    WriteVerbose($"Examining {item.FullName}");
                    var iniFile = new IniFile { Path = item.FullName };
                    var fileMap = IniFileHelper.GetFileMap(item.FullName);
                    foreach (var section in fileMap.Keys)
                    {
                        var sectionValues = fileMap[section];
                        var newSection = new IniFileSection { Path = item.FullName, SectionName = section, Values = sectionValues };
                        iniFile.Sections.Add(newSection);
                    }
                    WriteVerbose($"{item.FullName} finished!");
                    WriteObject(iniFile);
                }
            }
        }

        protected override void EndProcessing()
        {
            WriteVerbose("Get-IniFile End...");
            base.EndProcessing();
        }
    }

    [Cmdlet(VerbsCommon.Get, "IniFileSection", DefaultParameterSetName = "Object")]
    [OutputType(typeof(IniFileSection))]
    public class GetIniFileSection : PSCmdlet
    {

        #region Parameters
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "FileName")]
        public string IniFile { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Object")]
        public IniFile[] InputObject { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public string[] Section { get; set; }
        #endregion

        protected override void BeginProcessing()
        {
            WriteVerbose($"Get-IniFileSection Begin ParameterSetName={ParameterSetName}...");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            WriteVerbose($"Get-IniFileSection Process ParameterSetName={ParameterSetName}...");
            switch (ParameterSetName)
            {
                case "Object":
                    foreach (var item in InputObject)
                    {
                        foreach (var sect in this.Section)
                        {
                            WriteVerbose($"Retrieving section {sect}");

                            var section = item.Sections.Where(s => s.SectionName.Equals(sect, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            if (section != null)
                            {
                                WriteObject(section);
                            }
                        }
                    }
                    break;
                default:
                    var iniHelper = new IniFileHelper(this.IniFile);
                    foreach (var item in Section)
                    {
                        WriteVerbose($"Retrieving section {item}");
                        var fileSection = iniHelper.GetINISection(item);
                        var iniSection = new IniFileSection { Path = this.IniFile, SectionName = item, Values = fileSection };
                        WriteObject(iniSection);
                    }
                    break;
            }
        }

        protected override void EndProcessing()
        {
            WriteVerbose("Get-IniFileSection End...");
            base.EndProcessing();
        }
    }

    [Cmdlet(VerbsCommon.Get, "IniFileValue", DefaultParameterSetName = "Object")]
    [OutputType(typeof(string))]
    public class GetIniFileValue : PSCmdlet
    {

        #region Parameters
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "FileName")]
        public string IniFile { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Object")]
        public IniFile[] InputObject { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string Section { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string KeyName { get; set; }
        #endregion

        protected override void BeginProcessing()
        {
            WriteVerbose($"Get-IniFileSection Begin ParameterSetName={ParameterSetName}...");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            WriteVerbose("Get-IniFileValue Process...");
            base.ProcessRecord();
            switch (ParameterSetName)
            {
                case "Object":
                    foreach (var item in InputObject)
                    {
                        var section = item.Sections.Where(s => s.SectionName.Equals(this.Section, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (section != null)
                        {
                            var kValue = section.Values.Where(a => a.Key.Equals(this.KeyName, System.StringComparison.OrdinalIgnoreCase));
                            if (kValue != null)
                            {
                                WriteObject(kValue);
                            }
                        }
                    }
                    break;
                default:
                    var value = IniFileHelper.GetINIValue(this.IniFile, this.Section, this.KeyName);
                    if (!string.IsNullOrEmpty(value))
                    {
                        WriteObject(value);
                    }
                    break;
            }
        }

        protected override void EndProcessing()
        {
            WriteVerbose("Get-IniFileValue End..");
            base.EndProcessing();
        }
    }

    [Cmdlet(VerbsCommon.Set, "IniFileValue", DefaultParameterSetName = "Object")]
    [OutputType(typeof(void))]
    public class SetIniFileValue : PSCmdlet
    {
        #region Parameters
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "FileName")]
        public string IniFile { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Object")]
        public IniFile[] InputObject { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string Section { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string KeyName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string KeyValue { get; set; }
        #endregion

        protected override void BeginProcessing()
        {
            WriteVerbose($"Set-IniFileValue Begin ParameterSetName={ParameterSetName}...");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            WriteVerbose($"Set-IniFileValue End");
            base.ProcessRecord();
            switch (ParameterSetName)
            {
                case "Object":
                    foreach (var item in InputObject)
                    {
                        IniFileHelper.WriteINIValue(item.Path, this.Section, this.KeyValue, this.KeyValue);
                    }
                    break;
                case "FileName":
                    IniFileHelper.WriteINIValue(this.IniFile, this.Section, this.KeyName, this.KeyValue);
                    break;
            }
        }

        protected override void EndProcessing()
        {
            WriteVerbose($"Set-IniFileValue End");
            base.BeginProcessing();
        }

    }

    [Cmdlet(VerbsData.Save, "IniFile", DefaultParameterSetName = "Object")]
    [OutputType(typeof(void))]
    public class SaveIniFile : PSCmdlet
    {
        #region Parameters
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Object")]
        public IniFile[] InputObject { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Explicit")]
        public string FilePath { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Explicit")]
        public IniFileSection[] Sections { get; set; } 
        #endregion

        protected override void BeginProcessing()
        {
            WriteVerbose($"Save-IniFile Begin ParameterSetName={ParameterSetName}...");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            WriteVerbose("Save-IniFile Process");
            switch (ParameterSetName)
            {
                case "Object":
                    foreach (var item in InputObject)
                    {
                        SaveFile(item);
                    }
                    break;
                case "Explicit":
                    SaveFile(new IniFile { Path = FilePath, Sections = Sections.ToList() });
                    break;
            }
        }

        protected override void EndProcessing()
        {
            WriteVerbose("Save-IniFile End");
            base.EndProcessing();
        }

        private void SaveFile(IniFile file)
        {
            WriteVerbose($"Saving values to {file.Path}");
            var iniFileHelper = new IniFileHelper(file.Path);
            foreach (var section in file.Sections)
            {
                WriteVerbose($"Writing values to [{section.SectionName}] in {iniFileHelper.FilePath}");
                foreach (var key in section.Values.Keys)
                {
                    string value = section.Values[key];
                    WriteVerbose($"Writing {key}={value}");
                    iniFileHelper.WriteINIValue(section.SectionName, key,value);
                }
            }
        }

    }

    [Cmdlet(VerbsCommon.New, "IniFileSectionData",DefaultParameterSetName ="ByName")]
    [OutputType(typeof(void))]
    public class NewIniFileSectionData:PSCmdlet
    {
        [Parameter(Mandatory =true,ValueFromPipeline =true,ParameterSetName = "ByName")]
        public string[] SectionName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ByObject")]
        public Hashtable Values { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ByObject")]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose($"New-IniFileSectionData Process {ParameterSetName}");
            switch (ParameterSetName)
            {
                case "ByName":
                    foreach (var item in SectionName)
                    {
                        WriteObject(new IniFileSection { SectionName = item });
                    }
                    break;
                case "ByObject":
                    var section = new IniFileSection { SectionName = Name };
                    foreach (var item in Values.Keys)
                    {
                        section.Values.Add(item.ToString(), Values[item].ToString());
                    }
                    WriteObject(section);
                    break;
            }
        }
    }

}
