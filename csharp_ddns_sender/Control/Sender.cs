using csharp_ddns_sender.UI;
using ddns_setting.Control;
using ddns_setting.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static csharp_ddns_sender.Control.YamlProperty;

namespace csharp_ddns_sender.Control
{
    public class Sender
    {
        RecordItem argument;

        public void DoworkLogin(object sender, DoWorkEventArgs e)
        {
            string log = "로그인 시도, ";
            string result ="";
            DateTime time = DateTime.Now;
            try
            {
                string uri = String.Format("{0}&user={1}&auth={2}"
                    , YamlProperty.GetInstance.config.AuthUser
                    , YamlProperty.GetInstance.userData.User
                    , YamlProperty.GetInstance.userData.Auth
                    );
                string responseText = RequestWeb(uri);
                string jsonText = ReadXML(responseText);
                dynamic dict = JsonConvert.DeserializeObject(jsonText);
                result = dict.dnszi.status;
                log += "결과: " + result;
                log += ", 메시지: " + dict.dnszi.msg["#cdata-section"];
            }
            catch (Exception ex)
            {
                Logger.GetInstance.AppendErrorLog(ex);
                log += "결과: 실패, 메시지: 네트워크 오류.";
            }

            MainWindow.main.Dispatcher.BeginInvoke((Action)(() => {
                MainWindow.main.LoginResult(result);
            }));

            LogUpdate(log, time);
        }

        public void DoworkGetUser(object sender, DoWorkEventArgs e)
        {
            string log = "정보 가져오기 시도, ";
            DateTime time = DateTime.Now;
            try
            {
                string uri = String.Format("{0}&user={1}&auth={2}"
                    , YamlProperty.GetInstance.config.GetUser
                    , YamlProperty.GetInstance.userData.User
                    , YamlProperty.GetInstance.userData.Auth
                    );
                string responseText = RequestWeb(uri);
                ReadXMLRegex(responseText, (resultCount)=> {
                    if (-1 != resultCount)
                    {
                        log += "결과: 성공, 메시지: " + resultCount + "개 불러오기 성공.";
                    }
                    else
                    {
                        log += "실패: 성공, 메시지: 파싱 오류.";
                    }
                    LogUpdate(log, time);
                });
            }
            catch (Exception ex)
            {
                Logger.GetInstance.AppendErrorLog(ex);
                log += "결과: 실패, 메시지: 네트워크 오류.";
                LogUpdate(log, time);
            }
        }

        public void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            argument = e.Argument as RecordItem;

            while (true)
            {
                string log = argument.SubDomain + "." + argument.Domain + "을 갱신, ";
                DateTime time = DateTime.Now;
                try
                {
                    string uri = String.Format("{0}&user={1}&auth={2}&domain={3}&record={4}&ft={5}"
                        , YamlProperty.GetInstance.config.SetUser
                        , YamlProperty.GetInstance.userData.User
                        , YamlProperty.GetInstance.userData.Auth
                        , argument.Domain
                        , argument.SubDomain
                        , (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds.ToString()
                        );
                    string responseText = RequestWeb(uri);
                    string jsonText = ReadXML(responseText);
                    dynamic dict = JsonConvert.DeserializeObject(jsonText);
                    log += "결과: " + dict.dnszi.status;
                    log += ", 메시지: " + dict.dnszi.msg["#cdata-section"];
                }
                catch (Exception ex)
                {
                    Logger.GetInstance.AppendErrorLog(ex);
                    log += "결과: 실패, 메시지: 네트워크 오류.";
                }

                LogUpdate(log, time);
                Thread.Sleep(5 * 60 * 1000);
            }
        }

        // Completed Method
        public void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DateTime time = DateTime.Now;
            if (e.Cancelled)
            {
                LogUpdate(argument.SubDomain + "." + argument.Domain + "을 취소하였습니다.", time);
            }
            else if (e.Error != null)
            {
                LogUpdate(argument.SubDomain + "." + argument.Domain + "을 실패하였습니다.", time);
            }
            else
            {
                LogUpdate(argument.SubDomain + "." + argument.Domain + "을 완료하였습니다.", time);
            }
        }

        private void LogUpdate(string message,DateTime time)
        {
            MainWindow.main.LogList.Dispatcher.BeginInvoke((Action)(() => {
                ItemProperty.GetInstance.logItems.Insert(0, new LogItem() { Log = message, Time = time });
                MainWindow.main.LogList.Items.Refresh();
            }));
        }

        private string RequestWeb(string uri)
        {
            WebRequest request = WebRequest.Create(uri);

            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();

            return responseFromServer;
        }

        private string ReadXML(string text)
        {
            string jsonText = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(text);
                jsonText = JsonConvert.SerializeXmlNode(doc);
            }
            catch (IOException ex)
            {
                Logger.GetInstance.AppendErrorLog(ex);
            }

            return jsonText;
        }
        private void ReadXMLRegex(string text, Action<int> act)
        {
            int resultCount = 0; ;
            try
            {
                Regex regex = new Regex(@"<ddns>((.|\n)*?)<\/ddns>");
                MatchCollection regexMatch = regex.Matches(text);
                MainWindow.main.Dispatcher.BeginInvoke((Action)(() => {
                    if (regexMatch != null && regexMatch.Count != 0)
                    {
                        foreach(Match ma in regexMatch)
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(ma.Value);
                            string ddnsText = JsonConvert.SerializeXmlNode(doc);
                            dynamic ddnsObj = JsonConvert.DeserializeObject(ddnsText);

                            string origin = ddnsObj["ddns"]["origin"]["#cdata-section"];
                            foreach (var record in ddnsObj["ddns"]["records"]["record"])
                            {
                                resultCount++;
                                RecordItem item = new RecordItem() { SubDomain = record["#cdata-section"], Domain = origin };
                                ItemProperty.GetInstance.AllOriginItems.Add(item);

                                foreach (Origin chocedItem in YamlProperty.GetInstance.origins.OriginData)
                                {
                                    if (chocedItem.Record.Equals(item.SubDomain) && chocedItem.Domain.Equals(item.Domain))
                                    {
                                        ItemProperty.GetInstance.AllOriginItems.Remove(item);
                                        ItemProperty.GetInstance.ChoiceOriginItems.Add(item);
                                    }
                                }
                            }
                        }
                    }
                    act(resultCount);
                    MainWindow.main.AllOrigin.Items.Refresh();
                    MainWindow.main.ChoiceOrigin.Items.Refresh();
                }));
            }
            catch (IOException ex)
            {
                Logger.GetInstance.AppendErrorLog(ex);
                act(-1);
            }
        }
    }
}
