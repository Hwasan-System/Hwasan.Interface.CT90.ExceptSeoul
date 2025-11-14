using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static CT90.Constant;

namespace CT90
{
    public partial class Main : MetroFramework.Forms.MetroForm
    {

        private AxWinSck.Server AxWinSckServer;

        TrayManagement trayManager = new TrayManagement("tray_management.db");

        private delegate void RcvTextBoxDelegate(string strData);
        private delegate void SndTextBoxDelegate(string strData);
        private static int BufSize = 16384;
        private byte[] bytBufData = new byte[BufSize];
        private Socket sckServer;
        private Socket sckClient;
        private string BASE_ORD = "N";
        private string DEV_MODE = "N";
        private string ORD_TMR_STOP = "N";
        private string mstrData = "";
        private string mstrUse = "";
        private bool mblnStop = false;
        private bool mblnWait = false;
        private int mintCount = 0;
        private int mintWaitTime = 3;
        private string mstrAppPath = Directory.GetCurrentDirectory() + "\\";
        private string mstrDateTimeFormat = "yyyyMMdd-HH";

        private static string STX = ((char)2).ToString();
        private static string ETX = ((char)3).ToString();
        private static string EOT = ((char)4).ToString();
        private static string ENQ = ((char)5).ToString();
        private static string ACK = ((char)6).ToString();
        private static string TAB = ((char)9).ToString();
        private static string LF = ((char)10).ToString();
        private static string CR = ((char)13).ToString();
        private static string NAK = ((char)21).ToString();
        private static string GS = ((char)29).ToString();
        private static string RS = ((char)30).ToString();

        private Dictionary<string, string> _dicDeltaPanic = new Dictionary<string, string>();
        private Dictionary<string, string> _dicOrdTstCdAtStart = new Dictionary<string, string>();
        private Dictionary<string, string> _dctSetRackInfo = new Dictionary<string, string>();
        private Dictionary<string, string> _dctSortInfo = new Dictionary<string, string>();
        private Dictionary<string, string> _dctEqpTestDtTmByRack = new Dictionary<string, string>();
        private Dictionary<string, string> _dctRetiSpcNo = new Dictionary<string, string>();
        private BackgroundWorker mWorkers;

        public Main()
        {
            InitializeComponent();
        }

        #region BackGroundWorker --------------------
        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                object obj = e.Argument;
                System.ComponentModel.BackgroundWorker worker = (System.ComponentModel.BackgroundWorker)sender;

                switch (obj.GetType().Name)
                {
                    case "String":

                        switch (Common.P(obj.ToString(), DLM_HS, 1))
                        {
                            case "SetRackNoInfo":
                                string[] aryTemp = Common.P(obj.ToString(), DLM_HS, 3).Split('\t');
                                string strRackNo;
                                string strRackPos;
                                string strSpcNo;
                                string strTray;
                                string strBarNo;
                                string strPosSeq;
                                string strSortIdx;
                                string strTrayNo;
                                string strGubun = "A";
                                string strTrayBarNo = "";
                                string strTrayPos = "";
                                string filterExpression;
                                DataRow[] selectedRows;
                                string strOrgTrayBarNo = "";

                                strRackNo = aryTemp[0];
                                strRackPos = aryTemp[1];
                                strSpcNo = aryTemp[2];
                                strTray = aryTemp[3];
                                strBarNo = aryTemp[4];
                                strPosSeq = aryTemp[5];
                                strSortIdx = aryTemp[6];
                                strTrayNo = aryTemp[7];
                                strOrgTrayBarNo = aryTemp[8];

                                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"rackNo: {strRackNo}, rackPos: {strRackPos}, spcNo: {strSpcNo}, Tray: {strTray}, BarNo: {strBarNo}, PosSeq: {strPosSeq}, SortIdx: {strSortIdx}, TrayNo: {strTrayNo}, OrgTrayBarNo: {strOrgTrayBarNo}" + "\r\n",
                                                   false,
                                                   mstrAppPath + "log\\",
                                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                if (strSortIdx != "1") { strGubun = "S"; strTrayBarNo = strTray; }

                                if (DEV_IN_OFFICE == false && Common.IsNumeric(strSpcNo))
                                {
                                    Lis.Interface.clsBizSeeGene objApi;
                                    objApi = new Lis.Interface.clsBizSeeGene();

                                    Lis.Interface.clsParameterCollection Param = new Lis.Interface.clsParameterCollection();

                                    //Y  String	barNo	    바코드번호
                                    //Y  String	devcCd	    장비코드
                                    //Y  String	rsltPrgsCd	상태값(AT: 도착, DV: 분주, Or: 오더, TS: 검사, RT: 결과, SV: 보관)
                                    //Y  String	accDtm	    Insert 일시
                                    //   String  tlaSeqNum	TLA 호기
                                    //   String  trackComm	모듈명
                                    //   String  mdulCd	    모듈코드
                                    //   String  tsGbn	    TS진행상태(Hitachi AQM: Q, RFM: S, OBS: A/ Roche archive:A, seen: S/ Sysmex 분류:S, archive: A)
                                    //   String  rackNo	    Rack No(Rack Table)
                                    //   String  holeNo	    Rack Hole No
                                    //   String  trayNo	    Tray No
                                    //   String  traySeq	Tray 순번
                                    //   String  trayHoleNo	Tray Hole No
                                    //   String  tsGrpNo	TS그룹코드
                                    //   String  tsGrpNm	TS그룹명
                                    //   String  tsErr	    TS에러코드

                                    string strEqpTstDt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    string strSgTray = "";
                                    string strSgRack = "";

                                    Param.Items.Add("barNo", strSpcNo);
                                    Param.Items.Add("devcCd", SG_TS_EQP_CD);

                                    if (strSortIdx != "1")
                                    {
                                        Param.Items.Add("rsltPrgsCd", "SV");
                                        strSgTray = strTrayNo;
                                    }
                                    else
                                    {
                                        strTrayBarNo = strTray;
                                        Param.Items.Add("rsltPrgsCd", "AC");

                                        switch (Constant.SG_TS_EQP_CD)
                                        {
                                            case "620":
                                                strSgTray = "HH";
                                                break;

                                            case "621":
                                                strSgTray = "HG";
                                                break;

                                            case "039":
                                                strSgTray = "BH";
                                                break;

                                            case "922":
                                                strSgTray = "DE";
                                                break;

                                            case "842":
                                                strSgTray = "GE";
                                                break;

                                            case "724":
                                                strSgTray = "JE";
                                                break;

                                            default:
                                                strSgTray = strTrayNo;
                                                break;
                                        }
                                    }

                                    if (strSortIdx == "1")
                                    {
                                        if (strTrayBarNo == null)
                                            strTrayBarNo = "";

                                        if (strTrayBarNo.Length >= 6)
                                        {
                                            strSgRack = strSgTray + strTrayBarNo.Substring(3, 3);
                                        }
                                        else if (strTrayBarNo.Length > 3)
                                        {
                                            string afterPos3 = strTrayBarNo.Substring(3);
                                            strSgRack = strSgTray + afterPos3.PadLeft(3, '0');
                                        }
                                        else
                                        {
                                            string paddedValue = strTrayBarNo.PadLeft(3, '0');
                                            strSgRack = strSgTray + paddedValue;
                                        }
                                    }

                                    #region "+ RackClear API 호출"
                                    //2025-06-11 : AC 이면서 Hole번호 1번일 때 RackClear
                                    if (strPosSeq == "1" && string.IsNullOrEmpty(strSgRack) == false)
                                    {
                                        Dictionary<string, string> dctRackClear = new Dictionary<string, string>();
                                        dctRackClear.Add("cntrCd", Constant.gstrCenterCode);
                                        dctRackClear.Add("devcCd", Constant.SG_TS_EQP_CD);
                                        dctRackClear.Add("rackNo", strSgRack);
                                        string strRet = "";
                                        strRet = objApi.SaveRackClear(dctRackClear);
                                    }
                                    #endregion

                                    Param.Items.Add("accDtm", strEqpTstDt);
                                    Param.Items.Add("tlaSeqNum", "1");
                                    Param.Items.Add("trackComm", "TS10");
                                    Param.Items.Add("mdulCd", "TS10");

                                    //TS진행상태(Hitachi AQM:Q, RFM:S,OBS:A/Roche archive:A,seen:S/Sysmex 분류:S,archive:A)
                                    Param.Items.Add("tsGbn", strGubun);
                                    Param.Items.Add("rackNo", strSgRack);
                                    Param.Items.Add("holeNo", strPosSeq);
                                    Param.Items.Add("trayNo", strSgTray);
                                    Param.Items.Add("traySeq", strTrayNo);
                                    Param.Items.Add("trayHoleNo", strPosSeq);
                                    Param.Items.Add("tsGrpNo", strSortIdx);
                                    Param.Items.Add("tsGrpNm", strOrgTrayBarNo);
                                    Param.Items.Add("tsErr", "");

                                    string strRtn = objApi.SaveSampleTracking(Param);
                                    Param = null;
                                    objApi = null;
                                }

                                //2. 데이터로우 삭제
                                filterExpression = "rackNo = " + Common.STS(strRackNo) + " AND rackPos = " + Common.STS(strRackPos);
                                selectedRows = gdtInquirySpcNoList.Select(filterExpression);

                                foreach (DataRow row in selectedRows)
                                {
                                    Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"delete dataRows rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}" + "\r\n",
                                                       false,
                                                       mstrAppPath + "log\\",
                                                       DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                    row.Delete();
                                }

                                _dctSetRackInfo.Remove(Common.P(obj.ToString(), DLM_HS, 2));

                                break;
                        }

                        break;
                }

                if (worker.CancellationPending == true)
                    e.Cancel = true;
                else
                {
                    // Perform a time consuming operation and report progress.
                    System.Threading.Thread.Sleep(50);
                    worker.ReportProgress(100);
                }
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"WorkerDoWork Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }
        }

        private void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // 에러 처리
            if (e.Error != null)
            {
                //MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                //
            }
            else
            {
                //
                mWorkers = null;
            }
        }

        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //this.progressBar1.Value = e.ProgressPercentage;
        }
        #endregion BackGroundWorker --------------------

        #region Socket --------------------
        private void Tcp_SendData(string strData)
        {
            string strLog = strData;

            if (strLog == ENQ)
            {
                strLog = CR + LF + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + strLog;
            }

            Common.File_Record(strData, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + ".log");

            if (Constant.gblnLoggingTimeStamp == true)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + CR + LF + "[ E < H ]" + CR + LF + strData + "\r\n",
                    false,
                    mstrAppPath + "log\\",
                    DateTime.Now.ToString(mstrDateTimeFormat) + "-SckTimestamp.log");
            }

            AxWinSckServer.Socket_Write_Byte(strData, false);
        }

        private int Message_Send(string strData)
        {
            DateTime objSrtTm;
            TimeSpan objTmSpan;
            int intRet = -1;

            try
            {
                if (AxWinSckServer != null && (bool)AxWinSckServer.IsConnected())
                {
                    objSrtTm = DateTime.Now;
                    mblnWait = true;

                    Tcp_SendData(strData);

                    do
                    {
                        Application.DoEvents();

                        if (AxWinSckServer == null && (bool)AxWinSckServer.IsConnected() == false)
                        {
                            intRet = -1;
                            break;
                        }

                        objTmSpan = DateTime.Now.Subtract(objSrtTm);

                        if (mblnStop || objTmSpan.TotalSeconds >= mintWaitTime)
                        {
                            intRet = 0;
                            break;
                        }
                    }

                    while (mblnWait);

                    if (mblnWait)
                    {
                        mblnWait = false;
                    }
                    else
                    {
                        intRet = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"Tcp_SendData Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            SndTextBoxData(strData);
            return intRet;
        }
        #endregion BackGroundWorker --------------------

        // text box 대리자
        private void RcvTextBoxData(string strData)
        {
            try
            {
                if (txtRcv.InvokeRequired)
                {
                    txtRcv.Invoke(new RcvTextBoxDelegate(RcvTextBoxData), strData);
                }
                else
                {
                    txtRcv.Text = strData;
                }

                if (mtLblRcv.InvokeRequired)
                {
                    mtLblRcv.Invoke(new RcvTextBoxDelegate(RcvTextBoxData), strData);
                }
                else
                {
                    mtLblRcv.Text = strData;
                }
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"RcvTextBoxData Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }
        }

        private void SndTextBoxData(string strData)
        {
            try
            {
                if (txtSnd.InvokeRequired)
                {
                    txtSnd.Invoke(new SndTextBoxDelegate(SndTextBoxData), strData);
                }
                else
                {
                    txtSnd.Text = strData;
                }

                if (mtLblSend.InvokeRequired)
                {
                    mtLblSend.Invoke(new SndTextBoxDelegate(SndTextBoxData), strData);
                }
                else
                {
                    mtLblSend.Text = strData;
                }
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"SndTextBoxData Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }
        }

        private int ASTM_Seq(int intSeq)
        {
            intSeq++;
            if (intSeq > 7) intSeq = 0;
            return intSeq;
        }

        private int OrderSend(string strRackNo)
        {
            int intRet = -1;

            try
            {
                if (mblnStop)
                {
                    mstrUse = "";
                    return -1;
                }

                string strTemp = GetOrderMessage(strRackNo);
                string strInqType = Common.P(strTemp, Constant.DLM_HS, 4);
                string strSortIdxList = Common.P(strTemp, Constant.DLM_HS, 5);
                string strSortSpcNoList = Common.P(strTemp, Constant.DLM_HS, 6);
                string strSpcNoList = Common.P(strTemp, Constant.DLM_HS, 2);
                strTemp = Common.P(strTemp, Constant.DLM_HS, 1);

                if (strTemp != "")
                {
                    string[] aryTemp = strTemp.Split('\n');
                    int j = aryTemp.Length - 1, k = 0;

                    for (int i = 0; i < j; i++)
                    {
                        k++;
                    }

                    if (k > 0 && j == k)
                    {
                        if (mblnStop)
                        {
                            mstrUse = "";
                            return -1;
                        }
                        intRet = Message_Send(EOT);
                        if (intRet < 0) return intRet;

                        string filterExpression;
                        DataRow[] selectedRows;
                        filterExpression = "rackNo = " + Common.STS(strRackNo);
                        selectedRows = gdtInquirySpcNoList.Select(filterExpression);

                        foreach (DataRow row in selectedRows)
                        {
                            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "OrderSend 후 데이터 삭제" + TAB + $"rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}" + "\r\n",
                                false,
                                mstrAppPath + "log\\",
                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                            row.Delete();
                        }
                    }
                    else
                    {
                        intRet = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"OrderSend Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }
            return intRet;
        }

        private int OrderSend_Equip()
        {
            int intRet = -1;

            if (mblnStop)
            {
                mstrUse = "";
                return -1;
            }
            mstrUse = "host";
            mintCount = 0;
            try
            {
                string strTemp = "";
                string[] aryRackNo;

                DataView dataView = gdtInquirySpcNoList.DefaultView;

                try
                {
                    bool blnChkDataView = true;

                    // 데이터 테이블이 null인지 먼저 확인
                    if (dataView?.Table == null)
                    {
                        blnChkDataView = false;
                        Console.WriteLine("DataView 또는 Table이 null입니다");
                    }

                    // 데이터가 있는지 먼저 확인
                    if (dataView.Count == 0)
                    {
                        blnChkDataView = false;
                    }

                    // 안전하게 정렬 및 데이터 추출 시도
                    if (blnChkDataView == true)
                    {
                        dataView.Sort = "inputDtTm ASC";
                        DataTable tempTable = dataView.ToTable();

                        if (tempTable.Rows.Count > 0)
                        {
                            DataRow topRow = tempTable.Rows[0];
                            strTemp = topRow["rackNo"].ToString();
                            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB +
                                              $"OrderSend_Equip rackNo: {topRow["rackNo"]}, rackPos: {topRow["rackPos"]}, spcNo: {topRow["spcNo"]}" + "\r\n",
                                              false,
                                              mstrAppPath + "log\\",
                                              DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 예외 발생 시 간단히 로그만 남기고 계속 진행
                    Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB +
                                      $"OrderSend_Equip Error: {ex.Message}" + "\r\n",
                                      false,
                                      mstrAppPath + "log\\",
                                      DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                    // 원본 코드의 Select 대신 간단하게 Rows 접근
                    try
                    {
                        if (dataView.Count > 0)
                        {
                            DataRow topRow = dataView.ToTable().Rows[0];
                            strTemp = topRow["rackNo"].ToString();
                            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB +
                                              $"OrderSend_Equip (fallback) rackNo: {topRow["rackNo"]}, rackPos: {topRow["rackPos"]}, spcNo: {topRow["spcNo"]}" + "\r\n",
                                              false,
                                              mstrAppPath + "log\\",
                                              DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        }
                    }
                    catch
                    {
                        // 두 번째 시도도 실패하면 무시
                    }
                }

                if (mblnStop)
                {
                    Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "OrderSend_Equip Stop" + "\r\n",
                                       false,
                                       mstrAppPath + "log\\",
                                       DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    mstrUse = "";
                    return -1;
                }

                if (strTemp == "") return intRet;

                aryRackNo = strTemp.Split('^');

                for (int i = 0; i < aryRackNo.Length; i++)
                {
                    intRet = OrderSend(aryRackNo[i]);

                    if (intRet < 1 || mblnStop)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"OrderSend Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            mstrUse = "";
            return intRet;
        }

        private string GetOrderMessage(string strRackNo)
        {
            string strRet = "";
            string strSpcNoList = "";
            bool IsAutoRerun = false;
            string strSortIdxList = "";
            string strSortSpcNoList = "";
            string strInquiryType = "";
            string strUnitNo = "";

            try
            {
                string strDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string strDtm = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                int intSeq = 0, intPat = 0;
                string strSortInfo = "";
                bool IsFirstRequest = false;
                string filterExpression;
                DataRow[] selectedRows;

                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"GetOrderMessage Start" + "\r\n",
                    false,
                    mstrAppPath + "log\\",
                    DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                filterExpression = "rackNo = " + Common.STS(strRackNo);
                selectedRows = gdtInquirySpcNoList.Select(filterExpression);

                foreach (DataRow row in selectedRows)
                {
                    strInquiryType = row["inquiryType"].ToString();
                    strUnitNo = row["unitNo"].ToString();

                    Common.File_Record(TAB + $"rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}, inquiryType: {row["inquiryType"]}, unitNo: {row["unitNo"]}" + "\r\n",
                        false,
                        mstrAppPath + "log\\",
                        DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                }

                //2024-03-20 : Rack 번호별 장비검사일시 딕셔너리에 저장한 것 가져와서 응답일시 세팅
                if (_dctEqpTestDtTmByRack.ContainsKey(strRackNo) == true)
                {
                    strDateTime = _dctEqpTestDtTmByRack[strRackNo].ToString();
                    strDateTime = AddTwoSeconds(strDateTime);
                    _dctEqpTestDtTmByRack.Remove(strRackNo);
                }
                else
                {
                    Common.File_Record(TAB + $"_dctEqpTestDtTmByRack 에 없음 rackNo: {strRackNo}" + "\r\n",
                                       false,
                                       mstrAppPath + "log\\",
                                       DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                }

                if (strInquiryType == "B")
                {
                    IsFirstRequest = true;
                }
                else
                {
                    IsFirstRequest = false;
                }

                string strLog;

                strLog = "";
                strLog += $"{TAB}RackNo: " + strRackNo + "\r\n";
                strLog += $"{TAB}0. IsFirstRequest: " + IsFirstRequest.ToString() + "\r\n";
                strLog += $"{TAB}1. strInquryType: " + strInquiryType + "\r\n";

                DataSet dsTemp = ConvertToDataSet(selectedRows, "CT90");

                if (dsTemp.Tables.Count > 0)
                {
                    int intRet = -1;
                    if (mblnStop)
                    {

                        Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"GetOrderMessage End, 장비 데이터 수신 먼저 처리로 중지" + "\r\n",
                                           false,
                                           mstrAppPath + "log\\",
                                           DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                        mstrUse = "";
                        return "";
                    }

                    intRet = Message_Send(ENQ);

                    if (intRet < 0)
                    {

                        Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"GetOrderMessage End, ENQ 충돌로 중지" + "\r\n",
                                                                   false,
                                                                   mstrAppPath + "log\\",
                                                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                        return "";
                    }

                    string strTemp;
                    intSeq = ASTM_Seq(intSeq);
                    strTemp = intSeq.ToString() + "H|\\^&|||||||||||E1394-97|" + strDateTime;

                    intRet = Message_Send(STX + strTemp + CR + ETX + Common.CheckSum(strTemp + CR + ETX) + CR + LF);

                    strRet = strTemp;

                    bool IsPB = false;
                    bool IsFlagOfSlideMake = false;
                    bool IsFirstOrderOnCT90 = true;
                    bool IsGoingToSP = true;
                    bool IsMatchedSlideMakingRules = false;  //2020-02-22 : 씨젠 대구검사센터 검사결과 + 플래그에 따른 슬라이드 메이킹 여부 확인
                    string strSortIndex = "0";
                    string strAutoRerunParmeter = "";
                    string strNoCycle = "";

                    if (IsFirstRequest == false)
                    {
                        foreach (DataRow drTemp in dsTemp.Tables[0].Rows)
                        {
                            if (strSpcNoList == "")
                            {
                                strSpcNoList = Common.STS(drTemp["spcNo"].ToString());
                            }
                            else
                            {
                                strSpcNoList = strSpcNoList + "," + Common.STS(drTemp["spcNo"].ToString());
                            }
                        }

                        strLog += $"{TAB}2. IsFirstOrderOnCT90: " + IsFirstOrderOnCT90.ToString() + "\r\n";
                        strLog += $"{TAB}3. SpcNoList: " + strSpcNoList + "\r\n";

                        if (IsFirstOrderOnCT90 == false)
                        {
                            IsPB = false;

                            strAutoRerunParmeter = "";

                            strLog += $"{TAB}4. AutoRerunParmeter: " + strAutoRerunParmeter + "\r\n";

                            if (strAutoRerunParmeter == "")
                            {
                                IsAutoRerun = false;
                            }
                            else
                            {
                                IsAutoRerun = true;
                            }

                            if (IsPB == false)
                            {
                                IsMatchedSlideMakingRules = false;
                            }

                            strLog += $"{TAB}5. IsAutoRerun: " + IsAutoRerun.ToString() + "\r\n";
                            if (IsPB == false && IsFlagOfSlideMake == false) { IsGoingToSP = false; }
                            if (IsPB == false && IsMatchedSlideMakingRules == false) { IsGoingToSP = false; }
                            strLog += $"{TAB}6. IsGoingToSP: " + IsPB.ToString() + " " + IsMatchedSlideMakingRules.ToString() + IsGoingToSP.ToString() + "\r\n";
                        }
                    }

                    bool IsReti = false;
                    foreach (DataRow drTemp in dsTemp.Tables[0].Rows)
                    {
                        if (drTemp["ordInfo"].ToString() == "N")
                        {
                        }
                        else
                        {
                            string[] aryTemp = drTemp["ordInfo"].ToString().Split('^');
                            for (int i = 0; i < aryTemp.Length; i++)
                            {
                                if (aryTemp[i] != "")
                                {
                                    if (aryTemp[i].IndexOf("RET") > -1)
                                    {
                                        IsReti = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (IsReti == true) break;
                    }

                    List<string> lstReti = new List<string>();

                    foreach (DataRow drTemp in dsTemp.Tables[0].Rows)
                    {
                        string strTstId = "", strRptType = "Y";

                        if (drTemp["ordInfo"].ToString() == "N")
                        {
                            IsGoingToSP = false;
                            strTstId = "";
                            strRptType = "Y";
                            strLog += $"{TAB}7. No Order: " + drTemp["spcNo"].ToString() + "\r\n";
                        }
                        else
                        {
                            strRptType = "Q";

                            if (IsFirstRequest == true)
                            {
                                string[] aryTemp = drTemp["ordInfo"].ToString().Split('^');
                                for (int i = 0; i < aryTemp.Length; i++)
                                {
                                    if (aryTemp[i] != "")
                                    {
                                        if (strTstId == "")
                                        {
                                            strTstId = "^^^^" + aryTemp[i];
                                        }
                                        else
                                        {
                                            strTstId += "\\^^^^" + aryTemp[i];
                                        }

                                        if (String.IsNullOrEmpty(aryTemp[i]) == false && aryTemp[i] == "RET" && lstReti.Contains(drTemp["spcNo"].ToString()) == false)
                                        {
                                            lstReti.Add(drTemp["spcNo"].ToString());
                                        }

                                    }
                                }

                                strLog += $"{TAB}8. Order: " + drTemp["spcNo"].ToString() + "\t" + strTstId + "\t" + strRptType + "\r\n";
                            }
                            else
                            {
                                if (IsAutoRerun)
                                {
                                    strTstId = strAutoRerunParmeter;
                                }
                                else
                                {
                                    if (IsGoingToSP == true)
                                    {
                                        strTstId = "^^^^SP";

                                        if (Constant.SG_TS_EQP_CD == "621")
                                        {
                                            strTstId = "";
                                        }
                                    }
                                    else
                                    {
                                        strTstId = "";
                                        strRptType = "Y";
                                    }
                                }

                                strLog += $"{TAB}9. IsAutoRerun: " + IsAutoRerun.ToString() + "\r\n";
                                strLog += $"{TAB}10. Order: " + drTemp["spcNo"].ToString() + "\t" + strTstId + "\t" + strRptType + "\r\n";
                            }
                        }

                        switch (strInquiryType)
                        {
                            case "B":
                                strSortInfo = "0^^";

                                if (lstReti.Contains(drTemp["spcNo"].ToString()) == true)
                                {
                                    if (_dctRetiSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == false)
                                    {
                                        _dctRetiSpcNo.Add(drTemp["spcNo"].ToString(), "^^^^RET");
                                    }
                                }

                                break;

                            case "C":
                                strSortInfo = "0^^";
                                strTstId = "";
                                strRptType = "Y";

                                if (_dctRetiSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == false)
                                {
                                    Lis.Interface.clsBizSeeGene objApi;
                                    objApi = new Lis.Interface.clsBizSeeGene();
                                    Lis.Interface.clsParameterCollection Param = new Lis.Interface.clsParameterCollection();
                                    if (Param != null && Param.Items != null)
                                    {
                                        Param.Items.Add("EQP_CD", SG_TS_EQP_CD);
                                        Param.Items.Add("SPC_NO", drTemp["spcNo"].ToString());
                                        Param.Items.Add("RECP_STUS", "P");
                                        Param.Items.Add("CHK_ALL_YN", "N");

                                        if (objApi != null)
                                        {
                                            dsTemp = objApi.GetSpcInfo(Param);
                                        }
                                        Param = null;
                                    }

                                    if (dsTemp == null || dsTemp.Tables.Count == 0)
                                    {

                                    }
                                    else
                                    {
                                        filterExpression = "LIS_TST_CD = '11310' ";
                                        if (dsTemp.Tables.Count > 0 && dsTemp.Tables[0] != null)
                                        {
                                            selectedRows = dsTemp.Tables[0].Select(filterExpression);
                                            if (selectedRows != null && selectedRows.Length > 0)
                                            {
                                                if (Constant.SG_TS_EQP_CD == "842" || Constant.SG_TS_EQP_CD == "039")
                                                {
                                                    if (glstNoOrdByPB.Contains(drTemp["spcNo"].ToString()) == true)
                                                    {
                                                        //Pass
                                                        strLog += $"{TAB}10.1 RET 검사있지만 PB도 있어서 No오더처리 " + drTemp["spcNo"].ToString() + "\t" + strTstId + "\t" + strRptType + "\r\n";
                                                    }
                                                    else
                                                    {
                                                        _dctRetiSpcNo.Add(drTemp["spcNo"].ToString(), "^^^^RET");
                                                    }
                                                }
                                                else
                                                {
                                                    _dctRetiSpcNo.Add(drTemp["spcNo"].ToString(), "^^^^RET");
                                                }
                                            }
                                        }
                                    }
                                }

                                if (_dctRetiSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == true)
                                {
                                    switch (Constant.SG_TS_EQP_CD)
                                    {
                                        case "620":

                                            //4,5 번이 RETI
                                            if (strUnitNo == "03")
                                            {
                                                strTstId = "^^^^RET";
                                                strRptType = "Q";
                                                strLog += $"{TAB}10.1 RET 검사있어서 다시 RET만 오더처리 " + drTemp["spcNo"].ToString() + "\t" + strTstId + "\t" + strRptType + "\r\n";
                                            }

                                            break;

                                        case "621":

                                            //9,10 번이 RETI
                                            if (strUnitNo == "08")
                                            {
                                                strTstId = "^^^^RET";
                                                strRptType = "Q";
                                                strLog += $"{TAB}10.1 RET 검사있어서 다시 RET만 오더처리 " + drTemp["spcNo"].ToString() + "\t" + strTstId + "\t" + strRptType + "\r\n";
                                            }

                                            break;

                                        case "039":

                                            //부산
                                            //5,6 번이 RETI
                                            if (strUnitNo == "04")
                                            {
                                                strTstId = "^^^^RET";
                                                strRptType = "Q";
                                                strLog += $"{TAB}10.1 RET 검사있어서 다시 RET만 오더처리 " + drTemp["spcNo"].ToString() + "\t" + strTstId + "\t" + strRptType + "\r\n";
                                            }

                                            break;

                                        case "922":

                                            //대구
                                            //5번이 레티
                                            if (strUnitNo == "02" || strUnitNo == "03" || strUnitNo == "04")
                                            {
                                                strTstId = "^^^^RET";
                                                strRptType = "Q";
                                                strLog += $"{TAB}10.1 RET 검사있어서 다시 RET만 오더처리 " + drTemp["spcNo"].ToString() + "\t" + strTstId + "\t" + strRptType + "\r\n";
                                            }

                                            break;

                                        case "842":
                                            //광주
                                            //4,5 번이 RETI
                                            if (strUnitNo == "03")
                                            {
                                                strTstId = "^^^^RET";
                                                strRptType = "Q";
                                                strLog += $"{TAB}10.1 RET 검사있어서 다시 RET만 오더처리 " + drTemp["spcNo"].ToString() + "\t" + strTstId + "\t" + strRptType + "\r\n";
                                            }

                                            break;

                                        //case "724":

                                        //    //대전
                                        //    //5 번이 RETI
                                        //    if (strUnitNo == "03" || strUnitNo == "04")
                                        //    {
                                        //        strTstId = "^^^^RET";
                                        //        strRptType = "Q";
                                        //        strLog += " 10.1 RET 검사있어서 다시 RET만 오더처리 " + drTemp["spcNo"].ToString() + "\t" + strTstId + "\t" + strRptType + "\r\n";
                                        //    }

                                        //    break;

                                        default:
                                            strTstId = "";
                                            break;
                                    }
                                }

                                if (Constant.SG_TS_EQP_CD == "724")
                                {
                                    DataSet dsRsltXN;
                                    bool blnSP = false;
                                    dsRsltXN = BizData.GetRsltHematology(drTemp["spcNo"].ToString());
                                    if (dsRsltXN != null && dsRsltXN.Tables.Count > 0 && dsRsltXN.Tables[0].Rows.Count > 0)
                                    {
                                        filterExpression = "slid1JgmtVal in ('S1', 'S2')";
                                        selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                                        if (selectedRows != null && selectedRows.Length > 0)
                                        {
                                            foreach (DataRow row in selectedRows)
                                            {
                                                blnSP = true;
                                                break;
                                            }
                                        }

                                        //2025-04-29 : devcFlagCont 에 F 가 있을 경우 SP 오더처리
                                        if (blnSP == false)
                                        {
                                            filterExpression = "devcFlagCont LIKE '%F%'";
                                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                                            if (selectedRows != null && selectedRows.Length > 0)
                                            {
                                                foreach (DataRow row in selectedRows)
                                                {
                                                    blnSP = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (blnSP == false)
                                    {
                                        if (Common.gdctPBSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == true)
                                        {
                                            blnSP = true;
                                        }
                                    }

                                    if (blnSP == true)
                                    {
                                        strRptType = "Q";

                                        if (strTstId == "")
                                        {
                                            strTstId = "^^^^SP";
                                        }
                                        else
                                        {
                                            strTstId = strTstId + "\\^^^^SP";
                                        }
                                    }
                                }

                                break;

                            case "SI":

                                if (Constant.SG_TS_EQP_CD != "621" && _dctRetiSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == true)
                                {
                                    _dctRetiSpcNo.Remove(drTemp["spcNo"].ToString());
                                }

                                if (Common.gdctPBSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == true)
                                {
                                    Common.gdctPBSpcNo.Remove(drTemp["spcNo"].ToString());
                                }

                                if (glstNoOrdByPB.Contains(drTemp["spcNo"].ToString()) == true)
                                {
                                    glstNoOrdByPB.Remove(drTemp["spcNo"].ToString());
                                }

                                strSortIndex = "";

                                strUnitNo = drTemp["unitNo"].ToString();
                                strNoCycle = drTemp["sortInfo"].ToString();

                                // 마지막 문자가 3 또는 5인지 확인
                                string lastDigit = drTemp["spcNo"].ToString().Substring(drTemp["spcNo"].ToString().Length - 1);

                                if (Constant.SG_TS_EQP_CD == "724" && lastDigit == "6") { lastDigit = "5"; }
                                if (Constant.SG_TS_EQP_CD == "039" && lastDigit == "6") { lastDigit = "5"; }

                                if (lastDigit == "3" || lastDigit == "5")
                                {
                                    switch (Constant.SG_TS_EQP_CD)
                                    {
                                        case "620":
                                            strSortIndex = GetSortIndex_SeeGene_Seoul(drTemp["spcNo"].ToString(), drTemp["rackNo"].ToString());
                                            break;

                                        case "621":
                                            strSortIndex = GetSortIndex_SeeGene_Seoul(drTemp["spcNo"].ToString(), drTemp["rackNo"].ToString());
                                            break;

                                        case "039":
                                            strSortIndex = GetSortIndex_SeeGene_Busan(drTemp["spcNo"].ToString(), drTemp["rackNo"].ToString());
                                            break;

                                        case "922":
                                            strSortIndex = GetSortIndex_SeeGene_Daegu(drTemp["spcNo"].ToString(), drTemp["rackNo"].ToString());
                                            break;

                                        case "842":
                                            strSortIndex = GetSortIndex_SeeGene_Gwangju(drTemp["spcNo"].ToString(), drTemp["rackNo"].ToString());
                                            break;

                                        case "724":
                                            strSortIndex = GetSortIndex_SeeGene_Daejeon(drTemp["spcNo"].ToString(), drTemp["rackNo"].ToString());
                                            break;

                                        default:
                                            strSortIndex = "61";
                                            break;
                                    }
                                }
                                else
                                {
                                    string strSortDesc = "";

                                    #region ----- 2. 아카이브 모드
                                    if (chkArchive.Checked == true)
                                    {
                                        strSortIndex = "1";
                                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                        strLog = $"SpcNo: {drTemp["spcNo"].ToString()}, RackNo: {strRackNo}, Archive Mode, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                    }
                                    else
                                    {
                                        //무조건 에러
                                        switch (Constant.SG_TS_EQP_CD)
                                        {
                                            case "620":
                                                strSortIndex = "24";
                                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                                strLog = $"{TAB}SpcNo: {drTemp["spcNo"].ToString()}, RackNo: {strRackNo}, 데이터에러, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                                break;

                                            case "621":
                                                strSortIndex = "24";
                                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                                strLog = $"{TAB}SpcNo: {drTemp["spcNo"].ToString()}, RackNo: {strRackNo}, 데이터에러, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                                break;

                                            case "039":
                                                strSortIndex = "2";
                                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                                strLog = $"{TAB}SpcNo: {drTemp["spcNo"].ToString()}, RackNo: {strRackNo}, 데이터에러, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                                break;

                                            case "922":
                                                strSortIndex = "2";
                                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                                strLog = $"{TAB}SpcNo: {drTemp["spcNo"].ToString()}, RackNo: {strRackNo}, 데이터에러, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                                break;

                                            case "842":
                                                strSortIndex = "2";
                                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                                strLog = $"{TAB}SpcNo: {drTemp["spcNo"].ToString()}, RackNo: {strRackNo}, 데이터에러, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                                break;

                                            case "724":
                                                strSortIndex = "61";
                                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                                strLog = $"{TAB}SpcNo: {drTemp["spcNo"].ToString()}, RackNo: {strRackNo}, 데이터에러, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                                break;

                                            default:
                                                strSortIndex = "61";
                                                break;
                                        }
                                    }
                                    #endregion ----- 2. 아카이브 모드
                                }

                                if (strSortIndex == "0")
                                {

                                    if (Constant.SG_TS_EQP_CD == "621")
                                    {
                                        //2024-08-18 : 장비세팅이 역방향으로 되어 있어서 CBC+DIFF 오더처리 해야함.

                                        if (lstReti.Contains(drTemp["spcNo"].ToString()) == true)
                                        {
                                            if (_dctRetiSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == false)
                                            {
                                                _dctRetiSpcNo.Add(drTemp["spcNo"].ToString(), "^^^^RET");
                                            }
                                        }

                                        strRptType = "Q";

                                        string[] aryTemp = drTemp["ordInfo"].ToString().Split('^');
                                        for (int i = 0; i < aryTemp.Length; i++)
                                        {
                                            if (aryTemp[i] != "")
                                            {
                                                if (strTstId == "")
                                                {
                                                    strTstId = "^^^^" + aryTemp[i];
                                                }
                                                else
                                                {
                                                    strTstId += "\\^^^^" + aryTemp[i];
                                                }

                                                if (String.IsNullOrEmpty(aryTemp[i]) == false && aryTemp[i] == "RET" && lstReti.Contains(drTemp["spcNo"].ToString()) == false)
                                                {
                                                    lstReti.Add(drTemp["spcNo"].ToString());
                                                }

                                            }
                                        }

                                        strLog += $"{TAB}11. Order: " + drTemp["spcNo"].ToString() + "\t" + strTstId + "\t" + strRptType + "\r\n";
                                        strSortInfo = drTemp["spcNo"].ToString() + "^" + Common.P(strNoCycle, "^", 2) + "^" + Common.P(strNoCycle, "^", 1);
                                    }
                                    else
                                    {
                                        strSortInfo = "0" + "^" + Common.P(strNoCycle, "^", 2) + "^" + Common.P(strNoCycle, "^", 1);
                                    }

                                }
                                else
                                {
                                    strSortInfo = strSortIndex + "^" + Common.P(strNoCycle, "^", 2) + "^" + Common.P(strNoCycle, "^", 1);
                                }

                                if (strSortIdxList == "")
                                {
                                    strSortIdxList = strSortIndex;
                                    strSortSpcNoList = drTemp["spcNo"].ToString();
                                }
                                else
                                {
                                    strSortIdxList = strSortIdxList + "|" + strSortIndex;
                                    strSortSpcNoList = strSortSpcNoList + "|" + drTemp["spcNo"].ToString();
                                }

                                break;

                            case "SO":

                                strUnitNo = drTemp["unitNo"].ToString();
                                strNoCycle = drTemp["sortInfo"].ToString();

                                if (Common.gdctPBSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == true)
                                {
                                    Common.gdctPBSpcNo.Remove(drTemp["spcNo"].ToString());
                                }

                                if (glstNoOrdByPB.Contains(drTemp["spcNo"].ToString()) == true)
                                {
                                    glstNoOrdByPB.Remove(drTemp["spcNo"].ToString());
                                }

                                if (Constant.SG_TS_EQP_CD == "621")
                                {
                                    //2024-08-18 : 장비세팅이 역방향으로 되어 있어서 CBC+DIFF 오더처리 해야함.
                                    //lstReti.Add("507111050453");
                                    if (lstReti.Contains(drTemp["spcNo"].ToString()) == true)
                                    {
                                        if (_dctRetiSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == false)
                                        {
                                            _dctRetiSpcNo.Add(drTemp["spcNo"].ToString(), "^^^^RET");
                                        }
                                    }

                                    strRptType = "Q";

                                    string[] aryTemp = drTemp["ordInfo"].ToString().Split('^');
                                    for (int i = 0; i < aryTemp.Length; i++)
                                    {
                                        if (aryTemp[i] != "")
                                        {
                                            if (strTstId == "")
                                            {
                                                strTstId = "^^^^" + aryTemp[i];
                                            }
                                            else
                                            {
                                                strTstId += "\\^^^^" + aryTemp[i];
                                            }

                                            if (String.IsNullOrEmpty(aryTemp[i]) == false && aryTemp[i] == "RET" && lstReti.Contains(drTemp["spcNo"].ToString()) == false)
                                            {
                                                lstReti.Add(drTemp["spcNo"].ToString());
                                            }

                                        }
                                    }

                                    strLog += $"{TAB}11. Order: " + drTemp["spcNo"].ToString() + "\t" + strTstId + "\t" + strRptType + "\r\n";
                                    strSortInfo = drTemp["spcNo"].ToString() + "^" + Common.P(strNoCycle, "^", 2) + "^" + Common.P(strNoCycle, "^", 1);
                                }
                                else
                                {
                                    if (_dctRetiSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == true)
                                    {
                                        _dctRetiSpcNo.Remove(drTemp["spcNo"].ToString());
                                    }
                                    strSortInfo = "0" + "^" + Common.P(strNoCycle, "^", 2) + "^" + Common.P(strNoCycle, "^", 1);
                                }

                                //strSortInfo = "0" + "^" + Common.P(strNoCycle, "^", 2) + "^" + Common.P(strNoCycle, "^", 1);
                                break;

                            default:

                                if (_dctRetiSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == true)
                                {
                                    _dctRetiSpcNo.Remove(drTemp["spcNo"].ToString());
                                }

                                if (Common.gdctPBSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == true)
                                {
                                    Common.gdctPBSpcNo.Remove(drTemp["spcNo"].ToString());
                                }

                                if (glstNoOrdByPB.Contains(drTemp["spcNo"].ToString()) == true)
                                {
                                    glstNoOrdByPB.Remove(drTemp["spcNo"].ToString());
                                }

                                strSortInfo = "0^^";
                                break;
                        }

                        if (Constant.SG_TS_EQP_CD == "621" && Common.Val(strUnitNo) <= 8)
                        {
                            if (strTstId.IndexOf("RET") > -1)
                            {
                                if (_dctRetiSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == false)
                                {
                                    _dctRetiSpcNo.Add(drTemp["spcNo"].ToString(), "^^^^RET");
                                }
                            }

                            if (strUnitNo == "08" && _dctRetiSpcNo.ContainsKey(drTemp["spcNo"].ToString()) == true)
                            {
                                strTstId = "^^^^RET";
                                strRptType = "Q";
                                strLog += $"{TAB}11.0 RET 검사있어서 다시 RET만 오더처리 " + drTemp["spcNo"].ToString() + "\t" + strTstId + "\t" + strRptType + "\r\n";
                            }
                        }

                        strLog += $"{TAB}11.0 EqpCd: {Constant.SG_TS_EQP_CD}, unitNo: {strUnitNo}, Ret: {_dctRetiSpcNo.ContainsKey(drTemp["spcNo"].ToString())}, spcNo: {drTemp["spcNo"]}, tstId: {strTstId}, RptType: {strRptType}" + "\r\n";

                        intPat++;
                        intSeq = ASTM_Seq(intSeq);
                        strTemp = intSeq.ToString() + "P|" + intPat.ToString();
                        intRet = Message_Send(STX + strTemp + CR + ETX + Common.CheckSum(strTemp + CR + ETX) + CR + LF);

                        strRet += "\n" + strTemp;

                        intSeq = ASTM_Seq(intSeq);

                        strTemp = intSeq.ToString() + "O|";
                        strTemp = strTemp + "1|";
                        strTemp = strTemp + drTemp["rackNo"].ToString() + "^" + drTemp["rackPos"].ToString() + "^" + drTemp["spcNo"].ToString().PadLeft(22, ' ') + "^B^O|";
                        strTemp = strTemp + "|";
                        strTemp = strTemp + strTstId + "|";
                        strTemp = strTemp + "|";

                        strDateTime = AddThreeSeconds(strDateTime);

                        strTemp = strTemp + strDateTime + "|";
                        strTemp = strTemp + "||||N|||||||";
                        strTemp = strTemp + "00000000^" + strInquiryType + "|";
                        strTemp = strTemp + strSortInfo + "|";
                        strTemp = strTemp + "|||||";
                        strTemp = strTemp + strRptType;

                        intRet = Message_Send(STX + strTemp + CR + ETX + Common.CheckSum(strTemp + CR + ETX) + CR + LF);

                        strRet += "\n" + strTemp;

                        string strAddRow = "";
                        bool blnAddRow = true;

                        strAddRow = "Order" + TAB;
                        strAddRow = strAddRow + strDtm + TAB;

                        switch (strInquiryType)
                        {
                            case "B":
                                strAddRow = strAddRow + "BT" + TAB;
                                break;

                            case "C":
                                strAddRow = strAddRow + "CV" + TAB;
                                break;

                            case "SI":
                                strAddRow = strAddRow + "TS" + TAB;
                                break;

                            default:
                                strAddRow = strAddRow + strInquiryType + TAB;
                                blnAddRow = false;
                                break;
                        }

                        strAddRow = strAddRow + drTemp["spcNo"].ToString() + TAB;
                        strAddRow = strAddRow + strRackNo + TAB;
                        strAddRow = strAddRow + drTemp["rackPos"].ToString() + TAB;

                        if (strInquiryType == "SI")
                        {
                            string strSortDesc = "";

                            if (_dctSortInfo.ContainsKey(strSortIndex)) { strSortDesc = _dctSortInfo[strSortIndex]; }

                            if (string.IsNullOrEmpty(strSortDesc))
                            {
                                strAddRow = strAddRow + strSortIndex + TAB;
                            }
                            else
                            {
                                strAddRow = strAddRow + strSortDesc + TAB;
                            }
                        }
                        else
                        {
                            if (strRptType == "Q")
                            {
                                if (strTstId.IndexOf("SP") > -1)
                                {
                                    if (strTstId.IndexOf("CBC") > -1)
                                    {
                                        strAddRow = strAddRow + "XN, SP" + TAB;
                                    }
                                    else
                                    {
                                        strAddRow = strAddRow + "SP" + TAB;
                                    }
                                }
                                else
                                {
                                    strAddRow = strAddRow + strTstId + TAB;
                                }
                            }
                            else
                            {
                                strAddRow = strAddRow + "No Order.." + TAB;
                            }
                        }

                        if (blnAddRow) { GrdRowAdd(strAddRow); }
                    }

                    intSeq = ASTM_Seq(intSeq);
                    strTemp = intSeq.ToString() + "L|1|N";

                    intRet = Message_Send(STX + strTemp + CR + ETX + Common.CheckSum(strTemp + CR + ETX) + CR + LF);

                    strRet += "\n" + strTemp;

                    strLog += $"{TAB}11. Order: " + strRet + "\r\n";
                    strLog += "--------------------------------------------------------------------------" + "\r\n";
                    strLog += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "GetOrderMessage End" + "\r\n";

                    Common.File_Record(strLog + "\r\n",
                                       false,
                                       mstrAppPath + "log\\",
                                       DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                }
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"GetOrderMessage Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            return strRet + Constant.DLM_HS + strSpcNoList + Constant.DLM_HS + IsAutoRerun.ToString() + Constant.DLM_HS + strInquiryType + Constant.DLM_HS + strSortIdxList + Constant.DLM_HS + strSortSpcNoList;
        }

        private void Data_Analyzer(string strData)
        {
            try
            {
                long intPos;

                mintCount = 0;
                mblnWait = false;

                string strTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");
                string strDummy = strData;

                if (strDummy.IndexOf(ENQ) > -1)
                {
                    strDummy = CR + LF + strTimestamp + strDummy;
                }

                if (strDummy.IndexOf(EOT) > -1)
                {
                    strDummy = strDummy + strTimestamp + CR + LF;
                }

                Common.File_Record(strDummy, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + ".log");

                if (Constant.gblnLoggingTimeStamp == true)
                {
                    Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + CR + LF + "[ E > H ]" + CR + LF + strData + "\r\n",
                        false,
                        mstrAppPath + "log\\",
                        DateTime.Now.ToString(mstrDateTimeFormat) + "-SckTimestamp.log");
                }

                RcvTextBoxData(strData);

                if (strData.IndexOf(ENQ) > -1)
                {
                    //2025-03-16 : 장비에서 너무 빨리 다음 데이터 수신 시 이전에 처리못한 거 처리
                    if (string.IsNullOrEmpty(mstrData) == false && (mstrData.IndexOf(EOT) > -1 || strData.IndexOf(EOT) > -1))
                    {
                        if (strData.IndexOf(EOT) > -1) { mstrData = mstrData + EOT; }
                        Data_Analyzer_BeforeData(mstrData);
                    }
                    mintCount = 0;
                    mstrData = "";
                    mstrUse = "equip";
                    mblnStop = true;
                    Tcp_SendData(ACK);
                }

                if (strData.IndexOf(ENQ) > -1 || strData.IndexOf(ACK) > -1 || strData.IndexOf(NAK) > -1)
                {
                    return;
                }

                mstrData += strData;
                if (strData.IndexOf(LF) > -1)
                {
                    Tcp_SendData(ACK);
                }

                intPos = strData.IndexOf(EOT);

                if (intPos > -1)
                {
                    Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"수신 데이터 분석 및 처리 Start" + "\r\n",
                                       false,
                                       mstrAppPath + "log\\",
                                       DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                    //2025-03-19 : ENQ 수신 시 데이터 초기화하는 것 때문에 다른 변수에 담아서 처리하도록 수정
                    string strRcvData = mstrData;

                    string strType = "";
                    string strRackNo = "", strRackPos = "", strSpcNo = "";
                    string[] aryRackInfo;
                    string[] aryData = strRcvData.Split((char)2);     //STX로 Split

                    // 2017-11-28
                    // CT-90 Ver2.0 적용
                    //When TS-10 is not connected
                    //Q|1|123456^01^1234567890123456789012^B||||20010905150000||||B||<CR>
                    //When TS-10 is connected
                    //Q|1|123456^01^1234567890123456789012^B||||20010905150000||||C|1^1|<CR>
                    string strAnalysisParameterID = "";
                    string strInquiryType = "";
                    string strTS10Info = "";
                    string strDataOfMeasurementValue = "";
                    string strTrayNo = "";
                    string strTrayBarNo = "";
                    string strSortIdx = "";
                    string strDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string strDtm = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string strReulstList = "";
                    string strUnitNo = "";
                    string strTmpRackPos = "";
                    string strTmpSpcNo = "";
                    string strTmpBgwKey = "";
                    string filterExpression;
                    DataRow[] selectedRows;

                    // 2017-12-12
                    // CT-90 Ver2.0 + TS-10 연동 시 요청들어온대로 리턴해야지 에러안남!!!
                    // B : inquiry of BT - 최초 랙 장착 시 오더요청
                    // C : inquiry at other than BT, TS-10 (e.g. CVR) - 라인 지나갈 때
                    // SI : inquiry at TS-10's right arrival position of carry-in line. - 랙이 TS-10 에 들어갈 때
                    // SO : inquiry at TS-10's left arrival position of carry-in line. - 랙이 TS-10 에서 나올 때

                    //2Q|1|000009^01^          171211551208^B||||20171211230301||||C^06|1^1|
                    // TS-10 No. ^ Work Cycle : 1byte^1byte

                    string[] aryMsg = strRcvData.Split((char)4);

                    for (int k = 0; k < aryMsg.Length; k++)
                    {

                    }

                    for (int i = 0; i < aryData.Length; i++)
                    {
                        if (aryData[i].Trim() != "")
                        {
                            string[] aryTemp = aryData[i].Split('|');
                            switch (aryTemp[0].Substring(1))
                            {
                                case "Q":
                                    strType = "order";
                                    if (aryTemp[2] != "")
                                    {
                                        strInquiryType = Common.P(aryTemp[10], "^", 1);
                                        strUnitNo = Common.P(aryTemp[10], "^", 2);
                                        strTS10Info = aryTemp[11];

                                        string[] aryOrdInfo = aryTemp[2].Split('\\');
                                        for (int j = 0; j < aryOrdInfo.Length; j++)
                                        {
                                            if (aryOrdInfo[j] != "")
                                            {
                                                aryRackInfo = aryOrdInfo[j].Split('^');
                                                strRackNo = aryRackInfo[0].Trim();
                                                strTmpRackPos = aryRackInfo[1].Trim();
                                                strTmpSpcNo = aryRackInfo[2].Trim();

                                                if (strRackPos == "")
                                                {
                                                    strRackPos = strTmpRackPos;
                                                }
                                                else
                                                {
                                                    strRackPos += "^" + strTmpRackPos;
                                                }
                                                if (strSpcNo == "")
                                                {
                                                    strSpcNo = strTmpSpcNo;
                                                }
                                                else
                                                {
                                                    strSpcNo += "^" + strTmpSpcNo;
                                                }

                                                //2024-06-04 : BT, SI 라인일 경우 도착처리 호출
                                                if (strInquiryType == "B" || strInquiryType == "SI")
                                                {
                                                    //2025-02-21 : 검체번호의 마지막 시퀀스 번호가 3,5번일때만 도착처리하도록 수정
                                                    if (Common.IsNumeric(strTmpSpcNo))
                                                    {
                                                        // 마지막 문자가 3 또는 5인지 확인
                                                        string lastDigit = strTmpSpcNo.Substring(strTmpSpcNo.Length - 1);
                                                        if (lastDigit == "3" || lastDigit == "5")
                                                        {
                                                            // 여기에 조건을 만족할 때의 로직을 작성
                                                            Lis.Interface.clsParameterCollection Param = new Lis.Interface.clsParameterCollection();

                                                            string strEqpTstDt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                            Param.Items.Add("barNo", strTmpSpcNo);
                                                            Param.Items.Add("devcCd", SG_TS_EQP_CD);
                                                            Param.Items.Add("rsltPrgsCd", "AT");
                                                            Param.Items.Add("accDtm", strEqpTstDt);
                                                            Param.Items.Add("tlaSeqNum", "1");
                                                            Param.Items.Add("trackComm", "TS10");
                                                            Param.Items.Add("mdulCd", "TS10");

                                                            //TS진행상태(Hitachi AQM:Q, RFM:S,OBS:A/Roche archive:A,seen:S/Sysmex 분류:S,archive:A)
                                                            Param.Items.Add("tsGbn", "S");
                                                            Param.Items.Add("rackNo", strRackNo);
                                                            Param.Items.Add("holeNo", strTmpRackPos);
                                                            Param.Items.Add("trayNo", "");
                                                            Param.Items.Add("traySeq", "");
                                                            Param.Items.Add("trayHoleNo", "");
                                                            Param.Items.Add("tsGrpNo", "");
                                                            Param.Items.Add("tsGrpNm", "");
                                                            Param.Items.Add("tsErr", "");

                                                            if (DEV_IN_OFFICE == false && DEBUG_MODE == false)
                                                            {
                                                                Lis.Interface.clsBizSeeGene objApi;
                                                                objApi = new Lis.Interface.clsBizSeeGene();
                                                                string strRtn = objApi.SaveSampleTracking(Param);
                                                                Param = null;
                                                                objApi = null;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        //2024-03-20 : Rack 번호별 장비검사일시 딕셔너리에 저장
                                        if (_dctEqpTestDtTmByRack.ContainsKey(strRackNo) == false)
                                        {
                                            _dctEqpTestDtTmByRack.Add(strRackNo, aryTemp[6]);
                                        }
                                    }

                                    break;

                                case "O":
                                    strType = "result";

                                    if (aryTemp[3] != "")
                                    {
                                        aryRackInfo = aryTemp[3].Split('^');
                                        strRackNo = aryRackInfo[0].Trim();
                                        strTmpRackPos = aryRackInfo[1].Trim();
                                        strTmpSpcNo = aryRackInfo[2].Trim();

                                        if (string.IsNullOrEmpty(strRackPos))
                                        {
                                            strRackPos = strTmpRackPos;
                                        }
                                        else
                                        {
                                            strRackPos += "^" + strTmpRackPos;
                                        }

                                        if (string.IsNullOrEmpty(strSpcNo))
                                        {
                                            strSpcNo = strTmpSpcNo;
                                        }
                                        else
                                        {
                                            strSpcNo += "^" + strTmpSpcNo;
                                        }
                                    }

                                    break;

                                case "R":
                                    strAnalysisParameterID = aryTemp[2];
                                    strAnalysisParameterID = Common.P(strAnalysisParameterID, "^", 5);

                                    strDataOfMeasurementValue = aryTemp[3];

                                    if (aryTemp[3] != "")
                                    {
                                        if (strReulstList == "")
                                        {
                                            strReulstList = strDataOfMeasurementValue.Trim();
                                        }
                                        else
                                        {
                                            strReulstList += "|" + strDataOfMeasurementValue.Trim();
                                        }
                                    }

                                    break;

                                default:
                                    break;
                            }
                        }
                    }

                    if (strRackNo != "" && strRackPos != "" && strSpcNo != "")
                    {
                        // 처방 요청
                        if (strType == "order")
                        {
                            bool blnTemp = false;

                            // 처음 요청인지 판단
                            filterExpression = "rackNo = " + Common.STS(strRackNo);
                            selectedRows = gdtInquirySpcNoList.Select(filterExpression);

                            // Display the selected data
                            foreach (DataRow row in selectedRows)
                            {
                                Common.File_Record(TAB + $"최초요청 rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}" + "\r\n",
                                                   false,
                                                   mstrAppPath + "log\\",
                                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                if (strInquiryType == "B")
                                {
                                    row.Delete();
                                    Common.File_Record(TAB + "RackNo 중복으로 데이터 삭제 후 처리" + "\r\n",
                                        false, mstrAppPath + "log\\",
                                        DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                }
                                else
                                {
                                    blnTemp = true;
                                }
                            }

                            if (!blnTemp)
                            {
                                string strOrdInfo;
                                if (BASE_ORD == "Y")
                                {
                                    strOrdInfo = "CBC^DIFF^SP";
                                }
                                else
                                {
                                    strOrdInfo = "-";
                                }

                                string[] aryRackPos = strRackPos.Split('^');
                                string[] arySpcNo = strSpcNo.Split('^');
                                strDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                                for (int i = 0; i < aryRackPos.Length; i++)
                                {
                                    DataRow row1 = gdtInquirySpcNoList.NewRow();
                                    row1["rackNo"] = strRackNo;
                                    row1["rackPos"] = aryRackPos[i];
                                    row1["spcNo"] = arySpcNo[i];
                                    row1["ordInfo"] = strOrdInfo;
                                    row1["inquiryType"] = strInquiryType;
                                    row1["unitNo"] = strUnitNo;
                                    row1["sortInfo"] = strTS10Info;
                                    row1["inputDtTm"] = strDateTime;
                                    gdtInquirySpcNoList.Rows.Add(row1);
                                }

                            }
                            else
                            {
                                filterExpression = "rackNo = " + Common.STS(strRackNo);
                                selectedRows = gdtInquirySpcNoList.Select(filterExpression);

                                // Display the selected data
                                foreach (DataRow row in selectedRows)
                                {
                                    Common.File_Record(TAB + $"처방요청 rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}" + "\r\n",
                                                        false,
                                                        mstrAppPath + "log\\",
                                                        DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                    row["ordInfo"] = "Y";
                                }
                            }
                        }

                        // 결과 완료
                        if (strType == "result")
                        {
                            string strAddRow = "";

                            switch (strAnalysisParameterID)
                            {
                                case "FINAL":

                                    if (_dicOrdTstCdAtStart.ContainsKey(strSpcNo))
                                    {
                                        _dicOrdTstCdAtStart.Remove(strSpcNo);
                                    }

                                    strTmpBgwKey = DateTime.Now.ToString("yyyyMMddHHmmss.ffff");

                                    Common.File_Record(TAB +
                                                       $"{strAnalysisParameterID} _dctSetRackInfo key: {strTmpBgwKey}, spcNo: {strSpcNo}, rackNo: {strRackNo}, rackPos: {strRackPos}" + "\r\n",
                                                       false,
                                                       mstrAppPath + "log\\",
                                                       DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                    string[] aryRackPos = strRackPos.Split('^');
                                    string[] arySpcNo = strSpcNo.Split('^');
                                    string[] aryResult = strReulstList.Split('|');

                                    for (int i = 0; i < aryRackPos.Length; i++)
                                    {
                                        strAddRow = "Result" + TAB;
                                        strAddRow = strAddRow + strDtm + TAB;
                                        strAddRow = strAddRow + "ST" + TAB;
                                        strAddRow = strAddRow + arySpcNo[i] + TAB;
                                        strAddRow = strAddRow + strRackNo + TAB;
                                        strAddRow = strAddRow + aryRackPos[i] + TAB;

                                        //00^1858^OK^NG^OK
                                        strAddRow = strAddRow +
                                                    "XN: " + Common.P(aryResult[i], "^", 3) + ", " +
                                                    "SP: " + Common.P(aryResult[i], "^", 4) + TAB;

                                        GrdRowAdd(strAddRow);
                                    }

                                    break;

                                case "STORE-F":

                                    if (_dctRetiSpcNo.ContainsKey(strSpcNo) == true)
                                    {
                                        _dctRetiSpcNo.Remove(strSpcNo);
                                    }

                                    string strGubun = "A";
                                    string strTrayPos = "";
                                    string traySeqNo = "";
                                    string strRackDiv = "";

                                    // 2020-06-11 : sort index 저장하기
                                    strSortIdx = Common.P(strDataOfMeasurementValue, "^", 1);
                                    strTrayNo = Common.P(strDataOfMeasurementValue, "^", 2);
                                    strTrayBarNo = Common.P(strDataOfMeasurementValue, "^", 4);
                                    strTrayPos = Common.P(strDataOfMeasurementValue, "^", 5);
                                    strRackDiv = Common.P(strDataOfMeasurementValue, "^", 3);

                                    if (strTrayBarNo == "") { strGubun = "S"; strTrayBarNo = strTrayNo; }

                                    //2025-04-23 : 50개짜리 랙 중 Back Rack 일 경우 (50F, 50B) Front, Back
                                    if (strRackDiv == "50B")
                                    {
                                        //50개 앞에 랙 + 25개 공란
                                        strTrayPos = (Common.Val(strTrayPos) + 75).ToString();
                                    }

                                    filterExpression = "rackNo = " + Common.STS(strRackNo) + " AND rackPos = " + Common.STS(strRackPos);
                                    selectedRows = gdtInquirySpcNoList.Select(filterExpression);

                                    foreach (DataRow row in selectedRows)
                                    {
                                        Common.File_Record(TAB + $"STORE-F 완료 후 DataRow 삭제 rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}" + "\r\n",
                                                           false,
                                                           mstrAppPath + "log\\",
                                                           DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                        row.Delete();
                                    }

                                    switch (Constant.gstrCenterCode)
                                    {
                                        case "13100005":
                                            //서울
                                            break;

                                        case "13900000":
                                            //부산경남
                                            if (strTrayNo == "1" || strTrayNo == "2")
                                            {
                                                strGubun = "A";
                                            }
                                            else
                                            {
                                                strGubun = "S";
                                            }
                                            break;

                                        case "14100000":
                                            //대구경북
                                            break;

                                        case "14300000":
                                            //광주호남
                                            break;

                                        case "14500000":
                                            //대전충청
                                            break;

                                        default:
                                            break;
                                    }

                                    if (strGubun == "A" && string.IsNullOrEmpty(strTrayBarNo) == false)
                                    {
                                        if (strSpcNo.IndexOf("^") > -1)
                                        {
                                            strSpcNo = strSpcNo;
                                        }

                                        Common.File_Record(TAB + $"Archive spcNo {strSpcNo}" + "\r\n",
                                                            false,
                                                            mstrAppPath + "log\\",
                                                            DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        int sequenceNumber = trayManager.RegisterNewTray(strTrayBarNo, Common.Val(strTrayNo));

                                        Common.File_Record(TAB + string.Format("새 트레이가 등록되었습니다. 시퀀스 번호: {0}", sequenceNumber) + "\r\n",
                                                            false,
                                                            mstrAppPath + "log\\",
                                                            DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        // 샘플 저장
                                        bool stored = trayManager.StoreSample(strTrayBarNo, strSpcNo, Common.Val(strTrayPos));

                                        if (stored)
                                        {
                                            Common.File_Record(TAB + "샘플이 성공적으로 저장되었습니다." + "\r\n",
                                                                false,
                                                                mstrAppPath + "log\\",
                                                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        }

                                        // 사용 가능한 위치 조회
                                        List<int> availablePositions = trayManager.GetAvailablePositions(strTrayBarNo);

                                        Common.File_Record(TAB + string.Format("사용 가능한 위치 수: {0}", availablePositions.Count) + "\r\n",
                                                            false,
                                                            mstrAppPath + "log\\",
                                                            DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        // 활성 트레이 정보 조회
                                        List<TrayManagement.TrayInfo> activeTrays = trayManager.GetActiveTrayInfo();
                                        foreach (TrayManagement.TrayInfo tray in activeTrays)
                                        {
                                            if (strTrayBarNo == tray.TrayBarcode)
                                            {
                                                traySeqNo = tray.SequenceNumber.ToString();
                                            }

                                            Common.File_Record(TAB + string.Format("트레이: {0}, 시퀀스: {1}, 현재 샘플 수: {2}, TraySeqno: {3}, TrayBarNo: {4}",
                                                                tray.TrayBarcode,
                                                                tray.SequenceNumber,
                                                                tray.CurrentSamples,
                                                                traySeqNo,
                                                                strTrayBarNo) + "\r\n",
                                                                false,
                                                                mstrAppPath + "log\\",
                                                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        }
                                    }

                                    if (DEBUG_MODE == false)
                                    {
                                        strTmpBgwKey = DateTime.Now.ToString("yyyyMMddHHmmss.ffff") + TAB + strSpcNo + TAB + strRackNo + TAB + strRackPos;

                                        Common.File_Record(TAB +
                                            $"{strAnalysisParameterID} _dctSetRackInfo key: {strTmpBgwKey}, spcNo: {strSpcNo}, rackNo: {strRackNo}, rackPos: {strRackPos}" + "\r\n",
                                            false,
                                            mstrAppPath + "log\\",
                                            DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        if (_dctSetRackInfo.ContainsKey(strTmpBgwKey) == false)
                                        {
                                            Common.File_Record(TAB +
                                                $"key: {strTmpBgwKey}, 구분: {strGubun}, TrayBarNo: {strTrayBarNo}" + "\r\n",
                                                false,
                                                mstrAppPath + "log\\",
                                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                            if (strGubun == "A" && string.IsNullOrEmpty(strTrayBarNo) == false)
                                            {
                                                _dctSetRackInfo.Add(strTmpBgwKey, strRackNo + TAB + strRackPos + TAB + strSpcNo + TAB + traySeqNo + TAB + strSpcNo + TAB + strTrayPos + TAB + strSortIdx + TAB + strTrayNo + TAB + strTrayBarNo);

                                            }
                                            else
                                            {
                                                _dctSetRackInfo.Add(strTmpBgwKey, strRackNo + TAB + strRackPos + TAB + strSpcNo + TAB + strTrayBarNo + TAB + strSpcNo + TAB + strTrayPos + TAB + strSortIdx + TAB + strTrayNo + TAB + strTrayBarNo);
                                            }
                                        }
                                        else
                                        {
                                            Common.File_Record(TAB +
                                                                $"{strAnalysisParameterID} _dctSetRackInfo Duplicate key: {strTmpBgwKey}, spcNo: {strSpcNo}, rackNo: {strRackNo}, rackPos: {strRackPos}" + "\r\n",
                                                                false,
                                                                mstrAppPath + "log\\",
                                                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                        }
                                    }

                                    strAddRow = "Result" + TAB;
                                    strAddRow = strAddRow + strDtm + TAB;
                                    strAddRow = strAddRow + "TS" + TAB;
                                    strAddRow = strAddRow + strSpcNo + TAB;
                                    strAddRow = strAddRow + strRackNo + TAB;
                                    strAddRow = strAddRow + strRackPos + TAB;
                                    strAddRow = strAddRow + strTrayBarNo + "-" + Common.P(strDataOfMeasurementValue, "^", 5) + TAB;
                                    GrdRowAdd(strAddRow);
                                    break;

                                default:
                                    strAddRow = "Result" + TAB;
                                    strAddRow = strAddRow + strDtm + TAB;
                                    strAddRow = strAddRow + strAnalysisParameterID + TAB;
                                    strAddRow = strAddRow + strSpcNo + TAB;
                                    strAddRow = strAddRow + strRackNo + TAB;
                                    strAddRow = strAddRow + strRackPos + TAB;
                                    strAddRow = strAddRow + strDataOfMeasurementValue + TAB;
                                    GrdRowAdd(strAddRow);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (strAnalysisParameterID == "WORKCYCLE") { }
                        if (strAnalysisParameterID == "TRAY") { }
                        if (strAnalysisParameterID == "BUFFERAREA") { }
                        if (strAnalysisParameterID == "WORKAREA") { }
                        if (strAnalysisParameterID == "INITIALIZE") { }

                        string[] aryTray = strDataOfMeasurementValue.Split('\\');
                        string[] arySql = new string[1];
                        string strSql = "";
                        string strTrayType = "";
                        string strTS10RackNo = "";
                        bool blnExistTray = false;
                        string strLog = "";

                        // 2017-12-15
                        // TS-10
                        // 아카이브 랙 꽉차서 새로운 랙으로 교체 시!!!
                        // 기존 랙번호 들어오면 삭제 후 새로 생성!!!

                        if (strAnalysisParameterID == "EXCHANGE")
                        {
                            for (int i = 0; i < aryTray.Length; i++)
                            {
                                strTrayNo = Common.P(aryTray[i], "^", 1);
                                if (strTrayNo == "") { strTrayNo = Common.P(aryTray[i], "^", 2); }

                                strTrayBarNo = Common.P(aryTray[i], "^", 3);

                                if (strTrayBarNo != "")
                                {
                                    strTS10RackNo = strTrayBarNo;
                                    strTrayType = "ARCHIVE";

                                    trayManager.DeactivateTray(strTrayBarNo);

                                    Common.File_Record(TAB + $"Tray Exchange 로 비활성화 TrayBarNo: {strTrayBarNo}" + "\r\n", false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                    //AC일 수도 있으니
                                    string strSgTray;
                                    string strSgRack;

                                    switch (Constant.SG_TS_EQP_CD)
                                    {
                                        case "620":
                                            strSgTray = "HH";
                                            break;

                                        case "621":
                                            strSgTray = "HG";
                                            break;

                                        case "039":
                                            strSgTray = "BH";
                                            break;

                                        case "922":
                                            strSgTray = "DE";
                                            break;

                                        case "842":
                                            strSgTray = "GE";
                                            break;

                                        case "724":
                                            strSgTray = "JE";
                                            break;

                                        default:
                                            strSgTray = strTrayBarNo;
                                            break;
                                    }

                                    if (strTrayBarNo.Length >= 3)
                                    {
                                        strSgRack = strSgTray + strTrayBarNo.Substring(3, 3);
                                    }
                                    else
                                    {
                                        strSgRack = strSgTray + strTrayBarNo.PadLeft(3, '0').Substring(Math.Max(0, strTrayBarNo.Length - 3));
                                    }

                                    Common.File_Record(TAB + $"씨젠 랙번호로 변경: {strSgRack}" + "\r\n", false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                }
                            }
                        }

                        if (strAnalysisParameterID == "SAMPLE")
                        {
                            for (int i = 0; i < aryTray.Length; i++)
                            {
                                strTrayNo = Common.P(aryTray[i], "^", 1);
                                if (strTrayNo == "") { strTrayNo = Common.P(aryTray[i], "^", 2); }

                                strTrayBarNo = Common.P(aryTray[i], "^", 3);

                                if (strTrayBarNo != "")
                                {
                                    strTS10RackNo = strTrayBarNo;
                                    strTrayType = "ARCHIVE";
                                }
                                else
                                {
                                    strTS10RackNo = strTrayNo;
                                    strTrayType = "SORT";
                                }
                            }
                        }
                    }
                    mstrData = "";
                    mstrUse = "";
                    mblnStop = false;

                    Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"수신 데이터 분석 및 처리 End" + "\r\n",
                        false,
                        mstrAppPath + "log\\",
                        DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                }
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"수신 데이터 분석 및 처리 Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }
        }

        private void GetSortInformation()
        {

            switch (Constant.SG_TS_EQP_CD)
            {
                case "620":

                    _dctSortInfo.Add("0", "LINE OUT");
                    _dctSortInfo.Add("1", "보관");
                    _dctSortInfo.Add("2", "PB+CBC+OTHER");
                    _dctSortInfo.Add("3", "PB+CBC+RETI");
                    _dctSortInfo.Add("4", "OTHER");
                    _dctSortInfo.Add("5", "CBC+OTHER");
                    _dctSortInfo.Add("6", "ESR");
                    _dctSortInfo.Add("7", "CBC+ABO+ESR");
                    _dctSortInfo.Add("8", "ABO");
                    _dctSortInfo.Add("9", "CITRATE HE4");
                    _dctSortInfo.Add("10", "CBC+ESR");
                    _dctSortInfo.Add("11", "CBC+ABO");
                    _dctSortInfo.Add("12", "CITRATE HE5");
                    _dctSortInfo.Add("13", "CITRATE HE2");
                    _dctSortInfo.Add("14", "CITRATE ALL");
                    _dctSortInfo.Add("15", "HbA1C+OTHER");
                    _dctSortInfo.Add("18", "떠보기");
                    _dctSortInfo.Add("19", "SLIDE");
                    _dctSortInfo.Add("20", "1차재검");
                    _dctSortInfo.Add("21", "2차재검");
                    _dctSortInfo.Add("22", "HbA1C");
                    _dctSortInfo.Add("23", "SPECIAL IMM");
                    _dctSortInfo.Add("24", "DATA ERR");
                    _dctSortInfo.Add("25", "ALL SPECIAL IMM");
                    _dctSortInfo.Add("26", "CBC+IMMUNO");
                    _dctSortInfo.Add("61", "ERROR");

                    break;

                case "621":

                    _dctSortInfo.Add("0", "LINE OUT");
                    _dctSortInfo.Add("1", "보관");
                    _dctSortInfo.Add("2", "PB+CBC+OTHER");
                    _dctSortInfo.Add("3", "PB+CBC+RETI");
                    _dctSortInfo.Add("4", "OTHER");
                    _dctSortInfo.Add("5", "CBC+OTHER");
                    _dctSortInfo.Add("6", "ESR");
                    _dctSortInfo.Add("7", "CBC+ABO+ESR");
                    _dctSortInfo.Add("8", "ABO");
                    _dctSortInfo.Add("9", "CITRATE HE4");
                    _dctSortInfo.Add("10", "CBC+ESR");
                    _dctSortInfo.Add("11", "CBC+ABO");
                    _dctSortInfo.Add("12", "CITRATE HE5");
                    _dctSortInfo.Add("13", "CITRATE HE2");
                    _dctSortInfo.Add("14", "CITRATE ALL");
                    _dctSortInfo.Add("15", "HbA1C+OTHER");
                    _dctSortInfo.Add("18", "떠보기");
                    _dctSortInfo.Add("19", "SLIDE");
                    _dctSortInfo.Add("20", "1차재검");
                    _dctSortInfo.Add("21", "2차재검");
                    _dctSortInfo.Add("22", "HbA1C");
                    _dctSortInfo.Add("23", "SPECIAL IMM");
                    _dctSortInfo.Add("24", "DATA ERR");
                    _dctSortInfo.Add("25", "ALL SPECIAL IMM");
                    _dctSortInfo.Add("26", "CBC+IMMUNO");
                    _dctSortInfo.Add("61", "ERROR");

                    break;

                case "039":

                    //부산
                    _dctSortInfo.Add("0", "LINE OUT");
                    _dctSortInfo.Add("1", "보관");
                    _dctSortInfo.Add("2", "ERROR");
                    _dctSortInfo.Add("3", "서울 + 부산");
                    _dctSortInfo.Add("4", "PBS + HBA1C");
                    _dctSortInfo.Add("5", "OTHER");
                    _dctSortInfo.Add("6", "서울");
                    _dctSortInfo.Add("7", "CBC + HbA1C");
                    _dctSortInfo.Add("8", "사용안함");
                    _dctSortInfo.Add("9", "HbA1C + ABO + ESR");
                    _dctSortInfo.Add("10", "HbA1C + ABO");
                    _dctSortInfo.Add("11", "HbA1C + ESR");
                    _dctSortInfo.Add("12", "ABO + ESR");
                    _dctSortInfo.Add("13", "CBC + ABO + ESR");
                    _dctSortInfo.Add("14", "PBS + CBC");
                    _dctSortInfo.Add("15", "사용안함");
                    _dctSortInfo.Add("16", "사용안함");
                    _dctSortInfo.Add("17", "CBC + HBA1c + ABO + ESR");
                    _dctSortInfo.Add("18", "CBC + HBA1c + ABO");
                    _dctSortInfo.Add("19", "CBC + HBA1c + ESR");
                    _dctSortInfo.Add("20", "CBC + ABO");
                    _dctSortInfo.Add("21", "CBC + ESR");
                    _dctSortInfo.Add("22", "ABO");
                    _dctSortInfo.Add("23", "ESR");
                    _dctSortInfo.Add("24", "재검");
                    _dctSortInfo.Add("25", "SLIDE");
                    _dctSortInfo.Add("26", "MORE = 떠보기");
                    _dctSortInfo.Add("61", "ERROR");

                    break;

                case "922":

                    //대구
                    _dctSortInfo.Add("0", "LINE OUT");
                    _dctSortInfo.Add("1", "보관");
                    _dctSortInfo.Add("2", "Data Error");
                    _dctSortInfo.Add("3", "없음");
                    _dctSortInfo.Add("4", "IMH");
                    _dctSortInfo.Add("5", "서울");
                    _dctSortInfo.Add("6", "서울+대구");
                    _dctSortInfo.Add("7", "CBC+ESR");
                    _dctSortInfo.Add("8", "ESR");
                    _dctSortInfo.Add("9", "CBC+Other");
                    _dctSortInfo.Add("10", "Other");
                    _dctSortInfo.Add("11", "PB+CBC+RET");
                    _dctSortInfo.Add("12", "CBC+PB");
                    _dctSortInfo.Add("13", "CBC+ABO");
                    _dctSortInfo.Add("14", "ABO");
                    _dctSortInfo.Add("15", "CBC+ABO+ESR");
                    _dctSortInfo.Add("16", "IMU");
                    _dctSortInfo.Add("17", "Citrate All");
                    _dctSortInfo.Add("18", "Citrate2");
                    _dctSortInfo.Add("19", "RET");
                    _dctSortInfo.Add("20", "CBC+RET");
                    _dctSortInfo.Add("21", "HbA1c + @");
                    _dctSortInfo.Add("22", "AAI, AAD");
                    _dctSortInfo.Add("23", "Rerun Error");
                    _dctSortInfo.Add("24", "2차 재검");
                    _dctSortInfo.Add("25", "1차 재검");
                    _dctSortInfo.Add("26", "SLIDE");
                    _dctSortInfo.Add("27", "떠보기");
                    _dctSortInfo.Add("28", "HbA1c+CBC");
                    _dctSortInfo.Add("61", "ERROR");

                    break;

                case "842":

                    _dctSortInfo.Add("0", "LINE OUT");
                    _dctSortInfo.Add("1", "보관");
                    _dctSortInfo.Add("2", "Data Error");
                    _dctSortInfo.Add("3", "Barcode Error");
                    _dctSortInfo.Add("4", "서울");
                    _dctSortInfo.Add("5", "서울광주");
                    _dctSortInfo.Add("6", "ABO");
                    _dctSortInfo.Add("7", "ABO+ESR");
                    _dctSortInfo.Add("8", "ESR");
                    _dctSortInfo.Add("9", "CBC+Other");
                    _dctSortInfo.Add("10", "Other");
                    _dctSortInfo.Add("11", "PB+CBC+Reti");
                    _dctSortInfo.Add("12", "CBC+PBS");
                    _dctSortInfo.Add("13", "CBC+ABO");
                    _dctSortInfo.Add("14", "CBC+ESR");
                    _dctSortInfo.Add("15", "Citrate,PA8");
                    _dctSortInfo.Add("16", "Citrate All");
                    _dctSortInfo.Add("17", "Citrate 2,IMH");
                    _dctSortInfo.Add("18", "Reti");
                    _dctSortInfo.Add("19", "CBC+Reti");
                    _dctSortInfo.Add("20", "HbA1c Other");
                    _dctSortInfo.Add("21", "PCR");
                    _dctSortInfo.Add("22", "Rerun Error");
                    _dctSortInfo.Add("23", "2차재검");
                    _dctSortInfo.Add("24", "1차재검");
                    _dctSortInfo.Add("25", "SLIDE");
                    _dctSortInfo.Add("26", "떠보기");
                    _dctSortInfo.Add("27", "HbA1c");
                    _dctSortInfo.Add("61", "ERROR");

                    break;

                case "724":

                    _dctSortInfo.Add("0", "LINE OUT");
                    _dctSortInfo.Add("1", "보관");
                    _dctSortInfo.Add("2", "PBS+RETI");
                    _dctSortInfo.Add("3", "EDTA Plasma");
                    _dctSortInfo.Add("4", "서울");
                    _dctSortInfo.Add("5", "서울+대전");
                    _dctSortInfo.Add("6", "CBC+IH500");
                    _dctSortInfo.Add("7", "CBC+HbA1c+ESR");
                    _dctSortInfo.Add("8", "CBC+IH500+HbA1c");
                    _dctSortInfo.Add("9", "미완료");
                    _dctSortInfo.Add("10", "OTHER");
                    _dctSortInfo.Add("11", "재검+슬라이드");
                    _dctSortInfo.Add("12", "HbA1c+IH,HbA1c+ESR,ESR+IH");
                    _dctSortInfo.Add("13", "CBC+HbA1c");
                    _dctSortInfo.Add("14", "CBC+ESR");
                    _dctSortInfo.Add("61", "ERROR");

                    break;

                default:

                    //서울
                    _dctSortInfo.Add("0", "LINE OUT");
                    _dctSortInfo.Add("1", "보관");
                    _dctSortInfo.Add("2", "PB+CBC+OTHER");
                    _dctSortInfo.Add("3", "PB+CBC+RETI");
                    _dctSortInfo.Add("4", "OTHER");
                    _dctSortInfo.Add("5", "CBC+OTHER");
                    _dctSortInfo.Add("6", "ESR");
                    _dctSortInfo.Add("7", "CBC+ABO+ESR");
                    _dctSortInfo.Add("8", "ABO");
                    _dctSortInfo.Add("9", "CITRATE HE4");
                    _dctSortInfo.Add("10", "CBC+ESR");
                    _dctSortInfo.Add("11", "CBC+ABO");
                    _dctSortInfo.Add("12", "CITRATE HE5");
                    _dctSortInfo.Add("13", "CITRATE HE2");
                    _dctSortInfo.Add("14", "CITRATE ALL");
                    _dctSortInfo.Add("15", "HbA1C+OTHER");
                    _dctSortInfo.Add("18", "떠보기");
                    _dctSortInfo.Add("19", "SLIDE");
                    _dctSortInfo.Add("20", "1차재검");
                    _dctSortInfo.Add("21", "2차재검");
                    _dctSortInfo.Add("22", "HbA1C");
                    _dctSortInfo.Add("23", "SPECIAL IMM");
                    _dctSortInfo.Add("24", "DATA ERR");
                    _dctSortInfo.Add("25", "ALL SPECIAL IMM");
                    _dctSortInfo.Add("26", "CBC+IMMUNO");
                    _dctSortInfo.Add("61", "ERROR");

                    break;
            }

            string strPath = AppDomain.CurrentDomain.BaseDirectory;
            string strFile = "SortInformation.ini";
            string strTemp;
            string[] aryTemp;

            try
            {
                StreamReader objReader = new StreamReader(strPath + strFile, Encoding.Default);
                while ((strTemp = objReader.ReadLine()) != null)
                {
                    if (strTemp != "")
                    {
                        aryTemp = strTemp.Split('\t');

                        string strData = "";

                        strData = aryTemp[1].ToUpper() + TAB + aryTemp[2].ToUpper() + TAB + aryTemp[3].ToUpper() + TAB + aryTemp[4].ToUpper() + TAB + aryTemp[5].ToUpper();

                        if (Constant.gdctSortingRules.ContainsKey(aryTemp[0].ToUpper()) == false)
                        {
                            Constant.gdctSortingRules.Add(aryTemp[0].ToUpper(), strData);
                        }
                        else
                        {
                            Constant.gdctSortingRules[aryTemp[0].ToUpper()] = Constant.gdctSortingRules[aryTemp[0].ToUpper()] + CR + strData;
                        }
                    }
                }
                objReader.Close();
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"GetSortInformation Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {

            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "App Start" + "\r\n",
                               false,
                               mstrAppPath + "log\\",
                               DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

            if (File.Exists(mstrAppPath + "ord.tmp")) { BASE_ORD = "Y"; }
            if (File.Exists(mstrAppPath + "dev.tmp")) { DEV_MODE = "Y"; }
            if (File.Exists(mstrAppPath + "stop.tmp")) { ORD_TMR_STOP = "Y"; }
            if (File.Exists(mstrAppPath + "scktimestamp.tmp")) { Constant.gblnLoggingTimeStamp = true; }

            InitializeAxWinSck();
            GetEqpCd();

            // Define the connection string to the SQLite database
            string connectionString = "Data Source=interface.db;Version=3;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                // Open the connection
                connection.Open();

                // Define a SQL command to create a table
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS SampleTray (
                        TraceSequenceNumber INTEGER NOT NULL,
                        TrayNumber INTEGER NOT NULL,
                        RackNumber INTEGER NOT NULL,
                        HoleNumber INTEGER NOT NULL CHECK(HoleNumber <= 125),
                        SampleNumber TEXT NOT NULL,
                        InsertDate TEXT NOT NULL,
                        PRIMARY KEY(TraceSequenceNumber, TrayNumber, RackNumber, HoleNumber, SampleNumber, InsertDate)
                    );
                ";

                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    // Execute the command to create the table
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table created successfully.");
                }

                // Close the connection
                connection.Close();
            }

            //for Test : 2024-11-13
            //int sequenceNumber = trayManager.RegisterNewTray("001064", Common.Val("1"));

            //13100005 서울본원
            //13900000 부산경남검사센터
            //14100000 대구경북검사센터
            //14300000 광주호남검사센터
            //14500000 대전충청검사센터
            switch (Constant.SG_TS_EQP_CD)
            {
                case "620":
                    Constant.gstrCenterCode = "13100005";
                    break;

                case "621":
                    Constant.gstrCenterCode = "13100005";
                    break;

                case "039":
                    Constant.gstrCenterCode = "13900000";
                    break;

                case "922":
                    Constant.gstrCenterCode = "14100000";
                    break;

                case "842":
                    Constant.gstrCenterCode = "14300000";
                    break;

                case "724":
                    Constant.gstrCenterCode = "14500000";
                    break;

                default:
                    //Constant.gstrCenterCode = "13100005";
                    this.Dispose();
                    Application.Exit();

                    break;
            }

            mtTitleDB.BackColor = Color.Tomato;
            mtTitleSck.BackColor = Color.Tomato;
            mtTitleNetwork.BackColor = Color.Tomato;
            mtTitleHIS.BackColor = Color.Tomato;

            // Define columns for the DataTable
            DataColumn rackNo = new DataColumn("rackNo", typeof(string));
            DataColumn rackPos = new DataColumn("rackPos", typeof(string));
            DataColumn spcNo = new DataColumn("spcNo", typeof(string));
            DataColumn ordInfo = new DataColumn("ordInfo", typeof(string));
            DataColumn inquiryType = new DataColumn("inquiryType", typeof(string));
            DataColumn unitNo = new DataColumn("unitNo", typeof(string));
            DataColumn sortInfo = new DataColumn("sortInfo", typeof(string));
            DataColumn inputDtTm = new DataColumn("inputDtTm", typeof(string));

            // Add columns to the DataTable
            gdtInquirySpcNoList.Columns.Add(rackNo);
            gdtInquirySpcNoList.Columns.Add(rackPos);
            gdtInquirySpcNoList.Columns.Add(spcNo);
            gdtInquirySpcNoList.Columns.Add(ordInfo);
            gdtInquirySpcNoList.Columns.Add(inquiryType);
            gdtInquirySpcNoList.Columns.Add(unitNo);
            gdtInquirySpcNoList.Columns.Add(sortInfo);
            gdtInquirySpcNoList.Columns.Add(inputDtTm);

            bool isDBConnected = false;
            //씨젠의료재단 차세대 DB 연결 안함
            //isDBConnected = CommonDB.MySqlConnect();
            isDBConnected = true;

            Lis.Interface.clsBizSeeGene objApi;
            //Util.Library.clsConstant.Config.ConnectionType = 0;
            Util.Library.clsConstant.Config.ConnectionType = 1;
            Util.Library.clsConstant.CheckLength.Barcode = 11;
            Util.Library.clsConstant.CheckLength.SpcNo = 12;
            Util.Library.clsConstant.Config.CenterCode = Constant.gstrCenterCode;
            Util.Library.clsConstant.Config.HttpWebRequestTimeOutMS = 60000;
            Util.Library.clsConstant.Config.YnUseSpcInfoHist = "N";

            objApi = new Lis.Interface.clsBizSeeGene();

            Constant.gstrComputerName = Environment.MachineName;

            if (DEV_IN_OFFICE == false || DEBUG_MODE == false)
            {
                metroButton1.Visible = false;
            }

            if (DEV_IN_OFFICE == false && DEBUG_MODE == false)
            {
                DataSet dsData = objApi.GetDevcChannel(SG_TS_EQP_CD);
                WriteDataSetToCSV(dsData, mstrAppPath + "BakDevcChannel\\");
                objApi = null;
            }

            if (isDBConnected == false)
            {
                lblDbStatus.ForeColor = Color.Tomato;
                mtTitleDB.BackColor = Color.Tomato;
                mtTitleHIS.BackColor = Color.Tomato;
                MessageBox.Show("인터페이스 데이터베이스 접속실패");
            }
            else
            {
                lblDbStatus.ForeColor = Color.LightGreen;
                mtTitleDB.BackColor = Color.LightGreen;
                mtTitleHIS.BackColor = Color.LightGreen;

                NetworkChk();

                //2024-04-19 : 이건 임시
                //GetRackInformation();

                //사이트별로 ini파일로 관리 (단순 분류명 화면에 표시용도)
                GetSortInformation();

                mtGrdList.Rows.Clear();
                mtGrdList.Refresh();

                if (DEV_MODE == "Y") { mtEtc.Visible = true; mtEtc.Text = "1024"; }
                if (ORD_TMR_STOP == "Y") { mtEtc.Visible = true; mtEtc.Text = mtEtc.Text + " Stop"; }

                if (Constant.DEBUG_MODE)
                {
                    lblOrdTmr.Text = "Off";
                    tmrOrder.Enabled = true;
                }
                else
                {
                    lblOrdTmr.Text = "On";
                    //btnDevTest.Visible = false;
                    txtRcv.Visible = false;
                    txtSnd.Visible = false;
                    lblDbStatus.Visible = false;
                    lblSckStatus.Visible = false;
                    lblHisType.Visible = false;
                    lblSetFirstOrdTmr.Visible = false;
                    lblOrdTmr.Visible = false;

                    // 중요!!! 오더타이머 반드시 활성화
                    if (ORD_TMR_STOP == "N") { tmrOrder.Enabled = true; }
                }

                tmrBgw.Enabled = true;

                foreach (DataGridViewColumn i in mtGrdList.Columns)
                {
                    i.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }

            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "Main Load" + "\r\n",
                   false,
                   mstrAppPath + "log\\",
                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("프로그램을 종료하시겠습니까?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            CommonDB.DBClose();

            tmrOrder.Enabled = false;

            if (AxWinSckServer != null)
            {
                AxWinSckServer.DisPoseSck();
                AxWinSckServer = null;
            }

            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"Main_FormClosing" + "\r\n",
                               false,
                               mstrAppPath + "log\\",
                               DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

            this.Dispose();
            Application.Exit();
        }

        private void tmrOrder_Tick(object sender, EventArgs e)
        {
            tmrOrder.Enabled = false;
            if (File.Exists(mstrAppPath + "test.tmp")) { Common.REC_MODE = "Y"; }

            // ---------------------------------------------------------------------------------
            // 1. set first order

            string strLog = "";
            string strRackNo = "";
            string strSpcNoList = "";
            string strInqTyp = "";
            string strUnitNo = "";
            int intRet = -1;
            string filterExpression;
            DataRow[] selectedRows;

            if (AxWinSckServer != null && (bool)AxWinSckServer.IsConnected())
            {
                lblSckStatus.BackColor = Color.LightGreen;
                mtTitleSck.BackColor = Color.LightGreen;
            }

            if (mstrUse != "equip")
            {
                if (Constant.DEBUG_MODE == false)
                {
                    DataView dataView = gdtInquirySpcNoList.DefaultView;
                    try
                    {
                        // 데이터 테이블이 null인지 먼저 확인
                        if (dataView?.Table == null)
                        {
                            Console.WriteLine("DataView 또는 Table이 null입니다");
                        }

                        // 데이터가 있는지 먼저 확인
                        if (dataView.Count == 0)
                        {
                            //Console.WriteLine("데이터가 없습니다");
                        }

                        DataTable tempTable = null;
                        DataRow[] topRows = null;

                        // 안전하게 ToTable 실행
                        try
                        {
                            tempTable = dataView.ToTable();

                            // 열이 DataView에 존재하는지 확인 및 정렬 시도
                            if (dataView.Table.Columns.Contains("inputDtTm"))
                            {
                                try
                                {
                                    dataView.Sort = "inputDtTm ASC";
                                    tempTable = dataView.ToTable();  // 정렬된 테이블 다시 얻기
                                }
                                catch (Exception sortEx)
                                {
                                    Console.WriteLine($"정렬 중 오류 발생: {sortEx.Message}");
                                    // 정렬 실패 시 정렬 없이 진행
                                }
                            }

                            // 데이터를 안전하게 추출
                            if (tempTable != null && tempTable.Rows.Count > 0)
                            {
                                topRows = tempTable.Rows.Cast<DataRow>().ToArray();
                            }
                        }
                        catch (Exception tableEx)
                        {
                            Console.WriteLine($"테이블 생성 중 오류: {tableEx.Message}");

                            // ToTable() 실패 시 직접 배열 생성 시도
                            try
                            {
                                topRows = new DataRow[dataView.Count];
                                for (int i = 0; i < dataView.Count; i++)
                                {
                                    try
                                    {
                                        topRows[i] = dataView[i].Row;
                                    }
                                    catch
                                    {
                                        // 개별 행 접근 오류 처리
                                        continue;
                                    }
                                }
                                // null 항목 제거
                                topRows = topRows.Where(r => r != null).ToArray();
                            }
                            catch (Exception rowEx)
                            {
                                Console.WriteLine($"행 접근 중 오류: {rowEx.Message}");
                                topRows = new DataRow[0]; // 빈 배열로 초기화
                            }
                        }

                        // 결과 데이터 처리
                        if (topRows != null && topRows.Length > 0)
                        {
                            DataRow topRow = topRows[0];

                            // 안전하게 값 추출
                            try
                            {
                                // 각 열이 존재하는지 확인하고 값 추출
                                string rackNoVal = topRow.Table.Columns.Contains("rackNo") ?
                                    (topRow["rackNo"] != DBNull.Value ? topRow["rackNo"].ToString() : "") : "";

                                string rackPosVal = topRow.Table.Columns.Contains("rackPos") ?
                                    (topRow["rackPos"] != DBNull.Value ? topRow["rackPos"].ToString() : "") : "";

                                string spcNoVal = topRow.Table.Columns.Contains("spcNo") ?
                                    (topRow["spcNo"] != DBNull.Value ? topRow["spcNo"].ToString() : "") : "";

                                Console.WriteLine($"rackNo: {rackNoVal}, rackPos: {rackPosVal}, spcNo: {spcNoVal}");

                                // 필요한 값 추출 (열 존재 여부 및 null 체크)
                                strRackNo = topRow.Table.Columns.Contains("rackNo") && topRow["rackNo"] != DBNull.Value ?
                                    topRow["rackNo"].ToString() : "";

                                strInqTyp = topRow.Table.Columns.Contains("inquiryType") && topRow["inquiryType"] != DBNull.Value ?
                                    topRow["inquiryType"].ToString() : "";

                                strUnitNo = topRow.Table.Columns.Contains("unitNo") && topRow["unitNo"] != DBNull.Value ?
                                    topRow["unitNo"].ToString() : "";
                            }
                            catch (Exception valueEx)
                            {
                                Console.WriteLine($"값 추출 중 오류: {valueEx.Message}");
                                // 기본값 설정
                                strRackNo = "";
                                strInqTyp = "";
                                strUnitNo = "";
                            }
                        }
                        else
                        {
                            //Console.WriteLine("처리할 데이터가 없습니다");
                            strRackNo = "";
                            strInqTyp = "";
                            strUnitNo = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"전체 처리 오류: {ex.Message}");
                        // 기본값 설정
                        strRackNo = "";
                        strInqTyp = "";
                        strUnitNo = "";
                    }

                    if (strRackNo != "")
                    {
                        filterExpression = "rackNo = " + Common.STS(strRackNo);
                        selectedRows = gdtInquirySpcNoList.Select(filterExpression);

                        foreach (DataRow row in selectedRows)
                        {
                            Console.WriteLine($"rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}");
                            if (strSpcNoList == "")
                            {
                                strSpcNoList = row["spcNo"].ToString();
                            }
                            else
                            {
                                strSpcNoList = strSpcNoList + "," + row["spcNo"].ToString();
                            }
                        }
                    }

                    if (strSpcNoList != "")
                    {
                        intRet = BizData.SetOrderInfo(strSpcNoList, strInqTyp, strUnitNo, strRackNo);

                        strLog = "";
                        strLog = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"BT Order Request RackNo: {strRackNo}, spcNoList: {strSpcNoList}";
                        strLog = strLog + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    }
                }
            }

            // ---------------------------------------------------------------------------------
            // 2. order sending...

            mintCount++;

            if (mstrUse == "equip")
            {
                if (mintCount == 1)
                {
                    if (mblnStop)
                    {
                        mblnStop = false;
                    }
                }
            }

            if (mintCount >= mintWaitTime)
            {
                if (Constant.DEBUG_MODE == false)
                {
                    OrderSend_Equip();
                }
            }

            tmrOrder.Enabled = true;
        }

        private void NetworkChk()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == true)
            {
                mtTitleHIS.BackColor = Color.LightGreen;
                mtTitleNetwork.BackColor = Color.LightGreen;
            }
            else
            {
                mtTitleNetwork.BackColor = Color.Tomato;
                mtTitleHIS.BackColor = Color.Tomato;
            }
        }

        private void GrdRowAdd(string pData)
        {

            if (DEBUG_MODE) { return; }

            try
            {
                Thread addRowThread = new Thread(() =>
                {
                    // Simulate some time-consuming operation
                    Thread.Sleep(1000);

                    // Add row to the DataGridView
                    if (mtGrdList.InvokeRequired)
                    {
                        mtGrdList.Invoke(new Action(() =>
                        {
                            if ((mtGrdList.Rows.Count) > 1000) { mtGrdList.Rows.Clear(); }
                            DataGridViewRow row = (DataGridViewRow)(mtGrdList.Rows[0].Clone());

                            row.Cells[(int)Constant.EGridColumn.SEQ].Value = mtGrdList.RowCount;
                            row.Cells[(int)Constant.EGridColumn.TYPE].Value = Common.P(pData, TAB, 1);
                            row.Cells[(int)Constant.EGridColumn.DATETIME].Value = Common.P(pData, TAB, 2);
                            row.Cells[(int)Constant.EGridColumn.EQUIPMENT].Value = Common.P(pData, TAB, 3);
                            row.Cells[(int)Constant.EGridColumn.SPCNO].Value = Common.P(pData, TAB, 4);
                            row.Cells[(int)Constant.EGridColumn.RACK].Value = Common.P(pData, TAB, 5);
                            row.Cells[(int)Constant.EGridColumn.POS].Value = Common.P(pData, TAB, 6);
                            row.Cells[(int)Constant.EGridColumn.RESULT].Value = Common.P(pData, TAB, 7);

                            mtGrdList.Rows.Insert(0, row);
                        }));
                    }
                    else
                    {
                        if ((mtGrdList.Rows.Count) > 1000) { mtGrdList.Rows.Clear(); }
                        DataGridViewRow row = (DataGridViewRow)(mtGrdList.Rows[0].Clone());

                        row.Cells[(int)Constant.EGridColumn.SEQ].Value = mtGrdList.RowCount;
                        row.Cells[(int)Constant.EGridColumn.TYPE].Value = Common.P(pData, TAB, 1);
                        row.Cells[(int)Constant.EGridColumn.DATETIME].Value = Common.P(pData, TAB, 2);
                        row.Cells[(int)Constant.EGridColumn.EQUIPMENT].Value = Common.P(pData, TAB, 3);
                        row.Cells[(int)Constant.EGridColumn.SPCNO].Value = Common.P(pData, TAB, 4);
                        row.Cells[(int)Constant.EGridColumn.RACK].Value = Common.P(pData, TAB, 5);
                        row.Cells[(int)Constant.EGridColumn.POS].Value = Common.P(pData, TAB, 6);
                        row.Cells[(int)Constant.EGridColumn.RESULT].Value = Common.P(pData, TAB, 7);

                        mtGrdList.Rows.Insert(0, row);
                    }
                });

                addRowThread.Start();
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"GrdRowAdd Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }
        }

        private void mtTitleLicense_Click(object sender, EventArgs e)
        {
            var from = new License();
            from.Show(this);
            System.Windows.Forms.Application.DoEvents();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //
        }

        private void tmrBgw_Tick(object sender, EventArgs e)
        {

            this.Text = SG_TS_EQP_CD + " CT90 결과처리 중 : " + _dctSetRackInfo.Count.ToString();

            if (_dctSetRackInfo != null)
            {
                if (_dctSetRackInfo.Count > 0)
                {
                    bool blnBusyWorker = false;
                    bool blnGo = false;

                    if (mWorkers != null)
                    {
                        blnBusyWorker = true;
                    }

                    if (blnBusyWorker)
                    {
                        if (mWorkers.IsBusy == false)
                        {
                            blnGo = true;
                        }
                    }
                    else
                    {
                        blnGo = true;
                    }

                    if (blnGo)
                    {
                        string strDiv = "SetRackNoInfo";
                        string strKey = "";
                        string strData = "";

                        foreach (KeyValuePair<string, string> kvp in _dctSetRackInfo)
                        {
                            strKey = kvp.Key;
                            strData = kvp.Value;
                            break;
                        }

                        mWorkers = new BackgroundWorker();
                        mWorkers.WorkerReportsProgress = true;
                        mWorkers.WorkerSupportsCancellation = true;
                        mWorkers.DoWork += WorkerDoWork;
                        mWorkers.ProgressChanged += WorkerProgressChanged;
                        mWorkers.RunWorkerCompleted += WorkerCompleted;
                        mWorkers.RunWorkerAsync(strDiv + DLM_HS + strKey + DLM_HS + strData);
                    }
                }
            }
        }

        static DataSet ConvertToDataSet(DataRow[] rows, string dataSetName)
        {
            // Create a new DataSet
            DataSet dataSet = new DataSet(dataSetName);

            // Create a new DataTable
            DataTable dataTable = new DataTable("CT90");

            // Clone the structure of the original DataTable
            if (rows.Length > 0)
            {
                dataTable = rows[0].Table.Clone();
            }

            // Add the DataTable to the DataSet
            dataSet.Tables.Add(dataTable);

            // Import the DataRow objects into the DataTable
            foreach (DataRow row in rows)
            {
                dataSet.Tables["CT90"].ImportRow(row);
            }

            return dataSet;
        }

        private string GetSortIndex_SeeGene_Seoul(string strSpcNo, string strRackNo)
        {
            string strSortIndex = "";

            try
            {
                bool blnTestCompleted = false;
                List<string> lstJobGroupCd = new List<string>();
                DataSet dsData = null;
                DataSet dsSorted = null;
                DataSet dsRsltXN = null;
                DataSet dsRsltHIS = null;
                DataSet dsRerunPassRack = null;

                string strLog;
                string strLine = new string('-', 20) + "\r\n";
                string strLisTstCds = "";
                string strLisTstSubCd = "";
                string strSortDesc = "";

                bool blnExistOrderCDR = false;
                bool blnNoOrder = true;
                bool blnExistResultCDR = false;
                bool blnNextStep = false;
                bool blnSorted = false;
                bool blnApplicator = false;
                bool blnSlide = false;
                bool blnRerunFirst = false;
                bool blnRerunSecond = false;
                bool blnRerunPassRack = false;

                //2025-01-23
                bool blnImmuno = false;
                bool blnSpecialImmuno = false;

                //2025-02-27 : 단독여부 체크할 것
                bool blnFetalHB = false;
                int intHEB = 0;

                string spcGbn = "";
                string filterExpression;
                DataRow[] selectedRows;

                //2025-03-13 : 말라리아 추가
                bool blnMalaria = false;

                //2025-03-26 : PB
                bool blnPB = false;

                //2025-05-27 : HE 여부 체크
                List<string> lstChkHE = new List<string>();

                if (strSpcNo.Length == 12)
                {
                    spcGbn = strSpcNo[11].ToString(); // 문자열의 12번째 문자 (0부터 시작하므로 11번째 인덱스)
                }

                strLog = $"Start GetSortIndex(서울) SpcNo: {strSpcNo}, RackNo: {strRackNo}" + "\r\n"; ;

                Common.File_Record("\r\n", false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                #region ----- 씨젠의료재단 서울 센터 SYSMEX TS-10 분류코드
                //2024-01-26 : 씨젠의료재단 서울 센터 진단검사의학과 혈액학부 SYSMEX TS-10 검체 분류 및 아카이브
                //TS-10 Sort No
                //1  = 아카이브
                //2  = PB + CBC + OTHER
                //3  = PB + CBC + RETI
                //4  = OTHER
                //5  = CBC + OTHER
                //6  = ESR = HE0
                //7  = CBC + ABO + ESR
                //8  = ABO = HE8
                //9  = CITRATE + HE4
                //10 = CBC + ESR
                //11 = CBC + ABO
                //12 = CITRATE + HE5
                //13 = CITRATE + HE2
                //14 = CITRATE + ALL5
                //15 = HbA1C + OTHER
                //16 = 사용안함
                //17 = 사용안함
                //18 = 60                       떠보기기준
                //19 = SLIDE30                  슬라이드기준
                //20 = 1SAMPLE30                1차재검
                //21 = 2SAMPLES                 2차재검
                //22 = HbA1C
                //23 = SPECIAL IMM
                //24 = DATE ERR
                //25 = ALL SPECIAL IMM
                //26 = CBC + IMMUNO (IMH 또는 SE0)
                #endregion ----- 씨젠의료재단 서울 센터 SYSMEX TS-10 분류코드

                #region ----- 1. 바코드에러 = 61

                //1. 바코드에러 
                if (Common.IsNumeric(strSpcNo) == false)
                {
                    strSortIndex = "61";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 바코드에러, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 1. 바코드에러 = 61

                #region ----- 2. 아카이브 모드
                if (chkArchive.Checked == true)
                {
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Archive Mode, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }
                #endregion ----- 2. 아카이브 모드

                #region ----- 3. 오더조회

                strLog += "JOB_GRUP" + TAB + "LIS_TST_CD" + TAB + "LIS_TST_SUB_CD" + TAB + "TST_NM" + TAB + "STUS_CD" + "\r\n";

                //2025-03-13 : 5번 시퀀스일 때는 12자리로 오더조회
                dsData = BizData.GetSpcInfo(strSpcNo);

                //2025-03-27 : 데이터 에러처리
                filterExpression = "SPC_CD = 'E01' ";

                if (dsData != null && dsData.Tables.Count > 0)
                {
                    selectedRows = dsData.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        foreach (DataRow drTemp in selectedRows)
                        {
                            if (drTemp["JOB_GRUP"].ToString().Trim() == "HE6")
                            {
                                if (drTemp["LIS_TST_CD"].ToString().Trim() == "11023")
                                {
                                    //무조건 데이터 에러로 빠지게
                                    strSortIndex = "24";
                                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error(말라리아 E01검체), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                    return strSortIndex;
                                }
                            }
                        }
                    }
                }

                if (spcGbn == "5")
                {
                    filterExpression = "SPC_CD = 'A04' ";
                    if (dsData != null && dsData.Tables.Count > 0)
                    {
                        selectedRows = dsData.Tables[0].Select(filterExpression);

                        if (selectedRows != null && selectedRows.Length > 0)
                        {
                            //오더 조회되었으므로 정상 진행
                        }
                        else
                        {
                            //오더 조회 안되었으니 Data Error 로 분류
                            strSortIndex = "24";
                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}, 5번 검체이면서 Citrate(A04) 오더조회 안됨!" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            return strSortIndex;
                        }
                    }
                    else
                    {
                        //오더 조회 안되었으니 Data Error 로 분류
                        strSortIndex = "24";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}, 5번 검체이면서 Citrate(A04) 오더조회 안됨!" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }
                }

                //2025-02-17 : 무조건 데이터에러로 처리
                //              3610 추후송부
                //              3630 우선검사진행
                //              9070 검사제외
                filterExpression = "TST_STAT_CD IN ('3610', '3630', '9070') ";
                if (dsData != null && dsData.Tables.Count > 0)
                {
                    selectedRows = dsData.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        foreach (DataRow drTemp in selectedRows)
                        {
                            if (drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                strSortIndex = "24";
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }
                        }
                    }
                }

                filterExpression = "CNTR_CD = '13100005'";
                if (dsData != null && dsData.Tables.Count > 0)
                {
                    selectedRows = dsData.Tables[0].Select(filterExpression);

                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        foreach (DataRow drTemp in selectedRows)
                        {
                            bool blnChk = false;

                            //2025-05-26 : 타학부만 있는 지 체크하기 위해 추가
                            if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstChkHE.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                            {
                                lstChkHE.Add(drTemp["JOB_GRUP"].ToString().Trim());
                            }

                            //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                            if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                            {
                                System.Diagnostics.Debug.Print(drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["SPC_CD"].ToString().Trim());

                                if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                {
                                    if (spcGbn == "5")
                                    {
                                        if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                    else
                                    {
                                        if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "")
                                    {
                                        //if (aryTemp[i].IndexOf("RET") > -1)
                                        if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                        {
                                            if (spcGbn == "5")
                                            {
                                                if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                                {
                                                    blnChk = true;
                                                }
                                            }
                                            else
                                            {
                                                if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                                {
                                                    blnChk = true;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (blnChk == true)
                                {
                                    if (string.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                    {
                                        blnNoOrder = false;
                                    }

                                    strLisTstSubCd = drTemp["LIS_TST_SUB_CD"].ToString();
                                    if (strLisTstSubCd == "-") { strLisTstSubCd = ""; }
                                    strLisTstCds = drTemp["LIS_TST_CD"].ToString() + strLisTstSubCd;

                                    //11310 = Reti count
                                    //11017 = Eosinophil count (호산구수)
                                    if (drTemp["JOB_GRUP"].ToString().Trim() == "HE1") { blnExistOrderCDR = true; }
                                    if (drTemp["JOB_GRUP"].ToString().Trim() == "HE3" && strLisTstCds == "11310" && blnExistOrderCDR == false) { blnExistOrderCDR = true; }
                                    if (drTemp["JOB_GRUP"].ToString().Trim() == "HE9" && strLisTstCds == "11017" && blnExistOrderCDR == false) { blnExistOrderCDR = true; }

                                    //2025-02-27
                                    if (strLisTstCds == "00602" && blnFetalHB == false)
                                    {
                                        blnFetalHB = true;
                                    }

                                    //2025-03-13 : 말라리아 추가
                                    if (drTemp["JOB_GRUP"].ToString().Trim() == "HE6")
                                    {
                                        if (strLisTstCds == "11023")
                                        {
                                            if (blnMalaria == false) { blnMalaria = true; }
                                        }
                                        else
                                        {
                                            blnPB = true;
                                        }
                                    }

                                    if (drTemp["JOB_GRUP"].ToString().Trim() == "HEB")
                                    {
                                        intHEB++;
                                    }

                                    strLog += drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["LIS_TST_CD"].ToString().Trim() + TAB + drTemp["LIS_TST_SUB_CD"].ToString().Trim() + TAB;
                                    strLog += drTemp["TST_NM"].ToString().Trim() + TAB + drTemp["STUS_CD"].ToString().Trim() + "\r\n";
                                }
                            }
                        }
                    }
                }

                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                #endregion ----- 3. 오더조회

                #region ----- 4. 오더조회 안됨 = 24

                if (blnNoOrder == true)
                {
                    dsData = null;
                    strSortIndex = "24";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, No Order(Data Error), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 4. 오더조회 안됨 = 24

                if (lstChkHE.Any(x => x.StartsWith("HE")))
                {
                    //
                }
                else
                {
                    if (lstChkHE.Count > 0)
                    {
                        dsData = null;
                        strSortIndex = "24";

                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 4.1 타학부 검체, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }
                }

                #region ----- 5. CDR 오더가 있으나 XN에서 검사한 내역이 없을 경우 LINE OUT = 0

                dsRsltXN = BizData.GetRsltHematology(strSpcNo);
                if (dsRsltXN != null && dsRsltXN.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsRsltXN.Tables[0].Rows)
                    {
                        blnExistResultCDR = true;
                        break;
                    }
                }

                if (blnExistOrderCDR == true && blnExistResultCDR == false)
                {
                    dsData = null;
                    dsRsltXN = null;
                    strSortIndex = "0";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, CDR 오더가 있으나 검사결과가 없을 경우, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 5. CDR 오더가 있으나 XN에서 검사한 내역이 없을 경우 LINE OUT

                #region ----- 6. 재검통과 랙인지 확인하기 (* XN검사결과 판정결과가 재검대상일 경우 계속 재검으로 분류되므로 특정 랙일 경우 체크 안함)

                if (Common.IsNumeric(strRackNo)) { strRackNo = Common.Val(strRackNo).ToString(); }

                //2024-04-19 : SeeGene 리런패스랙 관리 테이블 조회
                //if (glstNextStepRack.Contains(strRackNo)) { blnNextStep = true; }

                dsRerunPassRack = BizData.GetRerunPassRack(Constant.gstrCenterCode, strRackNo);
                if (dsRerunPassRack != null && dsRerunPassRack.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsRerunPassRack.Tables[0].Rows)
                    {
                        if (strRackNo == drTemp["RACK_NO"].ToString())
                        {
                            blnNextStep = true;
                            blnRerunPassRack = true;
                            break;
                        }
                    }
                }

                #endregion ----- 6. 재검통과 랙인지 확인하기 (* XN검사결과 판정결과가 재검대상일 경우 계속 재검으로 분류되므로 특정 랙일 경우 체크 안함)

                #region ----- 7. 기분류된 내역 유무 조회

                dsSorted = BizData.GetSpcPos(strSpcNo);
                if (dsSorted != null && dsSorted.Tables.Count > 0)
                {
                    filterExpression = "tsGrupNo = " + Common.STS("18");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnApplicator = true; blnNextStep = true; }

                    filterExpression = "tsGrupNo = " + Common.STS("19");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnSlide = true; blnNextStep = true; }

                    filterExpression = "tsGrupNo = " + Common.STS("20");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnRerunFirst = true; blnNextStep = true; }

                    filterExpression = "tsGrupNo NOT IN ('18','19','20','21','24','0','1','') ";
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        blnSorted = true;
                    }

                    DataView view = dsSorted.Tables[0].DefaultView;
                    view.Sort = "regDtm DESC";

                    // Create a new DataTable from the sorted view
                    DataTable sortedTable = view.ToTable();

                    foreach (DataRow drTemp in sortedTable.Rows)
                    {
                        if (string.IsNullOrEmpty(drTemp["tsGrupNo"].ToString()) == false)
                        {

                            if (drTemp["tsGrupNo"].ToString().Trim() == "18") { blnApplicator = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "19") { blnSlide = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "20") { blnRerunFirst = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "21") { blnRerunSecond = true; }

                            if (drTemp["tsGrupNo"].ToString().Trim() != "18" && drTemp["tsGrupNo"].ToString().Trim() != "19" &&
                                drTemp["tsGrupNo"].ToString().Trim() != "20" && drTemp["tsGrupNo"].ToString().Trim() != "21" && drTemp["tsGrupNo"].ToString().Trim() != "24" &&
                                drTemp["tsGrupNo"].ToString().Trim() != "0" && drTemp["tsGrupNo"].ToString().Trim() != "1")
                            {
                                blnSorted = true;
                            }
                            break;
                        }
                    }
                }

                //2024-11-13 : lsh 재검일때 빨간랙태워서 다음타겟(ESR로) 갔을때 일반랙사용하면 Archive 되어야 된다고합니다..... 빨간랙이 재검패스랙인듯
                if (blnRerunFirst == true)
                {
                    if (blnRerunPassRack == true)
                    {
                        //1. 리런패스랙이면 다음 단계로 진행
                        blnNextStep = true;
                    }
                    else
                    {
                        //2. 일반랙일 경우 다시 재검으로 분류

                        blnNextStep = false;

                        //2.1 하지만 분류이력 중 리런이 아닌 다른 곳으로 분류된 이력이 있다면 !!! 다음 타겟
                        filterExpression = "tsGrupNo NOT IN ('20','21','') ";
                        selectedRows = dsSorted.Tables[0].Select(filterExpression);
                        if (selectedRows != null && selectedRows.Length > 0)
                        {
                            blnNextStep = true;
                        }
                    }
                }

                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, ExistResultCDR: {blnExistResultCDR}, NextStep: {blnNextStep}{CR}";
                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                #endregion ----- 7. 기분류된 내역 유무 조회

                #region ----- 8. CDR 오더가 있으나 XN에서 검사한 내역이 있을 경우 (떠보기 = 18, 슬라이드 = 19, 1차재검 = 20, 2차재검 = 21) 체크 && 특정 랙이 아닐 경우

                if (blnExistResultCDR == true && blnNextStep == false)
                {
                    if (dsRsltXN != null && dsRsltXN.Tables.Count > 0)
                    {
                        int chkReTest = 0;

                        var results = dsRsltXN.Tables[0].AsEnumerable()
                                        .Where(row => row.Field<string>("rtstJgmtVal") == "R")
                                        .GroupBy(row => DateTime.ParseExact(row.Field<string>("rsltDtm"),
                                            "yyyy-MM-dd HH:mm:ss",
                                            CultureInfo.InvariantCulture))
                                        .Select(group => new
                                        {
                                            ResultDateTime = group.Key,
                                            RetestCount = group.Count()
                                        });

                        foreach (var result in results)
                        {
                            chkReTest += 1;
                        }

                        if (chkReTest > 0)
                        {
                            if (chkReTest == 1)
                            {
                                strSortIndex = "20";
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}";
                            }
                            else
                            {
                                strSortIndex = "21";
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}";
                            }
                        }

                        if (blnSlide == false && strSortIndex == "")
                        {
                            filterExpression = "slid1JgmtVal in ('S1', 'S2')";

                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                foreach (DataRow row in selectedRows)
                                {
                                    strSortIndex = "19";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, S1: {row["slid1JgmtVal"]}, S2: {row["slid2JgmtVal"]}";
                                    break;
                                }
                            }
                        }

                        if (blnApplicator == false && strSortIndex == "")
                        {
                            filterExpression = "apctrJgmtVal = 'A'";

                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                foreach (DataRow row in selectedRows)
                                {
                                    strSortIndex = "18";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, 떠보기판정: {row["apctrJgmtVal"]}";
                                    break;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(strSortIndex) == false)
                        {
                            dsData = null;
                            dsRsltXN = null;
                            dsSorted = null;

                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog += $", 분류명: {strSortDesc}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                            return strSortIndex;
                        }
                    }
                }

                #endregion ----- 8. CDR 오더가 있으나 XN에서 검사한 내역이 있을 경우 (떠보기 = 18, 슬라이드 = 19, 1차재검 = 20, 2차재검 = 21) 체크 && 특정 랙이 아닐 경우

                #region ----- 9. 기분류된 검체일 경우 보관 = 1

                if (blnSorted == true)
                {
                    filterExpression = "JOB_GRUP IN ('IMH', 'SE0') AND SPC_CD IN('A02','A04','A05')";
                    if (dsData != null && dsData.Tables.Count > 0)
                    {
                        selectedRows = dsData.Tables[0].Select(filterExpression);

                        if (selectedRows != null && selectedRows.Length > 0)
                        {
                            blnImmuno = true;
                            blnSorted = false;
                        }

                        filterExpression = "JOB_GRUP = 'RC2' AND SPC_CD IN('A02','A04','A05')";
                        selectedRows = dsData.Tables[0].Select(filterExpression);

                        if (selectedRows != null && selectedRows.Length > 0)
                        {
                            blnSpecialImmuno = true;
                            blnSorted = false;
                        }

                        if (blnSorted == true && lstJobGroupCd.Any(x => x.StartsWith("HE")))
                        {
                            dsData = null;
                            dsRsltXN = null;
                            dsSorted = null;
                            strSortIndex = "1";
                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(기분류), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            return strSortIndex;
                        }
                    }
                }

                #endregion ----- 9. 기분류된 검체일 경우 보관 = 1

                #region ----- 10. 오더에 따른 분류

                filterExpression = "CNTR_CD = '13100005'";
                if (dsData != null && dsData.Tables.Count > 0)
                {
                    selectedRows = dsData.Tables[0].Select(filterExpression);

                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        foreach (DataRow drTemp in selectedRows)
                        {
                            bool blnChk = false;

                            //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                            if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                            {
                                System.Diagnostics.Debug.Print(drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["SPC_CD"].ToString().Trim());

                                if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                {
                                    if (spcGbn == "5")
                                    {
                                        if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                    else
                                    {
                                        if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "")
                                    {
                                        //if (aryTemp[i].IndexOf("RET") > -1)
                                        if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                        {
                                            if (spcGbn == "5")
                                            {
                                                if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                                {
                                                    blnChk = true;
                                                }
                                            }
                                            else
                                            {
                                                if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                                {
                                                    blnChk = true;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (blnChk == true)
                                {
                                    if (drTemp["JOB_GRUP"].ToString().Trim() == "IMH" || drTemp["JOB_GRUP"].ToString().Trim() == "SE0")
                                    {
                                        blnImmuno = true;
                                    }

                                    if (drTemp["JOB_GRUP"].ToString().Trim() == "RC2")
                                    {
                                        blnSpecialImmuno = true;
                                    }

                                    if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                    {
                                        switch (drTemp["JOB_GRUP"].ToString().Trim())
                                        {
                                            case "HEA":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "HEB":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "HE0":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "HE1":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "HE2":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "HE3":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "HE4":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "HE5":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "HE6":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "HE7":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "HE8":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "HE9":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "PA6":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "PA7":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "PA8":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            case "IMH":
                                                lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                                break;

                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(strSortIndex) == false)
                        {
                            dsData = null;
                            dsRsltXN = null;
                            dsSorted = null;

                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog += $", 분류명: {strSortDesc}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                            return strSortIndex;
                        }

                        if (blnExistOrderCDR == true && blnExistResultCDR && lstJobGroupCd.Contains("HE1") == false)
                        {
                            dsRsltHIS = BizData.GetRslt(strSpcNo);
                            if (dsRsltHIS != null && dsRsltHIS.Tables.Count > 0)
                            {
                                foreach (DataRow drTemp in dsRsltHIS.Tables[0].Rows)
                                {
                                    lstJobGroupCd.Add("HE1");
                                    break;
                                }
                            }
                        }

                        //2025-01-23 : C+D 결과가 있을 경우 분류조건 삭제
                        if (blnExistResultCDR && lstJobGroupCd.Contains("HE1") == true && blnFetalHB == false)
                        {
                            //2025-02-19 : CBC+DIFF 결과 나왔을 경우 HE9 도 삭제
                            if (lstJobGroupCd.Contains("HE9") == true)
                            {
                                lstJobGroupCd.Remove("HE9");

                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: HE9삭제" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            }

                            lstJobGroupCd.Remove("HE1");
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: HE1삭제" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        }

                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, ExistResultCDR {blnExistResultCDR}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                        //2025-01-13 : 이송현 대리 요청으로 RET 검사결과 나왔으면 RET 잡그룹 삭제해서 분류처리하도록 수정

                        //DIFF항목이나 RETI 떄문에 Other로 가는걸까요 ??
                        //요청하신건 CBC + Other를 안쓰는 방향으로 말씀하셨습니다.
                        //1.CBC + RETI 있는경우는 CBC +Other로 분류되는데 검사진행하고 정상검체면 Archive로 분류
                        //2.CBC + RETI + ESR 인 경우 CBC + RETI 찍었으니 CBC+ESR로 분류
                        //3.CBC + DIFF + ESR 인 경우 CBC + RETI 찍었으니 CBC+ESR로 분류

                        bool blnExistRetiRslt = false;

                        if (blnExistResultCDR && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == false)
                        {
                            filterExpression = "devcRsltChnl = 'RET%'";
                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                lstJobGroupCd.Remove("HE3");
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: HE3삭제" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            }
                        }

                        if (blnExistResultCDR == true)
                        {
                            filterExpression = "devcRsltChnl = 'RET%'";
                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                blnExistRetiRslt = true;
                            }
                        }

                        //2025-03-01 : PB 분류이력있으면 삭제
                        if (lstJobGroupCd.Contains("HE6") == true)
                        {
                            filterExpression = "tsGrupNo IN ('2','3') ";
                            selectedRows = dsSorted.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                lstJobGroupCd.Remove("HE6");
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: HE6삭제" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            }
                        }

                        //2025-03-01 : RET 분류이력있으면 삭제
                        if (lstJobGroupCd.Contains("HE3") == true)
                        {
                            filterExpression = "tsGrupNo IN ('3') ";
                            selectedRows = dsSorted.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                lstJobGroupCd.Remove("HE3");
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: HE3삭제" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            }
                        }

                        //2025-05-28 : A1C 분류이력있으면 삭제
                        if (lstJobGroupCd.Contains("HEB") == true)
                        {
                            filterExpression = "tsGrupNo IN ('15','22') ";
                            selectedRows = dsSorted.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                lstJobGroupCd.Remove("HEB");
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: HEB삭제" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            }
                        }

                        //2025-01-23 : 새로 추가
                        // "IMH", "SE0", "RC2"를 제거하고 "HE"로 시작하는 값만 유지
                        if (lstJobGroupCd.Any(x => x.StartsWith("HE")))
                        {
                            // IMH, SE0, RC2를 리스트에서 제거
                            lstJobGroupCd = lstJobGroupCd.Where(x => !x.StartsWith("IMH") && !x.StartsWith("SE0") && !x.StartsWith("RC2"))
                                                         .ToList();

                            // "HE"로 시작하는 값만 남기기
                            lstJobGroupCd = lstJobGroupCd.Where(x => x.StartsWith("HE")).Distinct().ToList();
                        }

                        if (lstJobGroupCd.Count > 0)
                        {
                            foreach (string job in lstJobGroupCd)
                            {
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: {job}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            }
                        }

                        //혈액학부팀장님 요청.
                        //-혈액학 모든오더 없고 타학부 오더 있을경우는 무조건 Data Error
                        //-혈액학 모든오더(선행분류 후) + 핵의학오더(RC2) > 23번 SPECIAL IMM 분류
                        //-혈액학 모든오더(선행분류 후) + 면역학(IMH, SE0) > 26번 CBC +IMMUNO(IMH 또는 SE0) 분류
                        //-혈액학 모든오더(선행분류 후) + 면역학(IMH, SE0) + 핵의학오더(RC2) > 26번 CBC +IMMUNO(IMH 또는 SE0) 분류
                        //25번 ALL SPECIAL IMM  사용하지 않음 만약에 25번으로 분류되는게 있다면 23번으로 분류.

                        if (lstJobGroupCd.Any(x => x.StartsWith("HE")))
                        {
                            //
                        }
                        else
                        {
                            if (lstJobGroupCd.Count > 0)
                            {

                                strSortIndex = "24";

                                //2025-03-26 : IMH 있을 경우 23번으로 분류
                                bool blnIMH = false;
                                bool blnRC2 = false;
                                bool blnSortedHE = false;

                                filterExpression = "JOB_GRUP IN ('IMH', 'SE0') AND SPC_CD IN('A02','A04','A05')";
                                selectedRows = dsData.Tables[0].Select(filterExpression);
                                if (selectedRows != null && selectedRows.Length > 0)
                                {
                                    blnIMH = true;
                                }

                                filterExpression = "JOB_GRUP IN ('RC2') AND SPC_CD IN('A02','A04','A05')";
                                selectedRows = dsData.Tables[0].Select(filterExpression);
                                if (selectedRows != null && selectedRows.Length > 0)
                                {
                                    blnRC2 = true;
                                }

                                filterExpression = "tsGrupNo IN ('2','3','5','6','7','8','9','10','11','12','13','14','15','18','19','20','21','22','26')";
                                selectedRows = dsSorted.Tables[0].Select(filterExpression);
                                if (selectedRows != null && selectedRows.Length > 0)
                                {
                                    //-혈액학 모든오더(선행분류 후) + 핵의학오더(RC2) > 23번 SPECIAL IMM 분류
                                    //-혈액학 모든오더(선행분류 후) + 면역학(IMH, SE0) > 26번 CBC +IMMUNO(IMH 또는 SE0) 분류
                                    //-혈액학 모든오더(선행분류 후) + 면역학(IMH, SE0) + 핵의학오더(RC2) > 26번 CBC +IMMUNO(IMH 또는 SE0) 분류
                                    blnSortedHE = true;

                                    if (blnRC2 == true && blnIMH == false)
                                    {
                                        strSortIndex = "23";
                                    }
                                    else
                                    {
                                        if (blnIMH)
                                        {
                                            strSortIndex = "26";
                                        }
                                    }

                                }
                                else
                                {
                                    // 그대로 DataError
                                    if (blnExistOrderCDR == true || blnExistResultCDR == true) { strSortIndex = "26"; }
                                }

                                dsData = null;
                                dsRsltXN = null;
                                dsSorted = null;
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 혈액학 선행분류: {blnSortedHE}, RC2: {blnRC2}, IMH: {blnIMH}, 타학부 검체, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }
                        }

                        if (lstJobGroupCd.Count == 0)
                        {
                            strSortIndex = "24";

                            //2025-03-26 : IMH 있을 경우 23번으로 분류
                            bool blnIMH = false;
                            bool blnRC2 = false;
                            bool blnSortedHE = false;

                            filterExpression = "JOB_GRUP IN ('IMH', 'SE0') AND SPC_CD IN('A02','A04','A05')";
                            selectedRows = dsData.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                blnIMH = true;
                            }

                            filterExpression = "JOB_GRUP IN ('RC2') AND SPC_CD IN('A02','A04','A05')";
                            selectedRows = dsData.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                blnRC2 = true;
                            }

                            filterExpression = "tsGrupNo IN ('2','3','5','6','7','8','9','10','11','12','13','14','15','18','19','20','21','22','26')";
                            selectedRows = dsSorted.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                //-혈액학 모든오더(선행분류 후) + 핵의학오더(RC2) > 23번 SPECIAL IMM 분류
                                //-혈액학 모든오더(선행분류 후) + 면역학(IMH, SE0) > 26번 CBC +IMMUNO(IMH 또는 SE0) 분류
                                //-혈액학 모든오더(선행분류 후) + 면역학(IMH, SE0) + 핵의학오더(RC2) > 26번 CBC +IMMUNO(IMH 또는 SE0) 분류
                                blnSortedHE = true;

                                if (blnRC2 == true && blnIMH == false)
                                {
                                    strSortIndex = "23";
                                }
                                else
                                {
                                    if (blnIMH)
                                    {
                                        strSortIndex = "26";
                                    }
                                }

                            }
                            else
                            {
                                // 그대로 DataError
                                if (blnExistResultCDR == true)
                                {
                                    if (blnRC2 == true && blnIMH == false)
                                    {
                                        strSortIndex = "23";
                                    }
                                    else
                                    {
                                        if (blnIMH)
                                        {
                                            strSortIndex = "26";
                                        }
                                    }
                                }
                            }

                            if (strSortIndex == "23" || strSortIndex == "26")
                            {
                                dsData = null;
                                dsRsltXN = null;
                                dsSorted = null;
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 혈액학 선행분류: {blnSortedHE}, RC2: {blnRC2}, IMH: {blnIMH}, 타학부 검체, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }
                            else
                            {
                                if (blnExistResultCDR == true) { strSortIndex = "1"; }
                            }

                            if (blnSorted == false)
                            {
                                filterExpression = "JOB_GRUP IN ('IMH', 'SE0') AND SPC_CD IN('A02','A04','A05')";
                                selectedRows = dsData.Tables[0].Select(filterExpression);

                                //if (dsData != null && dsData.Tables.Count > 0)
                                if (selectedRows != null && selectedRows.Length > 0)
                                {
                                    blnImmuno = true;
                                    blnSorted = false;
                                }

                                filterExpression = "JOB_GRUP = 'RC2' AND SPC_CD IN('A02','A04','A05')";
                                selectedRows = dsData.Tables[0].Select(filterExpression);

                                //if (dsData != null && dsData.Tables.Count > 0)
                                if (selectedRows != null && selectedRows.Length > 0)
                                {
                                    blnSpecialImmuno = true;
                                    blnSorted = false;
                                }
                            }

                            //2025-01-23
                            if (blnImmuno == true)
                            {
                                dsData = null;
                                dsRsltXN = null;
                                dsSorted = null;
                                strSortIndex = "26";
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }

                            if (blnSpecialImmuno == true)
                            {
                                dsData = null;
                                dsRsltXN = null;
                                dsSorted = null;
                                strSortIndex = "23";
                                //if (blnExistResultCDR == true) { strSortIndex = "25"; }
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }

                            if (strSortIndex == "24")
                            {
                                //1. 처음에 데이터에러 일 때 다음에 태울 때 데이터에러로
                                //2. 타학부 데이터에러 인데 다음에 태울 때 데이터에러로

                                filterExpression = "SPC_CD IN('A02','A04','A05') AND TST_STAT_CD < '3060'";
                                selectedRows = dsData.Tables[0].Select(filterExpression);
                                if (selectedRows != null && selectedRows.Length > 0)
                                {
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, SortedHE: {blnSortedHE}" + "\r\n";
                                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                    if (blnSortedHE == true)
                                    {
                                        filterExpression = "JOB_GRUP NOT LIKE 'HE%' AND SPC_CD IN('A02','A04','A05') AND TST_STAT_CD < '3060'";
                                        selectedRows = dsData.Tables[0].Select(filterExpression);
                                        if (selectedRows != null && selectedRows.Length > 0)
                                        {
                                            strSortIndex = "4";
                                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 타학부 처리, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                        }
                                        else
                                        {
                                            strSortIndex = "1";
                                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사완료로 아카이브처리, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                        }

                                        dsData = null;
                                        dsRsltXN = null;
                                        dsSorted = null;

                                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                        return strSortIndex;
                                    }
                                }
                                else
                                {
                                    strSortIndex = "1";

                                    dsData = null;
                                    dsRsltXN = null;
                                    dsSorted = null;

                                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사완료로 아카이브처리, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                    return strSortIndex;
                                }
                            }
                        }

                        if (lstJobGroupCd.Count == 1)
                        {
                            if (lstJobGroupCd.Contains("HE3") == true)
                            {
                                if (blnExistOrderCDR == true && blnExistResultCDR == true)
                                {
                                    dsData = null;
                                    dsRsltXN = null;
                                    dsSorted = null;
                                    strSortIndex = "1";
                                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(XN검사완료), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                    return strSortIndex;
                                }
                                else
                                {
                                    strSortIndex = "4";
                                }
                            }
                            if (lstJobGroupCd.Contains("HE6") == true)
                            {
                                if (blnExistResultCDR == true)
                                {
                                    //CBC결과가 나왔으면 으로 분류
                                    //strSortIndex = "2";
                                    //strSortIndex = "3";

                                    if (blnMalaria == true)
                                    {
                                        strSortIndex = "5";

                                        //2025-03-26 : PB + 말라리아 = PB + CBC + OTHER
                                        if (blnPB == true) { strSortIndex = "2"; }

                                        //2025-03-26 : HE1 없을 경우 그냥 Other 로 분류
                                        if (strSortIndex == "5")
                                        {
                                            filterExpression = "JOB_GRUP IN ('HE1','HE3') AND SPC_CD IN('A02','A04','A05')";
                                            selectedRows = dsData.Tables[0].Select(filterExpression);
                                            if (selectedRows != null && selectedRows.Length > 0)
                                            {
                                                //그대로 분류
                                            }
                                            else
                                            {
                                                strSortIndex = "4";
                                            }
                                        }

                                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, CBC+OTHER, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                    }
                                    else
                                    {
                                        if (blnExistRetiRslt == true)
                                        {
                                            strSortIndex = "3";
                                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, PB+CBC+RET, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                        }
                                        else
                                        {
                                            strSortIndex = "2";
                                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, PB+CBC+OTHER, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                        }
                                    }

                                    //2025-05-27 : PBS 분류일 경우, PBS 로 기분류된 내역 체크해서 보관처리
                                    if (strSortIndex == "2" || strSortIndex == "3")
                                    {
                                        filterExpression = "tsGrupNo in ('2','3','15')";
                                        selectedRows = dsSorted.Tables[0].Select(filterExpression);
                                        if (selectedRows != null && selectedRows.Length > 0)
                                        {
                                            strSortIndex = "1";
                                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, PB일 경우 기분류된 것으로 확인되어 보관, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";

                                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                            return strSortIndex;
                                        }
                                    }

                                    dsData = null;
                                    dsRsltXN = null;
                                    dsSorted = null;

                                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                    return strSortIndex;
                                }
                                else
                                {
                                    strSortIndex = "4";
                                }
                            }

                            if (lstJobGroupCd.Contains("HE0") == true)
                            {
                                if (blnExistResultCDR == true)
                                {
                                    strSortIndex = "10";
                                }
                                else
                                {
                                    strSortIndex = "6";
                                }
                            }

                            if (lstJobGroupCd.Contains("HE8") == true)
                            {
                                if (blnExistResultCDR == true)
                                {
                                    strSortIndex = "11";
                                }
                                else
                                {
                                    strSortIndex = "8";
                                }
                            }

                            if (lstJobGroupCd.Contains("HE4") == true) { strSortIndex = "9"; }
                            if (lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "12"; }
                            if (lstJobGroupCd.Contains("HE2") == true) { strSortIndex = "13"; }

                            if (lstJobGroupCd.Contains("HEB") == true)
                            {
                                if (blnFetalHB == true && intHEB == 1)
                                {
                                    //2025-03-27 : CBC + FetalHB
                                    //2025-02-27 : 00602 단독일 경우 Other
                                    if (blnExistOrderCDR == true)
                                    {
                                        strSortIndex = "5";
                                    }
                                    else
                                    {
                                        strSortIndex = "4";
                                    }
                                }
                                else
                                {
                                    //2025-03-27 : FetabHB 있을 경우
                                    //CBC + a1c + 00602                 a1c + Other
                                    //a1c + 00602                       a1c + Other
                                    //CBC + 00602)                      CBC + Other
                                    if (blnFetalHB == true)
                                    {
                                        if (blnExistOrderCDR == true)
                                        {
                                            strSortIndex = "5";
                                        }
                                        else
                                        {
                                            strSortIndex = "15";
                                        }
                                    }
                                    else
                                    {
                                        strSortIndex = "22";
                                    }
                                }
                            }

                            if (lstJobGroupCd.Contains("PA6") == true) { strSortIndex = "24"; }
                            if (lstJobGroupCd.Contains("PA7") == true) { strSortIndex = "24"; }
                            if (lstJobGroupCd.Contains("PA8") == true) { strSortIndex = "24"; }

                            //2025-01-23
                            if (blnImmuno == false && lstJobGroupCd.Contains("IMH") == true && spcGbn == "3") { blnImmuno = true; }

                            if (blnImmuno == true && blnFetalHB == false)
                            {
                                if (strSortIndex == "22")
                                {
                                    //if (lstJobGroupCd.Contains("HE1") == false) { strSortIndex = "15"; }
                                    //else { strSortIndex = "22"; }

                                    ////2025-03-01 : A1c 단독으로 먼저 빠진 후 A1c가 검사완료되면 immuno 로
                                    //filterExpression = $"JOB_GRUP = 'HEB' AND TST_STAT_CD < '3060'";
                                    //selectedRows = dsData.Tables[0].Select(filterExpression);

                                    //2025-03-01 : A1C 분류이력으로 처리
                                    filterExpression = "tsGrupNo = " + Common.STS("22");
                                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0)
                                    {
                                        strSortIndex = "26";
                                    }
                                }
                                else
                                {
                                    ////2025-03-01 : A1c 단독으로 먼저 빠진 후 A1c가 검사완료되면 immuno 로
                                    //filterExpression = $"JOB_GRUP = 'IMH' AND TST_NM = 'ACTH'";
                                    //selectedRows = dsData.Tables[0].Select(filterExpression);
                                    //if (selectedRows != null && selectedRows.Length > 0)
                                    //{
                                    //    strSortIndex = "26";
                                    //}
                                    //else
                                    //{
                                    //    strSortIndex = "26";
                                    //}

                                    //2025-03-01 : A1C 분류이력으로 처리
                                    filterExpression = "tsGrupNo = " + Common.STS("22");
                                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0)
                                    {
                                        strSortIndex = "26";
                                    }
                                }
                            }

                            if (blnSpecialImmuno == true && blnFetalHB == false)
                            {
                                if (strSortIndex == "22")
                                {
                                    //if (lstJobGroupCd.Contains("HE1") == false) { strSortIndex = "15"; }
                                    //else { strSortIndex = "22"; }

                                    ////2025-03-01 : A1c 단독으로 먼저 빠진 후 A1c가 검사완료되면 immuno 로
                                    //filterExpression = $"JOB_GRUP = 'HEB' AND TST_STAT_CD < '3060'";
                                    //selectedRows = dsData.Tables[0].Select(filterExpression);

                                    //if (selectedRows != null && selectedRows.Length > 0)
                                    //{

                                    //}
                                    //else
                                    //{
                                    //    strSortIndex = "23";
                                    //}

                                    //2025-03-01 : A1C 분류이력으로 처리
                                    filterExpression = "tsGrupNo = " + Common.STS("22");
                                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0)
                                    {
                                        strSortIndex = "23";
                                    }

                                }
                                else
                                {
                                    strSortIndex = "23";
                                }

                                if (strSortIndex == "23")
                                {
                                    //2025-03-26 : IMH 있을 경우 23번으로 분류
                                    bool blnIMH = false;
                                    bool blnRC2 = false;

                                    filterExpression = "JOB_GRUP IN ('IMH', 'SE0') AND SPC_CD IN('A02','A04','A05')";
                                    selectedRows = dsData.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0)
                                    {
                                        blnIMH = true;
                                    }

                                    filterExpression = "JOB_GRUP IN ('RC2') AND SPC_CD IN('A02','A04','A05')";
                                    selectedRows = dsData.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0)
                                    {
                                        blnRC2 = true;
                                    }

                                    if (blnIMH == true && blnRC2 == true)
                                    {
                                        if (blnExistOrderCDR == true && blnExistResultCDR == false) { strSortIndex = "26"; }
                                        if (blnExistOrderCDR == true && blnExistResultCDR == true) { strSortIndex = "26"; }
                                    }

                                }
                            }

                            //2025-03-14 : 말라이아 있을 경우
                            if (blnMalaria == true)
                            {
                                if (blnExistResultCDR == true) { strSortIndex = "5"; }
                                if (blnExistResultCDR == true) { strSortIndex = "5"; }
                                if (blnExistResultCDR == true) { strSortIndex = "5"; }

                                //2025-03-26 : PB + 말라리아 = PB + CBC + OTHER
                                if (blnPB == true) { strSortIndex = "2"; }

                                //2025-03-26 : HE1 없을 경우 그냥 Other 로 분류
                                if (strSortIndex == "5")
                                {
                                    filterExpression = "JOB_GRUP IN ('HE1','HE3') AND SPC_CD IN('A02','A04','A05')";
                                    selectedRows = dsData.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0)
                                    {
                                        //그대로 분류
                                    }
                                    else
                                    {
                                        strSortIndex = "4";
                                    }
                                }
                            }
                        }
                        else if (lstJobGroupCd.Count == 2)
                        {
                            //2025-03-13 : 혈액학 부팀장 요청으로 수정 PB+CBC+OTHER 분류를 PB+CBC+RET 로 분류
                            //if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "2"; }
                            if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "3"; }

                            if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true)
                            {
                                if (blnExistOrderCDR == true && blnExistResultCDR == true)
                                {
                                    dsData = null;
                                    dsRsltXN = null;
                                    dsSorted = null;
                                    strSortIndex = "1";
                                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(XN검사완료), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                    return strSortIndex;
                                }
                                else
                                {
                                    strSortIndex = "3";
                                }
                            }

                            if (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true)
                            {
                                if (blnMalaria == true)
                                {
                                    //2025-03-13 : CDR+PB+MALARIA
                                    strSortIndex = "2";
                                }
                                else
                                {
                                    strSortIndex = "3";
                                }
                            }

                            //2025-03-13 : 혈액학 부팀장 요청으로 수정 CBC+ESR 을 ESR 로 분류
                            //if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "10"; }
                            if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "6"; }

                            //2025-03-13 : 혈액학 부팀장 요청으로 수정 CBC+ABO 를 ABO 로 분류
                            if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "11"; }
                            if (lstJobGroupCd.Contains("HE2") == true && lstJobGroupCd.Contains("HE4") == true) { strSortIndex = "14"; }
                            if (lstJobGroupCd.Contains("HE2") == true && lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "14"; }
                            if (lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "14"; }

                            //2025-03-13 : 혈액학 부팀장 요청으로 추가 HE5 + *
                            if (lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "14"; }

                            //2025-03-13 : 혈액학 부팀장 요청으로 수정 CBC+OTHER 를 PB+CBC+OTHER 로 분류
                            //if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "4"; }
                            if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "2"; }

                            if (lstJobGroupCd.Contains("HEB") == true)
                            {
                                if (lstJobGroupCd.Contains("HE1") == false)
                                {
                                    //A1c + Other
                                    strSortIndex = "15";
                                }
                                else
                                {
                                    if (blnFetalHB == true && intHEB == 1)
                                    {
                                        //2025-02-27 : 00602 오더가 있는 경우 = CBC + (jobGrup\":\"HEB\",\"tstCd\":\"00602\",\"tstAfilCd\":\"-\) > CBC+Other
                                        strSortIndex = "5";
                                    }
                                    else
                                    {
                                        if (blnFetalHB == true)
                                        {
                                            strSortIndex = "15";
                                        }
                                        else
                                        {
                                            strSortIndex = "22";
                                        }
                                    }
                                }
                            }

                            //ESR + ABO
                            if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "7"; }

                            if (string.IsNullOrEmpty(strSortIndex) && lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "12"; }

                            //2025-03-14 : 말라이아 있을 경우
                            if (blnMalaria == true)
                            {
                                if (lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "4"; }
                                if (lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "4"; }

                                if (blnExistResultCDR == true && lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "5"; }
                                if (blnExistResultCDR == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "5"; }
                                if (blnExistResultCDR == true && lstJobGroupCd.Contains("HE3") == true) { strSortIndex = "5"; }

                                if (strSortIndex == "4" && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "15"; }
                                if (strSortIndex == "5" && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "15"; }

                                //2025-03-27 : HE0, HE8, A1C 우선순위 적용
                                //2025-03-26 : PB + 말라리아 = PB + CBC + OTHER
                                if (strSortIndex == "15" || strSortIndex == "4" || strSortIndex == "5")
                                {

                                }
                                else
                                {
                                    if (blnPB == true) { strSortIndex = "2"; }
                                }

                                //2025-03-26 : HE1 없을 경우 그냥 Other 로 분류
                                if (strSortIndex == "5")
                                {
                                    filterExpression = "JOB_GRUP IN ('HE1','HE3') AND SPC_CD IN('A02','A04','A05')";
                                    selectedRows = dsData.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0)
                                    {
                                        //그대로 분류
                                    }
                                    else
                                    {
                                        strSortIndex = "4";
                                    }
                                }

                            }
                            else
                            {
                                if ((blnExistResultCDR == true || lstJobGroupCd.Contains("HE1") == true) && lstJobGroupCd.Contains("HE6") == true)
                                {
                                    if (lstJobGroupCd.Contains("HE3") == true)
                                    {
                                        //PB + CBC + RET
                                        strSortIndex = "3";
                                    }
                                    else
                                    {
                                        strSortIndex = "5";
                                    }

                                    if (lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "15"; }
                                }
                            }

                        }
                        else
                        {
                            //2025-03-13 : 혈액학 부팀장 요청으로 수정 CBC+OTHER 에서 PB + CBC + OTHER 로
                            //if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "5"; }
                            if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "2"; }

                            if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "3"; }

                            //2025-03-13 : 혈액학 부팀장 요청으로 수정 OTHER 에서 PB + CBC + OTHER 로
                            //if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "4"; }
                            if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "2"; }

                            if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "7"; }
                            if (lstJobGroupCd.Contains("HE2") == true && lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "14"; }

                            if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true && string.IsNullOrEmpty(strSortIndex))
                            {
                                if (lstJobGroupCd.Contains("HE3") == false)
                                {
                                    //2025-03-13 : 혈액학 부팀장 요청으로 수정
                                    //strSortIndex = "5"; 
                                    strSortIndex = "2";
                                }
                                else
                                {
                                    strSortIndex = "3";
                                }
                            }

                            //2025-03-13 : 혈액학 부팀장 요청으로 수정
                            //if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == false && lstJobGroupCd.Contains("HEB") == false && string.IsNullOrEmpty(strSortIndex)) { strSortIndex = "5"; }
                            if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == false && lstJobGroupCd.Contains("HEB") == false && string.IsNullOrEmpty(strSortIndex)) { strSortIndex = "15"; }

                            if (lstJobGroupCd.Contains("HEB") == true)
                            {
                                //A1c + Other
                                strSortIndex = "15";
                            }

                            //2025-03-14 : 말라이아 있을 경우
                            if (blnMalaria == true)
                            {
                                if (lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "4"; }
                                if (lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "4"; }

                                if (blnExistResultCDR == true && lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "5"; }
                                if (blnExistResultCDR == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "5"; }
                                if (blnExistResultCDR == true && lstJobGroupCd.Contains("HE3") == true) { strSortIndex = "5"; }

                                if (strSortIndex == "4" && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "15"; }
                                if (strSortIndex == "5" && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "15"; }

                                //2025-03-27 : HbA1c 우선 적용
                                //2025-03-26 : PB + 말라리아 = PB + CBC + OTHER
                                if (strSortIndex == "15" || strSortIndex == "4" || strSortIndex == "5")
                                {

                                }
                                else
                                {
                                    if (blnPB == true) { strSortIndex = "2"; }
                                }

                                //2025-03-26 : HE1 없을 경우 그냥 Other 로 분류
                                if (strSortIndex == "5")
                                {
                                    filterExpression = "JOB_GRUP IN ('HE1','HE3') AND SPC_CD IN('A02','A04','A05')";
                                    selectedRows = dsData.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0)
                                    {
                                        //그대로 분류
                                    }
                                    else
                                    {
                                        strSortIndex = "4";
                                    }
                                }
                            }
                            else
                            {
                                if ((blnExistResultCDR == true || lstJobGroupCd.Contains("HE1") == true) && lstJobGroupCd.Contains("HE6") == true)
                                {
                                    if (lstJobGroupCd.Contains("HE3") == true)
                                    {
                                        //PB + CBC + RET
                                        strSortIndex = "3";

                                        //2025-03-18 : CBC + OTHER 로 수정
                                        strSortIndex = "5";
                                    }
                                    else
                                    {
                                        strSortIndex = "5";
                                    }

                                    if (lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "15"; }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(strSortIndex) == false)
                        {
                            if (strSortIndex == "24")
                            {
                                //2025-04-29 : Data Error 일 경우에는 기분류 조건 처리 안함.
                            }
                            else
                            {
                                //2025-03-24 : 기분류된 건 인지 체크 후 아카이브 여부 확인해서 아카이브 처리
                                filterExpression = "tsGrupNo = " + Common.STS(strSortIndex);
                                selectedRows = dsSorted.Tables[0].Select(filterExpression);
                                if (selectedRows != null && selectedRows.Length > 0)
                                {
                                    strSortIndex = "1";
                                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 기분류된 것으로 확인되어 보관, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                }

                                //2025-05-26 : PBS 분류일 경우, PBS 로 기분류된 내역 체크해서 보관처리
                                if (strSortIndex == "3")
                                {
                                    filterExpression = "tsGrupNo in ('2','3','15')";
                                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0)
                                    {
                                        strSortIndex = "1";
                                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, PB일 경우 기분류된 것으로 확인되어 보관, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                    }
                                }
                            }

                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";

                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            dsData = null;
                            dsRsltXN = null;
                            dsSorted = null;
                            return strSortIndex;
                        }
                    }
                }

                #endregion ----- 10. 오더에 따른 분류

                #region ----- 11. 분류조건을 확인할 수 없음

                if (blnExistOrderCDR == true && blnExistResultCDR == true)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;

                    if (lstJobGroupCd.Contains("IMH") == true && spcGbn == "3")
                    {
                        strSortIndex = "26";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, CBC+IMH, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    }
                    else
                    {
                        strSortIndex = "1";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류조건 확인X, 검사완료로 보관, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    }

                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                //filterExpression = $"JOB_GRUP = 'HEB' AND TST_STAT_CD < '3060'";
                //selectedRows = dsData.Tables[0].Select(filterExpression);

                //2025-03-01 : A1C 분류이력으로 처리
                filterExpression = "TST_STAT_CD < '3060' AND SPC_CD IN ('A02','A04','A05')";
                if (dsData != null && dsData.Tables.Count > 0)
                {
                    selectedRows = dsData.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        strSortIndex = "24";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류조건을 확인할 수 없음, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    }
                    else
                    {
                        //2025-03-21 : 검사완료로 보관처리
                        strSortIndex = "1";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류조건을 확인할 수 없음, 검사완료로 보관, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    }
                }

                dsData = null;
                dsRsltXN = null;
                dsSorted = null;

                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                return strSortIndex;

                #endregion ----- 11. 분류조건을 확인할 수 없음

            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"GetSortIndex Exception {ex}, spcNo: {strSpcNo}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "GetSortIndex" + TAB + strSpcNo + TAB + strSortIndex + "\r\n",
                              false,
                              mstrAppPath + "log\\",
                              DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");


            return strSortIndex;
        }

        private void btnMnuTst_Click(object sender, EventArgs e)
        {
            string strBarNo = txtBarNo.Text.Trim();
            string strRackNo = "";

            strRackNo = Common.P(strBarNo, ",", 2);
            strBarNo = Common.P(strBarNo, ",", 1);

            string strTmpIdx = "";
            string strAddRow = "";

            switch (Constant.SG_TS_EQP_CD)
            {
                case "620":
                    strTmpIdx = GetSortIndex_SeeGene_Seoul(strBarNo, strRackNo);
                    break;

                case "621":
                    strTmpIdx = GetSortIndex_SeeGene_Seoul(strBarNo, strRackNo);
                    break;

                case "039":
                    strTmpIdx = GetSortIndex_SeeGene_Busan(strBarNo, strRackNo);
                    break;

                case "922":
                    strTmpIdx = GetSortIndex_SeeGene_Daegu(strBarNo, strRackNo);
                    break;

                case "842":
                    strTmpIdx = GetSortIndex_SeeGene_Gwangju(strBarNo, strRackNo);
                    break;

                case "724":
                    strTmpIdx = GetSortIndex_SeeGene_Daejeon(strBarNo, strRackNo);
                    break;

                default:
                    strTmpIdx = "장비코드 확인하세요~!";
                    break;
            }

            strAddRow = "Result" + TAB;
            strAddRow = strAddRow + "" + TAB;
            strAddRow = strAddRow + "TS Test" + TAB;
            strAddRow = strAddRow + txtBarNo.Text.Trim() + TAB;
            strAddRow = strAddRow + "" + TAB;
            strAddRow = strAddRow + "" + TAB;
            strAddRow = strAddRow + "Sorting Idx: " + strTmpIdx + TAB;

            GrdRowAdd(strAddRow);
        }

        private void GetRackInformation()
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory;
            string strFile = "RackInformation.ini";
            string strTemp;
            string[] aryTemp;

            try
            {
                StreamReader objReader = new StreamReader(strPath + strFile, Encoding.Default);
                while ((strTemp = objReader.ReadLine()) != null)
                {
                    if (strTemp != "")
                    {
                        aryTemp = strTemp.Split('\t');

                        for (int i = 0; i < aryTemp.Length; i++)
                        {
                            if (string.IsNullOrEmpty(aryTemp[i]) == false)
                            {
                                if (glstNextStepRack.Contains(aryTemp[i]) == false)
                                {
                                    glstNextStepRack.Add(aryTemp[i]);
                                }
                            }
                        }
                    }
                }
                objReader.Close();
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"GetRackInformation Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }
        }

        private string GetSortIndex_SeeGene_Busan(string strSpcNo, string strRackNo)
        {
            string strSortIndex = "";
            bool blnComplete = false;

            try
            {
                List<string> lstJobGroupCd = new List<string>();
                DataSet dsData = null;
                DataSet dsSorted = null;
                DataSet dsRsltXN = null;
                DataSet dsRsltHIS = null;

                string strLog;
                string strLine = new string('-', 20) + "\r\n";
                string strLisTstCds = "";
                string strLisTstSubCd = "";
                string strSortDesc = "";
                string strCenterCode = "";

                bool blnExistOrderCDR = false;
                bool blnNoOrder = true;
                bool blnExistResultCDR = false;
                bool blnNextStep = false;
                bool blnSorted = false;
                bool blnApplicator = false;
                bool blnSlide = false;
                bool blnRerunFirst = false;
                bool blnRerunSecond = false;
                bool blnSeoul = false;
                bool blnBusan = false;
                bool blnPA8 = false;
                bool blnOther = false;
                string spcGbn = "";
                string filterExpression;
                DataRow[] selectedRows;
                bool blnCompleteChkSpcCd = false;
                bool blnCompleteChkJobGrup = false;

                bool blnSortedPB = false;

                bool otherUnit = false;
                bool a1c = false;
                bool hema = false;
                bool dat = false;
                bool uc = false;

                if (strSpcNo.Length == 12)
                {
                    spcGbn = strSpcNo[11].ToString(); // 문자열의 12번째 문자 (0부터 시작하므로 11번째 인덱스)
                }

                strLog = $"Start GetSortIndex_SeeGene_Busan SpcNo: {strSpcNo}, RackNo: {strRackNo}" + "\r\n"; ;

                Common.File_Record("\r\n", false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                #region ----- 씨젠의료재단 부산 센터 SYSMEX TS-10 분류코드
                //2024-01-26 : 씨젠의료재단 부산 센터 진단검사의학과 혈액학부 SYSMEX TS-10 검체 분류 및 아카이브
                //TS-10 Sort No
                //1  = 아카이브
                //2  = ERROR
                //3  = 서울 + 부산
                //4  = PBS + HBA1C
                //5  = OTHER                           = 타학부
                //6  = 서울
                //7  = CBC + HbA1C
                //8  = 사용안함
                //9  = HbA1C + ABO + ESR
                //10 = HbA1C + ABO
                //11 = HbA1C + ESR
                //12 = ABO + ESR
                //13 = CBC + ABO + ESR
                //14 = PBS + CBC
                //15 = 사용안함
                //16 = 사용안함
                //17 = CBC + HBA1c + ABO + ESR
                //18 = CBC + HBA1c + ABO
                //19 = CBC + HBA1c + ESR
                //20 = CBC + ABO
                //21 = CBC + ESR
                //22 = ABO
                //23 = ESR
                //24 = 재검
                //25 = SLIDE
                //26 = MORE = 떠보기
                #endregion ----- 씨젠의료재단 부산 센터 SYSMEX TS-10 분류코드

                #region ----- 1. 바코드에러 = 61

                //1. 바코드에러 
                if (Common.IsNumeric(strSpcNo) == false)
                {
                    strSortIndex = "61";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 바코드에러, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 1. 바코드에러 = 61

                #region ----- 2. 아카이브 모드
                if (chkArchive.Checked == true)
                {
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Archive Mode, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }
                #endregion ----- 2. 아카이브 모드

                #region ----- 3. 오더조회

                strLog += "JOB_GRUP" + TAB + "LIS_TST_CD" + TAB + "LIS_TST_SUB_CD" + TAB + "TST_NM" + TAB + "STUS_CD" + "\r\n";

                //13100005 서울본원
                //13900000 부산경남검사센터
                //14100000 대구경북검사센터
                //14300000 광주호남검사센터
                //14500000 대전충청검사센터

                dsData = BizData.GetSpcInfo(strSpcNo);

                if (spcGbn == "5")
                {
                    filterExpression = "SPC_CD = 'A04' ";
                    if (dsData != null && dsData.Tables.Count > 0)
                    {
                        selectedRows = dsData.Tables[0].Select(filterExpression);

                        if (selectedRows != null && selectedRows.Length > 0)
                        {
                            //오더 조회되었으므로 정상 진행
                        }
                        else
                        {
                            //오더 조회 안되었으니 Data Error 로 분류
                            strSortIndex = "2";
                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}, 5번 검체이면서 Citrate(A04) 오더조회 안됨!" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            return strSortIndex;
                        }
                    }
                    else
                    {
                        //오더 조회 안되었으니 Data Error 로 분류
                        strSortIndex = "2";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}, 5번 검체이면서 Citrate(A04) 오더조회 안됨!" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }
                }

                if (dsData != null && dsData.Tables.Count > 0)
                {
                    //2025-02-17 : 무조건 데이터에러로 처리
                    //              3610 추후송부
                    //              3630 우선검사진행
                    //              9070 검사제외                
                    filterExpression = "TST_STAT_CD IN ('3610', '3630', '9070') ";
                    selectedRows = dsData.Tables[0].Select(filterExpression);

                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        foreach (DataRow drTemp in selectedRows)
                        {
                            if (drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                strSortIndex = "2";
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }
                        }
                    }
                }

                if (dsData != null && dsData.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsData.Tables[0].Rows)
                    {
                        //EDTA: A05
                        //CITRATE : A04
                        //EDTA PLASMA: A02

                        bool blnChk = false;

                        //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                        if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                        {
                            System.Diagnostics.Debug.Print(drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["SPC_CD"].ToString().Trim());

                            if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                if (spcGbn == "5")
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                    {
                                        blnChk = true;
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                    {
                                        blnChk = true;
                                    }
                                }
                            }
                            else
                            {
                                if (drTemp["SPC_CD"].ToString().Trim() == "")
                                {
                                    //if (aryTemp[i].IndexOf("RET") > -1)
                                    if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                    {
                                        if (spcGbn == "5")
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                        else
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                    }
                                }
                            }

                            //strLog += "SPC_CD" + TAB + drTemp["SPC_CD"].ToString().Trim() + TAB + "blnChk" + TAB + blnChk .ToString() + "JOB_GRUP" + TAB + drTemp["JOB_GRUP"].ToString().Trim()  + "\r\n";

                            if (blnChk == true)
                            {
                                if (string.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                {
                                    blnNoOrder = false;
                                }

                                strLisTstSubCd = drTemp["LIS_TST_SUB_CD"].ToString();
                                if (strLisTstSubCd == "-") { strLisTstSubCd = ""; }
                                strLisTstCds = drTemp["LIS_TST_CD"].ToString() + strLisTstSubCd;

                                //11310 = Reti count
                                //11017 = Eosinophil count (호산구수)
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE1") { blnExistOrderCDR = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE3" && strLisTstCds == "11310" && blnExistOrderCDR == false) { blnExistOrderCDR = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE9" && strLisTstCds == "11017" && blnExistOrderCDR == false) { blnExistOrderCDR = true; }

                                strLog += drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["LIS_TST_CD"].ToString().Trim() + TAB + drTemp["LIS_TST_SUB_CD"].ToString().Trim() + TAB;
                                strLog += drTemp["TST_NM"].ToString().Trim() + TAB + drTemp["STUS_CD"].ToString().Trim() + "\r\n";

                                //2024-04-19 : 서울로 분류해야하는 기준 체크
                                if (drTemp["CNTR_CD"].ToString().Trim() == "13100005") { blnSeoul = true; }
                                if (drTemp["CNTR_CD"].ToString().Trim() == "13900000") { blnBusan = true; }

                                if (drTemp["JOB_GRUP"].ToString().Trim() == "PA8") { blnPA8 = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "IMH") { blnOther = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "CC5") { blnOther = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "AA4") { blnOther = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "AA5") { blnOther = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "AE1") { blnOther = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "AE2") { blnOther = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "AE7") { blnOther = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "AE9") { blnOther = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "AAD") { blnOther = true; }

                                if (drTemp["SPC_GBN"].ToString().Trim() == spcGbn && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                                {
                                    if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                    {
                                        lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                    }
                                }
                            }
                        }
                    }
                }

                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                #endregion ----- 3. 오더조회

                #region ----- 4. 오더조회 안됨 = 2

                if (blnNoOrder == true)
                {
                    if (dsData != null && dsData.Tables.Count > 0)
                    {
                        foreach (DataRow drTemp in dsData.Tables[0].Rows)
                        {
                            if (drTemp["CNTR_CD"].ToString().Trim() == "13100005" && drTemp["SPC_CD"].ToString().Trim() != "A01")
                            {
                                blnSeoul = true;
                                break;
                            }
                        }
                    }

                    if (blnSeoul == true)
                    {
                        //2025-04-25 : EDTA 오더있는 지 체크
                        filterExpression = $"SPC_CD IN ('A02','A04','A05')";
                        if (dsData != null && dsData.Tables.Count > 0)
                        {
                            selectedRows = dsData.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                //
                            }
                            else
                            {
                                blnSeoul = false;
                            }
                        }
                    }

                    if (blnSeoul == true)
                    {
                        dsData = null;
                        strSortIndex = "6";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, No Order(Seoul), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }

                    dsData = null;
                    strSortIndex = "2";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, No Order(Data Error), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 4. 오더조회 안됨 = 2

                #region ----- 5. CDR 오더가 있으나 XN에서 검사한 내역이 없을 경우 LINE OUT = 0

                dsRsltXN = BizData.GetRsltHematology(strSpcNo);
                if (dsRsltXN != null && dsRsltXN.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsRsltXN.Tables[0].Rows)
                    {
                        blnExistResultCDR = true;
                        break;
                    }
                }

                #region ----- 3-1. 오더에 따른 분류 (PBS 우선 분류)
                if (dsData != null && dsData.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsData.Tables[0].Rows)
                    {

                        blnCompleteChkSpcCd = true;
                        blnCompleteChkJobGrup = true;

                        filterExpression = $"SPC_CD IN ('A02','A04','A05') AND SPC_GBN = '{spcGbn}' AND TST_STAT_CD < '3060'";
                        selectedRows = dsData.Tables[0].Select(filterExpression);
                        if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkSpcCd = false; }

                        filterExpression = $"SPC_CD = '' AND JOB_GRUP LIKE 'HE%' AND SPC_GBN = '{spcGbn}' AND TST_STAT_CD < '3060'";
                        selectedRows = dsData.Tables[0].Select(filterExpression);
                        if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkJobGrup = false; }

                        //2024-12-17 : 9 번 검체구분자로 IM
                        if (spcGbn == "9")
                        {
                            //
                        }
                        else
                        {
                            if (blnCompleteChkJobGrup == true && blnCompleteChkSpcCd == true)
                            {
                                filterExpression = $"SPC_CD IN ('A02','A04','A05') AND SPC_GBN = '9' AND TST_STAT_CD < '3060'";
                                selectedRows = dsData.Tables[0].Select(filterExpression);
                                if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkSpcCd = false; }

                                filterExpression = $"SPC_CD = '' AND JOB_GRUP LIKE 'HE%' AND SPC_GBN = '9' AND TST_STAT_CD < '3060'";
                                selectedRows = dsData.Tables[0].Select(filterExpression);
                                if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkJobGrup = false; }
                            }
                        }

                        if (blnCompleteChkJobGrup == true && blnCompleteChkSpcCd == true)
                        {
                            blnComplete = true;
                        }

                        //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                        if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                        {
                            if (drTemp["SPC_CD"].ToString().Trim() != "A01" && drTemp["SPC_CD"].ToString().Trim() != "U01" && String.IsNullOrEmpty(drTemp["SPC_CD"].ToString().Trim()) == false)
                            {
                                if (drTemp["SPC_GBN"].ToString().Trim() == spcGbn && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                                {
                                    if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                    {
                                        lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                    }
                                }
                            }
                        }
                    }

                    if (lstJobGroupCd.Count > 0)
                    {
                        if (blnExistResultCDR && lstJobGroupCd.Contains("HE1") == true)
                        {
                            lstJobGroupCd.Remove("HE1");
                        }

                        foreach (string job in lstJobGroupCd)
                        {
                            strLog = $"PBS 우선분류 SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: {job}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        }
                    }

                    if (lstJobGroupCd.Count == 1)
                    {
                        if (blnSeoul == true)
                        {
                            //서울 오더 존재하는데 HEB 오더 1개 있는 경우 이쪽으로 타게 됨. blnBusan True로 체크하여 서울+부산 빠지도록 수정
                            //blnBusan 체크 니 HEB 말고 다른 부산 검사항목도 해당함.
                            if (blnBusan == true)
                            {
                                dsData = null;
                                strSortIndex = "3";
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, lstJobGroupCd.Count : 1, 서울 + 부산, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }
                            else
                            {
                                dsData = null;
                                strSortIndex = "6";
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, lstJobGroupCd.Count : 1, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }
                        }
                        else
                        {
                            if (lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "14"; }
                        }
                    }
                    else
                    {
                        //4  = PBS + HBA1C
                        //5  = OTHER
                        //6  = 서울
                        //7  = CBC + HbA1C
                        //8  = 사용안함
                        //9  = HbA1C + ABO + ESR
                        //10 = HbA1C + ABO
                        //11 = HbA1C + ESR
                        //12 = ABO + ESR
                        //13 = CBC + ABO + ESR
                        //14 = PBS + CBC

                        if (lstJobGroupCd.Contains("HE6") == true)
                        {
                            //PB + DAT = data error
                            if (lstJobGroupCd.Contains("HE7") == true)
                            {
                                dsData = null;
                                strSortIndex = "2";
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, PB+DAT(Data Error), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }

                            foreach (string job in lstJobGroupCd)
                            {
                                if (job.IndexOf("HE") > -1)
                                {
                                    if (job.IndexOf("HEB") > -1)
                                    {
                                        a1c = true;
                                    }
                                    else
                                    {
                                        hema = true;
                                    }
                                }
                                else
                                {
                                    if (job.IndexOf("UC") > -1)
                                    {
                                        uc = true;
                                    }
                                    else
                                    {
                                        otherUnit = true;
                                        break;
                                    }
                                }
                            }

                            //2025-07-11 : PBS + 다른검사 인 경우에 Error분류 요청
                            if (lstJobGroupCd.Count == 2 && (lstJobGroupCd.Contains("HE1") == true || lstJobGroupCd.Contains("HEB") == true))
                            {
                                // PB + CBC or PB + A1C
                            }
                            else
                            {
                                if (lstJobGroupCd.Count == 3 && (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HEB") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 2 && (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 2 && (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HEB") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 2 && (lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HEB") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 3 && (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 3 && (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HEB") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 4 && (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HEB") == true))
                                {
                                    //Pass
                                }
                                //2025-07-16 이현석 추가                               
                                else if (lstJobGroupCd.Count == 2 && (lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE9") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 2 && (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE9") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 3 && (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE9") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 3 && (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE9") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 3 && (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HEB") == true && lstJobGroupCd.Contains("HE9") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 3 && (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE9") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 3 && (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HEB") == true && lstJobGroupCd.Contains("HE9") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 3 && (lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HEB") == true && lstJobGroupCd.Contains("HE9") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 4 && (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE9") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 4 && (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HEB") == true && lstJobGroupCd.Contains("HE9") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 4 && (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HEB") == true && lstJobGroupCd.Contains("HE9") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 4 && (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HEB") == true && lstJobGroupCd.Contains("HE9") == true))
                                {
                                    //Pass
                                }
                                else if (lstJobGroupCd.Count == 5 && (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE9") == true && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HEB") == true))
                                {
                                    //Pass
                                }
                                else
                                {
                                    if (blnSeoul == true || otherUnit == true)
                                    {
                                        //기존 대로 처리
                                    }
                                    else
                                    {
                                        dsData = null;
                                        strSortIndex = "2";
                                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, PB+다른검사(Data Error), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                        return strSortIndex;
                                    }
                                }
                            }

                            if (lstJobGroupCd.Contains("HE1") == true || lstJobGroupCd.Contains("HE3") == true)
                            {
                                strSortIndex = "14";

                                if (blnSeoul == true)
                                {
                                    //서울+부산 분류 우선
                                    strSortIndex = "3";
                                }
                                else
                                {
                                    if (otherUnit)
                                    {
                                        strSortIndex = "5";
                                    }
                                    else
                                    {
                                        if (a1c)
                                        {
                                            strSortIndex = "4";
                                        }
                                        else
                                        {
                                            strSortIndex = "14";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (lstJobGroupCd.Contains("HE0") == true || lstJobGroupCd.Contains("HE1") == true || lstJobGroupCd.Contains("HE2") == true ||
                                    lstJobGroupCd.Contains("HE3") == true || lstJobGroupCd.Contains("HE4") == true || lstJobGroupCd.Contains("HE5") == true ||
                                    lstJobGroupCd.Contains("HE6") == true || lstJobGroupCd.Contains("HE7") == true || lstJobGroupCd.Contains("HE8") == true ||
                                    lstJobGroupCd.Contains("HEB") == true || lstJobGroupCd.Contains("UC1") == true)
                                {
                                    strSortIndex = "4";

                                    if (blnSeoul == true)
                                    {
                                        //서울+부산 분류 우선
                                        strSortIndex = "3";
                                    }
                                    else
                                    {
                                        if (otherUnit)
                                        {
                                            strSortIndex = "5";
                                        }
                                        else
                                        {
                                            if (a1c)
                                            {
                                                strSortIndex = "4";
                                            }
                                            else
                                            {
                                                strSortIndex = "14";
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    if (blnSeoul == true)
                                    {
                                        //서울+부산 분류 우선
                                        strSortIndex = "3";
                                    }
                                    else
                                    {
                                        if (otherUnit)
                                        {
                                            strSortIndex = "5";
                                        }
                                        else
                                        {
                                            if (a1c)
                                            {
                                                strSortIndex = "4";
                                            }
                                            else
                                            {
                                                strSortIndex = "14";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(strSortIndex) == false)
                    {
                        //dsData = null;
                        //dsRsltXN = null;
                        //dsSorted = null;

                        if (blnOther == true)
                        {
                            strSortIndex = "5";

                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, IMH, CC5 = 타학부, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            return strSortIndex;
                        }
                        else
                        {
                            //2024-10-28 : PBS 일 경우 기분류 체크해서 다음 차수 또는 아카이브
                            if (strSortIndex == "14")
                            {
                                dsSorted = BizData.GetSpcPos(strSpcNo);
                                if (dsSorted != null && dsSorted.Tables.Count > 0)
                                {
                                    filterExpression = "tsGrupNo = " + Common.STS("26");
                                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0) { blnApplicator = true; blnNextStep = true; }

                                    filterExpression = "tsGrupNo = " + Common.STS("25");
                                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0) { blnSlide = true; blnNextStep = true; }

                                    filterExpression = "tsGrupNo NOT IN ('14','24','25','26','2','1','0','') AND tsGrupNo IS NOT NULL ";
                                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0)
                                    {
                                        blnSorted = true;
                                    }

                                    filterExpression = "tsGrupNo = " + Common.STS("14");
                                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                                    if (selectedRows != null && selectedRows.Length > 0) { blnSortedPB = true; blnNextStep = true; }

                                    DataView view = dsSorted.Tables[0].DefaultView;
                                    view.Sort = "regDtm DESC";

                                    // Create a new DataTable from the sorted view
                                    DataTable sortedTable = view.ToTable();

                                    //foreach (DataRow drTemp in dsSorted.Tables[0].Rows)
                                    foreach (DataRow drTemp in sortedTable.Rows)
                                    {
                                        if (string.IsNullOrEmpty(drTemp["tsGrupNo"].ToString()) == false)
                                        {
                                            if (drTemp["tsGrupNo"].ToString().Trim() == "26") { blnApplicator = true; blnNextStep = true; }
                                            if (drTemp["tsGrupNo"].ToString().Trim() == "25") { blnSlide = true; blnNextStep = true; }
                                            if (drTemp["tsGrupNo"].ToString().Trim() == "24") { blnRerunFirst = true; blnNextStep = true; }
                                            if (drTemp["tsGrupNo"].ToString().Trim() == "24") { blnRerunSecond = true; blnNextStep = true; }
                                            if (drTemp["tsGrupNo"].ToString().Trim() == "14") { blnSortedPB = true; blnNextStep = true; }

                                            if (drTemp["tsGrupNo"].ToString().Trim() != "24" && drTemp["tsGrupNo"].ToString().Trim() != "25" &&
                                                drTemp["tsGrupNo"].ToString().Trim() != "26" && drTemp["tsGrupNo"].ToString().Trim() != "2" &&
                                                drTemp["tsGrupNo"].ToString().Trim() != "0" && drTemp["tsGrupNo"].ToString().Trim() != "1" && drTemp["tsGrupNo"].ToString().Trim() != "14")
                                            {
                                                blnSorted = true;
                                            }
                                            break;
                                        }
                                    }
                                }

                                if (blnSorted == true && blnSortedPB == false)
                                {
                                    dsData = null;
                                    dsRsltXN = null;
                                    dsSorted = null;
                                    strSortIndex = "1";
                                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(기분류), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                    return strSortIndex;
                                }
                            }

                            if (blnSortedPB == true)
                            {
                                //2025-04-15 : 기분류 시 Pass
                            }
                            else
                            {
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                return strSortIndex;
                            }
                        }
                    }
                }

                #endregion ----- 10. 오더에 따른 분류

                //if (blnExistOrderCDR == true && blnExistResultCDR == false && blnSeoul == false)
                if (blnExistOrderCDR == true && blnExistResultCDR == false)
                {
                    dsData = null;
                    dsRsltXN = null;
                    strSortIndex = "0";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, CDR 오더가 있으나 검사결과가 없을 경우, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 5. CDR 오더가 있으나 XN에서 검사한 내역이 없을 경우 LINE OUT

                dsSorted = BizData.GetSpcPos(strSpcNo);
                if (dsSorted != null && dsSorted.Tables.Count > 0)
                {

                    filterExpression = "tsGrupNo = " + Common.STS("24");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnRerunFirst = true; blnNextStep = true; }

                    filterExpression = "tsGrupNo = " + Common.STS("26");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnApplicator = true; blnNextStep = true; }

                    filterExpression = "tsGrupNo = " + Common.STS("25");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnSlide = true; blnNextStep = true; }

                    filterExpression = "tsGrupNo = " + Common.STS("14");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnSortedPB = true; blnNextStep = true; }

                    filterExpression = "tsGrupNo NOT IN ('14','24','25','26','2','1','0','') AND tsGrupNo IS NOT NULL ";
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        blnSorted = true;
                    }

                    DataView view = dsSorted.Tables[0].DefaultView;
                    view.Sort = "regDtm DESC";

                    // Create a new DataTable from the sorted view
                    DataTable sortedTable = view.ToTable();

                    //foreach (DataRow drTemp in dsSorted.Tables[0].Rows)
                    foreach (DataRow drTemp in sortedTable.Rows)
                    {
                        if (string.IsNullOrEmpty(drTemp["tsGrupNo"].ToString()) == false)
                        {
                            if (drTemp["tsGrupNo"].ToString().Trim() == "26") { blnApplicator = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "25") { blnSlide = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "24") { blnRerunFirst = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "24") { blnRerunSecond = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "14") { blnSortedPB = true; blnNextStep = true; }

                            if (drTemp["tsGrupNo"].ToString().Trim() != "24" && drTemp["tsGrupNo"].ToString().Trim() != "25" &&
                                drTemp["tsGrupNo"].ToString().Trim() != "26" && drTemp["tsGrupNo"].ToString().Trim() != "2" &&
                                drTemp["tsGrupNo"].ToString().Trim() != "0" && drTemp["tsGrupNo"].ToString().Trim() != "1" && drTemp["tsGrupNo"].ToString().Trim() != "14")
                            {
                                blnSorted = true;
                            }
                            break;
                        }
                    }
                }

                #region ----- 6. 재검통과 랙인지 확인하기 (* XN검사결과 판정결과가 재검대상일 경우 계속 재검으로 분류되므로 특정 랙일 경우 체크 안함)

                if (Common.IsNumeric(strRackNo)) { strRackNo = Common.Val(strRackNo).ToString(); }
                if (glstNextStepRack.Contains(strRackNo)) { blnNextStep = true; }

                #endregion ----- 6. 재검통과 랙인지 확인하기 (* XN검사결과 판정결과가 재검대상일 경우 계속 재검으로 분류되므로 특정 랙일 경우 체크 안함)

                #region ----- 7. 기분류된 내역 유무 조회

                if (blnSortedPB == true) { strSortIndex = ""; }

                //dsSorted = BizData.GetSpcPos(strSpcNo);
                if (dsSorted != null && dsSorted.Tables.Count > 0)
                {

                    filterExpression = "tsGrupNo = " + Common.STS("24");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnRerunFirst = true; blnNextStep = true; }

                    filterExpression = "tsGrupNo = " + Common.STS("26");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnApplicator = true; blnNextStep = true; }

                    filterExpression = "tsGrupNo = " + Common.STS("25");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnSlide = true; blnNextStep = true; }

                    filterExpression = "tsGrupNo = " + Common.STS("14");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnSortedPB = true; blnNextStep = true; }

                    DataView view = dsSorted.Tables[0].DefaultView;
                    view.Sort = "regDtm DESC";

                    // Create a new DataTable from the sorted view
                    DataTable sortedTable = view.ToTable();

                    //foreach (DataRow drTemp in dsSorted.Tables[0].Rows)
                    foreach (DataRow drTemp in sortedTable.Rows)
                    {
                        if (string.IsNullOrEmpty(drTemp["tsGrupNo"].ToString()) == false)
                        {
                            if (drTemp["tsGrupNo"].ToString().Trim() == "26") { blnApplicator = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "25") { blnSlide = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "24") { blnRerunFirst = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "24") { blnRerunSecond = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "14") { blnSortedPB = true; blnNextStep = true; }

                            if (drTemp["tsGrupNo"].ToString().Trim() != "24" && drTemp["tsGrupNo"].ToString().Trim() != "25" &&
                                drTemp["tsGrupNo"].ToString().Trim() != "26" && drTemp["tsGrupNo"].ToString().Trim() != "2" &&
                                drTemp["tsGrupNo"].ToString().Trim() != "0" && drTemp["tsGrupNo"].ToString().Trim() != "1" && drTemp["tsGrupNo"].ToString().Trim() != "14")
                            {
                                blnSorted = true;
                            }
                            break;
                        }
                    }
                }

                if (blnSortedPB == true) { strSortIndex = ""; }

                #endregion ----- 7. 기분류된 내역 유무 조회

                #region ----- 8. CDR 오더가 있으나 XN에서 검사한 내역이 있을 경우 (떠보기 = 26, 슬라이드 = 25, 1차재검 = 24, 2차재검 = 24) 체크 && 특정 랙이 아닐 경우

                if (blnExistResultCDR == true && blnNextStep == false)
                {
                    //rtstJgmtVal, apctrJgmtVal, slid1JgmtVal, slid2JgmtVal
                    if (dsRsltXN != null && dsRsltXN.Tables.Count > 0)
                    {

                        //재검, 슬라이드, 떠보기 순으로 우선순위임!

                        filterExpression = "rtstJgmtVal = " + Common.STS("R");

                        selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                        if (selectedRows != null && selectedRows.Length > 0)
                        {
                            foreach (DataRow row in selectedRows)
                            {
                                strSortIndex = "24";
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, 재검판정: {row["rtstJgmtVal"]}";
                                break;
                            }
                        }

                        if (blnSlide == false && strSortIndex == "")
                        {
                            filterExpression = "slid1JgmtVal in ('S1', 'S2')";

                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                foreach (DataRow row in selectedRows)
                                {
                                    strSortIndex = "25";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, S1: {row["slid1JgmtVal"]}, S2: {row["slid2JgmtVal"]}";
                                    break;
                                }
                            }
                        }

                        if (blnApplicator == false && strSortIndex == "")
                        {
                            filterExpression = "apctrJgmtVal = 'A'";

                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                foreach (DataRow row in selectedRows)
                                {
                                    strSortIndex = "26";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, 떠보기판정: {row["apctrJgmtVal"]}";
                                    break;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(strSortIndex) == false)
                        {
                            dsData = null;
                            dsRsltXN = null;
                            dsSorted = null;

                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog += $", 분류명: {strSortDesc}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                            return strSortIndex;
                        }
                    }
                }

                #endregion ----- 8. CDR 오더가 있으나 XN에서 검사한 내역이 있을 경우 (떠보기 = 18, 슬라이드 = 19, 1차재검 = 20, 2차재검 = 21) 체크 && 특정 랙이 아닐 경우

                #region ----- 5-1. 서울 = 6 / 부산은 서울이 우선!!! / 2024-10-28 : CBC오더 있는데 검사안할 경우 라인빼는 게 우선이라고 해서 재수정

                //2025-04-18 : 서울+부산 이지만 CBC결과가 비정상일 경우 때문에 우선순위에서 내림 5에서 8 다음으로

                if (blnSeoul == true && blnSorted == false)
                {
                    dsData = null;
                    strSortIndex = "6";

                    if (blnBusan == true) { strSortIndex = "3"; }
                    if (blnPA8 == true) { strSortIndex = "2"; }

                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 2 = No Order(Data Error), 6 = 서울, 3 = 서울+부산, 분류코드: {strSortIndex}, 분류명: {strSortDesc}, 부산 {blnBusan} , PA8 {blnPA8}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 5-1. 서울 = 2

                #region ----- 9. 기분류된 검체일 경우 보관 = 1

                if (blnSorted == true)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(기분류), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 9. 기분류된 검체일 경우 보관 = 1

                #region ----- 10. 오더에 따른 분류

                if (dsData != null && dsData.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsData.Tables[0].Rows)
                    {
                        bool blnChk = false;

                        //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                        if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                        {
                            System.Diagnostics.Debug.Print(drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["SPC_CD"].ToString().Trim());

                            if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                if (spcGbn == "5")
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                    {
                                        blnChk = true;
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                    {
                                        blnChk = true;
                                    }
                                }
                            }
                            else
                            {
                                if (drTemp["SPC_CD"].ToString().Trim() == "")
                                {
                                    //if (aryTemp[i].IndexOf("RET") > -1)
                                    if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                    {
                                        if (spcGbn == "5")
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                        else
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if (blnChk == true)
                            {
                                if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                {
                                    if (drTemp["SPC_GBN"].ToString().Trim() == spcGbn && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                                    {
                                        if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                        {
                                            lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(strSortIndex) == false)
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;

                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog += $", 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                        return strSortIndex;
                    }

                    if (blnExistResultCDR && lstJobGroupCd.Contains("HE1") == false)
                    {
                        dsRsltHIS = BizData.GetRslt(strSpcNo);
                        if (dsRsltHIS != null && dsRsltHIS.Tables.Count > 0)
                        {
                            foreach (DataRow drTemp in dsRsltHIS.Tables[0].Rows)
                            {
                                lstJobGroupCd.Add("HE1");
                                break;
                            }
                        }
                    }

                    //2024-07-03 : 부산은 CBC 검사결과가 있더라도 분류기준에서 제외안함!!!
                    ////2024-05-22 : CBC검사결과가 있을 경우 분류 기준에서 HE1 제외
                    //////2024-08-08 : CBC 결과있으면 분류기준에서 제외
                    if (blnExistResultCDR == true)
                    {
                        //2025-04-16 : 타학부, PB 일 때 HE1을 제외할 것!
                        if (lstJobGroupCd.Contains("HE1") == true)
                        {
                            if (lstJobGroupCd.Contains("HE6") == true || otherUnit == true)
                            {
                                lstJobGroupCd.Remove("HE1");
                            }
                        }

                        if (lstJobGroupCd.Contains("HE3") == true) { lstJobGroupCd.Remove("HE3"); }
                    }

                    if (blnOther == true)
                    {
                        strSortIndex = "5";

                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, IMH, CC5 = 타학부, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }

                    if (blnComplete == true)
                    {
                        strSortIndex = "1";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 결과가 있으므로 아카이브, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }

                    if (lstJobGroupCd.Count > 0)
                    {
                        foreach (string job in lstJobGroupCd)
                        {
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: {job}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        }
                    }

                    if (blnSortedPB == true)
                    {
                        if (lstJobGroupCd.Contains("HE6") == true)
                        {
                            lstJobGroupCd.Remove("HE6");
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 14번 분류이력있어서 HE6 삭제" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        }
                    }

                    if (lstJobGroupCd.Count == 1)
                    {
                        //if (lstJobGroupCd.Contains("HE3") == true || lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "14"; }
                        if (lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "14"; }
                        if (lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "23"; }
                        if (lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "22"; }
                        if (lstJobGroupCd.Contains("HE7") == true) { strSortIndex = "22"; }
                        if (lstJobGroupCd.Contains("HE4") == true) { strSortIndex = "5"; }
                        if (lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "5"; }
                        if (lstJobGroupCd.Contains("HE2") == true) { strSortIndex = "5"; }
                        if (lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "7"; }

                        if ((blnExistResultCDR == true && lstJobGroupCd.Contains("HE1") == true) || (blnExistResultCDR == true && lstJobGroupCd.Contains("HE3") == true))
                        {
                            dsData = null;
                            dsRsltXN = null;
                            dsSorted = null;
                            strSortIndex = "1";
                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, C+D 완료 아카이브, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            return strSortIndex;
                        }
                    }
                    else if (lstJobGroupCd.Count == 2)
                    {
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "14"; }    // PBS + CBC
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true) { strSortIndex = "5"; }     // OTHER
                        if (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "14"; }    // PBS + CBC
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "21"; }    // CBC + ESR
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "20"; }    // CBC + ABO
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE7") == true) { strSortIndex = "20"; }    // CBC + ABO
                        if (lstJobGroupCd.Contains("HE2") == true && lstJobGroupCd.Contains("HE4") == true) { strSortIndex = "5"; }     // OTHER
                        if (lstJobGroupCd.Contains("HE2") == true && lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "5"; }     // OTHER
                        if (lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "5"; }     // OTHER
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "12"; }    // HE0 + HE8
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE7") == true) { strSortIndex = "12"; }    // HE0 + HE8
                        if (lstJobGroupCd.Contains("HE8") == true && lstJobGroupCd.Contains("HE9") == true) { strSortIndex = "22"; }    // ABO
                        if (lstJobGroupCd.Contains("HEB") == true)
                        {
                            strSortIndex = "7";

                            if (lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "4"; }      // PBS + HBA1C
                            if (lstJobGroupCd.Contains("HE1") == true) { strSortIndex = "7"; }      // CBC + HBA1C
                            if (lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "10"; }     // ABO + HBA1C
                            if (lstJobGroupCd.Contains("HE7") == true) { strSortIndex = "10"; }     // ABO + HBA1C
                            if (lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "11"; }     // ESR + HBA1C

                            //서울
                            //if (lstJobGroupCd.Contains("HE1") == false) { strSortIndex = "15"; }
                            //else { strSortIndex = "22"; }
                        }

                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("IMH") == true && blnExistResultCDR == true) { strSortIndex = "5"; }     // OTHER
                        if (lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE8") == true && blnExistResultCDR == true) { strSortIndex = "20"; }    // CBC + ABO
                        if (lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE7") == true && blnExistResultCDR == true) { strSortIndex = "20"; }    // CBC + ABO

                        if (lstJobGroupCd.Contains("HE6") == true)
                        {
                            if (lstJobGroupCd.Contains("HE1") == true)
                            {
                                strSortIndex = "14";
                            }

                            if (lstJobGroupCd.Contains("HEB") == true)
                            {
                                strSortIndex = "4";
                            }
                        }

                    }
                    else
                    {
                        //TS-10 Sort No
                        //1  = 아카이브
                        //2  = ERROR
                        //3  = 서울 + 부산
                        //4  = PBS + HBA1C
                        //5  = OTHER
                        //6  = 서울
                        //7  = CBC + HbA1C
                        //8  = 사용안함
                        //9  = HbA1C + ABO + ESR
                        //10 = HbA1C + ABO
                        //11 = HbA1C + ESR
                        //12 = ABO + ESR
                        //13 = CBC + ABO + ESR
                        //14 = PBS + CBC
                        //15 = 사용안함
                        //16 = 사용안함
                        //17 = CBC + HBA1c + ABO + ESR
                        //18 = CBC + HBA1c + ABO
                        //19 = CBC + HBA1c + ESR
                        //20 = CBC + ABO
                        //21 = CBC + ESR
                        //22 = ABO
                        //23 = ESR
                        //24 = 재검
                        //25 = SLIDE
                        //26 = MORE = 떠보기
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE8") == true && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "9"; }    // ESR + ABO + HBA1C
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "13"; }   // ESR + CBC + ABO
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "18"; }   // CBC + ABO + HBA1C
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "20"; }   // CBC + ABO

                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE7") == true && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "9"; }    // ESR + ABO + HBA1C
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE7") == true) { strSortIndex = "13"; }   // ESR + CBC + ABO
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE7") == true && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "18"; }   // CBC + ABO + HBA1C
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE7") == true) { strSortIndex = "20"; }   // CBC + ABO

                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "19"; }   // ESR + CBC + HBA1C

                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true &&
                            lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "17"; }                                                                                    // ESR + CBC + ABO + HBA1C

                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE7") == true &&
                                                    lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "17"; }                                                                                    // ESR + CBC + ABO + HBA1C

                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("IMH") == true && blnExistResultCDR == true) { strSortIndex = "5"; }     // OTHER
                        if (lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE8") == true && blnExistResultCDR == true) { strSortIndex = "20"; }    // CBC + ABO
                        if (lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE7") == true && blnExistResultCDR == true) { strSortIndex = "20"; }    // CBC + ABO
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HEB") == true && blnExistResultCDR == true) { strSortIndex = "11"; }    // HbA1C + ESR

                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE8") == true && blnExistResultCDR == true && lstJobGroupCd.Contains("HEB") == false)
                        {
                            strSortIndex = "12";
                        }    // ESR + ABO
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE7") == true && blnExistResultCDR == true && lstJobGroupCd.Contains("HEB") == false)
                        {
                            strSortIndex = "12";
                        }    // ESR + ABO

                        if (lstJobGroupCd.Contains("HE6") == true)
                        {
                            if (lstJobGroupCd.Contains("HE1") == true)
                            {
                                strSortIndex = "14";
                            }

                            if (lstJobGroupCd.Contains("HEB") == true)
                            {
                                strSortIndex = "4";
                            }
                        }

                        if (lstJobGroupCd.Contains("HEB") && string.IsNullOrEmpty(strSortIndex) == true)
                        {
                            strSortIndex = "7";
                        }

                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "13"; }   // ESR + CBC + ABO
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE7") == true) { strSortIndex = "13"; }   // ESR + CBC + ABO
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "19"; }
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "17"; }
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE7") == true && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "17"; }
                    }

                    if (string.IsNullOrEmpty(strSortIndex) == false)
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;

                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                        return strSortIndex;
                    }
                }

                #endregion ----- 10. 오더에 따른 분류

                #region ----- 11. 분류조건을 확인할 수 없음

                dsData = null;
                dsRsltXN = null;
                dsSorted = null;
                strSortIndex = "2";

                if (blnExistResultCDR == true || blnComplete == true)
                {
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류조건을 확인할 수 없으나 혈액학 결과가 있으므로 아카이브, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류조건을 확인할 수 없음, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                return strSortIndex;

                #endregion ----- 11. 분류조건을 확인할 수 없음

            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "GetSortIndex" + TAB + strSpcNo + TAB + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "GetSortIndex" + TAB + strSpcNo + TAB + strSortIndex + "\r\n",
                              false,
                              mstrAppPath + "log\\",
                              DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");


            return strSortIndex;
        }

        private string GetSortIndex_SeeGene_Daegu(string strSpcNo, string strRackNo)
        {
            string strSortIndex = "";

            try
            {
                bool blnExistRsltAll = false;
                bool blnTestCompleted = false;
                List<string> lstJobGroupCd = new List<string>();
                DataSet dsData = null;
                DataSet dsSorted = null;
                DataSet dsRsltXN = null;
                DataSet dsRsltHIS = null;

                string strLog;
                string strLine = new string('-', 20) + "\r\n";
                string strLisTstCds = "";
                string strLisTstSubCd = "";
                string strSortDesc = "";

                bool blnExistOrderCDR = false;
                bool blnNoOrder = true;
                bool blnExistResultCDR = false;
                bool blnNextStep = false;
                bool blnSorted = false;
                bool blnApplicator = false;
                bool blnSlide = false;
                bool blnRerunFirst = false;
                bool blnRerunSecond = false;
                bool blnSeoul = false;
                bool blnDaegu = false;
                bool blnAmmonia = false; //00026
                bool blnCompleteChkSpcCd = false;
                bool blnCompleteChkJobGrup = false;
                bool blnComplete = false;
                string spcGbn = "";
                string filterExpression;
                DataRow[] selectedRows;
                bool blnIMH = false;

                if (strSpcNo.Length == 12)
                {
                    spcGbn = strSpcNo[11].ToString(); // 문자열의 12번째 문자 (0부터 시작하므로 11번째 인덱스)
                }

                strLog = $"Start GetSortIndex_SeeGene_Daegu SpcNo: {strSpcNo}, RackNo: {strRackNo}" + "\r\n"; ;

                Common.File_Record("\r\n", false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                #region ----- 씨젠의료재단 대구 센터 SYSMEX TS-10 분류코드
                //2024-01-26 : 씨젠의료재단 대구 센터 진단검사의학과 혈액학부 SYSMEX TS-10 검체 분류 및 아카이브
                //TS-10 Sort No
                //1  = 아카이브
                //2  = Data Error
                //3  = 없음
                //4  = 면역                     IMH + @  
                //5  = 서울                     센터코드로 판단
                //6  = 서울 + 대구              센터코드로 판단
                //7  = CBC + ESR                HE0 + HE1
                //8  = ESR                      HE0
                //9  = CBC + Other              HE1 + HE3/HE6
                //10 = Other                    HE0,HE3,HE6,HE8 필수=HE3,HE6
                //11 = PB + CBC + RET           HE1,HE3,HE6
                //12 = CBC + PBS, PBS           HE1,HE6
                //13 = CBC + ABO                HE1,HE8
                //14 = ABO                      HE8
                //15 = CBC + ABO + ESR          HE0,HE1,HE8
                //16 = IMU                      
                //17 = Citrate All              HE2,HE4
                //18 = Citrate2                 HE2
                //19 = RET                      HE3
                //20 = CBC + RET                HE1,HE3
                //21 = HbA1c + @                HEB + @ (단독과 + CBC는 28로)
                //22 = AAI, AAD                 AAI + AAD
                //23 = Rerun Error              재검에러
                //24 = 2차 재검
                //25 = 1차 재검
                //26 = SLIDE
                //27 = 떠보기
                //28 = HEB + CBC                HEB, HEB+HE1
                //61 = BarError
                #endregion ----- 씨젠의료재단 대구 센터 SYSMEX TS-10 분류코드

                #region ----- 1. 바코드에러 = 61

                //1. 바코드에러 
                if (Common.IsNumeric(strSpcNo) == false)
                {
                    strSortIndex = "61";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 바코드에러, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 1. 바코드에러 = 61

                #region ----- 2. 아카이브 모드
                if (chkArchive.Checked == true)
                {
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Archive Mode, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }
                #endregion ----- 2. 아카이브 모드

                #region ----- 3. 오더조회

                strLog += "JOB_GRUP" + TAB + "LIS_TST_CD" + TAB + "LIS_TST_SUB_CD" + TAB + "TST_NM" + TAB + "STUS_CD" + "\r\n";

                dsData = BizData.GetSpcInfo(strSpcNo);

                if (spcGbn == "5")
                {
                    filterExpression = "SPC_CD = 'A04' ";
                    if (dsData != null && dsData.Tables.Count > 0)
                    {
                        selectedRows = dsData.Tables[0].Select(filterExpression);

                        if (selectedRows != null && selectedRows.Length > 0)
                        {
                            //오더 조회되었으므로 정상 진행
                        }
                        else
                        {
                            //오더 조회 안되었으니 Data Error 로 분류
                            strSortIndex = "2";
                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}, 5번 검체이면서 Citrate(A04) 오더조회 안됨!" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            return strSortIndex;
                        }
                    }
                    else
                    {
                        //오더 조회 안되었으니 Data Error 로 분류
                        strSortIndex = "2";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}, 5번 검체이면서 Citrate(A04) 오더조회 안됨!" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }
                }

                //2025-02-17 : 무조건 데이터에러로 처리
                //              3610 추후송부
                //              3630 우선검사진행
                //              9070 검사제외
                filterExpression = "TST_STAT_CD IN ('3610', '3630', '9070') ";
                if (dsData != null && dsData.Tables.Count > 0)
                {
                    selectedRows = dsData.Tables[0].Select(filterExpression);

                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        foreach (DataRow drTemp in selectedRows)
                        {
                            if (drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                strSortIndex = "2";
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }
                        }
                    }
                }

                if (dsData != null && dsData.Tables.Count > 0)
                {
                    blnCompleteChkSpcCd = true;
                    blnCompleteChkJobGrup = true;

                    filterExpression = $"SPC_CD IN ('A02','A04','A05') AND SPC_GBN = '{spcGbn}' AND TST_STAT_CD < '3060'";
                    selectedRows = dsData.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkSpcCd = false; }

                    filterExpression = $"SPC_CD = '' AND JOB_GRUP LIKE 'HE%' AND SPC_GBN = '{spcGbn}' AND TST_STAT_CD < '3060'";
                    selectedRows = dsData.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkJobGrup = false; }

                    //2024-12-17 : 9 번 검체구분자로 IM
                    if (spcGbn == "9")
                    {
                        //
                    }
                    else
                    {
                        if (blnCompleteChkJobGrup == true && blnCompleteChkSpcCd == true)
                        {
                            filterExpression = $"SPC_CD IN ('A02','A04','A05') AND SPC_GBN = '9' AND TST_STAT_CD < '3060'";
                            selectedRows = dsData.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkSpcCd = false; }

                            filterExpression = $"SPC_CD = '' AND JOB_GRUP LIKE 'HE%' AND SPC_GBN = '9' AND TST_STAT_CD < '3060'";
                            selectedRows = dsData.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkJobGrup = false; }
                        }
                    }

                    if (blnCompleteChkJobGrup == true && blnCompleteChkSpcCd == true)
                    {
                        blnComplete = true;
                    }

                    foreach (DataRow drTemp in dsData.Tables[0].Rows)
                    {
                        //EDTA: A05
                        //CITRATE : A04     = 5
                        //EDTA PLASMA: A02

                        if (dsData.Tables[0].Rows.Count > 0) { blnExistRsltAll = true; }

                        bool blnChk = false;

                        //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                        if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                        {
                            if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                if (spcGbn == "5")
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                    {
                                        blnChk = true;
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                    {
                                        blnChk = true;
                                    }
                                }
                            }
                            else
                            {
                                if (drTemp["SPC_CD"].ToString().Trim() == "")
                                {
                                    if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                    {
                                        if (spcGbn == "5")
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                        else
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if (blnChk == true)
                            {
                                if (string.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                {
                                    blnNoOrder = false;
                                }

                                strLisTstSubCd = drTemp["LIS_TST_SUB_CD"].ToString();
                                if (strLisTstSubCd == "-") { strLisTstSubCd = ""; }
                                strLisTstCds = drTemp["LIS_TST_CD"].ToString() + strLisTstSubCd;

                                //11310 = Reti count
                                //11017 = Eosinophil count (호산구수)
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE1") { blnExistOrderCDR = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE3" && strLisTstCds == "11310" && blnExistOrderCDR == false) { blnExistOrderCDR = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE9" && strLisTstCds == "11017" && blnExistOrderCDR == false) { blnExistOrderCDR = true; }
                                if (strLisTstCds == "00026" || strLisTstCds == "0002600") { blnAmmonia = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "IMH") { blnIMH = true; }

                                //2024-04-19 : 서울로 분류해야하는 기준 체크
                                if (drTemp["CNTR_CD"].ToString().Trim() == "13100005") { blnSeoul = true; }
                                if (drTemp["CNTR_CD"].ToString().Trim() == "14100000") { blnDaegu = true; }

                                if (drTemp["SPC_GBN"].ToString().Trim() == spcGbn && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                                {
                                    if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                    {
                                        lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                    }
                                }

                                if (drTemp["TST_RST"].ToString().Trim() == "" && blnExistRsltAll == true)
                                {
                                    blnExistRsltAll = false;
                                }

                                strLog += drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["LIS_TST_CD"].ToString().Trim() + TAB + drTemp["LIS_TST_SUB_CD"].ToString().Trim() + TAB;
                                strLog += drTemp["TST_NM"].ToString().Trim() + TAB + drTemp["STUS_CD"].ToString().Trim() + "\r\n";
                            }
                        }
                    }
                }

                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                #endregion ----- 3. 오더조회

                #region ----- 4. 오더조회 안됨 = 2

                if (blnNoOrder == true)
                {
                    dsData = null;
                    strSortIndex = "2";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, No Order(Data Error), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 4. 오더조회 안됨 = 2

                #region ----- 5. CDR 오더가 있으나 XN에서 검사한 내역이 없을 경우 LINE OUT = 0

                dsRsltXN = BizData.GetRsltHematology(strSpcNo);
                if (dsRsltXN != null && dsRsltXN.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsRsltXN.Tables[0].Rows)
                    {
                        blnExistResultCDR = true;
                        break;
                    }
                }

                //2025-04-30 : 암모니아 분류 조건 추가
                if (blnAmmonia == true && blnExistResultCDR == false)
                {
                    dsData = null;
                    dsRsltXN = null;
                    strSortIndex = "9";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, CDR 오더 + 암모니아, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                if (blnExistOrderCDR == true && blnExistResultCDR == false && blnSeoul == false)
                {
                    dsData = null;
                    dsRsltXN = null;
                    strSortIndex = "0";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, CDR 오더가 있으나 검사결과가 없을 경우, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 5. CDR 오더가 있으나 XN에서 검사한 내역이 없을 경우 LINE OUT

                #region ----- 5-1. 서울 = 6

                if (blnSeoul == true)
                {
                    if (blnAmmonia == false)
                    {
                        dsData = null;
                        strSortIndex = "5";

                        if (blnDaegu == true) { strSortIndex = "6"; }

                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 서울 또는 서울대구, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }
                    else
                    {
                        if (blnExistOrderCDR == true)
                        {
                            //서울+CBC+암모니아 = CBC + Other
                            dsData = null;
                            strSortIndex = "9";
                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 서울+CBC+암모니아 = CBC + Other, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            return strSortIndex;
                        }
                        else
                        {
                            //서울+암모니아 = 서울+대구
                            dsData = null;
                            strSortIndex = "6";
                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 서울+암모니아 = 서울+대구, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            return strSortIndex;
                        }
                    }
                }

                #endregion ----- 5-1. 서울 = 2

                #region ----- 6. 재검통과 랙인지 확인하기 (* XN검사결과 판정결과가 재검대상일 경우 계속 재검으로 분류되므로 특정 랙일 경우 체크 안함)

                if (Common.IsNumeric(strRackNo)) { strRackNo = Common.Val(strRackNo).ToString(); }
                if (glstNextStepRack.Contains(strRackNo)) { blnNextStep = true; }

                #endregion ----- 6. 재검통과 랙인지 확인하기 (* XN검사결과 판정결과가 재검대상일 경우 계속 재검으로 분류되므로 특정 랙일 경우 체크 안함)

                #region ----- 7. 기분류된 내역 유무 조회

                dsSorted = BizData.GetSpcPos(strSpcNo);
                if (dsSorted != null && dsSorted.Tables.Count > 0)
                {
                    filterExpression = "tsGrupNo = " + Common.STS("27");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnApplicator = true; blnNextStep = true; }

                    filterExpression = "tsGrupNo = " + Common.STS("26");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnSlide = true; blnNextStep = true; }

                    filterExpression = "tsGrupNo = " + Common.STS("25");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnRerunFirst = true; blnNextStep = true; }

                    DataView view = dsSorted.Tables[0].DefaultView;
                    view.Sort = "regDtm DESC";

                    // Create a new DataTable from the sorted view
                    DataTable sortedTable = view.ToTable();

                    //foreach (DataRow drTemp in dsSorted.Tables[0].Rows)
                    foreach (DataRow drTemp in sortedTable.Rows)
                    {
                        if (string.IsNullOrEmpty(drTemp["tsGrupNo"].ToString()) == false)
                        {
                            if (drTemp["tsGrupNo"].ToString().Trim() == "27") { blnApplicator = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "26") { blnSlide = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "25") { blnRerunFirst = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "24") { blnRerunSecond = true; }

                            if (drTemp["tsGrupNo"].ToString().Trim() != "24" && drTemp["tsGrupNo"].ToString().Trim() != "25" &&
                                drTemp["tsGrupNo"].ToString().Trim() != "26" && drTemp["tsGrupNo"].ToString().Trim() != "27" && drTemp["tsGrupNo"].ToString().Trim() != "2" &&
                                drTemp["tsGrupNo"].ToString().Trim() != "0" && drTemp["tsGrupNo"].ToString().Trim() != "1")
                            {
                                blnSorted = true;
                            }
                            break;
                        }
                    }
                }

                #endregion ----- 7. 기분류된 내역 유무 조회

                #region ----- 8. CDR 오더가 있으나 XN에서 검사한 내역이 있을 경우 (떠보기 = 27, 슬라이드 = 26, 1차재검 = 25, 2차재검 = 24) 체크 && 특정 랙이 아닐 경우

                if (blnExistResultCDR == true && blnNextStep == false)
                {
                    //rtstJgmtVal, apctrJgmtVal, slid1JgmtVal, slid2JgmtVal
                    if (dsRsltXN != null && dsRsltXN.Tables.Count > 0)
                    {

                        //재검, 슬라이드, 떠보기 순으로 우선순위임!
                        filterExpression = "rtstJgmtVal = " + Common.STS("R");

                        selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                        if (selectedRows != null && selectedRows.Length > 0)
                        {
                            foreach (DataRow row in selectedRows)
                            {
                                strSortIndex = "25";
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, 재검판정: {row["rtstJgmtVal"]}";
                                break;
                            }
                        }

                        if (blnSlide == false && strSortIndex == "")
                        {
                            filterExpression = "slid1JgmtVal in ('S1', 'S2')";

                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                foreach (DataRow row in selectedRows)
                                {
                                    strSortIndex = "26";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, S1: {row["slid1JgmtVal"]}, S2: {row["slid2JgmtVal"]}";
                                    break;
                                }
                            }
                        }

                        if (blnApplicator == false && strSortIndex == "")
                        {
                            filterExpression = "apctrJgmtVal = 'A'";

                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                foreach (DataRow row in selectedRows)
                                {
                                    strSortIndex = "27";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, 떠보기판정: {row["apctrJgmtVal"]}";
                                    break;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(strSortIndex) == false)
                        {
                            dsData = null;
                            dsRsltXN = null;
                            dsSorted = null;

                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog += $", 분류명: {strSortDesc}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                            return strSortIndex;
                        }
                    }
                }

                #endregion ----- 8. CDR 오더가 있으나 XN에서 검사한 내역이 있을 경우 (떠보기 = 18, 슬라이드 = 19, 1차재검 = 20, 2차재검 = 21) 체크 && 특정 랙이 아닐 경우

                #region ----- 9.0. 검사완료 검체일 경우 보관 = 1
                if (blnComplete == true)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(검사완료), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }
                #endregion ----- 9.0. 검사완료 검체일 경우 보관 = 1

                #region ----- 9.1 기분류된 검체일 경우 보관 = 1

                //if (blnSorted == true)
                //{
                //    dsData = null;
                //    dsRsltXN = null;
                //    dsSorted = null;
                //    strSortIndex = "1";
                //    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                //    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(기분류), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                //    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                //    return strSortIndex;
                //}

                #endregion ----- 9. 기분류된 검체일 경우 보관 = 1

                #region ----- 10. 오더에 따른 분류
                if (dsData != null && dsData.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsData.Tables[0].Rows)
                    {

                        bool blnChk = false;

                        //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                        if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                        {
                            System.Diagnostics.Debug.Print(drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["SPC_CD"].ToString().Trim());

                            if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                if (spcGbn == "5")
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                    {
                                        blnChk = true;
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                    {
                                        blnChk = true;
                                    }
                                }
                            }
                            else
                            {
                                if (drTemp["SPC_CD"].ToString().Trim() == "")
                                {
                                    //if (aryTemp[i].IndexOf("RET") > -1)
                                    if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                    {
                                        if (spcGbn == "5")
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                        else
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if (blnChk == true)
                            {
                                //if (drTemp["JOB_GRUP"].ToString().Trim() == "IMH" || drTemp["JOB_GRUP"].ToString().Trim() == "IMU")
                                //{
                                //    strSortIndex = "16";
                                //    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: {drTemp["JOB_GRUP"]}, 분류코드: {strSortIndex}";
                                //    break;
                                //}

                                if (drTemp["SPC_GBN"].ToString().Trim() == spcGbn && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                                {
                                    if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                    {
                                        lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                    }
                                }
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(strSortIndex) == false)
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;

                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog += $", 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                        return strSortIndex;
                    }

                    if (lstJobGroupCd.Contains("HE1") == false)
                    {
                        dsRsltHIS = BizData.GetRslt(strSpcNo);
                        if (dsRsltHIS != null && dsRsltHIS.Tables.Count > 0)
                        {
                            foreach (DataRow drTemp in dsRsltHIS.Tables[0].Rows)
                            {
                                lstJobGroupCd.Add("HE1");
                                break;
                            }
                        }

                        //일반결과조회 API 일 때는 검사항목 체크해야함. 혈액학 결과조회 API로 변경하면서 주석처리
                        //if (dsRsltHIS != null && dsRsltHIS.Tables.Count > 0)
                        //{
                        //    string filterExpression;
                        //    DataRow[] selectedRows;

                        //    filterExpression = "devcRsltChnl IN ('WBC','RBC')";

                        //    selectedRows = dsRsltHIS.Tables[0].Select(filterExpression);
                        //    if (selectedRows != null && selectedRows.Length > 0)
                        //    {
                        //        foreach (DataRow row in selectedRows)
                        //        {
                        //            lstJobGroupCd.Add("HE1");
                        //            break;
                        //        }
                        //    }
                        //}
                    }

                    //2025-04-29 : 테스트용...
                    //DataSet dsRslt = BizData.GetDevcTestResult2(strSpcNo);
                    //if (dsRslt != null && dsRslt.Tables.Count > 0)
                    //{
                    //    foreach (DataRow drTemp in dsRslt.Tables[0].Rows)
                    //    {

                    //        break;
                    //    }
                    //}

                    if (blnExistResultCDR && lstJobGroupCd.Contains("HE1") == false)
                    {
                        if (dsRsltHIS != null && dsRsltHIS.Tables.Count > 0)
                        {
                            filterExpression = "devcRsltVal = " + Common.STS("");

                            selectedRows = dsRsltHIS.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                foreach (DataRow row in selectedRows)
                                {
                                    //strSortIndex = "24";
                                    //strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, 재검판정: {row["rtstJgmtVal"]}";
                                    break;
                                }
                            }

                            foreach (DataRow drTemp in dsRsltHIS.Tables[0].Rows)
                            {
                                lstJobGroupCd.Add("HE1");
                                break;
                            }
                        }
                    }

                    //if (lstJobGroupCd.Contains("HE6") == true && blnExistResultCDR == true && lstJobGroupCd.Contains("HE1") == true)
                    //{
                    //    lstJobGroupCd.Remove("HE1");
                    //}

                    //2025-03-01 
                    filterExpression = "devcRsltChnl = 'RET%'";
                    selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        if (lstJobGroupCd.Contains("HE3") == true) { lstJobGroupCd.Remove("HE3"); }
                    }

                    //2025-03-01 : 대구씨젠에서도 IMH는 최종 분류로 처리하고 싶어해서 추가
                    // "IMH", "SE0", "RC2"를 제거하고 "HE"로 시작하는 값만 유지
                    if (lstJobGroupCd.Any(x => x.StartsWith("HE")))
                    {
                        // IMH, SE0, RC2를 리스트에서 제거
                        lstJobGroupCd = lstJobGroupCd.Where(x => !x.StartsWith("IMH") && !x.StartsWith("SE0") && !x.StartsWith("RC2") && !x.StartsWith("IMU"))
                                                     .ToList();

                        // "HE"로 시작하는 값만 남기기
                        lstJobGroupCd = lstJobGroupCd.Where(x => x.StartsWith("HE")).Distinct().ToList();
                    }

                    //2025-04-27 : 대구 IMH 분류 처리 추가
                    if (blnIMH == true && lstJobGroupCd.Count == 1 && lstJobGroupCd.Contains("HE1") == true)
                    {
                        lstJobGroupCd.Add("IMH");
                    }

                    //2025-04-29 : 상태값으로 분류조건 제외하기
                    if (dsData != null && dsData.Tables.Count > 0)
                    {
                        foreach (DataRow drTemp in dsData.Tables[0].Rows)
                        {
                            bool blnChk = false;

                            //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                            if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                            {
                                System.Diagnostics.Debug.Print(drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["SPC_CD"].ToString().Trim());

                                if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                {
                                    if (spcGbn == "5")
                                    {
                                        if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                    else
                                    {
                                        if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "")
                                    {
                                        if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                        {
                                            if (spcGbn == "5")
                                            {
                                                if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                                {
                                                    blnChk = true;
                                                }
                                            }
                                            else
                                            {
                                                if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                                {
                                                    blnChk = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (drTemp["JOB_GRUP"].ToString().Trim() == "GE4")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                    }
                                }

                                if (blnChk == true)
                                {
                                    if (drTemp["JOB_GRUP"].ToString().Trim() == "IMH" || drTemp["JOB_GRUP"].ToString().Trim() == "CC5")
                                    {
                                        //
                                    }

                                    if (drTemp["SPC_GBN"].ToString().Trim() == spcGbn && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                                    {

                                    }
                                    else
                                    {
                                        if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == true)
                                        {
                                            lstJobGroupCd.Remove(drTemp["JOB_GRUP"].ToString().Trim());
                                            blnTestCompleted = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //2025-04-30 : CDR 결과있을 경우 HE1 삭제
                    if (blnExistResultCDR == true && lstJobGroupCd.Contains("HE1") == true)
                    {
                        lstJobGroupCd.Remove("HE1");
                    }

                    //2025-05-19 : CDR 결과있을 경우 HE9 삭제 (HE9 + HE6 일 때 분류조건 없어서 최종으로 빠지면서 CDR 결과가 있어서 보관되는 오류가 생김...
                    if (blnExistResultCDR == true && lstJobGroupCd.Contains("HE9") == true)
                    {
                        lstJobGroupCd.Remove("HE9");
                    }

                    //2025-05-29
                    if (lstJobGroupCd.Count == 0 && blnTestCompleted == true && blnIMH == true)
                    {
                        lstJobGroupCd.Add("IMH");
                    }

                    if (lstJobGroupCd.Count == 0 && blnIMH == true)
                    {
                        lstJobGroupCd.Add("IMH");
                    }

                    if (lstJobGroupCd.Count > 0)
                    {
                        foreach (string job in lstJobGroupCd)
                        {
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: {job}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        }
                    }

                    if (lstJobGroupCd.Count == 0 && blnTestCompleted == true)
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;
                        strSortIndex = "1";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관 검사완료, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }

                    if (lstJobGroupCd.Count == 1)
                    {
                        if (lstJobGroupCd.Contains("HE3") == true) { strSortIndex = "19"; }     //RET
                        if (lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "8"; }      //ESR
                        if (lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "14"; }     //ABO
                        if (lstJobGroupCd.Contains("HE4") == true) { strSortIndex = "17"; }     //Citrate All
                        if (lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "17"; }
                        if (lstJobGroupCd.Contains("HE2") == true) { strSortIndex = "18"; }     //Citrate2
                        if (lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "28"; }     //HEB
                        if (lstJobGroupCd.Contains("AAI") == true) { strSortIndex = "22"; }     //
                        if (lstJobGroupCd.Contains("AAD") == true) { strSortIndex = "22"; }     //

                        //PBS + CBC
                        if (lstJobGroupCd.Contains("HE6") == true) 
                        { 
                            if(blnIMH == true)
                            {
                                strSortIndex = "4";
                            }
                            else
                            {
                                strSortIndex = "12";
                            }
                        }

                        if (lstJobGroupCd.Contains("IMU") == true) { strSortIndex = "16"; }     //IMU
                        if (lstJobGroupCd.Contains("IMH") == true) { strSortIndex = "4"; }      //IMH

                        if (blnAmmonia == true) { strSortIndex = "10"; }                        //Other
                    }
                    else if (lstJobGroupCd.Count == 2)
                    {
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "12"; }    //CBC + PBS
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true) { strSortIndex = "20"; }    //CBC + RET
                        if (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "10"; }    //Other
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "7"; }     //CBC + ESR
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "13"; }    //CBC + ABO
                        if (lstJobGroupCd.Contains("HE2") == true && lstJobGroupCd.Contains("HE4") == true) { strSortIndex = "17"; }    //CBC + Citrate
                        if (lstJobGroupCd.Contains("HE2") == true && lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "17"; }
                        if (lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "17"; }
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "15"; }    //CBC + ABO + ESR
                        if (lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "10"; }    //OTHER

                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("IMH") == true) { strSortIndex = "4"; }     //CBC + OTHER

                        if (lstJobGroupCd.Contains("HEB") == true)
                        {
                            if (lstJobGroupCd.Contains("HE1") == false) { strSortIndex = "21"; }
                            else { strSortIndex = "28"; }
                        }

                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE6") == true) 
                        { 
                            if (blnIMH ==  true)
                            {
                                strSortIndex = "4";
                            }
                            else
                            {
                                strSortIndex = "17";
                            }

                        }

                        if (blnAmmonia == true && blnExistOrderCDR) { strSortIndex = "9"; }                        //CBC+Other

                    }
                    else
                    {
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE8") == true && lstJobGroupCd.Contains("HEB") == false) { strSortIndex = "10"; }   //Other
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "15"; }   //CBC + ABO + ESR
                        if (lstJobGroupCd.Contains("HE2") == true && lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "17"; }   //Citrate All

                        //2025-03-30 : HE1 + HE6 + HE9
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE9") == true) { strSortIndex = "12"; }   //CBC + PBS

                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true && string.IsNullOrEmpty(strSortIndex))
                        {
                            if (lstJobGroupCd.Contains("HE3") == false)
                            {
                                if (lstJobGroupCd.Contains("HEB") == true)
                                {
                                    strSortIndex = "21";
                                }
                                else
                                {
                                    strSortIndex = "9";
                                }
                            }
                            else
                            {
                                if (lstJobGroupCd.Contains("HEB") == true)
                                {
                                    strSortIndex = "21";
                                }
                                else
                                {
                                    strSortIndex = "11";
                                }
                            }
                        }

                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true && string.IsNullOrEmpty(strSortIndex))
                        {
                            if (lstJobGroupCd.Contains("HE6") == false) { strSortIndex = "9"; }
                            else { strSortIndex = "11"; }
                        }

                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == false && lstJobGroupCd.Contains("HEB") == false && string.IsNullOrEmpty(strSortIndex)) { strSortIndex = "9"; }

                        if (lstJobGroupCd.Contains("HEB") == true && string.IsNullOrEmpty(strSortIndex))
                        {
                            strSortIndex = "21";
                        }
                    }

                    if (string.IsNullOrEmpty(strSortIndex) == false)
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;

                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                        return strSortIndex;
                    }
                }

                #endregion ----- 10. 오더에 따른 분류

                #region ----- 11. 분류조건을 확인할 수 없음

                if ((blnExistResultCDR == true && lstJobGroupCd.Contains("HE1") == true) || (blnExistResultCDR == true && lstJobGroupCd.Contains("HE3") == true))
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, C+D 완료 아카이브, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                if (blnExistResultCDR == true)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, C+D 완료 아카이브, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                if (blnExistRsltAll == true)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(모두 결과입력됨), 분류코드: {strSortIndex}, 분류명: {strSortDesc}, IMH? {blnIMH}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                dsData = null;
                dsRsltXN = null;
                dsSorted = null;
                strSortIndex = "2";
                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류조건을 확인할 수 없음, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                return strSortIndex;

                #endregion ----- 11. 분류조건을 확인할 수 없음

            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "GetSortIndex" + TAB + strSpcNo + TAB + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "GetSortIndex" + TAB + strSpcNo + TAB + strSortIndex + "\r\n",
                              false,
                              mstrAppPath + "log\\",
                              DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");


            return strSortIndex;
        }

        private string GetSortIndex_SeeGene_Gwangju(string strSpcNo, string strRackNo)
        {
            string strSortIndex = "";

            try
            {
                bool blnTestCompleted = false;
                List<string> lstJobGroupCd = new List<string>();
                DataSet dsData = null;
                DataSet dsSorted = null;
                DataSet dsRsltXN = null;
                DataSet dsRsltHIS = null;

                string strLog;
                string strLine = new string('-', 20) + "\r\n";
                string strLisTstCds = "";
                string strLisTstSubCd = "";
                string strSortDesc = "";

                bool blnExistOrderCDR = false;
                bool blnNoOrder = true;
                bool blnExistResultCDR = false;
                bool blnNextStep = false;
                bool blnSorted = false;
                bool blnApplicator = false;
                bool blnSlide = false;
                bool blnRerunFirst = false;
                bool blnRerunSecond = false;

                bool blnExistRsltAll = false;
                bool blnCompleteChkSpcCd = false;
                bool blnCompleteChkJobGrup = false;
                bool blnComplete = false;

                string spcGbn = "";
                string filterExpression;
                DataRow[] selectedRows;
                bool blnEOC = false;
                bool blnIMH = false;

                if (strSpcNo.Length == 12)
                {
                    spcGbn = strSpcNo[11].ToString(); // 문자열의 12번째 문자 (0부터 시작하므로 11번째 인덱스)
                }

                strLog = $"Start GetSortIndex_SeeGene_Gwangju SpcNo: {strSpcNo}, RackNo: {strRackNo}" + "\r\n"; ;

                Common.File_Record("\r\n", false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                #region ----- 씨젠의료재단 광주 센터 SYSMEX TS-10 분류코드
                //2024-01-26 : 씨젠의료재단 광주 센터 진단검사의학과 혈액학부 SYSMEX TS-10 검체 분류 및 아카이브
                //TS-10 Sort No
                //	1	ARCHIVE	999		0		                        보관	                    1
                //	2	0	100		0		                            Data Error	                2
                //	3	BARERR	0		0		                        Barcode Error	            3
                //	4	서울	110		0		                        서울	                    4
                //	5	서울광주	299		0		                    서울광주	                5
                //	6	0	200	HE8	0		                            ABO	                        6
                //	8	0	200	HE0	0		                            ESR	                        7
                //	7	0	210	HE8,HE0	3	                            HE8,HE0	ABO+ESR	            8
                //	7	0	210	HE8,HE0,HE1	3	                        HE8,HE0,HE1	ABO+ESR	        9
                //	7	0	210	HE8,HE0,HE1	3	                        HE8,HE0	CBC+ABO+ESR	        10
                //	7	0	210	HE8,HE0,HE1	4	                        HE8,HE0	CBC+ABO+ESR	        11
                //	7	0	210	HE8,HE0,HE1	5	                        HE8,HE0	CBC+ABO+ESR	        12
                //	9	0	230	HE1,HE0,HE3,HE6,HE8	2	                HE1,HE3/HE1,HE6	CBC+Other	13
                //	10	0	220	HE0,HE3,HE6,HE8,PA8	2	                HE3,HE6	Other	            14
                //	11	0	200	HE6,HE1,HE3	0		                    PB+CBC+Reti	                15
                //	12	0	220	HE1,HE6	3	                            HE1,HE6/HE6	CBC+PBS	        16
                //	13	0	200	HE1,HE8	0		                        CBC+ABO	                    17
                //	14	0	200	HE1,HE0	0		                        CBC+ESR	                    18
                //	15	0	200	HE4	0		                            Citrate	                    19
                //	//	16	0	200	HE2,HE4	0		                    Citrate All	                20
                //	15	0	200	PA8	0		                            PA8	                        21
                //	17	0	200	HE2	0		                            Citrate 2	                22
                //	17	0	200	IMH	0		                            IMH	                        23
                //	17	0	200	HE1,IMH	2	                            IMH	IMH	                    23
                //	18	0	200	HE3	0		                            Reti/11019=2	            24
                //	19	0	200	HE1,HE3	0		                        CBC+Reti	                25
                //	20	0	210	HEB,HE0,HE3,HE6,HE8,HE1,PA7	4	        HEB	HbA1c Other/4=HE1미완	26
                //	21	0	200	AAI,AAD	1		                        PCR	                        27
                //	22	RERERR	5		0		                        Rerun Error	                28
                //	23	2차재검	2		0		                        2차재검	                    29
                //	24	1차재검	1	TCZ042T	0		                    1차재검	                    30
                //	25	SLIDE	3	TCZ042T	0		                    SLIDE	                    31
                //	26	떠보기	4	TCZ042T	0		                    떠보기	                    32
                //	27	0	220	HEB	5	HEB/HEB,HE1/HEB,HE1,PA7	        HbA1c(Hba1c+CBC)/5=HE1완료	33

                #endregion ----- 씨젠의료재단 광주 센터 SYSMEX TS-10 분류코드

                #region ----- 1. 바코드에러 = 61

                //1. 바코드에러 
                if (Common.IsNumeric(strSpcNo) == false)
                {
                    strSortIndex = "61";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 바코드에러, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 1. 바코드에러 = 61

                #region ----- 2. 아카이브 모드
                if (chkArchive.Checked == true)
                {
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Archive Mode, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }
                #endregion ----- 2. 아카이브 모드

                #region ----- 3. 오더조회

                strLog += "JOB_GRUP" + TAB + "LIS_TST_CD" + TAB + "LIS_TST_SUB_CD" + TAB + "TST_NM" + TAB + "STUS_CD" + "\r\n";

                bool blnRET = false;
                bool blnPB = false;
                bool blnSeoul = false;
                bool blnGJ = false;

                dsData = BizData.GetSpcInfo(strSpcNo);

                if (spcGbn == "5")
                {
                    filterExpression = "SPC_CD = 'A04' ";
                    if (dsData != null && dsData.Tables.Count > 0)
                    {
                        selectedRows = dsData.Tables[0].Select(filterExpression);

                        if (selectedRows != null && selectedRows.Length > 0)
                        {
                            //오더 조회되었으므로 정상 진행
                        }
                        else
                        {
                            //오더 조회 안되었으니 Data Error 로 분류
                            strSortIndex = "2";
                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}, 5번 검체이면서 Citrate(A04) 오더조회 안됨!" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            return strSortIndex;
                        }
                    }
                    else
                    {
                        //오더 조회 안되었으니 Data Error 로 분류
                        strSortIndex = "2";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}, 5번 검체이면서 Citrate(A04) 오더조회 안됨!" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }
                }

                //2025-02-17 : 무조건 데이터에러로 처리
                //              3610 추후송부
                //              3630 우선검사진행
                //              9070 검사제외
                filterExpression = "TST_STAT_CD IN ('3610', '3630', '9070') ";

                if (dsData != null && dsData.Tables.Count > 0)
                {
                    selectedRows = dsData.Tables[0].Select(filterExpression);

                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        foreach (DataRow drTemp in selectedRows)
                        {
                            if (drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                strSortIndex = "2";
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }
                        }
                    }
                }

                if (dsData != null && dsData.Tables.Count > 0)
                {
                    if (dsData.Tables[0].Rows.Count > 0) { blnExistRsltAll = true; }

                    blnCompleteChkSpcCd = true;
                    blnCompleteChkJobGrup = true;

                    DataTable newTable;

                    filterExpression = $"LIS_TST_CD NOT IN ('11054') AND JOB_GRUP NOT IN ('HE6')";
                    selectedRows = dsData.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        newTable = selectedRows.CopyToDataTable();
                        filterExpression = $"SPC_CD IN ('A02','A04','A05') AND SPC_GBN = '{spcGbn}' AND TST_STAT_CD < '3060'";
                        selectedRows = newTable.Select(filterExpression);
                        if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkSpcCd = false; }

                        //2025-02-25 : 3번, 5번
                        if (blnCompleteChkSpcCd == true)
                        {
                            filterExpression = $"SPC_CD IN ('A02','A04','A05') AND SPC_GBN IN ('3','5') AND TST_STAT_CD < '3060'";
                            selectedRows = newTable.Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkSpcCd = false; }
                        }

                        filterExpression = $"SPC_CD = '' AND JOB_GRUP LIKE 'HE%' AND SPC_GBN = '{spcGbn}' AND TST_STAT_CD < '3060'";
                        selectedRows = newTable.Select(filterExpression);
                        if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkJobGrup = false; }

                    }

                    //2024-12-17 : 9 번 검체구분자로 IM
                    if (spcGbn == "9")
                    {
                        //
                    }
                    else
                    {
                        if (blnCompleteChkJobGrup == true && blnCompleteChkSpcCd == true)
                        {
                            filterExpression = $"SPC_CD IN ('A02','A04','A05') AND SPC_GBN = '9' AND TST_STAT_CD < '3060'";
                            selectedRows = dsData.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkSpcCd = false; }

                            filterExpression = $"SPC_CD = '' AND JOB_GRUP LIKE 'HE%' AND SPC_GBN = '9' AND TST_STAT_CD < '3060'";
                            selectedRows = dsData.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkJobGrup = false; }
                        }
                    }

                    if (blnCompleteChkJobGrup == true && blnCompleteChkSpcCd == true)
                    {
                        blnComplete = true;
                    }

                    foreach (DataRow drTemp in dsData.Tables[0].Rows)
                    {
                        //EDTA: A05
                        //CITRATE : A04
                        //EDTA PLASMA: A02

                        bool blnChk = false;

                        //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                        if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                        {
                            System.Diagnostics.Debug.Print(drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["SPC_CD"].ToString().Trim());

                            if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                if (spcGbn == "5")
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                    {
                                        blnChk = true;
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                    {
                                        blnChk = true;
                                    }
                                }
                            }
                            else
                            {
                                if (drTemp["SPC_CD"].ToString().Trim() == "")
                                {
                                    //if (aryTemp[i].IndexOf("RET") > -1)
                                    if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                    {
                                        if (spcGbn == "5")
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                        else
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (drTemp["JOB_GRUP"].ToString().Trim() == "GE4")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                }
                            }

                            if (blnChk == true)
                            {
                                if (string.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                {
                                    blnNoOrder = false;
                                }

                                strLisTstSubCd = drTemp["LIS_TST_SUB_CD"].ToString();
                                if (strLisTstSubCd == "-") { strLisTstSubCd = ""; }
                                strLisTstCds = drTemp["LIS_TST_CD"].ToString() + strLisTstSubCd;

                                //11310 = Reti count
                                //11017 = Eosinophil count (호산구수)
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE1") { blnExistOrderCDR = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE3" && strLisTstCds == "11310") { blnRET = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE6") { blnPB = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE9" && strLisTstCds == "11017" && blnExistOrderCDR == false) { blnExistOrderCDR = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "IMH") { blnIMH = true; }
                                if (strLisTstCds == "11017") { blnEOC = true; }

                                if (drTemp["TST_RST"].ToString().Trim() == "" && blnExistRsltAll == true)
                                {
                                    blnExistRsltAll = false;
                                }

                                if (drTemp["CNTR_CD"].ToString().Trim() == "13100005") { blnSeoul = true; }
                                if (drTemp["CNTR_CD"].ToString().Trim() == "14300000") { blnGJ = true; }

                                if (drTemp["SPC_GBN"].ToString().Trim() == spcGbn)
                                {
                                    if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                    {
                                        if (drTemp["JOB_GRUP"].ToString().Trim() == "HE1" && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                                        {
                                            lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                        }
                                        else
                                        {
                                            lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                        }
                                    }
                                }


                                strLog += drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["LIS_TST_CD"].ToString().Trim() + TAB + drTemp["LIS_TST_SUB_CD"].ToString().Trim() + TAB;
                                strLog += drTemp["TST_NM"].ToString().Trim() + TAB + drTemp["STUS_CD"].ToString().Trim() + "\r\n";
                            }
                        }
                    }
                }

                if (blnRET == true || blnPB == true)
                {
                    blnExistOrderCDR = false;
                }

                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                #endregion ----- 3. 오더조회

                #region ----- 4. 오더조회 안됨 = 24

                if (blnNoOrder == true)
                {
                    dsData = null;
                    strSortIndex = "5";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, No Order(Data Error), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 4. 오더조회 안됨 = 24

                #region ----- 5. CDR 오더가 있으나 XN에서 검사한 내역이 없을 경우 LINE OUT = 0

                dsRsltXN = BizData.GetRsltHematology(strSpcNo);
                if (dsRsltXN != null && dsRsltXN.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsRsltXN.Tables[0].Rows)
                    {
                        blnExistResultCDR = true;
                        break;
                    }
                }

                if (blnExistOrderCDR == true && blnExistResultCDR == false && blnSeoul == false)
                {
                    dsData = null;
                    dsRsltXN = null;
                    strSortIndex = "0";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, CDR 오더가 있으나 검사결과가 없을 경우, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 5. CDR 오더가 있으나 XN에서 검사한 내역이 없을 경우 LINE OUT

                #region ----- 5-1. 서울 = 6

                if (blnSeoul == true)
                {
                    dsData = null;
                    strSortIndex = "4";

                    if (blnGJ == true)
                    {
                        if (lstJobGroupCd.Count >= 2 && lstJobGroupCd.Contains("HE1") == true && blnExistResultCDR == true)
                        {
                            //pass
                        }
                        else
                        {
                            strSortIndex = "5";
                        }
                    }

                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 서울 또는 서울+광주, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 5-1. 서울 = 2

                #region ----- 6. 재검통과 랙인지 확인하기 (* XN검사결과 판정결과가 재검대상일 경우 계속 재검으로 분류되므로 특정 랙일 경우 체크 안함)

                if (Common.IsNumeric(strRackNo)) { strRackNo = Common.Val(strRackNo).ToString(); }
                if (glstNextStepRack.Contains(strRackNo)) { blnNextStep = true; }

                #endregion ----- 6. 재검통과 랙인지 확인하기 (* XN검사결과 판정결과가 재검대상일 경우 계속 재검으로 분류되므로 특정 랙일 경우 체크 안함)

                #region ----- 7. 기분류된 내역 유무 조회

                dsSorted = BizData.GetSpcPos(strSpcNo);
                if (dsSorted != null && dsSorted.Tables.Count > 0)
                {
                    filterExpression = "tsGrupNo IN ('23','24','25','26')";
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnApplicator = true; blnSlide = true; blnRerunFirst = true; blnRerunSecond = true; blnNextStep = true; }

                    DataView view = dsSorted.Tables[0].DefaultView;
                    view.Sort = "regDtm DESC";

                    // Create a new DataTable from the sorted view
                    DataTable sortedTable = view.ToTable();

                    //foreach (DataRow drTemp in dsSorted.Tables[0].Rows)
                    foreach (DataRow drTemp in sortedTable.Rows)
                    {
                        if (string.IsNullOrEmpty(drTemp["tsGrupNo"].ToString()) == false)
                        {
                            if (drTemp["tsGrupNo"].ToString().Trim() == "26") { blnApplicator = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "25") { blnSlide = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "24") { blnRerunFirst = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "23") { blnRerunSecond = true; blnNextStep = true; }

                            if (drTemp["tsGrupNo"].ToString().Trim() != "26" && drTemp["tsGrupNo"].ToString().Trim() != "25" &&
                                drTemp["tsGrupNo"].ToString().Trim() != "24" && drTemp["tsGrupNo"].ToString().Trim() != "23" &&
                                drTemp["tsGrupNo"].ToString().Trim() != "0" && drTemp["tsGrupNo"].ToString().Trim() != "1")
                            {
                                blnSorted = true;
                            }
                            break;
                        }
                    }
                }

                #endregion ----- 7. 기분류된 내역 유무 조회

                #region ----- 8. CDR 오더가 있으나 XN에서 검사한 내역이 있을 경우 (떠보기 = 12, 슬라이드 = 12, 1차재검 = 11, 2차재검 = 11) 체크 && 특정 랙이 아닐 경우

                if (blnExistResultCDR == true && blnNextStep == false)
                {
                    //rtstJgmtVal, apctrJgmtVal, slid1JgmtVal, slid2JgmtVal
                    if (dsRsltXN != null && dsRsltXN.Tables.Count > 0)
                    {
                        //재검, 슬라이드, 떠보기 순으로 우선순위임!                       
                        filterExpression = "rtstJgmtVal = " + Common.STS("R");

                        selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                        if (selectedRows != null && selectedRows.Length > 0)
                        {
                            foreach (DataRow row in selectedRows)
                            {
                                strSortIndex = "24";
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, 재검판정: {row["rtstJgmtVal"]}";
                                break;
                            }
                        }

                        if (blnSlide == false && strSortIndex == "")
                        {
                            filterExpression = "slid1JgmtVal in ('S1', 'S2')";

                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                foreach (DataRow row in selectedRows)
                                {
                                    strSortIndex = "25";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, S1: {row["slid1JgmtVal"]}, S2: {row["slid2JgmtVal"]}";
                                    break;
                                }
                            }
                        }

                        if (blnApplicator == false && strSortIndex == "")
                        {
                            filterExpression = "apctrJgmtVal = 'A'";

                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                foreach (DataRow row in selectedRows)
                                {
                                    strSortIndex = "26";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, 떠보기: {row["apctrJgmtVal"]}";
                                    break;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(strSortIndex) == false)
                        {
                            dsData = null;
                            dsRsltXN = null;
                            dsSorted = null;

                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog += $", 분류명: {strSortDesc}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                            return strSortIndex;
                        }
                    }
                }

                #endregion ----- 8. CDR 오더가 있으나 XN에서 검사한 내역이 있을 경우 (떠보기 = 18, 슬라이드 = 19, 1차재검 = 20, 2차재검 = 21) 체크 && 특정 랙이 아닐 경우

                #region ----- 9.0. 검사완료 검체일 경우 보관 = 1
                if (blnComplete == true)
                {
                    string[] checkItems = { "HE1", "IMH", "HE0", "HE3", "HE6", "PA8", "HE4", "HE2", "HEB", "PA7", "AAI", "AAD" };

                    // HashSet을 사용하여 성능 향상
                    HashSet<string> checkSet = new HashSet<string>(checkItems, StringComparer.OrdinalIgnoreCase); // 대소문자 무시

                    bool hasAnyItem = lstJobGroupCd.Any(item => checkSet.Contains(item));

                    if (hasAnyItem)
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;
                        strSortIndex = "1";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(검사완료), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }
                    else
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;
                        strSortIndex = "5";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(검사완료), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }
                }
                #endregion ----- 9.0. 검사완료 검체일 경우 보관 = 1

                if (lstJobGroupCd.Count == 1 && lstJobGroupCd.Contains("HE1") && blnExistOrderCDR == true && blnExistResultCDR == true && blnIMH == false)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(검사완료), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                if (lstJobGroupCd.Count == 2 && lstJobGroupCd.Contains("HE1") && lstJobGroupCd.Contains("HE9") && blnExistOrderCDR == true && blnExistResultCDR == true && blnIMH == false)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(검사완료), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                if (lstJobGroupCd.Count == 2 && lstJobGroupCd.Contains("HE1") && lstJobGroupCd.Contains("HE3") && blnExistOrderCDR == true && blnExistResultCDR == true && blnIMH == false)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(검사완료), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #region ----- 9. 기분류된 검체일 경우 보관 = 1

                //if (blnSorted == true)
                //{
                //    dsData = null;
                //    dsRsltXN = null;
                //    dsSorted = null;
                //    strSortIndex = "1";
                //    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                //    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(기분류), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                //    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                //    return strSortIndex;
                //}

                #endregion ----- 9. 기분류된 검체일 경우 보관 = 1

                if (blnExistRsltAll == true)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(모두 결과입력됨), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #region ----- 10. 오더에 따른 분류

                if (blnEOC == true && lstJobGroupCd.Contains("HE9") == true) { lstJobGroupCd.Remove("HE9"); }

                if (dsData != null && dsData.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsData.Tables[0].Rows)
                    {

                        bool blnChk = false;

                        //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                        if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                        {
                            System.Diagnostics.Debug.Print(drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["SPC_CD"].ToString().Trim());

                            if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                if (spcGbn == "5")
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                    {
                                        blnChk = true;
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                    {
                                        blnChk = true;
                                    }
                                }
                            }
                            else
                            {
                                if (drTemp["SPC_CD"].ToString().Trim() == "")
                                {
                                    //if (aryTemp[i].IndexOf("RET") > -1)
                                    if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                    {
                                        if (spcGbn == "5")
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                        else
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if (blnChk == true)
                            {
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "IMH" || drTemp["JOB_GRUP"].ToString().Trim() == "SE0")
                                {
                                    strSortIndex = "17";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: {drTemp["JOB_GRUP"]}, 분류코드: {strSortIndex}";
                                    break;
                                }

                                if (drTemp["JOB_GRUP"].ToString().Trim() == "PA8")
                                {

                                    if (strSortIndex == "" && lstJobGroupCd.Contains("HEB") == true)
                                    {
                                        if (lstJobGroupCd.Contains("HE1") == true)
                                        {
                                            if (blnExistOrderCDR == true)
                                            {
                                                //HbA1c + OTHER
                                                strSortIndex = "20";
                                            }
                                            else
                                            {
                                                if (blnRET == true || blnPB == true)
                                                {
                                                    strSortIndex = "20";
                                                }
                                                else
                                                {
                                                    strSortIndex = "7";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            strSortIndex = "20";
                                        }
                                    }
                                    else
                                    {
                                        strSortIndex = "15";
                                    }

                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: {drTemp["JOB_GRUP"]}, 분류코드: {strSortIndex}";
                                    break;
                                }

                                if (drTemp["JOB_GRUP"].ToString().Trim() == "AAI" || drTemp["JOB_GRUP"].ToString().Trim() == "AAD")
                                {
                                    strSortIndex = "21";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: {drTemp["JOB_GRUP"]}, 분류코드: {strSortIndex}";
                                    break;
                                }

                                //TST_STAT_CD
                                //SPC_GBN

                                if (drTemp["SPC_GBN"].ToString().Trim() == spcGbn && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                                {
                                    if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                    {
                                        lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                    }
                                }
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(strSortIndex) == false)
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;

                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog += $", 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                        return strSortIndex;
                    }

                    //if (blnExistResultCDR && lstJobGroupCd.Contains("HE1") == false)
                    //{
                    //    dsRsltHIS = BizData.GetRslt(strSpcNo);
                    //    if (dsRsltHIS != null && dsRsltHIS.Tables.Count > 0)
                    //    {
                    //        foreach (DataRow drTemp in dsRsltHIS.Tables[0].Rows)
                    //        {
                    //            lstJobGroupCd.Add("HE1");
                    //            break;
                    //        }
                    //    }
                    //}

                    //2024-12-22 : C+D 결과 나오더라도 제외하지 않도록 수정

                    //if (blnExistResultCDR && lstJobGroupCd.Contains("HE1") == true)
                    //{
                    //    lstJobGroupCd.Remove("HE1");
                    //}

                    //2025-04-29 : 상태값으로 분류조건 제외하기
                    if (dsData != null && dsData.Tables.Count > 0)
                    {
                        foreach (DataRow drTemp in dsData.Tables[0].Rows)
                        {
                            bool blnChk = false;

                            //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                            if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                            {
                                System.Diagnostics.Debug.Print(drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["SPC_CD"].ToString().Trim());

                                if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                {
                                    if (spcGbn == "5")
                                    {
                                        if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                    else
                                    {
                                        if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "")
                                    {
                                        if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                        {
                                            if (spcGbn == "5")
                                            {
                                                if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                                {
                                                    blnChk = true;
                                                }
                                            }
                                            else
                                            {
                                                if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                                {
                                                    blnChk = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (drTemp["JOB_GRUP"].ToString().Trim() == "GE4")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                    }
                                }

                                if (blnChk == true)
                                {
                                    if (drTemp["JOB_GRUP"].ToString().Trim() == "IMH" || drTemp["JOB_GRUP"].ToString().Trim() == "CC5")
                                    {
                                        //
                                    }

                                    if (drTemp["SPC_GBN"].ToString().Trim() == spcGbn && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                                    {

                                    }
                                    else
                                    {
                                        if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == true)
                                        {
                                            lstJobGroupCd.Remove(drTemp["JOB_GRUP"].ToString().Trim());
                                            blnTestCompleted = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (lstJobGroupCd.Count > 0)
                    {
                        foreach (string job in lstJobGroupCd)
                        {
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: {job}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        }
                    }

                    Common.File_Record("GroupCd: " + lstJobGroupCd.Count.ToString() + "\r\n", false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                    if (lstJobGroupCd.Count == 0 && blnTestCompleted == true)
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;
                        strSortIndex = "1";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관 검사완료, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }

                    if (lstJobGroupCd.Count == 1)
                    {
                        if (lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "12"; }
                        if (lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "6"; }
                        if (lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "8"; }
                        if (lstJobGroupCd.Contains("HE4") == true) { strSortIndex = "15"; }
                        if (lstJobGroupCd.Contains("PA8") == true) { strSortIndex = "15"; }
                        if (lstJobGroupCd.Contains("HE2") == true) { strSortIndex = "17"; }
                        if (lstJobGroupCd.Contains("IMH") == true) { strSortIndex = "17"; }
                        if (lstJobGroupCd.Contains("HE3") == true) { strSortIndex = "18"; }
                        if (lstJobGroupCd.Contains("AAI") == true) { strSortIndex = "21"; }
                        if (lstJobGroupCd.Contains("AAD") == true) { strSortIndex = "21"; }
                        if (lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "27"; }
                    }
                    else if (lstJobGroupCd.Count == 2)
                    {

                        /*
1.CBC + A1c + 추가 오더있을때 > H1c Other
2. H1c 없고 다른 오더있으면 > CBC+Other
3. CBC + A1c , A1c면 > A1c + CBC

                	1	ARCHIVE	            999		0		                                보관	                    1
                	2	0	                100		0		                                Data Error	                2
                	3	BARERR	            0		0		                                Barcode Error	            3
                	4	서울	            110		0		                                서울	                    4
                	5	서울광주	        299		0		                                서울광주	                5
                	6	0	                200	HE8	0		                                ABO	                        6
                	8	0	                200	HE0	0		                                ESR	                        7
                	7	0	                210	HE8,HE0	3	HE8,HE0	                        ABO+ESR	                    8
                	7	0	                210	HE8,HE0,HE1	3	HE8,HE0,HE1	                ABO+ESR	                    9
                	7	0	                210	HE8,HE0,HE1	3	HE8,HE0	                    CBC+ABO+ESR	                10
                	7	0	                210	HE8,HE0,HE1	4	HE8,HE0	                    CBC+ABO+ESR	                11
                	7	0	                210	HE8,HE0,HE1	5	HE8,HE0	                    CBC+ABO+ESR	                12
                	9	0	                230	HE1,HE0,HE3,HE6,HE8	2   HE1,HE3/HE1,HE6	    CBC+Other	                13
                	10	0	                220	HE0,HE3,HE6,HE8,PA8	2   HE3,HE6	            Other	                    14
                	11	0	                200	HE6,HE1,HE3	0		                        PB+CBC+Reti	                15
                	12	0	                220	HE1,HE6	3	HE1,HE6/HE6	                    CBC+PBS	                    16
                	13	0	                200	HE1,HE8	0		                            CBC+ABO	                    17
                	14	0	                200	HE1,HE0	0		                            CBC+ESR	                    18
                	15	0	                200	HE4	0		                                Citrate	                    19
                    16	                0	200	HE2,HE4	0		                            Citrate All	                20
                	15	0	                200	PA8	0		                                PA8	                        21
                	17	0	                200	HE2	0		                                Citrate 2	                22
                	17	0	                200	IMH	0		                                IMH	                        23
                	17	0	                200	HE1,IMH	2	                                IMH	IMH	                    23
                	18	0	                200	HE3	0		                                Reti/11019=2	            24
                	19	0	                200	HE1,HE3	0		                            CBC+Reti	                25
                	20	0	                210	HEB,HE0,HE3,HE6,HE8,HE1,PA7	4	HEB	        HbA1c Other/4=HE1미완	    26
                	21	0	                200	AAI,AAD	1		                            PCR	                        27
                	22	RERERR	5		0		                                            Rerun Error	                28
                	23	2차재검	2		0		                                            2차재검	                    29
                	24	1차재검	1	TCZ042T	0		                                        1차재검	                    30
                	25	SLIDE	3	TCZ042T	0		                                        SLIDE	                    31
                	26	떠보기	4	TCZ042T	0		                                        떠보기	                    32
                	27	0	220	HEB	5	HEB/HEB,HE1/HEB,HE1,PA7	                            HbA1c(Hba1c+CBC)/5=HE1완료	33
                         * 
                         */

                        if (lstJobGroupCd.Contains("HE8") == true && lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "7"; }
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "12"; }
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "13"; }
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "14"; }
                        if (lstJobGroupCd.Contains("HE2") == true && lstJobGroupCd.Contains("HE4") == true) { strSortIndex = "16"; }
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("IMH") == true) { strSortIndex = "17"; }
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE9") == true) { strSortIndex = "9"; }
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true) { strSortIndex = "19"; }
                        if (lstJobGroupCd.Contains("AAI") == true && lstJobGroupCd.Contains("AAD") == true) { strSortIndex = "21"; }
                        if (lstJobGroupCd.Contains("HE7") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "6"; }
                        if (strSortIndex == "" && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true && blnExistResultCDR == true && lstJobGroupCd.Contains("HE2") == false && lstJobGroupCd.Contains("HEB") == false) { strSortIndex = "11"; }
                        if (strSortIndex == "" && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE1") == false && lstJobGroupCd.Contains("HE2") == false && lstJobGroupCd.Contains("HEB") == false) { strSortIndex = "10"; }

                        if (lstJobGroupCd.Contains("HEB") == true)
                        {
                            if (lstJobGroupCd.Contains("HE1") == true)
                            {
                                strSortIndex = "27";
                            }
                            else
                            {
                                strSortIndex = "20";
                            }
                        }
                        else
                        {
                            if (lstJobGroupCd.Contains("HE1") == true && strSortIndex == "")
                            {
                                strSortIndex = "9";
                            }
                        }


                    }
                    else
                    {
                        if (strSortIndex == "" && lstJobGroupCd.Contains("HEB") == true)
                        {
                            if (lstJobGroupCd.Contains("HE1") == true)
                            {
                                if (blnExistOrderCDR == true)
                                {
                                    //HbA1c + OTHER
                                    strSortIndex = "20";
                                }
                                else
                                {
                                    if (blnRET == true || blnPB == true)
                                    {
                                        strSortIndex = "20";
                                    }
                                    else
                                    {
                                        //CBC + OTHER
                                        //if (lstJobGroupCd.Contains("HE8") == true && lstJobGroupCd.Contains("HE0") == true) 
                                        //{ 
                                        //    strSortIndex = "7"; 
                                        //}
                                        //else
                                        //{
                                        //    strSortIndex = "9";
                                        //}
                                        strSortIndex = "7";
                                    }
                                }
                            }
                            else
                            {
                                strSortIndex = "20";
                            }
                        }
                        if (strSortIndex == "" && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "20"; }
                        if (strSortIndex == "" && lstJobGroupCd.Count == 3 && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "11"; }
                        if (strSortIndex == "" && lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true && lstJobGroupCd.Contains("HE6") == false) { strSortIndex = "7"; }
                        if (strSortIndex == "" && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "11"; }
                        if (strSortIndex == "" && lstJobGroupCd.Contains("HE2") == true && lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE6") == false) { strSortIndex = "20"; }
                        if (strSortIndex == "" && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "12"; }
                        if (strSortIndex == "" && lstJobGroupCd.Contains("HE1") == true) { strSortIndex = "9"; }
                        if (strSortIndex == "" && lstJobGroupCd.Contains("HE1") == false) { strSortIndex = "10"; }
                        if (strSortIndex == "" && lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE1") == false && lstJobGroupCd.Contains("HE2") == false && lstJobGroupCd.Contains("HEB") == false) { strSortIndex = "10"; }

                        if (strSortIndex == "")
                        {
                            if (lstJobGroupCd.Contains("HEB") == true)
                            {
                                strSortIndex = "20";
                            }
                            else
                            {
                                if (lstJobGroupCd.Contains("HE1") == true && strSortIndex == "")
                                {
                                    strSortIndex = "9";
                                }
                            }

                            if (lstJobGroupCd.Contains("HE6") == true && (lstJobGroupCd.Contains("HE0") == true || lstJobGroupCd.Contains("HE1") == true || lstJobGroupCd.Contains("HE3") == true || lstJobGroupCd.Contains("HE8") == true)) { strSortIndex = "9"; }
                        }
                    }

                    if (string.IsNullOrEmpty(strSortIndex) == false)
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;

                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                        return strSortIndex;
                    }
                }

                #endregion ----- 10. 오더에 따른 분류

                #region ----- 11. 분류조건을 확인할 수 없음

                if (blnExistOrderCDR == true && blnExistResultCDR == true)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(XN검사완료), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                dsData = null;
                dsRsltXN = null;
                dsSorted = null;
                strSortIndex = "5";
                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류조건을 확인할 수 없음, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                return strSortIndex;

                #endregion ----- 11. 분류조건을 확인할 수 없음

            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "GetSortIndex" + TAB + strSpcNo + TAB + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "GetSortIndex" + TAB + strSpcNo + TAB + strSortIndex + "\r\n",
                              false,
                              mstrAppPath + "log\\",
                              DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");


            return strSortIndex;
        }

        private string GetSortIndex_SeeGene_Daejeon(string strSpcNo, string strRackNo)
        {
            string strSortIndex = "";

            try
            {
                bool blnTestCompleted = false;
                List<string> lstJobGroupCd = new List<string>();
                DataSet dsData = null;
                DataSet dsSorted = null;
                DataSet dsRsltXN = null;
                DataSet dsRsltHIS = null;
                DataSet dsRerunPassRack = null;

                string strLog;
                string strLine = new string('-', 20) + "\r\n";
                string strLisTstCds = "";
                string strLisTstSubCd = "";
                string strSortDesc = "";

                bool blnExistOrderCDR = false;
                bool blnNoOrder = true;
                bool blnExistResultCDR = false;
                bool blnNextStep = false;
                bool blnSorted = false;
                bool blnApplicator = false;
                bool blnSlide = false;
                bool blnRerunFirst = false;
                bool blnRerunSecond = false;
                bool blnSeoul = false;
                bool blnDJ = false;
                bool blnPB = false;
                bool blnRET = false;
                bool blnCompleteChkSpcCd = false;
                bool blnCompleteChkJobGrup = false;
                bool blnComplete = false;
                string spcGbn = "";
                string filterExpression;
                DataRow[] selectedRows;
                bool blnOther = false;

                if (strSpcNo.Length == 12)
                {
                    spcGbn = strSpcNo[11].ToString(); // 문자열의 12번째 문자 (0부터 시작하므로 11번째 인덱스)
                }

                strLog = $"Start GetSortIndex_SeeGene_Daejeon SpcNo: {strSpcNo}, RackNo: {strRackNo}" + "\r\n"; ;

                Common.File_Record("\r\n", false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                #region ----- 씨젠의료재단 대전 센터 SYSMEX TS-10 분류코드
                //2024-01-26 : 씨젠의료재단 대전 센터 진단검사의학과 혈액학부 SYSMEX TS-10 검체 분류 및 아카이브
                //TS-10 Sort No
                //"0", "LINE OUT"
                //"1", "보관"
                //"2", "Data Error" >>> PBS+RETI
                //"3", "EDTA Plasma"
                //"4", "서울"
                //"5", "서울+대전"
                //"6", "CBC+IH500"
                //"7", "CBC+HbA1c+ESR"
                //"8", "CBC+IH500+HbA1c"
                //"9", "미완료"
                //"10", "OTHER"
                //"11", "재검"
                //"12", "SLIDE" >>> 11
                //"12", "HbA1c + IH500, HbA1c + ESR, ESR + IH500
                //"13", "CBC+HbA1c"
                //"14", "CBC+ESR"
                //"15", "RET"
                //"61", "ERROR"
                #endregion ----- 씨젠의료재단 대전 센터 SYSMEX TS-10 분류코드

                #region ----- 1. 바코드에러 = 61

                //1. 바코드에러 
                if (Common.IsNumeric(strSpcNo) == false)
                {
                    strSortIndex = "61";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 바코드에러, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 1. 바코드에러 = 61

                #region ----- 2. 아카이브 모드
                if (chkArchive.Checked == true)
                {
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Archive Mode, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }
                #endregion ----- 2. 아카이브 모드

                #region ----- 3. 오더조회

                strLog += "JOB_GRUP" + TAB + "LIS_TST_CD" + TAB + "LIS_TST_SUB_CD" + TAB + "TST_NM" + TAB + "STUS_CD" + "\r\n";

                dsData = BizData.GetSpcInfo(strSpcNo);

                if (spcGbn == "5")
                {
                    filterExpression = "SPC_CD = 'A04' ";
                    if (dsData != null && dsData.Tables.Count > 0)
                    {
                        selectedRows = dsData.Tables[0].Select(filterExpression);

                        if (selectedRows != null && selectedRows.Length > 0)
                        {
                            //오더 조회되었으므로 정상 진행
                        }
                        else
                        {
                            //오더 조회 안되었으니 Data Error 로 분류
                            strSortIndex = "61";
                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}, 5번 검체이면서 Citrate(A04) 오더조회 안됨!" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            return strSortIndex;
                        }
                    }
                    else
                    {
                        //오더 조회 안되었으니 Data Error 로 분류
                        strSortIndex = "61";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}, 5번 검체이면서 Citrate(A04) 오더조회 안됨!" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }
                }

                //2025-02-17 : 무조건 데이터에러로 처리
                //              3610 추후송부
                //              3630 우선검사진행
                //              9070 검사제외
                filterExpression = "TST_STAT_CD IN ('3610', '3630', '9070') ";
                if (dsData != null && dsData.Tables.Count > 0)
                {
                    selectedRows = dsData.Tables[0].Select(filterExpression);

                    if (selectedRows != null && selectedRows.Length > 0)
                    {
                        foreach (DataRow drTemp in selectedRows)
                        {
                            if (drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                strSortIndex = "61";
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, Data Error, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }
                        }
                    }
                }

                if (dsData != null && dsData.Tables.Count > 0)
                {

                    blnCompleteChkSpcCd = true;
                    blnCompleteChkJobGrup = true;

                    filterExpression = $"SPC_CD IN ('A02','A04','A05') AND SPC_GBN = '{spcGbn}' AND TST_STAT_CD < '3060'";
                    selectedRows = dsData.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkSpcCd = false; }

                    filterExpression = $"SPC_CD = '' AND JOB_GRUP LIKE 'HE%' AND SPC_GBN = '{spcGbn}' AND TST_STAT_CD < '3060'";
                    selectedRows = dsData.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkJobGrup = false; }

                    //2024-12-17 : 9 번 검체구분자로 IM
                    if (spcGbn == "9")
                    {
                        //
                    }
                    else
                    {
                        if (blnCompleteChkJobGrup == true && blnCompleteChkSpcCd == true)
                        {
                            filterExpression = $"SPC_CD IN ('A02','A04','A05') AND SPC_GBN = '9' AND TST_STAT_CD < '3060'";
                            selectedRows = dsData.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkSpcCd = false; }

                            filterExpression = $"SPC_CD = '' AND JOB_GRUP LIKE 'HE%' AND SPC_GBN = '9' AND TST_STAT_CD < '3060'";
                            selectedRows = dsData.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0) { blnCompleteChkJobGrup = false; }
                        }
                    }

                    if (blnCompleteChkJobGrup == true && blnCompleteChkSpcCd == true)
                    {
                        blnComplete = true;
                    }

                    foreach (DataRow drTemp in dsData.Tables[0].Rows)
                    {
                        bool blnChk = false;

                        //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                        if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                        {
                            System.Diagnostics.Debug.Print(drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["SPC_CD"].ToString().Trim());

                            if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                if (spcGbn == "5" || spcGbn == "6")
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                    {
                                        blnChk = true;
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                    {
                                        blnChk = true;
                                    }
                                }
                            }
                            else
                            {
                                if (drTemp["SPC_CD"].ToString().Trim() == "")
                                {
                                    //if (aryTemp[i].IndexOf("RET") > -1)
                                    if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                    {
                                        if (spcGbn == "5" || spcGbn == "6")
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                        else
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (drTemp["JOB_GRUP"].ToString().Trim() == "GE4")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                }
                            }

                            if (blnChk == true)
                            {
                                if (string.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                {
                                    blnNoOrder = false;
                                }

                                strLisTstSubCd = drTemp["LIS_TST_SUB_CD"].ToString();
                                if (strLisTstSubCd == "-") { strLisTstSubCd = ""; }
                                strLisTstCds = drTemp["LIS_TST_CD"].ToString() + strLisTstSubCd;

                                //11310 = Reti count
                                //11017 = Eosinophil count (호산구수)
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE1") { blnExistOrderCDR = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE3" && strLisTstCds == "11310" && blnExistOrderCDR == false) { blnExistOrderCDR = true; }
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE9" && strLisTstCds == "11017" && blnExistOrderCDR == false) { blnExistOrderCDR = true; }

                                //blnPB
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE6") { blnPB = true; }

                                //blnRET
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "HE3") { blnRET = true; }

                                //2024-04-19 : 서울로 분류해야하는 기준 체크
                                if (drTemp["CNTR_CD"].ToString().Trim() == "13100005") { blnSeoul = true; }
                                if (drTemp["CNTR_CD"].ToString().Trim() == "14500000") { blnDJ = true; }

                                //2025-04-28 : CC5
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "CC5") { blnOther = true; }

                                if (drTemp["SPC_GBN"].ToString().Trim() == spcGbn && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                                {
                                    if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                    {
                                        lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                    }
                                }

                                strLog += drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["LIS_TST_CD"].ToString().Trim() + TAB + drTemp["LIS_TST_SUB_CD"].ToString().Trim() + TAB;
                                strLog += drTemp["TST_NM"].ToString().Trim() + TAB + drTemp["STUS_CD"].ToString().Trim() + "\r\n";
                            }
                        }
                    }
                }

                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                #endregion ----- 3. 오더조회

                #region ----- 4. 오더조회 안됨 = 2

                if (blnNoOrder == true)
                {
                    dsData = null;
                    strSortIndex = "9";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, No Order(Data Error), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 4. 오더조회 안됨 = 2

                #region ----- 5. CDR 오더가 있으나 XN에서 검사한 내역이 없을 경우 LINE OUT = 0

                dsRsltXN = BizData.GetRsltHematology(strSpcNo);
                if (dsRsltXN != null && dsRsltXN.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsRsltXN.Tables[0].Rows)
                    {
                        blnExistResultCDR = true;
                        break;
                    }
                }

                if (blnExistOrderCDR == true && blnExistResultCDR == false && blnSeoul == false)
                {
                    dsData = null;
                    dsRsltXN = null;
                    strSortIndex = "9";

                    if (blnRET == true) { strSortIndex = "15"; }
                    if (blnPB == true) { strSortIndex = "2"; }

                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, CDR 오더가 있으나 검사결과가 없을 경우, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 5. CDR 오더가 있으나 XN에서 검사한 내역이 없을 경우 LINE OUT

                #region ----- 5-1. 서울 = 4, 서울+대전 = 5 

                if (blnSeoul == true)
                {
                    dsData = null;
                    strSortIndex = "4";

                    if (blnDJ == true)
                    {
                        if (lstJobGroupCd.Count >= 2 && lstJobGroupCd.Contains("HE1") == true && blnExistResultCDR == true)
                        {
                            //pass
                        }
                        else
                        {
                            strSortIndex = "5";
                        }
                    }

                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 서울, 서울대전, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #endregion ----- 5-1. 서울 = 4, 서울+대전 = 5 

                #region ----- 6. 재검통과 랙인지 확인하기 (* XN검사결과 판정결과가 재검대상일 경우 계속 재검으로 분류되므로 특정 랙일 경우 체크 안함)

                if (Common.IsNumeric(strRackNo)) { strRackNo = Common.Val(strRackNo).ToString(); }
                if (glstNextStepRack.Contains(strRackNo)) { blnNextStep = true; }

                #endregion ----- 6. 재검통과 랙인지 확인하기 (* XN검사결과 판정결과가 재검대상일 경우 계속 재검으로 분류되므로 특정 랙일 경우 체크 안함)

                #region ----- 7. 기분류된 내역 유무 조회

                dsSorted = BizData.GetSpcPos(strSpcNo);

                if (dsSorted != null && dsSorted.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsSorted.Tables[0].Rows)
                    {
                        if (string.IsNullOrEmpty(drTemp["tsGrupNo"].ToString()) == false)
                        {
                            //2025-03-16 : 기분류 시 아카이브
                            if (drTemp["tsGrupNo"].ToString().Trim() == "2" || drTemp["tsGrupNo"].ToString().Trim() == "6")
                            {
                                dsData = null;
                                strSortIndex = "1";
                                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 기분류일 경우 보관처리, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                return strSortIndex;
                            }
                        }
                    }
                }

                if (dsSorted != null && dsSorted.Tables.Count > 0)
                {
                    filterExpression = "tsGrupNo = " + Common.STS("11");
                    selectedRows = dsSorted.Tables[0].Select(filterExpression);
                    if (selectedRows != null && selectedRows.Length > 0) { blnApplicator = true; blnSlide = true; blnRerunFirst = true; blnNextStep = true; }

                    DataView view = dsSorted.Tables[0].DefaultView;
                    view.Sort = "regDtm DESC";

                    // Create a new DataTable from the sorted view
                    DataTable sortedTable = view.ToTable();

                    //foreach (DataRow drTemp in dsSorted.Tables[0].Rows)
                    foreach (DataRow drTemp in sortedTable.Rows)
                    {
                        if (string.IsNullOrEmpty(drTemp["tsGrupNo"].ToString()) == false)
                        {
                            if (drTemp["tsGrupNo"].ToString().Trim() == "11") { blnApplicator = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "11") { blnSlide = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "11") { blnRerunFirst = true; blnNextStep = true; }
                            if (drTemp["tsGrupNo"].ToString().Trim() == "11") { blnRerunSecond = true; blnNextStep = true; }

                            if (drTemp["tsGrupNo"].ToString().Trim() != "11" && drTemp["tsGrupNo"].ToString().Trim() != "11" &&
                                drTemp["tsGrupNo"].ToString().Trim() != "2" && drTemp["tsGrupNo"].ToString().Trim() != "9" &&
                                drTemp["tsGrupNo"].ToString().Trim() != "0" && drTemp["tsGrupNo"].ToString().Trim() != "1")
                            {
                                blnSorted = true;
                            }
                            break;
                        }
                    }
                }

                #endregion ----- 7. 기분류된 내역 유무 조회

                if (blnPB == true)
                {
                    dsData = null;
                    strSortIndex = "2";

                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, PB, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                #region ----- 8. CDR 오더가 있으나 XN에서 검사한 내역이 있을 경우 (떠보기 = 12, 슬라이드 = 12, 1차재검 = 11, 2차재검 = 11) 체크 && 특정 랙이 아닐 경우

                if (blnExistResultCDR == true && blnNextStep == false)
                {
                    //rtstJgmtVal, apctrJgmtVal, slid1JgmtVal, slid2JgmtVal
                    if (dsRsltXN != null && dsRsltXN.Tables.Count > 0)
                    {
                        //재검, 슬라이드, 떠보기 순으로 우선순위임!                    
                        filterExpression = "rtstJgmtVal = " + Common.STS("R");

                        selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                        if (selectedRows != null && selectedRows.Length > 0)
                        {
                            foreach (DataRow row in selectedRows)
                            {
                                strSortIndex = "11";
                                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, 재검판정: {row["rtstJgmtVal"]}";
                                break;
                            }
                        }

                        if (blnSlide == false && strSortIndex == "")
                        {
                            filterExpression = "slid1JgmtVal in ('S1', 'S2')";

                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                foreach (DataRow row in selectedRows)
                                {
                                    strSortIndex = "11";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, S1: {row["slid1JgmtVal"]}, S2: {row["slid2JgmtVal"]}";
                                    break;
                                }
                            }
                        }

                        if (blnApplicator == false && strSortIndex == "")
                        {
                            filterExpression = "apctrJgmtVal = 'A'";

                            selectedRows = dsRsltXN.Tables[0].Select(filterExpression);
                            if (selectedRows != null && selectedRows.Length > 0)
                            {
                                foreach (DataRow row in selectedRows)
                                {
                                    strSortIndex = "11";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 검사코드: {row["devcRsltChnl"]}, 검사결과: {row["devcRsltVal"]}, 떠보기판정: {row["apctrJgmtVal"]}";
                                    break;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(strSortIndex) == false)
                        {
                            dsData = null;
                            dsRsltXN = null;
                            dsSorted = null;

                            if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                            strLog += $", 분류명: {strSortDesc}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                            return strSortIndex;
                        }
                    }
                }

                #endregion ----- 8. CDR 오더가 있으나 XN에서 검사한 내역이 있을 경우 (떠보기 = 18, 슬라이드 = 19, 1차재검 = 20, 2차재검 = 21) 체크 && 특정 랙이 아닐 경우

                #region ----- 9.0. 검사완료 검체일 경우 보관 = 1
                if (blnComplete == true)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(검사완료), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }
                #endregion ----- 9.0. 검사완료 검체일 경우 보관 = 1

                #region ----- 9. 기분류된 검체일 경우 보관 = 1

                //if (blnSorted == true)
                //{
                //    dsData = null;
                //    dsRsltXN = null;
                //    dsSorted = null;
                //    strSortIndex = "1";
                //    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                //    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(기분류), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                //    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                //    return strSortIndex;
                //}

                #endregion ----- 9. 기분류된 검체일 경우 보관 = 1

                #region ----- 10. 오더에 따른 분류
                if (dsData != null && dsData.Tables.Count > 0)
                {
                    foreach (DataRow drTemp in dsData.Tables[0].Rows)
                    {

                        bool blnChk = false;

                        //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                        if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                        {
                            System.Diagnostics.Debug.Print(drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["SPC_CD"].ToString().Trim());

                            if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                            {
                                if (spcGbn == "5")
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                    {
                                        blnChk = true;
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                    {
                                        blnChk = true;
                                    }
                                }
                            }
                            else
                            {
                                if (drTemp["SPC_CD"].ToString().Trim() == "")
                                {
                                    if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                    {
                                        if (spcGbn == "5")
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                        else
                                        {
                                            if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (drTemp["JOB_GRUP"].ToString().Trim() == "GE4")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                }
                            }

                            if (blnChk == true)
                            {
                                if (drTemp["JOB_GRUP"].ToString().Trim() == "IMH" || drTemp["JOB_GRUP"].ToString().Trim() == "CC5")
                                {
                                    //EDTA Plasma
                                    strSortIndex = "3";
                                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: {drTemp["JOB_GRUP"]}, 분류코드: {strSortIndex}";
                                    break;
                                }

                                if (drTemp["SPC_GBN"].ToString().Trim() == spcGbn && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                                {
                                    if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == false)
                                    {
                                        lstJobGroupCd.Add(drTemp["JOB_GRUP"].ToString().Trim());
                                    }
                                }
                            }
                        }
                    }

                    //2025-03-16 : EDTA Plasma 분류 전 검사 완료 체크하기
                    if (strSortIndex == "3" && blnComplete == false)
                    {
                        strSortIndex = "";
                    }

                    if (string.IsNullOrEmpty(strSortIndex) == false)
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;

                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog += $", 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                        return strSortIndex;
                    }

                    if (blnExistResultCDR && lstJobGroupCd.Contains("HE1") == true)
                    {
                        lstJobGroupCd.Remove("HE1");
                    }

                    //2025-04-29 : 상태값으로 분류조건 제외하기
                    if (dsData != null && dsData.Tables.Count > 0)
                    {
                        foreach (DataRow drTemp in dsData.Tables[0].Rows)
                        {
                            bool blnChk = false;

                            //2025-01-06 : 이송현 대리 요청으로 타ID는 분류조건에서 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                            if (string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                            {
                                System.Diagnostics.Debug.Print(drTemp["JOB_GRUP"].ToString().Trim() + TAB + drTemp["SPC_CD"].ToString().Trim());

                                if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A04" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                {
                                    if (spcGbn == "5")
                                    {
                                        if (drTemp["SPC_CD"].ToString().Trim() == "A04")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                    else
                                    {
                                        if (drTemp["SPC_CD"].ToString().Trim() == "A02" || drTemp["SPC_CD"].ToString().Trim() == "A05")
                                        {
                                            blnChk = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (drTemp["SPC_CD"].ToString().Trim() == "")
                                    {
                                        if (drTemp["JOB_GRUP"].ToString().Trim().IndexOf("HE") > -1)
                                        {
                                            if (spcGbn == "5")
                                            {
                                                if (drTemp["JOB_GRUP"].ToString() == "HE2" || drTemp["JOB_GRUP"].ToString() == "HE4" || drTemp["JOB_GRUP"].ToString() == "HE5")
                                                {
                                                    blnChk = true;
                                                }
                                            }
                                            else
                                            {
                                                if (drTemp["JOB_GRUP"].ToString() != "HE2" && drTemp["JOB_GRUP"].ToString() != "HE4" && drTemp["JOB_GRUP"].ToString() != "HE5")
                                                {
                                                    blnChk = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (drTemp["JOB_GRUP"].ToString().Trim() == "GE4")
                                            {
                                                blnChk = true;
                                            }
                                        }
                                    }
                                }

                                if (blnChk == true)
                                {
                                    if (drTemp["JOB_GRUP"].ToString().Trim() == "IMH" || drTemp["JOB_GRUP"].ToString().Trim() == "CC5")
                                    {
                                        //
                                    }

                                    if (drTemp["SPC_GBN"].ToString().Trim() == spcGbn && Common.Val(drTemp["TST_STAT_CD"].ToString().Trim()) < 3060)
                                    {

                                    }
                                    else
                                    {
                                        if (String.IsNullOrEmpty(drTemp["JOB_GRUP"].ToString().Trim()) == false && lstJobGroupCd.Contains(drTemp["JOB_GRUP"].ToString().Trim()) == true)
                                        {
                                            lstJobGroupCd.Remove(drTemp["JOB_GRUP"].ToString().Trim());
                                            blnTestCompleted = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (lstJobGroupCd.Count > 0)
                    {
                        foreach (string job in lstJobGroupCd)
                        {
                            strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, JobGroup: {job}" + "\r\n";
                            Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        }
                    }

                    if (lstJobGroupCd.Count == 0 && blnTestCompleted == true)
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;
                        strSortIndex = "1";
                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관 검사완료, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return strSortIndex;
                    }

                    if (lstJobGroupCd.Count == 1)
                    {
                        if (lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "14"; }
                        if (lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "6"; }
                        if (lstJobGroupCd.Contains("HE7") == true) { strSortIndex = "6"; }
                        if (lstJobGroupCd.Contains("PA7") == true) { strSortIndex = "6"; }
                        if (lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "13"; }
                        if (lstJobGroupCd.Contains("IMH") == true) { strSortIndex = "3"; }
                        if (lstJobGroupCd.Contains("CC5") == true) { strSortIndex = "3"; }

                        if (lstJobGroupCd.Contains("HE2") == true) { strSortIndex = "3"; }
                        if (lstJobGroupCd.Contains("HE4") == true) { strSortIndex = "3"; }
                        if (lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "3"; }
                    }
                    else if (lstJobGroupCd.Count == 2)
                    {
                        if (lstJobGroupCd.Contains("HEB") == true && lstJobGroupCd.Contains("HE6") == true) { strSortIndex = "2"; }
                        if (lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "2"; }
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true) { strSortIndex = "14"; }
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE3") == true) { strSortIndex = "14"; }
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE9") == true) { strSortIndex = "14"; }
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "13"; }
                        if (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "13"; }
                        if (lstJobGroupCd.Contains("HE9") == true && lstJobGroupCd.Contains("HEB") == true) { strSortIndex = "13"; }
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "10"; }
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE7") == true) { strSortIndex = "10"; }
                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "6"; }
                        if (lstJobGroupCd.Contains("IMH") == true && lstJobGroupCd.Contains("CC5") == true) { strSortIndex = "3"; }
                        if (lstJobGroupCd.Contains("HE2") == true && lstJobGroupCd.Contains("HE4") == true) { strSortIndex = "3"; }
                        if (lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "3"; }
                        if (lstJobGroupCd.Contains("HE5") == true && lstJobGroupCd.Contains("HE2") == true) { strSortIndex = "3"; }

                        //"12", "HbA1c + IH500, HbA1c + ESR, ESR + IH500
                        if (lstJobGroupCd.Contains("HEB") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "12"; }
                        if (lstJobGroupCd.Contains("HEB") == true && lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "12"; }
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "12"; }

                        if (lstJobGroupCd.Contains("HEB") == true && string.IsNullOrEmpty(strSortIndex) == true) { strSortIndex = "13"; }

                    }
                    else
                    {
                        if (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "2"; }
                        if (lstJobGroupCd.Contains("HE3") == true && lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE0") == true) { strSortIndex = "2"; }
                        if (lstJobGroupCd.Contains("HE6") == true && lstJobGroupCd.Contains("HE7") == true && lstJobGroupCd.Contains("HE8") == true) { strSortIndex = "2"; }
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE3") == true) { strSortIndex = "14"; }
                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE9") == true) { strSortIndex = "14"; }

                        if (lstJobGroupCd.Contains("HE2") == true && lstJobGroupCd.Contains("HE4") == true) { strSortIndex = "3"; }
                        if (lstJobGroupCd.Contains("HE4") == true && lstJobGroupCd.Contains("HE5") == true) { strSortIndex = "3"; }
                        if (lstJobGroupCd.Contains("HE5") == true && lstJobGroupCd.Contains("HE2") == true) { strSortIndex = "3"; }

                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HEB") == true && string.IsNullOrEmpty(strSortIndex))
                        {
                            strSortIndex = "13";
                        }

                        if (lstJobGroupCd.Contains("HE0") == true && lstJobGroupCd.Contains("HEB") == true && string.IsNullOrEmpty(strSortIndex))
                        {
                            strSortIndex = "7";
                        }

                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE7") == true && string.IsNullOrEmpty(strSortIndex))
                        {
                            strSortIndex = "6";
                        }

                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("HE8") == true && string.IsNullOrEmpty(strSortIndex))
                        {
                            strSortIndex = "6";
                        }

                        if (lstJobGroupCd.Contains("HE1") == true && lstJobGroupCd.Contains("PA7") == true && string.IsNullOrEmpty(strSortIndex))
                        {
                            strSortIndex = "6";
                        }

                        if (lstJobGroupCd.Count >= 3)
                        {
                            strSortIndex = "10";
                        }
                    }

                    if (string.IsNullOrEmpty(strSortIndex) == false)
                    {
                        dsData = null;
                        dsRsltXN = null;
                        dsSorted = null;

                        if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                        strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                        Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                        return strSortIndex;
                    }
                }

                #endregion ----- 10. 오더에 따른 분류

                #region ----- 11. 분류조건을 확인할 수 없음

                if (blnExistOrderCDR == true && blnExistResultCDR == true)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "1";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 보관(XN검사완료), 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                if (blnOther == true)
                {
                    dsData = null;
                    dsRsltXN = null;
                    dsSorted = null;
                    strSortIndex = "3";
                    if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                    strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 타학부, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                    Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return strSortIndex;
                }

                dsData = null;
                dsRsltXN = null;
                dsSorted = null;
                strSortIndex = "9";
                if (_dctSortInfo.ContainsKey(strSortIndex) == true) { strSortDesc = _dctSortInfo[strSortIndex]; }
                strLog = $"SpcNo: {strSpcNo}, RackNo: {strRackNo}, 분류조건을 확인할 수 없음, 분류코드: {strSortIndex}, 분류명: {strSortDesc}" + "\r\n";
                Common.File_Record(strLog, false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                return strSortIndex;

                #endregion ----- 11. 분류조건을 확인할 수 없음

            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "GetSortIndex" + TAB + strSpcNo + TAB + ex.ToString() + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }

            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "GetSortIndex" + TAB + strSpcNo + TAB + strSortIndex + "\r\n",
                              false,
                              mstrAppPath + "log\\",
                              DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");


            return strSortIndex;
        }

        static string AddTwoSeconds(string inputTime)
        {
            if (DateTime.TryParseExact(inputTime, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime inputDateTime))
            {
                DateTime newDateTime = inputDateTime.AddSeconds(2);
                return newDateTime.ToString("yyyyMMddHHmmss");
            }
            else
            {
                return "Invalid input time format.";
            }
        }

        static string AddThreeSeconds(string inputTime)
        {
            if (DateTime.TryParseExact(inputTime, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime inputDateTime))
            {
                DateTime newDateTime = inputDateTime.AddSeconds(3);
                return newDateTime.ToString("yyyyMMddHHmmss");
            }
            else
            {
                return "Invalid input time format.";
            }
        }

        private void GetEqpCd()
        {
            string strPath = mstrAppPath;
            string strFile = @"\EqpCd.ini";
            string strTemp;

            try
            {
                StreamReader objReader = new StreamReader(strPath + strFile, Encoding.Default);
                while ((strTemp = objReader.ReadLine()) != null)
                {
                    if (strTemp != "")
                    {
                        SG_TS_EQP_CD = strTemp;
                        break;
                    }
                }
                objReader.Close();
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"GetEqpCd Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }
        }

        public void WriteDataSetToCSV(DataSet dataSet, string filePath)
        {
            try
            {

                // Ensure the directory exists
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                StringBuilder csvContent = new StringBuilder();

                foreach (DataTable table in dataSet.Tables)
                {
                    // Write the table name as a header
                    csvContent.AppendLine(table.TableName);

                    // Write the column headers
                    string[] columnNames = new string[table.Columns.Count];
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        columnNames[i] = table.Columns[i].ColumnName;
                    }
                    csvContent.AppendLine(string.Join(TAB, columnNames));

                    // Write the rows
                    foreach (DataRow row in table.Rows)
                    {
                        string[] fields = new string[table.Columns.Count];
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            fields[i] = row[i].ToString();
                        }
                        csvContent.AppendLine(string.Join(TAB, fields));
                    }

                    // Add a blank line to separate tables
                    csvContent.AppendLine();
                }

                File.WriteAllText(filePath + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv", csvContent.ToString());
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"WriteDataSetToCSV Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            string directoryPath = @"D:\___AS\CT90 대구씨젠 20240220/"; // 읽고자 하는 디렉토리 경로

            directoryPath = @"D:\___AS\CT90 테스트용 로그/";
            //directoryPath = @"D:\___AS\CT90 대전씨젠 20240523/";

            try
            {
                // 해당 경로의 모든 파일을 가져옴
                //string[] files = Directory.GetFiles(directoryPath, "*.log");

                var sortedFiles = Directory.GetFiles(directoryPath, "*.log")
                           .OrderBy(Path.GetFileName)
                           .ToList();

                foreach (string file in sortedFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string timePart = fileName.Substring(9, 2); // 파일 이름에서 시간 부분 추출 (9번째 인덱스부터 2글자)
                    string etc = fileName.Substring(11);

                    if (etc == ".log" && int.TryParse(timePart, out int hour) && hour >= 0 && hour <= 23)
                    {
                        Console.WriteLine($"Reading file: {file}");
                        string content = File.ReadAllText(file);
                        Console.WriteLine(content);
                        content = content.Replace(ACK, "");
                        int intPos;
                        string[] aryData = content.Split((char)4);

                        for (int i = 0; i < aryData.Length; i++)
                        {
                            aryData[i] = aryData[i].Replace(ENQ, "");

                            intPos = aryData[i].IndexOf("EXCHANGE");
                            if (intPos > -1)
                            {
                                Data_Analyzer(aryData[i] + EOT);
                                mstrData = "";
                            }

                            intPos = aryData[i].IndexOf("STORE-F");

                            if (intPos > -1)
                            {
                                Data_Analyzer(aryData[i] + EOT);
                                mstrData = "";

                                Thread.Sleep(100);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading files: {ex.Message}");
            }

            //insertSampleTray(1, 1, 10001, "1001");
            //insertSampleTray(2, 1, 10002, "1002");
            //insertSampleTray(1, 2, 10001, "1003");
            //insertSampleTray(1, 3, 10001, "1003");
            //insertSampleTray(3, 1, 10003, "1002");

            //insertSampleTray(4, 1, 10001, "4001");
            //insertSampleTray(4, 2, 10001, "4002");
            //insertSampleTray(3, 2, 10003, "4003");
            //insertSampleTray(4, 3, 10001, "4004");
            //insertSampleTray(3, 3, 10003, "4005");

            //BizData.SetOrderInfo(txtBarNo.Text, "B", "1", "1");

            //DataSet dsRsltHIS;
            //dsRsltHIS = BizData.GetRslt(txtBarNo.Text);
            //if (dsRsltHIS != null && dsRsltHIS.Tables.Count > 0)
            //{
            //    string filterExpression;
            //    DataRow[] selectedRows;

            //    filterExpression = "devcRsltChnl IN ('WBC','RBC')";

            //    selectedRows = dsRsltHIS.Tables[0].Select(filterExpression);
            //    if (selectedRows != null && selectedRows.Length > 0)
            //    {
            //        foreach (DataRow row in selectedRows)
            //        {
            //            //lstJobGroupCd.Add("HE1");
            //            break;
            //        }
            //    }
            //}
        }

        private void insertSampleTray(int trayNumber, int holeNumber, int rackNumber, string sampleId)
        {
            string connectionString = "Data Source=interface.db;Version=3;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                if (holeNumber > 125)
                {
                    return;
                }

                int traceSequenceNumber = GetOrIncrementTraceSequenceNumber(connection, rackNumber);
                string insertDate = DateTime.Now.ToString("yyyyMMddHHmmss");

                string insertDataQuery = @"
                    INSERT INTO SampleTray (TraceSequenceNumber, TrayNumber, RackNumber, HoleNumber, SampleNumber, InsertDate)
                    VALUES (@TraceSequenceNumber, @TrayNumber, @RackNumber, @HoleNumber, @SampleNumber, @InsertDate);
                ";

                using (SQLiteCommand command = new SQLiteCommand(insertDataQuery, connection))
                {
                    command.Parameters.AddWithValue("@TraceSequenceNumber", traceSequenceNumber);
                    command.Parameters.AddWithValue("@TrayNumber", trayNumber);
                    command.Parameters.AddWithValue("@RackNumber", rackNumber);
                    command.Parameters.AddWithValue("@HoleNumber", holeNumber);
                    command.Parameters.AddWithValue("@SampleNumber", sampleId);  // 예시값
                    command.Parameters.AddWithValue("@InsertDate", insertDate);

                    command.ExecuteNonQuery();
                    Console.WriteLine("Data inserted successfully.");
                }

                connection.Close();
            }
        }

        // 새로운 RackNumber가 추가될 경우 TraceSequenceNumber 증가
        static int GetOrIncrementTraceSequenceNumber(SQLiteConnection connection, int rackNumber)
        {
            // RackNumber가 이미 존재하는지 확인
            string checkRackQuery = @"
                SELECT COUNT(*) FROM SampleTray
                WHERE RackNumber = @RackNumber;
            ";
            using (SQLiteCommand checkRackCommand = new SQLiteCommand(checkRackQuery, connection))
            {
                checkRackCommand.Parameters.AddWithValue("@RackNumber", rackNumber);
                int rackCount = Convert.ToInt32(checkRackCommand.ExecuteScalar());

                // 새 RackNumber가 입력된 경우 TraceSequenceNumber 증가
                if (rackCount == 0)
                {
                    string maxSequenceQuery = "SELECT COALESCE(MAX(TraceSequenceNumber), 0) FROM SampleTray";
                    using (SQLiteCommand maxCommand = new SQLiteCommand(maxSequenceQuery, connection))
                    {
                        int currentMax = Convert.ToInt32(maxCommand.ExecuteScalar());
                        return (currentMax % 999) + 1; // 1부터 999까지 순환
                    }
                }
                else
                {
                    // 기존 TraceSequenceNumber 반환
                    string currentSequenceQuery = $"SELECT COALESCE(MAX(TraceSequenceNumber), 1) FROM SampleTray WHERE RackNumber = {rackNumber}";
                    using (SQLiteCommand currentCommand = new SQLiteCommand(currentSequenceQuery, connection))
                    {
                        return Convert.ToInt32(currentCommand.ExecuteScalar());
                    }
                }
            }
        }

        private void Data_Analyzer_BeforeData(string strData)
        {
            try
            {
                long intPos;

                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "이전 수신데이터 분석 및 처리 Start" + CR + LF + strData + "\r\n",
                    false,
                    mstrAppPath + "log\\",
                    DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                intPos = strData.IndexOf(EOT);

                if (intPos > -1)
                {
                    string strType = "";
                    string strRackNo = "", strRackPos = "", strSpcNo = "";
                    string[] aryRackInfo;
                    string[] aryData = strData.Split((char)2);

                    // 2017-11-28
                    // CT-90 Ver2.0 적용
                    //When TS-10 is not connected
                    //Q|1|123456^01^1234567890123456789012^B||||20010905150000||||B||<CR>
                    //When TS-10 is connected
                    //Q|1|123456^01^1234567890123456789012^B||||20010905150000||||C|1^1|<CR>
                    string strAnalysisParameterID = "";
                    string strInquiryType = "";
                    string strTS10Info = "";
                    string strDataOfMeasurementValue = "";
                    string strTrayNo = "";
                    string strTrayBarNo = "";
                    string strSortIdx = "";
                    string strDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string strDtm = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string strReulstList = "";
                    string strUnitNo = "";
                    string strTmpRackPos = "";
                    string strTmpSpcNo = "";
                    string strTmpBgwKey = "";
                    string filterExpression;
                    DataRow[] selectedRows;

                    // 2017-12-12
                    // CT-90 Ver2.0 + TS-10 연동 시 요청들어온대로 리턴해야지 에러안남!!!
                    // B : inquiry of BT - 최초 랙 장착 시 오더요청
                    // C : inquiry at other than BT, TS-10 (e.g. CVR) - 라인 지나갈 때
                    // SI : inquiry at TS-10's right arrival position of carry-in line. - 랙이 TS-10 에 들어갈 때
                    // SO : inquiry at TS-10's left arrival position of carry-in line. - 랙이 TS-10 에서 나올 때

                    //2Q|1|000009^01^          171211551208^B||||20171211230301||||C^06|1^1|
                    // TS-10 No. ^ Work Cycle : 1byte^1byte

                    string[] aryMsg = strData.Split((char)4);

                    for (int k = 0; k < aryMsg.Length; k++)
                    {

                    }

                    for (int i = 0; i < aryData.Length; i++)
                    {
                        if (aryData[i].Trim() != "")
                        {
                            string[] aryTemp = aryData[i].Split('|');
                            switch (aryTemp[0].Substring(1))
                            {
                                case "Q":
                                    strType = "order";
                                    if (aryTemp[2] != "")
                                    {
                                        strInquiryType = Common.P(aryTemp[10], "^", 1);
                                        strUnitNo = Common.P(aryTemp[10], "^", 2);
                                        strTS10Info = aryTemp[11];

                                        string[] aryOrdInfo = aryTemp[2].Split('\\');
                                        for (int j = 0; j < aryOrdInfo.Length; j++)
                                        {
                                            if (aryOrdInfo[j] != "")
                                            {
                                                aryRackInfo = aryOrdInfo[j].Split('^');
                                                strRackNo = aryRackInfo[0].Trim();
                                                strTmpRackPos = aryRackInfo[1].Trim();
                                                strTmpSpcNo = aryRackInfo[2].Trim();

                                                if (strRackPos == "")
                                                {
                                                    strRackPos = strTmpRackPos;
                                                }
                                                else
                                                {
                                                    strRackPos += "^" + strTmpRackPos;
                                                }
                                                if (strSpcNo == "")
                                                {
                                                    strSpcNo = strTmpSpcNo;
                                                }
                                                else
                                                {
                                                    strSpcNo += "^" + strTmpSpcNo;
                                                }

                                                //2024-06-04 : BT, SI 라인일 경우 도착처리 호출
                                                if (strInquiryType == "B" || strInquiryType == "SI")
                                                {
                                                    //2025-02-21 : 검체번호의 마지막 시퀀스 번호가 3,5번일때만 도착처리하도록 수정
                                                    if (Common.IsNumeric(strTmpSpcNo))
                                                    {
                                                        // 마지막 문자가 3 또는 5인지 확인
                                                        string lastDigit = strTmpSpcNo.Substring(strTmpSpcNo.Length - 1);
                                                        if (lastDigit == "3" || lastDigit == "5")
                                                        {
                                                            // 여기에 조건을 만족할 때의 로직을 작성
                                                            Lis.Interface.clsParameterCollection Param = new Lis.Interface.clsParameterCollection();

                                                            string strEqpTstDt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                            Param.Items.Add("barNo", strTmpSpcNo);
                                                            Param.Items.Add("devcCd", SG_TS_EQP_CD);
                                                            Param.Items.Add("rsltPrgsCd", "AT");
                                                            Param.Items.Add("accDtm", strEqpTstDt);
                                                            Param.Items.Add("tlaSeqNum", "1");
                                                            Param.Items.Add("trackComm", "TS10");
                                                            Param.Items.Add("mdulCd", "TS10");

                                                            //TS진행상태(Hitachi AQM:Q, RFM:S,OBS:A/Roche archive:A,seen:S/Sysmex 분류:S,archive:A)
                                                            Param.Items.Add("tsGbn", "S");
                                                            Param.Items.Add("rackNo", strRackNo);
                                                            Param.Items.Add("holeNo", strTmpRackPos);
                                                            Param.Items.Add("trayNo", "");
                                                            Param.Items.Add("traySeq", "");
                                                            Param.Items.Add("trayHoleNo", "");
                                                            Param.Items.Add("tsGrpNo", "");
                                                            Param.Items.Add("tsGrpNm", "");
                                                            Param.Items.Add("tsErr", "");

                                                            if (DEV_IN_OFFICE == false && DEBUG_MODE == false)
                                                            {
                                                                Lis.Interface.clsBizSeeGene objApi;
                                                                objApi = new Lis.Interface.clsBizSeeGene();
                                                                string strRtn = objApi.SaveSampleTracking(Param);
                                                                Param = null;
                                                                objApi = null;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        //2024-03-20 : Rack 번호별 장비검사일시 딕셔너리에 저장
                                        if (_dctEqpTestDtTmByRack.ContainsKey(strRackNo) == false)
                                        {
                                            _dctEqpTestDtTmByRack.Add(strRackNo, aryTemp[6]);
                                        }
                                    }

                                    break;

                                case "O":
                                    strType = "result";

                                    if (aryTemp[3] != "")
                                    {
                                        aryRackInfo = aryTemp[3].Split('^');
                                        strRackNo = aryRackInfo[0].Trim();
                                        strTmpRackPos = aryRackInfo[1].Trim();
                                        strTmpSpcNo = aryRackInfo[2].Trim();

                                        if (string.IsNullOrEmpty(strRackPos))
                                        {
                                            strRackPos = strTmpRackPos;
                                        }
                                        else
                                        {
                                            strRackPos += "^" + strTmpRackPos;
                                        }

                                        if (string.IsNullOrEmpty(strSpcNo))
                                        {
                                            strSpcNo = strTmpSpcNo;
                                        }
                                        else
                                        {
                                            strSpcNo += "^" + strTmpSpcNo;
                                        }
                                    }

                                    break;

                                case "R":
                                    strAnalysisParameterID = aryTemp[2];
                                    strAnalysisParameterID = Common.P(strAnalysisParameterID, "^", 5);

                                    strDataOfMeasurementValue = aryTemp[3];

                                    if (aryTemp[3] != "")
                                    {
                                        if (strReulstList == "")
                                        {
                                            strReulstList = strDataOfMeasurementValue.Trim();
                                        }
                                        else
                                        {
                                            strReulstList += "|" + strDataOfMeasurementValue.Trim();
                                        }
                                    }

                                    break;

                                default:
                                    break;
                            }
                        }
                    }

                    if (strRackNo != "" && strRackPos != "" && strSpcNo != "")
                    {
                        // 처방 요청
                        if (strType == "order")
                        {
                            bool blnTemp = false;

                            // 처음 요청인지 판단
                            filterExpression = "rackNo = " + Common.STS(strRackNo);
                            selectedRows = gdtInquirySpcNoList.Select(filterExpression);

                            // Display the selected data
                            foreach (DataRow row in selectedRows)
                            {
                                Common.File_Record(TAB + $"최초요청 rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}" + "\r\n",
                                                   false,
                                                   mstrAppPath + "log\\",
                                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                if (strInquiryType == "B")
                                {
                                    row.Delete();
                                    Common.File_Record(TAB + "RackNo 중복으로 데이터 삭제 후 처리" + "\r\n",
                                        false,
                                        mstrAppPath + "log\\",
                                        DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                }
                                else
                                {
                                    blnTemp = true;
                                }
                            }

                            if (!blnTemp)
                            {
                                string strOrdInfo;
                                if (BASE_ORD == "Y")
                                {
                                    strOrdInfo = "CBC^DIFF^SP";
                                }
                                else
                                {
                                    strOrdInfo = "-";
                                }

                                string[] aryRackPos = strRackPos.Split('^');
                                string[] arySpcNo = strSpcNo.Split('^');
                                strDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                                for (int i = 0; i < aryRackPos.Length; i++)
                                {
                                    DataRow row1 = gdtInquirySpcNoList.NewRow();
                                    row1["rackNo"] = strRackNo;
                                    row1["rackPos"] = aryRackPos[i];
                                    row1["spcNo"] = arySpcNo[i];
                                    row1["ordInfo"] = strOrdInfo;
                                    row1["inquiryType"] = strInquiryType;
                                    row1["unitNo"] = strUnitNo;
                                    row1["sortInfo"] = strTS10Info;
                                    row1["inputDtTm"] = strDateTime;
                                    gdtInquirySpcNoList.Rows.Add(row1);
                                }

                            }
                            else
                            {
                                filterExpression = "rackNo = " + Common.STS(strRackNo);
                                selectedRows = gdtInquirySpcNoList.Select(filterExpression);

                                // Display the selected data
                                foreach (DataRow row in selectedRows)
                                {
                                    Common.File_Record(TAB + $"처방요청 rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}" + "\r\n",
                                        false,
                                        mstrAppPath + "log\\",
                                        DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                    row["ordInfo"] = "Y";
                                }
                            }
                        }

                        // 결과 완료
                        if (strType == "result")
                        {
                            string strAddRow = "";

                            switch (strAnalysisParameterID)
                            {
                                case "FINAL":

                                    if (_dicOrdTstCdAtStart.ContainsKey(strSpcNo))
                                    {
                                        _dicOrdTstCdAtStart.Remove(strSpcNo);
                                    }

                                    strTmpBgwKey = DateTime.Now.ToString("yyyyMMddHHmmss.ffff");

                                    Common.File_Record(TAB +
                                                       $"{strAnalysisParameterID} _dctSetRackInfo key: {strTmpBgwKey}, spcNo: {strSpcNo}, rackNo: {strRackNo}, rackPos: {strRackPos}" + "\r\n",
                                                       false,
                                                       mstrAppPath + "log\\",
                                                       DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                    if (_dctSetRackInfo.ContainsKey(strTmpBgwKey) == false)
                                    {
                                        _dctSetRackInfo.Add(strTmpBgwKey, strRackNo + TAB + strRackPos + TAB + strSpcNo + TAB + "" + TAB + "" + TAB + "" + TAB + "" + TAB + "" + TAB + "");
                                    }
                                    else
                                    {
                                        Common.File_Record(TAB +
                                                            $"{strAnalysisParameterID} _dctSetRackInfo key Exist: {strTmpBgwKey}, spcNo: {strSpcNo}, rackNo: {strRackNo}, rackPos: {strRackPos}" + "\r\n",
                                                            false,
                                                            mstrAppPath + "log\\",
                                                            DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                    }

                                    string[] aryRackPos = strRackPos.Split('^');
                                    string[] arySpcNo = strSpcNo.Split('^');
                                    string[] aryResult = strReulstList.Split('|');

                                    for (int i = 0; i < aryRackPos.Length; i++)
                                    {
                                        strAddRow = "Result" + TAB;
                                        strAddRow = strAddRow + strDtm + TAB;
                                        strAddRow = strAddRow + "ST" + TAB;
                                        strAddRow = strAddRow + arySpcNo[i] + TAB;
                                        strAddRow = strAddRow + strRackNo + TAB;
                                        strAddRow = strAddRow + aryRackPos[i] + TAB;

                                        //00^1858^OK^NG^OK
                                        strAddRow = strAddRow +
                                                    "XN: " + Common.P(aryResult[i], "^", 3) + ", " +
                                                    "SP: " + Common.P(aryResult[i], "^", 4) + TAB;

                                        GrdRowAdd(strAddRow);
                                    }

                                    break;

                                case "STORE-F":

                                    if (_dctRetiSpcNo.ContainsKey(strSpcNo) == true)
                                    {
                                        _dctRetiSpcNo.Remove(strSpcNo);
                                    }

                                    string strGubun = "A";
                                    string strTrayPos = "";
                                    string traySeqNo = "";

                                    // 2020-06-11 : sort index 저장하기
                                    strSortIdx = Common.P(strDataOfMeasurementValue, "^", 1);
                                    strTrayNo = Common.P(strDataOfMeasurementValue, "^", 2);
                                    strTrayBarNo = Common.P(strDataOfMeasurementValue, "^", 4);
                                    strTrayPos = Common.P(strDataOfMeasurementValue, "^", 5);

                                    if (strTrayBarNo == "") { strGubun = "S"; strTrayBarNo = strTrayNo; }

                                    filterExpression = "rackNo = " + Common.STS(strRackNo) + " AND rackPos = " + Common.STS(strRackPos);
                                    selectedRows = gdtInquirySpcNoList.Select(filterExpression);

                                    foreach (DataRow row in selectedRows)
                                    {
                                        Common.File_Record(TAB + $"STORE-F 완료 후 DataRow 삭제 rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}" + "\r\n",
                                                           false,
                                                           mstrAppPath + "log\\",
                                                           DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                        row.Delete();
                                    }

                                    if (strGubun == "A" && string.IsNullOrEmpty(strTrayBarNo) == false)
                                    {
                                        if (strSpcNo.IndexOf("^") > -1)
                                        {
                                            strSpcNo = strSpcNo;
                                        }

                                        Common.File_Record(TAB + $"Archive spcNo: {strSpcNo}" + "\r\n",
                                                            false,
                                                            mstrAppPath + "log\\",
                                                            DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        int sequenceNumber = trayManager.RegisterNewTray(strTrayBarNo, Common.Val(strTrayNo));

                                        Common.File_Record(TAB + string.Format("새 트레이가 등록되었습니다. 시퀀스 번호: {0}", sequenceNumber) + "\r\n",
                                            false,
                                            mstrAppPath + "log\\",
                                            DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        // 샘플 저장
                                        bool stored = trayManager.StoreSample(strTrayBarNo, strSpcNo, Common.Val(strTrayPos));
                                        if (stored)
                                        {
                                            Common.File_Record(TAB + "샘플이 성공적으로 저장되었습니다." + "\r\n",
                                                                false,
                                                                mstrAppPath + "log\\",
                                                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        }

                                        // 사용 가능한 위치 조회
                                        List<int> availablePositions = trayManager.GetAvailablePositions(strTrayBarNo);

                                        Common.File_Record(TAB + string.Format("사용 가능한 위치 수: {0}", availablePositions.Count) + "\r\n",
                                            false,
                                            mstrAppPath + "log\\",
                                            DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        // 활성 트레이 정보 조회
                                        List<TrayManagement.TrayInfo> activeTrays = trayManager.GetActiveTrayInfo();
                                        foreach (TrayManagement.TrayInfo tray in activeTrays)
                                        {
                                            if (strTrayBarNo == tray.TrayBarcode)
                                            {
                                                traySeqNo = tray.SequenceNumber.ToString();
                                            }

                                            Common.File_Record(TAB + string.Format("트레이: {0}, 시퀀스: {1}, 현재 샘플 수: {2}, TraySeqno: {3}, TrayBarNo: {4}",
                                                                tray.TrayBarcode,
                                                                tray.SequenceNumber,
                                                                tray.CurrentSamples,
                                                                traySeqNo,
                                                                strTrayBarNo) + "\r\n",
                                                                false,
                                                                mstrAppPath + "log\\",
                                                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        }
                                    }

                                    if (DEBUG_MODE == false)
                                    {
                                        strTmpBgwKey = DateTime.Now.ToString("yyyyMMddHHmmss.ffff") + TAB + strSpcNo + TAB + strRackNo + TAB + strRackPos;

                                        Common.File_Record(TAB +
                                            $"{strAnalysisParameterID} _dctSetRackInfo key: {strTmpBgwKey}, spcNo: {strSpcNo}, rackNo: {strRackNo}, rackPos: {strRackPos}" + "\r\n",
                                            false,
                                            mstrAppPath + "log\\",
                                            DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        if (_dctSetRackInfo.ContainsKey(strTmpBgwKey) == false)
                                        {

                                            Common.File_Record(TAB +
                                                $"key: {strTmpBgwKey}, 구분: {strGubun}, TrayBarNo: {strTrayBarNo}" + "\r\n",
                                                false,
                                                mstrAppPath + "log\\",
                                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                            if (strGubun == "A" && string.IsNullOrEmpty(strTrayBarNo) == false)
                                            {
                                                _dctSetRackInfo.Add(strTmpBgwKey, strRackNo + TAB + strRackPos + TAB + strSpcNo + TAB + traySeqNo + TAB + strSpcNo + TAB + strTrayPos + TAB + strSortIdx + TAB + strTrayNo + TAB + strTrayBarNo);

                                            }
                                            else
                                            {
                                                _dctSetRackInfo.Add(strTmpBgwKey, strRackNo + TAB + strRackPos + TAB + strSpcNo + TAB + strTrayBarNo + TAB + strSpcNo + TAB + strTrayPos + TAB + strSortIdx + TAB + strTrayNo + TAB + strTrayBarNo);
                                            }
                                        }
                                        else
                                        {
                                            Common.File_Record(TAB +
                                                                $"{strAnalysisParameterID} _dctSetRackInfo Duplicate key: {strTmpBgwKey}, spcNo: {strSpcNo}, rackNo: {strRackNo}, rackPos: {strRackPos}" + "\r\n",
                                                                false,
                                                                mstrAppPath + "log\\",
                                                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                        }
                                    }

                                    strAddRow = "Result" + TAB;
                                    strAddRow = strAddRow + strDtm + TAB;
                                    strAddRow = strAddRow + "TS" + TAB;
                                    strAddRow = strAddRow + strSpcNo + TAB;
                                    strAddRow = strAddRow + strRackNo + TAB;
                                    strAddRow = strAddRow + strRackPos + TAB;
                                    strAddRow = strAddRow + strTrayBarNo + "-" + Common.P(strDataOfMeasurementValue, "^", 5) + TAB;
                                    GrdRowAdd(strAddRow);
                                    break;

                                default:
                                    strAddRow = "Result" + TAB;
                                    strAddRow = strAddRow + strDtm + TAB;
                                    strAddRow = strAddRow + strAnalysisParameterID + TAB;
                                    strAddRow = strAddRow + strSpcNo + TAB;
                                    strAddRow = strAddRow + strRackNo + TAB;
                                    strAddRow = strAddRow + strRackPos + TAB;
                                    strAddRow = strAddRow + strDataOfMeasurementValue + TAB;
                                    GrdRowAdd(strAddRow);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (strAnalysisParameterID == "WORKCYCLE") { }
                        if (strAnalysisParameterID == "TRAY") { }
                        if (strAnalysisParameterID == "BUFFERAREA") { }
                        if (strAnalysisParameterID == "WORKAREA") { }
                        if (strAnalysisParameterID == "INITIALIZE") { }

                        string[] aryTray = strDataOfMeasurementValue.Split('\\');
                        string[] arySql = new string[1];
                        string strSql = "";
                        string strTrayType = "";
                        string strTS10RackNo = "";
                        bool blnExistTray = false;
                        string strLog = "";

                        if (strAnalysisParameterID == "EXCHANGE")
                        {
                            for (int i = 0; i < aryTray.Length; i++)
                            {
                                strTrayNo = Common.P(aryTray[i], "^", 1);
                                if (strTrayNo == "") { strTrayNo = Common.P(aryTray[i], "^", 2); }

                                strTrayBarNo = Common.P(aryTray[i], "^", 3);

                                if (strTrayBarNo != "")
                                {
                                    strTS10RackNo = strTrayBarNo;
                                    strTrayType = "ARCHIVE";

                                    trayManager.DeactivateTray(strTrayBarNo);
                                    Common.File_Record(TAB + $"Tray Exchange 로 비활성화 TrayBarNo: {strTrayBarNo}" + "\r\n", false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                    //AC일 수도 있으니
                                    string strSgTray;
                                    string strSgRack;

                                    switch (Constant.SG_TS_EQP_CD)
                                    {
                                        case "620":
                                            strSgTray = "HH";
                                            break;

                                        case "621":
                                            strSgTray = "HG";
                                            break;

                                        case "039":
                                            strSgTray = "BH";
                                            break;

                                        case "922":
                                            strSgTray = "DE";
                                            break;

                                        case "842":
                                            strSgTray = "GE";
                                            break;

                                        case "724":
                                            strSgTray = "JE";
                                            break;

                                        default:
                                            strSgTray = strTrayBarNo;
                                            break;
                                    }

                                    if (strTrayBarNo.Length >= 3)
                                    {
                                        strSgRack = strSgTray + strTrayBarNo.Substring(3, 3);
                                    }
                                    else
                                    {
                                        strSgRack = strSgTray + strTrayBarNo.PadLeft(3, '0').Substring(Math.Max(0, strTrayBarNo.Length - 3));
                                    }

                                    Common.File_Record(TAB + $"씨젠 랙번호로 변경: {strSgRack}" + "\r\n", false, mstrAppPath + "log\\", DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                }
                            }
                        }

                        if (strAnalysisParameterID == "SAMPLE")
                        {
                            for (int i = 0; i < aryTray.Length; i++)
                            {
                                strTrayNo = Common.P(aryTray[i], "^", 1);
                                if (strTrayNo == "") { strTrayNo = Common.P(aryTray[i], "^", 2); }

                                strTrayBarNo = Common.P(aryTray[i], "^", 3);

                                if (strTrayBarNo != "")
                                {
                                    strTS10RackNo = strTrayBarNo;
                                    strTrayType = "ARCHIVE";
                                }
                                else
                                {
                                    strTS10RackNo = strTrayNo;
                                    strTrayType = "SORT";
                                }
                            }
                        }
                    }
                }

                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "이전 수신데이터 분석 및 처리 Start" + CR + LF + strData + "\r\n",
                    false,
                    mstrAppPath + "log\\",
                    DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"이전 수신데이터 분석 및 처리 Exception {ex}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
            }
        }

        // Event handler region
        #region "AxMSWinSck Server"
        private void InitializeAxWinSck()
        {
            AxWinSckServer = new AxWinSck.Server();

            if (DEV_MODE == "Y") { AxWinSckServer.ConnectSck(1024); }
            else { AxWinSckServer.ConnectSck(3000); }

            AxWinSckServer.ConnectEvent += AxWinSckServer_ConnectEvent;
            AxWinSckServer.CloseEvent += AxWinSckServer_CloseEvent;
            AxWinSckServer.DataArrivalEvent += AxWinSckServer_DataArrivalEvent;
        }

        private void AxWinSckServer_ConnectEvent()
        {
            lblSckStatus.BackColor = Color.LightGreen;
            mtTitleSck.BackColor = Color.LightGreen;
        }

        private void AxWinSckServer_CloseEvent()
        {
            lblSckStatus.BackColor = Color.Tomato;
            mtTitleSck.BackColor = Color.Tomato;
        }

        private void AxWinSckServer_DataArrivalEvent(string strMsg)
        {
            RcvTextBoxData(strMsg);
            Data_Analyzer(strMsg);
        }
        #endregion

    }
}