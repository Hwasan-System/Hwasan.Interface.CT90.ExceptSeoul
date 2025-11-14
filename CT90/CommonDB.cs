using System;
using System.IO;
using System.Xml;

namespace CT90
{
    public class CommonDB
    {
        private static string TAB = ((char)9).ToString();
        private static string mstrAppPath = Directory.GetCurrentDirectory() + "\\";
        private static string mstrDateTimeFormat = "yyyyMMdd-HH";

        #region == Methods ==

        private static bool DBConnectLiveChk(int intOpt)
        {
            bool blnConnect = false;

            //0 sql
            //1 oracle
            //2 mysql
            if (intOpt == 0)
            {
                if (SysInfo.DataBase.SqlConnection != null)
                {
                    System.Data.SqlClient.SqlCommand mysCmd = new System.Data.SqlClient.SqlCommand();

                    mysCmd.CommandType = System.Data.CommandType.Text;
                    mysCmd.CommandText = "select convert(varchar, getdate(), 120) as datetime";

                    mysCmd.Connection = SysInfo.DataBase.SqlConnection;

                    System.Data.SqlClient.SqlDataAdapter mysAdp = new System.Data.SqlClient.SqlDataAdapter();
                    mysAdp.SelectCommand = mysCmd;

                    System.Data.DataSet dsData = new System.Data.DataSet();

                    try
                    {
                        mysAdp.Fill(dsData);

                        blnConnect = true;
                    }
                    catch
                    {
                        blnConnect = false;
                    }
                }
            }

            if (intOpt == 1)
            {
                if (SysInfo.DataBase.OleDbConnection != null)
                {
                    System.Data.OleDb.OleDbCommand odbCmd = new System.Data.OleDb.OleDbCommand();

                    odbCmd.CommandType = System.Data.CommandType.Text;
                    odbCmd.CommandText = "select convert(varchar, getdate(), 120) as datetime";

                    odbCmd.Connection = SysInfo.DataBase.OleDbConnection;

                    System.Data.OleDb.OleDbDataAdapter odbAdp = new System.Data.OleDb.OleDbDataAdapter();
                    odbAdp.SelectCommand = odbCmd;

                    System.Data.DataSet dsData = new System.Data.DataSet();

                    try
                    {
                        odbAdp.Fill(dsData);

                        blnConnect = true;
                    }
                    catch
                    {
                        blnConnect = false;
                    }
                }
            }

            if (intOpt == 2)
            {
                if (SysInfo.DataBase.MySqlConnection != null)
                {
                    MySql.Data.MySqlClient.MySqlCommand mysCmd = new MySql.Data.MySqlClient.MySqlCommand();

                    mysCmd.CommandType = System.Data.CommandType.Text;
                    mysCmd.CommandText = "select now(6) as datetime";

                    mysCmd.Connection = SysInfo.DataBase.MySqlConnection;

                    MySql.Data.MySqlClient.MySqlDataAdapter mysAdp = new MySql.Data.MySqlClient.MySqlDataAdapter();
                    mysAdp.SelectCommand = mysCmd;

                    System.Data.DataSet dsData = new System.Data.DataSet();

                    try
                    {
                        mysAdp.Fill(dsData);

                        blnConnect = true;
                    }
                    catch
                    {
                        blnConnect = false;
                    }
                }
            }

            return blnConnect;
        }

        public static bool OleDbConnect()
        {
            if (DBConnectLiveChk(1) == true)
            {
                return true;
            }

            //string strFileNm = System.Windows.Forms.Application.StartupPath + @"\Server.xml";

            //System.IO.FileInfo fileinfo = new System.IO.FileInfo(strFileNm);

            //XmlDocument xml = new System.Xml.XmlDocument();

            //if (fileinfo.Exists == false)
            //{
            //	XmlNode xmlIP = xml.CreateElement("IP");
            //	xmlIP.InnerText = "";

            //	XmlNode xmlSvr = xml.CreateElement("SERVER");
            //	xmlSvr.AppendChild(xmlIP);

            //	xml.AppendChild(xmlSvr);

            //	xml.Save(strFileNm);
            //}

            //xml.Load(strFileNm);

            //XmlNode xmlSvrR = xml.SelectNodes("SERVER")[0];
            //string strIP = xmlSvrR.SelectNodes("IP")[0].InnerText;

            string strIP = "";
            string strUserId = "";
            string strPassword = "";

            if (Constant.DEBUG_MODE)
            {
                strIP = "davos";
                strUserId = "davos";
                strPassword = "davos";
            }
            else
            {
                strIP = "emrdb";
                strUserId = "ldi";
                strPassword = "ldi";
            }

            //MSDAORA.1
            //OraOLEDB.Oracle
            string strConnect = string.Format("Provider=MSDAORA.1;Data Source={0};User ID={1};Password={2}", strIP, strUserId, strPassword);

            System.Data.OleDb.OleDbConnection odbCn = new System.Data.OleDb.OleDbConnection();

            odbCn.ConnectionString = strConnect;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                odbCn.Open();
            }
            catch (Exception ex)
            {
                odbCn = null;
                SysInfo.DataBase.OleDbConnection = null;

                //ErrMsg.ErrCont(ex);
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            SysInfo.DataBase.OleDbConnection = odbCn;

            return odbCn == null ? false : true;
        }

        public static bool SqlConnect()
        {
            if (DBConnectLiveChk(0) == true)
            {
                return true;
            }

            if (Constant.DEBUG_MODE) return true;

            string strFileNm = System.Windows.Forms.Application.StartupPath + @"\Server.xml";

            System.IO.FileInfo fileinfo = new System.IO.FileInfo(strFileNm);

            XmlDocument xml = new System.Xml.XmlDocument();

            if (fileinfo.Exists == false)
            {
                XmlNode xmlIP = xml.CreateElement("IP");
                xmlIP.InnerText = "";

                XmlNode xmlSvr = xml.CreateElement("SERVER");
                xmlSvr.AppendChild(xmlIP);

                xml.AppendChild(xmlSvr);

                xml.Save(strFileNm);
            }

            xml.Load(strFileNm);

            XmlNode xmlSvrR = xml.SelectNodes("SERVER")[0];
            string strIP = xmlSvrR.SelectNodes("IP")[0].InnerText;

            strIP = "172.16.1.110,14330";

            if (Constant.DEBUG_MODE) strIP = "127.0.0.1";

            //strIP = "172.16.1.110,14330"
            //strServiceName = "SWLIS_IF"
            //strDatabase = "SWLIS_IF"
            //strUserId = "interface"
            //strPw = "interface"

            string strConnect = string.Format("SERVER={0};DATABASE={1};User ID={2};PASSWORD={3}", strIP, "SWLIS_IF", "interface", @"interface");

            System.Data.SqlClient.SqlConnection mysCn = new System.Data.SqlClient.SqlConnection();
            mysCn.ConnectionString = strConnect;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                mysCn.Open();
            }
            catch (Exception ex)
            {
                mysCn = null;
                SysInfo.DataBase.SqlConnection = null;

                //ErrMsg.ErrCont(ex);
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            SysInfo.DataBase.SqlConnection = mysCn;

            return mysCn == null ? false : true;
        }

        public static bool MySqlConnect()
        {
            if (DBConnectLiveChk(2) == true)
            {
                return true;
            }

            string strFileNm = System.Windows.Forms.Application.StartupPath + @"\Server.xml";

            System.IO.FileInfo fileinfo = new System.IO.FileInfo(strFileNm);

            XmlDocument xml = new System.Xml.XmlDocument();

            if (fileinfo.Exists == false)
            {
                XmlNode xmlIP = xml.CreateElement("IP");
                xmlIP.InnerText = "";

                XmlNode xmlSvr = xml.CreateElement("SERVER");
                xmlSvr.AppendChild(xmlIP);

                xml.AppendChild(xmlSvr);

                xml.Save(strFileNm);
            }

            xml.Load(strFileNm);

            XmlNode xmlSvrR = xml.SelectNodes("SERVER")[0];
            string strIP = xmlSvrR.SelectNodes("IP")[0].InnerText;

            Constant.gstrDatabaseName = "shinwon";

            if (Constant.DEBUG_MODE)
            {
                strIP = "127.0.0.1";
                Constant.gstrDatabaseName = "shinwon";
            }
            else
            {
                if (Constant.TS_ONLY)
                {
                    Constant.gstrDatabaseName = "shinwon_ts10";
                    //Constant.gstrDatabaseName = "shinwon";
                }
            }

            if (Constant.TS_INCLUDE)
            {

                strFileNm = System.Windows.Forms.Application.StartupPath + @"\ServerTS10.xml";
                fileinfo = new System.IO.FileInfo(strFileNm);
                xml = new System.Xml.XmlDocument();
                if (fileinfo.Exists == false)
                {
                    XmlNode xmlIP = xml.CreateElement("IP");
                    xmlIP.InnerText = "";
                    XmlNode xmlSvr = xml.CreateElement("SERVER");
                    xmlSvr.AppendChild(xmlIP);
                    xml.AppendChild(xmlSvr);
                    xml.Save(strFileNm);
                }
                xml.Load(strFileNm);

                xmlSvrR = xml.SelectNodes("SERVER")[0];
                strIP = xmlSvrR.SelectNodes("IP")[0].InnerText;

                Constant.gstrServerIpTS10 = strIP;
                Constant.gstrDatabaseNameTS10 = "shinwon_ts10";

                strIP = "127.0.0.1";
                Constant.gstrDatabaseName = "shinwon";
            }

            Constant.gstrServerIp = strIP;
            string strConnect = "";

            strConnect = string.Format("SERVER={0};DATABASE={1};User ID={2};PASSWORD={3}", strIP, Constant.gstrDatabaseName, "root", @"ghktks\\6736");

            MySql.Data.MySqlClient.MySqlConnection mysCn = new MySql.Data.MySqlClient.MySqlConnection();
            mysCn.ConnectionString = strConnect;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                mysCn.Open();
            }
            catch (Exception ex)
            {
                mysCn = null;
                SysInfo.DataBase.MySqlConnection = null;

                //ErrMsg.ErrCont(ex);
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            SysInfo.DataBase.MySqlConnection = mysCn;

            return mysCn == null ? false : true;
        }

        public static void DBClose()
        {
            try
            {
                if (SysInfo.DataBase.OleDbConnection != null)
                {
                    SysInfo.DataBase.OleDbConnection.Close();
                }
            }
            catch
            {
                //
            }

            try
            {
                if (SysInfo.DataBase.OleDbConnection != null)
                {
                    SysInfo.DataBase.OleDbConnection.Dispose();
                }
            }
            catch
            {
                //
            }

            try
            {
                if (SysInfo.DataBase.SqlConnection != null)
                {
                    SysInfo.DataBase.SqlConnection.Close();
                }
            }
            catch
            {
                //
            }

            try
            {
                if (SysInfo.DataBase.SqlConnection != null)
                {
                    SysInfo.DataBase.SqlConnection.Dispose();
                }
            }
            catch
            {
                //
            }

            try
            {
                if (SysInfo.DataBase.MySqlConnection != null)
                {
                    SysInfo.DataBase.MySqlConnection.Close();
                }
            }
            catch
            {
                //
            }

            try
            {
                if (SysInfo.DataBase.MySqlConnection != null)
                {
                    SysInfo.DataBase.MySqlConnection.Dispose();
                }
            }
            catch
            {
                //
            }

            SysInfo.DataBase.OleDbConnection = null;
            SysInfo.DataBase.SqlConnection = null;
            SysInfo.DataBase.MySqlConnection = null;
        }

        public static System.Data.DataSet DBSelect(System.Data.OleDb.OleDbCommand odbCmd)
        {
            OleDbConnect();

            if (SysInfo.DataBase.OleDbConnection == null)
            {
                return null;
            }

            odbCmd.Connection = SysInfo.DataBase.OleDbConnection;

            System.Data.OleDb.OleDbDataAdapter odbAdp = new System.Data.OleDb.OleDbDataAdapter();
            odbAdp.SelectCommand = odbCmd;

            System.Data.DataSet dsData = new System.Data.DataSet();

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                odbAdp.Fill(dsData);
            }
            catch (Exception ex)
            {
                dsData = null;

                //ErrMsg.ErrCont(ex, odbCmd);
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            odbAdp.Dispose();
            odbAdp = null;

            return dsData;
        }

        public static System.Data.DataSet DBSelect(System.Data.SqlClient.SqlCommand mysCmd)
        {
            SqlConnect();

            if (SysInfo.DataBase.SqlConnection == null)
            {
                return null;
            }

            mysCmd.Connection = SysInfo.DataBase.SqlConnection;

            System.Data.SqlClient.SqlDataAdapter mysAdp = new System.Data.SqlClient.SqlDataAdapter();
            mysAdp.SelectCommand = mysCmd;

            System.Data.DataSet dsData = new System.Data.DataSet();

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                mysAdp.Fill(dsData);
            }
            catch (Exception ex)
            {
                dsData = null;

                //ErrMsg.ErrCont(ex, mysCmd);
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            mysAdp.Dispose();
            mysAdp = null;

            return dsData;
        }

        public static System.Data.DataSet DBSelect(MySql.Data.MySqlClient.MySqlCommand mysCmd)
        {
            MySqlConnect();

            if (SysInfo.DataBase.MySqlConnection == null)
            {
                return null;
            }

            mysCmd.Connection = SysInfo.DataBase.MySqlConnection;

            MySql.Data.MySqlClient.MySqlDataAdapter mysAdp = new MySql.Data.MySqlClient.MySqlDataAdapter();
            mysAdp.SelectCommand = mysCmd;

            System.Data.DataSet dsData = new System.Data.DataSet();

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                mysAdp.Fill(dsData);
            }
            catch (Exception ex)
            {
                dsData = null;

                //ErrMsg.ErrCont(ex, mysCmd);
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            mysAdp.Dispose();
            mysAdp = null;

            return dsData;
        }

        public static int DBNonQuery(System.Data.OleDb.OleDbCommand odbCmd)
        {
            OleDbConnect();

            if (SysInfo.DataBase.OleDbConnection == null)
            {
                return -1;
            }

            System.Data.OleDb.OleDbTransaction odbTrans;
            odbTrans = SysInfo.DataBase.OleDbConnection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

            int intCnt;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                odbCmd.Connection = SysInfo.DataBase.OleDbConnection;
                odbCmd.Transaction = odbTrans;

                intCnt = odbCmd.ExecuteNonQuery();

                odbTrans.Commit();
            }
            catch (Exception ex)
            {
                odbTrans.Rollback();

                intCnt = -1;

                //ErrMsg.ErrCont(ex, odbCmd);
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            odbTrans.Dispose();
            odbTrans = null;

            return intCnt;
        }

        public static int DBNonQuery(System.Data.SqlClient.SqlCommand mysCmd)
        {
            SqlConnect();

            if (SysInfo.DataBase.SqlConnection == null)
            {
                return -1;
            }

            System.Data.SqlClient.SqlTransaction mysTrans;
            mysTrans = SysInfo.DataBase.SqlConnection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

            int intCnt;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                mysCmd.Connection = SysInfo.DataBase.SqlConnection;
                mysCmd.Transaction = mysTrans;

                intCnt = mysCmd.ExecuteNonQuery();

                mysTrans.Commit();
            }
            catch (Exception ex)
            {
                mysTrans.Rollback();

                intCnt = -1;

                //ErrMsg.ErrCont(ex, mysCmd);
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            mysTrans.Dispose();
            mysTrans = null;

            return intCnt;
        }

        public static int DBNonQuery(MySql.Data.MySqlClient.MySqlCommand mysCmd)
        {
            MySqlConnect();

            if (SysInfo.DataBase.MySqlConnection == null)
            {
                return -1;
            }

            MySql.Data.MySqlClient.MySqlTransaction mysTrans;
            mysTrans = SysInfo.DataBase.MySqlConnection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

            int intCnt;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                mysCmd.Connection = SysInfo.DataBase.MySqlConnection;
                mysCmd.Transaction = mysTrans;

                intCnt = mysCmd.ExecuteNonQuery();

                mysTrans.Commit();
            }
            catch (Exception ex)
            {
                mysTrans.Rollback();

                intCnt = -1;

                //ErrMsg.ErrCont(ex, mysCmd);
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            mysTrans.Dispose();
            mysTrans = null;

            return intCnt;
        }

        public static int DBNonQueryMulti(System.Data.OleDb.OleDbCommand[] odbCmd)
        {
            OleDbConnect();

            if (SysInfo.DataBase.OleDbConnection == null)
            {
                return -1;
            }

            System.Data.OleDb.OleDbTransaction odbTrans;
            odbTrans = SysInfo.DataBase.OleDbConnection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

            int intCnt = 0;
            int i = 0;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                for (i = 0; i < odbCmd.Length; i++)
                {
                    odbCmd[i].Connection = SysInfo.DataBase.OleDbConnection;
                    odbCmd[i].Transaction = odbTrans;

                    intCnt = odbCmd[i].ExecuteNonQuery();
                }

                odbTrans.Commit();
            }
            catch (Exception ex)
            {

                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                odbTrans.Rollback();

                intCnt = -1;
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            odbTrans.Dispose();
            odbTrans = null;

            return intCnt;
        }

        public static int DBNonQueryMulti(System.Data.SqlClient.SqlCommand[] mysCmd)
        {
            SqlConnect();

            if (SysInfo.DataBase.SqlConnection == null)
            {
                return -1;
            }

            System.Data.SqlClient.SqlTransaction mysTrans;
            mysTrans = SysInfo.DataBase.SqlConnection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

            int intCnt = 0;
            int i = 0;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                for (i = 0; i < mysCmd.Length; i++)
                {
                    mysCmd[i].Connection = SysInfo.DataBase.SqlConnection;
                    mysCmd[i].Transaction = mysTrans;

                    intCnt = mysCmd[i].ExecuteNonQuery();
                }

                mysTrans.Commit();
            }
            catch (Exception ex)
            {
                mysTrans.Rollback();

                intCnt = -1;

                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            mysTrans.Dispose();
            mysTrans = null;

            return intCnt;
        }

        public static int DBNonQueryMulti(MySql.Data.MySqlClient.MySqlCommand[] mysCmd)
        {
            MySqlConnect();

            if (SysInfo.DataBase.MySqlConnection == null)
            {
                return -1;
            }

            MySql.Data.MySqlClient.MySqlTransaction mysTrans;
            mysTrans = SysInfo.DataBase.MySqlConnection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

            int intCnt = 0;
            int i = 0;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                for (i = 0; i < mysCmd.Length; i++)
                {
                    mysCmd[i].Connection = SysInfo.DataBase.MySqlConnection;
                    mysCmd[i].Transaction = mysTrans;

                    intCnt = mysCmd[i].ExecuteNonQuery();
                }

                mysTrans.Commit();
            }
            catch (Exception ex)
            {
                mysTrans.Rollback();

                intCnt = -1;

                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            mysTrans.Dispose();
            mysTrans = null;

            return intCnt;
        }

        public static int DBNonQueryNoTrans(System.Data.OleDb.OleDbCommand odbCmd)
        {
            if (SysInfo.DataBase.OleDbConnection == null)
            {
                return -1;
            }

            int intCnt;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                odbCmd.Connection = SysInfo.DataBase.OleDbConnection;

                intCnt = odbCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                intCnt = -1;

                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            return intCnt;
        }

        public static int DBNonQueryNoTrans(System.Data.SqlClient.SqlCommand mysCmd)
        {
            if (SysInfo.DataBase.SqlConnection == null)
            {
                return -1;
            }

            int intCnt;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                mysCmd.Connection = SysInfo.DataBase.SqlConnection;

                intCnt = mysCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                intCnt = -1;

                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            return intCnt;
        }

        public static int DBNonQueryNoTrans(MySql.Data.MySqlClient.MySqlCommand mysCmd)
        {
            if (SysInfo.DataBase.MySqlConnection == null)
            {
                return -1;
            }

            int intCnt;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                mysCmd.Connection = SysInfo.DataBase.MySqlConnection;

                intCnt = mysCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                intCnt = -1;

                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            return intCnt;
        }

        public static string DBExecuteScalar(System.Data.OleDb.OleDbCommand odbCmd)
        {
            if (SysInfo.DataBase.OleDbConnection == null)
            {
                return "";
            }

            string strRtn;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                odbCmd.Connection = SysInfo.DataBase.OleDbConnection;
                strRtn = odbCmd.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                strRtn = "";

                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            return strRtn;
        }

        public static string DBExecuteScalar(System.Data.SqlClient.SqlCommand mysCmd)
        {
            if (SysInfo.DataBase.SqlConnection == null)
            {
                return "";
            }

            string strRtn;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                mysCmd.Connection = SysInfo.DataBase.SqlConnection;
                strRtn = mysCmd.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                strRtn = "";

                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            return strRtn;
        }

        public static string DBExecuteScalar(MySql.Data.MySqlClient.MySqlCommand mysCmd)
        {
            if (SysInfo.DataBase.MySqlConnection == null)
            {
                return "";
            }

            string strRtn;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                mysCmd.Connection = SysInfo.DataBase.MySqlConnection;
                strRtn = mysCmd.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                strRtn = "";

                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            return strRtn;
        }

        #endregion == Methods ==
    }
}