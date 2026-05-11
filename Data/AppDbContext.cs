using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using CourseWork.Models;
using System.Reflection.Metadata;
using System.Linq;
using CourseWork.Helpers;

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
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                // Сначала получаем хэш пароля из БД
                var sql = @"
                    SELECT id, username, last_name, first_name, patronymic, COALESCE(role, 1) as role, password
                    FROM users
                    WHERE username = @username";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);

                await using var reader = await cmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var storedHash = reader.GetString(6);
                    
                    // Проверяем пароль
                    if (PasswordHelper.VerifyPassword(password, storedHash))
                    {
                        return new UserWithRole
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            LastName = reader.GetString(2),
                            FirstName = reader.GetString(3),
                            Patronymic = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Role = (UserRole)reader.GetInt32(5)
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AuthenticateUserWithRoleAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> RegisterUserAsync(string username, string password, string lastName, string firstName, string? patronymic = null)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql = @"INSERT INTO users (username, password, last_name, first_name, patronymic) 
                           VALUES (@username, @password, @lastName, @firstName, @patronymic)";
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@lastName", lastName);
                cmd.Parameters.AddWithValue("@firstName", firstName);
                cmd.Parameters.AddWithValue("@patronymic", patronymic ?? (object)DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        
        
        
        
        
        public async Task<List<RecentDocument>> GetAllDocumentsAsync()
        {
            var documents = new List<RecentDocument>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"SELECT id, type_id, type_name, number, making_date, citizen_name 
                        FROM view_recent_documents 
                        ORDER BY making_date DESC 
                        LIMIT 100";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                documents.Add(MapReader(reader));

            Console.WriteLine($"[DEBUG] GetAllDocumentsAsync: загружено {documents.Count} документов");
            return documents;
        }

        
        
        
        public async Task<List<RecentDocument>> GetFavoriteDocumentsAsync(int userId)
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
                    cp.phone_number,                    -- ← телефон
                    c.passport_series_and_number,
                    s.name as working_place_name,
                    e.education as education_name,
                    fs.family_status as family_status_name,
                    cit.citizenship as citizenship_name,
                    c.criminal_record,
                    c.count_record,
                    p.post as post_name
                FROM citizens c
                LEFT JOIN citizen_phones cp ON cp.citizen = c.id_citizens AND cp.is_primary = true   -- ← этот JOIN
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
                    Phone = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Passport = reader.IsDBNull(7) ? null : reader.GetString(7),
                    WorkingPlaceName = reader.IsDBNull(8) ? null : reader.GetString(8),
                    EducationName = reader.IsDBNull(9) ? null : reader.GetString(9),
                    FamilyStatusName = reader.IsDBNull(10) ? null : reader.GetString(10),
                    CitizenshipName = reader.IsDBNull(11) ? null : reader.GetString(11),
                    CriminalRecord = reader.GetBoolean(12),           // ← добавить
                    CountRecord = reader.IsDBNull(13) ? null : reader.GetInt32(13),  // ← добавить
                    PostName = reader.IsDBNull(14) ? null : reader.GetString(14),    // ← добавить

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
                
        
                draft.Preview = ExtractPreview(formData);
                
        
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


       
        private string ExtractPreview(string json)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("content", out var content))
                {
                    var text = content.GetString();
                    return string.IsNullOrWhiteSpace(text) ? "(пусто)" : 
                        (text.Length > 100 ? text.Substring(0, 100) + "..." : text);
                }
            }
            catch { }
            return "(ошибка чтения)";
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
                    fields.Add(draft.PatientId != null);
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
                    COALESCE(d.deal_number::text, 'Б/Н') AS number,
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
                    Number = reader.IsDBNull(1) ? "Б/Н" : reader.GetString(1),
                    CitizenFullName = reader.IsDBNull(2) ? "Неизвестно" : reader.GetString(2),
                    DealDate = DateTime.Now
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
                    c.passport_series_and_number,  -- ✅ Добавьте паспорт
                    (SELECT cp.phone_number FROM citizen_phones cp WHERE cp.citizen = c.id_citizens AND cp.is_primary = TRUE LIMIT 1) AS phone,
                    s.name as working_place,       -- ✅ Место работы (название)
                    e.education,                   -- ✅ Образование (название)
                    fs.family_status,              -- ✅ Семейное положение (название)
                    cit.citizenship,               -- ✅ Гражданство (название)
                    p.post as post_name            -- ✅ Должность (название)
                FROM citizens c
                LEFT JOIN citizen_phones cp ON cp.citizen = c.id_citizens AND cp.is_primary = true
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

        public async Task<List<ExternalDocument>> GetExternalDocumentsAsync(int dealId, int? citizenId = null)
        {
            var documents = new List<ExternalDocument>();
            
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

    
            var sql = @"
                -- Протокол объяснения
                SELECT 
                    ep.id_explanation_protocol AS id,
                    'explanation_protocol' AS table_name,
                    'Протокол объяснения' AS document_type,
                    COALESCE(ep.number::text, 'Б/Н') AS number,
                    COALESCE(ep.making_date_and_time, NOW()) AS created_at,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                    COALESCE(d.deal_number::text, '') AS deal_number,
                    ep.citizen AS citizen_id,
                    ep.deal AS deal_id
                FROM public.explanation_protocol ep
                INNER JOIN public.deal d ON ep.deal = d.id_deal
                LEFT JOIN public.citizens c ON ep.citizen = c.id_citizens
                WHERE ep.deal = @dealId
                
                UNION ALL
                
                -- Административный протокол
                SELECT 
                    ap.id_protocol AS id,
                    'administrative_protocol' AS table_name,
                    'Административный протокол' AS document_type,
                    COALESCE(ap.protocol_number::text, 'Б/Н') AS number,
                    COALESCE(ap.making_date_and_time, NOW()) AS created_at,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                    COALESCE(d.deal_number::text, '') AS deal_number,
                    d.offender AS citizen_id,
                    ap.deal AS deal_id
                FROM public.administrative_protocol ap
                INNER JOIN public.deal d ON ap.deal = d.id_deal
                INNER JOIN public.citizens c ON d.offender = c.id_citizens
                WHERE ap.deal = @dealId
                
                UNION ALL
                
                -- Направление на мед. освидетельствование
                SELECT 
                    mer.id_medical_examination_report AS id,
                    'medical_examination_report' AS table_name,
                    'Направление на мед. освид.' AS document_type,
                    COALESCE(mer.number::text, 'Б/Н') AS number,
                    COALESCE(mer.making_date_and_time, NOW()) AS created_at,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                    COALESCE(d.deal_number::text, '') AS deal_number,
                    mer.patient AS citizen_id,
                    mer.deal AS deal_id
                FROM public.medical_examination_report mer
                INNER JOIN public.deal d ON mer.deal = d.id_deal
                INNER JOIN public.citizens c ON mer.patient = c.id_citizens
                WHERE mer.deal = @dealId
                
                ORDER BY created_at DESC";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("dealId", dealId);
    
            
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
                    DealId = reader.GetInt32(8)
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
                SELECT cap.id_citizens_and_posts
                FROM users u
                JOIN citizens c ON c.last_name = u.last_name AND c.first_name = u.first_name
                JOIN citizens_and_posts cap ON cap.citizen = c.id_citizens
                WHERE u.id = @userId
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


            var appealsSql = @"
                SELECT 
                    a.id_appeals,
                    a.number,
                    a.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                    a.content,
                    c.id_citizens,
                    EXISTS(SELECT 1 FROM user_favorites WHERE user_id = @userId AND target_table = 'appeals' AND document_id = a.id_appeals) as is_favorite
                FROM appeals a
                JOIN citizens c ON a.appeal_citizen = c.id_citizens
                WHERE a.police_officer = @userId";
            
            await using var cmd = new NpgsqlCommand(appealsSql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            await using var reader = await cmd.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                documents.Add(new MyDocument
                {
                    Id = reader.GetInt32(0),
                    DocumentType = "Обращение",
                    TableName = "appeals",
                    Number = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    CreatedAt = reader.GetDateTime(2),
                    CitizenFullName = reader.GetString(3),
                    Content = reader.GetString(4),
                    CitizenId = reader.GetInt32(5),
                    IsFavorite = reader.GetBoolean(6)
                });
            }
            await reader.CloseAsync();
            

            var statementsSql = @"
                SELECT 
                    s.id_statement,
                    s.number,
                    s.date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                    s.content,
                    c.id_citizens,
                    EXISTS(SELECT 1 FROM user_favorites WHERE user_id = @userId AND target_table = 'statement' AND document_id = s.id_statement) as is_favorite
                FROM statement s
                JOIN citizens c ON s.applicant = c.id_citizens
                WHERE s.police_officer = @userId";
            
            cmd.CommandText = statementsSql;
            await using var reader2 = await cmd.ExecuteReaderAsync();
            
            while (await reader2.ReadAsync())
            {
                documents.Add(new MyDocument
                {
                    Id = reader2.GetInt32(0),
                    DocumentType = "Заявление",
                    TableName = "statement",
                    Number = reader2.IsDBNull(1) ? null : reader2.GetInt32(1),
                    CreatedAt = reader2.GetDateTime(2),
                    CitizenFullName = reader2.GetString(3),
                    Content = reader2.GetString(4),
                    CitizenId = reader2.GetInt32(5),
                    IsFavorite = reader2.GetBoolean(6)
                });
            }
            await reader2.CloseAsync();
            

            var protocolsSql = @"
                SELECT 
                    ap.id_protocol,
                    ap.protocol_number,
                    ap.making_date_and_time,
                    'Неизвестно' AS citizen_full_name,
                    ap.description,
                    0 AS citizen_id,
                    EXISTS(SELECT 1 FROM user_favorites WHERE user_id = @userId AND target_table = 'administrative_protocol' AND document_id = ap.id_protocol) as is_favorite
                FROM administrative_protocol ap";
            
            cmd.CommandText = protocolsSql;
            await using var reader3 = await cmd.ExecuteReaderAsync();
            
            while (await reader3.ReadAsync())
            {
                documents.Add(new MyDocument
                {
                    Id = reader3.GetInt32(0),
                    DocumentType = "Административный протокол",
                    TableName = "administrative_protocol",
                    Number = reader3.GetInt32(1),
                    CreatedAt = reader3.GetDateTime(2),
                    CitizenFullName = reader3.GetString(3),
                    Content = reader3.GetString(4),
                    CitizenId = reader3.GetInt32(5),
                    IsFavorite = reader3.GetBoolean(6)
                });
            }
            await reader3.CloseAsync();
            

            var medicalSql = @"
                SELECT 
                    mer.id_medical_examination_report,
                    mer.number,
                    mer.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                    mer.content,
                    c.id_citizens,
                    EXISTS(SELECT 1 FROM user_favorites WHERE user_id = @userId AND target_table = 'medical_examination_report' AND document_id = mer.id_medical_examination_report) as is_favorite
                FROM medical_examination_report mer
                JOIN citizens c ON mer.patient = c.id_citizens";
            
            cmd.CommandText = medicalSql;
            await using var reader4 = await cmd.ExecuteReaderAsync();
            
            while (await reader4.ReadAsync())
            {
                documents.Add(new MyDocument
                {
                    Id = reader4.GetInt32(0),
                    DocumentType = "Направление на мед. освид.",
                    TableName = "medical_examination_report",
                    Number = reader4.IsDBNull(1) ? null : reader4.GetInt32(1),
                    CreatedAt = reader4.GetDateTime(2),
                    CitizenFullName = reader4.GetString(3),
                    Content = reader4.GetString(4),
                    CitizenId = reader4.GetInt32(5),
                    IsFavorite = reader4.GetBoolean(6)
                });
            }
            await reader4.CloseAsync();
            

            var explanationSql = @"
                SELECT 
                    ep.id_explanation_protocol,
                    ep.number,
                    ep.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                    ep.content,
                    c.id_citizens,
                    EXISTS(SELECT 1 FROM user_favorites WHERE user_id = @userId AND target_table = 'explanation_protocol' AND document_id = ep.id_explanation_protocol) as is_favorite
                FROM explanation_protocol ep
                JOIN citizens c ON ep.citizen = c.id_citizens";
            
            cmd.CommandText = explanationSql;
            await using var reader5 = await cmd.ExecuteReaderAsync();
            
            while (await reader5.ReadAsync())
            {
                documents.Add(new MyDocument
                {
                    Id = reader5.GetInt32(0),
                    DocumentType = "Протокол объяснения",
                    TableName = "explanation_protocol",
                    Number = reader5.IsDBNull(1) ? null : reader5.GetInt32(1),
                    CreatedAt = reader5.GetDateTime(2),
                    CitizenFullName = reader5.GetString(3),
                    Content = reader5.GetString(4),
                    CitizenId = reader5.GetInt32(5),
                    IsFavorite = reader5.GetBoolean(6)
                });
            }
            await reader5.CloseAsync();
            

            documents = documents.OrderByDescending(d => d.CreatedAt).ToList();
            
            return documents;
        }
        public async Task RemoveFromFavoritesAsync(int userId, string targetTable, int documentId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = "DELETE FROM user_favorites WHERE user_id = @userId AND target_table = @targetTable AND document_id = @documentId";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@targetTable", targetTable);
            cmd.Parameters.AddWithValue("@documentId", documentId);
            
            int deleted = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"[DEBUG] RemoveFromFavorites: удалено {deleted} записей");
        }

        public async Task<DocumentFull> GetFullDocumentAsync(string tableName, int documentId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = tableName switch
            {
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
                        COALESCE(co.last_name || ' ' || co.first_name || ' ' || COALESCE(co.patronymic, ''), 'Не указан') AS officer_name,
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
                    LEFT JOIN citizens co ON cap.citizen = co.id_citizens
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
                        'Не указан' AS officer_name,
                        COALESCE(a.number_of_article::text || ' - ' || a.description, 'Не указана') AS article_name,
                        '' AS patient_name,
                        '' AS report_type,
                        '' AS signs_of_intoxication
                    FROM explanation_protocol ep
                    LEFT JOIN deal d ON ep.deal = d.id_deal
                    LEFT JOIN citizens c ON ep.citizen = c.id_citizens
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
                        'Не указан' AS officer_name,
                        'Не указана' AS article_name,
                        COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS patient_name,
                        COALESCE(tr.type_report, '') AS report_type,
                        COALESCE(mer.signs_of_intoxication, '') AS signs_of_intoxication
                    FROM medical_examination_report mer
                    LEFT JOIN deal d ON mer.deal = d.id_deal
                    LEFT JOIN citizens c ON mer.patient = c.id_citizens
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
                        COALESCE(co.last_name || ' ' || co.first_name || ' ' || COALESCE(co.patronymic, ''), 'Не указан') AS officer_name,
                        'Не указана' AS article_name,
                        '' AS patient_name,
                        '' AS report_type,
                        '' AS signs_of_intoxication
                    FROM appeals a
                    LEFT JOIN citizens c ON a.appeal_citizen = c.id_citizens
                    LEFT JOIN citizens_and_posts cap ON a.police_officer = cap.id_citizens_and_posts
                    LEFT JOIN citizens co ON cap.citizen = co.id_citizens
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
                        COALESCE(co.last_name || ' ' || co.first_name || ' ' || COALESCE(co.patronymic, ''), 'Не указан') AS officer_name,
                        'Не указана' AS article_name,
                        '' AS patient_name,
                        '' AS report_type,
                        '' AS signs_of_intoxication
                    FROM statement s
                    LEFT JOIN citizens c ON s.applicant = c.id_citizens
                    LEFT JOIN citizens_and_posts cap ON s.police_officer = cap.id_citizens_and_posts
                    LEFT JOIN citizens co ON cap.citizen = co.id_citizens
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
                    s.content
                FROM statement s
                JOIN citizens c ON s.applicant = c.id_citizens
                WHERE s.applicant = @citizenId";


            var appealsSql = @"
                SELECT 
                    a.id_appeals AS id,
                    'appeals' AS table_name,
                    'Обращение' AS document_type,
                    a.number,
                    a.making_date_and_time AS created_at,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name,
                    a.content
                FROM appeals a
                JOIN citizens c ON a.appeal_citizen = c.id_citizens
                WHERE a.appeal_citizen = @citizenId";


            var explanationSql = @"
                SELECT 
                    ep.id_explanation_protocol AS id,
                    'explanation_protocol' AS table_name,
                    'Протокол объяснения' AS document_type,
                    ep.number,
                    ep.making_date_and_time AS created_at,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name,
                    ep.content
                FROM explanation_protocol ep
                JOIN citizens c ON ep.citizen = c.id_citizens
                WHERE ep.citizen = @citizenId";


            var medicalSql = @"
                SELECT 
                    mer.id_medical_examination_report AS id,
                    'medical_examination_report' AS table_name,
                    'Направление на мед. освид.' AS document_type,
                    mer.number,
                    mer.making_date_and_time AS created_at,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name,
                    mer.content
                FROM medical_examination_report mer
                JOIN citizens c ON mer.patient = c.id_citizens
                WHERE mer.patient = @citizenId";


            var protocolSql = @"
                SELECT 
                    ap.id_protocol AS id,
                    'administrative_protocol' AS table_name,
                    'Административный протокол' AS document_type,
                    ap.protocol_number AS number,
                    ap.making_date_and_time AS created_at,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name,
                    ap.description AS content
                FROM administrative_protocol ap
                JOIN deal d ON ap.deal = d.id_deal
                JOIN citizens c ON d.offender = c.id_citizens
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
                IsFavorite = false
            };
        }
    }
}