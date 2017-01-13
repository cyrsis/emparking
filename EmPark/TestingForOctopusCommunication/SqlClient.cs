using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace TestingForOctopusCommunication
{
    public class SqlClient
    {
        public void SucessfulTransaactionUpdate(string deviceID2, string cardID, int balance, int PollStatus,DateTime transDatetime)
            //Update SQL with Checking on balance
        {
            try
            {
                var sqlConnection =
                    new SqlConnection(
                        ConfigurationManager.ConnectionStrings["Carpark_ClientConnection"].ConnectionString);
                
                    if (sqlConnection.State != ConnectionState.Open)
                    {
                        sqlConnection.Close();
                        sqlConnection.Open();
                    }
                    
                    using (var command = sqlConnection.CreateCommand())
                    {
                        command.CommandTimeout = 80;
                        command.CommandType = CommandType.Text;
                        command.CommandText ="UPDATE HOUR_PARK_OCTOPUS SET DEVICE_ID= '" + deviceID2 + "', OCTOPUS_CARD_NO='" + cardID +
                            "', REMAIN_VALUE=" + (Convert.ToDecimal(balance)/10).ToString("#,##.0") +
                            ", TRANS_DATE_TIME='"+transDatetime.ToString("yyyyMMddHHmmss")+"',TRANS_NO='12345', STATUS_ID = 1 WHERE STATUS_ID = 2";
                        Form1.log.Info("SQL Statment "+command.CommandText);
                        command.ExecuteNonQuery();
                    }
                
               
            }
            catch (Exception e)
            {
                Form1.log.Warn(" SQL have problem with Connection in the Update Statment!!!-- " + e);

            }
        }



        public void ErrorUpdateTable(string deviceID2 = null, string cardID=null, int balance=0, int ErrorStatus=2)
        {


                try
                {
                    var sqlConnection =
                        new SqlConnection(
                            ConfigurationManager.ConnectionStrings["Carpark_ClientConnection"].ConnectionString);
                    {
                        if (sqlConnection.State != ConnectionState.Open)
                        {
                            sqlConnection.Close();
                            sqlConnection.Open();
                        }
                       
                        using (var command = sqlConnection.CreateCommand())
                        {
                            command.CommandTimeout = 55;
                            command.CommandType = CommandType.Text;
                            command.CommandText =
                                "UPDATE HOUR_PARK_OCTOPUS SET DEVICE_ID= '" + deviceID2 + "', OCTOPUS_CARD_NO='" + cardID +
                                "', REMAIN_VALUE=" + (Convert.ToDecimal(balance) / 10).ToString("#,##.0") +
                                ", TRANS_DATE_TIME='12345',TRANS_NO='12345', STATUS_ID= 2 WHERE STATUS_ID= 0";
                            Form1.log.Info("SQL Statment " + command.CommandText);

                            command.ExecuteNonQuery();
                        }
                    }
                   
                }
                catch (Exception e)
                {
                    Form1.log.Warn(" SQL have problem with Connection in the Update Statment!!!-- " + e);

                }
            }

        public void RemovePendingtRecord(string deviceID2 = "", string cardID = "", int balance = 0, int ErrorStatus = 0)
        {
            try
            {
                var sqlConnection =
                    new SqlConnection(
                        ConfigurationManager.ConnectionStrings["Carpark_ClientConnection"].ConnectionString);
                {
                    if (sqlConnection.State != ConnectionState.Open)
                    {
                        sqlConnection.Close();
                        sqlConnection.Open();
                    }

                    using (var command = sqlConnection.CreateCommand())
                    {

                        command.CommandTimeout = 55;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "DELETE FROM HOUR_PARK_OCTOPUS WHERE STATUS_ID = 2";
                        Form1.log.Info("SQL Statment " + command.CommandText);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Form1.log.Warn(" SQL have problem with Connection in the Update Statment!!!-- " + e);

            }
        }
        public void RemoveCurrentRecord(string deviceID2="", string cardID="", int balance=0, int ErrorStatus=0)
        {
            try
            {
                var sqlConnection =
                    new SqlConnection(
                        ConfigurationManager.ConnectionStrings["Carpark_ClientConnection"].ConnectionString);
                {
                    if (sqlConnection.State != ConnectionState.Open)
                    {
                        sqlConnection.Close();
                        sqlConnection.Open();
                    }

                    using (var command = sqlConnection.CreateCommand())
                    {
                        command.CommandTimeout = 55;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "DELETE FROM HOUR_PARK_OCTOPUS WHERE STATUS_ID = 0";
                        Form1.log.Info("SQL Statment " + command.CommandText);

                        command.ExecuteNonQuery();

                        command.CommandTimeout = 55;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "DELETE FROM HOUR_PARK_OCTOPUS WHERE STATUS_ID = 2";
                        Form1.log.Info("SQL Statment " + command.CommandText);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Form1.log.Warn(" SQL have problem with Connection in the Update Statment!!!-- " + e);

            }
        }
         }     
           
       }
