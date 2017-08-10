using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace IniFileModule
{
    public class IniFileHelper
    {

        internal static int _bufferSize = 4096;

        #region P/Invokes
        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "GetPrivateProfileStringW")]
        private static extern uint GetPrivateProfileStringW([System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpAppName, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpKeyName, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpDefault, [System.Runtime.InteropServices.OutAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] System.Text.StringBuilder lpReturnedString, uint nSize, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpFileName);

        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "GetPrivateProfileSectionNamesW")]
        private static extern uint GetPrivateProfileSectionNamesW(byte[] lpSzReturnBuffer, uint nSize, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpFileName);

        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "WritePrivateProfileStringW")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool WritePrivateProfileStringW([System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpAppName, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpKeyName, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpString, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpFileName);

        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "GetPrivateProfileSectionW")]
        private static extern uint GetPrivateProfileSectionW([System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpAppName, byte[] lpReturnedString, uint nSize, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpFileName);
        #endregion

        #region Static Methods

        public static bool SectionExists(string INIFile, string SectionName)
        {
            bool secExist = false;
            foreach (string section in GetINISectionNames(INIFile))
            {
                if (String.Equals(section, SectionName, StringComparison.OrdinalIgnoreCase))
                {
                    secExist = true;
                    break;
                }
            }
            return secExist;
        }

        public static List<string> GetINISectionNames(string INIFile)
        {
            List<string> sectionNames = null;
            uint bytesreturned;
            byte[] outputbuffer = new byte[_bufferSize];
            StringBuilder returnbuffer = new StringBuilder(outputbuffer.Length);
            bytesreturned = GetPrivateProfileSectionNamesW(outputbuffer, (uint)outputbuffer.Length, INIFile);
            if (bytesreturned != 0)
            {
                sectionNames = new List<string>();
                foreach (string s in System.Text.Encoding.Unicode.GetString(outputbuffer).Split('\0'))
                {
                    if (!String.IsNullOrEmpty(s.Trim()))
                    {
                        sectionNames.Add(s.Trim());
                    }
                }
            }
            return sectionNames;
        }

        public static void WriteINIValue(string INIFile, string INISection, string INIKey, string newINIValue)
        {
            if (!WritePrivateProfileStringW(INISection, INIKey, newINIValue, INIFile))
            {
                Exception e = new Win32Exception(String.Format("Error writing INI value [{1}] {2}={3} to {0}", INIFile, INISection, INIKey, newINIValue));
                throw e;
            }
        }

        public static Dictionary<string, string> GetINISection(string INIFile, string INISection)
        {
            Dictionary<string, string> sectionVals = null;
            try
            {
                uint bytesreturned;
                byte[] outputbuffer = new byte[_bufferSize];
                bytesreturned = GetPrivateProfileSectionW(INISection, outputbuffer, (uint)outputbuffer.Length, INIFile);
                if (bytesreturned > 0)
                {
                    sectionVals = new Dictionary<string, string>();
                    foreach (string s in Encoding.Unicode.GetString(outputbuffer).Split('\0'))     //Split the section by null char..
                    {
                        sectionVals.Add(s.Trim().Split('=')[0].Trim(), s.Trim().Split('=')[1].Trim()); //Split by the value pair...
                    }
                }
            }
            catch
            {
                //something??
            }
            return sectionVals;
        }

        public static string GetINIValue(string INIFile, string INISection, string INIKey)
        {
            StringBuilder sb = new StringBuilder(_bufferSize);
            uint returnval;
            string IniValue = String.Empty;
            returnval = GetPrivateProfileStringW(INISection, INIKey, String.Empty, sb, (uint)sb.Capacity, INIFile);
            if (returnval > 0)
            {
                IniValue = sb.ToString().TrimEnd();
            }
            return IniValue;
        }

        public static Dictionary<string, Dictionary<string, string>> GetFileMap(string INIFile)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            GetINISectionNames(INIFile).ForEach(s => { result.Add(s, GetINISection(INIFile, s)); });
            return result;
        }

        #endregion

        #region Constructors

        public IniFileHelper()
        {

        }

        public IniFileHelper(string filePath)
        {
            this.FilePath = filePath;
        }

        #endregion

        public string FilePath { get; set; }

        private void TestFileIsSet()
        {
            if (String.IsNullOrEmpty(this.FilePath))
            {
                throw new MissingMemberException("No file path is specified!");
            }
        }

        #region Methods

        public bool SectionExists(string SectionName)
        {
            TestFileIsSet();
            return SectionExists(this.FilePath, SectionName);
        }

        public List<string> GetINISectionNames()
        {
            TestFileIsSet();
            return GetINISectionNames(this.FilePath);
        }

        public void WriteINIValue(string SectionName, string KeyName, string Value)
        {
            TestFileIsSet();
            WriteINIValue(this.FilePath,SectionName, KeyName, Value);
        }

        public Dictionary<string, string> GetINISection(string SectionName)
        {
            TestFileIsSet();
            return GetINISection(SectionName);
        }

        public string GetINIValue(string SectionName, string KeyName)
        {
            TestFileIsSet();
            return GetINIValue(SectionName, KeyName);
        }

        public Dictionary<string, Dictionary<string, string>> GetFileMap()
        {
            TestFileIsSet();
            return GetFileMap(FilePath);
        }

        #endregion
    }
}
