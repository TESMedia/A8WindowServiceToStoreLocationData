using A8PlatformNotification.Models;
using Common.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A8PlatformNotification.Common
{
    public class ConnectToDataBase
    {
        public static ILog Log { get; private set; }= LogManager.GetLogger(typeof(ConnectToDataBase));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MacAddress"></param>
        /// <returns></returns>
        public static int CheckMacAdressIsTrackableOrNot(string MacAddress)
        {
            int returnValue = 0;
            using (MySqlConnection myConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                myConnection.Open();
                using (MySqlCommand myCommand = myConnection.CreateCommand())
                {
                    Log.Info(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                    // Start a local transaction
                    myCommand.Connection = myConnection;
                    MySqlDataReader dataReader;
                    try
                    {
                        myCommand.CommandText = "select count(Id) as Count from DeviceAssociateSite as devasssite" + " " +
                                                 "join Device as dev" + " " +
                                                 "on devasssite.DeviceId = dev.DeviceId" + " " +
                                                 "where dev.MacAddress='" + MacAddress + "'" + " " + "and" + " " + "devasssite.IsDeviceRegisterInRtls=" + true;
                        dataReader = myCommand.ExecuteReader();
                        Log.Info("call to the mysql to check the macadress exist or not as per site");
                        while (dataReader.Read())
                        {
                            returnValue = Convert.ToInt32(dataReader["Count"]);
                        }
                    }
                    catch (Exception ex)
                    {
                       Log.Error($"Exception : {ex.Message}");
                    }
                    finally
                    {
                        myConnection.Close();
                    }
                }
                return returnValue;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLocationData"></param>
        public static void InsertLocationDashboard(LocationData objLocationData)
        {
            using (MySqlConnection myConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                myConnection.Open();
                using (MySqlCommand myCommand = myConnection.CreateCommand())
                {
                    // Start a local transaction
                    myCommand.Connection = myConnection;
                    try
                    {
                        myCommand.CommandText = "insert into LocationData (mac,sequence,sn,bn,fn,x,y,z,last_seen_ts,LastSeenDateTime,action,fix_result,AreaName) VALUES('" + objLocationData.mac + "'," + objLocationData.sequence + ",'" + objLocationData.sn + "','" + objLocationData.bn + "','" + objLocationData.fn + "'," + objLocationData.x + "," + objLocationData.y + "," + objLocationData.z + ",'" + objLocationData.last_seen_ts+"','"+ objLocationData.strLastSeenDatetime+"','" + objLocationData.action + "','" + objLocationData.fix_result + "','" + objLocationData.AreaName + "')";
                        myCommand.ExecuteNonQuery();
                        Log.Info("insert into the database");
                    }
                    catch (Exception ex)
                    {
                       Log.Error($"Exception : {ex.Message}");
                    }
                    finally
                    {
                        myConnection.Close();
                    }
                }  
            }
        }
    }
}

