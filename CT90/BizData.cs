using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using static CT90.Constant;

namespace CT90
{
    public class BizData
    {
        private static string TAB = ((char)9).ToString();
        private static string CR = ((char)13).ToString();
        private static string DLM_HS = "ː";
        private static string mstrAppPath = Directory.GetCurrentDirectory() + "\\";
        private static string mstrDateTimeFormat = "yyyyMMdd-HH";

        public static DataSet GetSpcInfo(string strSpcNo)
        {
            if (DEV_IN_OFFICE == true)
            {
                return null;
            }

            DataSet dsData = null;
            string strTmpSpcNo = "";

            try
            {

                Lis.Interface.clsBizSeeGene objApi;
                Lis.Interface.clsParameterCollection Param = new Lis.Interface.clsParameterCollection();
                string spcGbn = "";

                if (strSpcNo.Length == 12)
                {
                    spcGbn = strSpcNo[11].ToString(); // 문자열의 12번째 문자 (0부터 시작하므로 11번째 인덱스)
                }

                if (Common.IsNumeric(strSpcNo) && strSpcNo.Length == 12)
                {
                    strTmpSpcNo = strSpcNo.Substring(0, 11);
                }

                objApi = new Lis.Interface.clsBizSeeGene();
                Param = new Lis.Interface.clsParameterCollection();
                Param.Items.Add("EQP_CD", SG_TS_EQP_CD);

                if (spcGbn == "5")
                {
                    Param.Items.Add("SPC_NO", strSpcNo);
                    Param.Items.Add("RECP_STUS", "P");
                }
                else
                {
                    Param.Items.Add("SPC_NO", strTmpSpcNo);
                    Param.Items.Add("RECP_STUS", "T");
                }

                Param.Items.Add("CHK_ALL_YN", "N");
                Param.Items.Add("CNTR_CD_REMOVE", "Y");
                dsData = objApi.GetSpcInfo(Param);
                Param = null;

                objApi = null;
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"GetSpcInfo Exception {ex}, spcNo: {strSpcNo}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                return dsData;
            }

            return dsData;
        }

        public static DataSet GetSpcPos(string strSpcNo)
        {
            //조회 예) OutPut 되는 컬럼을 명시해서 호출 시 DataSet 으로 리턴하도록 만듦. 최초 컬럼은 ROW_NO 로 고정
            //Dim strSql As String = "SELECT CNTR_CD, DEVC_CD, BAR_NO, TST_DT, TST_TM, devcRsltChnl, DEVC_VAL_DIV_CD, devcRsltVal, JOB_NO, RACK_NO, HOLE_NO, DEVC_MDUL_NO, LOT_NO, MTTR_CD, MTTR_LVL_CD, RMRK_CD, RMRK_CONT, LH_JGMT_VAL, PANC_JGMT_VAL, rtstJgmtVal, apctrJgmtVal, slid1JgmtVal, slid2JgmtVal, TST_PGSTP, TRNS_DTM, REG_DTM, RGUR_ID, UPDT_DTM, UPUR_ID  FROM EQPM_INFC.T_HMTLG_TST_RSLT_MGMT WHERE 1=1 AND BAR_NO = '40028103201'"
            //Dim strCol As String = "ROW_NO, CNTR_CD, DEVC_CD, BAR_NO, TST_DT, TST_TM, devcRsltChnl, DEVC_VAL_DIV_CD, devcRsltVal, JOB_NO, RACK_NO, HOLE_NO, DEVC_MDUL_NO, LOT_NO, MTTR_CD, MTTR_LVL_CD, RMRK_CD, RMRK_CONT, LH_JGMT_VAL, PANC_JGMT_VAL, rtstJgmtVal, apctrJgmtVal, slid1JgmtVal, slid2JgmtVal, TST_PGSTP, TRNS_DTM, REG_DTM, RGUR_ID, UPDT_DTM, UPUR_ID"
            //obj.GetSetOrderQuery(strSql, strCol)

            //Insert 예)
            //Dim strSql As String = "INSERT INTO EQPM_INFC.T_HMTLG_TST_RSLT_MGMT (CNTR_CD, DEVC_CD, BAR_NO, TST_DT, TST_TM, devcRsltChnl, DEVC_VAL_DIV_CD, devcRsltVal) VALUES ('14100000', '907', '40028103201', '20240111', '115801', 'MCH', 'R', '29.6')"
            //obj.GetSetOrderQuery(strSql)

            if (DEV_IN_OFFICE == true)
            {
                return null;
            }

            DataSet dsData = null;
            Lis.Interface.clsBizSeeGene objApi;
            string strSql = "";
            string strCol = "";

            strSql = "SELECT ";
            strSql += " CNTR_CD, TST_DT, BAR_NO, RACK_NO, HOLE_NO, TRAY_SEQN, TRAY_NO, TRAY_HOLE_NO, tsGrupNo, TS_GRUP_NM, REG_DTM";
            strSql += " FROM EQPM_INFC.T_SMPL_POS";
            strSql += " WHERE 1=1";
            strSql += " AND BAR_NO = " + Common.STS(strSpcNo);

            switch (Constant.SG_TS_EQP_CD)
            {
                case "620":
                    strSql += " AND DEVC_CD IN ('620', '621')";
                    break;

                case "621":
                    strSql += " AND DEVC_CD IN ('620', '621')";
                    break;

                case "039":
                    strSql += " AND DEVC_CD IN ('039')";
                    break;

                case "922":
                    strSql += " AND DEVC_CD IN ('922')";
                    break;

                case "842":
                    strSql += " AND DEVC_CD IN ('842')";
                    break;

                case "724":
                    strSql += " AND DEVC_CD IN ('724')";
                    break;

                default:
                    //
                    break;
            }

            strSql += " ORDER BY REG_DTM DESC";

            strCol = "ROW_NO, CNTR_CD, TST_DT, BAR_NO, RACK_NO, HOLE_NO, TRAY_SEQN, TRAY_NO, TRAY_HOLE_NO, tsGrupNo, TS_GRUP_NM, REG_DTM";

            objApi = new Lis.Interface.clsBizSeeGene();
            //dsData = objApi.GetDataSetOrderQuery(strSql, strCol);

            string strEqpCds = "";

            //서울: '255', '256', '257', '258', '259', '260', '261', '262', '263', '264', '265', '266'
            //부산: '095', '096', '097'
            //대구: '907', '908', '01067'
            //광주: '843', '01069', '01070'
            //대전: '722', '740', '741'

            switch (Constant.SG_TS_EQP_CD)
            {
                case "620":
                    //2024-05-17 : 현재는 1개만 됨, 추후 업데이트 필요
                    strEqpCds = "620,621";
                    //strDecdCds = "620";
                    break;

                case "621":
                    strEqpCds = "620,621";
                    break;

                case "039":
                    strEqpCds = "039";
                    break;

                case "922":
                    strEqpCds = "922";
                    break;

                case "842":
                    strEqpCds = "842";
                    break;

                case "724":
                    strEqpCds = "724";
                    break;

                default:
                    //
                    break;
            }

            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"GetSpcPos, spcNo: {strSpcNo}, EqpCd: {strEqpCds}" + "\r\n",
                               false,
                               mstrAppPath + "log\\",
                               DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

            dsData = objApi.GetSpmlPos(strSpcNo, strEqpCds);

            objApi = null;

            return dsData;
        }

        public static DataSet GetRsltHematology(string strSpcNo)
        {
            if (DEV_IN_OFFICE == true)
            {
                return null;
            }

            DataSet dsData = null;
            Lis.Interface.clsBizSeeGene objApi;
            objApi = new Lis.Interface.clsBizSeeGene();

            //'rsltDtm          검사결과일시(결과일시)
            //'transDtm         검사결과전송일시
            //'devcRsltChnl     장비측검사코드
            //'slid1JgmtVal     슬라이드1판정값
            //'slid2JgmtVal     슬라이드2판정값
            //rtstJgmtVal       재검판정값
            //apctrJgmtVal      떠보기판정값
            //devcRsltVal       장비결과
            //devcFlagCont      판정상태값

            dsData = objApi.GetTestResultHematology(strSpcNo);
            objApi = null;

            return dsData;
        }

        public static DataSet GetRslt(string strSpcNo)
        {

            if (DEV_IN_OFFICE == true)
            {
                return null;
            }

            DataSet dsData = null;
            Lis.Interface.clsBizSeeGene objApi;
            objApi = new Lis.Interface.clsBizSeeGene();

            string strDecdCds = "";

            //서울: '255', '256', '257', '258', '259', '260', '261', '262', '263', '264', '265', '266'
            //부산: '095', '096', '097'
            //대구: '907', '908', '01067'
            //광주: '843', '01069', '01070'
            //대전: '722', '740', '741'

            switch (Constant.SG_TS_EQP_CD)
            {
                case "620":
                    strDecdCds = "255,256,257,258,259,260";
                    break;

                case "621":
                    strDecdCds = "255,256,257,258,259,260";
                    break;

                case "039":
                    strDecdCds = "095,096,097";
                    break;

                case "922":
                    strDecdCds = "907,908,01067";
                    break;

                case "842":
                    strDecdCds = "843,01069,01070";
                    break;

                case "724":
                    strDecdCds = "722,740,741";
                    break;

                default:
                    //
                    break;
            }

            //2024-05-17 : 장비코드 구현이 안되어서 일단 통으로 조회
            strDecdCds = "";

            //2024-12-10 : 결과조회 API에서 혈액학결과조회 API로 변경
            //dsData = objApi.GetDevcTestResult2(strSpcNo, strDecdCds);
            dsData = objApi.GetXnDevcResult(strSpcNo);

            objApi = null;

            return dsData;
        }

        public static DataSet GetDevcTestResult2(string strSpcNo)
        {

            if (DEV_IN_OFFICE == true)
            {
                return null;
            }

            string strDecdCds = "";
            DataSet dsData = null;
            Lis.Interface.clsBizSeeGene objApi;

            objApi = new Lis.Interface.clsBizSeeGene();
            
            //서울: '255', '256', '257', '258', '259', '260', '261', '262', '263', '264', '265', '266'
            //부산: '095', '096', '097'
            //대구: '907', '908', '01067'
            //광주: '843', '01069', '01070'
            //대전: '722', '740', '741'
            switch (Constant.SG_TS_EQP_CD)
            {
                case "620":
                    strDecdCds = "255,256,257,258,259,260";
                    break;

                case "621":
                    strDecdCds = "255,256,257,258,259,260";
                    break;

                case "039":
                    strDecdCds = "095,096,097";
                    break;

                case "922":
                    strDecdCds = "907,908,01067";
                    break;

                case "842":
                    strDecdCds = "843,01069,01070";
                    break;

                case "724":
                    strDecdCds = "722,740,741";
                    break;

                default:
                    //
                    break;
            }

            strDecdCds = "";
            dsData = objApi.GetDevcTestResult2(strSpcNo, strDecdCds);
            objApi = null;
            return dsData;
        }

        public static DateTime GetDBSysDate()
        {
            DateTime dtmDate = DateTime.Now;

            DataSet dsData = null;

            if (SysInfo.DataBase.OleDbConnection != null)
            {
                System.Data.OleDb.OleDbCommand odbCmd = new System.Data.OleDb.OleDbCommand();

                odbCmd.CommandType = System.Data.CommandType.Text;
                odbCmd.CommandText = "select sysdate as datetime from dual";

                dsData = CommonDB.DBSelect(odbCmd);
            }
            else
            {
                if (SysInfo.DataBase.MySqlConnection != null)
                {
                    MySql.Data.MySqlClient.MySqlCommand mysCmd = new MySql.Data.MySqlClient.MySqlCommand();

                    mysCmd.CommandType = System.Data.CommandType.Text;
                    mysCmd.CommandText = "select now(6) as datetime";

                    dsData = CommonDB.DBSelect(mysCmd);
                }
            }

            if (dsData != null && dsData.Tables.Count > 0)
            {
                if (dsData.Tables[0].Rows.Count > 0)
                {
                    dtmDate = Convert.ToDateTime(dsData.Tables[0].Rows[0][0]);
                }

                dsData.Clear();
            }

            dsData = null;

            return dtmDate;
        }

        public static int SetOrderInfo(string strSpcNoList, string strInqTyp, string UnitNo, string RackNo)
        {
            DataSet dsTemp = null;
            string[] arySpcNo = strSpcNoList.Split(',');
            string[] arySql = new string[arySpcNo.Length];
            string strLisTstCdList = "";
            string strHisStatus = "";
            string strCT90OrderParmeter = "";
            List<string> lstLisGrpPrscCd = new List<string>();
            bool blnPB = false;
            bool blnRET = false;
            bool blnCBC = false;
            bool blnDiff = false;
            bool blnAmmonia = false;
            bool blnNoOrd = false;

            string filterExpression;
            DataRow[] selectedRows;
            List<string> lstNoOrd = new List<string>();
            List<string> lstOtherCntrCd = new List<string>();
            List<string> lstCntrCd = new List<string>();
            bool blnChkOrd = false;

            Lis.Interface.clsBizSeeGene objApi = null;

            try
            {
                // 파라미터 유효성 검사
                if (string.IsNullOrEmpty(strSpcNoList) || string.IsNullOrEmpty(RackNo))
                {
                    Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB +
                                    "SetOrderInfo: 유효하지 않은 파라미터 - strSpcNoList 또는 RackNo가 null이거나 비어 있습니다." + "\r\n",
                                    false,
                                    mstrAppPath + "log\\",
                                    DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return -1;
                }

                // gdtInquirySpcNoList가 null인지 확인
                if (gdtInquirySpcNoList == null)
                {
                    Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB +
                                    "SetOrderInfo: gdtInquirySpcNoList가 null입니다" + "\r\n",
                                    false,
                                    mstrAppPath + "log\\",
                                    DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                    return -1;
                }

                if (DEV_IN_OFFICE == false && DEBUG_MODE == false)
                {
                    objApi = new Lis.Interface.clsBizSeeGene();
                    if (objApi == null)
                    {
                        Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB +
                                        "SetOrderInfo: objApi 객체를 초기화할 수 없습니다" + "\r\n",
                                        false,
                                        mstrAppPath + "log\\",
                                        DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        return -1;
                    }
                }

                for (int i = 0; i < arySpcNo.Length; i++)
                {
                    strLisTstCdList = "";
                    strHisStatus = "";
                    strCT90OrderParmeter = "";
                    lstLisGrpPrscCd.Clear();

                    blnPB = false;
                    blnRET = false;
                    blnCBC = false;
                    blnDiff = false;
                    blnNoOrd = false;
                    bool setNoOrder = false;

                    if (!(strInqTyp == "SI" || strInqTyp == "SO"))
                    {
                        blnChkOrd = true;
                    }
                    else
                    {
                        if (Constant.SG_TS_EQP_CD == "621")
                        {
                            blnChkOrd = true;
                        }
                    }

                    if (blnChkOrd == true)
                    {
                        Lis.Interface.clsParameterCollection Param = new Lis.Interface.clsParameterCollection();
                        if (Param != null && Param.Items != null)
                        {
                            Param.Items.Add("EQP_CD", SG_TS_EQP_CD);
                            Param.Items.Add("SPC_NO", arySpcNo[i]);
                            Param.Items.Add("RECP_STUS", "P");
                            Param.Items.Add("CHK_ALL_YN", "N");

                            if (Constant.gstrComputerName != "KKH" && objApi != null)
                            {
                                dsTemp = objApi.GetSpcInfo(Param);
                            }
                            Param = null;
                        }

                        if (dsTemp == null || dsTemp.Tables.Count == 0)
                        {
                            // dsTemp가 없거나 테이블이 없는 경우 바로 no order 처리
                            lstNoOrd.Add(arySpcNo[i]);
                        }
                        else
                        {
                            // 2025-02-17 : 무조건 no order 처리
                            //              3610 추후송부
                            //              3630 우선검사진행
                            //              9070 검사제외
                            filterExpression = "TST_STAT_CD IN ('3610', '3630', '9070') ";

                            if (dsTemp.Tables.Count > 0 && dsTemp.Tables[0] != null)
                            {
                                selectedRows = dsTemp.Tables[0].Select(filterExpression);

                                if (selectedRows != null && selectedRows.Length > 0)
                                {
                                    foreach (DataRow drTemp in selectedRows)
                                    {
                                        if (drTemp != null && drTemp["SPC_CD"] != null && drTemp["SPC_CD"].ToString().Trim() == "A05")
                                        {
                                            // A05 코드가 발견되면 no order 처리 플래그 설정
                                            setNoOrder = true;
                                            break; // 조건 충족했으므로 루프 중단
                                        }
                                    }
                                }
                            }
                        }

                        // 루프 바깥에서 no order 처리 결정
                        if (setNoOrder)
                        {
                            //set no order
                            lstNoOrd.Add(arySpcNo[i]);
                        }
                        else if (dsTemp != null && dsTemp.Tables.Count > 0 && dsTemp.Tables[0] != null && dsTemp.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow drTemp in dsTemp.Tables[0].Rows)
                            {
                                if (drTemp == null) continue;

                                System.Windows.Forms.Application.DoEvents();

                                //2020-07-31 : Ammonia 검사가 접수된 검체의 경우( ammonia 단독 / ammonia+CBC / ammonia + EDTA로 하는 모든검사 ) CBC 검사 하지않고 서울 & 서울+대구 렉에 위치될수 있도록 수정 바랍니다. (CBC 검사를 먼저 할 경우 Ammonia 검사불가능)
                                //00026

                                //2025-01-07 : 이현석 대리 요청으로 오더처리 시에도 타ID는 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                                if (drTemp["OTHER_RCPT_DT"] != null && string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                                {
                                    string strLisTstCd = "";

                                    if (drTemp["LIS_TST_SUB_CD"] != null && (drTemp["LIS_TST_SUB_CD"].ToString() == "" || drTemp["LIS_TST_SUB_CD"].ToString() == "-"))
                                    {
                                        if (drTemp["LIS_TST_CD"] != null)
                                        {
                                            strLisTstCd = drTemp["LIS_TST_CD"].ToString();
                                        }
                                    }
                                    else
                                    {
                                        if (drTemp["LIS_TST_CD"] != null && drTemp["LIS_TST_SUB_CD"] != null)
                                        {
                                            strLisTstCd = drTemp["LIS_TST_CD"].ToString() + drTemp["LIS_TST_SUB_CD"].ToString();
                                        }
                                    }

                                    if (strLisTstCd == "00026" || strLisTstCd == "0002600") { blnAmmonia = true; }

                                    if (strLisTstCd == "11052" || strLisTstCd == "1105200")
                                    {
                                        blnPB = true;

                                        // 2025-07-14 : 대구씨젠도 서울씨젠처럼 RET 오더처리
                                        if (Constant.SG_TS_EQP_CD == "620" || Constant.SG_TS_EQP_CD == "621" || Constant.SG_TS_EQP_CD == "922")
                                        {
                                            //서울씨젠은 RET 오더일때만 RET 오더처리
                                        }
                                        else
                                        {
                                            blnRET = true;
                                        }
                                    }

                                    if (strLisTstCd == "11310" || strLisTstCd == "1131000") { blnRET = true; }

                                    if (blnDiff == false)
                                    {
                                        if (strLisTstCd == "11002" || strLisTstCd == "1100200") { blnDiff = true; }
                                        if (strLisTstCd == "11003" || strLisTstCd == "1100300") { blnDiff = true; }
                                    }

                                    if (blnCBC == false)
                                    {
                                        if (strLisTstCd == "11001" || strLisTstCd == "1100100") { blnCBC = true; }
                                        if (strLisTstCd == "11004" || strLisTstCd == "1100400") { blnCBC = true; }
                                        if (strLisTstCd == "11005" || strLisTstCd == "1100500") { blnCBC = true; }
                                        if (strLisTstCd == "11006" || strLisTstCd == "1100600") { blnCBC = true; }
                                        if (strLisTstCd == "11007" || strLisTstCd == "1100700") { blnCBC = true; }
                                        if (strLisTstCd == "11008" || strLisTstCd == "1100800") { blnCBC = true; }
                                        if (strLisTstCd == "11009" || strLisTstCd == "1100900") { blnCBC = true; }
                                        if (strLisTstCd == "11011" || strLisTstCd == "1101100") { blnCBC = true; }
                                        if (strLisTstCd == "11012" || strLisTstCd == "1101200") { blnCBC = true; }
                                        if (strLisTstCd == "11013" || strLisTstCd == "1101300") { blnCBC = true; }
                                        if (strLisTstCd == "11014" || strLisTstCd == "1101400") { blnCBC = true; }
                                        if (strLisTstCd == "11015" || strLisTstCd == "1101500") { blnCBC = true; }
                                        if (strLisTstCd == "11017" || strLisTstCd == "1101700") { blnCBC = true; }
                                        if (strLisTstCd == "68000" || strLisTstCd == "6800000") { blnCBC = true; }
                                        if (strLisTstCd == "68033" || strLisTstCd == "6803300") { blnCBC = true; }
                                    }

                                    if (lstLisGrpPrscCd.Contains(strLisTstCd) == false)
                                    {
                                        lstLisGrpPrscCd.Add(strLisTstCd);

                                        if (strLisTstCdList == "")
                                        {
                                            strLisTstCdList = strLisTstCd;
                                        }
                                        else
                                        {
                                            strLisTstCdList = strLisTstCdList + "," + strLisTstCd;
                                        }
                                    }

                                    if (drTemp["CNTR_CD"] != null && Constant.gstrCenterCode == drTemp["CNTR_CD"].ToString())
                                    {
                                        if (drTemp["SPC_NO"] != null && lstCntrCd.Contains(drTemp["SPC_NO"].ToString()) == false)
                                        {
                                            lstCntrCd.Add(drTemp["SPC_NO"].ToString());
                                        }
                                    }
                                    else
                                    {
                                        if (drTemp["SPC_NO"] != null && lstOtherCntrCd.Contains(drTemp["SPC_NO"].ToString()) == false)
                                        {
                                            lstOtherCntrCd.Add(drTemp["SPC_NO"].ToString());
                                        }
                                    }

                                    if (strLisTstCd == "" && drTemp["SPC_NO"] != null && lstNoOrd.Contains(drTemp["SPC_NO"].ToString()) == false)
                                    {
                                        lstNoOrd.Add(drTemp["SPC_NO"].ToString());
                                    }
                                }
                            }
                        }

                        if (dsTemp != null) { dsTemp.Clear(); }
                        dsTemp = null;

                        //2025-05-14 : 대구씨젠 암모니아 오더처리 위해 9번 시퀀스로 오더조회 1번더
                        if (Constant.SG_TS_EQP_CD == "922")
                        {
                            if (Common.IsNumeric(arySpcNo[i]) && arySpcNo[i].Length == 12)
                            {
                                string strTmpSpcNo;
                                strTmpSpcNo = arySpcNo[i].Substring(0, 11);
                                strTmpSpcNo = strTmpSpcNo + "9";

                                Param = new Lis.Interface.clsParameterCollection();
                                if (Param != null && Param.Items != null)
                                {
                                    Param.Items.Add("EQP_CD", SG_TS_EQP_CD);
                                    Param.Items.Add("SPC_NO", strTmpSpcNo);
                                    Param.Items.Add("RECP_STUS", "P");
                                    Param.Items.Add("CHK_ALL_YN", "N");

                                    if (Constant.gstrComputerName != "KKH" && objApi != null)
                                    {
                                        dsTemp = objApi.GetSpcInfo(Param);
                                    }
                                    Param = null;

                                    if (dsTemp != null && dsTemp.Tables.Count > 0 && dsTemp.Tables[0] != null && dsTemp.Tables[0].Rows.Count > 0)
                                    {
                                        foreach (DataRow drTemp in dsTemp.Tables[0].Rows)
                                        {
                                            if (drTemp == null) continue;

                                            //2025-01-07 : 이현석 대리 요청으로 오더처리 시에도 타ID는 제외 = OTHER_RCPT_DT, OTHER_RCPT_NO
                                            if (drTemp["OTHER_RCPT_DT"] != null && string.IsNullOrEmpty(drTemp["OTHER_RCPT_DT"].ToString().Trim()) == true)
                                            {
                                                string strLisTstCd = "";

                                                if (drTemp["LIS_TST_SUB_CD"] != null && (drTemp["LIS_TST_SUB_CD"].ToString() == "" || drTemp["LIS_TST_SUB_CD"].ToString() == "-"))
                                                {
                                                    if (drTemp["LIS_TST_CD"] != null)
                                                    {
                                                        strLisTstCd = drTemp["LIS_TST_CD"].ToString();
                                                    }
                                                }
                                                else
                                                {
                                                    if (drTemp["LIS_TST_CD"] != null && drTemp["LIS_TST_SUB_CD"] != null)
                                                    {
                                                        strLisTstCd = drTemp["LIS_TST_CD"].ToString() + drTemp["LIS_TST_SUB_CD"].ToString();
                                                    }
                                                }

                                                if (strLisTstCd == "00026" || strLisTstCd == "0002600") { blnAmmonia = true; }                                              
                                            }
                                        }
                                    }
                                }
                                if (dsTemp != null) { dsTemp.Clear(); }
                                dsTemp = null;
                            }
                        }

                        //2020-06-12 : 씨젠 광주검사센터 - PBS, RET 오더가 있을 경우 검사하지 않음, 소팅 후 수동으로 잡아서 검사시에는 검사해야 함.
                        if (Constant.SG_TS_EQP_CD == "842" || Constant.SG_TS_EQP_CD == "039")
                        {
                            //2025-05-10 RET or PB 에서 PB 일 경우로 변경
                            if (blnPB == true)
                            {
                                strLisTstCdList = "";
                                blnNoOrd = true;

                                if (glstNoOrdByPB.Contains(arySpcNo[i]) == false)
                                {
                                    glstNoOrdByPB.Add(arySpcNo[i]);
                                }

                                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "SetOrdInfo 광주씨젠 PB 오더있을 경우 No Order 처리" + "\r\n",
                                                   false,
                                                   mstrAppPath + "log\\",
                                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                            }
                        }

                        //부산씨젠
                        if (Constant.SG_TS_EQP_CD == "039")
                        {
                            if (blnPB == true)
                            {
                                strLisTstCdList = "";
                                blnNoOrd = true;
                                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + "SetOrdInfo 부산씨젠 PB 오더있을 경우 No Order 처리" + "\r\n",
                                                   false,
                                                   mstrAppPath + "log\\",
                                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                            }
                        }

                        //2025-04-28 : 대전씨젠 CT-90 에서 PB 일 때 SP 오더처리 하는 것 중지 요청해서 처리
                        //대전씨젠 PB 오더체크
                        //if (Constant.SG_TS_EQP_CD == "724")
                        //{
                        //    if (blnPB == true)
                        //    {
                        //        blnRET = false;

                        //        if (Common.gdctPBSpcNo != null && Common.gdctPBSpcNo.ContainsKey(arySpcNo[i]) == false)
                        //        {
                        //            Common.gdctPBSpcNo.Add(arySpcNo[i], "^^^^SP");
                        //        }
                        //    }
                        //}

                        if (strLisTstCdList == "")
                        {
                            strCT90OrderParmeter = "N";
                        }
                        else
                        {
                            //ESR, HbA1c 단독오더일 경우 C+D+R 검사하지 않음
                            if ((strLisTstCdList == "11138") || (strLisTstCdList == "11139") || (strLisTstCdList == "00095"))
                            {
                                strCT90OrderParmeter = "N";
                            }
                            else
                            {
                                //strCT90OrderParmeter = "CBC^DIFF^SP";

                                if (blnCBC == true || blnDiff == true)
                                {
                                    if (blnRET)
                                    {
                                        strCT90OrderParmeter = "CBC^DIFF^RET";
                                    }
                                    else
                                    {
                                        strCT90OrderParmeter = "CBC^DIFF";
                                    }
                                }
                                else
                                {
                                    if (blnRET)
                                    {
                                        //2025-07-14 : 대구씨젠도 추가
                                        //2025-03-17 : 서울씨젠 RET 단독일 경우 RET만 오더처리
                                        if (Constant.SG_TS_EQP_CD == "620" || Constant.SG_TS_EQP_CD == "621" || Constant.SG_TS_EQP_CD == "922")
                                        {
                                            strCT90OrderParmeter = "RET";
                                        }
                                        else
                                        {
                                            strCT90OrderParmeter = "CBC^DIFF^RET";
                                        }
                                    }
                                    else
                                    {
                                        strCT90OrderParmeter = "N";
                                    }
                                }
                            }
                        }

                        //2022-04-13 : 대구씨젠 요청으로 PB, RET 오더처리하도록 함.
                        //if (blnPB == true) { strCT90OrderParmeter = "N"; }
                        //if (blnRET == true) { strCT90OrderParmeter = "N"; }
                        if (blnAmmonia == true) { strCT90OrderParmeter = "N"; }

                        Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB +
                            $"SetOrdInfo: spcNo {arySpcNo[i]}, InqTyp {strInqTyp}, OrdCd {strCT90OrderParmeter}, NoOrd {lstNoOrd.Contains(arySpcNo[i])}, OtherCntrCd {lstOtherCntrCd.Contains(arySpcNo[i])}, CntrCd {lstCntrCd.Contains(arySpcNo[i])}" + "\r\n",
                            false,
                            mstrAppPath + "log\\",
                            DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                        if (strInqTyp == "B" && strCT90OrderParmeter == "N")
                        {
                            if (blnNoOrd == true)
                            {
                                strCT90OrderParmeter = "";
                            }
                            else
                            {
                                //strCT90OrderParmeter = "CBC^DIFF";
                                strCT90OrderParmeter = "";
                            }

                            //2025-04-28 : 대전 CT-90 PB 일 때 SP 오더처리 안하도록 요청
                            //if (Constant.SG_TS_EQP_CD == "724" && blnPB == true)
                            //{
                            //    strCT90OrderParmeter = "SP";
                            //}

                            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"SetOrdInfo: spcNo: {arySpcNo[i]}" + "\r\n",
                                                                false,
                                                                mstrAppPath + "log\\",
                                                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        }

                        if (!string.IsNullOrEmpty(arySpcNo[i]))
                        {
                            //arySql[i] = "";
                            //arySql[i] = "update TCT90DM set ord_info = " + Common.STS(strCT90OrderParmeter);
                            //arySql[i] = arySql[i] + " where spc_no = " + Common.STS(arySpcNo[i]);
                            //Common.File_RecordN(arySql[i] + "\r\n", "Log", "sql", "log");
                            //Array.Resize(ref mssCmd, mssCmd == null ? 1 : mssCmd.Length + 1);
                            //mssCmd[mssCmd.Length - 1] = new System.Data.SqlClient.SqlCommand();
                            //mssCmd[mssCmd.Length - 1].CommandType = CommandType.Text;
                            //mssCmd[mssCmd.Length - 1].CommandText = arySql[i];

                            filterExpression = "rackNo = " + Common.STS(RackNo) + " AND spcNo = " + Common.STS(arySpcNo[i]);

                            if (gdtInquirySpcNoList != null)
                            {
                                selectedRows = gdtInquirySpcNoList.Select(filterExpression);

                                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB +
                                                $"SetOrdInfo: spcNo {arySpcNo[i]}, selectedRows {(selectedRows != null ? selectedRows.Length : 0)}" + "\r\n",
                                                false,
                                                mstrAppPath + "log\\",
                                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                if (selectedRows != null)
                                {
                                    foreach (DataRow row in selectedRows)
                                    {
                                        if (row == null) continue;

                                        Console.WriteLine($"rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}");

                                        Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB +
                                                        $"rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}" + "\r\n",
                                                        false,
                                                        mstrAppPath + "log\\",
                                                        DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                                        if (row["spcNo"] != null && lstNoOrd.Contains(row["spcNo"].ToString()) == true)
                                        {
                                            row["ordInfo"] = "N";

                                            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"No Order: spcNo: {row["spcNo"]}" + "\r\n",
                                                false,
                                                mstrAppPath + "log\\",
                                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                        }
                                        else
                                        {
                                            if (row["spcNo"] != null && lstOtherCntrCd.Contains(row["spcNo"].ToString()) == true &&
                                                !lstCntrCd.Contains(row["spcNo"].ToString()))
                                            {
                                                row["ordInfo"] = "N";

                                                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"No Order: spcNo: {row["spcNo"]}" + "\r\n",
                                                    false,
                                                    mstrAppPath + "log\\",
                                                    DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                                            }
                                            else
                                            {
                                                row["ordInfo"] = strCT90OrderParmeter;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB +
                                                "SetOrderInfo: gdtInquirySpcNoList가 null입니다" + "\r\n",
                                                false,
                                                mstrAppPath + "log\\",
                                                DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                            }
                        }
                    }
                    else
                    {
                        strCT90OrderParmeter = "N";
                    }
                }

                if (objApi != null)
                {
                    objApi = null;
                }

                if (strInqTyp == "SI" || strInqTyp == "SO")
                {
                    if (Constant.SG_TS_EQP_CD == "621")
                    {
                        //
                    }
                    else
                    {
                        strCT90OrderParmeter = "N";
                        //arySql[0] = "";
                        //arySql[0] = "update TCT90DM set ord_info = " + Common.STS(strCT90OrderParmeter);
                        //arySql[0] = arySql[0] + " where rack_no = " + Common.STS(RackNo);
                        //Common.File_RecordN(arySql[0] + "\r\n", "Log", "sql", "log");
                        //Array.Resize(ref mssCmd, mssCmd == null ? 1 : mssCmd.Length + 1);
                        //mssCmd[mssCmd.Length - 1] = new System.Data.SqlClient.SqlCommand();
                        //mssCmd[mssCmd.Length - 1].CommandType = CommandType.Text;
                        //mssCmd[mssCmd.Length - 1].CommandText = arySql[0];

                        filterExpression = "rackNo = " + Common.STS(RackNo);
                        if (gdtInquirySpcNoList != null)
                        {
                            selectedRows = gdtInquirySpcNoList.Select(filterExpression);
                            if (selectedRows != null)
                            {
                                foreach (DataRow row in selectedRows)
                                {
                                    if (row == null) continue;
                                    Console.WriteLine($"rackNo: {row["rackNo"]}, rackPos: {row["rackPos"]}, spcNo: {row["spcNo"]}");
                                    row["ordInfo"] = strCT90OrderParmeter;
                                }
                            }
                        }
                        else
                        {
                            Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB +
                                            "SetOrderInfo: gdtInquirySpcNoList가 null입니다" + "\r\n",
                                            false,
                                            mstrAppPath + "log\\",
                                            DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.File_Record(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + TAB + $"SetOrderInfo Exception {ex}, spcNoList: {strSpcNoList}, InqTyp: {strInqTyp}, UnitNo: {UnitNo}, RackNo: {RackNo}" + "\r\n",
                                   false,
                                   mstrAppPath + "log\\",
                                   DateTime.Now.ToString(mstrDateTimeFormat) + "-TimeStamp.log");

                return -1;
            }
            finally
            {
                // 리소스 정리
                if (dsTemp != null)
                {
                    dsTemp.Dispose();
                    dsTemp = null;
                }

                if (objApi != null)
                {
                    objApi = null;
                }
            }

            return 1;
        }

        public static DataSet GetRerunPassRack(string strCntrCd, string strRackNo)
        {
            //조회 예) OutPut 되는 컬럼을 명시해서 호출 시 DataSet 으로 리턴하도록 만듦. 최초 컬럼은 ROW_NO 로 고정
            //Dim strSql As String = "SELECT CNTR_CD, DEVC_CD, BAR_NO, TST_DT, TST_TM, devcRsltChnl, DEVC_VAL_DIV_CD, devcRsltVal, JOB_NO, RACK_NO, HOLE_NO, DEVC_MDUL_NO, LOT_NO, MTTR_CD, MTTR_LVL_CD, RMRK_CD, RMRK_CONT, LH_JGMT_VAL, PANC_JGMT_VAL, rtstJgmtVal, apctrJgmtVal, slid1JgmtVal, slid2JgmtVal, TST_PGSTP, TRNS_DTM, REG_DTM, RGUR_ID, UPDT_DTM, UPUR_ID  FROM EQPM_INFC.T_HMTLG_TST_RSLT_MGMT WHERE 1=1 AND BAR_NO = '40028103201'"
            //Dim strCol As String = "ROW_NO, CNTR_CD, DEVC_CD, BAR_NO, TST_DT, TST_TM, devcRsltChnl, DEVC_VAL_DIV_CD, devcRsltVal, JOB_NO, RACK_NO, HOLE_NO, DEVC_MDUL_NO, LOT_NO, MTTR_CD, MTTR_LVL_CD, RMRK_CD, RMRK_CONT, LH_JGMT_VAL, PANC_JGMT_VAL, rtstJgmtVal, apctrJgmtVal, slid1JgmtVal, slid2JgmtVal, TST_PGSTP, TRNS_DTM, REG_DTM, RGUR_ID, UPDT_DTM, UPUR_ID"
            //obj.GetSetOrderQuery(strSql, strCol)

            //Insert 예)
            //Dim strSql As String = "INSERT INTO EQPM_INFC.T_HMTLG_TST_RSLT_MGMT (CNTR_CD, DEVC_CD, BAR_NO, TST_DT, TST_TM, devcRsltChnl, DEVC_VAL_DIV_CD, devcRsltVal) VALUES ('14100000', '907', '40028103201', '20240111', '115801', 'MCH', 'R', '29.6')"
            //obj.GetSetOrderQuery(strSql)

            DataSet dsData = null;
            Lis.Interface.clsBizSeeGene objApi;
            string strSql = "";
            string strCol = "";

            strSql = "SELECT CNTR_CD,DEVC_CD,RACK_NO,USE_YN,REG_DTM,RGUR_ID,UPDT_DTM,UPUR_ID";
            strSql += " FROM EQPM_INFC.T_HMTLG_RACK_MGMT";
            strSql += " WHERE 1=1";
            strSql += " AND CNTR_CD = " + Common.STS(strCntrCd);
            strSql += " AND RACK_NO = " + Common.STS(strRackNo);

            strCol = "ROW_NO,CNTR_CD,DEVC_CD,RACK_NO,USE_YN,REG_DTM,RGUR_ID,UPDT_DTM,UPUR_ID";

            objApi = new Lis.Interface.clsBizSeeGene();
            //dsData = objApi.GetDataSetOrderQuery(strSql, strCol);

            dsData = objApi.GetRerunPassRackHematology(strCntrCd);

            objApi = null;

            return dsData;
        }
    }
}