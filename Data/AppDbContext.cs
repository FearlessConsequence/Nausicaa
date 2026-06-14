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
            try {   
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

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
                        return user;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUTH ERROR] {ex.Message}");
                Console.WriteLine($"[AUTH ERROR] {ex.StackTrace}");
                throw;
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
                
                
                case UserRole.AdminInspector:
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
                            WHERE a.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                            
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
                            WHERE s.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                            
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
                            
                            UNION ALL
                            
                            -- Акты мед. освид.
                            SELECT 
                                mec.id_medical_examination_certificate AS id,
                                6 AS type_id,
                                'Акт медицинского освидетельствования' AS type_name,
                                mec.number,
                                mec.making_date_and_time AS making_date,
                                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                            FROM medical_examination_certificate mec
                            JOIN medical_examination_report mer ON mec.medical_examination_report = mer.id_medical_examination_report
                            JOIN citizens c ON mer.patient = c.id_citizens
                            JOIN deal d ON mer.deal = d.id_deal
                            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                            
                            UNION ALL
                            
                            -- Судебно-медицинские экспертизы
                            SELECT 
                                fe.id_forensic_medical_examination AS id,
                                7 AS type_id,
                                'Судебно-медицинская экспертиза' AS type_name,
                                fe.number,
                                fe.making_date_and_time AS making_date,
                                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                            FROM forensic_medical_examination fe
                            JOIN deal d ON fe.deal = d.id_deal
                            JOIN citizens c ON d.offender = c.id_citizens
                            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                            
                            UNION ALL
                            
                            -- Постановления
                            SELECT 
                                r.id_resolution AS id,
                                8 AS type_id,
                                'Постановление' AS type_name,
                                r.protocol_number AS number,
                                r.making_date_and_time AS making_date,
                                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                            FROM resolution r
                            JOIN deal d ON r.deal = d.id_deal
                            JOIN citizens c ON d.offender = c.id_citizens
                            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                            
                            UNION ALL
                            
                            -- ДЕЛА
                            SELECT 
                                d.id_deal AS id,
                                13 AS type_id,
                                'Дело' AS type_name,
                                d.deal_number AS number,
                                d.making_date AS making_date,
                                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
                            FROM deal d
                            JOIN citizens c ON d.offender = c.id_citizens
                            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                        ) AS docs
                        ORDER BY making_date DESC
                        LIMIT 100";
                    break;
                case UserRole.ChiefOfPolice:
    sql = @"
        SELECT id, type_id, type_name, number, making_date, citizen_name FROM (
            SELECT 
                a.id_appeals AS id,
                2 AS type_id,
                'Обращение' AS type_name,
                a.number,
                a.making_date_and_time AS making_date,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
            FROM appeals a
            JOIN citizens c ON a.appeal_citizen = c.id_citizens
            WHERE a.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
            
            UNION ALL
            
            SELECT 
                s.id_statement AS id,
                1 AS type_id,
                'Заявление' AS type_name,
                s.number,
                s.date_and_time AS making_date,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
            FROM statement s
            JOIN citizens c ON s.applicant = c.id_citizens
            WHERE s.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
            
            UNION ALL
            
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
            
            UNION ALL
            
            SELECT 
                mec.id_medical_examination_certificate AS id,
                6 AS type_id,
                'Акт медицинского освидетельствования' AS type_name,
                mec.number,
                mec.making_date_and_time AS making_date,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
            FROM medical_examination_certificate mec
            JOIN medical_examination_report mer ON mec.medical_examination_report = mer.id_medical_examination_report
            JOIN citizens c ON mer.patient = c.id_citizens
            JOIN deal d ON mer.deal = d.id_deal
            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
            
            UNION ALL
            
            SELECT 
                fe.id_forensic_medical_examination AS id,
                7 AS type_id,
                'Судебно-медицинская экспертиза' AS type_name,
                fe.number,
                fe.making_date_and_time AS making_date,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
            FROM forensic_medical_examination fe
            JOIN deal d ON fe.deal = d.id_deal
            JOIN citizens c ON d.offender = c.id_citizens
            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
            
            UNION ALL
            
            SELECT 
                r.id_resolution AS id,
                8 AS type_id,
                'Постановление' AS type_name,
                r.protocol_number AS number,
                r.making_date_and_time AS making_date,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
            FROM resolution r
            JOIN deal d ON r.deal = d.id_deal
            JOIN citizens c ON d.offender = c.id_citizens
            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
            
            UNION ALL
            
            SELECT 
                d.id_deal AS id,
                13 AS type_id,
                'Дело' AS type_name,
                d.deal_number AS number,
                d.making_date AS making_date,
                c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_name
            FROM deal d
            JOIN citizens c ON d.offender = c.id_citizens
            WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
        ) AS docs
        ORDER BY making_date DESC
        LIMIT 100";
    break;

               

        

        default:
            sql = @"
                SELECT id, type_id, type_name, number, making_date, citizen_name 
                FROM view_recent_documents 
                ORDER BY making_date DESC 
                LIMIT 100";
            break;
    }

           await using var cmd = new NpgsqlCommand(sql, conn);
            if (role == UserRole.PoliceOfficer || 
                role == UserRole.ForensicExpert || 
                role == UserRole.AdminInspector || 
                role == UserRole.ChiefOfPolice)
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
                    Number = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    MakingDateAndTime = reader.GetDateTime(4),
                    CitizenName = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }
            return documents;
        }

        public async Task<List<RecentDocument>> GetFavoriteDocumentsAsync(int userId)
        {
            var documents = new List<RecentDocument>();
            var role = App.CurrentUserRole;
            
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // ==================== ИНСПЕКТОР ====================
            if (role == UserRole.AdminInspector)
            {
                var sql = @"
                    SELECT 
                        f.document_id,
                        f.target_table,
                        CASE f.target_table
                            WHEN 'appeals' THEN a.number
                            WHEN 'statement' THEN s.number
                            WHEN 'explanation_protocol' THEN ep.number
                            WHEN 'medical_examination_report' THEN mer.number
                            WHEN 'administrative_protocol' THEN ap.protocol_number
                            WHEN 'medical_examination_certificate' THEN mc.number
                            WHEN 'forensic_medical_examination' THEN fe.number
                            WHEN 'resolution' THEN r.protocol_number
                            WHEN 'deal' THEN d.deal_number
                        END AS doc_number,
                        CASE f.target_table
                            WHEN 'appeals' THEN a.making_date_and_time
                            WHEN 'statement' THEN s.date_and_time
                            WHEN 'explanation_protocol' THEN ep.making_date_and_time
                            WHEN 'medical_examination_report' THEN mer.making_date_and_time
                            WHEN 'administrative_protocol' THEN ap.making_date_and_time
                            WHEN 'medical_examination_certificate' THEN mc.making_date_and_time
                            WHEN 'forensic_medical_examination' THEN fe.making_date_and_time
                            WHEN 'resolution' THEN r.making_date_and_time
                            WHEN 'deal' THEN d.making_date
                        END AS doc_date,
                        CASE f.target_table
                            WHEN 'appeals' THEN c_a.last_name || ' ' || c_a.first_name || ' ' || COALESCE(c_a.patronymic, '')
                            WHEN 'statement' THEN c_s.last_name || ' ' || c_s.first_name || ' ' || COALESCE(c_s.patronymic, '')
                            WHEN 'explanation_protocol' THEN c_ep.last_name || ' ' || c_ep.first_name || ' ' || COALESCE(c_ep.patronymic, '')
                            WHEN 'medical_examination_report' THEN c_mer.last_name || ' ' || c_mer.first_name || ' ' || COALESCE(c_mer.patronymic, '')
                            WHEN 'administrative_protocol' THEN c_ap.last_name || ' ' || c_ap.first_name || ' ' || COALESCE(c_ap.patronymic, '')
                            WHEN 'medical_examination_certificate' THEN c_mc.last_name || ' ' || c_mc.first_name || ' ' || COALESCE(c_mc.patronymic, '')
                            WHEN 'forensic_medical_examination' THEN c_fe.last_name || ' ' || c_fe.first_name || ' ' || COALESCE(c_fe.patronymic, '')
                            WHEN 'resolution' THEN c_r.last_name || ' ' || c_r.first_name || ' ' || COALESCE(c_r.patronymic, '')
                            WHEN 'deal' THEN c_d.last_name || ' ' || c_d.first_name || ' ' || COALESCE(c_d.patronymic, '')
                        END AS citizen_name
                    FROM user_favorites f
                    LEFT JOIN appeals a ON f.target_table = 'appeals' AND f.document_id = a.id_appeals
                    LEFT JOIN citizens c_a ON a.appeal_citizen = c_a.id_citizens
                    LEFT JOIN statement s ON f.target_table = 'statement' AND f.document_id = s.id_statement
                    LEFT JOIN citizens c_s ON s.applicant = c_s.id_citizens
                    LEFT JOIN explanation_protocol ep ON f.target_table = 'explanation_protocol' AND f.document_id = ep.id_explanation_protocol
                    LEFT JOIN citizens c_ep ON ep.citizen = c_ep.id_citizens
                    LEFT JOIN medical_examination_report mer ON f.target_table = 'medical_examination_report' AND f.document_id = mer.id_medical_examination_report
                    LEFT JOIN citizens c_mer ON mer.patient = c_mer.id_citizens
                    LEFT JOIN administrative_protocol ap ON f.target_table = 'administrative_protocol' AND f.document_id = ap.id_protocol
                    LEFT JOIN deal d_ap ON ap.deal = d_ap.id_deal
                    LEFT JOIN citizens c_ap ON d_ap.offender = c_ap.id_citizens
                    LEFT JOIN medical_examination_certificate mc ON f.target_table = 'medical_examination_certificate' AND f.document_id = mc.id_medical_examination_certificate
                    LEFT JOIN medical_examination_report mer_mc ON mc.medical_examination_report = mer_mc.id_medical_examination_report
                    LEFT JOIN citizens c_mc ON mer_mc.patient = c_mc.id_citizens
                    LEFT JOIN forensic_medical_examination fe ON f.target_table = 'forensic_medical_examination' AND f.document_id = fe.id_forensic_medical_examination
                    LEFT JOIN deal d_fe ON fe.deal = d_fe.id_deal
                    LEFT JOIN citizens c_fe ON d_fe.offender = c_fe.id_citizens
                    LEFT JOIN resolution r ON f.target_table = 'resolution' AND f.document_id = r.id_resolution
                    LEFT JOIN deal d_r ON r.deal = d_r.id_deal
                    LEFT JOIN citizens c_r ON d_r.offender = c_r.id_citizens
                    LEFT JOIN deal d ON f.target_table = 'deal' AND f.document_id = d.id_deal
                    LEFT JOIN citizens c_d ON d.offender = c_d.id_citizens
                    WHERE f.user_id = @userId";
                
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                await using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var doc = new RecentDocument
                    {
                        Id = reader.GetInt32(0),
                        DocumentType = GetDocumentTypeName(reader.GetString(1)),
                        DocumentTypeId = GetDocumentTypeId(reader.GetString(1)),
                        Number = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                        MakingDateAndTime = reader.IsDBNull(3) ? DateTime.Now : reader.GetDateTime(3),
                        CitizenName = reader.IsDBNull(4) ? "Неизвестно" : reader.GetString(4)
                    };
                    documents.Add(doc);
                }
                return documents;
            }

            // ==================== НАЧАЛЬНИК ====================
            if (role == UserRole.ChiefOfPolice)
            {
                // Начальник видит все избранные документы (без ограничений)
                var sql = @"
                    SELECT 
                        f.document_id,
                        f.target_table,
                        CASE f.target_table
                            WHEN 'appeals' THEN a.number
                            WHEN 'statement' THEN s.number
                            WHEN 'explanation_protocol' THEN ep.number
                            WHEN 'medical_examination_report' THEN mer.number
                            WHEN 'administrative_protocol' THEN ap.protocol_number
                            WHEN 'medical_examination_certificate' THEN mc.number
                            WHEN 'forensic_medical_examination' THEN fe.number
                            WHEN 'resolution' THEN r.protocol_number
                            WHEN 'deal' THEN d.deal_number
                        END AS doc_number,
                        CASE f.target_table
                            WHEN 'appeals' THEN a.making_date_and_time
                            WHEN 'statement' THEN s.date_and_time
                            WHEN 'explanation_protocol' THEN ep.making_date_and_time
                            WHEN 'medical_examination_report' THEN mer.making_date_and_time
                            WHEN 'administrative_protocol' THEN ap.making_date_and_time
                            WHEN 'medical_examination_certificate' THEN mc.making_date_and_time
                            WHEN 'forensic_medical_examination' THEN fe.making_date_and_time
                            WHEN 'resolution' THEN r.making_date_and_time
                            WHEN 'deal' THEN d.making_date
                        END AS doc_date,
                        CASE f.target_table
                            WHEN 'appeals' THEN c_a.last_name || ' ' || c_a.first_name || ' ' || COALESCE(c_a.patronymic, '')
                            WHEN 'statement' THEN c_s.last_name || ' ' || c_s.first_name || ' ' || COALESCE(c_s.patronymic, '')
                            WHEN 'explanation_protocol' THEN c_ep.last_name || ' ' || c_ep.first_name || ' ' || COALESCE(c_ep.patronymic, '')
                            WHEN 'medical_examination_report' THEN c_mer.last_name || ' ' || c_mer.first_name || ' ' || COALESCE(c_mer.patronymic, '')
                            WHEN 'administrative_protocol' THEN c_ap.last_name || ' ' || c_ap.first_name || ' ' || COALESCE(c_ap.patronymic, '')
                            WHEN 'medical_examination_certificate' THEN c_mc.last_name || ' ' || c_mc.first_name || ' ' || COALESCE(c_mc.patronymic, '')
                            WHEN 'forensic_medical_examination' THEN c_fe.last_name || ' ' || c_fe.first_name || ' ' || COALESCE(c_fe.patronymic, '')
                            WHEN 'resolution' THEN c_r.last_name || ' ' || c_r.first_name || ' ' || COALESCE(c_r.patronymic, '')
                            WHEN 'deal' THEN c_d.last_name || ' ' || c_d.first_name || ' ' || COALESCE(c_d.patronymic, '')
                        END AS citizen_name
                    FROM user_favorites f
                    LEFT JOIN appeals a ON f.target_table = 'appeals' AND f.document_id = a.id_appeals
                    LEFT JOIN citizens c_a ON a.appeal_citizen = c_a.id_citizens
                    LEFT JOIN statement s ON f.target_table = 'statement' AND f.document_id = s.id_statement
                    LEFT JOIN citizens c_s ON s.applicant = c_s.id_citizens
                    LEFT JOIN explanation_protocol ep ON f.target_table = 'explanation_protocol' AND f.document_id = ep.id_explanation_protocol
                    LEFT JOIN citizens c_ep ON ep.citizen = c_ep.id_citizens
                    LEFT JOIN medical_examination_report mer ON f.target_table = 'medical_examination_report' AND f.document_id = mer.id_medical_examination_report
                    LEFT JOIN citizens c_mer ON mer.patient = c_mer.id_citizens
                    LEFT JOIN administrative_protocol ap ON f.target_table = 'administrative_protocol' AND f.document_id = ap.id_protocol
                    LEFT JOIN deal d_ap ON ap.deal = d_ap.id_deal
                    LEFT JOIN citizens c_ap ON d_ap.offender = c_ap.id_citizens
                    LEFT JOIN medical_examination_certificate mc ON f.target_table = 'medical_examination_certificate' AND f.document_id = mc.id_medical_examination_certificate
                    LEFT JOIN medical_examination_report mer_mc ON mc.medical_examination_report = mer_mc.id_medical_examination_report
                    LEFT JOIN citizens c_mc ON mer_mc.patient = c_mc.id_citizens
                    LEFT JOIN forensic_medical_examination fe ON f.target_table = 'forensic_medical_examination' AND f.document_id = fe.id_forensic_medical_examination
                    LEFT JOIN deal d_fe ON fe.deal = d_fe.id_deal
                    LEFT JOIN citizens c_fe ON d_fe.offender = c_fe.id_citizens
                    LEFT JOIN resolution r ON f.target_table = 'resolution' AND f.document_id = r.id_resolution
                    LEFT JOIN deal d_r ON r.deal = d_r.id_deal
                    LEFT JOIN citizens c_r ON d_r.offender = c_r.id_citizens
                    LEFT JOIN deal d ON f.target_table = 'deal' AND f.document_id = d.id_deal
                    LEFT JOIN citizens c_d ON d.offender = c_d.id_citizens
                    WHERE f.user_id = @userId";
                
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                await using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var doc = new RecentDocument
                    {
                        Id = reader.GetInt32(0),
                        DocumentType = GetDocumentTypeName(reader.GetString(1)),
                        DocumentTypeId = GetDocumentTypeId(reader.GetString(1)),
                        Number = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                        MakingDateAndTime = reader.IsDBNull(3) ? DateTime.Now : reader.GetDateTime(3),
                        CitizenName = reader.IsDBNull(4) ? "Неизвестно" : reader.GetString(4)
                    };
                    documents.Add(doc);
                }
                return documents;
            }

            // ==================== ОСТАЛЬНЫЕ РОЛИ (Полицейский, Врач, Судья, Эксперт) ====================
            var sqlDefault = @"
                SELECT 
                    f.document_id,
                    f.target_table,
                    CASE f.target_table
                        WHEN 'statement' THEN s.number
                        WHEN 'appeals' THEN a.number
                        WHEN 'explanation_protocol' THEN ep.number
                        WHEN 'medical_examination_report' THEN mer.number
                        WHEN 'administrative_protocol' THEN ap.protocol_number
                        WHEN 'medical_examination_certificate' THEN mc.number
                        WHEN 'forensic_medical_examination' THEN fe.number
                        WHEN 'resolution' THEN r.protocol_number
                        WHEN 'deal' THEN d.deal_number
                    END AS doc_number,
                    CASE f.target_table
                        WHEN 'statement' THEN s.date_and_time
                        WHEN 'appeals' THEN a.making_date_and_time
                        WHEN 'explanation_protocol' THEN ep.making_date_and_time
                        WHEN 'medical_examination_report' THEN mer.making_date_and_time
                        WHEN 'administrative_protocol' THEN ap.making_date_and_time
                        WHEN 'medical_examination_certificate' THEN mc.making_date_and_time
                        WHEN 'forensic_medical_examination' THEN fe.making_date_and_time
                        WHEN 'resolution' THEN r.making_date_and_time
                        WHEN 'deal' THEN d.making_date
                    END AS doc_date,
                    CASE f.target_table
                        WHEN 'statement' THEN c_s.last_name || ' ' || c_s.first_name || ' ' || COALESCE(c_s.patronymic, '')
                        WHEN 'appeals' THEN c_a.last_name || ' ' || c_a.first_name || ' ' || COALESCE(c_a.patronymic, '')
                        WHEN 'explanation_protocol' THEN c_ep.last_name || ' ' || c_ep.first_name || ' ' || COALESCE(c_ep.patronymic, '')
                        WHEN 'medical_examination_report' THEN c_mer.last_name || ' ' || c_mer.first_name || ' ' || COALESCE(c_mer.patronymic, '')
                        WHEN 'administrative_protocol' THEN c_ap.last_name || ' ' || c_ap.first_name || ' ' || COALESCE(c_ap.patronymic, '')
                        WHEN 'medical_examination_certificate' THEN c_mc.last_name || ' ' || c_mc.first_name || ' ' || COALESCE(c_mc.patronymic, '')
                        WHEN 'forensic_medical_examination' THEN c_fe.last_name || ' ' || c_fe.first_name || ' ' || COALESCE(c_fe.patronymic, '')
                        WHEN 'resolution' THEN c_r.last_name || ' ' || c_r.first_name || ' ' || COALESCE(c_r.patronymic, '')
                        WHEN 'deal' THEN c_d.last_name || ' ' || c_d.first_name || ' ' || COALESCE(c_d.patronymic, '')
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
                LEFT JOIN deal d_ap ON ap.deal = d_ap.id_deal
                LEFT JOIN citizens c_ap ON d_ap.offender = c_ap.id_citizens
                LEFT JOIN medical_examination_certificate mc ON f.target_table = 'medical_examination_certificate' AND f.document_id = mc.id_medical_examination_certificate
                LEFT JOIN medical_examination_report mer_mc ON mc.medical_examination_report = mer_mc.id_medical_examination_report
                LEFT JOIN citizens c_mc ON mer_mc.patient = c_mc.id_citizens
                LEFT JOIN forensic_medical_examination fe ON f.target_table = 'forensic_medical_examination' AND f.document_id = fe.id_forensic_medical_examination
                LEFT JOIN deal d_fe ON fe.deal = d_fe.id_deal
                LEFT JOIN citizens c_fe ON d_fe.offender = c_fe.id_citizens
                LEFT JOIN resolution r ON f.target_table = 'resolution' AND f.document_id = r.id_resolution
                LEFT JOIN deal d_r ON r.deal = d_r.id_deal
                LEFT JOIN citizens c_r ON d_r.offender = c_r.id_citizens
                LEFT JOIN deal d ON f.target_table = 'deal' AND f.document_id = d.id_deal
                LEFT JOIN citizens c_d ON d.offender = c_d.id_citizens
                WHERE f.user_id = @userId";

            await using var cmdDefault = new NpgsqlCommand(sqlDefault, conn);
            cmdDefault.Parameters.AddWithValue("@userId", userId);
            await using var readerDefault = await cmdDefault.ExecuteReaderAsync();

            while (await readerDefault.ReadAsync())
            {
                var doc = new RecentDocument
                {
                    Id = readerDefault.GetInt32(0),
                    DocumentType = GetDocumentTypeName(readerDefault.GetString(1)),
                    DocumentTypeId = GetDocumentTypeId(readerDefault.GetString(1)),
                    Number = readerDefault.IsDBNull(2) ? null : readerDefault.GetInt32(2),
                    MakingDateAndTime = readerDefault.IsDBNull(3) ? DateTime.Now : readerDefault.GetDateTime(3),
                    CitizenName = readerDefault.IsDBNull(4) ? "Неизвестно" : readerDefault.GetString(4)
                };
                documents.Add(doc);
            }
            
            return documents;
        }
        private string GetDocumentTypeName(string tableName)
        {
            switch (tableName)
            {
                case "statement": return "Заявление";
                case "appeals": return "Обращение";
                case "explanation_protocol": return "Протокол объяснения";
                case "medical_examination_report": return "Направление на мед. освид.";
                case "administrative_protocol": return "Административный протокол";
                case "medical_examination_certificate": return "Акт медицинского освидетельствования";
                case "forensic_medical_examination": return "Судебно-медицинская экспертиза";
                case "resolution": return "Постановление";
                case "deal": return "Дело";
                default: return tableName;
            }
        }

        private int GetDocumentTypeId(string tableName)
        {
            switch (tableName)
            {
                case "statement": return 1;
                case "appeals": return 2;
                case "explanation_protocol": return 3;
                case "medical_examination_report": return 4;
                case "administrative_protocol": return 5;
                case "medical_examination_certificate": return 6;
                case "forensic_medical_examination": return 7;
                case "resolution": return 8;
                default: return 0;
            }
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

            var sql = @"
            INSERT INTO statement 
                (applicant, content, date_and_time, police_officer, number, 
                signature_applicant, signature_police_officer) 
            VALUES (@applicant, @content, @dateTime, @officer, @number, 
                @signApplicant, @signOfficer) 
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

        public async Task<int> SaveDraftAsync(int userId, string documentType, string formDataJson)
        {
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

        public async Task<List<Deal>> GetDealsAsync()
        {
            var deals = new List<Deal>();   
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var role = App.CurrentUserRole;
            string sql;

            if (role == UserRole.ChiefOfPolice || role == UserRole.AdminInspector)
            {
                sql = @"
                    SELECT 
                        d.id_deal,
                        COALESCE(d.deal_number::text, 'Б/Н') AS number,
                        COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_full_name,
                        d.making_date
                    FROM deal d
                    LEFT JOIN citizens c ON d.offender = c.id_citizens
                    ORDER BY d.making_date DESC";
            }
            else
            {
                sql = @"
                    SELECT 
                        d.id_deal,
                        COALESCE(d.deal_number::text, 'Б/Н') AS number,
                        COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_full_name,
                        d.making_date
                    FROM deal d
                    LEFT JOIN citizens c ON d.offender = c.id_citizens
                    WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)
                    ORDER BY d.making_date DESC";
            }

            await using var cmd = new NpgsqlCommand(sql, conn);
            if (role != UserRole.ChiefOfPolice && role != UserRole.AdminInspector)
            {
                cmd.Parameters.AddWithValue("@userId", App.CurrentUserId);
            }
            
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                deals.Add(new Deal
                {
                    Id = reader.GetInt32(0),
                    Number = reader.IsDBNull(1) ? "Б/Н" : reader.GetString(1),
                    CitizenFullName = reader.IsDBNull(2) ? "Неизвестно" : reader.GetString(2),
                    DealDate = reader.GetDateTime(3)
                });
            }

            return deals;
        }

        public async Task<int> CreateCitizenAsync(Citizen citizen)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO citizens (
                    last_name, first_name, patronymic, birthday, 
                    address_registration, 
                    working_place, post, 
                    criminal_record, count_record, 
                    passport_series_and_number, 
                    family_status, education, citizenship
                )
                VALUES (
                    @lastName, @firstName, @patronymic, @birthday,
                    @address,
                    @workPlace, @post,
                    @criminalRecord, @countRecord,
                    @passport,
                    @familyStatus, @education, @citizenship
                )
                RETURNING id_citizens";

            await using var cmd = new NpgsqlCommand(sql, conn);
            
            cmd.Parameters.AddWithValue("@lastName", citizen.LastName);
            cmd.Parameters.AddWithValue("@firstName", citizen.FirstName);
            cmd.Parameters.AddWithValue("@patronymic", citizen.Patronymic ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@birthday", citizen.Birthday);
            cmd.Parameters.AddWithValue("@address", citizen.Address ?? "");
            cmd.Parameters.AddWithValue("@workPlace", citizen.WorkingPlace ?? 1);
            cmd.Parameters.AddWithValue("@post", citizen.Post ?? 1);
            cmd.Parameters.AddWithValue("@criminalRecord", citizen.CriminalRecord);
            cmd.Parameters.AddWithValue("@countRecord", citizen.CountRecord ?? 0);
            cmd.Parameters.AddWithValue("@passport", citizen.Passport ?? "");
            cmd.Parameters.AddWithValue("@familyStatus", citizen.FamilyStatus ?? 1);
            cmd.Parameters.AddWithValue("@education", citizen.Education ?? 1);
            cmd.Parameters.AddWithValue("@citizenship", citizen.Citizenship ?? 1);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
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
                    'medical_examination_certificate' AS table_name,
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

        public async Task<List<MyDocument>> GetUserDocumentsAsync(int userId, UserRole role)
        {
            var documents = new List<MyDocument>();
            
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            // ==================== ПОЛИЦЕЙСКИЙ ====================
            if (role == UserRole.PoliceOfficer)
            {
                // 1. Обращения
                var appealsSql = @"
                    SELECT 
                        a.id_appeals,
                        a.number,
                        a.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        a.content,
                        c.id_citizens,
                        'appeals' as table_name,
                        'Обращение' as document_type
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
                        Number = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                        CreatedAt = reader.GetDateTime(2),
                        CitizenFullName = reader.GetString(3),
                        Content = reader.GetString(4),
                        CitizenId = reader.GetInt32(5),
                        TableName = reader.GetString(6),
                        DocumentType = reader.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader.CloseAsync();

                // 2. Заявления
                var statementsSql = @"
                    SELECT 
                        s.id_statement,
                        s.number,
                        s.date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        s.content,
                        c.id_citizens,
                        'statement' as table_name,
                        'Заявление' as document_type
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
                        Number = reader2.IsDBNull(1) ? null : reader2.GetInt32(1),
                        CreatedAt = reader2.GetDateTime(2),
                        CitizenFullName = reader2.GetString(3),
                        Content = reader2.GetString(4),
                        CitizenId = reader2.GetInt32(5),
                        TableName = reader2.GetString(6),
                        DocumentType = reader2.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader2.CloseAsync();

                // 3. Административные протоколы
                var protocolsSql = @"
                    SELECT 
                        ap.id_protocol,
                        ap.protocol_number,
                        ap.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        ap.description as content,
                        c.id_citizens,
                        'administrative_protocol' as table_name,
                        'Административный протокол' as document_type
                    FROM administrative_protocol ap
                    JOIN deal d ON ap.deal = d.id_deal
                    JOIN citizens c ON d.offender = c.id_citizens
                    WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)";
                
                cmd.CommandText = protocolsSql;
                await using var reader3 = await cmd.ExecuteReaderAsync();
                
                while (await reader3.ReadAsync())
                {
                    documents.Add(new MyDocument
                    {
                        Id = reader3.GetInt32(0),
                        Number = reader3.GetInt32(1),
                        CreatedAt = reader3.GetDateTime(2),
                        CitizenFullName = reader3.GetString(3),
                        Content = reader3.GetString(4),
                        CitizenId = reader3.GetInt32(5),
                        TableName = reader3.GetString(6),
                        DocumentType = reader3.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader3.CloseAsync();

                // 4. Протоколы объяснения
                var explanationsSql = @"
                    SELECT 
                        ep.id_explanation_protocol,
                        ep.number,
                        ep.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        ep.content,
                        c.id_citizens,
                        'explanation_protocol' as table_name,
                        'Протокол объяснения' as document_type
                    FROM explanation_protocol ep
                    JOIN citizens c ON ep.citizen = c.id_citizens
                    JOIN deal d ON ep.deal = d.id_deal
                    WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)";
                
                cmd.CommandText = explanationsSql;
                await using var reader4 = await cmd.ExecuteReaderAsync();
                
                while (await reader4.ReadAsync())
                {
                    documents.Add(new MyDocument
                    {
                        Id = reader4.GetInt32(0),
                        Number = reader4.IsDBNull(1) ? null : reader4.GetInt32(1),
                        CreatedAt = reader4.GetDateTime(2),
                        CitizenFullName = reader4.GetString(3),
                        Content = reader4.GetString(4),
                        CitizenId = reader4.GetInt32(5),
                        TableName = reader4.GetString(6),
                        DocumentType = reader4.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader4.CloseAsync();

                // 5. Направления на мед. освид.
                var reportsSql = @"
                    SELECT 
                        mer.id_medical_examination_report,
                        mer.number,
                        mer.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        mer.content,
                        c.id_citizens,
                        'medical_examination_report' as table_name,
                        'Направление на мед. освид.' as document_type
                    FROM medical_examination_report mer
                    JOIN citizens c ON mer.patient = c.id_citizens
                    JOIN deal d ON mer.deal = d.id_deal
                    WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)";
                
                cmd.CommandText = reportsSql;
                await using var reader5 = await cmd.ExecuteReaderAsync();
                
                while (await reader5.ReadAsync())
                {
                    documents.Add(new MyDocument
                    {
                        Id = reader5.GetInt32(0),
                        Number = reader5.IsDBNull(1) ? null : reader5.GetInt32(1),
                        CreatedAt = reader5.GetDateTime(2),
                        CitizenFullName = reader5.GetString(3),
                        Content = reader5.GetString(4),
                        CitizenId = reader5.GetInt32(5),
                        TableName = reader5.GetString(6),
                        DocumentType = reader5.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader5.CloseAsync();

                // 6. Акты медицинского освидетельствования
                var certificatesSql = @"
                    SELECT 
                        mec.id_medical_examination_certificate,
                        mec.number,
                        mec.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        mec.signs_of_intoxication as content,
                        c.id_citizens,
                        'medical_examination_certificate' as table_name,
                        'Акт медицинского освидетельствования' as document_type
                    FROM medical_examination_certificate mec
                    JOIN medical_examination_report mer ON mec.medical_examination_report = mer.id_medical_examination_report
                    JOIN citizens c ON mer.patient = c.id_citizens
                    JOIN deal d ON mer.deal = d.id_deal
                    WHERE d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)";
                
                cmd.CommandText = certificatesSql;
                await using var reader6 = await cmd.ExecuteReaderAsync();
                
                while (await reader6.ReadAsync())
                {
                    documents.Add(new MyDocument
                    {
                        Id = reader6.GetInt32(0),
                        Number = reader6.IsDBNull(1) ? null : reader6.GetInt32(1),
                        CreatedAt = reader6.GetDateTime(2),
                        CitizenFullName = reader6.GetString(3),
                        Content = reader6.GetString(4),
                        CitizenId = reader6.GetInt32(5),
                        TableName = reader6.GetString(6),
                        DocumentType = reader6.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader6.CloseAsync();
                
                return documents.OrderByDescending(d => d.CreatedAt).ToList();
            }
            
            // ==================== ВРАЧ ====================
            else if (role == UserRole.MedicalExpert)
            {
                // 1. Направления на мед. освид.
                var reportsSql = @"
                    SELECT 
                        mer.id_medical_examination_report,
                        mer.number,
                        mer.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        mer.content,
                        c.id_citizens,
                        'medical_examination_report' as table_name,
                        'Направление на мед. освид.' as document_type
                    FROM medical_examination_report mer
                    JOIN citizens c ON mer.patient = c.id_citizens";
                
                await using var cmd = new NpgsqlCommand(reportsSql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    documents.Add(new MyDocument
                    {
                        Id = reader.GetInt32(0),
                        Number = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                        CreatedAt = reader.GetDateTime(2),
                        CitizenFullName = reader.GetString(3),
                        Content = reader.GetString(4),
                        CitizenId = reader.GetInt32(5),
                        TableName = reader.GetString(6),
                        DocumentType = reader.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader.CloseAsync();

                // 2. Акты медицинского освидетельствования
                var certificatesSql = @"
                    SELECT 
                        mec.id_medical_examination_certificate,
                        mec.number,
                        mec.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        mec.signs_of_intoxication as content,
                        c.id_citizens,
                        'medical_examination_certificate' as table_name,
                        'Акт медицинского освидетельствования' as document_type
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
                        Number = reader2.IsDBNull(1) ? null : reader2.GetInt32(1),
                        CreatedAt = reader2.GetDateTime(2),
                        CitizenFullName = reader2.GetString(3),
                        Content = reader2.GetString(4),
                        CitizenId = reader2.GetInt32(5),
                        TableName = reader2.GetString(6),
                        DocumentType = reader2.GetString(7),
                        IsFavorite = false
                    });
                }
                
                return documents.OrderByDescending(d => d.CreatedAt).ToList();
            }
            
            // ==================== СУДЬЯ ====================
            else if (role == UserRole.Judge)
            {
                // 1. Обращения
                var appealsSql = @"
                    SELECT 
                        a.id_appeals,
                        a.number,
                        a.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        a.content,
                        c.id_citizens,
                        'appeals' as table_name,
                        'Обращение' as document_type
                    FROM appeals a
                    JOIN citizens c ON a.appeal_citizen = c.id_citizens";
                
                await using var cmd = new NpgsqlCommand(appealsSql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    documents.Add(new MyDocument
                    {
                        Id = reader.GetInt32(0),
                        Number = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                        CreatedAt = reader.GetDateTime(2),
                        CitizenFullName = reader.GetString(3),
                        Content = reader.GetString(4),
                        CitizenId = reader.GetInt32(5),
                        TableName = reader.GetString(6),
                        DocumentType = reader.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader.CloseAsync();

                // 2. Заявления
                var statementsSql = @"
                    SELECT 
                        s.id_statement,
                        s.number,
                        s.date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        s.content,
                        c.id_citizens,
                        'statement' as table_name,
                        'Заявление' as document_type
                    FROM statement s
                    JOIN citizens c ON s.applicant = c.id_citizens";
                
                cmd.CommandText = statementsSql;
                await using var reader2 = await cmd.ExecuteReaderAsync();
                
                while (await reader2.ReadAsync())
                {
                    documents.Add(new MyDocument
                    {
                        Id = reader2.GetInt32(0),
                        Number = reader2.IsDBNull(1) ? null : reader2.GetInt32(1),
                        CreatedAt = reader2.GetDateTime(2),
                        CitizenFullName = reader2.GetString(3),
                        Content = reader2.GetString(4),
                        CitizenId = reader2.GetInt32(5),
                        TableName = reader2.GetString(6),
                        DocumentType = reader2.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader2.CloseAsync();

                // 3. Административные протоколы
                var protocolsSql = @"
                    SELECT 
                        ap.id_protocol,
                        ap.protocol_number,
                        ap.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        ap.description as content,
                        c.id_citizens,
                        'administrative_protocol' as table_name,
                        'Административный протокол' as document_type
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
                        Number = reader3.GetInt32(1),
                        CreatedAt = reader3.GetDateTime(2),
                        CitizenFullName = reader3.GetString(3),
                        Content = reader3.GetString(4),
                        CitizenId = reader3.GetInt32(5),
                        TableName = reader3.GetString(6),
                        DocumentType = reader3.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader3.CloseAsync();

                // 4. Протоколы объяснения
                var explanationsSql = @"
                    SELECT 
                        ep.id_explanation_protocol,
                        ep.number,
                        ep.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        ep.content,
                        c.id_citizens,
                        'explanation_protocol' as table_name,
                        'Протокол объяснения' as document_type
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
                        Number = reader4.IsDBNull(1) ? null : reader4.GetInt32(1),
                        CreatedAt = reader4.GetDateTime(2),
                        CitizenFullName = reader4.GetString(3),
                        Content = reader4.GetString(4),
                        CitizenId = reader4.GetInt32(5),
                        TableName = reader4.GetString(6),
                        DocumentType = reader4.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader4.CloseAsync();

                // 5. Направления на мед. освид.
                var reportsSql = @"
                    SELECT 
                        mer.id_medical_examination_report,
                        mer.number,
                        mer.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        mer.content,
                        c.id_citizens,
                        'medical_examination_report' as table_name,
                        'Направление на мед. освид.' as document_type
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
                        Number = reader5.IsDBNull(1) ? null : reader5.GetInt32(1),
                        CreatedAt = reader5.GetDateTime(2),
                        CitizenFullName = reader5.GetString(3),
                        Content = reader5.GetString(4),
                        CitizenId = reader5.GetInt32(5),
                        TableName = reader5.GetString(6),
                        DocumentType = reader5.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader5.CloseAsync();

                // 6. Акты медицинского освидетельствования
                var certificatesSql = @"
                    SELECT 
                        mec.id_medical_examination_certificate,
                        mec.number,
                        mec.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        mec.signs_of_intoxication as content,
                        c.id_citizens,
                        'medical_examination_certificate' as table_name,
                        'Акт медицинского освидетельствования' as document_type
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
                        Number = reader6.IsDBNull(1) ? null : reader6.GetInt32(1),
                        CreatedAt = reader6.GetDateTime(2),
                        CitizenFullName = reader6.GetString(3),
                        Content = reader6.GetString(4),
                        CitizenId = reader6.GetInt32(5),
                        TableName = reader6.GetString(6),
                        DocumentType = reader6.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader6.CloseAsync();

                // 7. Судебно-медицинские экспертизы
                var forensicSql = @"
                    SELECT 
                        fe.id_forensic_medical_examination,
                        fe.number,
                        fe.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        fe.content,
                        c.id_citizens,
                        'forensic_medical_examination' as table_name,
                        'Судебно-медицинская экспертиза' as document_type
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
                        Number = reader7.GetInt32(1),
                        CreatedAt = reader7.GetDateTime(2),
                        CitizenFullName = reader7.GetString(3),
                        Content = reader7.GetString(4),
                        CitizenId = reader7.GetInt32(5),
                        TableName = reader7.GetString(6),
                        DocumentType = reader7.GetString(7),
                        IsFavorite = false
                    });
                }
                await reader7.CloseAsync();

                // 8. Постановления
                var resolutionsSql = @"
                    SELECT 
                        r.id_resolution,
                        r.protocol_number,
                        r.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        r.resolution as content,
                        c.id_citizens,
                        'resolution' as table_name,
                        'Постановление' as document_type
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
                        Number = reader8.GetInt32(1),
                        CreatedAt = reader8.GetDateTime(2),
                        CitizenFullName = reader8.GetString(3),
                        Content = reader8.GetString(4),
                        CitizenId = reader8.GetInt32(5),
                        TableName = reader8.GetString(6),
                        DocumentType = reader8.GetString(7),
                        IsFavorite = false
                    });
                }
                
                return documents.OrderByDescending(d => d.CreatedAt).ToList();
            }
            
            // ==================== ЭКСПЕРТ ====================
            else if (role == UserRole.ForensicExpert)
            {
                var forensicSql = @"
                    SELECT 
                        fe.id_forensic_medical_examination,
                        fe.number,
                        fe.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                        fe.content,
                        c.id_citizens,
                        'forensic_medical_examination' as table_name,
                        'Судебно-медицинская экспертиза' as document_type
                    FROM forensic_medical_examination fe
                    JOIN deal d ON fe.deal = d.id_deal
                    JOIN citizens c ON d.offender = c.id_citizens
                    WHERE fe.expert = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId)";
                
                await using var cmd = new NpgsqlCommand(forensicSql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                await using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    documents.Add(new MyDocument
                    {
                        Id = reader.GetInt32(0),
                        Number = reader.GetInt32(1),
                        CreatedAt = reader.GetDateTime(2),
                        CitizenFullName = reader.GetString(3),
                        Content = reader.GetString(4),
                        CitizenId = reader.GetInt32(5),
                        TableName = reader.GetString(6),
                        DocumentType = reader.GetString(7),
                        IsFavorite = false
                    });
                }
                
                return documents.OrderByDescending(d => d.CreatedAt).ToList();
            }

            // ==================== НАЧАЛЬНИК ====================
            else if (role == UserRole.ChiefOfPolice)
            {
                // 1. Обращения
                var appealsSql = @"
                    SELECT 
                        a.id_appeals,
                        a.number,
                        a.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        a.content,
                        c.id_citizens,
                        'appeals',
                        'Обращение'
                    FROM appeals a
                    JOIN citizens c ON a.appeal_citizen = c.id_citizens";
                
                await using var cmd = new NpgsqlCommand(appealsSql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader));
                await reader.CloseAsync();
                
                // 2. Заявления
                var statementsSql = @"
                    SELECT 
                        s.id_statement,
                        s.number,
                        s.date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        s.content,
                        c.id_citizens,
                        'statement',
                        'Заявление'
                    FROM statement s
                    JOIN citizens c ON s.applicant = c.id_citizens";
                
                cmd.CommandText = statementsSql;
                await using var reader2 = await cmd.ExecuteReaderAsync();
                while (await reader2.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader2));
                await reader2.CloseAsync();
                
                // 3. Протоколы объяснения
                var explanationsSql = @"
                    SELECT 
                        ep.id_explanation_protocol,
                        ep.number,
                        ep.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        ep.content,
                        c.id_citizens,
                        'explanation_protocol',
                        'Протокол объяснения'
                    FROM explanation_protocol ep
                    JOIN citizens c ON ep.citizen = c.id_citizens";
                
                cmd.CommandText = explanationsSql;
                await using var reader3 = await cmd.ExecuteReaderAsync();
                while (await reader3.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader3));
                await reader3.CloseAsync();
                
                // 4. Административные протоколы
                var protocolsSql = @"
                    SELECT 
                        ap.id_protocol,
                        ap.protocol_number,
                        ap.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        ap.description,
                        c.id_citizens,
                        'administrative_protocol',
                        'Административный протокол'
                    FROM administrative_protocol ap
                    JOIN deal d ON ap.deal = d.id_deal
                    JOIN citizens c ON d.offender = c.id_citizens";
                
                cmd.CommandText = protocolsSql;
                await using var reader4 = await cmd.ExecuteReaderAsync();
                while (await reader4.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader4));
                await reader4.CloseAsync();
                
                // 5. Направления на мед. освид.
                var reportsSql = @"
                    SELECT 
                        mer.id_medical_examination_report,
                        mer.number,
                        mer.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        mer.content,
                        c.id_citizens,
                        'medical_examination_report',
                        'Направление на мед. освид.'
                    FROM medical_examination_report mer
                    JOIN citizens c ON mer.patient = c.id_citizens";
                
                cmd.CommandText = reportsSql;
                await using var reader5 = await cmd.ExecuteReaderAsync();
                while (await reader5.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader5));
                await reader5.CloseAsync();
                
                // 6. Акты мед. освид.
                var certificatesSql = @"
                    SELECT 
                        mec.id_medical_examination_certificate,
                        mec.number,
                        mec.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        mec.signs_of_intoxication,
                        c.id_citizens,
                        'medical_examination_certificate',
                        'Акт медицинского освидетельствования'
                    FROM medical_examination_certificate mec
                    JOIN medical_examination_report mer ON mec.medical_examination_report = mer.id_medical_examination_report
                    JOIN citizens c ON mer.patient = c.id_citizens";
                
                cmd.CommandText = certificatesSql;
                await using var reader6 = await cmd.ExecuteReaderAsync();
                while (await reader6.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader6));
                await reader6.CloseAsync();
                
                // 7. Судебно-медицинские экспертизы
                var forensicSql = @"
                    SELECT 
                        fe.id_forensic_medical_examination,
                        fe.number,
                        fe.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        fe.content,
                        c.id_citizens,
                        'forensic_medical_examination',
                        'Судебно-медицинская экспертиза'
                    FROM forensic_medical_examination fe
                    JOIN deal d ON fe.deal = d.id_deal
                    JOIN citizens c ON d.offender = c.id_citizens";
                
                cmd.CommandText = forensicSql;
                await using var reader7 = await cmd.ExecuteReaderAsync();
                while (await reader7.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader7));
                await reader7.CloseAsync();
                
                // 8. Постановления
                var resolutionSql = @"
                    SELECT 
                        r.id_resolution,
                        r.protocol_number,
                        r.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        r.resolution,
                        c.id_citizens,
                        'resolution',
                        'Постановление'
                    FROM resolution r
                    JOIN deal d ON r.deal = d.id_deal
                    JOIN citizens c ON d.offender = c.id_citizens";
                
                cmd.CommandText = resolutionSql;
                await using var reader8 = await cmd.ExecuteReaderAsync();
                while (await reader8.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader8));
                await reader8.CloseAsync();
                
                return documents.OrderByDescending(d => d.CreatedAt).ToList();
            }

            // ==================== ИНСПЕКТОР АДМ. ПРАКТИКИ ====================
            else if (role == UserRole.AdminInspector)
            {
                // Получаем citizen_post_id для инспектора
                var getOfficerIdSql = "SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId";
                await using var officerCmd = new NpgsqlCommand(getOfficerIdSql, conn);
                officerCmd.Parameters.AddWithValue("@userId", userId);
                var citizenPostIdObj = await officerCmd.ExecuteScalarAsync();
                
                if (citizenPostIdObj == null)
                {
                    return documents;
                }
                
                int citizenPostId = Convert.ToInt32(citizenPostIdObj);
                
                // 1. Обращения
                var appealsSql = @"
                    SELECT 
                        a.id_appeals,
                        a.number,
                        a.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        a.content,
                        c.id_citizens,
                        'appeals',
                        'Обращение'
                    FROM appeals a
                    JOIN citizens c ON a.appeal_citizen = c.id_citizens
                    WHERE a.police_officer = @officerId";
                
                await using var cmd = new NpgsqlCommand(appealsSql, conn);
                cmd.Parameters.AddWithValue("@officerId", citizenPostId);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader));
                await reader.CloseAsync();
                
                // 2. Заявления
                var statementsSql = @"
                    SELECT 
                        s.id_statement,
                        s.number,
                        s.date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        s.content,
                        c.id_citizens,
                        'statement',
                        'Заявление'
                    FROM statement s
                    JOIN citizens c ON s.applicant = c.id_citizens
                    WHERE s.police_officer = @officerId";
                
                cmd.CommandText = statementsSql;
                await using var reader2 = await cmd.ExecuteReaderAsync();
                while (await reader2.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader2));
                await reader2.CloseAsync();
                
                // 3. Протоколы объяснения
                var explanationsSql = @"
                    SELECT 
                        ep.id_explanation_protocol,
                        ep.number,
                        ep.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        ep.content,
                        c.id_citizens,
                        'explanation_protocol',
                        'Протокол объяснения'
                    FROM explanation_protocol ep
                    JOIN citizens c ON ep.citizen = c.id_citizens
                    JOIN deal d ON ep.deal = d.id_deal
                    WHERE d.police_officer = @officerId";
                
                cmd.CommandText = explanationsSql;
                await using var reader3 = await cmd.ExecuteReaderAsync();
                while (await reader3.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader3));
                await reader3.CloseAsync();
                
                // 4. Административные протоколы
                var protocolsSql = @"
                    SELECT 
                        ap.id_protocol,
                        ap.protocol_number,
                        ap.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        ap.description,
                        c.id_citizens,
                        'administrative_protocol',
                        'Административный протокол'
                    FROM administrative_protocol ap
                    JOIN deal d ON ap.deal = d.id_deal
                    JOIN citizens c ON d.offender = c.id_citizens
                    WHERE d.police_officer = @officerId";
                
                cmd.CommandText = protocolsSql;
                await using var reader4 = await cmd.ExecuteReaderAsync();
                while (await reader4.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader4));
                await reader4.CloseAsync();
                
                // 5. Направления на мед. освид.
                var reportsSql = @"
                    SELECT 
                        mer.id_medical_examination_report,
                        mer.number,
                        mer.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        mer.content,
                        c.id_citizens,
                        'medical_examination_report',
                        'Направление на мед. освид.'
                    FROM medical_examination_report mer
                    JOIN citizens c ON mer.patient = c.id_citizens
                    JOIN deal d ON mer.deal = d.id_deal
                    WHERE d.police_officer = @officerId";
                
                cmd.CommandText = reportsSql;
                await using var reader5 = await cmd.ExecuteReaderAsync();
                while (await reader5.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader5));
                await reader5.CloseAsync();
                
                // 6. Акты мед. освид.
                var certificatesSql = @"
                    SELECT 
                        mec.id_medical_examination_certificate,
                        mec.number,
                        mec.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        mec.signs_of_intoxication,
                        c.id_citizens,
                        'medical_examination_certificate',
                        'Акт медицинского освидетельствования'
                    FROM medical_examination_certificate mec
                    JOIN medical_examination_report mer ON mec.medical_examination_report = mer.id_medical_examination_report
                    JOIN citizens c ON mer.patient = c.id_citizens
                    JOIN deal d ON mer.deal = d.id_deal
                    WHERE d.police_officer = @officerId";
                
                cmd.CommandText = certificatesSql;
                await using var reader6 = await cmd.ExecuteReaderAsync();
                while (await reader6.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader6));
                await reader6.CloseAsync();
                
                // 7. Судебно-медицинские экспертизы
                var forensicSql = @"
                    SELECT 
                        fe.id_forensic_medical_examination,
                        fe.number,
                        fe.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        fe.content,
                        c.id_citizens,
                        'forensic_medical_examination',
                        'Судебно-медицинская экспертиза'
                    FROM forensic_medical_examination fe
                    JOIN deal d ON fe.deal = d.id_deal
                    JOIN citizens c ON d.offender = c.id_citizens
                    WHERE d.police_officer = @officerId";
                
                cmd.CommandText = forensicSql;
                await using var reader7 = await cmd.ExecuteReaderAsync();
                while (await reader7.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader7));
                await reader7.CloseAsync();
                
                // 8. Постановления
                var resolutionSql = @"
                    SELECT 
                        r.id_resolution,
                        r.protocol_number,
                        r.making_date_and_time,
                        c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                        r.resolution,
                        c.id_citizens,
                        'resolution',
                        'Постановление'
                    FROM resolution r
                    JOIN deal d ON r.deal = d.id_deal
                    JOIN citizens c ON d.offender = c.id_citizens
                    WHERE d.police_officer = @officerId";
                
                cmd.CommandText = resolutionSql;
                await using var reader8 = await cmd.ExecuteReaderAsync();
                while (await reader8.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader8));
                await reader8.CloseAsync();
                return documents.OrderByDescending(d => d.CreatedAt).ToList();
            }
            return documents;
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

        private MyDocument MapToMyDocument(NpgsqlDataReader reader)
        {
            return new MyDocument
            {
                Id = reader.GetInt32(0),
                Number = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                CreatedAt = reader.GetDateTime(2),
                CitizenFullName = reader.GetString(3),
                Content = reader.GetString(4),
                CitizenId = reader.GetInt32(5),
                TableName = reader.GetString(6),
                DocumentType = reader.GetString(7),
                IsFavorite = reader.GetBoolean(8)            
            };
        }

        private MyDocument MapToMyDocumentSimple(NpgsqlDataReader reader)
        {
            return new MyDocument
            {
                Id = reader.GetInt32(0),
                Number = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                CreatedAt = reader.GetDateTime(2),
                CitizenFullName = reader.GetString(3),
                Content = reader.GetString(4),
                CitizenId = reader.GetInt32(5),
                TableName = reader.GetString(6),
                DocumentType = reader.GetString(7),
                IsFavorite = false
            };
        }

        public async Task ToggleFavoriteAsync(int userId, string targetTable, int documentId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var checkSql = @"SELECT COUNT(*) 
            FROM user_favorites 
            WHERE user_id = @userId AND target_table = @targetTable AND document_id = @documentId";
            await using var checkCmd = new NpgsqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@userId", userId);
            checkCmd.Parameters.AddWithValue("@targetTable", targetTable);
            checkCmd.Parameters.AddWithValue("@documentId", documentId);
            
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

            if (exists)
            {
                var deleteSql = @"DELETE FROM user_favorites 
                WHERE user_id = @userId AND target_table = @targetTable AND document_id = @documentId";
                await using var deleteCmd = new NpgsqlCommand(deleteSql, conn);
                deleteCmd.Parameters.AddWithValue("@userId", userId);
                deleteCmd.Parameters.AddWithValue("@targetTable", targetTable);
                deleteCmd.Parameters.AddWithValue("@documentId", documentId);
                await deleteCmd.ExecuteNonQueryAsync();
            }
            else
            {
                var insertSql = @"INSERT INTO user_favorites (user_id, target_table, document_id) 
                VALUES (@userId, @targetTable, @documentId)";
                await using var insertCmd = new NpgsqlCommand(insertSql, conn);
                insertCmd.Parameters.AddWithValue("@userId", userId);
                insertCmd.Parameters.AddWithValue("@targetTable", targetTable);
                insertCmd.Parameters.AddWithValue("@documentId", documentId);
                await insertCmd.ExecuteNonQueryAsync();
            }
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
            return deleted;
        }

        public async Task<DocumentFull> GetFullDocumentAsync(string tableName, int documentId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = tableName switch
            {
                "medical_examination_certificate" => @"
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

                "forensic_medical_examination" => @"
                    SELECT 
                        'Судебно-медицинская экспертиза' AS document_type,
                        COALESCE(fe.number::text, 'Б/Н') AS number,
                        fe.making_date_and_time AS created_at,
                        COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                        fe.content AS content,
                        COALESCE(d.deal_number::text, 'Б/Н') AS deal_number,
                        fe.content AS description,
                        '' AS other_information,
                        COALESCE(fe.signature_expert, false) AS signature,
                        'Не указан' AS first_witness,
                        'Не указан' AS second_witness,
                        COALESCE(expert.last_name || ' ' || expert.first_name || ' ' || COALESCE(expert.patronymic, ''), 'Не указан') AS officer_name,
                        'Не указана' AS article_name,
                        '' AS patient_name,
                        '' AS report_type,
                        '' AS signs_of_intoxication
                    FROM forensic_medical_examination fe
                    LEFT JOIN deal d ON fe.deal = d.id_deal
                    LEFT JOIN citizens c ON d.offender = c.id_citizens
                    LEFT JOIN citizens expert ON fe.expert = expert.id_citizens
                    WHERE fe.id_forensic_medical_examination = @id",
                    
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
                
                "resolution" => @"
                SELECT 
                    'Постановление' AS document_type,
                    COALESCE(r.protocol_number::text, 'Б/Н') AS number,
                    r.making_date_and_time AS created_at,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                    r.resolution AS content,
                    COALESCE(d.deal_number::text, 'Б/Н') AS deal_number,
                    r.resolution AS description,
                    '' AS other_information,
                    false AS signature,
                    'Не указан' AS first_witness,
                    'Не указан' AS second_witness,
                    COALESCE(officer.last_name || ' ' || officer.first_name || ' ' || COALESCE(officer.patronymic, ''), 'Не указан') AS officer_name,
                    COALESCE(a.number_of_article::text || ' - ' || a.description, 'Не указана') AS article_name,
                    '' AS patient_name,
                    '' AS report_type,
                    '' AS signs_of_intoxication
                FROM resolution r
                LEFT JOIN deal d ON r.deal = d.id_deal
                LEFT JOIN citizens c ON d.offender = c.id_citizens
                LEFT JOIN citizens_and_posts cap ON d.police_officer = cap.id_citizens_and_posts
                LEFT JOIN citizens officer ON cap.citizen = officer.id_citizens
                LEFT JOIN article a ON d.article = a.id_article
                WHERE r.id_resolution = @id",
                    
                "deal" => @"
                    SELECT 
                        'Дело' AS document_type,
                        COALESCE(d.deal_number::text, 'Б/Н') AS number,
                        d.making_date AS created_at,
                        COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_name,
                        'Дело №' || d.deal_number AS content,
                        '' AS deal_number,
                        'Дело об административном правонарушении' AS description,
                        COALESCE('Нарушитель: ' || c.last_name || ' ' || c.first_name, 'Не указан') AS other_information,
                        false AS signature,
                        COALESCE(cw1.last_name || ' ' || cw1.first_name, 'Не указан') AS first_witness,
                        COALESCE(cw2.last_name || ' ' || cw2.first_name, 'Не указан') AS second_witness,
                        COALESCE(officer.last_name || ' ' || officer.first_name, 'Не указан') AS officer_name,
                        COALESCE(a.number_of_article::text || ' - ' || a.description, 'Не указана') AS article_name,
                        '' AS patient_name,
                        '' AS report_type,
                        '' AS signs_of_intoxication
                    FROM deal d
                    LEFT JOIN citizens c ON d.offender = c.id_citizens
                    LEFT JOIN citizens cw1 ON d.first_witness = cw1.id_citizens
                    LEFT JOIN citizens cw2 ON d.second_witness = cw2.id_citizens
                    LEFT JOIN citizens_and_posts cap ON d.police_officer = cap.id_citizens_and_posts
                    LEFT JOIN citizens officer ON cap.citizen = officer.id_citizens
                    LEFT JOIN article a ON d.article = a.id_article
                    WHERE d.id_deal = @id",
                    
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
        public async Task<List<MyDocument>> GetCitizenDocumentsAsync(int citizenId)
        {
            var documents = new List<MyDocument>();
            
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // 1. Заявления
            var statementsSql = @"
                SELECT 
                    s.id_statement,
                    s.number,
                    s.date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    s.content,
                    c.id_citizens,
                    'statement',
                    'Заявление'
                FROM statement s
                JOIN citizens c ON s.applicant = c.id_citizens
                WHERE s.applicant = @citizenId";

            await using var cmd = new NpgsqlCommand(statementsSql, conn);
            cmd.Parameters.AddWithValue("@citizenId", citizenId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader));
            await reader.CloseAsync();

            // 2. Обращения
            var appealsSql = @"
                SELECT 
                    a.id_appeals,
                    a.number,
                    a.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    a.content,
                    c.id_citizens,
                    'appeals',
                    'Обращение'
                FROM appeals a
                JOIN citizens c ON a.appeal_citizen = c.id_citizens
                WHERE a.appeal_citizen = @citizenId";

            cmd.CommandText = appealsSql;
            await using var reader2 = await cmd.ExecuteReaderAsync();
            while (await reader2.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader2));
            await reader2.CloseAsync();

            // 3. Протоколы объяснения
            var explanationSql = @"
                SELECT 
                    ep.id_explanation_protocol,
                    ep.number,
                    ep.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    ep.content,
                    c.id_citizens,
                    'explanation_protocol',
                    'Протокол объяснения'
                FROM explanation_protocol ep
                JOIN citizens c ON ep.citizen = c.id_citizens
                WHERE ep.citizen = @citizenId";

            cmd.CommandText = explanationSql;
            await using var reader3 = await cmd.ExecuteReaderAsync();
            while (await reader3.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader3));
            await reader3.CloseAsync();

            // 4. Направления на мед. освид.
            var medicalSql = @"
                SELECT 
                    mer.id_medical_examination_report,
                    mer.number,
                    mer.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    mer.content,
                    c.id_citizens,
                    'medical_examination_report',
                    'Направление на мед. освид.'
                FROM medical_examination_report mer
                JOIN citizens c ON mer.patient = c.id_citizens
                WHERE mer.patient = @citizenId";

            cmd.CommandText = medicalSql;
            await using var reader4 = await cmd.ExecuteReaderAsync();
            while (await reader4.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader4));
            await reader4.CloseAsync();

            // 5. Административные протоколы
            var protocolSql = @"
                SELECT 
                    ap.id_protocol,
                    ap.protocol_number,
                    ap.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    ap.description,
                    c.id_citizens,
                    'administrative_protocol',
                    'Административный протокол'
                FROM administrative_protocol ap
                JOIN deal d ON ap.deal = d.id_deal
                JOIN citizens c ON d.offender = c.id_citizens
                WHERE d.offender = @citizenId";

            cmd.CommandText = protocolSql;
            await using var reader5 = await cmd.ExecuteReaderAsync();
            while (await reader5.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader5));
            await reader5.CloseAsync();

            // 6. Акты медицинского освидетельствования
            var certificateSql = @"
                SELECT 
                    mec.id_medical_examination_certificate,
                    mec.number,
                    mec.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    mec.signs_of_intoxication,
                    c.id_citizens,
                    'medical_examination_certificate',
                    'Акт медицинского освидетельствования'
                FROM medical_examination_certificate mec
                JOIN medical_examination_report mer ON mec.medical_examination_report = mer.id_medical_examination_report
                JOIN citizens c ON mer.patient = c.id_citizens
                WHERE mer.patient = @citizenId";

            cmd.CommandText = certificateSql;
            await using var reader6 = await cmd.ExecuteReaderAsync();
            while (await reader6.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader6));
            await reader6.CloseAsync();

            // 7. Судебно-медицинские экспертизы
            var forensicSql = @"
                SELECT 
                    fe.id_forensic_medical_examination,
                    fe.number,
                    fe.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    fe.content,
                    c.id_citizens,
                    'forensic_medical_examination',
                    'Судебно-медицинская экспертиза'
                FROM forensic_medical_examination fe
                JOIN deal d ON fe.deal = d.id_deal
                JOIN citizens c ON d.offender = c.id_citizens
                WHERE d.offender = @citizenId";

            cmd.CommandText = forensicSql;
            await using var reader7 = await cmd.ExecuteReaderAsync();
            while (await reader7.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader7));
            await reader7.CloseAsync();

            // 8. Постановления
            var resolutionSql = @"
                SELECT 
                    r.id_resolution,
                    r.protocol_number,
                    r.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    r.resolution,
                    c.id_citizens,
                    'resolution',
                    'Постановление'
                FROM resolution r
                JOIN deal d ON r.deal = d.id_deal
                JOIN citizens c ON d.offender = c.id_citizens
                WHERE d.offender = @citizenId";

            cmd.CommandText = resolutionSql;
            await using var reader8 = await cmd.ExecuteReaderAsync();
            while (await reader8.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader8));
            await reader8.CloseAsync();

            return documents.OrderByDescending(d => d.CreatedAt).ToList();
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

        public async Task<List<Deal>> GetDealsByUserAsync(int userId)
        {
            var deals = new List<Deal>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Узнай правильное название поля даты в таблице deal
            // Временно убираем сортировку по дате
            var sql = @"
                SELECT 
                    d.id_deal,
                    d.deal_number,
                    COALESCE(c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''), 'Неизвестно') AS citizen_full_name
                FROM deal d
                LEFT JOIN citizens c ON d.offender = c.id_citizens
                ORDER BY d.id_deal DESC
                LIMIT 50";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                deals.Add(new Deal
                {
                    Id = reader.GetInt32(0),
                    Number = reader.GetInt32(1).ToString(),
                    DealDate = DateTime.Now, // временно, пока нет поля даты
                    CitizenFullName = reader.GetString(2)
                });
            }
            return deals;
        }

        // 2. Поиск дел
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

       public async Task<int> CreateDealAsync(int dealNumber, int offenderId, int? firstWitnessId, int? secondWitnessId, 
            int policeOfficerId, int articleId, int responsibilityId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO deal (deal_number, settlement, offender, first_witness, second_witness, police_officer, article, responsibility, making_date)
                VALUES (@dealNumber, 1, @offender, @firstWitness, @secondWitness, @policeOfficer, @article, @responsibility, NOW())
                RETURNING id_deal";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dealNumber", dealNumber);
            cmd.Parameters.AddWithValue("@offender", offenderId);
            cmd.Parameters.AddWithValue("@firstWitness", firstWitnessId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@secondWitness", secondWitnessId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@policeOfficer", policeOfficerId);
            cmd.Parameters.AddWithValue("@article", articleId);
            cmd.Parameters.AddWithValue("@responsibility", responsibilityId);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // 4. Получение дела по ID
        public async Task<Deal?> GetDealByIdAsync(int dealId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    d.id_deal,
                    d.deal_number,
                    d.making_date,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, '') AS citizen_full_name,
                    c.id_citizens AS offender_id
                FROM deal d
                LEFT JOIN citizens c ON d.offender = c.id_citizens
                WHERE d.id_deal = @dealId";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dealId", dealId);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Deal
                {
                    Id = reader.GetInt32(0),
                    Number = reader.GetInt32(1).ToString(),
                    DealDate = reader.GetDateTime(2),
                    CitizenFullName = reader.IsDBNull(3) ? "Неизвестно" : reader.GetString(3)
                };
            }
            return null;
        }

       public async Task<List<UserWithRole>> GetOfficersAsync(int currentUserId)
        {
            var officers = new List<UserWithRole>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Сначала получаем working_place текущего пользователя (начальника)
            var getWorkPlaceSql = @"
                SELECT c.working_place
                FROM users u
                JOIN user_citizen_link ucl ON u.id = ucl.user_id
                JOIN citizens_and_posts cap ON ucl.citizen_post_id = cap.id_citizens_and_posts
                JOIN citizens c ON cap.citizen = c.id_citizens
                WHERE u.id = @userId";

            await using var workPlaceCmd = new NpgsqlCommand(getWorkPlaceSql, conn);
            workPlaceCmd.Parameters.AddWithValue("@userId", currentUserId);
            var currentWorkPlace = await workPlaceCmd.ExecuteScalarAsync();
            
            Console.WriteLine($"[DEBUG] working_place начальника: {currentWorkPlace}");
            
            // Если у начальника нет working_place, возвращаем пустой список
            if (currentWorkPlace == null || currentWorkPlace == DBNull.Value)
            {
                Console.WriteLine("[DEBUG] У начальника нет working_place");
                return officers;
            }
            
            int workPlaceId = Convert.ToInt32(currentWorkPlace);
            Console.WriteLine($"[DEBUG] Ищем сотрудников с working_place = {workPlaceId}");

            var sql = @"
                SELECT 
                    u.id, 
                    u.username, 
                    u.last_name, 
                    u.first_name, 
                    u.patronymic, 
                    u.role,
                    r.rank,
                    EXTRACT(YEAR FROM AGE(c.birthday)) as age,
                    s.name as working_place_name
                FROM users u
                INNER JOIN user_citizen_link ucl ON u.id = ucl.user_id
                INNER JOIN citizens_and_posts cap ON ucl.citizen_post_id = cap.id_citizens_and_posts
                INNER JOIN citizens c ON cap.citizen = c.id_citizens
                LEFT JOIN cap_ranks cr ON ucl.id = cr.user_citizen_link
                LEFT JOIN rank r ON cr.rank = r.id
                LEFT JOIN structures s ON c.working_place = s.id_structures
                WHERE u.role IN (1, 5)  -- Полицейские и инспекторы
                AND c.working_place = @workPlaceId
                ORDER BY u.last_name, u.first_name";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@workPlaceId", workPlaceId);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var officer = new UserWithRole
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    LastName = reader.GetString(2),
                    FirstName = reader.GetString(3),
                    Patronymic = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Role = (UserRole)reader.GetInt32(5),
                    Rank = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Age = reader.IsDBNull(7) ? null : (int?)reader.GetDecimal(7),
                    WorkPlace = reader.IsDBNull(8) ? null : reader.GetString(8)
                };
                
                Console.WriteLine($"[DEBUG] Найден сотрудник: {officer.FullName}, ID={officer.Id}, Role={officer.Role}, WorkPlace={officer.WorkPlace}");
                officers.Add(officer);
            }
            return officers;
        }

        public async Task<List<DealStatItem>> GetFilteredDealsAsync(DateTime? dateFrom, DateTime? dateTo, int? officerId, string articleNumber)
        {
            var items = new List<DealStatItem>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            if (dateFrom.HasValue)
            {
                conditions.Add("d.making_date >= @dateFrom");
                parameters.Add("@dateFrom", dateFrom.Value);
            }
            if (dateTo.HasValue)
            {
                conditions.Add("d.making_date <= @dateTo");
                parameters.Add("@dateTo", dateTo.Value);
            }
            if (officerId.HasValue)
            {
                conditions.Add("d.police_officer = (SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @officerId)");
                parameters.Add("@officerId", officerId.Value);
            }
            if (!string.IsNullOrWhiteSpace(articleNumber))
            {
                conditions.Add("CAST(a.number_of_article AS TEXT) ILIKE @articleNumber");
                parameters.Add("@articleNumber", $"%{articleNumber}%");
            }

            string whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";

           var sql = $@"
            SELECT DISTINCT
                d.id_deal,
                d.deal_number,
                a.number_of_article,
                a.description,
                (SELECT u.last_name || ' ' || u.first_name 
                FROM user_citizen_link ucl 
                JOIN users u ON ucl.user_id = u.id 
                WHERE ucl.citizen_post_id = d.police_officer 
                LIMIT 1) AS officer_name,
                d.making_date,
                CASE WHEN EXISTS (SELECT 1 FROM resolution r WHERE r.deal = d.id_deal) THEN 1 ELSE 0 END AS has_resolution
            FROM deal d
            LEFT JOIN article a ON d.article = a.id_article
            {whereClause}
            ORDER BY d.making_date DESC";

            await using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new DealStatItem
                {
                    DealId = reader.GetInt32(0),
                    DealNumber = reader.GetInt32(1).ToString(),
                    ArticleName = reader.IsDBNull(2) ? "Не указана" : $"{reader.GetDecimal(2)} - {reader.GetString(3)}",
                    OfficerName = reader.IsDBNull(4) ? "Не указан" : reader.GetString(4),
                    DealDate = reader.GetDateTime(5),
                    HasResolution = reader.GetInt32(6) == 1
                });
            }
            return items;
        }

        public async Task<int?> GetArticleIdByNumberAsync(string articleNumber)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Берём только число из введённой строки
            string searchNumber = articleNumber.Trim().Split(' ')[0];
            
            var sql = "SELECT id_article FROM article WHERE CAST(number_of_article AS TEXT) LIKE @number";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@number", $"{searchNumber}%");
            
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : null;
        }


        public async Task<List<MyDocument>> GetChiefDocumentsAsync(int userId)
        {
            var documents = new List<MyDocument>();
            
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Получаем citizen_post_id для начальника
            var getOfficerIdSql = "SELECT citizen_post_id FROM user_citizen_link WHERE user_id = @userId";
            await using var officerCmd = new NpgsqlCommand(getOfficerIdSql, conn);
            officerCmd.Parameters.AddWithValue("@userId", userId);
            var citizenPostIdObj = await officerCmd.ExecuteScalarAsync();
            
            if (citizenPostIdObj == null)
            {
                return documents;
            }
            
            int citizenPostId = Convert.ToInt32(citizenPostIdObj);
            
            // 1. Обращения
            var appealsSql = @"
                SELECT 
                    a.id_appeals,
                    a.number,
                    a.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    a.content,
                    c.id_citizens,
                    'appeals',
                    'Обращение'
                FROM appeals a
                JOIN citizens c ON a.appeal_citizen = c.id_citizens
                WHERE a.police_officer = @officerId";
            
            await using var cmd = new NpgsqlCommand(appealsSql, conn);
            cmd.Parameters.AddWithValue("@officerId", citizenPostId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader));
            await reader.CloseAsync();
            
            // 2. Заявления
            var statementsSql = @"
                SELECT 
                    s.id_statement,
                    s.number,
                    s.date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    s.content,
                    c.id_citizens,
                    'statement',
                    'Заявление'
                FROM statement s
                JOIN citizens c ON s.applicant = c.id_citizens
                WHERE s.police_officer = @officerId";
            
            cmd.CommandText = statementsSql;
            await using var reader2 = await cmd.ExecuteReaderAsync();
            while (await reader2.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader2));
            await reader2.CloseAsync();
            
            // 3. Протоколы объяснения
            var explanationsSql = @"
                SELECT 
                    ep.id_explanation_protocol,
                    ep.number,
                    ep.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    ep.content,
                    c.id_citizens,
                    'explanation_protocol',
                    'Протокол объяснения'
                FROM explanation_protocol ep
                JOIN citizens c ON ep.citizen = c.id_citizens
                JOIN deal d ON ep.deal = d.id_deal
                WHERE d.police_officer = @officerId";
            
            cmd.CommandText = explanationsSql;
            await using var reader3 = await cmd.ExecuteReaderAsync();
            while (await reader3.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader3));
            await reader3.CloseAsync();
            
            // 4. Административные протоколы
            var protocolsSql = @"
                SELECT 
                    ap.id_protocol,
                    ap.protocol_number,
                    ap.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    ap.description,
                    c.id_citizens,
                    'administrative_protocol',
                    'Административный протокол'
                FROM administrative_protocol ap
                JOIN deal d ON ap.deal = d.id_deal
                JOIN citizens c ON d.offender = c.id_citizens
                WHERE d.police_officer = @officerId";
            
            cmd.CommandText = protocolsSql;
            await using var reader4 = await cmd.ExecuteReaderAsync();
            while (await reader4.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader4));
            await reader4.CloseAsync();
            
            // 5. Направления на мед. освид.
            var reportsSql = @"
                SELECT 
                    mer.id_medical_examination_report,
                    mer.number,
                    mer.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    mer.content,
                    c.id_citizens,
                    'medical_examination_report',
                    'Направление на мед. освид.'
                FROM medical_examination_report mer
                JOIN citizens c ON mer.patient = c.id_citizens
                JOIN deal d ON mer.deal = d.id_deal
                WHERE d.police_officer = @officerId";
            
            cmd.CommandText = reportsSql;
            await using var reader5 = await cmd.ExecuteReaderAsync();
            while (await reader5.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader5));
            await reader5.CloseAsync();
            
            // 6. Акты мед. освид.
            var certificatesSql = @"
                SELECT 
                    mec.id_medical_examination_certificate,
                    mec.number,
                    mec.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    mec.signs_of_intoxication,
                    c.id_citizens,
                    'medical_examination_certificate',
                    'Акт медицинского освидетельствования'
                FROM medical_examination_certificate mec
                JOIN medical_examination_report mer ON mec.medical_examination_report = mer.id_medical_examination_report
                JOIN citizens c ON mer.patient = c.id_citizens
                JOIN deal d ON mer.deal = d.id_deal
                WHERE d.police_officer = @officerId";
            
            cmd.CommandText = certificatesSql;
            await using var reader6 = await cmd.ExecuteReaderAsync();
            while (await reader6.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader6));
            await reader6.CloseAsync();
            
            // 7. Судебно-медицинские экспертизы
            var forensicSql = @"
                SELECT 
                    fe.id_forensic_medical_examination,
                    fe.number,
                    fe.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    fe.content,
                    c.id_citizens,
                    'forensic_medical_examination',
                    'Судебно-медицинская экспертиза'
                FROM forensic_medical_examination fe
                JOIN deal d ON fe.deal = d.id_deal
                JOIN citizens c ON d.offender = c.id_citizens
                WHERE d.police_officer = @officerId";
            
            cmd.CommandText = forensicSql;
            await using var reader7 = await cmd.ExecuteReaderAsync();
            while (await reader7.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader7));
            await reader7.CloseAsync();
            
            // 8. Постановления
            var resolutionSql = @"
                SELECT 
                    r.id_resolution,
                    r.protocol_number,
                    r.making_date_and_time,
                    c.last_name || ' ' || c.first_name || ' ' || COALESCE(c.patronymic, ''),
                    r.resolution,
                    c.id_citizens,
                    'resolution',
                    'Постановление'
                FROM resolution r
                JOIN deal d ON r.deal = d.id_deal
                JOIN citizens c ON d.offender = c.id_citizens
                WHERE d.police_officer = @officerId";
            
            cmd.CommandText = resolutionSql;
            await using var reader8 = await cmd.ExecuteReaderAsync();
            while (await reader8.ReadAsync()) documents.Add(MapToMyDocumentSimple(reader8));
            await reader8.CloseAsync();
            
            return documents.OrderByDescending(d => d.CreatedAt).ToList();
        }
    }
}