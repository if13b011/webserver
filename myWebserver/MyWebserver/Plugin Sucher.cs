using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace MyWebserver
{
    class Plugin_Sucher
    {
        public  Plugin_Sucher()
        {
            string _pathToPlugins = Environment.CurrentDirectory + "\\Plugins";
            _Plugins = new Dictionary<string, object>();
            LoadPlugins(_pathToPlugins);
        }
        public Dictionary<string, object> GetPlugins()
        {
            return _Plugins;
        }
        static void LoadPlugins(string _pluginPath)
        {
            if (Directory.Exists(_pluginPath))
            {
                string[] _files = Directory.GetFiles(_pluginPath);
                foreach (string _file in _files)
                {
                    FileInfo _fileInfo = new FileInfo(_file);
                    if (_fileInfo.Extension.Equals(".dll"))
                    {
                        Dictionary<string, object> _dictionary = GetModul(_file, typeof(IPlugin.IPlugin));
                        foreach (var _a in _dictionary)
                            _Plugins.Add(_a.Key, _a.Value);
                    }
                }
            }
        }
        static Dictionary<string, object> GetModul(string _fileName, Type _interface)
        {
            Dictionary<string, object> _interfaceInstanzen = new Dictionary<string, object>();
            Assembly _assembly = Assembly.LoadFrom(_fileName);
            foreach (Type _type in _assembly.GetTypes())
                if (_type.IsPublic)
                    if (!_type.IsAbstract)
                    {
                        Type _typeInterface = _type.GetInterface(_interface.ToString(), true);
                        if (_typeInterface != null)
                        {
                            try
                            {
                                object _aktiveInstanz = Activator.CreateInstance(_type);
                                if (_aktiveInstanz != null)
                                    _interfaceInstanzen.Add(_type.Name, _aktiveInstanz);
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine(e);
                            }
                            _typeInterface = null;
                        }

                    }
            _assembly = null;
            return _interfaceInstanzen;
        }

 
        static Dictionary<string, object> _Plugins { get; set; }
    }
}
