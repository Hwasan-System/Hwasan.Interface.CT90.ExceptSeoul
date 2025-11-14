using System;

namespace CT90
{
    public class ErrMsg
    {
        #region == Fields ==
        private static ErrMsgForm _ErrMsgForm;
        #endregion == Fields ==

        #region == Constants ==
        #endregion == Constants ==

        #region == Properties ==
        #endregion == Properties ==

        #region == Delegates ==
        #endregion == Delegates ==

        #region == Methods ==
        private static string STS(string strStmt)
        {
            return "'" + strStmt.Replace("'", "''") + "'";
        }

        public static void ErrCont(Exception ex)
        {
            ErrCont(ex, "");
        }

        public static void ErrCont(Exception ex, System.Data.OleDb.OleDbCommand odbCmd)
        {
            string strErrSql = "";

            if (odbCmd != null)
            {
                strErrSql = odbCmd.CommandText;

                for (int i = 0; i < odbCmd.Parameters.Count; i++)
                {
                    strErrSql += (i == 0 ? "\r\n\r\n" + "[ Parameter ]" + "\r\n" : "\r\n");

                    switch (odbCmd.Parameters[i].OleDbType)
                    {
                        case System.Data.OleDb.OleDbType.Char:
                        case System.Data.OleDb.OleDbType.VarChar:
                            if (odbCmd.CommandType == System.Data.CommandType.Text)
                            {
                                strErrSql += "[" + i.ToString() + "] : " + (odbCmd.Parameters[i].Value != null ? STS(odbCmd.Parameters[i].Value.ToString()) : "");
                            }
                            else if (odbCmd.CommandType == System.Data.CommandType.StoredProcedure)
                            {
                                strErrSql += odbCmd.Parameters[i].ParameterName + " : " + (odbCmd.Parameters[i].Value != null ? STS(odbCmd.Parameters[i].Value.ToString()) : "");
                            }

                            break;

                        default:
                            if (odbCmd.CommandType == System.Data.CommandType.Text)
                            {
                                strErrSql += "[" + i.ToString() + "] : " + (odbCmd.Parameters[i].Value != null ? odbCmd.Parameters[i].Value.ToString() : "");
                            }
                            else if (odbCmd.CommandType == System.Data.CommandType.StoredProcedure)
                            {
                                strErrSql += odbCmd.Parameters[i].ParameterName + " : " + (odbCmd.Parameters[i].Value != null ? odbCmd.Parameters[i].Value.ToString() : "");
                            }

                            break;
                    }
                }
            }

            if (strErrSql != "")
            {
                strErrSql = "[ Error Sql ]" + "\r\n" + strErrSql;
            }

            ErrCont(ex, strErrSql);
        }

        public static void ErrCont(Exception ex, System.Data.SqlClient.SqlCommand mysCmd)
        {
            string strErrSql = "";

            if (mysCmd != null)
            {
                strErrSql = mysCmd.CommandText;

                for (int i = 0; i < mysCmd.Parameters.Count; i++)
                {
                    strErrSql += (i == 0 ? "\r\n\r\n" + "[ Parameter ]" + "\r\n" : "\r\n");

                    switch (mysCmd.Parameters[i].SqlDbType)
                    {
                        case System.Data.SqlDbType.VarChar:
                            if (mysCmd.CommandType == System.Data.CommandType.Text)
                            {
                                strErrSql += "[" + i.ToString() + "] : " + (mysCmd.Parameters[i].Value != null ? STS(mysCmd.Parameters[i].Value.ToString()) : "");
                            }
                            else if (mysCmd.CommandType == System.Data.CommandType.StoredProcedure)
                            {
                                strErrSql += mysCmd.Parameters[i].ParameterName + " : " + (mysCmd.Parameters[i].Value != null ? STS(mysCmd.Parameters[i].Value.ToString()) : "");
                            }

                            break;
                        default:
                            if (mysCmd.CommandType == System.Data.CommandType.Text)
                            {
                                strErrSql += "[" + i.ToString() + "] : " + (mysCmd.Parameters[i].Value != null ? mysCmd.Parameters[i].Value.ToString() : "");
                            }
                            else if (mysCmd.CommandType == System.Data.CommandType.StoredProcedure)
                            {
                                strErrSql += mysCmd.Parameters[i].ParameterName + " : " + (mysCmd.Parameters[i].Value != null ? mysCmd.Parameters[i].Value.ToString() : "");
                            }

                            break;
                    }
                }
            }

            if (strErrSql != "")
            {
                strErrSql = "[ Error Sql ]" + "\r\n" + strErrSql;
            }

            ErrCont(ex, strErrSql);
        }

        public static void ErrCont(Exception ex, string pErrMsg)
        {
            string strErrMsg = "";

            if (pErrMsg != "")
            {
                if (pErrMsg.IndexOf("[ Error Sql ]") >= 0)
                {
                    strErrMsg = pErrMsg;
                }
                else
                {
                    strErrMsg = "[ Error Message ]" + "\r\n" + pErrMsg;
                }
            }

            if (ex != null)
            {
                strErrMsg += (strErrMsg == "" ? "" : "\r\n\r\n") +
                            "[ Message ]" + "\r\n" + ex.Message + "\r\n\r\n" +
                            "[ Source ]" + "\r\n" + ex.Source + "\r\n\r\n" +
                            "[ Target Site ]" + "\r\n" + ex.TargetSite + "\r\n\r\n" +
                            "[ Stack Trace ]" + "\r\n" + ex.StackTrace + "\r\n\r\n" +
                            "[ InnerException ]" + "\r\n" + ex.InnerException + "\r\n\r\n" +
                            "[ HResult ]" + "\r\n" + ex.HResult;
            }

            if (_ErrMsgForm == null)
            {
                _ErrMsgForm = new ErrMsgForm();
            }
            else
            {
                if (_ErrMsgForm.IsDisposed == true || _ErrMsgForm.CanFocus == false)
                {
                    try
                    {
                        _ErrMsgForm.Dispose();
                    }
                    catch
                    {
                        //
                    }

                    _ErrMsgForm = null;

                    _ErrMsgForm = new ErrMsgForm();
                }
            }

            _ErrMsgForm.txtErrMsg.Text = strErrMsg;
            _ErrMsgForm.txtErrMsg.SelectionStart = 0;
            _ErrMsgForm.Show();
            _ErrMsgForm.Activate();
            System.Windows.Forms.Application.DoEvents();

            DateTime dtmDate = BizData.GetDBSysDate();

            strErrMsg = dtmDate.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\r\n" + ("".PadRight(100, '=')) + "\r\n" + strErrMsg + "\r\n" + ("".PadRight(100, '=')) + "\r\n\r\n";

            //string strDirPath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\ErrorLog";
            string strDirPath = System.Windows.Forms.Application.StartupPath + @"\ErrorLog";

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(strDirPath);
            if (di.Exists == false)
            {
                di.Create();
            }

            string strFileNm = strDirPath + @"\" + dtmDate.ToString("yyyyMMdd") + ".log";

            System.IO.TextWriter tw = new System.IO.StreamWriter(strFileNm, true);
            tw.Write(strErrMsg);
            tw.Close();
        }
        #endregion == Methods ==
    }
}
