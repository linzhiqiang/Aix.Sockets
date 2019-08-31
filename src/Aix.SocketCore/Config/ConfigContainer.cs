using Aix.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Config
{
    public class ConfigConstant
    {
        public const string Backlog = "Backlog";
        public const string AutoRead = "AutoRead";
        public const string HeartbeatIntervalSecond = "HeartbeatIntervalSecond";
    }
    public class ConfigContainer
    {
        public static ConfigContainer Instance = new ConfigContainer();
        private static Dictionary<string, object> ConfigData = new Dictionary<string, object>();
        private ConfigContainer()
        {

        }

        public ConfigContainer SetConfig(string key, object value)
        {
            ConfigData.Add(key, value);
            return this;
        }


        public int Backlog { get { return ToInt(GetValue(ConfigConstant.Backlog), 10240); } }

        public bool AutoRead { get { return ToBool(GetValue(ConfigConstant.AutoRead), true); } }

        /// <summary>
        /// 心跳 单位秒 
        /// </summary>
        public int HeartbeatIntervalSecond{ get { return ToInt(GetValue(ConfigConstant.HeartbeatIntervalSecond), 0); } }

        private static object GetValue(string key)
        {
            if (ConfigData.TryGetValue(key, out object value))
            {
                return value;
            }
            return null;
        }


        static int ToInt(object value, int defaultValue)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return defaultValue;
            }
            return int.Parse(value.ToString());
        }
        static bool ToBool(object value, bool defaultValue)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return defaultValue;
            }
            return bool.Parse(value.ToString());
        }
    }


}
