﻿/*
                           __                           __     
                         /'__`\                  __    /\ \    
 _____      __     _ __ /\ \/\ \    ___     ___ /\_\   \_\ \   
/\ '__`\  /'__`\  /\`'__\ \ \ \ \ /' _ `\  / __`\/\ \  /'_` \  
\ \ \L\ \/\ \L\.\_\ \ \/ \ \ \_\ \/\ \/\ \/\ \L\ \ \ \/\ \L\ \ 
 \ \ ,__/\ \__/.\_\\ \_\  \ \____/\ \_\ \_\ \____/\ \_\ \___,_\
  \ \ \/  \/__/\/_/ \/_/   \/___/  \/_/\/_/\/___/  \/_/\/__,_ /
   \ \_\                                                       
    \/_/                                      addicted to code


Copyright (C) 2018  Stefan 'par0noid' Zehnpfennig

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

namespace par0noid
{
    /// <summary>
    /// A lightweight class for read/write/work with Config.ini's
    /// </summary>
    public class Config : IEnumerable<Config.ConfigSection>
    {
        /// <summary>
        /// List of sections
        /// </summary>
        private List<ConfigSection> _Sections;
        
        private Encoding _Encoding;
        private string _Path = null;

        /// <summary>
        /// Count of config-sections in this config
        /// </summary>
        public int Count => _Sections.Count;

        /// <summary>
        /// Encoding of config content text
        /// </summary>
        public Encoding Encoding => _Encoding;
        
        /// <summary>
        /// Path to config file
        /// </summary>
        public string Path => _Path;

        /// <summary>
        /// Initializes an empty config
        /// </summary>
        public Config()
        {
            _Sections = new List<ConfigSection>();
            _Encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Initializes a config with the content of the given file
        /// </summary>
        /// <param name="Path">Path to config file</param>
        /// <param name="ConfigEncoding">Encoding of the config file</param>
        public Config(string Path, Encoding ConfigEncoding = null) : this()
        {
            _Encoding = ConfigEncoding == null ? Encoding.UTF8 : ConfigEncoding;


            string[] ConfigLines;

            try
            {
                ConfigLines = File.ReadAllLines(Path, _Encoding);
            }
            catch { throw new FileNotFoundException("Cannot read configfile"); }

            _Path = Path;

            string CurrentSection = "default";

            foreach(string Line in ConfigLines)
            {
                if(Line.Contains("=") || Line.Contains("["))
                {
                    if (Line.Replace(" ", "").Replace("\t", "").StartsWith("#") || Line.Replace(" ", "").Replace("\t", "").StartsWith("//"))
                    {
                        //Auskommentiert
                    }
                    else
                    {
                        if (Line.Contains("="))
                        {
                            //Entry

                            string[] splitted = Line.Split(new char[] { '=' });

                            string Key = splitted[0].Replace(" ", "").Replace("\t", "");

                            string[] ValueSplitted = new string[splitted.Length - 1];

                            Array.Copy(splitted, 1, ValueSplitted, 0, ValueSplitted.Length);

                            string Value = string.Join("=", ValueSplitted).TrimStart(new char[] { ' ', '\t' });

                            Add(CurrentSection, Key, Value);

                        }
                        else
                        {
                            //Section

                            string CleanedLine = Line.Replace(" ", "").Replace("\t", "");

                            Regex r = new Regex(@"^\[(?<section>[\w\-\.]+)\]$", RegexOptions.None);

                            if(r.IsMatch(CleanedLine))
                            {
                                Match m = r.Match(CleanedLine);

                                Add(m.Groups["section"].ToString());
                                CurrentSection = m.Groups["section"].ToString();
                            }
                        }
                    }
                    
                }
                else
                {
                    //Kein Eintrag
                }
            }
        }


        /// <summary>
        /// Initializes a config with the content of the given file
        /// </summary>
        /// <param name="Path">Path to config file</param>
        public static implicit operator Config(string Path) => new Config(Path);

        /// <summary>
        /// Initializes a config with the content of the given file
        /// </summary>
        /// <param name="fileInfo">FileInfo-Object</param>
        public static implicit operator Config(FileInfo fileInfo) => new Config(fileInfo.FullName);

        /// <summary>
        /// Returns the content of the config as string
        /// </summary>
        /// <param name="config">Config-Object</param>
        public static implicit operator string(Config config) => config.GenerateConfigContent();

        /// <summary>
        /// Returns the content of the config as string
        /// </summary>
        public override string ToString() => GenerateConfigContent();

        /// <summary>
        /// Adds a section to the config
        /// </summary>
        /// <param name="SectionName">Name of the section</param>
        /// <returns>true on success, false if the section already exists</returns>
        public bool Add(string SectionName)
        {
            if(!HasSection(SectionName))
            {
                _Sections.Add(new ConfigSection(SectionName));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a section with entry to the config
        /// </summary>
        /// <param name="SectionName">Name of the section</param>
        /// <param name="EntryName">Name of the entry</param>
        /// <param name="Value">Value of the entry</param>
        /// <returns>true on success, false if the section or entry already exists</returns>
        public bool Add(string SectionName, string EntryName, object Value)
        {
            if (!HasSection(SectionName))
            {
                _Sections.Add(new ConfigSection(SectionName));
                this[SectionName].Add(EntryName, Value);
                return true;
            }
            else
            {
                if(!HasEntry(SectionName, EntryName))
                {
                    this[SectionName].Add(EntryName, Value);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Deletes a section from config
        /// </summary>
        /// <param name="SectionName">Name of the section</param>
        /// <returns>true on success, false if the section doesn't exist</returns>
        public bool Delete(string SectionName)
        {
            if(HasSection(SectionName))
            {
                _Sections.Remove((ConfigSection)this[SectionName]);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the config to the initial config path. It will return false if the config is not initialized with a path.
        /// </summary>
        /// <returns>true on success, false if the config is not initialized with a path or the config file can't be written</returns>
        public virtual bool Save()
        {
            if(_Path == null)
            {
                return false;
            }

            string Content = GenerateConfigContent();

            try
            {
                File.WriteAllText(_Path, Content, _Encoding);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the config to the given path
        /// </summary>
        /// <returns>true on success, false if the config file can't be written</returns>
        public virtual bool Save(string Path)
        {
            _Path = Path;
            return Save();
        }

        /// <summary>
        /// Returns the section
        /// </summary>
        /// <param name="SectionName">Name of the section</param>
        /// <returns>Section-Object on success, null on fail</returns>
        public ConfigSection this[string SectionName]
        {
            get
            {
                if(HasSection(SectionName))
                {
                    return (from x in _Sections where x.Name.ToLower() == SectionName.ToLower() select x).First();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns the section
        /// </summary>
        /// <param name="Index">Index of the section</param>
        /// <returns>Section-Object on success, null on fail</returns>
        public ConfigSection this[int Index]
        {
            get
            {
                if (Index >= 0 && Index < Count)
                {
                    return _Sections[Index];
                }
                else
                {
                    return null;
                }
            }

        }

        /// <summary>
        /// Checks if the section exists
        /// </summary>
        /// <param name="SectionName">Name of the section</param>
        /// <returns>true if it exists, false if not</returns>
        public bool HasSection(string SectionName) => (from x in _Sections where x.Name.ToLower() == SectionName.ToLower() select x).Count() == 1;

        /// <summary>
        /// Checks if the entry exists
        /// </summary>
        /// <param name="SectionName">Name of the section</param>
        /// <param name="EntryName">Name of the entry</param>
        /// <returns>true if it exists, false if not</returns>
        public bool HasEntry(string SectionName, string EntryName)
        {
            if ((from x in _Sections where x.Name.ToLower() == SectionName.ToLower() select x).Count() == 1)
            {
                return (from x in _Sections where x.Name.ToLower() == SectionName.ToLower() select x).First().HasEntry(EntryName);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Generates the config-text-content
        /// </summary>
        /// <returns>Config content as string</returns>
        private string GenerateConfigContent()
        {
            StringBuilder output = new StringBuilder();

            output.AppendLine($"# Saved ({DateTime.Now.ToString()})");

            foreach (ConfigSection section in _Sections)
            {
                output.AppendLine();
                output.AppendLine($"[{section.Name}]");
                output.AppendLine();

                foreach (ConfigEntry entry in section.Entrys)
                {
                    output.AppendLine($"{entry.Name} = {entry.Value}");
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Returns enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<ConfigSection> GetEnumerator() => ((IEnumerable<ConfigSection>)_Sections).GetEnumerator();

        /// <summary>
        /// Returns enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ConfigSection>)_Sections).GetEnumerator();

        /// <summary>
        /// A section of a config
        /// </summary>
        public class ConfigSection : IEnumerable<Config.ConfigEntry>
        {
            /// <summary>
            /// Name of the section
            /// </summary>
            private string _Name;
            /// <summary>
            /// Entrys of the seciton
            /// </summary>
            private List<ConfigEntry> _Entrys;

            /// <summary>
            /// Count of config-entrys in this section
            /// </summary>
            public int Count => _Entrys.Count;

            /// <summary>
            /// Name of the section
            /// </summary>
            public string Name
            {
                get { return _Name; }
                set { _Name = value.ToLower(); }
            }

            /// <summary>
            /// Entrys of the seciton
            /// </summary>
            public ConfigEntry[] Entrys => _Entrys.ToArray();

            /// <summary>
            /// Initializes a config-section with the given name
            /// </summary>
            /// <param name="Name">Name of the section</param>
            public ConfigSection(string Name)
            {
                _Name = Name.ToLower();
                _Entrys = new List<ConfigEntry>();
            }

            /// <summary>
            /// Adds an entry to the section
            /// </summary>
            /// <param name="EntryName">Name of the entry</param>
            /// <param name="Value">Value of the entry</param>
            /// <returns>true on success, false if it already exists</returns>
            public bool Add(string EntryName, object Value)
            {
                if (!HasEntry(EntryName))
                {
                    _Entrys.Add(new ConfigEntry(EntryName, Value));
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Deletes an entry from the section
            /// </summary>
            /// <param name="EntryName">Name of the entry</param>
            /// <returns>true on success, false if it doesn't exist</returns>
            public bool Delete(string EntryName)
            {
                if (HasEntry(EntryName))
                {
                    _Entrys.Remove((ConfigEntry)this[EntryName]);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Check if the section has an entry with the given name
            /// </summary>
            /// <param name="EntryName">Name of the entry</param>
            /// <returns>true/false</returns>
            public bool HasEntry(string EntryName) => (from x in _Entrys where x.Name.ToLower() == EntryName.ToLower() select x).Count() == 1;

            /// <summary>
            /// Returns enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public IEnumerator<ConfigEntry> GetEnumerator() => ((IEnumerable<ConfigEntry>)_Entrys).GetEnumerator();

            /// <summary>
            /// Returns enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ConfigEntry>)_Entrys).GetEnumerator();

            /// <summary>
            /// Returns the config-entry
            /// </summary>
            /// <param name="EntryName">Name of the entry</param>
            /// <returns>Config-entry-Object on success, null on fail</returns>
            public ConfigEntry this[string EntryName]
            {
                get
                {
                    if ((from x in _Entrys where x.Name.ToLower() == EntryName.ToLower() select x).Count() == 1)
                    {
                        return (from x in _Entrys where x.Name.ToLower() == EntryName.ToLower() select x).First();
                    }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    (from x in _Entrys where x.Name.ToLower() == EntryName.ToLower() select x).First().Value = value.ToString();
                }
            }
            

            /// <summary>
            /// Returns the entry
            /// </summary>
            /// <param name="Index">Index of the entry</param>
            /// <returns>Entry-Object on success, null on fail</returns>
            public ConfigEntry this[int Index]
            {
                get
                {
                    if (Index >= 0 && Index < Count)
                    {
                        return _Entrys[Index];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }


        /// <summary>
        /// An entry of a config-section
        /// </summary>
        public class ConfigEntry
        {
            /// <summary>
            /// Name of the entry
            /// </summary>
            private string _Name;
            /// <summary>
            /// Value of the entry
            /// </summary>
            private string _Value;

            /// <summary>
            /// Name of the entry
            /// </summary>
            public string Name
            {
                get { return _Name; }
                set { _Name = value.ToLower(); }
            }
            /// <summary>
            /// Value of the entry
            /// </summary>
            public object Value
            {
                get { return _Value; }
                set { _Value = value.ToString(); }
            }

            /// <summary>
            /// Initializes a config-entry with the given name and value
            /// </summary>
            /// <param name="Name">Name of the entry</param>
            /// <param name="Value">Value of the entry</param>
            public ConfigEntry(string Name, object Value)
            {
                _Name = Name.ToLower();
                _Value = Value.ToString();
            }

            /// <summary>
            /// Returns the config-entry value as string
            /// </summary>
            /// <returns>Value as string</returns>
            public override string ToString() => _Value;

            /// <summary>
            /// Returns the config-entry value as integer
            /// </summary>
            /// <returns>Value as integer</returns>
            public int ToInt()
            {
                int value;

                if (int.TryParse(_Value, out value))
                {
                    return value;
                }
                else
                {
                    return 0;
                }
            }

            /// <summary>
            /// Returns the config-entry value as unsigned integer
            /// </summary>
            /// <returns>Value as unsigned integer</returns>
            public uint ToUInt()
            {
                uint value;

                if (uint.TryParse(_Value, out value))
                {
                    return value;
                }
                else
                {
                    return 0;
                }
            }

            /// <summary>
            /// Returns the config-entry value as long
            /// </summary>
            /// <returns>Value as long</returns>
            public long ToLong()
            {
                long value;

                if (long.TryParse(_Value, out value))
                {
                    return value;
                }
                else
                {
                    return 0;
                }
            }

            /// <summary>
            /// Returns the config-entry value as ulong
            /// </summary>
            /// <returns>Value as ulong</returns>
            public ulong ToULong()
            {
                ulong value;

                if (ulong.TryParse(_Value, out value))
                {
                    return value;
                }
                else
                {
                    return 0;
                }
            }

            /// <summary>
            /// Returns the config-entry value as bool
            /// </summary>
            /// <returns>Value as bool</returns>
            public bool ToBool() => _Value.ToLower() == "true" || _Value.ToLower() == "on" || _Value.ToLower() == "1" ? true : false;

            /// <summary>
            /// Returns the config-entry value as short
            /// </summary>
            /// <returns>Value as short</returns>
            public short ToShort()
            {
                short value;

                if (short.TryParse(_Value, out value))
                {
                    return value;
                }
                else
                {
                    return 0;
                }
            }

            /// <summary>
            /// Returns the config-entry value as ushort
            /// </summary>
            /// <returns>Value as short</returns>
            public ushort ToUShort()
            {
                ushort value;

                if (ushort.TryParse(_Value, out value))
                {
                    return value;
                }
                else
                {
                    return 0;
                }
            }

            /// <summary>
            /// Returns the config-entry value as chararray
            /// </summary>
            /// <returns>Value as chararray</returns>
            public char[] ToCharArray() => _Value.ToCharArray();

            /// <summary>
            /// Returns the config-entry value as DateTime
            /// </summary>
            /// <returns>Value as DateTime</returns>
            public DateTime ToDateTime()
            {
                DateTime value;

                if (DateTime.TryParse(_Value, out value))
                {
                    return value;
                }
                else
                {
                    return new DateTime();
                }
            }

            /// <summary>
            /// Returns the config-entry value as bool
            /// </summary>
            /// <param name="Entry">Config-entry-object</param>
            public static implicit operator bool(ConfigEntry Entry) => Entry.ToBool();

            /// <summary>
            /// Returns the config-entry value as integer
            /// </summary>
            /// <param name="Entry">Config-entry-object</param>
            public static implicit operator int(ConfigEntry Entry) => Entry.ToInt();

            /// <summary>
            /// Returns the config-entry value as string
            /// </summary>
            /// <param name="Entry">Config-entry-object</param>
            public static implicit operator string(ConfigEntry Entry) => Entry.ToString();

            /// <summary>
            /// Returns the config-entry value as short
            /// </summary>
            /// <param name="Entry">Config-entry-object</param>
            public static implicit operator short(ConfigEntry Entry) => Entry.ToShort();

            /// <summary>
            /// Returns the config-entry value as DateTime
            /// </summary>
            /// <param name="Entry">Config-entry-object</param>
            public static implicit operator DateTime(ConfigEntry Entry) => Entry.ToDateTime();

            /// <summary>
            /// Returns the config-entry value as chararray
            /// </summary>
            /// <param name="Entry">Config-entry-object</param>
            public static implicit operator char[] (ConfigEntry Entry) => Entry.ToCharArray();

        }
    }

}
