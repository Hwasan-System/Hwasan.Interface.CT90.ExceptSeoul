using System.Collections.Generic;
using System.Data;

namespace CT90
{
    public class Constant
    {
        //2025-03-19 : 사무실에서 작업할 때 true 로 처리
        //public const bool DEV_IN_OFFICE = true;
        public const bool DEV_IN_OFFICE = false;

        //public const bool DEBUG_MODE = true;
        public const bool DEBUG_MODE = false;
        public const bool TS_ONLY = false;
        public const bool TS_INCLUDE = false;

        public static string SG_TS_EQP_CD = "";

        //서울
        //public const string SG_TS_EQP_CD = "620";
        //public const string SG_TS_EQP_CD = "621";

        //부산 BS
        //public const string SG_TS_EQP_CD = "039";

        //대구 DK
        //public const string SG_TS_EQP_CD = "922";

        //광주 GH
        //public const string SG_TS_EQP_CD = "842";

        //대전 DJ
        //public const string SG_TS_EQP_CD = "724";

        public static DataTable gdtInquirySpcNoList = new DataTable("CT90");
        public static string gstrServerIp = "";
        public static string gstrDatabaseName = "";
        public static string gstrServerIpTS10 = "";     //TS-10 검체 아카이브 이력 정보는 공용으로 사용
        public static string gstrDatabaseNameTS10 = ""; //TS-10 검체 아카이브 이력 정보는 공용으로 사용
        public static string gstrCenterCode = "";       //센터코드
        public static string gstrComputerName = "";
        public static bool gblnLoggingTimeStamp = false;
        public static List<string> glstNextStepRack = new List<string>();
        public static List<string> glstA1C = new List<string>();
        public static Dictionary<string, string> gdicPtInfo = new Dictionary<string, string>();
        public static Dictionary<string, string> gdicSortInformation = new Dictionary<string, string>();
        public static Dictionary<string, string> gdctSortingRules = new Dictionary<string, string>();
        public static List<string> glstNoOrdByPB = new List<string>();

        public const string HOSP_CD = "01";
        public const string EQP_CD = "CT90";
        public const string DLM_HS = "ː";
        public const int LEN_SPCNO = 15;

        public enum EGridColumn
        {
            SEQ = 0,
            TYPE = 1,
            DATETIME = 2,
            EQUIPMENT = 3,
            SPCNO = 4,
            RACK = 5,
            POS = 6,
            RESULT = 7,
        }

        public enum DBTypes
        {
            Oracle,
            SQLServer,
            MDB,
        }
    }
}