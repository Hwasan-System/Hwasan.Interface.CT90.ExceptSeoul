using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace CT90
{
    public class TrayManagement
    {
        private static string mstrAppPath = Directory.GetCurrentDirectory() + "\\";
        private static string mstrDateTimeFormat = "yyyyMMdd-HH";
        private readonly string _connectionString;

        public TrayManagement(string dbPath)
        {
            _connectionString = string.Format("Data Source={0}", dbPath);
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    // 테이블 생성 - TraySequence는 단일 레코드만 사용
                    command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS TraySequence (
                        current_sequence INTEGER NOT NULL,
                        last_updated DATETIME DEFAULT CURRENT_TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS TrayInfo (
                        tray_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        tray_barcode TEXT NOT NULL,
                        tray_no INTEGER NOT NULL,
                        sequence_number INTEGER NOT NULL,
                        is_active INTEGER DEFAULT 1,
                        created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                        deactivated_date DATETIME
                    );

                    CREATE TABLE IF NOT EXISTS SampleStorage (
                        storage_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        tray_id INTEGER,
                        sample_barcode TEXT NOT NULL,
                        position INTEGER NOT NULL CHECK (position BETWEEN 1 AND 125),
                        storage_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                        removal_date DATETIME,
                        FOREIGN KEY (tray_id) REFERENCES TrayInfo(tray_id)
                    );";
                    command.ExecuteNonQuery();

                    // 초기 시퀀스 확인 및 삽입 - 단일 레코드 확인
                    command.CommandText = "SELECT COUNT(*) FROM TraySequence";
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count == 0)
                    {
                        command.CommandText = "INSERT INTO TraySequence (current_sequence) VALUES (0)";
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public int RegisterNewTray(string trayBarcode, int trayNo)
        {
            if (trayBarcode == "000037" && trayNo == 2)
            {
                trayBarcode = trayBarcode;
            }

            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int existingTrayId = -1;

                        // Check if there's an active tray with the same trayNo
                        using (SQLiteCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                        SELECT tray_id, tray_barcode 
                        FROM TrayInfo 
                        WHERE tray_no = $trayNo AND is_active = 1";
                            command.Parameters.AddWithValue("$trayNo", trayNo);

                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    existingTrayId = reader.GetInt32(0);
                                    string existingBarcode = reader.GetString(1);

                                    // If an active tray with the same trayNo but different barcode exists, deactivate it
                                    if (existingBarcode != trayBarcode)
                                    {
                                        reader.Close(); // Close the reader before executing another command

                                        using (SQLiteCommand deactivateCommand = connection.CreateCommand())
                                        {
                                            deactivateCommand.CommandText = @"
                                        UPDATE TrayInfo 
                                        SET is_active = 0, 
                                            deactivated_date = CURRENT_TIMESTAMP 
                                        WHERE tray_id = $existingTrayId";
                                            deactivateCommand.Parameters.AddWithValue("$existingTrayId", existingTrayId);
                                            deactivateCommand.ExecuteNonQuery();
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        // Same trayNo and same barcode means we can skip re-registering
                                        transaction.Commit();
                                        return existingTrayId; // Return the existing tray's sequence
                                    }
                                }
                            }
                        }

                        //2024-11-13 : active 가 꺼져 있으면 다시 활성화
                        using (SQLiteCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                        SELECT tray_id, tray_barcode 
                        FROM TrayInfo 
                        WHERE tray_no = $trayNo AND is_active = 0 AND tray_barcode = $tray_barcode";

                            command.Parameters.AddWithValue("$trayNo", trayNo);
                            command.Parameters.AddWithValue("$tray_barcode", trayBarcode);

                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    existingTrayId = reader.GetInt32(0);
                                    string existingBarcode = reader.GetString(1);

                                    // If an active tray with the same trayNo but different barcode exists, deactivate it
                                    if (existingBarcode == trayBarcode)
                                    {
                                        reader.Close(); // Close the reader before executing another command
                                        break;
                                    }
                                }
                            }
                        }

                        // Retrieve and increment sequence
                        using (SQLiteCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT current_sequence FROM TraySequence";
                            int currentSequence = Convert.ToInt32(command.ExecuteScalar());
                            int nextSequence = (currentSequence >= 999) ? 1 : currentSequence + 1;

                            // 기존 데이터 확인
                            command.CommandText = "SELECT COUNT(*) FROM TrayInfo WHERE tray_barcode = $barcode";
                            command.Parameters.AddWithValue("$barcode", trayBarcode);

                            int existingCount = Convert.ToInt32(command.ExecuteScalar());
                            if (existingCount > 0)
                            {
                                // 로그 기록
                                Console.WriteLine($"Warning: Existing tray with barcode {trayBarcode} will be replaced.");

                                // 기존 SampleStorage 데이터 삭제
                                command.CommandText = @"
                            DELETE FROM SampleStorage 
                            WHERE tray_id IN (
                                SELECT tray_id 
                                FROM TrayInfo 
                                WHERE tray_barcode = $barcode
                            )";
                                command.ExecuteNonQuery();

                                // 기존 TrayInfo 데이터 삭제
                                command.CommandText = "DELETE FROM TrayInfo WHERE tray_barcode = $barcode";
                                command.ExecuteNonQuery();
                            }

                            // 새로운 Tray 등록
                            command.CommandText = @"
                        INSERT INTO TrayInfo (tray_barcode, tray_no, sequence_number)
                        VALUES ($barcode, $trayNo, $sequence)";

                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("$barcode", trayBarcode);
                            command.Parameters.AddWithValue("$trayNo", trayNo);
                            command.Parameters.AddWithValue("$sequence", nextSequence);
                            command.ExecuteNonQuery();

                            // Update the tray sequence
                            command.CommandText = @"
                        UPDATE TraySequence 
                        SET current_sequence = $sequence,
                            last_updated = CURRENT_TIMESTAMP";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("$sequence", nextSequence);
                            command.ExecuteNonQuery();

                            transaction.Commit();

                            return nextSequence;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error registering new tray: {ex.Message}");
                        throw;
                    }
                }
            }
        }


        public bool StoreSample(string trayBarcode, string sampleBarcode, int position)
        {
            if (position < 1 || position > 125)
            {
                throw new ArgumentException("Position must be between 1 and 125");
            }

            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    // 트레이 ID 조회 및 유효성 검사
                    command.CommandText = @"
                    SELECT tray_id FROM TrayInfo 
                    WHERE tray_barcode = $barcode AND is_active = 1";
                    command.Parameters.AddWithValue("$barcode", trayBarcode);
                    object trayId = command.ExecuteScalar();

                    if (trayId == null)
                    {
                        return false;
                    }

                    // 위치 중복 검사
                    command.CommandText = @"
                    SELECT COUNT(*) FROM SampleStorage
                    WHERE tray_id = $trayId AND position = $position AND removal_date IS NULL";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("$trayId", trayId);
                    command.Parameters.AddWithValue("$position", position);
                    int count = Convert.ToInt32(command.ExecuteScalar());

                    if (count > 0)
                    {
                        return false;
                    }

                    // 샘플 저장
                    command.CommandText = @"
                    INSERT INTO SampleStorage (tray_id, sample_barcode, position)
                    VALUES ($trayId, $sampleBarcode, $position)";
                    command.Parameters.AddWithValue("$sampleBarcode", sampleBarcode);
                    command.ExecuteNonQuery();

                    return true;
                }
            }
        }

        public void DeactivateTray(string trayBarcode)
        {
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                    UPDATE TrayInfo
                    SET is_active = 0,
                        deactivated_date = CURRENT_TIMESTAMP
                    WHERE tray_barcode = $barcode";
                    command.Parameters.AddWithValue("$barcode", trayBarcode);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<int> GetAvailablePositions(string trayBarcode)
        {
            List<int> availablePositions = new List<int>();
            HashSet<int> usedPositions = new HashSet<int>();

            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                    SELECT position
                    FROM SampleStorage s
                    JOIN TrayInfo t ON s.tray_id = t.tray_id
                    WHERE t.tray_barcode = $barcode
                    AND s.removal_date IS NULL";
                    command.Parameters.AddWithValue("$barcode", trayBarcode);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usedPositions.Add(reader.GetInt32(0));
                        }
                    }
                }
            }

            // 사용 가능한 위치 계산
            for (int i = 1; i <= 125; i++)
            {
                if (!usedPositions.Contains(i))
                {
                    availablePositions.Add(i);
                }
            }

            return availablePositions;
        }

        public class TrayInfo
        {
            public string TrayBarcode { get; set; }
            public int SequenceNumber { get; set; }
            public int CurrentSamples { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public List<TrayInfo> GetActiveTrayInfo()
        {
            List<TrayInfo> result = new List<TrayInfo>();

            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                    SELECT 
                        t.tray_barcode,
                        t.sequence_number,
                        COUNT(CASE WHEN s.removal_date IS NULL THEN 1 END) as current_samples,
                        t.created_date
                    FROM TrayInfo t
                    LEFT JOIN SampleStorage s ON t.tray_id = s.tray_id
                    WHERE t.is_active = 1
                    GROUP BY t.tray_id";

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TrayInfo trayInfo = new TrayInfo
                            {
                                TrayBarcode = reader.GetString(0),
                                SequenceNumber = reader.GetInt32(1),
                                CurrentSamples = reader.GetInt32(2),
                                CreatedDate = reader.GetDateTime(3)
                            };
                            result.Add(trayInfo);
                        }
                    }
                }
            }

            return result;
        }

        public void ClearAllData()
        {
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    // 외래 키 관계를 고려한 삭제 순서
                    command.CommandText = @"
                DELETE FROM SampleStorage;
                DELETE FROM TrayInfo;
                DELETE FROM TraySequence;
                
                -- TraySequence 테이블 초기화 (시퀀스 0으로 리셋)
                INSERT INTO TraySequence (current_sequence) VALUES (0);
                
                -- SQLite의 auto-increment 값 리셋
                DELETE FROM sqlite_sequence WHERE name IN ('SampleStorage', 'TrayInfo');";

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
