namespace CT90
{
    public class SysInfo
    {
        public struct DataBase
        {
            public static System.Data.OleDb.OleDbConnection OleDbConnection;
            public static System.Data.SqlClient.SqlConnection SqlConnection;
            public static MySql.Data.MySqlClient.MySqlConnection MySqlConnection;
        }
    }
}