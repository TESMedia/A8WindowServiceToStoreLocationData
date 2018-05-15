using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using A8PlatformNotification.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Configuration;

namespace A8PlatformNotification.Common
{
    public class Library
    {
        private int _minCheckConsecutiveShownDiffInSeconds = 5;
        private int _maxCheckConsecutiveShownDiffInSeconds = 15;
        public int SkipEntryNotificationForSeconds;
        public int SkipApproachNotificationForSeconds;
        public string ApproachNotifyEndpoint = null;
        public string EntryNotifyEndpoint = null;
        public List<string> ListOfAreaForNotify = new List<string>();
        private MySqlConnection _con = null;

        public int NotificationSiteId = string.IsNullOrEmpty(ConfigurationManager.AppSettings["SiteId"].ToString()) ? 0 : int.Parse(ConfigurationManager.AppSettings["SiteId"].ToString());

        /// <summary>
        /// Default Constructor 
        /// </summary>
        public Library()
        {
            _con = new MySqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());

            Init();
        }

        void Init()
        {
            try
            {
                _minCheckConsecutiveShownDiffInSeconds = (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MINCheckConsecutiveShownDiffInSeconds"].ToString()) ? int.Parse(ConfigurationManager.AppSettings["MINCheckConsecutiveShownDiffInSeconds"].ToString()) : _minCheckConsecutiveShownDiffInSeconds);
            }
            catch (Exception) { }
            try
            {
                _maxCheckConsecutiveShownDiffInSeconds = (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MAXCheckConsecutiveShownDiffInSeconds"].ToString()) ? int.Parse(ConfigurationManager.AppSettings["MAXCheckConsecutiveShownDiffInSeconds"].ToString()) : _maxCheckConsecutiveShownDiffInSeconds);
            }
            catch (Exception) { }

            ReadConfigFromDb(NotificationSiteId);
        }
        /// <summary>
        /// Update configuration from Database
        /// </summary>
        /// <param name="siteid"></param>
        public void ReadConfigFromDb(int siteid)
        {
            try
            {
                MySqlDataReader rdr = null;
                MySqlCommand selectCommand = new MySqlCommand("SELECT * FROM rtlsconfiguration Where SiteId='" + siteid + "'", _con);
                _con.Open();
                rdr = selectCommand.ExecuteReader();
                while (rdr.Read())
                {
                    SkipApproachNotificationForSeconds = (int)rdr["ApproachNotification"];
                    SkipEntryNotificationForSeconds = (int)rdr["AreaNotification"];

                    #region TODO
                    //TODO- nedd to add provision for Admin to enable this section. Currently in sprint-35 this is not eanabled and need to read from config.
                    //_ApproachNotifyEndpoint = (string)rdr["EndPointUrl"];
                    // _EntryNotifyEndpoint = (string)rdr[""];
                    #endregion
                }
                //Read Area
                rdr.Close();
                rdr = null;
                selectCommand = new MySqlCommand("SELECT GeoFencedAreas FROM rtlsarea Where RtlsConfigurationId='" + siteid + "'", _con);
                rdr = selectCommand.ExecuteReader();
                while (rdr.Read())
                {
                    string val = (string)rdr["GeoFencedAreas"];
                    if (!string.IsNullOrEmpty(val)) ListOfAreaForNotify.Add(val);

                }
                rdr.Close();
                rdr = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _con.Close();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLocationData"></param>
        /// <param name="eventType"></param>
        /// <param name="siteid"></param>
        /// <returns></returns>
        public bool InsertData(LocationData objLocationData, NotificationType eventType, int siteid)
        {
            try
            {
                MySqlCommand insertCommand = new MySqlCommand("INSERT INTO TrackMacNotification(MacAddress,EntryLastVisitDateTime,EntryLastNotifiedDateTime,ApproachLastVisitDateTime,ApproachLastNotifiedDateTime,SiteName,Siteid) VALUES (@mac,@EntryLastVisitDateTime,@EntryLastNotifiedDateTime,@ApproachLastVisitDateTime,@ApproachLastNotifiedDateTime,@SiteName,@Siteid)", _con);
                insertCommand.Parameters.Add(new MySqlParameter("@mac", objLocationData.mac));
                insertCommand.Parameters.Add(
                    new MySqlParameter("@EntryLastVisitDateTime", objLocationData.LastSeenDatetime));

                insertCommand.Parameters.Add(new MySqlParameter("@EntryLastNotifiedDateTime", null));

                insertCommand.Parameters.Add(new MySqlParameter("@ApproachLastVisitDateTime", objLocationData.LastSeenDatetime));

                insertCommand.Parameters.Add(new MySqlParameter("@ApproachLastNotifiedDateTime", null));

                insertCommand.Parameters.Add(new MySqlParameter("@SiteName", objLocationData.sn));
                insertCommand.Parameters.Add(new MySqlParameter("@Siteid", siteid));
                _con.Open();
                Console.WriteLine("Commands executed! Total rows affected are " + insertCommand.ExecuteNonQuery());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _con.Close();
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLocationData"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public bool UpdateLastVisiteDate(LocationData objLocationData, NotificationType eventType)
        {
            try
            {
                switch (eventType)
                {
                    case NotificationType.Approach:
                    {
                        MySqlCommand updateCommand = new MySqlCommand("UPDATE TrackMacNotification SET ApproachLastVisitDateTime = '" + objLocationData.LastSeenDatetime.ToString("yyyy-MM-dd hh:MM:ss") + "' where mac='" + objLocationData.mac + "';", _con);
                        _con.Open();
                        Console.WriteLine("Commands executed! Total rows affected are " + updateCommand.ExecuteNonQuery());
                    }
                        break;
                    case NotificationType.Entry:
                    {
                        MySqlCommand updateCommand = new MySqlCommand("UPDATE TrackMacNotification SET EntryLastVisitDateTime = '" + objLocationData.LastSeenDatetime.ToString("yyyy-MM-dd hh:MM:ss") + "' where mac='" + objLocationData.mac + "';", _con);
                        _con.Open();
                        Console.WriteLine("Commands executed! Total rows affected are " + updateCommand.ExecuteNonQuery());
                    }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _con.Close();
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool UpdateNotificationData(LocationData objLocationData, NotificationType eventType)
        {
            try
            {
                switch (eventType)
                {
                    case NotificationType.Approach:
                    {
                        MySqlCommand updateCommand = new MySqlCommand("UPDATE TrackMacNotification SET ApproachLastVisitDateTime = '" + objLocationData.LastSeenDatetime.ToString("yyyy-MM-dd H:mm:ss") + "', ApproachLastNotifiedDateTime ='" + objLocationData.LastSeenDatetime.ToString("yyyy-MM-dd H:mm:ss") + "' where MacAddress='" + objLocationData.mac + "';", _con);
                        _con.Open();
                        Console.WriteLine("Commands executed! Total rows affected are " + updateCommand.ExecuteNonQuery());
                    }
                        break;
                    case NotificationType.Entry:
                    {
                        MySqlCommand updateCommand = new MySqlCommand("UPDATE TrackMacNotification SET EntryLastVisitDateTime = '" + objLocationData.LastSeenDatetime.ToString("yyyy-MM-dd H:mm:ss") + "', EntryLastNotifiedDateTime ='" + objLocationData.LastSeenDatetime.ToString("yyyy-MM-dd H:mm:ss") + "' where MacAddress='" + objLocationData.mac + "';", _con);
                        _con.Open();
                        Console.WriteLine("Commands executed! Total rows affected are " + updateCommand.ExecuteNonQuery());
                    }
                        break;
                    case NotificationType.All:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _con.Close();
            }
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public int IsMacAddressExist(LocationData objLocationData, int siteId)
        {
            int count = 0;
            try
            {
                #region MyRegion

                //switch (eventType)
                //{
                //    case NotificationType.Approach:
                //    {
                //        MySqlCommand comm = new MySqlCommand("select count(*) from device a, deviceassociatesite b where b.DeviceId = a.DeviceId and  b.IsEntryNotify = 1 and b.SiteId = " + siteId + "  and a.MacAddress='" + objLocationData.mac + "'", _con);
                //        _con.Open();
                //        count = int.Parse(comm.ExecuteScalar().ToString());
                //        }
                //        break;
                //    case NotificationType.Entry:
                //    {
                //        MySqlCommand comm = new MySqlCommand("select count(*) from device a, deviceassociatesite b where b.DeviceId = a.DeviceId and  b.IsTrackByAdmin = 1 and b.SiteId = " + siteId + "  and a.MacAddress='" + objLocationData.mac + "'", _con);
                //        _con.Open();
                //        count = int.Parse(comm.ExecuteScalar().ToString());
                //        }
                //        break;
                //    case NotificationType.All:
                //        break;
                //    default:
                //        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                //}
                #endregion
                MySqlCommand comm = new MySqlCommand("select count(*) from device a, deviceassociatesite b where b.DeviceId = a.DeviceId and  (b.IsEntryNotify = 1 or  b.IsTrackByAdmin = 1) and b.SiteId = " + siteId + "  and a.MacAddress='" + objLocationData.mac + "'", _con);
                _con.Open();
                count = int.Parse(comm.ExecuteScalar().ToString());
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                _con.Close();
            }
            return count;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLocationData"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public NotificationType GetNotificationType(LocationData objLocationData, int siteId)
        {
            int count = 0;
            bool isEntry = false;
            bool isApproach = false;
            NotificationType retVal = NotificationType.Empty;

            try
            {
                MySqlCommand comm =
                    new MySqlCommand(
                        "select count(*) from device a, deviceassociatesite b where b.DeviceId = a.DeviceId and  b.IsEntryNotify = 1 and b.SiteId = " +
                        siteId + "  and a.MacAddress='" + objLocationData.mac + "'", _con);
                _con.Open();
                count = int.Parse(comm.ExecuteScalar().ToString());
                if (count > 0) isApproach = true;
                count = 0;
                _con.Close();
                MySqlCommand comm1 =
                    new MySqlCommand(
                        "select count(*) from device a, deviceassociatesite b where b.DeviceId = a.DeviceId and  b.IsTrackByAdmin = 1 and b.SiteId = " +
                        siteId + "  and a.MacAddress='" + objLocationData.mac + "'", _con);
                _con.Open();
                count = int.Parse(comm1.ExecuteScalar().ToString());
                if (count > 0) isEntry = true;
                if (isEntry && isApproach) retVal = NotificationType.All;
                else
                {
                    if (isEntry) retVal = NotificationType.Entry;
                    if (isApproach) retVal = NotificationType.Approach;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                _con.Close();
            }
            return retVal;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int IsNotificationSentBefore(LocationData objLocationData)
        {
            int countNotification = 0;
            try
            {
                MySqlCommand selectCommand = new MySqlCommand("SELECT COUNT(*) FROM TrackMacNotification Where MacAddress='" + objLocationData.mac + "'", _con);
                _con.Open();
                countNotification = int.Parse(selectCommand.ExecuteScalar().ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _con.Close();
            }
            return countNotification;
        }

        public DateTime LastVisitDateTimeForMacAddress(string mac, NotificationType eventType)
        {
            DateTime notifiedDateTime = DateTime.MinValue;
            try
            {
                switch (eventType)
                {
                    case NotificationType.Approach:
                    {
                        MySqlCommand selectCommand = new MySqlCommand("SELECT ApproachLastVisitDateTime FROM TrackMacNotification Where MacAddress='" + mac + "'", _con);
                        _con.Open();
                        notifiedDateTime = DateTime.Parse(selectCommand.ExecuteScalar().ToString());
                    }
                        break;
                    case NotificationType.Entry:
                    {
                        MySqlCommand selectCommand = new MySqlCommand("SELECT EntryLastVisitDateTime FROM TrackMacNotification Where MacAddress='" + mac + "'", _con);
                        _con.Open();
                        notifiedDateTime = DateTime.Parse(selectCommand.ExecuteScalar().ToString());
                    }
                        break;
                    case NotificationType.All:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _con.Close();
            }
            return notifiedDateTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public DateTime LastNotifiedDateTimeForMacAddress(string mac, NotificationType eventType)
        {
            DateTime notifiedDateTime = DateTime.MinValue;
            try
            {
                switch (eventType)
                {
                    case NotificationType.Approach:
                    {
                        MySqlCommand selectCommand = new MySqlCommand("SELECT ApproachLastNotifiedDateTime FROM TrackMacNotification Where MacAddress='" + mac + "'", _con);
                        _con.Open();
                        string val = selectCommand.ExecuteScalar().ToString();
                        notifiedDateTime = (!string.IsNullOrEmpty(val) ? DateTime.Parse(val) : DateTime.MinValue);
                    }
                        break;
                    case NotificationType.Entry:
                    {
                        MySqlCommand selectCommand = new MySqlCommand("SELECT EntryLastNotifiedDateTime FROM TrackMacNotification Where MacAddress='" + mac + "'", _con);
                        _con.Open();
                        string val = selectCommand.ExecuteScalar().ToString();
                        notifiedDateTime = (!string.IsNullOrEmpty(val) ? DateTime.Parse(val) : DateTime.MinValue);
                    }
                        break;
                    case NotificationType.All:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _con.Close();
            }
            return notifiedDateTime;
        }


        public bool IsAlreadyNotified(string mac, NotificationType eventType)
        {
            bool retVal = false;
            DateTime lastNotified;
            try
            {
                lastNotified = LastNotifiedDateTimeForMacAddress(mac, eventType);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (lastNotified != null && lastNotified != DateTime.MinValue) retVal = true;
            return retVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsSeenAfterConstantMinute(LocationData objLocationData, NotificationType eventType)
        {
            int compareToSkipTime = 0;
            switch (eventType)
            {
                case NotificationType.Entry:
                    compareToSkipTime = SkipEntryNotificationForSeconds;
                    break;
                case NotificationType.Approach:
                    compareToSkipTime = SkipApproachNotificationForSeconds;
                    break;
            }

            try
            {

                DateTime lastVistDateTime = LastVisitDateTimeForMacAddress(objLocationData.mac, eventType);
                Console.WriteLine("Last Vist Date Time - " + lastVistDateTime);
                Console.WriteLine("Seconds Diff" + objLocationData.LastSeenDatetime.Subtract(lastVistDateTime).Seconds);

                if (objLocationData.LastSeenDatetime.Subtract(lastVistDateTime).Seconds >= compareToSkipTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _con.Close();
            }
        }

        /// <summary>
        /// If difference between two datetime is 5s for Particular MacAddress then its a consecutice Data
        /// </summary>
        /// <returns></returns>
        public bool IsConsecutiveStreamDataForMac(LocationData objLocationData, NotificationType eventType)
        {
            DateTime lastVisitDateTime = LastVisitDateTimeForMacAddress(objLocationData.mac, eventType);
            Console.WriteLine("LastVist time - " + lastVisitDateTime.ToString() + "  For MacAddress -" + objLocationData.mac);
            int secDiff = objLocationData.LastSeenDatetime.Subtract(lastVisitDateTime).Seconds;
            Console.WriteLine("Sec Diff - " + secDiff);
            //if (eventType == NotificationType.Entry)
            //{
            //    _maxCheckConsecutiveShownDiffInSeconds = SkipEntryNotificationForSeconds;
            //}else if(eventType == NotificationType.Approach)
            //    _maxCheckConsecutiveShownDiffInSeconds = SkipApproachNotificationForSeconds;

            if (secDiff >= _minCheckConsecutiveShownDiffInSeconds && secDiff <= _maxCheckConsecutiveShownDiffInSeconds)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLocationData"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public bool PostRestCall(LocationData objLocationData, NotificationType eventType)
        {
            Console.WriteLine("Enter into the PostRestCall");
            bool retBoolValue = false;
            objLocationData.PostDateTime = DateTime.Now;
            String resContent = JsonConvert.SerializeObject(objLocationData);
            string targetUrl = "";
            try
            {
                //PostingTime
                using (HttpClient httpClient = new HttpClient())
                {
                    
                    if (eventType == NotificationType.Approach)
                    {
                        targetUrl = ConfigurationManager.AppSettings["EntryNotifyUrl"].ToString();
                    }
                    else if (eventType == NotificationType.Entry)
                    {
                        targetUrl = ConfigurationManager.AppSettings["ApproachNotifyUrl"].ToString();
                    }
                    Console.WriteLine(targetUrl);
                    httpClient.BaseAddress = new Uri(targetUrl);
                    var result = httpClient.PostAsync(httpClient.BaseAddress, new StringContent(resContent, Encoding.UTF8, "application/json")).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Successfully sent to the Member Application");
                        var resultContent = result.Content.ReadAsStringAsync();
                        Notification objNotifications =
                            JsonConvert.DeserializeObject<Notification>(resultContent.Result);
                        Console.WriteLine(objNotifications.result.errmsg);
                        retBoolValue = objNotifications.result.returncode == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured Post client Call :" + targetUrl + Environment.NewLine + resContent);
            }
            return retBoolValue;
        }
    }
}
