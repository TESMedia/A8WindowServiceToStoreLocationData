using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using A8PlatformNotification.Models;
using Newtonsoft.Json;
using ZeroMQ;
using Common.Logging;
using System.Globalization;

namespace A8PlatformNotification.Common
{
    public class ProcessNotification
    {
        public bool ContinueProcessing { get; set; } = false;
        public ILog Log { get; private set; }

        public ProcessNotification (ILog logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            Log = logger;
        }

        public void StartProcess()
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                var topicName = ConfigurationManager.AppSettings["TopicName"].ToString();
                var fatiNotificationServerUrl = ConfigurationManager.AppSettings["FatiNotificationServerUrl"]
                    .ToString();
                using (var context = new ZContext())
                using (var subscriber = new ZSocket(context, ZSocketType.SUB))
                {
                    //Create the Subscriber Connection
                    subscriber.Connect(fatiNotificationServerUrl);
                    subscriber.Subscribe(topicName);
                    
                    Log.Info($"Subscriber started for Topic with URL : {topicName} {fatiNotificationServerUrl}");
                    ContinueProcessing = true;
                    Log.Info($"Continue Processing Set to : {ContinueProcessing}");

                    while (ContinueProcessing)
                        using (var message = subscriber.ReceiveMessage())
                        {
                            // Read message contents
                            var contents = message[1].ReadString();

                            Log.Info($"Processing record : {contents}");
                            Console.WriteLine(contents);
                            LocationData objLocationData = JsonConvert.DeserializeObject<ListOfArea>(contents).device_notification.records.FirstOrDefault();
                            DateTime macFoundDatetime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local).AddSeconds(objLocationData.last_seen_ts);
                            objLocationData.strLastSeenDatetime = macFoundDatetime.ToString("yyyy-MM-dd HH:mm:ss");
                            objLocationData.AreaName = objLocationData.an != null && objLocationData.an.Length > 0 ? objLocationData.an[0].ToString() : "";
                            Log.Info("Check the Location Data Exist");
                            //if (ConnectToDataBase.CheckMacAdressIsTrackableOrNot(objLocationData.mac) == 1)
                            //{
                                Log.Info("Insert the Location Data");
                                ConnectToDataBase.InsertLocationDashboard(objLocationData);
                            //}
                        }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception : {ex.Message}");
            }
        }
    }
}
