using csharp_ddns_sender.UI;
using ddns_setting.Control;
using ddns_setting.Module;
using ddns_setting.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace csharp_ddns_sender.Control
{
    public class YamlProperty : BaseSingleton<YamlProperty>
    {
        public Config config;
        public UserData userData;
        public Origins origins;
        public YamlProperty()
        {
            config = ParseYaml(@".\config.yml", typeof(Config)) as Config;
            userData = ParseYaml(@".\user.yml", typeof(UserData)) as UserData;
            origins = ParseYaml(@".\origins.yml", typeof(Origins)) as Origins;
        }

        // 선택된 origin 기록
        public void SaveOrigin()
        {
            string filePath = @".\origins.yml";
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(filePath, false))
                {
                    streamWriter.WriteLine(String.Format("originData:"));
                    foreach(RecordItem item in ItemProperty.GetInstance.ChoiceOriginItems)
                    {
                        streamWriter.WriteLine(String.Format("  - domain: '{0}'", item.Domain));
                        streamWriter.WriteLine(String.Format("    record: '{0}'", item.SubDomain));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.GetInstance.AppendErrorLog(ex);
            }
        }

        // 유저 정보 기록
        public void SaveUser()
        {
            string filePath = @".\user.yml";
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(filePath, false))
                {
                    streamWriter.WriteLine(String.Format("user: {0}", YamlProperty.GetInstance.userData.User));
                    streamWriter.WriteLine(String.Format("auth: {0}", YamlProperty.GetInstance.userData.Auth));
                }
            }
            catch (Exception ex)
            {
                Logger.GetInstance.AppendErrorLog(ex);
            }
        }
        // yml 파싱
        private Object ParseYaml(string path, Type type)
        {
            Object order = null;
            try
            {
                string configText = File.ReadAllText(path);
                StringReader input = new StringReader(configText);
                Deserializer deserializer = new DeserializerBuilder()
                    .WithNamingConvention(new CamelCaseNamingConvention())
                    .Build();
                if (type == typeof(Config))
                    order = deserializer.Deserialize<Config>(input);
                if (type == typeof(UserData))
                    order = deserializer.Deserialize<UserData>(input);
                if (type == typeof(Origins))
                    order = deserializer.Deserialize<Origins>(input);
            }
            catch (Exception ex)
            {
                Logger.GetInstance.AppendErrorLog(ex);
                return null;
            }
            return order;
        }
    }
}
