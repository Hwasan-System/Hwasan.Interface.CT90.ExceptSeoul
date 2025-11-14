using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace CT90
{
    public class Common
    {
        #region Properties --------------------

        public static string APP_PATH = Directory.GetCurrentDirectory() + "\\";
        public static string strErrorFile = "yyyyMMdd";
        public static string strTimeFile = "yyyyMMdd-HH";
        public static string strFlagListOfSlideMake = "";
        public static string REC_MODE = "";
        public static Dictionary<string, string> dctSlideMakingRule;
        public static Dictionary<string, string> dctTargetLisTstCd;
        //public static Dictionary<string, string> dctSortingRules;
        public static Dictionary<string, string> dctSortingRulesByRstVal;
        public static Dictionary<string, string> gdctPBSpcNo = new Dictionary<string, string>();
        #endregion Properties --------------------

        #region == Methods ==

        #region == P() [P 함수]==

        public static string P(string pStmt, string pDmt, int pPos, object pObj1 = null, object pObj2 = null)
        {
            string strData = "";

            if (pObj1 == null && pObj2 == null)
            {
                strData = SinglePiece(pStmt, pDmt, pPos);
            }
            else if (pObj1 == null || pObj2 == null)
            {
                if (pObj1.GetType().Name == "Int16" || pObj1.GetType().Name == "Int32" || pObj1.GetType().Name == "Int64" || pObj1.GetType().Name == "Int")
                {
                    strData = MultiPiece(pStmt, pDmt, pPos, Convert.ToInt32(pObj1));
                }
                else
                {
                    strData = SinglePieceSet(pStmt, pDmt, pPos, pObj1.ToString());
                }
            }
            else if (pObj1 != null && pObj2 != null)
            {
                strData = MultiPieceSet(pStmt, pDmt, pPos, Convert.ToInt32(pObj1), pObj2.ToString());
            }

            return strData;
        }

        #endregion == P() [P 함수]==

        #region == SinglePiece() [P 함수] ==

        private static string SinglePiece(string pStmt, string pDmt, int pPos)
        {
            string strData = "";

            if (!string.IsNullOrEmpty(pStmt))
            {
                string[] aryStmt = pStmt.Split(new string[] { pDmt }, StringSplitOptions.None);

                if (aryStmt.Length >= pPos)
                {
                    strData = aryStmt[pPos - 1];
                }
            }

            return strData;
        }

        #endregion == SinglePiece() [P 함수] ==

        #region == MultiPiece() [P 함수] ==

        private static string MultiPiece(string pStmt, string pDmt, int pFromPos, int pToPos)
        {
            string strData = "";

            if (!string.IsNullOrEmpty(pStmt))
            {
                string[] aryStmt = pStmt.Split(new string[] { pDmt }, StringSplitOptions.None);

                if (aryStmt.Length >= pFromPos)
                {
                    int k = aryStmt.Length >= pToPos ? pToPos : aryStmt.Length;

                    for (int i = pFromPos; i <= k; i++)
                    {
                        strData += aryStmt[i - 1];

                        if (i != pToPos)
                        {
                            strData += pDmt;
                        }
                    }
                }
            }

            return strData;
        }

        #endregion == MultiPiece() [P 함수] ==

        #region == SinglePieceSet [P 함수] ==

        private static string SinglePieceSet(string pStmt, string pDmt, int pPos, string pChar)
        {
            string strData = "";

            if (!string.IsNullOrEmpty(pStmt))
            {
                string[] aryStmt = pStmt.Split(new string[] { pDmt }, StringSplitOptions.None);

                if (aryStmt.Length >= pPos)
                {
                    aryStmt[pPos - 1] = pChar;
                }

                strData = string.Join(pDmt, aryStmt);
            }

            if (strData == "")
            {
                strData = pStmt;
            }

            return strData;
        }

        #endregion == SinglePieceSet [P 함수] ==

        #region == MultiPieceSet() [P 함수] ==

        private static string MultiPieceSet(string pStmt, string pDmt, int pFromPos, int pToPos, string pChar)
        {
            string strData = "";

            if (!string.IsNullOrEmpty(pStmt))
            {
                string[] aryStmt = pStmt.Split(new string[] { pDmt }, StringSplitOptions.None);

                if (aryStmt.Length >= pFromPos)
                {
                    int k = aryStmt.Length >= pToPos ? pToPos : aryStmt.Length;

                    for (int i = pFromPos; i <= k; i++)
                    {
                        aryStmt[i - 1] = pChar;
                    }

                    strData = string.Join(pDmt, aryStmt);
                }
            }

            if (strData == "")
            {
                strData = pStmt;
            }

            return strData;
        }

        #endregion == MultiPieceSet() [P 함수] ==

        #region == L() [L 함수] ==

        public static int L(string pStmt, string pDmt)
        {
            return pStmt.Split(new string[] { pDmt }, StringSplitOptions.None).Length;
        }

        #endregion == L() [L 함수] ==

        #region == LengthKor() [한글 2바이트 계산] ==

        public static int LengthKor(string pStmt)
        {
            //byte[] bytArray = Encoding.Default.GetBytes(strStmt);
            byte[] bytStmt = Encoding.GetEncoding("korean").GetBytes(pStmt);

            return bytStmt.Length;

            //string tmp = "한글English";

            //byte []bArray_ =Encoding.Default.GetBytes(tmp);
            //Console.Out.WriteLine(Encoding.Default.EncodingName);
            //Console.Out.WriteLine(Encoding.Default.GetString(bArray_));
            //Console.Out.WriteLine(bArray_.Length);

            //byte []u7Array_ = Encoding.Convert(Encoding.Default, Encoding.UTF7, bArray_);
            //Console.Out.WriteLine(Encoding.UTF7.EncodingName);
            //Console.Out.WriteLine(Encoding.UTF7.GetString(u7Array_));
            //Console.Out.WriteLine(u7Array_.Length);

            //byte []u8Array_ = Encoding.Convert(Encoding.Default, Encoding.UTF8, bArray_);
            //Console.Out.WriteLine(Encoding.UTF8.EncodingName);
            //Console.Out.WriteLine(Encoding.UTF8.GetString(u8Array_));
            //Console.Out.WriteLine(u8Array_.Length);

            //byte []uArray_ = Encoding.Convert(Encoding.Default, Encoding.Unicode, bArray_);
            //Console.Out.WriteLine(Encoding.Unicode.EncodingName);
            //Console.Out.WriteLine(Encoding.Unicode.GetString(uArray_));
            //Console.Out.WriteLine(uArray_.Length);

            //byte []aArray_ = Encoding.Convert(Encoding.Default, Encoding.ASCII, bArray_);
            //Console.Out.WriteLine(Encoding.ASCII.EncodingName);
            //Console.Out.WriteLine(Encoding.ASCII.GetString(aArray_));
            //Console.Out.WriteLine(aArray_.Length);

            //-----------------------------------------------------------------------------------------
            //한국어
            //한글English
            //11

            //유니코드(UTF-7)
            //한글English
            //15

            //유니코드(UTF-8)
            //한글English
            //13

            //유니코드
            //한글English
            //18

            //US-ASCII
            //??English
            //9
        }

        #endregion == LengthKor() [한글 2바이트 계산] ==

        #region == SubstringKor() [한글포함 문자열 자르기] ==

        public static string SubstringKor(string pStmt, int pStartIndex, int pLength)   //한글포함 문자열를 자른다.
        {
            byte[] bytStmt = Encoding.GetEncoding("korean").GetBytes(pStmt);

            if (pLength == 0)
            {
                pLength = bytStmt.Length;
            }

            if ((bytStmt.Length - pStartIndex) < pLength)
            {
                pLength = bytStmt.Length - pStartIndex;
            }

            //if (intLength == 0)
            //    return System.Text.Encoding.GetEncoding("korean").GetString(bytStmt, intStart, bytStmt.Length - intStart);
            //else
            //    return System.Text.Encoding.GetEncoding("korean").GetString(bytStmt, intStart, intLength);

            int j = 1;
            string strTmp = "";

            for (int i = 0; i < pStmt.Length; i++)
            {
                if (j - 1 >= pStartIndex)
                {
                    strTmp += pStmt.Substring(i, 1);
                }

                if (char.ConvertToUtf32(pStmt, i) > 128)
                {
                    j++;
                }

                if (j >= pStartIndex + pLength)
                {
                    break;
                }

                j++;
            }

            return strTmp;
        }

        #endregion == SubstringKor() [한글포함 문자열 자르기] ==

        #region == SentenceCrLf() [문장내용에 특정길이마다 CrLf 넣기] ==

        public static string SentenceCrLf(string strStmt, int intLength)
        {
            string strData = "";

            if (LengthKor(strStmt) <= intLength)
            {
                strData = strStmt;
            }
            else
            {
                string[] aryStmt = strStmt.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                for (int i = 0; i < aryStmt.Length; i++)
                {
                    if (LengthKor(aryStmt[i]) <= intLength)
                    {
                        strData += (i == 0 ? "" : "\r\n") + aryStmt[i];
                    }
                    else
                    {
                        while (aryStmt[i].Length > 0)
                        {
                            if (aryStmt[i].Length == 0)
                            {
                                break;
                            }

                            string strTmp = SubstringKor(aryStmt[i], 0, intLength);

                            strData += (strData == "" ? "" : "\r\n") + strTmp;

                            aryStmt[i] = aryStmt[i].Substring(strTmp.Length);
                        }
                    }
                }
            }

            return strData;
        }

        #endregion == SentenceCrLf() [문장내용에 특정길이마다 CrLf 넣기] ==

        #region == getIpAddress() [IP Address 가져오기] ==

        public static string getIpAddress()
        {
            string strIP = string.Empty;

            //System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

            //for (int i = 0; i < host.AddressList.Length; i++)
            //{
            //    if (host.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            //    {
            //        strIP = host.AddressList[i].ToString();
            //        break;
            //    }
            //}

            System.Net.IPAddress[] ip = System.Net.Dns.GetHostAddresses(Environment.MachineName);

            for (int i = 0; i < ip.Length; i++)
            {
                if (ip[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    strIP = ip[i].ToString();
                    break;
                }
            }

            return strIP;
        }

        #endregion == getIpAddress() [IP Address 가져오기] ==

        #region == getMacAddress() [Mac Address 가져오기] ==

        public static string getMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string strMacAddress = string.Empty;
            long lngMaxSpeed = -1;

            foreach (System.Net.NetworkInformation.NetworkInterface nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                string strTmpMac = nic.GetPhysicalAddress().ToString();

                if (nic.Speed > lngMaxSpeed && !string.IsNullOrEmpty(strTmpMac) && strTmpMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    lngMaxSpeed = nic.Speed;
                    strMacAddress = strTmpMac;
                }
            }

            return strMacAddress;
        }

        #endregion == getMacAddress() [Mac Address 가져오기] ==

        #region == getSysDate() [현재시간가져오기] ==

        public static DateTime getSysDate()
        {
            DateTime dtmDate = DateTime.Now;

            //DataSet dstData = FKHIS.LIS.RemoteLink.LAB.Instance.SelLlcGetSysdate().DataSet;

            //if (dstData != null)
            //{
            //    if (dstData.Tables[0].Rows.Count > 0)
            //    {
            //        dtmDate = Convert.ToDateTime(dstData.Tables[0].Rows[0][0]);
            //    }
            //}

            return dtmDate;
        }

        #endregion == getSysDate() [현재시간가져오기] ==

        #region == getDOBToAge() [생년월일로 나이구하기] ==

        public static string getDOBToAge(string pDOB)
        {
            pDOB.Replace("-", "");

            try
            {
                DateTime dtmDOB = Convert.ToDateTime(pDOB.Substring(0, 4) + "-" + pDOB.Substring(4, 2) + "-" + pDOB.Substring(6, 2));
                DateTime dtmDBDate = getSysDate();
                int intAge = dtmDBDate.Year - dtmDOB.Year;

                if (Convert.ToInt32(dtmDBDate.ToString("MMdd")) < Convert.ToInt32(dtmDOB.ToString("MMdd")))
                {
                    intAge--;
                }

                return intAge.ToString();
            }
            catch
            {
                return "";
            }
        }

        #endregion == getDOBToAge() [생년월일로 나이구하기] ==

        #region == getSSNToDOB() [주민등록번호로 생년월일구하기] ==

        public static string getSSNToDOB(string pSSN)
        {
            pSSN.Replace("-", "");

            if (pSSN.Length > 6)
            {
                switch (pSSN.Substring(6, 1))
                {
                    case "3":
                    case "4":
                    case "7":
                    case "8":
                        return "20" + pSSN.Substring(0, 6);

                    default:
                        return "19" + pSSN.Substring(0, 6);
                }
            }
            else
            {
                return "";
            }
        }

        #endregion == getSSNToDOB() [주민등록번호로 생년월일구하기] ==

        #region == getSSNToAge() [주민등록번호로 나이구하기] ==

        public static string getSSNToAge(string pSSN)
        {
            return getDOBToAge(getSSNToDOB(pSSN));
        }

        #endregion == getSSNToAge() [주민등록번호로 나이구하기] ==

        #region == IsNumeric() [데이터가 숫자형인지 체크] ==

        public static bool IsNumeric(string pData, bool pSignReplaceNull = false)
        {
            if (pSignReplaceNull == true)
            {
                pData = pData.Replace(">=", "").Replace("<=", "").Replace(">", "").Replace("<", "").Replace("=", "").Replace("-", "").Replace(",", "");
            }

            double output;
            return double.TryParse(pData, out output);
        }

        #endregion == IsNumeric() [데이터가 숫자형인지 체크] ==

        #region == ImageToBytes() [image를 byte배열로 변환] ==

        public static byte[] ImageToBytes(System.Drawing.Image pImage)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            pImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        #endregion == ImageToBytes() [image를 byte배열로 변환] ==

        #region == BytesToImage() [byte배열을 image로 변환] ==

        public static System.Drawing.Image BytesToImage(byte[] pByte)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(pByte);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            return returnImage;
        }

        #endregion == BytesToImage() [byte배열을 image로 변환] ==

        #region == FileToBytes() [file을 byte배열로 변환] ==

        public static byte[] FileToBytes(string pFilePath)
        {
            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(pFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                System.IO.BinaryReader br = new System.IO.BinaryReader(fs);

                byte[] bytes = br.ReadBytes((int)fs.Length);

                br.Close();
                fs.Close();

                return bytes;
            }
            catch
            {
                return new byte[0];
            }
        }

        #endregion == FileToBytes() [file을 byte배열로 변환] ==

        #region == BytesToFile() [byte배열을 file로 변환] ==

        public static bool BytesToFile(byte[] pByte, string pFileNm)
        {
            System.IO.FileStream fs = null;

            try
            {
                fs = new System.IO.FileStream(pFileNm, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                fs.Write(pByte, 0, pByte.Length);
                fs.Close();

                return true;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return false;
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }
        }

        #endregion == BytesToFile() [byte배열을 file로 변환] ==

        #region == DateDiff() [DateDiff] ==

        public enum DateInterval
        {
            Day,
            DayOfYear,
            Hour,
            Minute,
            Month,
            Quarter,
            Second,
            Weekday,
            WeekOfYear,
            Year
        }

        public static int DateDiff(DateInterval pIntervalType, System.DateTime pDateOne, System.DateTime pDateTwo)
        {
            switch (pIntervalType)
            {
                case DateInterval.Day:
                case DateInterval.DayOfYear:
                    System.TimeSpan spanForDays = pDateTwo - pDateOne;

                    return (int)spanForDays.TotalDays;

                case DateInterval.Hour:
                    System.TimeSpan spanForHours = pDateTwo - pDateOne;

                    return (int)spanForHours.TotalHours;

                case DateInterval.Minute:
                    System.TimeSpan spanForMinutes = pDateTwo - pDateOne;

                    return (int)spanForMinutes.TotalMinutes;

                case DateInterval.Month:
                    return ((pDateTwo.Year - pDateOne.Year) * 12) + (pDateTwo.Month - pDateOne.Month);

                case DateInterval.Quarter:
                    int dateOneQuarter = (int)System.Math.Ceiling(pDateOne.Month / 3.0);
                    int dateTwoQuarter = (int)System.Math.Ceiling(pDateTwo.Month / 3.0);

                    return (4 * (pDateTwo.Year - pDateOne.Year)) + dateTwoQuarter - dateOneQuarter;

                case DateInterval.Second:
                    System.TimeSpan spanForSeconds = pDateTwo - pDateOne;

                    return (int)spanForSeconds.TotalSeconds;

                case DateInterval.Weekday:
                    System.TimeSpan spanForWeekdays = pDateTwo - pDateOne;

                    return (int)(spanForWeekdays.TotalDays / 7.0);

                case DateInterval.WeekOfYear:
                    System.DateTime dateOneModified = pDateOne;
                    System.DateTime dateTwoModified = pDateTwo;

                    while (dateTwoModified.DayOfWeek != System.Globalization.DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek)
                    {
                        dateTwoModified = dateTwoModified.AddDays(-1);
                    }

                    while (dateOneModified.DayOfWeek != System.Globalization.DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek)
                    {
                        dateOneModified = dateOneModified.AddDays(-1);
                    }

                    System.TimeSpan spanForWeekOfYear = dateTwoModified - dateOneModified;

                    return (int)(spanForWeekOfYear.TotalDays / 7.0);

                case DateInterval.Year:
                    return pDateTwo.Year - pDateOne.Year;

                default:
                    return 0;
            }
        }

        #endregion == DateDiff() [DateDiff] ==

        #region == Truncate() [버림] ==

        public static double Truncate(double pValue, int pDigits)
        {
            if (pDigits > 0)
            {
                return Math.Floor(Math.Pow(10, pDigits) * pValue) / Math.Pow(10, pDigits);
            }
            else
            {
                return Math.Truncate(pValue);
            }
        }

        #endregion == Truncate() [버림] ==

        #region == FormatDate() [####-##-## 포맷으로 표현] ==

        public static string FormatDate(string pData, bool pYY = false, bool pExceptYear = false)
        {
            string strRet = pData;
            double dblOutPut;

            if (double.TryParse(pData, out dblOutPut) == true)
            {
                if (pData.Length == 8)
                {
                    if (pExceptYear == false)
                    {
                        if (pYY == false)
                        {
                            strRet = string.Format("{0:0###-##-##}", Convert.ToDouble(pData));
                        }
                        else
                        {
                            strRet = string.Format("{0:0#-##-##}", Convert.ToDouble(pData.Substring(2)));
                        }
                    }
                    else
                    {
                        strRet = string.Format("{0:0#-##}", Convert.ToDouble(pData.Substring(4)));
                    }
                }
            }

            return strRet;
        }

        #endregion == FormatDate() [####-##-## 포맷으로 표현] ==

        #region == FormatTime() [##:## 포맷으로 표현] ==

        public static string FormatTime(string pData, bool pSec = false)
        {
            string strRet = pData;
            double dblOutPut;

            if (double.TryParse(pData, out dblOutPut) == true)
            {
                if (pSec == false)
                {
                    if (pData.Length == 4 || pData.Length == 6)
                    {
                        strRet = string.Format("{0:00:##}", Convert.ToDouble(pData.Substring(0, 4)));
                    }
                }
                else
                {
                    if (pData.Length == 6)
                    {
                        strRet = string.Format("{0:00:##:##}", Convert.ToDouble(pData));
                    }
                    else if (pData.Length == 4)
                    {
                        strRet = string.Format("{0:00:##}", Convert.ToDouble(pData));
                    }
                }
            }

            return strRet;
        }

        #endregion == FormatTime() [##:## 포맷으로 표현] ==

        #region == ListViewColumnSet() [ListView 컬럼 셋팅] ==

        public static void ListViewColumnSet(ref ListView pListView, string pColumnName, string pColumnText, string pColumnSize, string pAlignment = "")
        {
            //strAlignment
            // '0' : LeftJustify
            // '1' : RightJustify
            // '2' : Center

            if (pColumnName == null)
            {
                pColumnName = "";
            }

            string[] aryColumnName = pColumnName.Split(",".ToCharArray());
            string[] aryColumnText = pColumnText.Split(",".ToCharArray());
            string[] aryWidth = pColumnSize.Split(",".ToCharArray());
            string[] aryAlignment = pAlignment.Split(",".ToCharArray());

            if (aryColumnName.Length < aryColumnText.Length)
            {
                Array.Resize(ref aryColumnName, aryColumnText.Length);
            }

            if (aryWidth.Length < aryColumnText.Length)
            {
                Array.Resize(ref aryWidth, aryColumnText.Length);
            }

            if (aryAlignment.Length < aryColumnText.Length)
            {
                Array.Resize(ref aryAlignment, aryColumnText.Length);
            }

            pListView.Columns.Clear();
            pListView.View = View.Details;

            for (int i = 0; i < aryColumnText.Length; i++)
            {
                if (aryWidth[i] == "")
                {
                    //aryWidth[i] = "0";
                    aryWidth[i] = "30";
                }

                if (aryAlignment[i] == "")
                {
                    aryAlignment[i] = "0";
                }

                if (Convert.ToInt32(aryAlignment[i]) > Convert.ToInt32(HorizontalAlignment.Center))
                {
                    aryAlignment[i] = HorizontalAlignment.Left.ToString();
                }

                pListView.Columns.Add(new ColumnHeader());

                if (string.IsNullOrEmpty(aryColumnName[i]) == false)
                {
                    pListView.Columns[i].Name = aryColumnName[i];
                }

                pListView.Columns[i].Text = aryColumnText[i];
                //pListView.Columns[i].Width = (pListView.Width / aryColumnText.Length) + Convert.ToInt32(aryWidth[i]);
                pListView.Columns[i].Width = Convert.ToInt32(aryWidth[i]);

                switch (aryAlignment[i])
                {
                    case "1":
                        pListView.Columns[i].TextAlign = HorizontalAlignment.Right;
                        break;

                    case "2":
                        pListView.Columns[i].TextAlign = HorizontalAlignment.Center;
                        break;

                    default:
                        pListView.Columns[i].TextAlign = HorizontalAlignment.Left;
                        break;
                }
            }
        }

        #endregion == ListViewColumnSet() [ListView 컬럼 셋팅] ==

        #region == ListViewDataLoadStr() [ListView 데이터 넣기(문자열)] ==

        public static void ListViewDataLoadStr(ref ListView pListView, string pRowDel, string pColDel, string pData,
            string pTag = "", string pDataTag = "", int pIndex = -1, string pImageKey = "", bool pDupChk = false, bool pCheckBox = false)
        {
            //int intCol = P(pData, pRowDel, 1).Split(pColDel.ToCharArray()).Length;

            //string[] aryData = pData.Split(pRowDel.ToCharArray());
            //string[] aryTag = pTag.Split(pRowDel.ToCharArray());
            //string[] arySubTag = pDataTag.Split(pRowDel.ToCharArray());

            int intCol = P(pData, pRowDel, 1).Split(new string[] { pColDel }, StringSplitOptions.None).Length;

            string[] aryData = pData.Split(new string[] { pRowDel }, StringSplitOptions.None);
            string[] aryTag = pTag.Split(new string[] { pRowDel }, StringSplitOptions.None);
            string[] arySubTag = pDataTag.Split(new string[] { pRowDel }, StringSplitOptions.None);

            if (aryData.Length > aryTag.Length)
            {
                Array.Resize(ref aryTag, aryData.Length);
            }

            if (aryData.Length > arySubTag.Length)
            {
                Array.Resize(ref arySubTag, aryData.Length);
            }

            if (aryData.Length < 1) return;

            for (int i = 0; i < aryData.Length; i++)
            {
                ListViewItem itmFind;
                ListViewItem itmX = null;

                if (pDupChk == true)
                {
                    if (pListView.Items.Count > 0)
                    {
                        itmFind = pListView.FindItemWithText(P(aryData[i], pColDel, 1), false, 0, false);
                    }
                    else
                    {
                        itmFind = null;
                    }
                }
                else
                {
                    itmFind = null;
                }

                if (itmFind == null)
                {
                    for (int j = 1; j <= intCol; j++)
                    {
                        if (j == 1)
                        {
                            if (pIndex == -1)
                            {
                                itmX = pListView.Items.Add(new ListViewItem());
                            }
                            else
                            {
                                itmX = pListView.Items.Insert(pIndex, new ListViewItem());
                            }

                            itmX.Text = P(aryData[i], pColDel, j);
                            itmX.Tag = aryTag[i];

                            if (pImageKey != "")
                            {
                                itmX.ImageKey = pImageKey;
                            }

                            if (pListView.CheckBoxes == true && pCheckBox == true)
                            {
                                itmX.Checked = pCheckBox;
                            }
                        }
                        else
                        {
                            if (j <= pListView.Columns.Count)
                            {
                                itmX.SubItems.Add(new ListViewItem.ListViewSubItem());

                                itmX.SubItems[j - 1].Text = P(aryData[i], pColDel, j);
                                itmX.SubItems[j - 1].Tag = P(arySubTag[i], pColDel, j);
                            }
                        }
                    }
                }
            }
        }

        #endregion == ListViewDataLoadStr() [ListView 데이터 넣기(문자열)] ==

        #region == ListViewDataLoadDataSet() [ListView 데이터 넣기(DataSet)] ==

        public static void ListViewDataLoadDataSet(ref ListView lvwListView, DataSet dsData, DataSet dsTag = null,
            DataSet dsSubTag = null, bool blnColumnName = false, int intIndex = -1, string strImageKey = "", bool blnDupChk = false, bool blnCheck = false)
        {
            if (dsData == null || dsData.Tables.Count == 0) return;
            if (dsData.Tables[0].Rows.Count == 0) return;

            for (int i = 0; i < dsData.Tables[0].Rows.Count; i++)
            {
                ListViewItem itmFind;
                ListViewItem itmX = null;

                if (blnDupChk == true)
                {
                    if (lvwListView.Items.Count > 0)
                    {
                        itmFind = lvwListView.FindItemWithText(dsData.Tables[0].Rows[i][0].ToString(), false, 0, false);
                    }
                    else
                    {
                        itmFind = null;
                    }
                }
                else
                {
                    itmFind = null;
                }

                if (itmFind == null)
                {
                    if (blnColumnName == false)
                    {
                        for (int j = 0; j < dsData.Tables[0].Columns.Count; j++)
                        {
                            if (j == 0)
                            {
                                if (intIndex == -1)
                                {
                                    itmX = lvwListView.Items.Add(new ListViewItem());
                                }
                                else
                                {
                                    itmX = lvwListView.Items.Insert(intIndex, new ListViewItem());
                                }

                                if (dsData.Tables[0].Rows[i][dsData.Tables[0].Columns[j].ColumnName] == null)
                                {
                                    itmX.Text = "";
                                }
                                else
                                {
                                    itmX.Text = dsData.Tables[0].Rows[i][j].ToString();
                                }

                                if (dsTag != null)
                                {
                                    if (dsTag.Tables[0].Rows.Count >= i + 1)
                                    {
                                        if (dsTag.Tables[0].Rows[i][dsTag.Tables[0].Columns[0].ColumnName] != null)
                                        {
                                            itmX.Tag = dsTag.Tables[0].Rows[i][0];
                                        }
                                    }
                                }

                                if (strImageKey != "")
                                {
                                    itmX.ImageKey = strImageKey;
                                }

                                if (lvwListView.CheckBoxes == true && blnCheck == true)
                                {
                                    itmX.Checked = blnCheck;
                                }
                            }
                            else
                            {
                                if (j < lvwListView.Columns.Count)
                                {
                                    itmX.SubItems.Add(new ListViewItem.ListViewSubItem());

                                    if (dsData.Tables[0].Rows[i][dsData.Tables[0].Columns[j].ColumnName] == null)
                                    {
                                        itmX.SubItems[j].Text = "";
                                    }
                                    else
                                    {
                                        itmX.SubItems[j].Text = dsData.Tables[0].Rows[i][j].ToString();
                                    }

                                    if (dsSubTag != null)
                                    {
                                        if (dsSubTag.Tables[0].Rows.Count >= i + 1)
                                        {
                                            if (dsSubTag.Tables[0].Columns.Count >= j + 1)
                                            {
                                                if (dsSubTag.Tables[0].Rows[i][dsSubTag.Tables[0].Columns[j].ColumnName] != null)
                                                {
                                                    itmX.SubItems[j].Tag = dsSubTag.Tables[0].Rows[i][j];
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (ColumnHeader colX in lvwListView.Columns)
                        {
                            if (colX.Index == 0)
                            {
                                if (intIndex == -1)
                                {
                                    itmX = lvwListView.Items.Add(new ListViewItem());
                                }
                                else
                                {
                                    itmX = lvwListView.Items.Insert(intIndex, new ListViewItem());
                                }

                                if (dsData.Tables[0].Rows[i][colX.Name] == null)
                                {
                                    itmX.Text = "";
                                }
                                else
                                {
                                    itmX.Text = dsData.Tables[0].Rows[i][colX.Name].ToString();
                                }

                                if (dsTag != null)
                                {
                                    if (dsTag.Tables[0].Rows.Count >= i + 1)
                                    {
                                        if (dsTag.Tables[0].Rows[i][colX.Name] != null)
                                        {
                                            itmX.Tag = dsTag.Tables[0].Rows[i][colX.Name];
                                        }
                                    }
                                }

                                if (strImageKey != "")
                                {
                                    itmX.ImageKey = strImageKey;
                                }

                                if (lvwListView.CheckBoxes == true && blnCheck == true)
                                {
                                    itmX.Checked = blnCheck;
                                }
                            }
                            else
                            {
                                itmX.SubItems.Add(new ListViewItem.ListViewSubItem());

                                if (dsData.Tables[0].Rows[i][colX.Name] == null)
                                {
                                    itmX.SubItems[colX.Index].Text = "";
                                }
                                else
                                {
                                    itmX.SubItems[colX.Index].Text = dsData.Tables[0].Rows[i][colX.Name].ToString();
                                }

                                if (dsSubTag != null)
                                {
                                    if (dsSubTag.Tables[0].Rows.Count >= i + 1)
                                    {
                                        if (dsSubTag.Tables[0].Rows[i][colX.Name] != null)
                                        {
                                            itmX.SubItems[colX.Index].Tag = dsSubTag.Tables[0].Rows[i][colX.Name];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion == ListViewDataLoadDataSet() [ListView 데이터 넣기(DataSet)] ==

        #region == ListViewDataLoadDataTbl() [ListView 데이터 넣기(DataTable)] ==

        public static void ListViewDataLoadDataTbl(ref ListView lvwListView, DataTable dtData, DataTable dtTag = null,
            DataTable dtSubTag = null, bool blnColumnName = false, int intIndex = -1, string strImageKey = "", bool blnDupchk = false, bool blnCheck = false)
        {
            if (dtData == null) return;
            if (dtData.Rows.Count == 0) return;

            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                ListViewItem itmFind;
                ListViewItem itmX = null;

                if (blnDupchk == true)
                {
                    if (lvwListView.Items.Count > 0)
                    {
                        itmFind = lvwListView.FindItemWithText(dtData.Rows[i][0].ToString(), false, 0, false);
                    }
                    else
                    {
                        itmFind = null;
                    }
                }
                else
                {
                    itmFind = null;
                }

                if (itmFind == null)
                {
                    if (blnColumnName == false)
                    {
                        for (int j = 0; j < dtData.Columns.Count; j++)
                        {
                            if (j == 0)
                            {
                                if (intIndex == -1)
                                {
                                    itmX = lvwListView.Items.Add(new ListViewItem());
                                }
                                else
                                {
                                    itmX = lvwListView.Items.Insert(intIndex, new ListViewItem());
                                }

                                if (dtData.Rows[i][dtData.Columns[j].ColumnName] == null)
                                {
                                    itmX.Text = "";
                                }
                                else
                                {
                                    itmX.Text = dtData.Rows[i][j].ToString();
                                }

                                if (dtTag != null)
                                {
                                    if (dtTag.Rows.Count >= i + 1)
                                    {
                                        if (dtTag.Rows[i][dtTag.Columns[0].ColumnName] != null)
                                        {
                                            itmX.Tag = dtTag.Rows[i][0];
                                        }
                                    }
                                }

                                if (strImageKey != "")
                                {
                                    itmX.ImageKey = strImageKey;
                                }

                                if (lvwListView.CheckBoxes == true && blnCheck == true)
                                {
                                    itmX.Checked = blnCheck;
                                }
                            }
                            else
                            {
                                if (j < lvwListView.Columns.Count)
                                {
                                    itmX.SubItems.Add(new ListViewItem.ListViewSubItem());

                                    if (dtData.Rows[i][dtData.Columns[j].ColumnName] == null)
                                    {
                                        itmX.SubItems[j].Text = "";
                                    }
                                    else
                                    {
                                        itmX.SubItems[j].Text = dtData.Rows[i][j].ToString();
                                    }

                                    if (dtSubTag != null)
                                    {
                                        if (dtSubTag.Rows.Count >= i + 1)
                                        {
                                            if (dtSubTag.Columns.Count >= j + 1)
                                            {
                                                if (dtSubTag.Rows[i][dtSubTag.Columns[j].ColumnName] != null)
                                                {
                                                    itmX.SubItems[j].Tag = dtSubTag.Rows[i][j];
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (ColumnHeader colX in lvwListView.Columns)
                        {
                            if (colX.Index == 0)
                            {
                                if (intIndex == -1)
                                {
                                    itmX = lvwListView.Items.Add(new ListViewItem());
                                }
                                else
                                {
                                    itmX = lvwListView.Items.Insert(intIndex, new ListViewItem());
                                }

                                if (dtData.Rows[i][colX.Name] == null)
                                {
                                    itmX.Text = "";
                                }
                                else
                                {
                                    itmX.Text = dtData.Rows[i][colX.Name].ToString();
                                }

                                if (dtTag != null)
                                {
                                    if (dtTag.Rows.Count >= i + 1)
                                    {
                                        if (dtTag.Rows[i][colX.Name] != null)
                                        {
                                            itmX.Tag = dtTag.Rows[i][colX.Name];
                                        }
                                    }
                                }

                                if (strImageKey != "")
                                {
                                    itmX.ImageKey = strImageKey;
                                }

                                if (lvwListView.CheckBoxes == true && blnCheck == true)
                                {
                                    itmX.Checked = blnCheck;
                                }
                            }
                            else
                            {
                                itmX.SubItems.Add(new ListViewItem.ListViewSubItem());

                                if (dtData.Rows[i][colX.Name] == null)
                                {
                                    itmX.SubItems[colX.Index].Text = "";
                                }
                                else
                                {
                                    itmX.SubItems[colX.Index].Text = dtData.Rows[i][colX.Name].ToString();
                                }

                                if (dtSubTag != null)
                                {
                                    if (dtSubTag.Rows.Count >= i + 1)
                                    {
                                        if (dtSubTag.Rows[i][colX.Name] != null)
                                        {
                                            itmX.SubItems[colX.Index].Tag = dtSubTag.Rows[i][colX.Name];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion == ListViewDataLoadDataTbl() [ListView 데이터 넣기(DataTable)] ==

        #region == ComboBoxDataLoad() [ComboBox 데이터 넣기] ==

        public static void ComboBoxDataLoad(ref ComboBox pComboBox, string[] Value, string[] Text)
        {
            if (Value.Length > Text.Length)
            {
                Array.Resize(ref Text, Value.Length);
            }

            DataTable dtCombo = new DataTable("TABLE1");

            dtCombo.Columns.Add("CD");
            dtCombo.Columns.Add("NM");

            for (int i = 0; i < Value.Length; i++)
            {
                DataRow drRow = dtCombo.NewRow();

                drRow["CD"] = Value[i];
                drRow["NM"] = Text[i];

                dtCombo.Rows.Add(drRow);
            }

            pComboBox.DataSource = dtCombo;
            pComboBox.ValueMember = "CD";
            pComboBox.DisplayMember = "NM";

            pComboBox.SelectedIndex = 0;
        }

        #endregion == ComboBoxDataLoad() [ComboBox 데이터 넣기] ==

        #region == Wrap() [Wrap] ==

        public static string Wrap(string str, int maxLength)
        {
            return Wrap(str, maxLength, "");
        }

        /// <summary>
        /// Forces the string to word wrap so that each line doesn't exceed the maxLineLength.
        /// </summary>
        /// <param name="str">The string to wrap.</param>
        /// <param name="maxLength">The maximum number of characters per line.</param>
        /// <param name="prefix">Adds this string to the beginning of each line.</param>
        /// <returns></returns>
        public static string Wrap(string str, int maxLength, string prefix)
        {
            if (string.IsNullOrEmpty(str)) return "";
            if (maxLength <= 0) return prefix + str;

            var lines = new List<string>();

            str = str.Replace("\r", "");

            // breaking the string into lines makes it easier to process.
            foreach (string line in str.Split("\n".ToCharArray()))
            {
                var remainingLine = line;
                do
                {
                    var newLine = GetLine(remainingLine, maxLength - prefix.Length);
                    lines.Add(newLine);
                    remainingLine = remainingLine.Substring(newLine.Length);

                    if (remainingLine.Length > 1)
                    {
                        if (remainingLine.Substring(0, 1) == " ")
                        {
                            remainingLine = remainingLine.Substring(1);
                        }
                    }

                    // Keep iterating as int as we've got words remaining
                    // in the line.
                } while (remainingLine.Length > 0);
            }

            return string.Join(Environment.NewLine + prefix, lines.ToArray());
        }

        private static string GetLine(string str, int maxLength)
        {
            // The string is less than the max length so just return it.
            //if (str.Length <= maxLength) return str;
            if (LengthKor(str) <= maxLength) return str;

            // Search backwords in the string for a whitespace char
            // starting with the char one after the maximum length
            // (if the next char is a whitespace, the last word fits).
            for (int i = maxLength; i >= 0; i--)
            {
                string strTmp = SubstringKor(str, i, 1);

                if (strTmp == "")
                {
                    if (i == maxLength)
                    {
                        return SubstringKor(str, 0, i - 1);
                    }
                    else
                    {
                        //if (char.ConvertToUtf32(SubstringKor(str, i + 1, 1), 0) > 128)
                        if (SubstringKor(str, i + 1, 1) == "")
                        {
                            return SubstringKor(str, 0, i);
                        }
                    }
                }
                else
                {
                    if (i < maxLength)
                    {
                        if (SubstringKor(str, i + 1, 1) != "" && char.ConvertToUtf32(SubstringKor(str, i + 1, 1), 0) > 128)
                        {
                            return SubstringKor(str, 0, i + 1);
                        }
                    }
                    else
                    {
                        if (SubstringKor(str, i - 1, 1) == "")
                        {
                            return SubstringKor(str, 0, i);
                        }
                    }

                    if (char.IsWhiteSpace(strTmp[0]))
                    {
                        //return str.Substring(0, i).TrimEnd();
                        return SubstringKor(str, 0, i + 1);
                    }
                }

                ////if (char.IsWhiteSpace(str[i]))
                ////    //return str.Substring(0, i).TrimEnd();
                ////    return SubstringKor(str, 0, i).TrimEnd();
            }

            // No whitespace chars, just break the word at the maxlength.
            //return str.Substring(0, maxLength);
            return SubstringKor(str, 0, maxLength);
        }

        #endregion == Wrap() [Wrap] ==

        #region == XmlToDataSet() [xml 데이터 DataSet으로 변환] ==

        public static DataSet XmlToDataSet(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            System.IO.MemoryStream ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(xmlDoc.InnerXml));

            DataSet dsData = new DataSet();

            dsData.ReadXml(ms);

            return dsData;
        }

        #endregion == XmlToDataSet() [xml 데이터 DataSet으로 변환] ==

        #region == CheckSum() [체크섬] ==

        public static string CheckSum(string strData)
        {
            int intSum = 0;
            byte[] bytData = Encoding.Default.GetBytes(strData);
            //for (int intIdx = 0; intIdx < bytData.Length; intIdx++)
            //{
            //    intSum += bytData[intIdx];
            //}
            foreach (byte bytChar in bytData)
            {
                intSum += bytChar;
            }
            string strRet = intSum.ToString("X2");
            strRet = strRet.Substring(strRet.Length - 2);
            return strRet;
        }

        #endregion == CheckSum() [체크섬] ==

        #region == KorToEng() [한글을 영문으로 변환] ==

        public static string KorToEng(string pKor)
        {
            string[] aryChoSung = new string[] { "k", "kk", "n", "d", "tt", "r", "m", "p", "pp", "s", "ss", "", "j", "cc", "ch", "kh", "t", "p", "h" };
            string[] aryJungSung = new string[] { "a", "ae", "ya", "yae", "eo", "e", "yeo", "ye", "o", "wa", "oae", "oe", "yo", "u", "weo", "we", "wi", "yu", "eu", "yi", "i" };
            string[] aryJongSung = new string[] { "", "k", "kk", "ks", "n", "nc", "nh", "t", "l", "lk", "lm", "lp", "ls", "lth", "lph", "lh", "m", "p", "ps", "s", "ss", "ng", "c", "ch", "kh", "th", "ph", "h" };

            int intUnicode = 0xAC00;
            string strEng = "";

            try
            {
                for (int i = 0; i < pKor.Length; i++)
                {
                    int intCd = Convert.ToInt32(Convert.ToChar(pKor.Substring(i, 1)));

                    if (intCd == 32 || (intCd > 43 && intCd < 125))
                    {
                        strEng += ((char)intCd).ToString();
                    }
                    else
                    {
                        intCd -= intUnicode;

                        if (intCd < 0)
                        {
                            break;
                        }

                        //초성
                        int intChoSung = Convert.ToInt32(intCd / (21 * 28));

                        intCd = intCd % (21 * 28);

                        //중성
                        int intJungSung = Convert.ToInt32(intCd / 28);
                        //종성
                        int intJongSung = intCd % 28;

                        strEng += aryChoSung[intChoSung];
                        strEng += aryJungSung[intJungSung];
                        strEng += aryJongSung[intJongSung];

                        if (i + 1 != pKor.Length)
                        {
                            strEng += " ";
                        }
                    }
                }

                if (strEng != "")
                {
                    if (strEng.Length >= 2 && strEng.Substring(0, 2) == "i ")
                    {
                        strEng = "lee " + strEng.Substring(2);
                    }
                    else if (strEng.Length >= 4 && strEng.Substring(0, 4) == "pak ")
                    {
                        strEng = "park " + strEng.Substring(4);
                    }
                }
            }
            catch
            {
                //
            }

            return strEng;
        }

        #endregion == KorToEng() [한글을 영문으로 변환] ==

        #region XmlToDataSetUseStringReader() [xml 데이터 DataSet으로 변환 MemoryStream 사용안함] ------------------------------------

        public static DataSet XmlToDataSetUseStringReader(string xml)
        {
            StringReader reader = new StringReader(xml);

            DataSet dsData = new DataSet();

            dsData.ReadXml(reader);

            return dsData;
        }

        #endregion XmlToDataSetUseStringReader() [xml 데이터 DataSet으로 변환 MemoryStream 사용안함] ------------------------------------

        #region Val() [VB Val함수 구현 integer typea만 dobule 구현해야 함] ------------------------------------

        public static int Val(string expression)
        {
            int testInt;

            if (int.TryParse(expression, out testInt))
            {
                return testInt;
            }
            else
            {
                return 0;
            }
        }

        #endregion Val() [VB Val함수 구현 integer typea만 dobule 구현해야 함] ------------------------------------

        #region IsDateTime() ------------------------------------

        public static bool IsDateTime(string pDate)
        {
            DateTime tempDate;
            return DateTime.TryParse(pDate, out tempDate);
        }

        #endregion IsDateTime() ------------------------------------

        public static string BlockCheckCharacter(string strData)
        {
            byte bytSum = 0;
            byte[] bytData = Encoding.Default.GetBytes(strData);
            if (bytData != null && bytData.Length > 0)
            {
                // Exclude SOH during BCC calculation
                for (int i = 0; i < bytData.Length; i++)
                {
                    bytSum ^= bytData[i];
                }
            }

            string strRet = bytSum.ToString("X2");
            strRet = strRet.Substring(strRet.Length - 2);
            return strRet;
        }

        /// <summary>
        /// db single quote
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string STS(string strText)
        {
            return "'" + strText.Replace("'", "''") + "'";
        }

        #region SaveLog() [Log파일 저장] ------------------------------------

        public static void File_Record(string strData, bool blnStamp = true, string strPath = "", string strFile = "", string strTag = "")
        {

            if (Constant.DEBUG_MODE == true) { return; }

            try
            {
                if ((strFile.IndexOf("-BT") > -1) || (strFile.IndexOf("-trace") > -1) || (strFile.IndexOf("SckSend") > -1))
                {
                    return;
                }

                if (strFile.IndexOf("-time") > -1)
                {
                    if (strFile.IndexOf("-timestamp") > -1)
                    {
                    }
                    else
                    {
                        return;
                    }
                }

                if (strPath == "")
                {
                    strPath = Directory.GetCurrentDirectory() + "\\";
                }
                else
                {
                    if (Directory.Exists(strPath) == false) Directory.CreateDirectory(strPath);
                }
                if (strFile == "")
                {
                    strFile = Constant.EQP_CD + strTag + ".log";
                }

                if (blnStamp == true) strData = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "]\r\n" + strData;

                StreamWriter sw = new StreamWriter(strPath + strFile, true, Encoding.Default);
                sw.Write(strData);
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        //public static void SaveLog(string strMethod, string strParam = "", string strRet = "", string strTag = "", string strFlow = "")
        //{
        //    string strFile = "";
        //    strTag = strTag.ToLower();

        //    switch (strTag)
        //    {
        //        case "error":
        //            strFile = DateTime.Now.ToString(strErrorFile) + "-" + strTag + ".log";
        //            break;

        //        case "time":
        //            strFile = DateTime.Now.ToString(strTimeFile) + "-" + strTag + ".log";
        //            break;

        //        case "trace":
        //            return;

        //            strFile = DateTime.Now.ToString(strTimeFile) + "-" + strTag + ".log";
        //            break;

        //        case "sort":
        //            strFile = DateTime.Now.ToString(strTimeFile) + "-" + strTag + ".log";
        //            break;

        //        default:
        //            break;
        //    }
        //    File_Record(strFlow + "(" + strMethod + ")\t" + strParam + "\r\nRet=" + strRet + "\r\n", true, Common.APP_PATH + "log\\", strFile);
        //}

        //public static void SaveLogN(string pLogData, string pFolder = "Log", string pTag = "", string pExt = "log")
        //{
        //    try
        //    {
        //        DateTime dtmDate = System.DateTime.Now;

        //        string strErrMsg = dtmDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\r\n" + ("".PadRight(100, '=')) + "\r\n" + pLogData + "\r\n" + ("".PadRight(100, '=')) + "\r\n\r\n";
        //        string strDirPath = System.Windows.Forms.Application.StartupPath + @"\" + pFolder;

        //        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(strDirPath);
        //        if (di.Exists == false)
        //        {
        //            di.Create();
        //        }

        //        string strFileNm = strDirPath + @"\" + dtmDate.ToString("yyyyMMdd") + (pTag == "" ? "" : "_" + pTag) + "." + pExt;

        //        System.IO.TextWriter tw = new System.IO.StreamWriter(strFileNm, true);
        //        tw.Write(strErrMsg);
        //        tw.Close();
        //    }
        //    catch
        //    {
        //        //
        //    }
        //}

        #endregion SaveLog() [Log파일 저장] ------------------------------------

        #endregion == Methods ==

        public static string GetElapsedTime(DateTime startTime)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;

            string formattedTime = $"{elapsedTime.Hours:D2}:{elapsedTime.Minutes:D2}:{elapsedTime.Seconds:D2}";

            return formattedTime;
        }

        public static string Get_SeeGene_OrdBcd(string strBcd)
        {
            string strRet;
            strRet = strBcd;
            return strRet;
        }
    }


}