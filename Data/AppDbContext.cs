using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using CourseWork.Models;
using System.Reflection.Metadata;
using System.Linq;
using CourseWork.Helpers;
using CourseWork.Controls;
using System.Text.Json;

namespace CourseWork.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper()
        {
            _connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123;SearchPath=public";
        }

        private RecentDocument MapReader(NpgsqlDataReader reader)
        {
            return new RecentDocument
            {
                Id = reader.GetInt32(0),
                DocumentTypeId = reader.GetInt32(1),
                DocumentType = reader.GetString(2),
                Number = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                MakingDateAndTime = reader.GetDateTime(4),
                CitizenName = reader.IsDBNull(5) ? null : reader.GetString(5)
            };
        }

        
        
        public async Task<UserWithRole?> AuthenticateUserWithRoleAsync(string username, string password)
        {
            try
            {
                Console.WriteLine($"[AUTH] Попытка входа: username={username}");
                
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                
                Console.WriteLine($"[AUTH] Подключение к БД открыто");

                var sql = @"
                    SELECT id, username, last_name, first_name, patronymic, COALESCE(role, 1) as role, password
                    FROM users
                    WHERE username = @username";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);

                await using var reader = await cmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    Console.WriteLine($"[AUTH] Пользователь найден в БД");
                    
                    var storedHash = reader.GetString(6);
                    bool passwordValid = PasswordHelper.VerifyPassword(password, storedHash);
                    
                    Console.WriteLine($"[AUTH] Проверка пароля: {passwordValid}");
                    
                    if (passwordValid)
                    {
                        var user = new UserWithRole
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            LastName = reader.GetString(2),
                            FirstName = reader.GetString(3),
                            Patronymic = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Role = (UserRole)reader.GetInt32(5)
                        };
                        Console.WriteLine($"[AUTH] Успех! UserId={user.Id}, Role={user.Role}");
                        return user;
                    }
                }
                else
                {
                    Console.WriteLine($"[AUTH] Пользователь '{username}' не найден");
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUTH ERROR] {ex.Message}");
                Console.WriteLine($"[AUTH ERROR] {ex.StackTrace}");
                throw; // Пробрасываем дальше для отображения в UI
            }
        }

        public async Task<List<RecentDocument>> GetAllDocumentsAsync(UserRole role, int userId)
        {
            var documents = new List<RecentDocument>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = "";

            switch (role)
            {
                case UserRole.MedicalExpert:
                    sql = @"
                        SELECT 
                            mer.id_medical_examination_report AS id,
                            4 AS type_id,
                            'Направление на мед. освид.' AS type_name,
                            mer.number,
                            mer.making_date_and_time AS making_date,
                            c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                        FROM medical_examination_report mer
                        LEFT JOIN citizens c ON mer.patient = c.id_citizens
                        
                        UNION ALL
                        
                        SELECT 
                            mec.id_medical_examination_certificate AS id,
                            6 AS type_id,
                            'Акт медицинского освидетельствования' AS type_name,
                            mec.number,
                            mec.making_date_and_time AS making_date,
                            c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                        FROM medical_examination_certificate mec
                        LEFT JOIN medical_examination_report mer ON mec.medical_examination_report = mer.id_medical_examination_report
                        LEFT JOIN citizens c ON mer.patient = c.id_citizens
                        
                        ORDER BY making_date DESC
                        LIMIT 100";
                    break;

                case UserRole.PoliceOfficer:
                    sql = @"
                        SELECT id, type_id, type_name, number, making_date, citizen_name FROM (
                            -- Обращения
                            SELECT 
                                a.id_appeals AS id,
                                2 AS type_id,
                                'Обращение' AS type_name,
                                a.number,
                                a.making_date_and_time AS making_date,
                                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                            FROM appeals a
                            JOIN citizens c ON a.appeal_citizen = c.id_citizens
                            WHERE a.police_officer = @userId
                            
                            UNION ALL
                            
                            -- Заявления
                            SELECT 
                                s.id_statement AS id,
                                1 AS type_id,
                                'Заявление' AS type_name,
                                s.number,
                                s.date_and_time AS making_date,
                                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                            FROM statement s
                            JOIN citizens c ON s.applicant = c.id_citizens
                            WHERE s.police_officer = @userId
                            
                            UNION ALL
                            
                            -- Протоколы объяснения
                            SELECT 
                                ep.id_explanation_protocol AS id,
                                3 AS type_id,
                                'Протокол объяснения' AS type_name,
                                ep.number,
                                ep.making_date_and_time AS making_date,
                                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                            FROM explanation_protocol ep
                            JOIN citizens c ON ep.citizen = c.id_citizens
                            JOIN deal d ON ep.deal = d.id_deal
                            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                            
                            UNION ALL
                            
                            -- Направления на мед. освид.
                            SELECT 
                                mer.id_medical_examination_report AS id,
                                4 AS type_id,
                                'Направление на мед. освид.' AS type_name,
                                mer.number,
                                mer.making_date_and_time AS making_date,
                                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                            FROM medical_examination_report mer
                            JOIN citizens c ON mer.patient = c.id_citizens
                            JOIN deal d ON mer.deal = d.id_deal
                            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                            
                            UNION ALL
                            
                            -- Административные протоколы
                            SELECT 
                                ap.id_protocol AS id,
                                5 AS type_id,
                                'Административный протокол' AS type_name,
                                ap.protocol_number AS number,
                                ap.making_date_and_time AS making_date,
                                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                            FROM administrative_protocol ap
                            JOIN deal d ON ap.deal = d.id_deal
                            JOIN citizens c ON d.offender = c.id_citizens
                            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                        ) AS docs
                        ORDER BY making_date DESC
                        LIMIT 100";
                    break;

                case UserRole.ForensicExpert:
                    sql = @"
                        SELECT 
                            fe.id_forensic_medical_examination AS id,
                            7 AS type_id,
                            'Судебно-медицинская экспертиза' AS type_name,
                            fe.number AS number,
                            fe.making_date_and_time AS making_date,
                            c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                        FROM forensic_medical_examination fe
                        LEFT JOIN deal d ON fe.deal = d.id_deal
                        LEFT JOIN citizens c ON d.offender = c.id_citizens
                        WHERE fe.expert = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                        ORDER BY fe.making_date_and_time DESC
                        LIMIT 100";
                    break;

                case UserRole.Judge:
                    sql = @"
                        SELECT 
                            r.id_resolution AS id,
                            8 AS type_id,
                            'Постановление' AS type_name,
                            r.protocol_number AS number,
                            r.making_date_and_time AS making_date,
                            c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                        FROM resolution r
                        LEFT JOIN deal d ON r.deal = d.id_deal
                        LEFT JOIN citizens c ON d.offender = c.id_citizens
                        ORDER BY r.making_date_and_time DESC
                        LIMIT 100";
                    break;
            }

            await using var cmd = new NpgsqlCommand(sql, conn);
            if (role == UserRole.PoliceOfficer || role == UserRole.ForensicExpert)
            {
                cmd.Parameters.AddWithValue("@userId", userId);
            }

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                documents.Add(new RecentDocument
                {
                    Id = reader.GetInt32(0),
                    DocumentTypeId = reader.GetInt32(1),
                    DocumentType = reader.GetString(2),
                    Number = reader.IsDBNull(3) ? null : reader.GetInt32(3),     // ← безопасное чтение
                    MakingDateAndTime = reader.GetDateTime(4),
                    CitizenName = reader.IsDBNull(5) ? null : reader.GetString(5)  // ← безопасное чтение
                });
            }
            return documents;
        }























































































        
        
        
        public async Task<List<RecentDocument>> GetFavoriteDocumentsAsync(int userId)
        {
            var documents = new List<RecentDocument>();
            
            // Отладка: начало метода
            Console.WriteLine($"[DEBUG] GetFavoriteDocumentsAsync вызван для userId={userId}");
            NotificationsControl.ShowInfo("Отладка", $"GetFavoriteDocumentsAsync: userId={userId}");
            
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
    SELECT 
        f.document_id AS id,
        CASE f.target_table
            WHEN 'statement' THEN 1
            WHEN 'appeals' THEN 2
            WHEN 'explanation_protocol' THEN 3
            WHEN 'medical_examination_report' THEN 4
            WHEN 'administrative_protocol' THEN 5
            WHEN 'medical_certificate' THEN 6
            WHEN 'forensic_medical_examination' THEN 7
            WHEN 'resolution' THEN 8
        END AS type_id,
        CASE f.target_table
            WHEN 'statement' THEN 'Заявление'
            WHEN 'appeals' THEN 'Обращение'
            WHEN 'explanation_protocol' THEN 'Протокол объяснения'
            WHEN 'medical_examination_report' THEN 'Направление на мед. освид.'
            WHEN 'administrative_protocol' THEN 'Административный протокол'
            WHEN 'medical_certificate' THEN 'Акт медицинского освидетельствования'
            WHEN 'forensic_medical_examination' THEN 'Судебно-медицинская экспертиза'
            WHEN 'resolution' THEN 'Постановление'
        END AS type_name,
        CASE f.target_table
            WHEN 'statement' THEN s.number
            WHEN 'appeals' THEN a.number
            WHEN 'explanation_protocol' THEN ep.number
            WHEN 'medical_examination_report' THEN mer.number
            WHEN 'administrative_protocol' THEN ap.protocol_number
            WHEN 'medical_certificate' THEN mc.number
            WHEN 'forensic_medical_examination' THEN fe.number
            WHEN 'resolution' THEN r.protocol_number
        END AS number,
        CASE f.target_table
            WHEN 'statement' THEN s.date_and_time
            WHEN 'appeals' THEN a.making_date_and_time
            WHEN 'explanation_protocol' THEN ep.making_date_and_time
            WHEN 'medical_examination_report' THEN mer.making_date_and_time
            WHEN 'administrative_protocol' THEN ap.making_date_and_time
            WHEN 'medical_certificate' THEN mc.making_date_and_time
            WHEN 'forensic_medical_examination' THEN fe.making_date_and_time
            WHEN 'resolution' THEN r.making_date_and_time
        END AS making_date,
        CASE f.target_table
            WHEN 'statement' THEN c_s.last_name || ' ' || c_s.first_name || ' ' || COALESCE(c_s.patronymic, '')
            WHEN 'appeals' THEN c_a.last_name || ' ' || c_a.first_name || ' ' || COALESCE(c_a.patronymic, '')
            WHEN 'explanation_protocol' THEN c_ep.last_name || ' ' || c_ep.first_name || ' ' || COALESCE(c_ep.patronymic, '')
            WHEN 'medical_examination_report' THEN c_mer.last_name || ' ' || c_mer.first_name || ' ' || COALESCE(c_mer.patronymic, '')
            WHEN 'administrative_protocol' THEN c_ap.last_name || ' ' || c_ap.first_name || ' ' || COALESCE(c_ap.patronymic, '')
            WHEN 'medical_certificate' THEN c_mc.last_name || ' ' || c_mc.first_name || ' ' || COALESCE(c_mc.patronymic, '')
            WHEN 'forensic_medical_examination' THEN c_fe.last_name || ' ' || c_fe.first_name || ' ' || COALESCE(c_fe.patronymic, '')
            WHEN 'resolution' THEN c_r.last_name || ' ' || c_r.first_name || ' ' || COALESCE(c_r.patronymic, '')
        END AS citizen_name
    FROM user_favorites f
    LEFT JOIN statement s ON f.target_table = 'statement' AND f.document_id = s.id_statement
    LEFT JOIN citizens c_s ON s.applicant = c_s.id_citizens
    LEFT JOIN appeals a ON f.target_table = 'appeals' AND f.document_id = a.id_appeals
    LEFT JOIN citizens c_a ON a.appeal_citizen = c_a.id_citizens
    LEFT JOIN explanation_protocol ep ON f.target_table = 'explanation_protocol' AND f.document_id = ep.id_explanation_protocol
    LEFT JOIN citizens c_ep ON ep.citizen = c_ep.id_citizens
    LEFT JOIN medical_examination_report mer ON f.target_table = 'medical_examination_report' AND f.document_id = mer.id_medical_examination_report
    LEFT JOIN citizens c_mer ON mer.patient = c_mer.id_citizens
    LEFT JOIN administrative_protocol ap ON f.target_table = 'administrative_protocol' AND f.document_id = ap.id_protocol
    LEFT JOIN deal d ON ap.deal = d.id_deal
    LEFT JOIN citizens c_ap ON d.offender = c_ap.id_citizens
    LEFT JOIN medical_examination_certificate mc ON f.target_table = 'medical_certificate' AND f.document_id = mc.id_medical_examination_certificate
    LEFT JOIN medical_examination_report mer_mc ON mc.medical_examination_report = mer_mc.id_medical_examination_report
    LEFT JOIN citizens c_mc ON mer_mc.patient = c_mc.id_citizens
    LEFT JOIN forensic_medical_examination fe ON f.target_table = 'forensic_medical_examination' AND f.document_id = fe.id_forensic_medical_examination
    LEFT JOIN deal d_fe ON fe.deal = d_fe.id_deal
    LEFT JOIN citizens c_fe ON d_fe.offender = c_fe.id_citizens
    LEFT JOIN resolution r ON f.target_table = 'resolution' AND f.document_id = r.id_resolution
    LEFT JOIN deal d_r ON r.deal = d_r.id_deal
    LEFT JOIN citizens c_r ON d_r.offender = c_r.id_citizens
    WHERE f.user_id = @userId";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            
            // Отладка: SQL запрос
            Console.WriteLine($"[DEBUG] SQL запрос: {sql}");
            
            await using var reader = await cmd.ExecuteReaderAsync();
            
            // Отладка: проверка наличия данных
            int rowCount = 0;
            while (await reader.ReadAsync())
            {
                rowCount++;
                var docId = reader.GetInt32(0);
                var docType = reader.GetString(2);
                var docNumber = reader.IsDBNull(3) ? "NULL" : reader.GetInt32(3).ToString();
                var docDate = reader.GetDateTime(4);
                var citizenName = reader.IsDBNull(5) ? "NULL" : reader.GetString(5);
                
                // Отладка: каждый документ
                NotificationsControl.ShowInfo("Отладка", 
                    $"Найден документ в избранном:\n" +
                    $"ID: {docId}\n" +
                    $"Тип: {docType}\n" +
                    $"Номер: {docNumber}\n" +
                    $"Дата: {docDate}\n" +
                    $"Гражданин: {citizenName}");
                
                documents.Add(new RecentDocument
                {
                    Id = docId,
                    DocumentTypeId = reader.GetInt32(1),
                    DocumentType = docType,
                    Number = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    MakingDateAndTime = docDate,
                    CitizenName = citizenName
                });
            }
            
            // Отладка: итог
            NotificationsControl.ShowInfo("Отладка", 
                $"GetFavoriteDocumentsAsync завершён:\n" +
                $"userId={userId}\n" +
                $"Найдено документов: {rowCount}");
            
            Console.WriteLine($"[DEBUG] GetFavoriteDocumentsAsync: userId={userId}, найдено документов={rowCount}");
            
            return documents;
        }
        
        
        
        public async Task<Citizen?> GetCitizenByIdAsync(int citizenId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    c.id_citizens,
                    c.last_name,
                    c.first_name,
                    c.patronymic,
                    c.birthday,
                    c.address_registration,
                    c.passport_series_and_number,
                    (SELECT phone_number FROM citizen_phones WHERE citizen = c.id_citizens AND is_primary = true LIMIT 1) AS phone,
                    s.name as working_place_name,
                    e.education as education_name,
                    fs.family_status as family_status_name,
                    cit.citizenship as citizenship_name,
                    c.criminal_record,
                    c.count_record,
                    p.post as post_name
                FROM citizens c
                LEFT JOIN structures s ON c.working_place = s.id_structures
                LEFT JOIN education e ON c.education = e.id_education
                LEFT JOIN family_status fs ON c.family_status = fs.id_family_status
                LEFT JOIN citizenship cit ON c.citizenship = cit.id_citizenship
                LEFT JOIN post p ON c.post = p.id_post
                WHERE c.id_citizens = @citizenId";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("citizenId", citizenId);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Citizen
                {
                    Id = reader.GetInt32(0),
                    LastName = reader.GetString(1),
                    FirstName = reader.GetString(2),
                    Patronymic = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Birthday = reader.GetDateTime(4),
                    Address = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Passport = reader.IsDBNull(6) ? null : reader.GetString(6),       // ← passport на индексе 6
                    Phone = reader.IsDBNull(7) ? null : reader.GetString(7),          // ← phone на индексе 7
                    WorkingPlaceName = reader.IsDBNull(8) ? null : reader.GetString(8),
                    EducationName = reader.IsDBNull(9) ? null : reader.GetString(9),
                    FamilyStatusName = reader.IsDBNull(10) ? null : reader.GetString(10),
                    CitizenshipName = reader.IsDBNull(11) ? null : reader.GetString(11),
                    CriminalRecord = reader.GetBoolean(12),
                    CountRecord = reader.IsDBNull(13) ? null : reader.GetInt32(13),
                    PostName = reader.IsDBNull(14) ? null : reader.GetString(14),

                    
                    // Эти поля не используются, но пусть будут
                    WorkingPlace = null,
                    Education = null,
                    FamilyStatus = null,
                    Citizenship = null,
                };
                
            }   
            return null;
        }
        
        public async Task<List<RecentDocument>> SearchDocumentsByUserAsync(string searchText, int userId)
        {
            var documents = new List<RecentDocument>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT v.id, v.type_id, v.type_name, v.number, v.making_date, v.citizen_name
                FROM view_recent_documents v
                WHERE 
                    (v.citizen_name ILIKE @search OR v.number::TEXT ILIKE @search OR v.type_name ILIKE @search)
                    AND (
                        (v.type_id = 1 AND EXISTS (
                            SELECT 1 FROM statement s
                            JOIN citizens_and_posts cap ON s.police_officer = cap.id_citizens_and_posts
                            JOIN citizens c ON cap.citizen = c.id_citizens
                            WHERE s.id_statement = v.id 
                            AND c.last_name = (SELECT last_name FROM users WHERE id = @userId)
                            AND c.first_name = (SELECT first_name FROM users WHERE id = @userId)
                        ))
                        OR
                        (v.type_id = 2 AND EXISTS (
                            SELECT 1 FROM appeals a
                            JOIN citizens_and_posts cap ON a.police_officer = cap.id_citizens_and_posts
                            JOIN citizens c ON cap.citizen = c.id_citizens
                            WHERE a.id_appeals = v.id 
                            AND c.last_name = (SELECT last_name FROM users WHERE id = @userId)
                            AND c.first_name = (SELECT first_name FROM users WHERE id = @userId)
                        ))
                        OR
                        (v.type_id = 3 AND EXISTS (
                            SELECT 1 FROM explanation_protocol ep
                            JOIN deal d ON ep.deal = d.id_deal
                            JOIN citizens_and_posts cap ON d.police_officer = cap.id_citizens_and_posts
                            JOIN citizens c ON cap.citizen = c.id_citizens
                            WHERE ep.id_explanation_protocol = v.id 
                            AND c.last_name = (SELECT last_name FROM users WHERE id = @userId)
                            AND c.first_name = (SELECT first_name FROM users WHERE id = @userId)
                        ))
                        OR
                        (v.type_id = 5 AND EXISTS (
                            SELECT 1 FROM administrative_protocol ap
                            JOIN deal d ON ap.deal = d.id_deal
                            JOIN citizens_and_posts cap ON d.police_officer = cap.id_citizens_and_posts
                            JOIN citizens c ON cap.citizen = c.id_citizens
                            WHERE ap.id_protocol = v.id 
                            AND c.last_name = (SELECT last_name FROM users WHERE id = @userId)
                            AND c.first_name = (SELECT first_name FROM users WHERE id = @userId)
                        ))
                    )
                ORDER BY v.making_date DESC 
                LIMIT 50";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
            
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                documents.Add(MapReader(reader));

            return documents;
        }
                
        public async Task<List<RecentDocument>> GetRecentDocumentsByUserAsync(int userId)
        {
            var documents = new List<RecentDocument>();
            
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql = @"
                    SELECT id, type_id, type_name, number, making_date, citizen_name
                    FROM view_recent_documents 
                    ORDER BY making_date DESC 
                    LIMIT 50";

                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    documents.Add(new RecentDocument
                    {
                        Id = reader.GetInt32(0),
                        DocumentTypeId = reader.GetInt32(1),
                        DocumentType = reader.GetString(2),
                        Number = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                        MakingDateAndTime = reader.GetDateTime(4),
                        CitizenName = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
                
                Console.WriteLine($"[DEBUG] GetRecentDocumentsByUserAsync: загружено {documents.Count} документов");
                return documents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetRecentDocumentsByUserAsync: {ex.Message}");
                return documents;
            }
        }

        
        public async Task ToggleFavoriteAsync(int userId, string targetTable, int documentId)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string checkSql = "SELECT id FROM user_favorites WHERE user_id = @userId AND target_table = @targetTable AND document_id = @documentId";
                await using var checkCmd = new NpgsqlCommand(checkSql, conn);
                checkCmd.Parameters.AddWithValue("@userId", userId);
                checkCmd.Parameters.AddWithValue("@targetTable", targetTable);
                checkCmd.Parameters.AddWithValue("@documentId", documentId);
                
                var exists = await checkCmd.ExecuteScalarAsync();

                if (exists != null)
                {
                    string deleteSql = "DELETE FROM user_favorites WHERE user_id = @userId AND target_table = @targetTable AND document_id = @documentId";
                    await using var deleteCmd = new NpgsqlCommand(deleteSql, conn);
                    deleteCmd.Parameters.AddWithValue("@userId", userId);
                    deleteCmd.Parameters.AddWithValue("@targetTable", targetTable);
                    deleteCmd.Parameters.AddWithValue("@documentId", documentId);
                    int deleted = await deleteCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"[DEBUG] Удалено из избранного: {deleted} записей");
                }
                else
                {   
                    string insertSql = "INSERT INTO user_favorites (user_id, target_table, document_id, created_at) VALUES (@userId, @targetTable, @documentId, @createdAt)";
                    await using var insertCmd = new NpgsqlCommand(insertSql, conn);
                    insertCmd.Parameters.AddWithValue("@userId", userId);
                    insertCmd.Parameters.AddWithValue("@targetTable", targetTable);
                    insertCmd.Parameters.AddWithValue("@documentId", documentId);
                    insertCmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
                    int inserted = await insertCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"[DEBUG] Добавлено в избранное: {inserted} записей");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ToggleFavoriteAsync: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        
        
        
        public async Task<bool> IsFavoriteAsync(int userId, string targetTable, int documentId)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql = "SELECT COUNT(*) FROM user_favorites WHERE user_id = @userId AND target_table = @targetTable AND document_id = @documentId";
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@targetTable", targetTable);
                cmd.Parameters.AddWithValue("@documentId", documentId);

                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                Console.WriteLine($"[DEBUG] IsFavoriteAsync: count={count}");
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] IsFavoriteAsync: {ex.Message}");
                throw;
            }
        }

        

        
        
        



        public async Task<int> CreateStatementAsync(int applicantId, string content, 
        int policeOfficerId, int? number = null, DateTime? makingDate = null, 
        bool signatureApplicant = false, bool signatureOfficer = false)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            DateTime dateToUse = makingDate ?? DateTime.Now;

            var sql = @"INSERT INTO statement (applicant, content, date_and_time, police_officer, number, signature_applicant, signature_police_officer) 
                    VALUES (@applicant, @content, @dateTime, @officer, @number, @signApplicant, @signOfficer) 
                    RETURNING id_statement";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@applicant", applicantId);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.Parameters.AddWithValue("@dateTime", dateToUse);
            cmd.Parameters.AddWithValue("@officer", policeOfficerId);
            cmd.Parameters.AddWithValue("@number", number ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@signApplicant", signatureApplicant);
            cmd.Parameters.AddWithValue("@signOfficer", signatureOfficer);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
                
        
        

       public async Task<int> CreateAppealAsync(int citizenId, string content, int userId, int? number = null, DateTime? makingDate = null)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            int? policeOfficerId = await GetCitizensAndPostsIdByUserIdAsync(userId);
            
            if (policeOfficerId == null)
            {
                throw new Exception($"Сотрудник с user_id={userId} не найден в таблице citizens_and_posts");
            }

            DateTime dateToUse = makingDate ?? DateTime.Now;

            var sql = @"INSERT INTO appeals (appeal_citizen, content, making_date_and_time, police_officer, number) 
                        VALUES (@citizen, @content, @makingDate, @officer, @number) 
                        RETURNING id_appeals";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@citizen", citizenId);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.Parameters.AddWithValue("@makingDate", dateToUse);
            cmd.Parameters.AddWithValue("@officer", policeOfficerId.Value);
            cmd.Parameters.AddWithValue("@number", number ?? (object)DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        
        
        
        public async Task<int> CreateExplanationProtocolAsync(int citizenId, int dealId, string content, int? number = null)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO explanation_protocol (citizen, deal, making_date_and_time, content, number, 
                       need_forensic_medical_examination, need_medical_examination_certificate) 
                       VALUES (@citizen, @deal, NOW(), @content, @number, FALSE, FALSE) 
                       RETURNING id_explanation_protocol";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@citizen", citizenId);
            cmd.Parameters.AddWithValue("@deal", dealId);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.Parameters.AddWithValue("@number", number ?? (object)DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        
    


        public async Task<int> CreateAdministrativeProtocolAsync(int dealId, int protocolNumber, string description, string otherInfo, int firstWitnessId, int? secondWitnessId = null)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO administrative_protocol 
                        (protocol_number, making_date_and_time, deal, description, other_information, first_witness, second_witness) 
                        VALUES (@protocolNumber, NOW(), @dealId, @description, @otherInfo, @witness1, @witness2) 
                        RETURNING id_protocol";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@protocolNumber", protocolNumber);
            cmd.Parameters.AddWithValue("@dealId", dealId);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.Parameters.AddWithValue("@otherInfo", otherInfo);
            cmd.Parameters.AddWithValue("@witness1", firstWitnessId);
            cmd.Parameters.AddWithValue("@witness2", secondWitnessId ?? (object)DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        

        
        
        
        public async Task<Dictionary<string, object?>> GetStatementDetailsAsync(int id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM statement WHERE id_statement = @id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var data = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                    data[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                return data;
            }
            return new Dictionary<string, object?>();
        }

        
        
        
        public async Task<Dictionary<string, object?>> GetAppealDetailsAsync(int id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM appeals WHERE id_appeals = @id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var data = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                    data[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                return data;
            }
            return new Dictionary<string, object?>();
        }

        
        
        
        public async Task<Dictionary<string, object?>> GetProtocolDetailsAsync(int id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT * FROM administrative_protocol WHERE id_protocol = @id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var data = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                    data[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                return data;
            }
            return new Dictionary<string, object?>();
        }

        

        
        
        
        public async Task<Dictionary<int, string>> GetDocumentTypesAsync()
        {
            var types = new Dictionary<int, string>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT id, document_type FROM documents_type";
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                types[reader.GetInt32(0)] = reader.GetString(1);

            return types;
        }

        
        
        
        public async Task<int?> GetDocumentTypeIdByNameAsync(string typeName)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT id FROM documents_type WHERE document_type = @name";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", typeName);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : null;
        }
        

        public async Task<List<Appeal>> GetRecentAppealsAsync(int userId)
        {
            var appeals = new List<Appeal>();
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                
                var countSql = "SELECT COUNT(*) FROM appeals";
                await using var countCmd = new NpgsqlCommand(countSql, conn);
                var totalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                Console.WriteLine($"[DB] Всего обращений в таблице: {totalCount}");

                
                var sql = @"
                    SELECT 
                        a.id_appeals,
                        a.number,
                        a.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        a.content
                    FROM appeals a
                    JOIN citizens c ON a.appeal_citizen = c.id_citizens
                    WHERE a.police_officer = @userId
                    ORDER BY a.making_date_and_time DESC
                    LIMIT 50";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                
                Console.WriteLine($"[DB] Выполняется запрос для пользователя ID: {userId}");
                
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var appeal = new Appeal
                    {
                        Id = reader.GetInt32(0),
                        Number = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                        CreatedAt = reader.GetDateTime(2),
                        CitizenFullName = reader.GetString(3),
                        Content = reader.GetString(4)
                    };
                    appeals.Add(appeal);
                }
                
                Console.WriteLine($"[DB] Запрос вернул {appeals.Count} записей");
                return appeals;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] {ex.Message}");
                Console.WriteLine($"[DB ERROR] {ex.StackTrace}");
                throw; 
            }
        }

        public async Task<List<Appeal>> GetFavouriteAppealsAsync(int userId)
        {
            var appeals = new List<Appeal>();
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql = @"
                    SELECT 
                        a.id_appeals,
                        a.number,
                        a.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        a.content
                    FROM appeals a
                    JOIN citizens c ON a.appeal_citizen = c.id_citizens
                    JOIN user_favorites f ON f.target_table = 'appeals' AND f.document_id = a.id_appeals
                    WHERE f.user_id = @userId
                    ORDER BY a.making_date_and_time DESC";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    appeals.Add(new Appeal
                    {
                        Id = reader.GetInt32(0),
                        Number = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                        CreatedAt = reader.GetDateTime(2),
                        CitizenFullName = reader.GetString(3),
                        Content = reader.GetString(4)
                    });
                }

                return appeals;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetFavouriteAppealsAsync: {ex.Message}");
                throw;
            }
        }





        public async Task<int> SaveDraftAsync(int userId, string documentType, string formDataJson)
        {
            Console.WriteLine($"[DEBUG] SaveDraftAsync: userId={userId}");
            

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            var checkSql = "SELECT COUNT(*) FROM users WHERE id = @userId";
            await using var checkCmd = new NpgsqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@userId", userId);
            var result = await checkCmd.ExecuteScalarAsync();
            var exists = result != null ? (long)result : 0;
            
            Console.WriteLine($"[DEBUG] Пользователь с id={userId} существует: {exists > 0}");
            
            if (exists == 0)
            {
    
                var insertUserSql = @"
                    INSERT INTO users (id, username, password, last_name, first_name) 
                    VALUES (@userId, 'user' || @userId, '123', 'User', 'User')
                    ON CONFLICT (id) DO NOTHING";
                await using var insertCmd = new NpgsqlCommand(insertUserSql, conn);
                insertCmd.Parameters.AddWithValue("@userId", userId);
                await insertCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[DEBUG] Создан пользователь с id={userId}");
            }
            
            var sql = @"
                INSERT INTO drafts (user_id, document_type, form_data, updated_at)
                VALUES (@userId, @docType, @formData::jsonb, NOW())
                RETURNING id_draft";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("docType", documentType);
            cmd.Parameters.AddWithValue("formData", formDataJson);

            result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }


        public async Task UpdateDraftAsync(int draftId, string formDataJson)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                UPDATE drafts 
                SET form_data = @formData::jsonb, updated_at = NOW()
                WHERE id_draft = @draftId";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("draftId", draftId);
            cmd.Parameters.AddWithValue("formData", formDataJson);

            await cmd.ExecuteNonQueryAsync();
        }



        public async Task<List<Draft>> GetDraftsAsync(int userId)
        {
            var drafts = new List<Draft>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT id_draft, user_id, document_type, form_data, created_at, updated_at
                FROM drafts
                WHERE user_id = @userId
                ORDER BY updated_at DESC";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("userId", userId);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var formData = reader.IsDBNull(3) ? "{}" : reader.GetString(3);
                
                var draft = new Draft
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    DocumentType = reader.GetString(2),
                    FormDataJson = formData,
                    CreatedAt = reader.GetDateTime(4),
                    UpdatedAt = reader.GetDateTime(5)
                };
                
        
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(formData);
                    var root = doc.RootElement;
                    
            
                    if (root.TryGetProperty("appeal_citizen", out var citizenProp) && 
                        citizenProp.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        draft.CitizenId = citizenProp.GetInt32();
                    }
                    
            
                    if (root.TryGetProperty("number", out var numberProp) && 
                        numberProp.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        draft.Number = numberProp.GetString();
                    }
                    
            
                    DateTime? documentDate = null;
                    if (root.TryGetProperty("making_date", out var dateProp) && 
                        dateProp.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        if (DateTime.TryParse(dateProp.GetString(), out var date))
                        {
                            documentDate = date;
                            if (root.TryGetProperty("making_time", out var timeProp) && 
                                timeProp.ValueKind != System.Text.Json.JsonValueKind.Null)
                            {
                                if (TimeSpan.TryParse(timeProp.GetString(), out var time))
                                {
                                    documentDate = date.Date + time;
                                }
                            }
                        }
                    }
                    draft.DocumentDate = documentDate;
                    
            
                    if (root.TryGetProperty("content", out var contentProp) && 
                        contentProp.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        draft.Content = contentProp.GetString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Parsing draft {draft.Id}: {ex.Message}");
                }
                
        
                draft.Preview = ExtractPreview(formData, draft.DocumentType);
                
        
                draft.ProgressPercent = CalculateProgress(draft);
                
                drafts.Add(draft);
            }

            return drafts;
        }



        public async Task DeleteDraftAsync(int draftId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM drafts WHERE id_draft = @draftId";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("draftId", draftId);
            
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"[DEBUG] Удалён черновик ID: {draftId}, затронуто строк: {rowsAffected}");
        }


        public async Task<Draft?> GetDraftByIdAsync(int draftId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            var sql = "SELECT * FROM drafts WHERE id_draft = @draftId";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("draftId", draftId);
            await using var reader = await cmd.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new Draft
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    DocumentType = reader.GetString(2),
                    FormDataJson = reader.IsDBNull(3) ? "{}" : reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    UpdatedAt = reader.GetDateTime(5)
                };
            }
            return null;
        }


       
        private string ExtractPreview(string json, string documentType)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                
                // Пытаемся получить номер документа
                string number = "";
                if (root.TryGetProperty("number", out var numProp) && numProp.ValueKind != JsonValueKind.Null)
                    number = numProp.GetString();
                else if (root.TryGetProperty("protocol_number", out var protocolProp) && protocolProp.ValueKind != JsonValueKind.Null)
                    number = protocolProp.GetString();
                
                // Пытаемся получить ФИО гражданина
                string citizenName = "";
                if (root.TryGetProperty("citizen_name", out var citizenProp) && citizenProp.ValueKind != JsonValueKind.Null)
                    citizenName = citizenProp.GetString();
                else if (root.TryGetProperty("patient_name", out var patientProp) && patientProp.ValueKind != JsonValueKind.Null)
                    citizenName = patientProp.GetString();
                else if (root.TryGetProperty("applicant_name", out var applicantProp) && applicantProp.ValueKind != JsonValueKind.Null)
                    citizenName = applicantProp.GetString();
                
                // Формируем превью
                if (!string.IsNullOrWhiteSpace(number) && !string.IsNullOrWhiteSpace(citizenName))
                    return $"№{number} - {citizenName}";
                else if (!string.IsNullOrWhiteSpace(number))
                    return $"№{number}";
                else if (!string.IsNullOrWhiteSpace(citizenName))
                    return citizenName;
                
                return "(нет данных)";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ExtractPreview: {ex.Message}");
                return "(ошибка чтения)";
            }
        }

        private int? ExtractCitizenId(string json)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("appeal_citizen", out var prop) ||
                    doc.RootElement.TryGetProperty("applicant", out prop) ||
                    doc.RootElement.TryGetProperty("citizen_id", out prop))
                {
                    if (prop.TryGetInt32(out int id) && id > 0)
                        return id;
                }
            }
            catch { }
            return null;
        }

        



        public int CalculateProgress(Draft draft)
        {
            var fields = new List<bool>();
            
            switch (draft.DocumentType)
            {
                case "appeals":
                    fields.Add(draft.CitizenId != null);
                    fields.Add(!string.IsNullOrWhiteSpace(draft.Number));
                    fields.Add(draft.DocumentDate != null);
                    fields.Add(!string.IsNullOrWhiteSpace(draft.Content));
                    break;
                    
                case "statement":
                    fields.Add(draft.ApplicantId != null);
                    fields.Add(!string.IsNullOrWhiteSpace(draft.Number));
                    fields.Add(draft.DocumentDate != null);
                    fields.Add(!string.IsNullOrWhiteSpace(draft.Content));
                    fields.Add(draft.SignatureApplicant != null);
                    fields.Add(draft.SignatureOfficer != null);
                    break;
                    
                case "administrative_protocol":
                    fields.Add(draft.DealId != null);
                    fields.Add(!string.IsNullOrWhiteSpace(draft.ProtocolNumber));
                    fields.Add(draft.DocumentDate != null);
                    fields.Add(!string.IsNullOrWhiteSpace(draft.Description));
                    fields.Add(!string.IsNullOrWhiteSpace(draft.OtherInfo));
                    fields.Add(draft.Witness1Id != null);
                    fields.Add(draft.Witness2Id != null);
                    break;
                    
                case "examination_report":
                    fields.Add(draft.DealId != null);
                    fields.Add(draft.ReportTypeId != null);
                    fields.Add(!string.IsNullOrWhiteSpace(draft.Content));
                    fields.Add(!string.IsNullOrWhiteSpace(draft.Signs));
                    fields.Add(!string.IsNullOrWhiteSpace(draft.Number));
                    break;
                    
                case "explanation_protocol":
                    fields.Add(draft.CitizenId != null);
                    fields.Add(draft.DealId != null);
                    fields.Add(!string.IsNullOrWhiteSpace(draft.Content));
                    fields.Add(!string.IsNullOrWhiteSpace(draft.Number));
                    fields.Add(draft.NeedMedicalExamination != null);
                    fields.Add(draft.NeedCertificate != null);
                    break;
                    
                default:
                    return 0;
            }
            
            int filled = fields.Count(f => f);
            int total = fields.Count;
            
            if (total == 0) return 0;
            
            return (filled * 100) / total;
        }







        public async Task<List<Statement>> GetStatementsByUserAsync(int userId)
        {
            var statements = new List<Statement>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    s.id_statement,
                    s.number,
                    s.applicant,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS applicant_name,
                    s.content,
                    s.date_and_time,
                    s.police_officer,
                    u.last_name || ' ' || u.first_name AS officer_name,
                    s.signature_applicant,
                    s.signature_police_officer
                FROM statement s
                JOIN citizens c ON s.applicant = c.id_citizens
                JOIN users u ON s.police_officer = u.id
                WHERE s.police_officer = @userId
                ORDER BY s.date_and_time DESC
                LIMIT 100";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                statements.Add(new Statement
                {
                    Id = reader.GetInt32(0),
                    Number = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    ApplicantId = reader.GetInt32(2),
                    ApplicantFullName = reader.GetString(3),
                    Content = reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5),
                    OfficerId = reader.GetInt32(6),
                    OfficerFullName = reader.GetString(7),
                    SignatureApplicant = reader.GetBoolean(8),
                    SignaturePoliceOfficer = reader.GetBoolean(9)
                });
            }

            return statements;
        }




        public async Task<List<Statement>> GetFavoriteStatementsAsync(int userId)
        {
            var statements = new List<Statement>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    s.id_statement,
                    s.number,
                    s.applicant,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS applicant_name,
                    s.content,
                    s.date_and_time,
                    s.police_officer,
                    u.last_name || ' ' || u.first_name AS officer_name,
                    s.signature_applicant,
                    s.signature_police_officer
                FROM statement s
                JOIN citizens c ON s.applicant = c.id_citizens
                JOIN users u ON s.police_officer = u.id
                JOIN user_favorites f ON f.target_table = 'statement' AND f.document_id = s.id_statement
                WHERE f.user_id = @userId
                ORDER BY s.date_and_time DESC";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                statements.Add(new Statement
                {
                    Id = reader.GetInt32(0),
                    Number = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    ApplicantId = reader.GetInt32(2),
                    ApplicantFullName = reader.GetString(3),
                    Content = reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5),
                    OfficerId = reader.GetInt32(6),
                    OfficerFullName = reader.GetString(7),
                    SignatureApplicant = reader.GetBoolean(8),
                    SignaturePoliceOfficer = reader.GetBoolean(9)
                });
            }

            return statements;
        }



        public async Task<List<Deal>> GetDealsAsync()
        {
            var deals = new List<Deal>();   
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    d.id_deal,
                    d.deal_number,
                    d.making_date,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_full_name
                FROM deal d
                LEFT JOIN citizens c ON d.offender = c.id_citizens
                ORDER BY d.deal_number DESC";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                deals.Add(new Deal
                {
                    Id = reader.GetInt32(0),
                    Number = reader.IsDBNull(1) ? "Б/Н" : reader.GetInt32(1).ToString(),
                    DealDate = reader.GetDateTime(2),
                    CitizenFullName = reader.IsDBNull(3) ? "Неизвестно" : reader.GetString(3)
                });
            }

            return deals;
        }


        public async Task<List<Deal>> SearchDealsAsync(DealSearchParams searchParams)
        {
            var deals = new List<Deal>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();

    
            if (!string.IsNullOrWhiteSpace(searchParams.DealNumber))
            {
                conditions.Add("CAST(d.deal_number AS TEXT) ILIKE @dealNumber");
                parameters.Add("@dealNumber", $"%{searchParams.DealNumber}%");
            }

    
            if (!string.IsNullOrWhiteSpace(searchParams.FullName))
            {
                conditions.Add("(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '')) ILIKE @fullname");
                parameters.Add("@fullname", $"%{searchParams.FullName}%");
            }

    
            if (searchParams.DateFrom.HasValue)
            {
                conditions.Add("d.making_date >= @dateFrom");
                parameters.Add("@dateFrom", searchParams.DateFrom.Value);
            }

            if (searchParams.DateTo.HasValue)
            {
                conditions.Add("d.making_date <= @dateTo");
                parameters.Add("@dateTo", searchParams.DateTo.Value);
            }

            string whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";

            var sql = $@"
                SELECT 
                    d.id_deal,
                    d.deal_number,
                    d.making_date,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name
                FROM deal d
                LEFT JOIN citizens c ON d.offender = c.id_citizens
                {whereClause}
                ORDER BY d.making_date DESC
                LIMIT 100";

            await using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                deals.Add(new Deal
                {
                    Id = reader.GetInt32(0),
                    Number = reader.GetInt32(1).ToString(),
                    DealDate = reader.GetDateTime(2),
                    CitizenFullName = reader.IsDBNull(3) ? "Неизвестно" : reader.GetString(3)
                });
            }

            return deals;
        }


        public async Task<int> CreateMedicalExaminationReportAsync(
    int patientId, int? dealId, string reportType, string content, string signs,
    int? number, DateTime makingDateTime, bool citizenSig, bool officerSig)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            int reportTypeId = await GetReportTypeIdByNameAsync(reportType);
            
            if (reportTypeId == 0)
            {
                throw new Exception($"Не найден тип освидетельствования: {reportType}");
            }

            var sql = @"
                INSERT INTO medical_examination_report 
                (patient, deal, report, content, signs_of_intoxication, 
                number, making_date_and_time, citizen_signature, officer_signature)
                VALUES (@patient, @deal, @report, @content, @signs, 
                        @number, @makingDateTime, @citizenSig, @officerSig)
                RETURNING id_medical_examination_report";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("patient", patientId);
            cmd.Parameters.AddWithValue("deal", dealId is null ? (object)DBNull.Value : dealId.Value);
            cmd.Parameters.AddWithValue("report", reportTypeId);
            cmd.Parameters.AddWithValue("content", content);
            cmd.Parameters.AddWithValue("signs", signs);
            cmd.Parameters.AddWithValue("number", number is null ? (object)DBNull.Value : number.Value);
            cmd.Parameters.AddWithValue("makingDateTime", makingDateTime);
            cmd.Parameters.AddWithValue("citizenSig", citizenSig);
            cmd.Parameters.AddWithValue("officerSig", officerSig);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        private async Task<int> GetReportTypeIdByNameAsync(string reportTypeName)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            reportTypeName = reportTypeName.Trim();
            
            Console.WriteLine($"[DEBUG] Ищем тип: '{reportTypeName}'");

            var sql = "SELECT id_type_report FROM type_report WHERE type_report = @name";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", reportTypeName);
            
            var result = await cmd.ExecuteScalarAsync();
            
            if (result == null)
            {
                var likeSql = "SELECT id_type_report, type_report FROM type_report WHERE type_report ILIKE @name";
                await using var likeCmd = new NpgsqlCommand(likeSql, conn);
                likeCmd.Parameters.AddWithValue("@name", $"%{reportTypeName}%");
                
                await using var reader = await likeCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int id = reader.GetInt32(0);
                    string foundName = reader.GetString(1);
                    Console.WriteLine($"[DEBUG] Найдено по LIKE: '{foundName}' -> ID={id}");
                    return id;
                }
                
                Console.WriteLine($"[ERROR] Тип '{reportTypeName}' не найден в БД");
                return 0;
            }
            
            int foundId = Convert.ToInt32(result);
            Console.WriteLine($"[DEBUG] Найден ID={foundId}");
            return foundId;
        }



        public async Task<List<ArticleItem>> GetArticlesAsync()
        {
            var items = new List<ArticleItem>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT id_article, CAST(number_of_article AS TEXT), description FROM article ORDER BY number_of_article";
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new ArticleItem
                {
                    Id = reader.GetInt32(0),
                    Number = reader.GetString(1),
                    Description = reader.GetString(2)
                });
            }
            return items;
        }

        public async Task<List<PostItem>> GetPostsAsync()
        {
            var items = new List<PostItem>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT id_post, post FROM post ORDER BY post";
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new PostItem
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1)
                });
            }
            return items;
        }

        public async Task<List<StructureItem>> GetStructuresAsync()
        {
            var items = new List<StructureItem>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT s.id_structures, s.name, st.title_of_settlements, s.description 
                FROM structures s
                JOIN settlements st ON s.settlement = st.id_settlements
                ORDER BY s.name";
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new StructureItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Settlement = reader.GetString(2),
                    Description = reader.GetString(3)
                });
            }
            return items;
        }




        public async Task<List<ArticleItem>> SearchArticlesAsync(string searchText)
        {
            var items = new List<ArticleItem>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

    
            var sql = @"SELECT id_article, CAST(number_of_article AS TEXT), description 
                        FROM article 
                        WHERE CAST(number_of_article AS TEXT) ILIKE @search OR description ILIKE @search
                        ORDER BY number_of_article";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new ArticleItem
                {
                    Id = reader.GetInt32(0),
                    Number = reader.GetString(1),
                    Description = reader.GetString(2)
                });
            }
            return items;
        }


        public async Task<List<PostItem>> SearchPostsAsync(string searchText)
        {
            var items = new List<PostItem>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT id_post, post FROM post WHERE post ILIKE @search ORDER BY post";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new PostItem
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1)
                });
            }
            return items;
        }


        public async Task<List<StructureItem>> SearchStructuresAsync(string searchText)
        {
            var items = new List<StructureItem>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT s.id_structures, s.name, st.title_of_settlements, s.description 
                FROM structures s
                JOIN settlements st ON s.settlement = st.id_settlements
                WHERE s.name ILIKE @search OR s.description ILIKE @search OR st.title_of_settlements ILIKE @search
                ORDER BY s.name";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new StructureItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Settlement = reader.GetString(2),
                    Description = reader.GetString(3)
                });
            }
            return items;
        }


        public async Task<List<Citizen>> SearchCitizensAsync(CitizenSearchParams searchParams)
        {
            var citizens = new List<Citizen>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(searchParams.FullName))
            {
                conditions.Add(@"(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '')) ILIKE @fullname");
                parameters.Add("@fullname", $"%{searchParams.FullName}%");
            }

            if (!string.IsNullOrWhiteSpace(searchParams.LastName))
            {
                conditions.Add("c.last_name ILIKE @lastName");
                parameters.Add("@lastName", $"%{searchParams.LastName}%");
            }

            if (!string.IsNullOrWhiteSpace(searchParams.FirstName))
            {
                conditions.Add("c.first_name ILIKE @firstName");
                parameters.Add("@firstName", $"%{searchParams.FirstName}%");
            }

            if (searchParams.Birthday.HasValue)
            {
                conditions.Add("c.birthday = @birthday");
                parameters.Add("@birthday", searchParams.Birthday.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchParams.Address))
            {
                conditions.Add("c.address_registration ILIKE @address");
                parameters.Add("@address", $"%{searchParams.Address}%");
            }

            if (!string.IsNullOrWhiteSpace(searchParams.Phone))
            {
                conditions.Add(@"EXISTS (SELECT 1 FROM citizen_phones cp 
                                WHERE cp.citizen = c.id_citizens AND cp.phone_number ILIKE @phone)");
                parameters.Add("@phone", $"%{searchParams.Phone}%");
            }

            if (!string.IsNullOrWhiteSpace(searchParams.Passport))
            {
                conditions.Add("c.passport_series_and_number ILIKE @passport");
                parameters.Add("@passport", $"%{searchParams.Passport}%");
            }

            string whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";

            var sql = $@"
                SELECT
                    c.id_citizens,
                    c.last_name,
                    c.first_name,
                    c.patronymic,
                    c.birthday,
                    c.address_registration,
                    c.passport_series_and_number,
                    (SELECT cp.phone_number FROM citizen_phones cp WHERE cp.citizen = c.id_citizens AND cp.is_primary = TRUE LIMIT 1) AS phone,
                    COALESCE(s.name, 'Не указано') AS working_place,
                    COALESCE(e.education, 'Не указано') AS education,
                    COALESCE(fs.family_status, 'Не указано') AS family_status,
                    COALESCE(cit.citizenship, 'Не указано') AS citizenship,
                    COALESCE(p.post, 'Не указана') AS post_name
                FROM citizens c
                LEFT JOIN structures s ON c.working_place = s.id_structures
                LEFT JOIN education e ON c.education = e.id_education
                LEFT JOIN family_status fs ON c.family_status = fs.id_family_status
                LEFT JOIN citizenship cit ON c.citizenship = cit.id_citizenship
                LEFT JOIN post p ON c.post = p.id_post
                {whereClause}
                ORDER BY c.last_name, c.first_name
                LIMIT 100";

            await using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // Внутри SearchCitizensAsync, в блоке citizens.Add(...):
                citizens.Add(new Citizen
                {
                    Id = reader.GetInt32(0),
                    LastName = reader.GetString(1),
                    FirstName = reader.GetString(2),
                    Patronymic = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Birthday = reader.GetDateTime(4),
                    Address = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Passport = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Phone = reader.IsDBNull(7) ? null : reader.GetString(7),
                    WorkingPlaceName = reader.IsDBNull(8) ? null : reader.GetString(8),
                    EducationName = reader.IsDBNull(9) ? null : reader.GetString(9),
                    FamilyStatusName = reader.IsDBNull(10) ? null : reader.GetString(10),
                    CitizenshipName = reader.IsDBNull(11) ? null : reader.GetString(11),
                    PostName = reader.IsDBNull(12) ? null : reader.GetString(12),
                });
            }

            return citizens;
        }

        public async Task<List<ExternalDocument>> GetExternalDocumentsAsync(int? dealId, int? citizenId = null)
        {
            var documents = new List<ExternalDocument>();
            
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            int dealIdValue = dealId ?? -1;

            var sql = @"
                -- Обращения
                SELECT 
                    a.id_appeals AS id,
                    'appeals' AS table_name,
                    'Обращение' AS document_type,
                    COALESCE(a.number::text, 'Б/Н') AS number,
                    COALESCE(a.making_date_and_time, NOW()) AS created_at,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                    '' AS deal_number,
                    c.id_citizens AS citizen_id,
                    NULL::int AS deal_id
                FROM public.appeals a
                LEFT JOIN public.citizens c ON a.appeal_citizen = c.id_citizens
                WHERE a.police_officer != (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                
                UNION ALL
                
                -- Заявления
                SELECT 
                    s.id_statement AS id,
                    'statement' AS table_name,
                    'Заявление' AS document_type,
                    COALESCE(s.number::text, 'Б/Н') AS number,
                    COALESCE(s.date_and_time, NOW()) AS created_at,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                    '' AS deal_number,
                    c.id_citizens AS citizen_id,
                    NULL::int AS deal_id
                FROM public.statement s
                LEFT JOIN public.citizens c ON s.applicant = c.id_citizens
                WHERE s.police_officer != (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                
                UNION ALL
                
                -- Протоколы объяснения
                SELECT 
                    ep.id_explanation_protocol AS id,
                    'explanation_protocol' AS table_name,
                    'Протокол объяснения' AS document_type,
                    COALESCE(ep.number::text, 'Б/Н') AS number,
                    COALESCE(ep.making_date_and_time, NOW()) AS created_at,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                    COALESCE(d.deal_number::text, '') AS deal_number,
                    c.id_citizens AS citizen_id,
                    d.id_deal AS deal_id
                FROM public.explanation_protocol ep
                INNER JOIN public.deal d ON ep.deal = d.id_deal
                LEFT JOIN public.citizens c ON ep.citizen = c.id_citizens
                WHERE (@dealId = -1 OR d.id_deal = @dealId)
                
                UNION ALL
                
                -- Административные протоколы
                SELECT 
                    ap.id_protocol AS id,
                    'administrative_protocol' AS table_name,
                    'Административный протокол' AS document_type,
                    COALESCE(ap.protocol_number::text, 'Б/Н') AS number,
                    COALESCE(ap.making_date_and_time, NOW()) AS created_at,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                    COALESCE(d.deal_number::text, '') AS deal_number,
                    c.id_citizens AS citizen_id,
                    d.id_deal AS deal_id
                FROM public.administrative_protocol ap
                INNER JOIN public.deal d ON ap.deal = d.id_deal
                INNER JOIN public.citizens c ON d.offender = c.id_citizens
                WHERE (@dealId = -1 OR d.id_deal = @dealId)
                
                UNION ALL
                
                -- Направления на мед. освидетельствование
                SELECT 
                    mer.id_medical_examination_report AS id,
                    'medical_examination_report' AS table_name,
                    'Направление на мед. освид.' AS document_type,
                    COALESCE(mer.number::text, 'Б/Н') AS number,
                    COALESCE(mer.making_date_and_time, NOW()) AS created_at,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                    COALESCE(d.deal_number::text, '') AS deal_number,
                    c.id_citizens AS citizen_id,
                    d.id_deal AS deal_id
                FROM public.medical_examination_report mer
                INNER JOIN public.deal d ON mer.deal = d.id_deal
                LEFT JOIN public.citizens c ON mer.patient = c.id_citizens
                WHERE (@dealId = -1 OR d.id_deal = @dealId)
                
                UNION ALL
                
                -- Акты медицинского освидетельствования
                SELECT 
                    mec.id_medical_examination_certificate AS id,
                    'medical_certificate' AS table_name,
                    'Акт медицинского освидетельствования' AS document_type,
                    COALESCE(mec.number::text, 'Б/Н') AS number,
                    COALESCE(mec.making_date_and_time, NOW()) AS created_at,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                    COALESCE(d.deal_number::text, '') AS deal_number,
                    c.id_citizens AS citizen_id,
                    d.id_deal AS deal_id
                FROM public.medical_examination_certificate mec
                INNER JOIN public.medical_examination_report mer ON mec.medical_examination_report = mer.id_medical_examination_report
                INNER JOIN public.deal d ON mer.deal = d.id_deal
                LEFT JOIN public.citizens c ON mer.patient = c.id_citizens
                WHERE (@dealId = -1 OR d.id_deal = @dealId)
                
                UNION ALL
                
                -- Судебно-медицинские экспертизы
                SELECT 
                    fe.id_forensic_medical_examination AS id,
                    'forensic_medical_examination' AS table_name,
                    'Судебно-медицинская экспертиза' AS document_type,
                    COALESCE(fe.number::text, 'Б/Н') AS number,
                    COALESCE(fe.making_date_and_time, NOW()) AS created_at,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                    COALESCE(d.deal_number::text, '') AS deal_number,
                    c.id_citizens AS citizen_id,
                    d.id_deal AS deal_id
                FROM public.forensic_medical_examination fe
                INNER JOIN public.deal d ON fe.deal = d.id_deal
                LEFT JOIN public.citizens c ON d.offender = c.id_citizens
                WHERE (@dealId = -1 OR d.id_deal = @dealId)
                
                UNION ALL
                
                -- Постановления
                SELECT 
                    r.id_resolution AS id,
                    'resolution' AS table_name,
                    'Постановление' AS document_type,
                    COALESCE(r.protocol_number::text, 'Б/Н') AS number,
                    COALESCE(r.making_date_and_time, NOW()) AS created_at,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                    COALESCE(d.deal_number::text, '') AS deal_number,
                    c.id_citizens AS citizen_id,
                    d.id_deal AS deal_id
                FROM public.resolution r
                INNER JOIN public.deal d ON r.deal = d.id_deal
                LEFT JOIN public.citizens c ON d.offender = c.id_citizens
                WHERE (@dealId = -1 OR d.id_deal = @dealId)
                
                ORDER BY created_at DESC";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dealId", dealIdValue);
            cmd.Parameters.AddWithValue("@userId", App.CurrentUserId);
            
            await using var reader = await cmd.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var number = reader.IsDBNull(3) ? "Б/Н" : reader.GetString(3);
                
                documents.Add(new ExternalDocument
                {
                    Id = reader.GetInt32(0),
                    TableName = reader.GetString(1),
                    DocumentType = reader.GetString(2),
                    Number = number,
                    MaskedNumber = number.Length > 4 ? "***" + number.Substring(number.Length - 4) : number,
                    CreatedAt = reader.GetDateTime(4),
                    CitizenFullName = reader.IsDBNull(5) ? "Неизвестно" : reader.GetString(5),
                    DealInfo = reader.IsDBNull(6) ? "" : $"Дело №{reader.GetString(6)}",
                    CitizenId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    DealId = reader.IsDBNull(8) ? null : reader.GetInt32(8)
                });
            }

            return documents;
        }
        private string MaskDocumentNumber(string number)
    {
        if (string.IsNullOrEmpty(number) || number == "Б/Н") return "Б/Н";
        if (number.Length <= 4) return number;
        return "***" + number.Substring(number.Length - 4);
    }

        private string MaskNumber(string number)
        {
            if (string.IsNullOrEmpty(number) || number.Length <= 4)
                return "***";
            return number.Substring(0, 2) + "***" + number.Substring(number.Length - 2);
        }


        public async Task SaveDocumentAccessRequestAsync(int userId, string tableName, int documentId, string reason)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO document_access_requests (user_id, table_name, document_id, reason, request_date)
                VALUES (@userId, @tableName, @documentId, @reason, NOW())";
            
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@tableName", tableName);
            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@reason", reason);
            
            await cmd.ExecuteNonQueryAsync();
        }


        public async Task<int?> GetCitizensAndPostsIdByUserIdAsync(int userId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT citizen_post_id 
                FROM user_citizen_link 
                WHERE user_id = @userId 
                LIMIT 1";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : null;
        }



















































public async Task<List<MyDocument>> GetUserDocumentsAsync(int userId)
{
    var documents = new List<MyDocument>();
    await using var conn = new NpgsqlConnection(_connectionString);
    await conn.OpenAsync();

    var role = App.CurrentUserRole;

    // Для врача - только медицинские документы (все, не только свои)
    if (role == UserRole.MedicalExpert)
    {
        // Направления на мед. освидетельствование
        var reportsSql = @"
            SELECT 
                mer.id_medical_examination_report,
                mer.number,
                mer.making_date_and_time,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                COALESCE(mer.content, '') as content,
                c.id_citizens,
                'medical_examination_report' as table_name,
                'Направление на мед. освид.' as document_type,
                false as is_favorite,
                NULL as deal_number
            FROM medical_examination_report mer
            JOIN citizens c ON mer.patient = c.id_citizens";

        await using var cmd = new NpgsqlCommand(reportsSql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = reader.GetInt32(0),
                DocumentType = reader.GetString(7),
                TableName = reader.GetString(6),
                Number = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                CreatedAt = reader.GetDateTime(2),
                CitizenFullName = reader.GetString(3),
                Content = reader.GetString(4),
                CitizenId = reader.GetInt32(5),
                IsFavorite = false,
                DealNumber = null
            });
        }
        await reader.CloseAsync();

        // Акты медицинского освидетельствования
        var certificatesSql = @"
            SELECT 
                mec.id_medical_examination_certificate,
                mec.number,
                mec.making_date_and_time,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                COALESCE(mec.signs_of_intoxication, '') as content,
                c.id_citizens,
                'medical_certificate' as table_name,
                'Акт медицинского освидетельствования' as document_type,
                false as is_favorite,
                NULL as deal_number
            FROM medical_examination_certificate mec
            JOIN medical_examination_report mer ON mec.medical_examination_report = mer.id_medical_examination_report
            JOIN citizens c ON mer.patient = c.id_citizens";

        cmd.CommandText = certificatesSql;
        await using var reader2 = await cmd.ExecuteReaderAsync();
        
        while (await reader2.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = reader2.GetInt32(0),
                DocumentType = reader2.GetString(7),
                TableName = reader2.GetString(6),
                Number = reader2.IsDBNull(1) ? null : reader2.GetInt32(1),
                CreatedAt = reader2.GetDateTime(2),
                CitizenFullName = reader2.GetString(3),
                Content = reader2.GetString(4),
                CitizenId = reader2.GetInt32(5),
                IsFavorite = false,
                DealNumber = null
            });
        }
        
        return documents.OrderByDescending(d => d.CreatedAt).ToList();
    }

    // Для судьи - все документы из всех таблиц
    if (role == UserRole.Judge)
    {
        // Обращения
        var appealsSql = @"
            SELECT 
                a.id_appeals as id,
                'Обращение' as document_type,
                'appeals' as table_name,
                a.number,
                a.making_date_and_time as created_at,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') as citizen_full_name,
                a.content,
                c.id_citizens,
                NULL as deal_number
            FROM appeals a
            JOIN citizens c ON a.appeal_citizen = c.id_citizens";

        await using var cmd = new NpgsqlCommand(appealsSql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = reader.GetInt32(0),
                DocumentType = reader.GetString(1),
                TableName = reader.GetString(2),
                Number = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                CreatedAt = reader.GetDateTime(4),
                CitizenFullName = reader.GetString(5),
                Content = reader.GetString(6),
                CitizenId = reader.GetInt32(7),
                DealNumber = reader.IsDBNull(8) ? null : reader.GetString(8),
                IsFavorite = false
            });
        }
        await reader.CloseAsync();

        // Заявления
        var statementsSql = @"
            SELECT 
                s.id_statement as id,
                'Заявление' as document_type,
                'statement' as table_name,
                s.number,
                s.date_and_time as created_at,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') as citizen_full_name,
                s.content,
                c.id_citizens,
                NULL as deal_number
            FROM statement s
            JOIN citizens c ON s.applicant = c.id_citizens";

        cmd.CommandText = statementsSql;
        await using var reader2 = await cmd.ExecuteReaderAsync();
        
        while (await reader2.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = reader2.GetInt32(0),
                DocumentType = reader2.GetString(1),
                TableName = reader2.GetString(2),
                Number = reader2.IsDBNull(3) ? null : reader2.GetInt32(3),
                CreatedAt = reader2.GetDateTime(4),
                CitizenFullName = reader2.GetString(5),
                Content = reader2.GetString(6),
                CitizenId = reader2.GetInt32(7),
                DealNumber = reader2.IsDBNull(8) ? null : reader2.GetString(8),
                IsFavorite = false
            });
        }
        await reader2.CloseAsync();

        // Административные протоколы
        var protocolsSql = @"
            SELECT 
                ap.id_protocol as id,
                'Административный протокол' as document_type,
                'administrative_protocol' as table_name,
                ap.protocol_number as number,
                ap.making_date_and_time as created_at,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') as citizen_full_name,
                ap.description as content,
                c.id_citizens,
                d.deal_number::text as deal_number
            FROM administrative_protocol ap
            JOIN deal d ON ap.deal = d.id_deal
            JOIN citizens c ON d.offender = c.id_citizens";

        cmd.CommandText = protocolsSql;
        await using var reader3 = await cmd.ExecuteReaderAsync();
        
        while (await reader3.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = reader3.GetInt32(0),
                DocumentType = reader3.GetString(1),
                TableName = reader3.GetString(2),
                Number = reader3.GetInt32(3),
                CreatedAt = reader3.GetDateTime(4),
                CitizenFullName = reader3.GetString(5),
                Content = reader3.GetString(6),
                CitizenId = reader3.GetInt32(7),
                DealNumber = reader3.IsDBNull(8) ? null : reader3.GetString(8),
                IsFavorite = false
            });
        }
        await reader3.CloseAsync();

        // Протоколы объяснения
        var explanationsSql = @"
            SELECT 
                ep.id_explanation_protocol as id,
                'Протокол объяснения' as document_type,
                'explanation_protocol' as table_name,
                ep.number,
                ep.making_date_and_time as created_at,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') as citizen_full_name,
                ep.content,
                c.id_citizens,
                d.deal_number::text as deal_number
            FROM explanation_protocol ep
            JOIN citizens c ON ep.citizen = c.id_citizens
            JOIN deal d ON ep.deal = d.id_deal";

        cmd.CommandText = explanationsSql;
        await using var reader4 = await cmd.ExecuteReaderAsync();
        
        while (await reader4.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = reader4.GetInt32(0),
                DocumentType = reader4.GetString(1),
                TableName = reader4.GetString(2),
                Number = reader4.IsDBNull(3) ? null : reader4.GetInt32(3),
                CreatedAt = reader4.GetDateTime(4),
                CitizenFullName = reader4.GetString(5),
                Content = reader4.GetString(6),
                CitizenId = reader4.GetInt32(7),
                DealNumber = reader4.IsDBNull(8) ? null : reader4.GetString(8),
                IsFavorite = false
            });
        }
        await reader4.CloseAsync();

        // Направления на мед. освид.
        var reportsSql = @"
            SELECT 
                mer.id_medical_examination_report as id,
                'Направление на мед. освид.' as document_type,
                'medical_examination_report' as table_name,
                mer.number,
                mer.making_date_and_time as created_at,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') as citizen_full_name,
                mer.content,
                c.id_citizens,
                d.deal_number::text as deal_number
            FROM medical_examination_report mer
            JOIN citizens c ON mer.patient = c.id_citizens
            JOIN deal d ON mer.deal = d.id_deal";

        cmd.CommandText = reportsSql;
        await using var reader5 = await cmd.ExecuteReaderAsync();
        
        while (await reader5.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = reader5.GetInt32(0),
                DocumentType = reader5.GetString(1),
                TableName = reader5.GetString(2),
                Number = reader5.IsDBNull(3) ? null : reader5.GetInt32(3),
                CreatedAt = reader5.GetDateTime(4),
                CitizenFullName = reader5.GetString(5),
                Content = reader5.GetString(6),
                CitizenId = reader5.GetInt32(7),
                DealNumber = reader5.IsDBNull(8) ? null : reader5.GetString(8),
                IsFavorite = false
            });
        }
        await reader5.CloseAsync();

        // Акты медицинского освидетельствования
        var certificatesSql = @"
            SELECT 
                mec.id_medical_examination_certificate as id,
                'Акт медицинского освидетельствования' as document_type,
                'medical_certificate' as table_name,
                mec.number,
                mec.making_date_and_time as created_at,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') as citizen_full_name,
                COALESCE(mec.signs_of_intoxication, '') as content,
                c.id_citizens,
                d.deal_number::text as deal_number
            FROM medical_examination_certificate mec
            JOIN medical_examination_report mer ON mec.medical_examination_report = mer.id_medical_examination_report
            JOIN citizens c ON mer.patient = c.id_citizens
            JOIN deal d ON mer.deal = d.id_deal";

        cmd.CommandText = certificatesSql;
        await using var reader6 = await cmd.ExecuteReaderAsync();
        
        while (await reader6.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = reader6.GetInt32(0),
                DocumentType = reader6.GetString(1),
                TableName = reader6.GetString(2),
                Number = reader6.IsDBNull(3) ? null : reader6.GetInt32(3),
                CreatedAt = reader6.GetDateTime(4),
                CitizenFullName = reader6.GetString(5),
                Content = reader6.GetString(6),
                CitizenId = reader6.GetInt32(7),
                DealNumber = reader6.IsDBNull(8) ? null : reader6.GetString(8),
                IsFavorite = false
            });
        }
        await reader6.CloseAsync();

        // Судебно-медицинские экспертизы
        var forensicSql = @"
            SELECT 
                fe.id_forensic_medical_examination as id,
                'Судебно-медицинская экспертиза' as document_type,
                'forensic_medical_examination' as table_name,
                fe.number,
                fe.making_date_and_time as created_at,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') as citizen_full_name,
                fe.content,
                c.id_citizens,
                d.deal_number::text as deal_number
            FROM forensic_medical_examination fe
            JOIN deal d ON fe.deal = d.id_deal
            JOIN citizens c ON d.offender = c.id_citizens";

        cmd.CommandText = forensicSql;
        await using var reader7 = await cmd.ExecuteReaderAsync();
        
        while (await reader7.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = reader7.GetInt32(0),
                DocumentType = reader7.GetString(1),
                TableName = reader7.GetString(2),
                Number = reader7.GetInt32(3),
                CreatedAt = reader7.GetDateTime(4),
                CitizenFullName = reader7.GetString(5),
                Content = reader7.GetString(6),
                CitizenId = reader7.GetInt32(7),
                DealNumber = reader7.IsDBNull(8) ? null : reader7.GetString(8),
                IsFavorite = false
            });
        }
        await reader7.CloseAsync();

        // Постановления
        var resolutionsSql = @"
            SELECT 
                r.id_resolution as id,
                'Постановление' as document_type,
                'resolution' as table_name,
                r.protocol_number as number,
                r.making_date_and_time as created_at,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') as citizen_full_name,
                r.resolution as content,
                c.id_citizens,
                d.deal_number::text as deal_number
            FROM resolution r
            JOIN deal d ON r.deal = d.id_deal
            JOIN citizens c ON d.offender = c.id_citizens";

        cmd.CommandText = resolutionsSql;
        await using var reader8 = await cmd.ExecuteReaderAsync();
        
        while (await reader8.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = reader8.GetInt32(0),
                DocumentType = reader8.GetString(1),
                TableName = reader8.GetString(2),
                Number = reader8.GetInt32(3),
                CreatedAt = reader8.GetDateTime(4),
                CitizenFullName = reader8.GetString(5),
                Content = reader8.GetString(6),
                CitizenId = reader8.GetInt32(7),
                DealNumber = reader8.IsDBNull(8) ? null : reader8.GetString(8),
                IsFavorite = false
            });
        }
        await reader8.CloseAsync();

        return documents.OrderByDescending(d => d.CreatedAt).ToList();
    }

    // Для полицейского и эксперта - только свои документы
    // Обращения
    var appealsSql2 = @"
        SELECT 
            a.id_appeals,
            a.number,
            a.making_date_and_time,
            c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
            a.content,
            c.id_citizens,
            'appeals' as table_name,
            'Обращение' as document_type,
            EXISTS(SELECT 1 FROM user_favorites WHERE user_id = @userId AND target_table = 'appeals' AND document_id = a.id_appeals) as is_favorite,
            NULL as deal_number
        FROM appeals a
        JOIN citizens c ON a.appeal_citizen = c.id_citizens
        WHERE a.police_officer = @userId";

    await using var cmdPolice = new NpgsqlCommand(appealsSql2, conn);
    cmdPolice.Parameters.AddWithValue("@userId", userId);
    await using var readerAppeals = await cmdPolice.ExecuteReaderAsync();

    while (await readerAppeals.ReadAsync())
    {
        documents.Add(new MyDocument
        {
            Id = readerAppeals.GetInt32(0),
            DocumentType = readerAppeals.GetString(7),
            TableName = readerAppeals.GetString(6),
            Number = readerAppeals.IsDBNull(1) ? null : readerAppeals.GetInt32(1),
            CreatedAt = readerAppeals.GetDateTime(2),
            CitizenFullName = readerAppeals.GetString(3),
            Content = readerAppeals.GetString(4),
            CitizenId = readerAppeals.GetInt32(5),
            IsFavorite = readerAppeals.GetBoolean(8),
            DealNumber = null
        });
    }
    await readerAppeals.CloseAsync();

    // Заявления
    var statementsSql2 = @"
        SELECT 
            s.id_statement,
            s.number,
            s.date_and_time,
            c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
            s.content,
            c.id_citizens,
            'statement' as table_name,
            'Заявление' as document_type,
            EXISTS(SELECT 1 FROM user_favorites WHERE user_id = @userId AND target_table = 'statement' AND document_id = s.id_statement) as is_favorite,
            NULL as deal_number
        FROM statement s
        JOIN citizens c ON s.applicant = c.id_citizens
        WHERE s.police_officer = @userId";

    cmdPolice.CommandText = statementsSql2;
    await using var readerStatements = await cmdPolice.ExecuteReaderAsync();

    while (await readerStatements.ReadAsync())
    {
        documents.Add(new MyDocument
        {
            Id = readerStatements.GetInt32(0),
            DocumentType = readerStatements.GetString(7),
            TableName = readerStatements.GetString(6),
            Number = readerStatements.IsDBNull(1) ? null : readerStatements.GetInt32(1),
            CreatedAt = readerStatements.GetDateTime(2),
            CitizenFullName = readerStatements.GetString(3),
            Content = readerStatements.GetString(4),
            CitizenId = readerStatements.GetInt32(5),
            IsFavorite = readerStatements.GetBoolean(8),
            DealNumber = null
        });
    }
    await readerStatements.CloseAsync();

    // Административные протоколы (для полицейского - только его)
    if (role == UserRole.PoliceOfficer)
    {
        var protocolsSql2 = @"
            SELECT 
                ap.id_protocol,
                ap.protocol_number,
                ap.making_date_and_time,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                ap.description as content,
                c.id_citizens,
                'administrative_protocol' as table_name,
                'Административный протокол' as document_type,
                EXISTS(SELECT 1 FROM user_favorites WHERE user_id = @userId AND target_table = 'administrative_protocol' AND document_id = ap.id_protocol) as is_favorite,
                d.deal_number::text as deal_number
            FROM administrative_protocol ap
            JOIN deal d ON ap.deal = d.id_deal
            JOIN citizens c ON d.offender = c.id_citizens
            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)";
        
        cmdPolice.CommandText = protocolsSql2;
        await using var readerProtocols = await cmdPolice.ExecuteReaderAsync();
        
        while (await readerProtocols.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = readerProtocols.GetInt32(0),
                DocumentType = readerProtocols.GetString(7),
                TableName = readerProtocols.GetString(6),
                Number = readerProtocols.GetInt32(1),
                CreatedAt = readerProtocols.GetDateTime(2),
                CitizenFullName = readerProtocols.GetString(3),
                Content = readerProtocols.GetString(4),
                CitizenId = readerProtocols.GetInt32(5),
                IsFavorite = readerProtocols.GetBoolean(8),
                DealNumber = readerProtocols.IsDBNull(9) ? null : readerProtocols.GetString(9)
            });
        }
        await readerProtocols.CloseAsync();
    }

    // Протоколы объяснения (для полицейского - только его)
    if (role == UserRole.PoliceOfficer)
    {
        var explanationsSql2 = @"
            SELECT 
                ep.id_explanation_protocol,
                ep.number,
                ep.making_date_and_time,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                ep.content,
                c.id_citizens,
                'explanation_protocol' as table_name,
                'Протокол объяснения' as document_type,
                EXISTS(SELECT 1 FROM user_favorites WHERE user_id = @userId AND target_table = 'explanation_protocol' AND document_id = ep.id_explanation_protocol) as is_favorite,
                d.deal_number::text as deal_number
            FROM explanation_protocol ep
            JOIN citizens c ON ep.citizen = c.id_citizens
            JOIN deal d ON ep.deal = d.id_deal
            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)";
        
        cmdPolice.CommandText = explanationsSql2;
        await using var readerExplanations = await cmdPolice.ExecuteReaderAsync();
        
        while (await readerExplanations.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = readerExplanations.GetInt32(0),
                DocumentType = readerExplanations.GetString(7),
                TableName = readerExplanations.GetString(6),
                Number = readerExplanations.IsDBNull(1) ? null : readerExplanations.GetInt32(1),
                CreatedAt = readerExplanations.GetDateTime(2),
                CitizenFullName = readerExplanations.GetString(3),
                Content = readerExplanations.GetString(4),
                CitizenId = readerExplanations.GetInt32(5),
                IsFavorite = readerExplanations.GetBoolean(8),
                DealNumber = readerExplanations.IsDBNull(9) ? null : readerExplanations.GetString(9)
            });
        }
        await readerExplanations.CloseAsync();
    }

    // Направления на мед. освид. (для полицейского - только его)
    if (role == UserRole.PoliceOfficer)
    {
        var reportsSql2 = @"
            SELECT 
                mer.id_medical_examination_report,
                mer.number,
                mer.making_date_and_time,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                mer.content,
                c.id_citizens,
                'medical_examination_report' as table_name,
                'Направление на мед. освид.' as document_type,
                EXISTS(SELECT 1 FROM user_favorites WHERE user_id = @userId AND target_table = 'medical_examination_report' AND document_id = mer.id_medical_examination_report) as is_favorite,
                d.deal_number::text as deal_number
            FROM medical_examination_report mer
            JOIN citizens c ON mer.patient = c.id_citizens
            JOIN deal d ON mer.deal = d.id_deal
            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)";
        
        cmdPolice.CommandText = reportsSql2;
        await using var readerReports = await cmdPolice.ExecuteReaderAsync();
        
        while (await readerReports.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = readerReports.GetInt32(0),
                DocumentType = readerReports.GetString(7),
                TableName = readerReports.GetString(6),
                Number = readerReports.IsDBNull(1) ? null : readerReports.GetInt32(1),
                CreatedAt = readerReports.GetDateTime(2),
                CitizenFullName = readerReports.GetString(3),
                Content = readerReports.GetString(4),
                CitizenId = readerReports.GetInt32(5),
                IsFavorite = readerReports.GetBoolean(8),
                DealNumber = readerReports.IsDBNull(9) ? null : readerReports.GetString(9)
            });
        }
        await readerReports.CloseAsync();
    }

    // Для эксперта - судебно-медицинские экспертизы
    if (role == UserRole.ForensicExpert)
    {
        var forensicSql2 = @"
            SELECT 
                fe.id_forensic_medical_examination,
                fe.number,
                fe.making_date_and_time,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                fe.content,
                c.id_citizens,
                'forensic_medical_examination' as table_name,
                'Судебно-медицинская экспертиза' as document_type,
                EXISTS(SELECT 1 FROM user_favorites WHERE user_id = @userId AND target_table = 'forensic_medical_examination' AND document_id = fe.id_forensic_medical_examination) as is_favorite,
                d.deal_number::text as deal_number
            FROM forensic_medical_examination fe
            JOIN deal d ON fe.deal = d.id_deal
            JOIN citizens c ON d.offender = c.id_citizens
            WHERE fe.expert = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)";
        
        cmdPolice.CommandText = forensicSql2;
        await using var readerForensic = await cmdPolice.ExecuteReaderAsync();
        
        while (await readerForensic.ReadAsync())
        {
            documents.Add(new MyDocument
            {
                Id = readerForensic.GetInt32(0),
                DocumentType = readerForensic.GetString(7),
                TableName = readerForensic.GetString(6),
                Number = readerForensic.GetInt32(1),
                CreatedAt = readerForensic.GetDateTime(2),
                CitizenFullName = readerForensic.GetString(3),
                Content = readerForensic.GetString(4),
                CitizenId = readerForensic.GetInt32(5),
                IsFavorite = readerForensic.GetBoolean(8),
                DealNumber = readerForensic.IsDBNull(9) ? null : readerForensic.GetString(9)
            });
        }
        await readerForensic.CloseAsync();
    }

    return documents.OrderByDescending(d => d.CreatedAt).ToList();
}












































        public async Task<int> RemoveFromFavoritesAsync(int userId, string targetTable, int documentId)
        {
            Console.WriteLine($"[DEBUG] RemoveFromFavorites: userId={userId}, table={targetTable}, docId={documentId}");
            
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = "DELETE FROM user_favorites WHERE user_id = @userId AND target_table = @targetTable AND document_id = @documentId";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@targetTable", targetTable);
            cmd.Parameters.AddWithValue("@documentId", documentId);
            
            int deleted = await cmd.ExecuteNonQueryAsync();
            NotificationsControl.ShowSuccess("удалено", $"удалено записей {deleted}");
            return deleted;
        }

        public async Task<DocumentFull> GetFullDocumentAsync(string tableName, int documentId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = tableName switch
            {
               "medical_certificate" => @"
                    SELECT 
                        'Акт медицинского освидетельствования' AS document_type,
                        COALESCE(mec.number::text, 'Б/Н') AS number,
                        mec.making_date_and_time AS created_at,
                        COALESCE(patient.last_name || ' ' || patient.first_name || ' ' || COALESCE(patient.patronymic, ''), 'Неизвестно') AS citizen_name,
                        COALESCE(mec.signs_of_intoxication, '') AS content,
                        COALESCE(d.deal_number::text, '') AS deal_number,
                        COALESCE(mec.signs_of_intoxication, '') AS description,
                        COALESCE(mec.result, '') AS other_information,
                        COALESCE(mec.doctor_signature, false) AS signature,
                        'Не указан' AS first_witness,
                        'Не указан' AS second_witness,
                        COALESCE(doc.last_name || ' ' || doc.first_name || ' ' || COALESCE(doc.patronymic, ''), 'Не указан') AS officer_name,
                        'Не указана' AS article_name,
                        COALESCE(patient.last_name || ' ' || patient.first_name || ' ' || COALESCE(patient.patronymic, ''), 'Неизвестно') AS patient_name,
                        COALESCE(tr.type_report, '') AS report_type,
                        COALESCE(mec.signs_of_intoxication, '') AS signs_of_intoxication
                    FROM medical_examination_certificate mec
                    LEFT JOIN medical_examination_report mer ON mec.medical_examination_report = mer.id_medical_examination_report
                    LEFT JOIN deal d ON mer.deal = d.id_deal
                    LEFT JOIN citizens patient ON mer.patient = patient.id_citizens
                    LEFT JOIN type_report tr ON mer.report = tr.id_type_report
                    LEFT JOIN citizens doc ON mec.doctor = doc.id_citizens
                    WHERE mec.id_medical_examination_certificate = @id",

                "administrative_protocol" => @"
                    SELECT 
                        'Административный протокол' AS document_type,
                        COALESCE(ap.protocol_number::text, 'Б/Н') AS number,
                        ap.making_date_and_time AS created_at,
                        COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                        ap.description AS content,
                        COALESCE(d.deal_number::text, 'Б/Н') AS deal_number,
                        ap.description AS description,
                        COALESCE(ap.other_information, '') AS other_information,
                        false AS signature,
                        COALESCE(cw1.last_name || ' ' || cw1.first_name || ' ' || COALESCE(cw1.patronymic, ''), 'Не указан') AS first_witness,
                        COALESCE(cw2.last_name || ' ' || cw2.first_name || ' ' || COALESCE(cw2.patronymic, ''), 'Не указан') AS second_witness,
                        COALESCE(officer.last_name || ' ' || officer.first_name || ' ' || COALESCE(officer.patronymic, ''), 'Не указан') AS officer_name,
                        COALESCE(a.number_of_article::text || ' - ' || a.description, 'Не указана') AS article_name,
                        '' AS patient_name,
                        '' AS report_type,
                        '' AS signs_of_intoxication
                    FROM administrative_protocol ap
                    LEFT JOIN deal d ON ap.deal = d.id_deal
                    LEFT JOIN citizens c ON d.offender = c.id_citizens
                    LEFT JOIN citizens cw1 ON ap.first_witness = cw1.id_citizens
                    LEFT JOIN citizens cw2 ON ap.second_witness = cw2.id_citizens
                    LEFT JOIN citizens_and_posts cap ON d.police_officer = cap.id_citizens_and_posts
                    LEFT JOIN citizens officer ON cap.citizen = officer.id_citizens
                    LEFT JOIN article a ON d.article = a.id_article
                    WHERE ap.id_protocol = @id",
                    
                "explanation_protocol" => @"
                    SELECT 
                        'Протокол объяснения' AS document_type,
                        COALESCE(ep.number::text, 'Б/Н') AS number,
                        ep.making_date_and_time AS created_at,
                        COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                        ep.content AS content,
                        COALESCE(d.deal_number::text, 'Б/Н') AS deal_number,
                        ep.content AS description,
                        '' AS other_information,
                        false AS signature,
                        'Не указан' AS first_witness,
                        'Не указан' AS second_witness,
                        COALESCE(officer.last_name || ' ' || officer.first_name || ' ' || COALESCE(officer.patronymic, ''), 'Не указан') AS officer_name,
                        COALESCE(a.number_of_article::text || ' - ' || a.description, 'Не указана') AS article_name,
                        '' AS patient_name,
                        '' AS report_type,
                        '' AS signs_of_intoxication
                    FROM explanation_protocol ep
                    LEFT JOIN deal d ON ep.deal = d.id_deal
                    LEFT JOIN citizens c ON ep.citizen = c.id_citizens
                    LEFT JOIN citizens_and_posts cap ON d.police_officer = cap.id_citizens_and_posts
                    LEFT JOIN citizens officer ON cap.citizen = officer.id_citizens
                    LEFT JOIN article a ON d.article = a.id_article
                    WHERE ep.id_explanation_protocol = @id",
                    
                "medical_examination_report" => @"
                    SELECT 
                        'Направление на мед. освид.' AS document_type,
                        COALESCE(mer.number::text, 'Б/Н') AS number,
                        mer.making_date_and_time AS created_at,
                        COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                        mer.content AS content,
                        COALESCE(d.deal_number::text, 'Б/Н') AS deal_number,
                        mer.content AS description,
                        COALESCE(mer.signs_of_intoxication, '') AS other_information,
                        COALESCE(mer.citizen_signature, false) AS signature,
                        'Не указан' AS first_witness,
                        'Не указан' AS second_witness,
                        COALESCE(officer.last_name || ' ' || officer.first_name || ' ' || COALESCE(officer.patronymic, ''), 'Не указан') AS officer_name,
                        'Не указана' AS article_name,
                        COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS patient_name,
                        COALESCE(tr.type_report, '') AS report_type,
                        COALESCE(mer.signs_of_intoxication, '') AS signs_of_intoxication
                    FROM medical_examination_report mer
                    LEFT JOIN deal d ON mer.deal = d.id_deal
                    LEFT JOIN citizens c ON mer.patient = c.id_citizens
                    LEFT JOIN citizens_and_posts cap ON d.police_officer = cap.id_citizens_and_posts
                    LEFT JOIN citizens officer ON cap.citizen = officer.id_citizens
                    LEFT JOIN type_report tr ON mer.report = tr.id_type_report
                    WHERE mer.id_medical_examination_report = @id",
                    
                "appeals" => @"
                    SELECT 
                        'Обращение' AS document_type,
                        COALESCE(a.number::text, 'Б/Н') AS number,
                        a.making_date_and_time AS created_at,
                        COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                        a.content AS content,
                        '' AS deal_number,
                        a.content AS description,
                        '' AS other_information,
                        false AS signature,
                        'Не указан' AS first_witness,
                        'Не указан' AS second_witness,
                        COALESCE(officer.last_name || ' ' || officer.first_name || ' ' || COALESCE(officer.patronymic, ''), 'Не указан') AS officer_name,
                        'Не указана' AS article_name,
                        '' AS patient_name,
                        '' AS report_type,
                        '' AS signs_of_intoxication
                    FROM appeals a
                    LEFT JOIN citizens c ON a.appeal_citizen = c.id_citizens
                    LEFT JOIN citizens_and_posts cap ON a.police_officer = cap.id_citizens_and_posts
                    LEFT JOIN citizens officer ON cap.citizen = officer.id_citizens
                    WHERE a.id_appeals = @id",
                    
                "statement" => @"
                    SELECT 
                        'Заявление' AS document_type,
                        COALESCE(s.number::text, 'Б/Н') AS number,
                        s.date_and_time AS created_at,
                        COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                        s.content AS content,
                        '' AS deal_number,
                        s.content AS description,
                        '' AS other_information,
                        false AS signature,
                        'Не указан' AS first_witness,
                        'Не указан' AS second_witness,
                        COALESCE(officer.last_name || ' ' || officer.first_name || ' ' || COALESCE(officer.patronymic, ''), 'Не указан') AS officer_name,
                        'Не указана' AS article_name,
                        '' AS patient_name,
                        '' AS report_type,
                        '' AS signs_of_intoxication
                    FROM statement s
                    LEFT JOIN citizens c ON s.applicant = c.id_citizens
                    LEFT JOIN citizens_and_posts cap ON s.police_officer = cap.id_citizens_and_posts
                    LEFT JOIN citizens officer ON cap.citizen = officer.id_citizens
                    WHERE s.id_statement = @id",
                    
                _ => throw new ArgumentException($"Неизвестная таблица: {tableName}")
            };

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", documentId);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new DocumentFull
                {
                    DocumentType = reader.GetString(0),
                    Number = reader.GetString(1),
                    CreatedAt = reader.GetDateTime(2),
                    CitizenFullName = reader.GetString(3),
                    Content = reader.GetString(4),
                    DealNumber = reader.GetString(5),
                    Description = reader.GetString(6),
                    OtherInformation = reader.GetString(7),
                    SignatureForKnowing = reader.GetBoolean(8),
                    FirstWitnessName = reader.GetString(9),
                    SecondWitnessName = reader.GetString(10),
                    OfficerName = reader.GetString(11),
                    ArticleName = reader.GetString(12),
                    PatientName = reader.GetString(13),
                    ReportType = reader.GetString(14),
                    SignsOfIntoxication = reader.GetString(15)
                };
            }

            throw new Exception("Документ не найден");
        }


        public async Task<List<RecentDocument>> GetUserFavoritesAsync(int userId)
        {
            var documents = new List<RecentDocument>();
            
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    f.document_id AS id,
                    CASE f.target_table
                        WHEN 'statement' THEN 1
                        WHEN 'appeals' THEN 2
                        WHEN 'explanation_protocol' THEN 3
                        WHEN 'medical_examination_report' THEN 4
                        WHEN 'administrative_protocol' THEN 5
                    END AS type_id,
                    CASE f.target_table
                        WHEN 'statement' THEN 'Заявление'
                        WHEN 'appeals' THEN 'Обращение'
                        WHEN 'explanation_protocol' THEN 'Протокол объяснения'
                        WHEN 'medical_examination_report' THEN 'Направление на мед. освид.'
                        WHEN 'administrative_protocol' THEN 'Административный протокол'
                    END AS type_name,
                    CASE f.target_table
                        WHEN 'statement' THEN s.number
                        WHEN 'appeals' THEN a.number
                        WHEN 'explanation_protocol' THEN ep.number
                        WHEN 'medical_examination_report' THEN mer.number
                        WHEN 'administrative_protocol' THEN ap.protocol_number
                    END AS number,
                    CASE f.target_table
                        WHEN 'statement' THEN s.date_and_time
                        WHEN 'appeals' THEN a.making_date_and_time
                        WHEN 'explanation_protocol' THEN ep.making_date_and_time
                        WHEN 'medical_examination_certificate' THEN mc.making_date_and_time
                        WHEN 'medical_examination_report' THEN mer.making_date_and_time
                        WHEN 'administrative_protocol' THEN ap.making_date_and_time
                    END AS making_date,
                    CASE f.target_table
                        WHEN 'statement' THEN c_s.last_name || ' ' || c_s.first_name || ' ' || COALESCE(c_s.patronymic, '')
                        WHEN 'appeals' THEN c_a.last_name || ' ' || c_a.first_name || ' ' || COALESCE(c_a.patronymic, '')
                        WHEN 'explanation_protocol' THEN c_ep.last_name || ' ' || c_ep.first_name || ' ' || COALESCE(c_ep.patronymic, '')
                        WHEN 'medical_examination_report' THEN c_mer.last_name || ' ' || c_mer.first_name || ' ' || COALESCE(c_mer.patronymic, '')
                        WHEN 'administrative_protocol' THEN c_ap.last_name || ' ' || c_ap.first_name || ' ' || COALESCE(c_ap.patronymic, '')
                    END AS citizen_name
                FROM user_favorites f
                LEFT JOIN statement s ON f.target_table = 'statement' AND f.document_id = s.id_statement
                LEFT JOIN citizens c_s ON s.applicant = c_s.id_citizens
                LEFT JOIN appeals a ON f.target_table = 'appeals' AND f.document_id = a.id_appeals
                LEFT JOIN citizens c_a ON a.appeal_citizen = c_a.id_citizens
                LEFT JOIN explanation_protocol ep ON f.target_table = 'explanation_protocol' AND f.document_id = ep.id_explanation_protocol
                LEFT JOIN citizens c_ep ON ep.citizen = c_ep.id_citizens
                LEFT JOIN medical_examination_report mer ON f.target_table = 'medical_examination_report' AND f.document_id = mer.id_medical_examination_report
                LEFT JOIN citizens c_mer ON mer.patient = c_mer.id_citizens
                LEFT JOIN administrative_protocol ap ON f.target_table = 'administrative_protocol' AND f.document_id = ap.id_protocol
                LEFT JOIN deal d ON ap.deal = d.id_deal
                LEFT JOIN citizens c_ap ON d.offender = c_ap.id_citizens
                WHERE f.user_id = @userId
                ORDER BY making_date DESC";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            
            await using var reader = await cmd.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                documents.Add(new RecentDocument
                {
                    Id = reader.GetInt32(0),
                    DocumentTypeId = reader.GetInt32(1),
                    DocumentType = reader.GetString(2),
                    Number = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    MakingDateAndTime = reader.GetDateTime(4),
                    CitizenName = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }

            return documents;
        }

        public async Task<List<MyDocument>> GetCitizenDocumentsAsync(int citizenId)
        {
            var documents = new List<MyDocument>();
            
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var statementsSql = @"
               SELECT 
                s.id_statement AS id,
                'statement' AS table_name,
                'Заявление' AS document_type,
                s.number,
                s.date_and_time AS created_at,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name,
                s.content,
                COALESCE(officer.last_name || ' ' || officer.first_name, 'Не указан') AS officer_name
            FROM statement s
            JOIN citizens c ON s.applicant = c.id_citizens
            LEFT JOIN citizens_and_posts cap ON s.police_officer = cap.id_citizens_and_posts
            LEFT JOIN citizens officer ON cap.citizen = officer.id_citizens
            WHERE s.applicant = @citizenId";


            var appealsSql = @"
               SELECT 
                    a.id_appeals AS id,
                    'appeals' AS table_name,
                    'Обращение' AS document_type,
                    a.number,
                    a.making_date_and_time AS created_at,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name,
                    a.content,
                    COALESCE(officer.last_name || ' ' || officer.first_name, 'Не указан') AS officer_name
                FROM appeals a
                JOIN citizens c ON a.appeal_citizen = c.id_citizens
                LEFT JOIN citizens_and_posts cap ON a.police_officer = cap.id_citizens_and_posts
                LEFT JOIN citizens officer ON cap.citizen = officer.id_citizens
                WHERE a.appeal_citizen = @citizenId";


            var explanationSql = @"
               SELECT 
                    ep.id_explanation_protocol AS id,
                    'explanation_protocol' AS table_name,
                    'Протокол объяснения' AS document_type,
                    ep.number,
                    ep.making_date_and_time AS created_at,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name,
                    ep.content,
                    COALESCE(officer.last_name || ' ' || officer.first_name, 'Не указан') AS officer_name
                FROM explanation_protocol ep
                JOIN citizens c ON ep.citizen = c.id_citizens
                LEFT JOIN deal d ON ep.deal = d.id_deal
                LEFT JOIN citizens_and_posts cap ON d.police_officer = cap.id_citizens_and_posts
                LEFT JOIN citizens officer ON cap.citizen = officer.id_citizens
                WHERE ep.citizen = @citizenId";


            var medicalSql = @"
                SELECT 
                mer.id_medical_examination_report AS id,
                'medical_examination_report' AS table_name,
                'Направление на мед. освид.' AS document_type,
                mer.number,
                mer.making_date_and_time AS created_at,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name,
                mer.content,
                COALESCE(officer.last_name || ' ' || officer.first_name, 'Не указан') AS officer_name
            FROM medical_examination_report mer
            JOIN citizens c ON mer.patient = c.id_citizens
            LEFT JOIN deal d ON mer.deal = d.id_deal
            LEFT JOIN citizens_and_posts cap ON d.police_officer = cap.id_citizens_and_posts
            LEFT JOIN citizens officer ON cap.citizen = officer.id_citizens
            WHERE mer.patient = @citizenId";


            var protocolSql = @"
                SELECT 
                    ap.id_protocol AS id,
                    'administrative_protocol' AS table_name,
                    'Административный протокол' AS document_type,
                    ap.protocol_number AS number,
                    ap.making_date_and_time AS created_at,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name,
                    ap.description AS content,
                    COALESCE(officer.last_name || ' ' || officer.first_name, 'Не указан') AS officer_name
                FROM administrative_protocol ap
                LEFT JOIN deal d ON ap.deal = d.id_deal
                LEFT JOIN citizens c ON d.offender = c.id_citizens
                LEFT JOIN citizens_and_posts cap ON d.police_officer = cap.id_citizens_and_posts
                LEFT JOIN citizens officer ON cap.citizen = officer.id_citizens
                WHERE d.offender = @citizenId";


            await using var cmd = new NpgsqlCommand(statementsSql, conn);
            cmd.Parameters.AddWithValue("@citizenId", citizenId);
            
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                documents.Add(MapToMyDocument(reader));
            }
            await reader.CloseAsync();

            cmd.CommandText = appealsSql;
            await using var reader2 = await cmd.ExecuteReaderAsync();
            while (await reader2.ReadAsync())
            {
                documents.Add(MapToMyDocument(reader2));
            }
            await reader2.CloseAsync();

            cmd.CommandText = explanationSql;
            await using var reader3 = await cmd.ExecuteReaderAsync();
            while (await reader3.ReadAsync())
            {
                documents.Add(MapToMyDocument(reader3));
            }
            await reader3.CloseAsync();

            cmd.CommandText = medicalSql;
            await using var reader4 = await cmd.ExecuteReaderAsync();
            while (await reader4.ReadAsync())
            {
                documents.Add(MapToMyDocument(reader4));
            }
            await reader4.CloseAsync();

            cmd.CommandText = protocolSql;
            await using var reader5 = await cmd.ExecuteReaderAsync();
            while (await reader5.ReadAsync())
            {
                documents.Add(MapToMyDocument(reader5));
            }
            await reader5.CloseAsync();


            return documents.OrderByDescending(d => d.CreatedAt).ToList();
        }

        private MyDocument MapToMyDocument(NpgsqlDataReader reader)
        {
            return new MyDocument
            {
                Id = reader.GetInt32(0),
                TableName = reader.GetString(1),
                DocumentType = reader.GetString(2),
                Number = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                CreatedAt = reader.GetDateTime(4),
                CitizenFullName = reader.IsDBNull(5) ? "Неизвестно" : reader.GetString(5),
                Content = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                OfficerName = reader.IsDBNull(7) ? "Не указан" : reader.GetString(7),
                IsFavorite = false
            };
        }

        public async Task<List<MedicalExaminationReport>> GetMedicalExaminationReportsAsync(string citizenName = "", string dealNumber = "", DateTime? date = null)
        {
            var reports = new List<MedicalExaminationReport>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(citizenName))
            {
                conditions.Add("(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '')) ILIKE @citizenName");
                parameters.Add("@citizenName", $"%{citizenName}%");
            }

            if (!string.IsNullOrWhiteSpace(dealNumber))
            {
                conditions.Add("d.deal_number::text ILIKE @dealNumber");
                parameters.Add("@dealNumber", $"%{dealNumber}%");
            }

            if (date.HasValue)
            {
                conditions.Add("mer.making_date_and_time::date = @date");
                parameters.Add("@date", date.Value.Date);
            }

            string whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";

            var sql = $@"
                SELECT 
                    mer.id_medical_examination_report,
                    mer.number,
                    mer.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS patient_full_name,
                    c.id_citizens,
                    d.deal_number
                FROM medical_examination_report mer
                JOIN citizens c ON mer.patient = c.id_citizens
                LEFT JOIN deal d ON mer.deal = d.id_deal
                {whereClause}
                ORDER BY mer.making_date_and_time DESC";

            await using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }
            
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reports.Add(new MedicalExaminationReport
                {
                    Id = reader.GetInt32(0),
                    Number = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    MakingDate = reader.GetDateTime(2),
                    PatientFullName = reader.GetString(3),
                    PatientId = reader.GetInt32(4),
                    DealNumber = reader.IsDBNull(5) ? null : reader.GetInt32(5).ToString()
                });
            }
            return reports;
        }

        public async Task<int> CreateMedicalCertificateAsync(
            int medicalExaminationReportId,
            string number, 
            DateTime makingDateAndTime, 
            string signsOfIntoxication, 
            string typeIntoxication, 
            string result, 
            bool doctorSignature,
            int medicalInstitutionId, // Добавлено
            int doctorId) // Добавлено
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO medical_examination_certificate 
                    (number, medical_examination_report, making_date_and_time, 
                    signs_of_intoxication, result, type_intoxication, 
                    doctor_signature, medical_institution, doctor)
                VALUES 
                    (@number, @reportId, @dateTime, @signs, @result, @type, @signature, @institutionId, @doctorId) 
                RETURNING id_medical_examination_certificate";

            await using var cmd = new NpgsqlCommand(sql, conn);
            
            // Параметры
            cmd.Parameters.AddWithValue("@number", int.Parse(number));
            cmd.Parameters.AddWithValue("@reportId", medicalExaminationReportId);
            cmd.Parameters.AddWithValue("@dateTime", makingDateAndTime);
            cmd.Parameters.AddWithValue("@signs", signsOfIntoxication);
            cmd.Parameters.AddWithValue("@result", result);
            
            // Приводим тип интоксикации к числу (пример)
            int typeCode = typeIntoxication switch
            {
                "Алкогольное" => 1,
                "Наркотическое" => 2,
                "Токсическое" => 3,
                _ => 0 // "Не выявлено"
            };
            cmd.Parameters.AddWithValue("@type", typeCode);
            
            cmd.Parameters.AddWithValue("@signature", doctorSignature);
            
            // --- Новые обязательные параметры ---
            cmd.Parameters.AddWithValue("@institutionId", medicalInstitutionId);
            cmd.Parameters.AddWithValue("@doctorId", doctorId);

            var resultId = await cmd.ExecuteScalarAsync();
            return resultId != null ? Convert.ToInt32(resultId) : 0;
        }

        /// <summary>
        /// Создать Постановление (Resolution)
        /// </summary>
        public async Task<int> CreateResolutionAsync(
            int dealId,
            int protocolNumber,
            DateTime dateTime,
            string content,
            int punishmentId,
            int courtStaffId,
            int? fineSum = null)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO resolution 
                (
                    protocol_number,
                    making_date_and_time,
                    court_staff,
                    deal,
                    resolution,
                    punishment,
                    fine_sum
                ) 
                VALUES 
                (
                    @protocolNumber,
                    @dateTime,
                    @courtStaffId,
                    @dealId,
                    @content,
                    @punishmentId,
                    @fineSum
                ) 
                RETURNING id_resolution";

            await using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@protocolNumber", protocolNumber);
            cmd.Parameters.AddWithValue("@dateTime", dateTime);
            cmd.Parameters.AddWithValue("@courtStaffId", courtStaffId);
            cmd.Parameters.AddWithValue("@dealId", dealId);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.Parameters.AddWithValue("@punishmentId", punishmentId);
            
            if (fineSum.HasValue)
                cmd.Parameters.AddWithValue("@fineSum", fineSum.Value);
            else
                cmd.Parameters.AddWithValue("@fineSum", DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<int> CreateForensicExpertiseAsync(
            int dealId,
            int number,
            DateTime dateTime,
            int structureId,
            int expertId,
            string content,
            bool physicalInjuries,
            bool severityHarm,
            bool couldOccur,
            bool signatureExpert)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

           var sql = @"
                INSERT INTO forensic_medical_examination 
                (
                    deal,
                    number,
                    making_date_and_time,
                    structure,
                    expert,
                    content,
                    physical_injuries,
                    severity_of_harm_to_health,
                    could_injuries_have_occurred_on_time,
                    signature_expert
                ) 
                VALUES 
                (
                    @dealId,
                    @number,
                    @dateTime,
                    @structureId,
                    @expertId,
                    @content,
                    @physicalInjuries,
                    @severityHarm,
                    @couldOccur,
                    @signatureExpert
                ) 
                RETURNING id_forensic_medical_examination";

            await using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@dealId", dealId);
            cmd.Parameters.AddWithValue("@number", number);
            cmd.Parameters.AddWithValue("@dateTime", dateTime);
            cmd.Parameters.AddWithValue("@structureId", structureId);
            cmd.Parameters.AddWithValue("@expertId", expertId);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.Parameters.AddWithValue("@physicalInjuries", physicalInjuries);
            cmd.Parameters.AddWithValue("@severityHarm", severityHarm);
            cmd.Parameters.AddWithValue("@couldOccur", couldOccur);
            cmd.Parameters.AddWithValue("@signatureExpert", signatureExpert);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}