﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UDTApp.Encryption;

namespace UDTApp.Settings
{
    public class AppSettings
    {
        public AppSettings()
        {
            fileSettings = new List<FileSetting>();
            serverSettings = new List<ServerSetting>();
        }

        static private AppSettings _appSettings = null;
        static public AppSettings appSettings
        {
            get
            {
                if(_appSettings == null)
                {
                    _appSettings = Read();
                }
                return _appSettings;
            }
        }

        private List<FileSetting> _fileSettings = null;
        public List<FileSetting> fileSettings
        {
            get
            {
                return _fileSettings; 
            }
            set { _fileSettings = value; }
        }

        private List<ServerSetting> _serverSettings = null;
        public List<ServerSetting> serverSettings
        {
            get
            {
                return _serverSettings;
            }
            set { _serverSettings = value; }
        }

        public bool findPojectName(string name)
        {
            foreach(FileSetting setting in fileSettings)
            {
                if (Path.GetFileNameWithoutExtension(setting.filePath).ToUpper() == name.ToUpper())
                    return true;
            }
            return false;
        }

        public void addServer(string name, string user, string password)
        {
            //string securePwd = SecureIt.EncryptString(SecureIt.ToSecureString(password)); 
            serverSettings.Add(new ServerSetting { serverName = name, userId = user, pwd = password });
        }

        public ServerSetting getServer(string name)
        {
            return serverSettings.FirstOrDefault(p => p.serverName == name);
        }

        public void remvoeServer(string name)
        {
            ServerSetting sc = serverSettings.FirstOrDefault(p => p.serverName == name);
            if (sc != null)
                serverSettings.Remove(sc);
        }

        public void removeFile(string _filePath)
        {
            FileSetting setting = fileSettings.Find(fs => fs.filePath == _filePath);
            if (setting != null)
            {
                fileSettings.Remove(setting);
            }
        }

        public void addFile(string _filePath)
        {
            FileSetting setting = fileSettings.Find(fs => fs.filePath == _filePath);
            if(setting != null)
            {
                fileSettings.Remove(setting);
                setting.dateTime = DateTime.Now.ToString();
            }
            else
            {
                setting = new FileSetting() { dateTime = DateTime.Now.ToString(), filePath = _filePath };
            }

            autoOpenFile = setting;
            fileSettings.Add(setting);
            fileSettings = fileSettings.OrderByDescending(fs => DateTime.Parse(fs.dateTime)).ToList();
            if(fileSettings.Count > 10)
            {
                fileSettings.RemoveRange(10, fileSettings.Count - 10);
            }
            
        }

        private FileSetting _autoOpenFile = null;
        public FileSetting autoOpenFile
        {
            get { return _autoOpenFile; }
            set { _autoOpenFile = value; }
        }

        private bool _designView = false;
        public bool designView
        {
            get { return _designView; }
            set { _designView = value; }
        }

        static public void Save(AppSettings appSettings, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(AppSettings));
                xmls.Serialize(sw, appSettings);
            }
        }

        static private AppSettings Read()
        {
            using (StreamReader sw = new StreamReader(settingFilePath))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(AppSettings));
                return xmls.Deserialize(sw) as AppSettings;
            }
        }

        public static string settingFilePath
        {
            get
            {
                string tempFolder = Path.GetTempPath();
                tempFolder += "udtSettings";
                if (!Directory.Exists(tempFolder))
                    Directory.CreateDirectory(tempFolder);
                string settingsFile = tempFolder + "\\settings.xml";
                if(!File.Exists(settingsFile))
                {
                    Save(new AppSettings(), settingsFile);
                }
                return settingsFile;
            }
        }
    }
}
