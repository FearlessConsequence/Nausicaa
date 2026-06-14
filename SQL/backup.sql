--
-- PostgreSQL database dump
--

\restrict GucGWTjIWskJf3aavSfTFESEnz8vQcWeuwDJkqLOymuBYKioubQ0DfQxMZQPtlL

-- Dumped from database version 18.0
-- Dumped by pg_dump version 18.0

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: Music; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA "Music";


ALTER SCHEMA "Music" OWNER TO postgres;

--
-- Name: Practice; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA "Practice";


ALTER SCHEMA "Practice" OWNER TO postgres;

--
-- Name: Practice 11/30/2025; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA "Practice 11/30/2025";


ALTER SCHEMA "Practice 11/30/2025" OWNER TO postgres;

--
-- Name: Study; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA "Study";


ALTER SCHEMA "Study" OWNER TO postgres;

--
-- Name: bilet1; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA bilet1;


ALTER SCHEMA bilet1 OWNER TO postgres;

--
-- Name: testdb; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA testdb;


ALTER SCHEMA testdb OWNER TO postgres;

--
-- Name: rating_domain; Type: DOMAIN; Schema: Music; Owner: postgres
--

CREATE DOMAIN "Music".rating_domain AS integer
	CONSTRAINT rating_domain_check CHECK (((VALUE >= 0) AND (VALUE <= 10)));


ALTER DOMAIN "Music".rating_domain OWNER TO postgres;

--
-- Name: address_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".address_domain AS character varying(100)
	CONSTRAINT address_domain_check CHECK (((length((VALUE)::text) >= 5) AND ((VALUE)::text ~ '^[А-Яа-яЁё0-9\s\.,-]+$'::text)));


ALTER DOMAIN "Practice 11/30/2025".address_domain OWNER TO postgres;

--
-- Name: amount_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".amount_domain AS integer;


ALTER DOMAIN "Practice 11/30/2025".amount_domain OWNER TO postgres;

--
-- Name: bool_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".bool_domain AS boolean;


ALTER DOMAIN "Practice 11/30/2025".bool_domain OWNER TO postgres;

--
-- Name: date_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".date_domain AS date;


ALTER DOMAIN "Practice 11/30/2025".date_domain OWNER TO postgres;

--
-- Name: description_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".description_domain AS text;


ALTER DOMAIN "Practice 11/30/2025".description_domain OWNER TO postgres;

--
-- Name: fine_amount_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".fine_amount_domain AS integer
	CONSTRAINT fine_amount_domain_check CHECK (((VALUE >= 0) AND (VALUE <= 1000000)));


ALTER DOMAIN "Practice 11/30/2025".fine_amount_domain OWNER TO postgres;

--
-- Name: id_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".id_domain AS integer;


ALTER DOMAIN "Practice 11/30/2025".id_domain OWNER TO postgres;

--
-- Name: name_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".name_domain AS text;


ALTER DOMAIN "Practice 11/30/2025".name_domain OWNER TO postgres;

--
-- Name: passport_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".passport_domain AS bigint
	CONSTRAINT passport_domain_check CHECK (((VALUE >= 1000000000) AND (VALUE <= '9999999999'::bigint)));


ALTER DOMAIN "Practice 11/30/2025".passport_domain OWNER TO postgres;

--
-- Name: protocol_number_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".protocol_number_domain AS integer
	CONSTRAINT protocol_number_domain_check CHECK (((VALUE >= 1000) AND (VALUE <= 999999)));


ALTER DOMAIN "Practice 11/30/2025".protocol_number_domain OWNER TO postgres;

--
-- Name: salary_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".salary_domain AS numeric(10,2);


ALTER DOMAIN "Practice 11/30/2025".salary_domain OWNER TO postgres;

--
-- Name: short_string_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".short_string_domain AS text;


ALTER DOMAIN "Practice 11/30/2025".short_string_domain OWNER TO postgres;

--
-- Name: time_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".time_domain AS time without time zone;


ALTER DOMAIN "Practice 11/30/2025".time_domain OWNER TO postgres;

--
-- Name: time_interval_domain; Type: DOMAIN; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE DOMAIN "Practice 11/30/2025".time_interval_domain AS time without time zone
	CONSTRAINT time_interval_domain_check CHECK (((VALUE >= '00:00:00'::time without time zone) AND (VALUE <= '23:59:59'::time without time zone)));


ALTER DOMAIN "Practice 11/30/2025".time_interval_domain OWNER TO postgres;

--
-- Name: address_domain; Type: DOMAIN; Schema: public; Owner: postgres
--

CREATE DOMAIN public.address_domain AS character varying(100)
	CONSTRAINT address_domain_check CHECK (((length((VALUE)::text) >= 5) AND ((VALUE)::text ~ '^[А-Яа-яЁё0-9\s\.,-]+$'::text)));


ALTER DOMAIN public.address_domain OWNER TO postgres;

--
-- Name: article_number_domain; Type: DOMAIN; Schema: public; Owner: postgres
--

CREATE DOMAIN public.article_number_domain AS numeric(4,2)
	CONSTRAINT article_number_domain_check CHECK (((VALUE >= (1)::numeric) AND (VALUE <= 99.99)));


ALTER DOMAIN public.article_number_domain OWNER TO postgres;

--
-- Name: fine_amount_domain; Type: DOMAIN; Schema: public; Owner: postgres
--

CREATE DOMAIN public.fine_amount_domain AS integer
	CONSTRAINT fine_amount_domain_check CHECK (((VALUE >= 0) AND (VALUE <= 1000000)));


ALTER DOMAIN public.fine_amount_domain OWNER TO postgres;

--
-- Name: name_domain; Type: DOMAIN; Schema: public; Owner: postgres
--

CREATE DOMAIN public.name_domain AS character varying(50)
	CONSTRAINT name_domain_check CHECK (((VALUE)::text ~ '^[А-ЯЁ][а-яё]+([-][А-ЯЁ][а-яё]+)*$'::text));


ALTER DOMAIN public.name_domain OWNER TO postgres;

--
-- Name: passport_domain; Type: DOMAIN; Schema: public; Owner: postgres
--

CREATE DOMAIN public.passport_domain AS bigint
	CONSTRAINT passport_domain_check CHECK (((VALUE >= 1000000000) AND (VALUE <= '9999999999'::bigint)));


ALTER DOMAIN public.passport_domain OWNER TO postgres;

--
-- Name: protocol_number_domain; Type: DOMAIN; Schema: public; Owner: postgres
--

CREATE DOMAIN public.protocol_number_domain AS integer
	CONSTRAINT protocol_number_domain_check CHECK (((VALUE >= 1000) AND (VALUE <= 999999)));


ALTER DOMAIN public.protocol_number_domain OWNER TO postgres;

--
-- Name: time_interval_domain; Type: DOMAIN; Schema: public; Owner: postgres
--

CREATE DOMAIN public.time_interval_domain AS time without time zone
	CONSTRAINT time_interval_domain_check CHECK (((VALUE >= '00:00:00'::time without time zone) AND (VALUE <= '23:59:59'::time without time zone)));


ALTER DOMAIN public.time_interval_domain OWNER TO postgres;

--
-- Name: add_limit(integer, date, date, numeric); Type: PROCEDURE; Schema: Practice; Owner: postgres
--

CREATE PROCEDURE "Practice".add_limit(IN p_user_id integer, IN p_date_beginning date, IN p_date_ending date, IN p_sum_limit numeric)
    LANGUAGE plpgsql
    AS $$
BEGIN
    INSERT INTO limits (user_id, date_beginning, date_ending, sum_limit)
    VALUES (p_user_id, p_date_beginning, p_date_ending, p_sum_limit);
END;
$$;


ALTER PROCEDURE "Practice".add_limit(IN p_user_id integer, IN p_date_beginning date, IN p_date_ending date, IN p_sum_limit numeric) OWNER TO postgres;

--
-- Name: add_transaction(integer, integer, numeric); Type: PROCEDURE; Schema: Practice; Owner: postgres
--

CREATE PROCEDURE "Practice".add_transaction(IN p_user_id integer, IN p_category_id integer, IN p_amount numeric)
    LANGUAGE plpgsql
    AS $$
BEGIN
    INSERT INTO transactions (user_id1, category_id, amount, transaction_date)
    VALUES (p_user_id, p_category_id, p_amount, NOW());
END;
$$;


ALTER PROCEDURE "Practice".add_transaction(IN p_user_id integer, IN p_category_id integer, IN p_amount numeric) OWNER TO postgres;

--
-- Name: check_daily_spending_limit(); Type: FUNCTION; Schema: Practice; Owner: postgres
--

CREATE FUNCTION "Practice".check_daily_spending_limit() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    daily_total decimal(10,2);
    user_limit decimal(10,2);
BEGIN
    IF NEW.amount < 0 THEN
        SELECT COALESCE(SUM(amount), 0) INTO daily_total
        FROM transactions
        WHERE user_id1 = NEW.user_id1
          AND amount < 0
          AND DATE(transaction_date) = DATE(NEW.transaction_date);

        SELECT balance * 0.1 INTO user_limit
        FROM users
        WHERE id = NEW.user_id1;

        IF (ABS(daily_total) + ABS(NEW.amount)) > user_limit THEN
            RAISE EXCEPTION 'Превышен дневной лимит расходов. Лимит: %, потрачено: %', user_limit, ABS(daily_total) + ABS(NEW.amount);
        END IF;
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION "Practice".check_daily_spending_limit() OWNER TO postgres;

--
-- Name: current_user_id(); Type: FUNCTION; Schema: Practice; Owner: postgres
--

CREATE FUNCTION "Practice".current_user_id() RETURNS integer
    LANGUAGE plpgsql SECURITY DEFINER
    AS $$
DECLARE
    user_login TEXT;
    user_id INTEGER;
BEGIN
    user_login := CURRENT_USER;
    
    SELECT id INTO user_id 
    FROM users WHERE login = user_login;
    
    RETURN user_id;
END;
$$;


ALTER FUNCTION "Practice".current_user_id() OWNER TO postgres;

--
-- Name: regular_payments_reminder(); Type: FUNCTION; Schema: Practice; Owner: postgres
--

CREATE FUNCTION "Practice".regular_payments_reminder() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    similar_count integer;
BEGIN
    IF NEW.category_id IN (1, 9) AND NEW.amount < 0 THEN
        SELECT COUNT(*) INTO similar_count
        FROM transactions
        WHERE user_id1 = NEW.user_id1
          AND category_id = NEW.category_id
          AND recipient_id = NEW.recipient_id
          AND ABS(amount - NEW.amount) <= 10
          AND transaction_date >= NEW.transaction_date - INTERVAL '1 hour'
          AND transaction_date <= NEW.transaction_date + INTERVAL '1 hour';

        IF similar_count > 0 THEN
            RAISE EXCEPTION 'Ошибка: повторяющийся платеж обнаружен';
        END IF;
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION "Practice".regular_payments_reminder() OWNER TO postgres;

--
-- Name: update_user_balance(); Type: FUNCTION; Schema: Practice; Owner: postgres
--

CREATE FUNCTION "Practice".update_user_balance() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        UPDATE users SET balance = balance + NEW.amount WHERE id = NEW.user_id1;
    ELSIF TG_OP = 'UPDATE' THEN
        UPDATE users SET balance = balance - OLD.amount + NEW.amount WHERE id = NEW.user_id1;
    ELSIF TG_OP = 'DELETE' THEN
        UPDATE users SET balance = balance - OLD.amount WHERE id = OLD.user_id1;
    END IF;
    RETURN COALESCE(NEW, OLD);
END;
$$;


ALTER FUNCTION "Practice".update_user_balance() OWNER TO postgres;

--
-- Name: check_offender_age(); Type: FUNCTION; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE FUNCTION "Practice 11/30/2025".check_offender_age() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF (SELECT EXTRACT(YEAR FROM AGE(CURRENT_DATE, birthday)) 
        FROM citizens WHERE id_citizen = NEW.offender) < 16 THEN
        RAISE EXCEPTION 'Нарушитель должен быть не младше 16 лет 
		для привлечения к административной ответственности';
    END IF;
    
    RETURN NEW;
END;
$$;


ALTER FUNCTION "Practice 11/30/2025".check_offender_age() OWNER TO postgres;

--
-- Name: check_witness_age(); Type: FUNCTION; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE FUNCTION "Practice 11/30/2025".check_witness_age() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    -- Проверяем возраст первого свидетеля (должен быть совершеннолетним)
    IF (SELECT EXTRACT(YEAR FROM AGE(CURRENT_DATE, birthday)) 
        FROM citizens WHERE id_citizen = NEW.first_witness) < 18 THEN
        RAISE EXCEPTION 'Первый свидетель должен быть совершеннолетним';
    END IF;
    
    -- Проверяем возраст второго свидетеля, если он указан
    IF NEW.second_witness IS NOT NULL THEN
        IF (SELECT EXTRACT(YEAR FROM AGE(CURRENT_DATE, birthday)) 
            FROM citizens WHERE id_citizen = NEW.second_witness) < 18 THEN
            RAISE EXCEPTION 'Второй свидетель должен быть совершеннолетним';
        END IF;
    END IF;
    
    RETURN NEW;
END;
$$;


ALTER FUNCTION "Practice 11/30/2025".check_witness_age() OWNER TO postgres;

--
-- Name: check_user_permissions(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.check_user_permissions() RETURNS TABLE(role_name text, permissions text)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY
    SELECT 
        CURRENT_USER::text,
        CASE 
            WHEN pg_has_role(CURRENT_USER, 'admin_role', 'member') THEN 'Полные права на все таблицы'
            WHEN pg_has_role(CURRENT_USER, 'police_officer_role', 'member') THEN 'Чтение справочников + запись своих документов'
            WHEN pg_has_role(CURRENT_USER, 'doctor_role', 'member') THEN 'Чтение справочников + запись своих сертификатов'
            WHEN pg_has_role(CURRENT_USER, 'medical_expert_role', 'member') THEN 'Чтение справочников + запись своих экспертиз'
            WHEN pg_has_role(CURRENT_USER, 'judge_role', 'member') THEN 'Чтение всех данных + запись постановлений'
            ELSE 'Ограниченные права'
        END;
END;
$$;


ALTER FUNCTION public.check_user_permissions() OWNER TO postgres;

--
-- Name: create_medical_only(integer, integer, boolean, boolean); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.create_medical_only(deal_id integer, citizen_id integer, need_medical boolean, need_forensic boolean) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    medical_report_id INTEGER;
BEGIN
    -- ТОЛЬКО создаем направление на медосвидетельствование
    INSERT INTO medical_examination_report (
        deal, report, patient, date, time, 
        signs_of_intoxication, content, approval, signature
    ) VALUES (
        deal_id,
        1,
        citizen_id,
        CURRENT_DATE,
        CURRENT_TIME,
        'Признаки требуют освидетельствования',
        'Направление на медицинское освидетельствование',
        false,
        true
    ) RETURNING id_medical_examination_report INTO medical_report_id;

    RAISE NOTICE 'Медицинское направление создано! ID: %', medical_report_id;
    
    RETURN medical_report_id;
END;
$$;


ALTER FUNCTION public.create_medical_only(deal_id integer, citizen_id integer, need_medical boolean, need_forensic boolean) OWNER TO postgres;

--
-- Name: create_user(character varying, text, character varying, character varying, date, text, character varying, integer, integer, character varying, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.create_user(p_username character varying, p_password text, p_last_name character varying, p_first_name character varying, p_birthday date, p_address_registration text, p_passport_series_and_number character varying, p_post_id integer, p_role integer, p_patronymic character varying DEFAULT NULL::character varying, p_settlement_of_birth integer DEFAULT NULL::integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_citizen_id INT;
    v_citizen_post_id INT;
    v_user_id INT;
    v_password_hash TEXT;
BEGIN
    -- 1. Хэшируем пароль (используем расширение pgcrypto)
    v_password_hash := crypt(p_password, gen_salt('bf'));
    
    -- 2. Создаём гражданина
    INSERT INTO citizens (
        last_name,
        first_name,
        patronymic,
        birthday,
        settlement_of_birth,
        address_registration,
        passport_series_and_number,
        criminal_record,
        count_record
    ) VALUES (
        p_last_name,
        p_first_name,
        p_patronymic,
        p_birthday,
        p_settlement_of_birth,
        p_address_registration,
        p_passport_series_and_number,
        FALSE,
        0
    ) RETURNING id_citizens INTO v_citizen_id;
    
    -- 3. Создаём связь гражданина с должностью
    INSERT INTO citizens_and_posts (citizen, post)
    VALUES (v_citizen_id, p_post_id)
    RETURNING id_citizens_and_posts INTO v_citizen_post_id;
    
    -- 4. Создаём пользователя
    INSERT INTO users (
        username,
        password,
        role,
        citizen_post_id
    ) VALUES (
        p_username,
        v_password_hash,
        p_role,
        v_citizen_post_id
    ) RETURNING id INTO v_user_id;
    
    -- 5. Создаём связь в user_citizen_link (для обратной совместимости)
    INSERT INTO user_citizen_link (user_id, citizen_post_id)
    VALUES (v_user_id, v_citizen_post_id)
    ON CONFLICT (user_id) DO NOTHING;
    
    RETURN v_user_id;
END;
$$;


ALTER FUNCTION public.create_user(p_username character varying, p_password text, p_last_name character varying, p_first_name character varying, p_birthday date, p_address_registration text, p_passport_series_and_number character varying, p_post_id integer, p_role integer, p_patronymic character varying, p_settlement_of_birth integer) OWNER TO postgres;

--
-- Name: fill_medical_from_explanation(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fill_medical_from_explanation(explanation_id integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    medical_report_id INTEGER;
    v_deal_id INTEGER;
    v_citizen_id INTEGER;
    v_need_medical BOOLEAN;
    v_need_forensic BOOLEAN;
BEGIN
    -- Берем все данные из протокола объяснения
    SELECT deal, citizen, need_medical_examination_certificate, need_forensic_medical_examination
    INTO v_deal_id, v_citizen_id, v_need_medical, v_need_forensic
    FROM explanation_protocol
    WHERE id_explanation_protocol = explanation_id;

    -- Если нужен медосмотр - создаем направление
    IF v_need_medical THEN
        INSERT INTO medical_examination_report (
            deal, report, patient, date, time, 
            signs_of_intoxication, content, approval, signature
        ) VALUES (
            v_deal_id,
            1,
            v_citizen_id,
            CURRENT_DATE,
            CURRENT_TIME,
            'Признаки требуют освидетельствования',
            'Направление на медицинское освидетельствование',
            false,
            true
        ) RETURNING id_medical_examination_report INTO medical_report_id;

        RAISE NOTICE 'Медицинское направление создано! ID: %', medical_report_id;
    ELSE
        medical_report_id := 0;
        RAISE NOTICE 'Медосмотр не требуется';
    END IF;

    RETURN medical_report_id;
END;
$$;


ALTER FUNCTION public.fill_medical_from_explanation(explanation_id integer) OWNER TO postgres;

--
-- Name: get_recent_documents_for_user(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.get_recent_documents_for_user(current_user_id integer) RETURNS TABLE(id integer, type_id integer, type_name character varying, number integer, making_date timestamp without time zone, citizen_id integer, citizen_name text)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY
    SELECT 
        v.id,
        v.type_id,
        v.type_name,
        v.number,
        v.making_date,
        v.citizen_id,
        v.citizen_name
    FROM view_recent_documents v
    WHERE 
        -- Заявление
        (v.type_id = 1 AND EXISTS (
            SELECT 1 FROM statement s 
            WHERE s.id_statement = v.id 
            AND s.police_officer IN (
                SELECT id_citizens_and_posts FROM citizens_and_posts 
                WHERE citizen = current_user_id
            )
        ))
        OR
        -- Обращение
        (v.type_id = 2 AND EXISTS (
            SELECT 1 FROM appeals a 
            WHERE a.id_appeals = v.id 
            AND a.police_officer IN (
                SELECT id_citizens_and_posts FROM citizens_and_posts 
                WHERE citizen = current_user_id
            )
        ))
        OR
        -- Протокол объяснения (через deal)
        (v.type_id = 3 AND EXISTS (
            SELECT 1 FROM explanation_protocol ep
            JOIN deal d ON ep.deal = d.id_deal
            WHERE ep.id_explanation_protocol = v.id
            AND d.police_officer IN (
                SELECT id_citizens_and_posts FROM citizens_and_posts 
                WHERE citizen = current_user_id
            )
        ))
        OR
        -- Направление на мед. освид. (через police_officer в statement если есть связь)
        (v.type_id = 4)
        OR
        -- Административный протокол (через deal)
        (v.type_id = 5 AND EXISTS (
            SELECT 1 FROM administrative_protocol ap
            JOIN deal d ON ap.deal = d.id_deal
            WHERE ap.id_protocol = v.id
            AND d.police_officer IN (
                SELECT id_citizens_and_posts FROM citizens_and_posts 
                WHERE citizen = current_user_id
            )
        ));
END;
$$;


ALTER FUNCTION public.get_recent_documents_for_user(current_user_id integer) OWNER TO postgres;

--
-- Name: increment(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.increment() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    -- Логика триггера
    UPDATE citizens
    SET count_record = count_record + 1
    WHERE id_citizens = NEW.offender;

    -- Сообщение для отладки
    RAISE NOTICE 'Триггер сработал! Гражданин ID: %', NEW.offender;
    
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.increment() OWNER TO postgres;

--
-- Name: increment_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.increment_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
	update citizens
	set count_record = count_record + 1
	where id_citizens = new.offender;

	raise notice 'Триггер сработал! Гражданин ID: %', new.offender;
	return new;
end;
$$;


ALTER FUNCTION public.increment_function() OWNER TO postgres;

--
-- Name: trg_update_criminal_record_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.trg_update_criminal_record_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE citizens
    SET criminal_record = true,
        count_record = count_record + 1
    WHERE id_citizens = (
        SELECT d.offender 
        FROM deal d 
        WHERE d.id_deal = NEW.deal
    );
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.trg_update_criminal_record_function() OWNER TO postgres;

--
-- Name: validate_admin_protocol_sequence_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_admin_protocol_sequence_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM explanation_protocol WHERE deal = NEW.deal) THEN
        RAISE EXCEPTION 'Нельзя создать административный протокол без протокола объяснения';
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_admin_protocol_sequence_function() OWNER TO postgres;

--
-- Name: validate_date_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_date_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NEW.making_date > CURRENT_DATE THEN
        RAISE EXCEPTION 'Дата в таблице % не может быть в будущем. Указанная дата: %', TG_TABLE_NAME, NEW.making_date;
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_date_function() OWNER TO postgres;

--
-- Name: validate_deal_sequence_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_deal_sequence_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM statement s
        JOIN citizens applicant ON s.applicant = applicant.id_citizens
        JOIN citizens offender ON NEW.offender = offender.id_citizens
        WHERE (
            (s.applicant != NEW.offender)
            OR (s.content LIKE '%' || offender.last_name || '%')
            OR (s.content LIKE '%кража%' OR s.content LIKE '%нарушение%' OR s.content LIKE '%избиение%')
        )
        AND s.date <= CURRENT_DATE
        AND s.date >= CURRENT_DATE - INTERVAL '1 year'
    ) THEN
        RAISE EXCEPTION 'Нельзя создать дело №% на гражданина % без соответствующего заявления', 
            NEW.deal_number, 
            (SELECT last_name FROM citizens WHERE id_citizens = NEW.offender);
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_deal_sequence_function() OWNER TO postgres;

--
-- Name: validate_explanation_date_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_explanation_date_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NEW.date > CURRENT_DATE THEN
        RAISE EXCEPTION 'Дата в таблице explanation_protocol не может быть в будущем. Указанная дата: %', NEW.date;
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_explanation_date_function() OWNER TO postgres;

--
-- Name: validate_explanation_protocol_sequence_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_explanation_protocol_sequence_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM deal WHERE id_deal = NEW.deal) THEN
        RAISE EXCEPTION 'Нельзя создать протокол объяснения без дела';
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_explanation_protocol_sequence_function() OWNER TO postgres;

--
-- Name: validate_forensic_exam_sequence_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_forensic_exam_sequence_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM explanation_protocol
        WHERE deal = NEW.deal
        AND need_forensic_medical_examination = true
    ) THEN
        RAISE EXCEPTION 'Нельзя создать судебно-медицинскую экспертизу без соответствующего указания в протоколе объяснения';
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_forensic_exam_sequence_function() OWNER TO postgres;

--
-- Name: validate_forensic_examination_date_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_forensic_examination_date_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NEW.date > CURRENT_DATE THEN
        RAISE EXCEPTION 'Дата в таблице forensic_medical_examination не может быть в будущем. Указанная дата: %', NEW.date;
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_forensic_examination_date_function() OWNER TO postgres;

--
-- Name: validate_medical_certificate_date_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_medical_certificate_date_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NEW.date > CURRENT_DATE THEN
        RAISE EXCEPTION 'Дата в таблице medical_examination_certificate не может быть в будущем. Указанная дата: %', NEW.date;
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_medical_certificate_date_function() OWNER TO postgres;

--
-- Name: validate_medical_certificate_sequence_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_medical_certificate_sequence_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM medical_examination_report
        WHERE id_medical_examination_report = NEW.medical_examination_report
    ) THEN
        RAISE EXCEPTION 'Нельзя создать акт медицинского освидетельствования без направления на медицинское освидетельствование';
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_medical_certificate_sequence_function() OWNER TO postgres;

--
-- Name: validate_medical_report_date_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_medical_report_date_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NEW.date > CURRENT_DATE THEN
        RAISE EXCEPTION 'Дата в таблице medical_examination_report не может быть в будущем. Указанная дата: %', NEW.date;
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_medical_report_date_function() OWNER TO postgres;

--
-- Name: validate_medical_report_sequence_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_medical_report_sequence_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM explanation_protocol
        WHERE deal = NEW.deal
        AND need_medical_examination_certificate = true
    ) THEN
        RAISE EXCEPTION 'Нельзя создать направление на медицинское освидетельствование без соответствующего указания в протоколе объяснения';
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_medical_report_sequence_function() OWNER TO postgres;

--
-- Name: validate_offender_age_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_offender_age_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF (
        SELECT EXTRACT(YEAR FROM AGE(c.birthday)) 
        FROM citizens c 
        JOIN deal d ON c.id_citizens = d.offender 
        WHERE d.id_deal = NEW.deal
    ) < 16 THEN
        RAISE EXCEPTION 'Нельзя составить административный протокол на гражданина младше 16 лет';
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_offender_age_function() OWNER TO postgres;

--
-- Name: validate_resolution_date_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_resolution_date_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NEW.date > CURRENT_DATE THEN
        RAISE EXCEPTION 'Дата в таблице resolution не может быть в будущем. Указанная дата: %', NEW.date;
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_resolution_date_function() OWNER TO postgres;

--
-- Name: validate_resolution_sequence_function(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.validate_resolution_sequence_function() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM administrative_protocol WHERE deal = NEW.deal) THEN
        RAISE EXCEPTION 'Нельзя создать постановление без административного протокола';
    END IF;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.validate_resolution_sequence_function() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: Альбомы; Type: TABLE; Schema: Music; Owner: postgres
--

CREATE TABLE "Music"."Альбомы" (
    "Код" integer NOT NULL,
    "Название" character varying(70),
    "Исполнитель" integer NOT NULL,
    "Жанр" integer NOT NULL,
    "Дата_выхода" date NOT NULL
);


ALTER TABLE "Music"."Альбомы" OWNER TO postgres;

--
-- Name: Альбомы_Код_seq; Type: SEQUENCE; Schema: Music; Owner: postgres
--

CREATE SEQUENCE "Music"."Альбомы_Код_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Music"."Альбомы_Код_seq" OWNER TO postgres;

--
-- Name: Альбомы_Код_seq; Type: SEQUENCE OWNED BY; Schema: Music; Owner: postgres
--

ALTER SEQUENCE "Music"."Альбомы_Код_seq" OWNED BY "Music"."Альбомы"."Код";


--
-- Name: Жанры; Type: TABLE; Schema: Music; Owner: postgres
--

CREATE TABLE "Music"."Жанры" (
    "Код" integer NOT NULL,
    "Название_жанра" character varying(100) NOT NULL,
    "Описание" text
);


ALTER TABLE "Music"."Жанры" OWNER TO postgres;

--
-- Name: Жанры_Код_seq; Type: SEQUENCE; Schema: Music; Owner: postgres
--

CREATE SEQUENCE "Music"."Жанры_Код_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Music"."Жанры_Код_seq" OWNER TO postgres;

--
-- Name: Жанры_Код_seq; Type: SEQUENCE OWNED BY; Schema: Music; Owner: postgres
--

ALTER SEQUENCE "Music"."Жанры_Код_seq" OWNED BY "Music"."Жанры"."Код";


--
-- Name: Жанры_и_исполнители; Type: TABLE; Schema: Music; Owner: postgres
--

CREATE TABLE "Music"."Жанры_и_исполнители" (
    "Код" integer NOT NULL,
    "Исполнитель" integer CONSTRAINT "Жанры_и_исполнит_Исполнитель_not_null" NOT NULL,
    "Жанр" integer NOT NULL
);


ALTER TABLE "Music"."Жанры_и_исполнители" OWNER TO postgres;

--
-- Name: Жанры_и_исполнители_Код_seq; Type: SEQUENCE; Schema: Music; Owner: postgres
--

CREATE SEQUENCE "Music"."Жанры_и_исполнители_Код_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Music"."Жанры_и_исполнители_Код_seq" OWNER TO postgres;

--
-- Name: Жанры_и_исполнители_Код_seq; Type: SEQUENCE OWNED BY; Schema: Music; Owner: postgres
--

ALTER SEQUENCE "Music"."Жанры_и_исполнители_Код_seq" OWNED BY "Music"."Жанры_и_исполнители"."Код";


--
-- Name: Исполнители; Type: TABLE; Schema: Music; Owner: postgres
--

CREATE TABLE "Music"."Исполнители" (
    "Код" integer NOT NULL,
    "Исполнитель" character varying(100) NOT NULL,
    "Описание" text
);


ALTER TABLE "Music"."Исполнители" OWNER TO postgres;

--
-- Name: Исполнители_Код_seq; Type: SEQUENCE; Schema: Music; Owner: postgres
--

CREATE SEQUENCE "Music"."Исполнители_Код_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Music"."Исполнители_Код_seq" OWNER TO postgres;

--
-- Name: Исполнители_Код_seq; Type: SEQUENCE OWNED BY; Schema: Music; Owner: postgres
--

ALTER SEQUENCE "Music"."Исполнители_Код_seq" OWNED BY "Music"."Исполнители"."Код";


--
-- Name: Композиции; Type: TABLE; Schema: Music; Owner: postgres
--

CREATE TABLE "Music"."Композиции" (
    "Код" integer NOT NULL,
    "Название_композиции" character varying(100) CONSTRAINT "Композиции_Название_композиц_not_null" NOT NULL,
    "Жанр" integer NOT NULL,
    "Описание" text,
    "Оценка" "Music".rating_domain,
    "Альбом" integer
);


ALTER TABLE "Music"."Композиции" OWNER TO postgres;

--
-- Name: Композиции_Код_seq; Type: SEQUENCE; Schema: Music; Owner: postgres
--

CREATE SEQUENCE "Music"."Композиции_Код_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Music"."Композиции_Код_seq" OWNER TO postgres;

--
-- Name: Композиции_Код_seq; Type: SEQUENCE OWNED BY; Schema: Music; Owner: postgres
--

ALTER SEQUENCE "Music"."Композиции_Код_seq" OWNED BY "Music"."Композиции"."Код";


--
-- Name: Композиции_и_исполнители; Type: TABLE; Schema: Music; Owner: postgres
--

CREATE TABLE "Music"."Композиции_и_исполнители" (
    "Код" integer NOT NULL,
    "Композиция" integer CONSTRAINT "Композиции_и_испо_Композиция_not_null" NOT NULL,
    "Исполнитель" integer CONSTRAINT "Композиции_и_исп_Исполнитель_not_null" NOT NULL
);


ALTER TABLE "Music"."Композиции_и_исполнители" OWNER TO postgres;

--
-- Name: Композиции_и_исполнители_Код_seq; Type: SEQUENCE; Schema: Music; Owner: postgres
--

CREATE SEQUENCE "Music"."Композиции_и_исполнители_Код_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Music"."Композиции_и_исполнители_Код_seq" OWNER TO postgres;

--
-- Name: Композиции_и_исполнители_Код_seq; Type: SEQUENCE OWNED BY; Schema: Music; Owner: postgres
--

ALTER SEQUENCE "Music"."Композиции_и_исполнители_Код_seq" OWNED BY "Music"."Композиции_и_исполнители"."Код";


--
-- Name: categories; Type: TABLE; Schema: Practice; Owner: postgres
--

CREATE TABLE "Practice".categories (
    id integer NOT NULL,
    name character varying(50) NOT NULL,
    description text NOT NULL
);


ALTER TABLE "Practice".categories OWNER TO postgres;

--
-- Name: categories_id_seq; Type: SEQUENCE; Schema: Practice; Owner: postgres
--

CREATE SEQUENCE "Practice".categories_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Practice".categories_id_seq OWNER TO postgres;

--
-- Name: categories_id_seq; Type: SEQUENCE OWNED BY; Schema: Practice; Owner: postgres
--

ALTER SEQUENCE "Practice".categories_id_seq OWNED BY "Practice".categories.id;


--
-- Name: limits; Type: TABLE; Schema: Practice; Owner: postgres
--

CREATE TABLE "Practice".limits (
    id integer NOT NULL,
    user_id integer NOT NULL,
    date_beginning date NOT NULL,
    date_ending date NOT NULL,
    sum_limit numeric(10,2) NOT NULL,
    CONSTRAINT limits_sum_limit_check CHECK ((sum_limit >= (0)::numeric))
);


ALTER TABLE "Practice".limits OWNER TO postgres;

--
-- Name: limits_id_seq; Type: SEQUENCE; Schema: Practice; Owner: postgres
--

CREATE SEQUENCE "Practice".limits_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Practice".limits_id_seq OWNER TO postgres;

--
-- Name: limits_id_seq; Type: SEQUENCE OWNED BY; Schema: Practice; Owner: postgres
--

ALTER SEQUENCE "Practice".limits_id_seq OWNED BY "Practice".limits.id;


--
-- Name: transactions; Type: TABLE; Schema: Practice; Owner: postgres
--

CREATE TABLE "Practice".transactions (
    id integer NOT NULL,
    sender_id integer,
    user_id2 integer,
    category_id integer NOT NULL,
    recipient_id integer,
    user_id1 integer,
    amount numeric(10,2) NOT NULL,
    transaction_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT transactions_amount_check CHECK ((amount <> (0)::numeric))
);


ALTER TABLE "Practice".transactions OWNER TO postgres;

--
-- Name: my_limits; Type: VIEW; Schema: Practice; Owner: postgres
--

CREATE VIEW "Practice".my_limits AS
 SELECT id,
    date_beginning,
    date_ending,
    sum_limit,
    ( SELECT COALESCE(sum(abs(transactions.amount)), (0)::numeric) AS "coalesce"
           FROM "Practice".transactions
          WHERE ((transactions.user_id1 = "Practice".current_user_id()) AND (transactions.amount < (0)::numeric) AND ((transactions.transaction_date >= limits.date_beginning) AND (transactions.transaction_date <= limits.date_ending)))) AS current_spent,
    ( SELECT round(((COALESCE(sum(abs(transactions.amount)), (0)::numeric) / limits.sum_limit) * (100)::numeric), 2) AS round
           FROM "Practice".transactions
          WHERE ((transactions.user_id1 = "Practice".current_user_id()) AND (transactions.amount < (0)::numeric) AND ((transactions.transaction_date >= limits.date_beginning) AND (transactions.transaction_date <= limits.date_ending)))) AS usage_percent
   FROM "Practice".limits
  WHERE (user_id = "Practice".current_user_id());


ALTER VIEW "Practice".my_limits OWNER TO postgres;

--
-- Name: recipient; Type: TABLE; Schema: Practice; Owner: postgres
--

CREATE TABLE "Practice".recipient (
    id integer NOT NULL,
    user_id integer,
    name character varying(50),
    requisites bigint,
    description text
);


ALTER TABLE "Practice".recipient OWNER TO postgres;

--
-- Name: users; Type: TABLE; Schema: Practice; Owner: postgres
--

CREATE TABLE "Practice".users (
    id integer NOT NULL,
    lastname character varying(50) NOT NULL,
    firstname character varying(50) NOT NULL,
    patronymic character varying(50),
    login character varying(50) NOT NULL,
    password character varying(255) NOT NULL,
    balance numeric(10,2) DEFAULT 0 NOT NULL,
    role integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT users_balance_check CHECK ((balance >= (0)::numeric))
);


ALTER TABLE "Practice".users OWNER TO postgres;

--
-- Name: my_transactions; Type: VIEW; Schema: Practice; Owner: postgres
--

CREATE VIEW "Practice".my_transactions AS
 SELECT t.id,
    t.transaction_date,
    c.name AS category_name,
    COALESCE(r.name, ((((u2.lastname)::text || ' '::text) || (u2.firstname)::text))::character varying) AS recipient_name,
    t.amount,
        CASE
            WHEN (t.amount < (0)::numeric) THEN 'Расход'::text
            ELSE 'Доход'::text
        END AS operation_type
   FROM ((("Practice".transactions t
     JOIN "Practice".categories c ON ((t.category_id = c.id)))
     LEFT JOIN "Practice".recipient r ON ((t.recipient_id = r.id)))
     LEFT JOIN "Practice".users u2 ON (((t.user_id2 = u2.id) OR (r.user_id = u2.id))))
  WHERE (t.user_id1 = "Practice".current_user_id());


ALTER VIEW "Practice".my_transactions OWNER TO postgres;

--
-- Name: recipient_id_seq; Type: SEQUENCE; Schema: Practice; Owner: postgres
--

CREATE SEQUENCE "Practice".recipient_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Practice".recipient_id_seq OWNER TO postgres;

--
-- Name: recipient_id_seq; Type: SEQUENCE OWNED BY; Schema: Practice; Owner: postgres
--

ALTER SEQUENCE "Practice".recipient_id_seq OWNED BY "Practice".recipient.id;


--
-- Name: roles; Type: TABLE; Schema: Practice; Owner: postgres
--

CREATE TABLE "Practice".roles (
    id integer NOT NULL,
    role_id character varying(50) NOT NULL,
    description text NOT NULL
);


ALTER TABLE "Practice".roles OWNER TO postgres;

--
-- Name: roles_id_seq; Type: SEQUENCE; Schema: Practice; Owner: postgres
--

CREATE SEQUENCE "Practice".roles_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Practice".roles_id_seq OWNER TO postgres;

--
-- Name: roles_id_seq; Type: SEQUENCE OWNED BY; Schema: Practice; Owner: postgres
--

ALTER SEQUENCE "Practice".roles_id_seq OWNED BY "Practice".roles.id;


--
-- Name: sender; Type: TABLE; Schema: Practice; Owner: postgres
--

CREATE TABLE "Practice".sender (
    id integer NOT NULL,
    sender character varying(50) NOT NULL,
    description text,
    requisits character varying(50)
);


ALTER TABLE "Practice".sender OWNER TO postgres;

--
-- Name: transactions_id_seq; Type: SEQUENCE; Schema: Practice; Owner: postgres
--

CREATE SEQUENCE "Practice".transactions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Practice".transactions_id_seq OWNER TO postgres;

--
-- Name: transactions_id_seq; Type: SEQUENCE OWNED BY; Schema: Practice; Owner: postgres
--

ALTER SEQUENCE "Practice".transactions_id_seq OWNED BY "Practice".transactions.id;


--
-- Name: users_id_seq; Type: SEQUENCE; Schema: Practice; Owner: postgres
--

CREATE SEQUENCE "Practice".users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Practice".users_id_seq OWNER TO postgres;

--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: Practice; Owner: postgres
--

ALTER SEQUENCE "Practice".users_id_seq OWNED BY "Practice".users.id;


--
-- Name: article; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".article (
    id_article integer NOT NULL,
    number_of_article public.article_number_domain NOT NULL,
    description text NOT NULL
);


ALTER TABLE "Practice 11/30/2025".article OWNER TO postgres;

--
-- Name: articles_and_responsibility; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".articles_and_responsibility (
    id_articles_and_responsibility integer CONSTRAINT articles_and_responsibility_id_articles_and_responsibi_not_null NOT NULL,
    responsibility integer NOT NULL,
    article integer NOT NULL
);


ALTER TABLE "Practice 11/30/2025".articles_and_responsibility OWNER TO postgres;

--
-- Name: citizens; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".citizens (
    id_citizen integer NOT NULL,
    last_name "Practice 11/30/2025".name_domain NOT NULL,
    first_name "Practice 11/30/2025".name_domain NOT NULL,
    patronymic "Practice 11/30/2025".name_domain,
    birthday date NOT NULL,
    settlement_citizen integer NOT NULL,
    place_registration "Practice 11/30/2025".address_domain NOT NULL,
    work_place integer,
    post integer,
    salary integer NOT NULL,
    criminal_record boolean NOT NULL,
    count_record integer,
    family_status integer,
    passport "Practice 11/30/2025".passport_domain NOT NULL
);


ALTER TABLE "Practice 11/30/2025".citizens OWNER TO postgres;

--
-- Name: citizens_and_posts; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".citizens_and_posts (
    id_citizens_and_posts integer NOT NULL,
    citizen integer NOT NULL,
    post integer NOT NULL
);


ALTER TABLE "Practice 11/30/2025".citizens_and_posts OWNER TO postgres;

--
-- Name: family_status; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".family_status (
    id_family_status integer NOT NULL,
    family_status "Practice 11/30/2025".name_domain NOT NULL
);


ALTER TABLE "Practice 11/30/2025".family_status OWNER TO postgres;

--
-- Name: medical_examination_report; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".medical_examination_report (
    id_medical_examination_report integer CONSTRAINT medical_examination_report_id_medical_examination_repo_not_null NOT NULL,
    report integer NOT NULL,
    number_of_report "Practice 11/30/2025".protocol_number_domain NOT NULL,
    settlements_report integer NOT NULL,
    police_officers_in_report integer NOT NULL,
    patient integer NOT NULL,
    date_of_making date NOT NULL,
    time_of_making "Practice 11/30/2025".time_interval_domain NOT NULL,
    hospital_staff integer NOT NULL,
    sign_of_intoxication text NOT NULL,
    access_for_report boolean NOT NULL,
    first_witness integer NOT NULL,
    second_witness integer
);


ALTER TABLE "Practice 11/30/2025".medical_examination_report OWNER TO postgres;

--
-- Name: post; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".post (
    id_post integer NOT NULL,
    post_name "Practice 11/30/2025".name_domain NOT NULL
);


ALTER TABLE "Practice 11/30/2025".post OWNER TO postgres;

--
-- Name: protocol; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".protocol (
    id_protocol integer NOT NULL,
    name_of_protocol "Practice 11/30/2025".protocol_number_domain NOT NULL,
    date_of_making_protocol date NOT NULL,
    time_of_making_protocol "Practice 11/30/2025".time_interval_domain NOT NULL,
    settlement_of_making integer NOT NULL,
    police_officers_in_protocol integer NOT NULL,
    offender integer NOT NULL,
    description text,
    disputes boolean NOT NULL,
    article_of_protocol integer NOT NULL,
    first_witness integer NOT NULL,
    second_witness integer
);


ALTER TABLE "Practice 11/30/2025".protocol OWNER TO postgres;

--
-- Name: resolution; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".resolution (
    id_resolution integer NOT NULL,
    number_of_protocol integer NOT NULL,
    settlements_resolution integer NOT NULL,
    court_staff integer NOT NULL,
    kdm_employee integer NOT NULL,
    resolution text NOT NULL,
    punishment integer NOT NULL,
    sum_of_fine "Practice 11/30/2025".fine_amount_domain,
    days_of_arrest integer,
    days_of_forced_labor integer,
    id_article integer NOT NULL,
    id_responsibility integer NOT NULL
);


ALTER TABLE "Practice 11/30/2025".resolution OWNER TO postgres;

--
-- Name: responsibility; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".responsibility (
    id_responsibility integer NOT NULL,
    type_of_responsibility "Practice 11/30/2025".name_domain NOT NULL
);


ALTER TABLE "Practice 11/30/2025".responsibility OWNER TO postgres;

--
-- Name: settlements; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".settlements (
    id_settlement integer NOT NULL,
    title_of_settlement "Practice 11/30/2025".name_domain NOT NULL
);


ALTER TABLE "Practice 11/30/2025".settlements OWNER TO postgres;

--
-- Name: structures; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".structures (
    id_structure integer NOT NULL,
    name_structure "Practice 11/30/2025".name_domain NOT NULL,
    settlement_structures integer NOT NULL,
    description_structure text NOT NULL
);


ALTER TABLE "Practice 11/30/2025".structures OWNER TO postgres;

--
-- Name: type_of_face; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".type_of_face (
    id_type_of_face integer NOT NULL,
    type_of_face "Practice 11/30/2025".name_domain NOT NULL
);


ALTER TABLE "Practice 11/30/2025".type_of_face OWNER TO postgres;

--
-- Name: type_of_punishment; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".type_of_punishment (
    id_type_of_punishment integer NOT NULL,
    type_of_punishment "Practice 11/30/2025".name_domain NOT NULL
);


ALTER TABLE "Practice 11/30/2025".type_of_punishment OWNER TO postgres;

--
-- Name: type_of_report; Type: TABLE; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TABLE "Practice 11/30/2025".type_of_report (
    id_type_of_report integer NOT NULL,
    type_of_report "Practice 11/30/2025".name_domain NOT NULL
);


ALTER TABLE "Practice 11/30/2025".type_of_report OWNER TO postgres;

--
-- Name: authors; Type: TABLE; Schema: Study; Owner: postgres
--

CREATE TABLE "Study".authors (
    id integer NOT NULL,
    name_author character varying(100) NOT NULL
);


ALTER TABLE "Study".authors OWNER TO postgres;

--
-- Name: authors_id_seq; Type: SEQUENCE; Schema: Study; Owner: postgres
--

CREATE SEQUENCE "Study".authors_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE "Study".authors_id_seq OWNER TO postgres;

--
-- Name: authors_id_seq; Type: SEQUENCE OWNED BY; Schema: Study; Owner: postgres
--

ALTER SEQUENCE "Study".authors_id_seq OWNED BY "Study".authors.id;


--
-- Name: circles; Type: TABLE; Schema: bilet1; Owner: postgres
--

CREATE TABLE bilet1.circles (
    circle_id integer NOT NULL,
    circle_name character varying(100) NOT NULL,
    education_level character varying(20) NOT NULL
);


ALTER TABLE bilet1.circles OWNER TO postgres;

--
-- Name: circles_circle_id_seq; Type: SEQUENCE; Schema: bilet1; Owner: postgres
--

CREATE SEQUENCE bilet1.circles_circle_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE bilet1.circles_circle_id_seq OWNER TO postgres;

--
-- Name: circles_circle_id_seq; Type: SEQUENCE OWNED BY; Schema: bilet1; Owner: postgres
--

ALTER SEQUENCE bilet1.circles_circle_id_seq OWNED BY bilet1.circles.circle_id;


--
-- Name: leaders; Type: TABLE; Schema: bilet1; Owner: postgres
--

CREATE TABLE bilet1.leaders (
    leader_id integer NOT NULL,
    full_name character varying(100) NOT NULL,
    circle_id integer NOT NULL
);


ALTER TABLE bilet1.leaders OWNER TO postgres;

--
-- Name: leaders_leader_id_seq; Type: SEQUENCE; Schema: bilet1; Owner: postgres
--

CREATE SEQUENCE bilet1.leaders_leader_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE bilet1.leaders_leader_id_seq OWNER TO postgres;

--
-- Name: leaders_leader_id_seq; Type: SEQUENCE OWNED BY; Schema: bilet1; Owner: postgres
--

ALTER SEQUENCE bilet1.leaders_leader_id_seq OWNED BY bilet1.leaders.leader_id;


--
-- Name: visits; Type: TABLE; Schema: bilet1; Owner: postgres
--

CREATE TABLE bilet1.visits (
    visit_id integer NOT NULL,
    leader_id integer NOT NULL,
    visit_date date DEFAULT CURRENT_DATE NOT NULL,
    children_count integer NOT NULL,
    CONSTRAINT visits_children_count_check CHECK ((children_count > 0))
);


ALTER TABLE bilet1.visits OWNER TO postgres;

--
-- Name: visits_visit_id_seq; Type: SEQUENCE; Schema: bilet1; Owner: postgres
--

CREATE SEQUENCE bilet1.visits_visit_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE bilet1.visits_visit_id_seq OWNER TO postgres;

--
-- Name: visits_visit_id_seq; Type: SEQUENCE OWNED BY; Schema: bilet1; Owner: postgres
--

ALTER SEQUENCE bilet1.visits_visit_id_seq OWNED BY bilet1.visits.visit_id;


--
-- Name: administrative_protocol; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.administrative_protocol (
    id_protocol integer NOT NULL,
    protocol_number public.protocol_number_domain NOT NULL,
    making_date_and_time timestamp without time zone NOT NULL,
    deal integer NOT NULL,
    description text NOT NULL,
    other_information text NOT NULL,
    signature_for_knowing_everithing boolean,
    first_witness integer NOT NULL,
    second_witness integer
);


ALTER TABLE public.administrative_protocol OWNER TO postgres;

--
-- Name: administrative_protocol_id_protocol_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.administrative_protocol_id_protocol_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.administrative_protocol_id_protocol_seq OWNER TO postgres;

--
-- Name: administrative_protocol_id_protocol_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.administrative_protocol_id_protocol_seq OWNED BY public.administrative_protocol.id_protocol;


--
-- Name: albums; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.albums (
    album_id integer NOT NULL,
    album_name character varying(100) NOT NULL,
    artist_id integer NOT NULL
);


ALTER TABLE public.albums OWNER TO postgres;

--
-- Name: albums_album_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.albums_album_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.albums_album_id_seq OWNER TO postgres;

--
-- Name: albums_album_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.albums_album_id_seq OWNED BY public.albums.album_id;


--
-- Name: appeals; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.appeals (
    id_appeals integer NOT NULL,
    number integer,
    appeal_citizen integer NOT NULL,
    police_officer integer NOT NULL,
    content text NOT NULL,
    making_date_and_time timestamp without time zone NOT NULL
);


ALTER TABLE public.appeals OWNER TO postgres;

--
-- Name: appeals_id_appeals_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.appeals_id_appeals_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.appeals_id_appeals_seq OWNER TO postgres;

--
-- Name: appeals_id_appeals_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.appeals_id_appeals_seq OWNED BY public.appeals.id_appeals;


--
-- Name: article; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.article (
    id_article integer NOT NULL,
    number_of_article public.article_number_domain NOT NULL,
    description text NOT NULL
);


ALTER TABLE public.article OWNER TO postgres;

--
-- Name: article_id_article_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.article_id_article_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.article_id_article_seq OWNER TO postgres;

--
-- Name: article_id_article_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.article_id_article_seq OWNED BY public.article.id_article;


--
-- Name: articles_and_responsobility; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.articles_and_responsobility (
    id_articles_and_responsibility integer CONSTRAINT articles_and_responsobility_id_articles_and_responsibi_not_null NOT NULL,
    responsibility integer NOT NULL,
    article integer NOT NULL
);


ALTER TABLE public.articles_and_responsobility OWNER TO postgres;

--
-- Name: articles_and_responsobility_id_articles_and_responsibility_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.articles_and_responsobility_id_articles_and_responsibility_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.articles_and_responsobility_id_articles_and_responsibility_seq OWNER TO postgres;

--
-- Name: articles_and_responsobility_id_articles_and_responsibility_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.articles_and_responsobility_id_articles_and_responsibility_seq OWNED BY public.articles_and_responsobility.id_articles_and_responsibility;


--
-- Name: artists; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.artists (
    artist_id integer NOT NULL,
    artist_name character varying(100) NOT NULL
);


ALTER TABLE public.artists OWNER TO postgres;

--
-- Name: artists_artist_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.artists_artist_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.artists_artist_id_seq OWNER TO postgres;

--
-- Name: artists_artist_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.artists_artist_id_seq OWNED BY public.artists.artist_id;


--
-- Name: cap_ranks; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cap_ranks (
    id integer NOT NULL,
    user_citizen_link integer NOT NULL,
    rank integer NOT NULL
);


ALTER TABLE public.cap_ranks OWNER TO postgres;

--
-- Name: citizen_phones; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.citizen_phones (
    id integer NOT NULL,
    phone_number text NOT NULL,
    citizen integer NOT NULL,
    is_primary boolean DEFAULT false
);


ALTER TABLE public.citizen_phones OWNER TO postgres;

--
-- Name: citizen_phones_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.citizen_phones_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.citizen_phones_id_seq OWNER TO postgres;

--
-- Name: citizen_phones_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.citizen_phones_id_seq OWNED BY public.citizen_phones.id;


--
-- Name: citizens; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.citizens (
    id_citizens integer NOT NULL,
    last_name public.name_domain NOT NULL,
    first_name public.name_domain NOT NULL,
    patronymic public.name_domain,
    birthday date NOT NULL,
    address_registration public.address_domain NOT NULL,
    working_place integer NOT NULL,
    post integer NOT NULL,
    criminal_record boolean NOT NULL,
    count_record integer NOT NULL,
    passport_series_and_number character varying(12) NOT NULL,
    family_status integer NOT NULL,
    education integer NOT NULL,
    citizenship integer NOT NULL
);


ALTER TABLE public.citizens OWNER TO postgres;

--
-- Name: citizens_and_posts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.citizens_and_posts (
    id_citizens_and_posts integer NOT NULL,
    citizen integer NOT NULL,
    citizen_post integer NOT NULL
);


ALTER TABLE public.citizens_and_posts OWNER TO postgres;

--
-- Name: citizens_and_posts_id_citizens_and_posts_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.citizens_and_posts_id_citizens_and_posts_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.citizens_and_posts_id_citizens_and_posts_seq OWNER TO postgres;

--
-- Name: citizens_and_posts_id_citizens_and_posts_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.citizens_and_posts_id_citizens_and_posts_seq OWNED BY public.citizens_and_posts.id_citizens_and_posts;


--
-- Name: citizens_id_citizens_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.citizens_id_citizens_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.citizens_id_citizens_seq OWNER TO postgres;

--
-- Name: citizens_id_citizens_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.citizens_id_citizens_seq OWNED BY public.citizens.id_citizens;


--
-- Name: citizenship; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.citizenship (
    id_citizenship integer NOT NULL,
    citizenship character varying(50) NOT NULL
);


ALTER TABLE public.citizenship OWNER TO postgres;

--
-- Name: citizenship_id_citizenship_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.citizenship_id_citizenship_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.citizenship_id_citizenship_seq OWNER TO postgres;

--
-- Name: citizenship_id_citizenship_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.citizenship_id_citizenship_seq OWNED BY public.citizenship.id_citizenship;


--
-- Name: deal; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.deal (
    id_deal integer NOT NULL,
    deal_number integer NOT NULL,
    settlement integer NOT NULL,
    offender integer NOT NULL,
    first_witness integer NOT NULL,
    second_witness integer NOT NULL,
    police_officer integer NOT NULL,
    article integer NOT NULL,
    responsibility integer NOT NULL,
    making_date timestamp without time zone DEFAULT now()
);


ALTER TABLE public.deal OWNER TO postgres;

--
-- Name: deal_id_deal_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.deal_id_deal_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.deal_id_deal_seq OWNER TO postgres;

--
-- Name: deal_id_deal_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.deal_id_deal_seq OWNED BY public.deal.id_deal;


--
-- Name: document_access_requests; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.document_access_requests (
    id integer NOT NULL,
    user_id integer NOT NULL,
    table_name character varying(50) NOT NULL,
    document_id integer NOT NULL,
    reason text NOT NULL,
    request_date timestamp without time zone NOT NULL,
    status character varying(20) DEFAULT 'pending'::character varying
);


ALTER TABLE public.document_access_requests OWNER TO postgres;

--
-- Name: document_access_requests_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.document_access_requests_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.document_access_requests_id_seq OWNER TO postgres;

--
-- Name: document_access_requests_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.document_access_requests_id_seq OWNED BY public.document_access_requests.id;


--
-- Name: documents_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.documents_type (
    id integer NOT NULL,
    document_type character varying(50) NOT NULL
);


ALTER TABLE public.documents_type OWNER TO postgres;

--
-- Name: documents_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.documents_type_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.documents_type_id_seq OWNER TO postgres;

--
-- Name: documents_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.documents_type_id_seq OWNED BY public.documents_type.id;


--
-- Name: drafts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.drafts (
    id_draft integer NOT NULL,
    user_id integer NOT NULL,
    document_type character varying(50) NOT NULL,
    form_data jsonb NOT NULL,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.drafts OWNER TO postgres;

--
-- Name: drafts_id_draft_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.drafts_id_draft_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.drafts_id_draft_seq OWNER TO postgres;

--
-- Name: drafts_id_draft_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.drafts_id_draft_seq OWNED BY public.drafts.id_draft;


--
-- Name: education; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.education (
    id_education integer NOT NULL,
    education character varying(50) NOT NULL
);


ALTER TABLE public.education OWNER TO postgres;

--
-- Name: education_id_education_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.education_id_education_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.education_id_education_seq OWNER TO postgres;

--
-- Name: education_id_education_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.education_id_education_seq OWNED BY public.education.id_education;


--
-- Name: explanation_protocol; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.explanation_protocol (
    id_explanation_protocol integer NOT NULL,
    number integer,
    making_date_and_time timestamp without time zone NOT NULL,
    citizen integer NOT NULL,
    deal integer NOT NULL,
    signature_for_error_testimony boolean,
    signature_for_knowing_everithing boolean,
    content text NOT NULL,
    need_forensic_medical_examination boolean NOT NULL,
    need_medical_examination_certificate boolean CONSTRAINT explanation_protocol_need_medical_examination_certific_not_null NOT NULL,
    citizen_signature boolean,
    police_officer_signature boolean
);


ALTER TABLE public.explanation_protocol OWNER TO postgres;

--
-- Name: explanation_protocol_id_explanation_protocol_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.explanation_protocol_id_explanation_protocol_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.explanation_protocol_id_explanation_protocol_seq OWNER TO postgres;

--
-- Name: explanation_protocol_id_explanation_protocol_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.explanation_protocol_id_explanation_protocol_seq OWNED BY public.explanation_protocol.id_explanation_protocol;


--
-- Name: family_status; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.family_status (
    id_family_status integer NOT NULL,
    family_status character varying(50) NOT NULL
);


ALTER TABLE public.family_status OWNER TO postgres;

--
-- Name: family_status_id_family_status_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.family_status_id_family_status_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.family_status_id_family_status_seq OWNER TO postgres;

--
-- Name: family_status_id_family_status_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.family_status_id_family_status_seq OWNED BY public.family_status.id_family_status;


--
-- Name: forensic_medical_examination; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.forensic_medical_examination (
    id_forensic_medical_examination integer CONSTRAINT forensic_medical_examinatio_id_forensic_medical_examin_not_null NOT NULL,
    number integer,
    making_date_and_time timestamp without time zone NOT NULL,
    structure integer NOT NULL,
    deal integer,
    expert integer NOT NULL,
    content text NOT NULL,
    physical_injuries boolean NOT NULL,
    severity_of_harm_to_health boolean CONSTRAINT forensic_medical_examinatio_severity_of_harm_to_health_not_null NOT NULL,
    could_injuries_have_occurred_on_time boolean CONSTRAINT forensic_medical_examinatio_could_injuries_have_occurr_not_null NOT NULL,
    signature_expert boolean
);


ALTER TABLE public.forensic_medical_examination OWNER TO postgres;

--
-- Name: forensic_medical_examination_id_forensic_medical_examinatio_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.forensic_medical_examination_id_forensic_medical_examinatio_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.forensic_medical_examination_id_forensic_medical_examinatio_seq OWNER TO postgres;

--
-- Name: forensic_medical_examination_id_forensic_medical_examinatio_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.forensic_medical_examination_id_forensic_medical_examinatio_seq OWNED BY public.forensic_medical_examination.id_forensic_medical_examination;


--
-- Name: medical_examination_certificate; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.medical_examination_certificate (
    id_medical_examination_certificate integer CONSTRAINT medical_examination_certifi_id_medical_examination_cer_not_null NOT NULL,
    number integer,
    medical_examination_report integer CONSTRAINT medical_examination_certifi_medical_examination_report_not_null NOT NULL,
    making_date_and_time timestamp without time zone NOT NULL,
    medical_institution integer NOT NULL,
    doctor integer NOT NULL,
    signs_of_intoxication text NOT NULL,
    result text NOT NULL,
    type_intoxication integer NOT NULL,
    doctor_signature boolean
);


ALTER TABLE public.medical_examination_certificate OWNER TO postgres;

--
-- Name: medical_examination_certifica_id_medical_examination_certif_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.medical_examination_certifica_id_medical_examination_certif_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.medical_examination_certifica_id_medical_examination_certif_seq OWNER TO postgres;

--
-- Name: medical_examination_certifica_id_medical_examination_certif_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.medical_examination_certifica_id_medical_examination_certif_seq OWNED BY public.medical_examination_certificate.id_medical_examination_certificate;


--
-- Name: medical_examination_report; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.medical_examination_report (
    id_medical_examination_report integer CONSTRAINT medical_examination_report_id_medical_examination_repo_not_null NOT NULL,
    number integer,
    deal integer,
    report integer NOT NULL,
    patient integer NOT NULL,
    making_date_and_time timestamp without time zone NOT NULL,
    signs_of_intoxication text NOT NULL,
    content text NOT NULL,
    officer_signature boolean,
    citizen_signature boolean
);


ALTER TABLE public.medical_examination_report OWNER TO postgres;

--
-- Name: medical_examination_report_id_medical_examination_report_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.medical_examination_report_id_medical_examination_report_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.medical_examination_report_id_medical_examination_report_seq OWNER TO postgres;

--
-- Name: medical_examination_report_id_medical_examination_report_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.medical_examination_report_id_medical_examination_report_seq OWNED BY public.medical_examination_report.id_medical_examination_report;


--
-- Name: post; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.post (
    id_post integer NOT NULL,
    post character varying(50) NOT NULL
);


ALTER TABLE public.post OWNER TO postgres;

--
-- Name: post_id_post_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.post_id_post_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.post_id_post_seq OWNER TO postgres;

--
-- Name: post_id_post_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.post_id_post_seq OWNED BY public.post.id_post;


--
-- Name: rank; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.rank (
    id integer NOT NULL,
    rank character varying(100) NOT NULL
);


ALTER TABLE public.rank OWNER TO postgres;

--
-- Name: resolution; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.resolution (
    id_resolution integer NOT NULL,
    protocol_number public.protocol_number_domain,
    making_date_and_time timestamp without time zone NOT NULL,
    court_staff integer NOT NULL,
    deal integer NOT NULL,
    resolution text NOT NULL,
    punishment integer NOT NULL,
    fine_sum public.fine_amount_domain
);


ALTER TABLE public.resolution OWNER TO postgres;

--
-- Name: resolution_id_resolution_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.resolution_id_resolution_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.resolution_id_resolution_seq OWNER TO postgres;

--
-- Name: resolution_id_resolution_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.resolution_id_resolution_seq OWNED BY public.resolution.id_resolution;


--
-- Name: responsibility; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.responsibility (
    id_responsibility integer NOT NULL,
    type_of_responsibility character varying(50) NOT NULL
);


ALTER TABLE public.responsibility OWNER TO postgres;

--
-- Name: responsibility_id_responsibility_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.responsibility_id_responsibility_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.responsibility_id_responsibility_seq OWNER TO postgres;

--
-- Name: responsibility_id_responsibility_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.responsibility_id_responsibility_seq OWNED BY public.responsibility.id_responsibility;


--
-- Name: roles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.roles (
    id integer NOT NULL,
    role character varying(50) NOT NULL
);


ALTER TABLE public.roles OWNER TO postgres;

--
-- Name: settlements; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.settlements (
    id_settlements integer NOT NULL,
    title_of_settlements character varying(50) NOT NULL
);


ALTER TABLE public.settlements OWNER TO postgres;

--
-- Name: settlements_id_settlements_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.settlements_id_settlements_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.settlements_id_settlements_seq OWNER TO postgres;

--
-- Name: settlements_id_settlements_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.settlements_id_settlements_seq OWNED BY public.settlements.id_settlements;


--
-- Name: songs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.songs (
    song_id integer NOT NULL,
    song_name character varying(200) NOT NULL,
    album_id integer NOT NULL
);


ALTER TABLE public.songs OWNER TO postgres;

--
-- Name: songs_song_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.songs_song_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.songs_song_id_seq OWNER TO postgres;

--
-- Name: songs_song_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.songs_song_id_seq OWNED BY public.songs.song_id;


--
-- Name: statement; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.statement (
    id_statement integer NOT NULL,
    applicant integer NOT NULL,
    content text NOT NULL,
    date_and_time timestamp without time zone NOT NULL,
    police_officer integer NOT NULL,
    signature_applicant boolean,
    signature_police_officer boolean,
    number integer
);


ALTER TABLE public.statement OWNER TO postgres;

--
-- Name: statement_id_statement_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.statement_id_statement_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.statement_id_statement_seq OWNER TO postgres;

--
-- Name: statement_id_statement_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.statement_id_statement_seq OWNED BY public.statement.id_statement;


--
-- Name: structures; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.structures (
    id_structures integer NOT NULL,
    name character varying(50) NOT NULL,
    settlement integer NOT NULL,
    description text NOT NULL
);


ALTER TABLE public.structures OWNER TO postgres;

--
-- Name: structures_id_structures_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.structures_id_structures_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.structures_id_structures_seq OWNER TO postgres;

--
-- Name: structures_id_structures_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.structures_id_structures_seq OWNED BY public.structures.id_structures;


--
-- Name: type_intoxication; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.type_intoxication (
    id_type_intoxication integer NOT NULL,
    type_intoxication character varying(50) NOT NULL
);


ALTER TABLE public.type_intoxication OWNER TO postgres;

--
-- Name: type_intoxication_id_type_intoxication_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.type_intoxication_id_type_intoxication_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.type_intoxication_id_type_intoxication_seq OWNER TO postgres;

--
-- Name: type_intoxication_id_type_intoxication_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.type_intoxication_id_type_intoxication_seq OWNED BY public.type_intoxication.id_type_intoxication;


--
-- Name: type_of_punishment; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.type_of_punishment (
    id_type_of_punishment integer NOT NULL,
    type_of_punishment character varying(50) NOT NULL
);


ALTER TABLE public.type_of_punishment OWNER TO postgres;

--
-- Name: type_of_punishment_id_type_of_punishment_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.type_of_punishment_id_type_of_punishment_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.type_of_punishment_id_type_of_punishment_seq OWNER TO postgres;

--
-- Name: type_of_punishment_id_type_of_punishment_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.type_of_punishment_id_type_of_punishment_seq OWNED BY public.type_of_punishment.id_type_of_punishment;


--
-- Name: type_report; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.type_report (
    id_type_report integer NOT NULL,
    type_report character varying(50) NOT NULL
);


ALTER TABLE public.type_report OWNER TO postgres;

--
-- Name: type_report_id_type_report_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.type_report_id_type_report_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.type_report_id_type_report_seq OWNER TO postgres;

--
-- Name: type_report_id_type_report_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.type_report_id_type_report_seq OWNED BY public.type_report.id_type_report;


--
-- Name: user_citizen_link; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.user_citizen_link (
    id integer NOT NULL,
    user_id integer NOT NULL,
    citizen_post_id integer NOT NULL
);


ALTER TABLE public.user_citizen_link OWNER TO postgres;

--
-- Name: user_citizen_link_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.user_citizen_link_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.user_citizen_link_id_seq OWNER TO postgres;

--
-- Name: user_citizen_link_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.user_citizen_link_id_seq OWNED BY public.user_citizen_link.id;


--
-- Name: user_favorites; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.user_favorites (
    id integer NOT NULL,
    user_id integer NOT NULL,
    target_table character varying(50) NOT NULL,
    document_id integer NOT NULL,
    created_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.user_favorites OWNER TO postgres;

--
-- Name: user_favorites_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.user_favorites_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.user_favorites_id_seq OWNER TO postgres;

--
-- Name: user_favorites_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.user_favorites_id_seq OWNED BY public.user_favorites.id;


--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id integer NOT NULL,
    username character varying(50) NOT NULL,
    password character varying(100) NOT NULL,
    last_name character varying(50) NOT NULL,
    first_name character varying(50) NOT NULL,
    patronymic character varying(50),
    created_at timestamp without time zone DEFAULT now(),
    role integer NOT NULL
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_id_seq OWNER TO postgres;

--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: view_recent_documents; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.view_recent_documents AS
 SELECT s.id_statement AS id,
    1 AS type_id,
    'Заявление'::text AS type_name,
    s.number,
    s.date_and_time AS making_date,
    s.applicant AS citizen_id,
    (((((c.last_name)::text || ' '::text) || (c.first_name)::text) || ' '::text) || (COALESCE((c.patronymic)::character varying, ''::character varying))::text) AS citizen_name
   FROM (public.statement s
     LEFT JOIN public.citizens c ON ((s.applicant = c.id_citizens)))
UNION ALL
 SELECT a.id_appeals AS id,
    2 AS type_id,
    'Обращение'::text AS type_name,
    a.number,
    a.making_date_and_time AS making_date,
    a.appeal_citizen AS citizen_id,
    (((((c.last_name)::text || ' '::text) || (c.first_name)::text) || ' '::text) || (COALESCE((c.patronymic)::character varying, ''::character varying))::text) AS citizen_name
   FROM (public.appeals a
     LEFT JOIN public.citizens c ON ((a.appeal_citizen = c.id_citizens)))
UNION ALL
 SELECT ep.id_explanation_protocol AS id,
    3 AS type_id,
    'Протокол объяснения'::text AS type_name,
    ep.number,
    ep.making_date_and_time AS making_date,
    ep.citizen AS citizen_id,
    (((((c.last_name)::text || ' '::text) || (c.first_name)::text) || ' '::text) || (COALESCE((c.patronymic)::character varying, ''::character varying))::text) AS citizen_name
   FROM (public.explanation_protocol ep
     LEFT JOIN public.citizens c ON ((ep.citizen = c.id_citizens)))
UNION ALL
 SELECT mer.id_medical_examination_report AS id,
    4 AS type_id,
    'Направление на мед. освид.'::text AS type_name,
    mer.number,
    mer.making_date_and_time AS making_date,
    mer.patient AS citizen_id,
    (((((c.last_name)::text || ' '::text) || (c.first_name)::text) || ' '::text) || (COALESCE((c.patronymic)::character varying, ''::character varying))::text) AS citizen_name
   FROM (public.medical_examination_report mer
     LEFT JOIN public.citizens c ON ((mer.patient = c.id_citizens)))
UNION ALL
 SELECT ap.id_protocol AS id,
    5 AS type_id,
    'Административный протокол'::text AS type_name,
    ap.protocol_number AS number,
    ap.making_date_and_time AS making_date,
    d.offender AS citizen_id,
    (((((c.last_name)::text || ' '::text) || (c.first_name)::text) || ' '::text) || (COALESCE((c.patronymic)::character varying, ''::character varying))::text) AS citizen_name
   FROM ((public.administrative_protocol ap
     LEFT JOIN public.deal d ON ((ap.deal = d.id_deal)))
     LEFT JOIN public.citizens c ON ((d.offender = c.id_citizens)));


ALTER VIEW public.view_recent_documents OWNER TO postgres;

--
-- Name: albums; Type: TABLE; Schema: testdb; Owner: postgres
--

CREATE TABLE testdb.albums (
    album_id integer NOT NULL,
    album_name character varying(100) NOT NULL,
    artist_id integer NOT NULL
);


ALTER TABLE testdb.albums OWNER TO postgres;

--
-- Name: albums_album_id_seq; Type: SEQUENCE; Schema: testdb; Owner: postgres
--

CREATE SEQUENCE testdb.albums_album_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE testdb.albums_album_id_seq OWNER TO postgres;

--
-- Name: albums_album_id_seq; Type: SEQUENCE OWNED BY; Schema: testdb; Owner: postgres
--

ALTER SEQUENCE testdb.albums_album_id_seq OWNED BY testdb.albums.album_id;


--
-- Name: artists; Type: TABLE; Schema: testdb; Owner: postgres
--

CREATE TABLE testdb.artists (
    artist_id integer NOT NULL,
    artist_name character varying(100) NOT NULL
);


ALTER TABLE testdb.artists OWNER TO postgres;

--
-- Name: artists_artist_id_seq; Type: SEQUENCE; Schema: testdb; Owner: postgres
--

CREATE SEQUENCE testdb.artists_artist_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE testdb.artists_artist_id_seq OWNER TO postgres;

--
-- Name: artists_artist_id_seq; Type: SEQUENCE OWNED BY; Schema: testdb; Owner: postgres
--

ALTER SEQUENCE testdb.artists_artist_id_seq OWNED BY testdb.artists.artist_id;


--
-- Name: rockgroups_notes; Type: TABLE; Schema: testdb; Owner: postgres
--

CREATE TABLE testdb.rockgroups_notes (
    id integer NOT NULL,
    song_name character varying(100) NOT NULL,
    group_name character varying(100) NOT NULL,
    album_name character varying(50) NOT NULL
);


ALTER TABLE testdb.rockgroups_notes OWNER TO postgres;

--
-- Name: rockgroups_notes_id_seq; Type: SEQUENCE; Schema: testdb; Owner: postgres
--

CREATE SEQUENCE testdb.rockgroups_notes_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE testdb.rockgroups_notes_id_seq OWNER TO postgres;

--
-- Name: rockgroups_notes_id_seq; Type: SEQUENCE OWNED BY; Schema: testdb; Owner: postgres
--

ALTER SEQUENCE testdb.rockgroups_notes_id_seq OWNED BY testdb.rockgroups_notes.id;


--
-- Name: songs; Type: TABLE; Schema: testdb; Owner: postgres
--

CREATE TABLE testdb.songs (
    song_id integer NOT NULL,
    song_name character varying(200) NOT NULL,
    album_id integer NOT NULL
);


ALTER TABLE testdb.songs OWNER TO postgres;

--
-- Name: songs_song_id_seq; Type: SEQUENCE; Schema: testdb; Owner: postgres
--

CREATE SEQUENCE testdb.songs_song_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE testdb.songs_song_id_seq OWNER TO postgres;

--
-- Name: songs_song_id_seq; Type: SEQUENCE OWNED BY; Schema: testdb; Owner: postgres
--

ALTER SEQUENCE testdb.songs_song_id_seq OWNED BY testdb.songs.song_id;


--
-- Name: Альбомы Код; Type: DEFAULT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Альбомы" ALTER COLUMN "Код" SET DEFAULT nextval('"Music"."Альбомы_Код_seq"'::regclass);


--
-- Name: Жанры Код; Type: DEFAULT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Жанры" ALTER COLUMN "Код" SET DEFAULT nextval('"Music"."Жанры_Код_seq"'::regclass);


--
-- Name: Жанры_и_исполнители Код; Type: DEFAULT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Жанры_и_исполнители" ALTER COLUMN "Код" SET DEFAULT nextval('"Music"."Жанры_и_исполнители_Код_seq"'::regclass);


--
-- Name: Исполнители Код; Type: DEFAULT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Исполнители" ALTER COLUMN "Код" SET DEFAULT nextval('"Music"."Исполнители_Код_seq"'::regclass);


--
-- Name: Композиции Код; Type: DEFAULT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Композиции" ALTER COLUMN "Код" SET DEFAULT nextval('"Music"."Композиции_Код_seq"'::regclass);


--
-- Name: Композиции_и_исполнители Код; Type: DEFAULT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Композиции_и_исполнители" ALTER COLUMN "Код" SET DEFAULT nextval('"Music"."Композиции_и_исполнители_Код_seq"'::regclass);


--
-- Name: categories id; Type: DEFAULT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".categories ALTER COLUMN id SET DEFAULT nextval('"Practice".categories_id_seq'::regclass);


--
-- Name: limits id; Type: DEFAULT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".limits ALTER COLUMN id SET DEFAULT nextval('"Practice".limits_id_seq'::regclass);


--
-- Name: recipient id; Type: DEFAULT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".recipient ALTER COLUMN id SET DEFAULT nextval('"Practice".recipient_id_seq'::regclass);


--
-- Name: roles id; Type: DEFAULT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".roles ALTER COLUMN id SET DEFAULT nextval('"Practice".roles_id_seq'::regclass);


--
-- Name: transactions id; Type: DEFAULT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".transactions ALTER COLUMN id SET DEFAULT nextval('"Practice".transactions_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".users ALTER COLUMN id SET DEFAULT nextval('"Practice".users_id_seq'::regclass);


--
-- Name: authors id; Type: DEFAULT; Schema: Study; Owner: postgres
--

ALTER TABLE ONLY "Study".authors ALTER COLUMN id SET DEFAULT nextval('"Study".authors_id_seq'::regclass);


--
-- Name: circles circle_id; Type: DEFAULT; Schema: bilet1; Owner: postgres
--

ALTER TABLE ONLY bilet1.circles ALTER COLUMN circle_id SET DEFAULT nextval('bilet1.circles_circle_id_seq'::regclass);


--
-- Name: leaders leader_id; Type: DEFAULT; Schema: bilet1; Owner: postgres
--

ALTER TABLE ONLY bilet1.leaders ALTER COLUMN leader_id SET DEFAULT nextval('bilet1.leaders_leader_id_seq'::regclass);


--
-- Name: visits visit_id; Type: DEFAULT; Schema: bilet1; Owner: postgres
--

ALTER TABLE ONLY bilet1.visits ALTER COLUMN visit_id SET DEFAULT nextval('bilet1.visits_visit_id_seq'::regclass);


--
-- Name: administrative_protocol id_protocol; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.administrative_protocol ALTER COLUMN id_protocol SET DEFAULT nextval('public.administrative_protocol_id_protocol_seq'::regclass);


--
-- Name: albums album_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.albums ALTER COLUMN album_id SET DEFAULT nextval('public.albums_album_id_seq'::regclass);


--
-- Name: appeals id_appeals; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.appeals ALTER COLUMN id_appeals SET DEFAULT nextval('public.appeals_id_appeals_seq'::regclass);


--
-- Name: article id_article; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.article ALTER COLUMN id_article SET DEFAULT nextval('public.article_id_article_seq'::regclass);


--
-- Name: articles_and_responsobility id_articles_and_responsibility; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.articles_and_responsobility ALTER COLUMN id_articles_and_responsibility SET DEFAULT nextval('public.articles_and_responsobility_id_articles_and_responsibility_seq'::regclass);


--
-- Name: artists artist_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.artists ALTER COLUMN artist_id SET DEFAULT nextval('public.artists_artist_id_seq'::regclass);


--
-- Name: citizen_phones id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizen_phones ALTER COLUMN id SET DEFAULT nextval('public.citizen_phones_id_seq'::regclass);


--
-- Name: citizens id_citizens; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizens ALTER COLUMN id_citizens SET DEFAULT nextval('public.citizens_id_citizens_seq'::regclass);


--
-- Name: citizens_and_posts id_citizens_and_posts; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizens_and_posts ALTER COLUMN id_citizens_and_posts SET DEFAULT nextval('public.citizens_and_posts_id_citizens_and_posts_seq'::regclass);


--
-- Name: citizenship id_citizenship; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizenship ALTER COLUMN id_citizenship SET DEFAULT nextval('public.citizenship_id_citizenship_seq'::regclass);


--
-- Name: deal id_deal; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deal ALTER COLUMN id_deal SET DEFAULT nextval('public.deal_id_deal_seq'::regclass);


--
-- Name: document_access_requests id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.document_access_requests ALTER COLUMN id SET DEFAULT nextval('public.document_access_requests_id_seq'::regclass);


--
-- Name: documents_type id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.documents_type ALTER COLUMN id SET DEFAULT nextval('public.documents_type_id_seq'::regclass);


--
-- Name: drafts id_draft; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.drafts ALTER COLUMN id_draft SET DEFAULT nextval('public.drafts_id_draft_seq'::regclass);


--
-- Name: education id_education; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.education ALTER COLUMN id_education SET DEFAULT nextval('public.education_id_education_seq'::regclass);


--
-- Name: explanation_protocol id_explanation_protocol; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.explanation_protocol ALTER COLUMN id_explanation_protocol SET DEFAULT nextval('public.explanation_protocol_id_explanation_protocol_seq'::regclass);


--
-- Name: family_status id_family_status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.family_status ALTER COLUMN id_family_status SET DEFAULT nextval('public.family_status_id_family_status_seq'::regclass);


--
-- Name: forensic_medical_examination id_forensic_medical_examination; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.forensic_medical_examination ALTER COLUMN id_forensic_medical_examination SET DEFAULT nextval('public.forensic_medical_examination_id_forensic_medical_examinatio_seq'::regclass);


--
-- Name: medical_examination_certificate id_medical_examination_certificate; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medical_examination_certificate ALTER COLUMN id_medical_examination_certificate SET DEFAULT nextval('public.medical_examination_certifica_id_medical_examination_certif_seq'::regclass);


--
-- Name: medical_examination_report id_medical_examination_report; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medical_examination_report ALTER COLUMN id_medical_examination_report SET DEFAULT nextval('public.medical_examination_report_id_medical_examination_report_seq'::regclass);


--
-- Name: post id_post; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.post ALTER COLUMN id_post SET DEFAULT nextval('public.post_id_post_seq'::regclass);


--
-- Name: resolution id_resolution; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.resolution ALTER COLUMN id_resolution SET DEFAULT nextval('public.resolution_id_resolution_seq'::regclass);


--
-- Name: responsibility id_responsibility; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.responsibility ALTER COLUMN id_responsibility SET DEFAULT nextval('public.responsibility_id_responsibility_seq'::regclass);


--
-- Name: settlements id_settlements; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.settlements ALTER COLUMN id_settlements SET DEFAULT nextval('public.settlements_id_settlements_seq'::regclass);


--
-- Name: songs song_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.songs ALTER COLUMN song_id SET DEFAULT nextval('public.songs_song_id_seq'::regclass);


--
-- Name: statement id_statement; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statement ALTER COLUMN id_statement SET DEFAULT nextval('public.statement_id_statement_seq'::regclass);


--
-- Name: structures id_structures; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.structures ALTER COLUMN id_structures SET DEFAULT nextval('public.structures_id_structures_seq'::regclass);


--
-- Name: type_intoxication id_type_intoxication; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.type_intoxication ALTER COLUMN id_type_intoxication SET DEFAULT nextval('public.type_intoxication_id_type_intoxication_seq'::regclass);


--
-- Name: type_of_punishment id_type_of_punishment; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.type_of_punishment ALTER COLUMN id_type_of_punishment SET DEFAULT nextval('public.type_of_punishment_id_type_of_punishment_seq'::regclass);


--
-- Name: type_report id_type_report; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.type_report ALTER COLUMN id_type_report SET DEFAULT nextval('public.type_report_id_type_report_seq'::regclass);


--
-- Name: user_citizen_link id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_citizen_link ALTER COLUMN id SET DEFAULT nextval('public.user_citizen_link_id_seq'::regclass);


--
-- Name: user_favorites id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_favorites ALTER COLUMN id SET DEFAULT nextval('public.user_favorites_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Name: albums album_id; Type: DEFAULT; Schema: testdb; Owner: postgres
--

ALTER TABLE ONLY testdb.albums ALTER COLUMN album_id SET DEFAULT nextval('testdb.albums_album_id_seq'::regclass);


--
-- Name: artists artist_id; Type: DEFAULT; Schema: testdb; Owner: postgres
--

ALTER TABLE ONLY testdb.artists ALTER COLUMN artist_id SET DEFAULT nextval('testdb.artists_artist_id_seq'::regclass);


--
-- Name: rockgroups_notes id; Type: DEFAULT; Schema: testdb; Owner: postgres
--

ALTER TABLE ONLY testdb.rockgroups_notes ALTER COLUMN id SET DEFAULT nextval('testdb.rockgroups_notes_id_seq'::regclass);


--
-- Name: songs song_id; Type: DEFAULT; Schema: testdb; Owner: postgres
--

ALTER TABLE ONLY testdb.songs ALTER COLUMN song_id SET DEFAULT nextval('testdb.songs_song_id_seq'::regclass);


--
-- Data for Name: Альбомы; Type: TABLE DATA; Schema: Music; Owner: postgres
--

COPY "Music"."Альбомы" ("Код", "Название", "Исполнитель", "Жанр", "Дата_выхода") FROM stdin;
1	REMAINS	1	1	2020-05-30
2	REMAINS	3	1	2020-05-30
\.


--
-- Data for Name: Жанры; Type: TABLE DATA; Schema: Music; Owner: postgres
--

COPY "Music"."Жанры" ("Код", "Название_жанра", "Описание") FROM stdin;
1	Рэп	\N
2	Классика	\N
3	Неоклассика	\N
4	Кантри	\N
5	Поп	\N
6	Современная инструментальная музыка	\N
7	Рок	\N
8	Хэви-метал	\N
9	Метал	\N
10	Деф метал	\N
11	Классический рэп	\N
\.


--
-- Data for Name: Жанры_и_исполнители; Type: TABLE DATA; Schema: Music; Owner: postgres
--

COPY "Music"."Жанры_и_исполнители" ("Код", "Исполнитель", "Жанр") FROM stdin;
\.


--
-- Data for Name: Исполнители; Type: TABLE DATA; Schema: Music; Owner: postgres
--

COPY "Music"."Исполнители" ("Код", "Исполнитель", "Описание") FROM stdin;
1	BONES	Популярный андеграунд репер
2	Бетховен	Величайший композитор в истории
3	LYSON	Продюссер команды Team Sesh
4	Tomoya Naka	\N
5	Lizer	\N
6	Taylor Swift	\N
8	Ice Watch	\N
\.


--
-- Data for Name: Композиции; Type: TABLE DATA; Schema: Music; Owner: postgres
--

COPY "Music"."Композиции" ("Код", "Название_композиции", "Жанр", "Описание", "Оценка", "Альбом") FROM stdin;
12	Floor105	1	\N	\N	\N
13	CousinEddie	1	\N	\N	\N
14	Topaz	1	\N	\N	\N
15	SweetTooth	1	\N	\N	\N
16	SalamanderSandals	1	\N	\N	\N
17	Driveway	1	\N	\N	\N
18	MaryTylerMoore	1	\N	\N	\N
19	DeadEnd	1	\N	\N	\N
20	Ballerina	1	\N	\N	\N
21	Quebec	1	\N	\N	\N
22	CallWaiting	1	\N	\N	\N
23	Tonic	1	\N	\N	\N
24	DeadMansParadise	1	\N	\N	\N
25	WeCanGetGum	1	\N	\N	\N
26	SmokingLongDoors	1	\N	\N	\N
27	TurnTheAirOn,ItsHotAsHell	1	\N	\N	\N
28	CannonBall	1	\N	\N	\N
29	BackToBedBungo	1	\N	\N	\N
30	ForbiddenImage	1	\N	\N	\N
31	SugarFree	1	\N	\N	\N
32	Bandwidth	1	\N	\N	\N
33	CurseOfWalls	1	\N	\N	\N
34	HowMayHelpYou	1	\N	\N	\N
35	FlashbacksOfHowYouLeftMe	1	\N	\N	\N
36	MissingTextures	1	\N	\N	\N
37	IckyVicky	1	\N	\N	\N
38	HeadCrash	1	\N	\N	\N
39	StarkCounty	1	\N	\N	\N
40	NoLight	1	\N	\N	\N
41	DontShootTheMessenger	1	\N	\N	\N
42	ClickOfDeath	1	\N	\N	\N
43	PaidProgramming	1	\N	\N	\N
44	JonathanTaylorThomas	1	\N	\N	\N
45	WakingUpCrying	1	\N	\N	\N
46	RotatingBed	1	\N	\N	\N
47	Dial-Up	1	\N	\N	\N
48	AmericanBeauty	1	\N	\N	\N
49	281-330-8004	1	\N	\N	\N
50	JDayOutLook	1	\N	\N	\N
51	Cut	1	\N	\N	\N
52	FlashFloodWatch	1	\N	\N	\N
53	TeenageBoy	1	\N	\N	\N
54	Snow	1	\N	\N	\N
55	SevereWeatherWarning	1	\N	\N	\N
56	Rust	1	\N	\N	\N
57	30DayFreeTrial	1	\N	\N	\N
58	StoneColdStunner	1	\N	\N	\N
59	BackstreetBoy	1	\N	\N	\N
60	Nightmare	1	\N	\N	\N
61	BamMargera	1	\N	\N	\N
62	BoyBand	1	\N	\N	\N
63	DieforMe	1	\N	\N	\N
64	NoShirt	1	\N	\N	\N
65	BurnitDown	1	\N	\N	\N
66	Dirt	1	\N	\N	\N
67	DeathMetal	1	\N	\N	\N
68	StayTheNight	1	\N	\N	\N
69	GraveyardGod	1	\N	\N	\N
70	Corpse	1	\N	\N	\N
71	BathHouseBlunts	1	\N	\N	\N
72	Sanctuary	1	\N	\N	\N
73	HauntedHouse	1	\N	\N	\N
74	RedVelvetSofa	1	\N	\N	\N
75	BobbyKennedy	1	\N	\N	\N
76	TheWhiteWitch	1	\N	\N	\N
77	WhereTheSidewalkEnds	1	\N	\N	\N
78	Moshptis	1	\N	\N	\N
79	Bacteria	1	\N	\N	\N
80	SapphireSarcophagus	1	\N	\N	\N
81	Thorns	1	\N	\N	\N
82	Gravel	1	\N	\N	\N
83	TheSoundsOfDowntown	1	\N	\N	\N
84	BringMeToLife	1	\N	\N	\N
85	Cumulonimbus	1	\N	\N	\N
86	Worthless	1	\N	\N	\N
87	ClayAlken	1	\N	\N	\N
88	Shampoo	1	\N	\N	\N
89	Backroads	1	\N	\N	\N
90	LocalForecast	1	\N	\N	\N
91	UndergroundLegends	1	\N	\N	\N
92	BlownOut	1	\N	\N	\N
1	MonsterMash	1	\N	\N	1
2	Bently	1	\N	\N	1
3	SocialCues	1	\N	\N	1
4	Lacquer	1	\N	\N	1
5	BadReception	1	\N	\N	1
6	TakeCover	1	\N	\N	1
7	SkeletalLove	1	\N	\N	1
8	WaitingForHammmerToFall	1	\N	\N	1
9	SpiderSilkRobes	1	\N	\N	1
10	PeaceAndQuiet	1	\N	\N	1
11	WhateverFloatYourBoat	1	\N	\N	1
\.


--
-- Data for Name: Композиции_и_исполнители; Type: TABLE DATA; Schema: Music; Owner: postgres
--

COPY "Music"."Композиции_и_исполнители" ("Код", "Композиция", "Исполнитель") FROM stdin;
1	1	1
2	2	1
3	3	1
4	4	1
5	5	1
6	6	1
7	7	1
8	8	1
9	9	1
10	10	1
11	11	1
12	12	1
13	1	3
14	2	3
15	3	3
16	4	3
17	5	3
18	6	3
19	7	3
20	8	3
21	9	3
22	10	3
23	11	3
24	12	3
25	13	3
26	14	3
27	15	3
28	16	3
29	17	3
30	18	3
31	19	3
32	20	3
33	21	3
34	22	3
35	23	3
36	24	3
37	25	3
38	26	3
39	27	3
40	28	3
41	29	3
42	13	1
43	14	1
44	15	1
45	16	1
46	17	1
47	18	1
48	19	1
49	20	1
50	21	1
51	22	1
52	23	1
53	24	1
54	25	1
55	26	1
56	27	1
57	28	1
58	29	1
59	13	1
60	14	1
61	15	1
62	16	1
63	17	1
64	18	1
65	19	1
66	20	1
67	21	1
68	22	1
69	23	1
70	24	1
71	25	1
72	26	1
73	27	1
74	28	1
75	29	1
76	13	1
77	14	1
78	15	1
79	16	1
80	17	1
81	18	1
82	19	1
83	20	1
84	21	1
85	22	1
86	23	1
87	24	1
88	25	1
89	26	1
90	27	1
91	28	1
92	29	1
93	13	1
94	14	1
95	15	1
96	16	1
97	17	1
98	18	1
99	19	1
100	20	1
101	21	1
102	22	1
103	23	1
104	24	1
105	25	1
106	26	1
107	27	1
108	28	1
109	29	1
110	13	1
111	14	1
112	15	1
113	16	1
114	17	1
115	18	1
116	19	1
117	20	1
118	21	1
119	22	1
120	23	1
121	24	1
122	25	1
123	26	1
124	27	1
125	28	1
126	29	1
127	13	1
128	14	1
129	15	1
130	16	1
131	17	1
132	18	1
133	19	1
134	20	1
135	21	1
136	22	1
137	23	1
138	24	1
139	25	1
140	26	1
141	27	1
142	28	1
143	29	1
144	13	1
145	14	1
146	15	1
147	16	1
148	17	1
149	18	1
150	19	1
151	20	1
152	21	1
153	22	1
154	23	1
155	24	1
156	25	1
157	26	1
158	27	1
159	28	1
160	29	1
161	13	1
162	14	1
163	15	1
164	16	1
165	17	1
166	18	1
167	19	1
168	20	1
169	21	1
170	22	1
171	23	1
172	24	1
173	25	1
174	26	1
175	27	1
176	28	1
177	29	1
178	13	1
179	14	1
180	15	1
181	16	1
182	17	1
183	18	1
184	19	1
185	20	1
186	21	1
187	22	1
188	23	1
189	24	1
190	25	1
191	26	1
192	27	1
193	28	1
194	29	1
195	73	1
196	74	1
197	75	1
198	76	1
199	77	1
200	78	1
201	79	1
202	80	1
203	81	1
204	82	1
205	83	1
206	84	1
207	85	1
208	86	1
209	87	1
210	88	1
211	89	1
212	90	1
213	91	1
214	92	1
215	1	1
216	2	1
217	3	1
218	4	1
219	5	1
220	6	1
221	7	1
222	8	1
223	9	1
224	10	1
225	11	1
226	12	1
227	13	1
228	14	1
229	15	1
230	16	1
231	17	1
232	18	1
233	19	1
234	20	1
235	21	1
236	22	1
237	23	1
238	24	1
239	25	1
240	26	1
241	27	1
242	28	1
243	29	1
244	30	1
245	31	1
246	32	1
247	33	1
248	34	1
249	35	1
250	36	1
251	37	1
252	38	1
253	39	1
254	40	1
255	41	1
256	42	1
257	43	1
258	44	1
259	45	1
260	46	1
261	47	1
262	48	1
263	49	1
264	50	1
265	51	1
266	52	1
267	53	1
268	54	1
269	55	1
270	56	1
271	57	1
272	58	1
273	59	1
274	60	1
275	61	1
276	62	1
277	63	1
278	64	1
279	65	1
280	66	1
281	67	1
282	68	1
283	69	1
284	70	1
285	71	1
286	72	1
287	73	1
288	74	1
289	75	1
290	76	1
291	77	1
292	78	1
293	79	1
294	80	1
295	81	1
296	82	1
297	83	1
298	84	1
299	85	1
300	86	1
301	87	1
302	88	1
303	89	1
304	90	1
305	91	1
306	92	1
\.


--
-- Data for Name: categories; Type: TABLE DATA; Schema: Practice; Owner: postgres
--

COPY "Practice".categories (id, name, description) FROM stdin;
1	Коммунальные платежи	Квартплата, электричество, газ, вода
2	Автомобиль	Бензин, ремонт, страховка, ТО
3	Питание и быт	Продукты питания, хозяйственные товары
4	Медицина	Лекарства, прием врача, анализы
5	Образование	Курсы, книги, обучение
6	Развлечения	Кино, рестораны, хобби
7	Одежда	Одежда, обувь, аксессуары
8	Транспорт	Общественный транспорт, такси
9	Связь	Интернет, мобильная связь, ТВ
10	Прочее	Прочие расходы и доходы
\.


--
-- Data for Name: limits; Type: TABLE DATA; Schema: Practice; Owner: postgres
--

COPY "Practice".limits (id, user_id, date_beginning, date_ending, sum_limit) FROM stdin;
1	1	2024-01-01	2024-01-31	30000.00
2	2	2024-01-01	2024-01-31	25000.00
3	3	2024-01-01	2024-01-31	35000.00
4	1	2024-02-01	2024-02-29	32000.00
5	2	2024-02-01	2024-02-29	27000.00
6	4	2024-01-01	2024-01-31	28000.00
7	5	2024-01-01	2024-01-31	26000.00
8	6	2024-01-01	2024-01-31	38000.00
9	7	2024-01-01	2024-01-31	32000.00
10	8	2024-01-01	2024-01-31	27000.00
11	3	2024-02-01	2024-02-29	36000.00
12	4	2024-02-01	2024-02-29	29000.00
13	5	2024-02-01	2024-02-29	28000.00
14	6	2024-02-01	2024-02-29	40000.00
15	7	2024-02-01	2024-02-29	34000.00
16	1	2024-03-01	2024-03-31	35000.00
17	2	2024-03-01	2024-03-31	30000.00
18	3	2024-03-01	2024-03-31	37000.00
19	4	2024-03-01	2024-03-31	30000.00
20	5	2024-03-01	2024-03-31	29000.00
\.


--
-- Data for Name: recipient; Type: TABLE DATA; Schema: Practice; Owner: postgres
--

COPY "Practice".recipient (id, user_id, name, requisites, description) FROM stdin;
1	\N	ЖКХ Комфорт	12345	Коммунальные услуги
2	\N	АЗС Лукойл	23456	Заправка автомобиля
3	\N	Супермаркет Пятерочка	34567	Продукты питания
4	\N	Поликлиника №1	45678	Медицинские услуги
5	\N	Ростелеком	56789	Интернет и телефония
6	\N	Магазин Эльдорадо	67890	Бытовая техника
7	\N	Салон связи Связной	78901	Мобильная связь
8	\N	Кофейня Starbucks	\N	Кофе и закуски
9	\N	Фитнес клуб World Class	89012	Абонемент в спортзал
10	1	\N	891011	Перевод Василию на день рождения
11	2	\N	121314	Одолжил деньги Дмитрию
12	3	\N	\N	Вернул долг Петру
13	4	\N	151617	Подарок Василию
14	5	\N	\N	Перевод Анне за помощь
15	\N	Аптека №3	90123	Лекарства и медицинские товары
16	\N	Кафе Шоколадница	\N	Кофейня и ресторан
17	\N	Салон Красоты Люкс	123456	Парикмахерские услуги
18	\N	Кинотеатр Синема	234567	Билеты в кино
19	\N	Такси Сити	345678	Транспортные услуги
20	\N	Супермаркет Магнит	456789	Продуктовый магазин
21	\N	Спортмастер	567890	Спортивные товары
22	\N	Бургер Кинг	\N	Фастфуд
23	\N	Книжный магазин Читай	678901	Книги и канцтовары
24	\N	АЗС Газпром	789012	Заправка автомобиля
25	6	\N	890123	Перевод на подарок
26	7	\N	901234	Одолжил на ремонт
27	8	\N	\N	Возврат долга
28	9	\N	123789	Совместный подарок
29	10	\N	\N	Оплата услуг
\.


--
-- Data for Name: roles; Type: TABLE DATA; Schema: Practice; Owner: postgres
--

COPY "Practice".roles (id, role_id, description) FROM stdin;
1	admin	Администратор системы
2	user	Обычный пользователь
3	manager	Менеджер
\.


--
-- Data for Name: sender; Type: TABLE DATA; Schema: Practice; Owner: postgres
--

COPY "Practice".sender (id, sender, description, requisits) FROM stdin;
1	ООО Рога и копыта	Зарплата	40702810123456789012
2	ИП Сидоров	Аванс	40817810098765432109
3	Банк ВТБ	Проценты по вкладу	30101810700000000123
4	Фриланс биржа	Оплата за проект	42307810444455556666
5	Государство	Пенсия	40101810500000000111
\.


--
-- Data for Name: transactions; Type: TABLE DATA; Schema: Practice; Owner: postgres
--

COPY "Practice".transactions (id, sender_id, user_id2, category_id, recipient_id, user_id1, amount, transaction_date, created_at) FROM stdin;
1	1	\N	10	\N	1	50000.00	2024-01-05 09:00:00	2025-11-14 13:56:23.403596
2	2	\N	10	\N	2	35000.00	2024-01-05 10:00:00	2025-11-14 13:56:23.403596
3	3	\N	10	\N	3	42000.00	2024-01-05 11:00:00	2025-11-14 13:56:23.403596
4	\N	\N	1	1	1	-2964.58	2024-01-15 10:30:00	2025-11-14 13:56:23.403596
5	\N	\N	6	8	1	-450.00	2024-01-15 11:00:00	2025-11-14 13:56:23.403596
6	\N	\N	2	2	1	-2238.00	2024-01-16 14:20:00	2025-11-14 13:56:23.403596
7	\N	\N	3	3	1	-159.20	2024-01-17 09:45:00	2025-11-14 13:56:23.403596
8	\N	\N	4	4	1	-450.00	2024-01-18 16:10:00	2025-11-14 13:56:23.403596
9	\N	2	10	\N	1	-1000.00	2024-01-20 12:00:00	2025-11-14 13:56:23.403596
10	\N	3	10	\N	2	-500.00	2024-01-21 14:00:00	2025-11-14 13:56:23.403596
11	\N	\N	7	\N	2	-1200.00	2024-01-21 19:30:00	2025-11-14 13:56:23.403596
12	\N	\N	8	\N	4	-150.00	2024-01-10 08:15:00	2025-11-14 13:56:23.403596
13	\N	\N	6	\N	5	-2500.00	2024-01-12 14:30:00	2025-11-14 13:56:23.403596
14	4	\N	10	\N	4	28000.00	2024-01-05 12:00:00	2025-11-14 13:56:23.403596
15	5	\N	10	\N	5	31000.00	2024-01-05 13:00:00	2025-11-14 13:56:23.403596
16	\N	\N	3	3	2	-2750.50	2024-01-06 10:15:00	2025-11-14 13:56:23.403596
17	\N	\N	4	4	3	-1200.00	2024-01-07 11:20:00	2025-11-14 13:56:23.403596
18	\N	\N	9	5	4	-850.00	2024-01-08 14:30:00	2025-11-14 13:56:23.403596
19	\N	\N	7	6	5	-3200.00	2024-01-09 16:45:00	2025-11-14 13:56:23.403596
20	\N	\N	8	7	6	-550.00	2024-01-10 09:10:00	2025-11-14 13:56:23.403596
21	\N	\N	6	8	7	-1800.00	2024-01-11 17:25:00	2025-11-14 13:56:23.403596
22	\N	4	10	\N	2	-1500.00	2024-01-12 13:40:00	2025-11-14 13:56:23.403596
23	\N	5	10	\N	3	-2000.00	2024-01-13 15:55:00	2025-11-14 13:56:23.403596
24	1	\N	10	\N	1	52000.00	2024-02-05 09:30:00	2025-11-14 13:56:23.403596
25	2	\N	10	\N	2	36000.00	2024-02-05 10:45:00	2025-11-14 13:56:23.403596
26	\N	\N	4	15	1	-800.00	2024-02-06 11:20:00	2025-11-14 13:56:23.403596
27	\N	\N	6	16	2	-1200.00	2024-02-07 14:35:00	2025-11-14 13:56:23.403596
28	\N	\N	7	17	3	-2500.00	2024-02-08 16:50:00	2025-11-14 13:56:23.403596
29	\N	\N	6	18	4	-900.00	2024-02-09 18:15:00	2025-11-14 13:56:23.403596
30	\N	\N	8	19	5	-650.00	2024-02-10 10:40:00	2025-11-14 13:56:23.403596
31	\N	\N	3	20	6	-4850.00	2024-02-11 12:55:00	2025-11-14 13:56:23.403596
32	\N	\N	7	21	7	-3100.00	2024-02-12 15:10:00	2025-11-14 13:56:23.403596
33	\N	3	10	\N	1	-3000.00	2024-02-13 17:25:00	2025-11-14 13:56:23.403596
34	3	\N	10	\N	3	44000.00	2024-03-05 08:20:00	2025-11-14 13:56:23.403596
35	4	\N	10	\N	4	30000.00	2024-03-05 09:35:00	2025-11-14 13:56:23.403596
36	\N	\N	6	22	1	-750.00	2024-03-06 11:50:00	2025-11-14 13:56:23.403596
37	\N	\N	5	23	2	-1500.00	2024-03-07 13:05:00	2025-11-14 13:56:23.403596
38	\N	\N	2	24	3	-2800.00	2024-03-08 15:20:00	2025-11-14 13:56:23.403596
39	\N	\N	4	15	4	-950.00	2024-03-09 17:35:00	2025-11-14 13:56:23.403596
40	\N	\N	6	16	5	-1100.00	2024-03-10 19:50:00	2025-11-14 13:56:23.403596
41	\N	\N	7	17	6	-2700.00	2024-03-11 10:15:00	2025-11-14 13:56:23.403596
42	\N	\N	6	18	7	-850.00	2024-03-12 12:30:00	2025-11-14 13:56:23.403596
43	\N	6	10	\N	2	-1800.00	2024-03-13 14:45:00	2025-11-14 13:56:23.403596
44	\N	\N	1	1	8	-3100.00	2024-01-14 08:00:00	2025-11-14 13:56:23.403596
45	\N	\N	2	2	9	-1950.00	2024-01-15 10:30:00	2025-11-14 13:56:23.403596
46	\N	\N	3	3	10	-820.00	2024-01-16 13:45:00	2025-11-14 13:56:23.403596
47	\N	\N	4	4	11	-1300.00	2024-01-17 16:20:00	2025-11-14 13:56:23.403596
48	\N	\N	9	5	12	-720.00	2024-01-18 18:55:00	2025-11-14 13:56:23.403596
49	\N	\N	7	6	13	-2900.00	2024-01-19 11:10:00	2025-11-14 13:56:23.403596
50	\N	\N	8	7	14	-480.00	2024-01-20 14:25:00	2025-11-14 13:56:23.403596
51	\N	\N	6	8	15	-1650.00	2024-01-21 17:40:00	2025-11-14 13:56:23.403596
52	\N	9	10	\N	8	-2200.00	2024-01-22 19:15:00	2025-11-14 13:56:23.403596
53	\N	11	10	\N	10	-1700.00	2024-01-23 12:30:00	2025-11-14 13:56:23.403596
54	\N	13	10	\N	12	-2500.00	2024-02-14 15:45:00	2025-11-14 13:56:23.403596
55	\N	15	10	\N	14	-1900.00	2024-02-15 18:20:00	2025-11-14 13:56:23.403596
56	\N	17	10	\N	16	-2100.00	2024-02-16 10:35:00	2025-11-14 13:56:23.403596
57	\N	19	10	\N	18	-2800.00	2024-02-17 13:50:00	2025-11-14 13:56:23.403596
58	\N	21	10	\N	20	-1600.00	2024-02-18 16:05:00	2025-11-14 13:56:23.403596
59	\N	\N	8	19	8	-720.00	2024-03-14 09:20:00	2025-11-14 13:56:23.403596
60	\N	\N	3	20	9	-3850.00	2024-03-15 11:45:00	2025-11-14 13:56:23.403596
61	\N	\N	7	21	10	-2450.00	2024-03-16 14:10:00	2025-11-14 13:56:23.403596
62	\N	\N	6	22	11	-980.00	2024-03-17 16:35:00	2025-11-14 13:56:23.403596
63	\N	\N	5	23	12	-1350.00	2024-03-18 19:00:00	2025-11-14 13:56:23.403596
64	\N	\N	2	24	13	-2650.00	2024-03-19 10:25:00	2025-11-14 13:56:23.403596
65	\N	\N	4	15	14	-1100.00	2024-03-20 12:50:00	2025-11-14 13:56:23.403596
66	\N	\N	6	16	15	-1250.00	2024-03-21 15:15:00	2025-11-14 13:56:23.403596
67	\N	\N	7	17	16	-2950.00	2024-03-22 17:40:00	2025-11-14 13:56:23.403596
68	\N	\N	6	18	17	-920.00	2024-03-23 20:05:00	2025-11-14 13:56:23.403596
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: Practice; Owner: postgres
--

COPY "Practice".users (id, lastname, firstname, patronymic, login, password, balance, role, created_at) FROM stdin;
1	Бойко	Игорь	Петрович	boico	aut_aliasg	50000.00	2	2025-11-14 13:56:18.694432
2	Василенко	Василий	Александрович	vasilenco	qwerty	35000.00	2	2025-11-14 13:56:18.694432
3	Контеенко	Дмитрий	Семенович	konteenco	placeat1972l	42000.00	2	2025-11-14 13:56:18.694432
4	Лазарьков	Петр	Михайлович	lazarkov	equam442	28000.00	2	2025-11-14 13:56:18.694432
5	Кузнецов	Василий	Семенович	kuznetsov	libero%88f	31000.00	2	2025-11-14 13:56:18.694432
6	Дорофеева	Анна	Геннадьевна	test	12345	40000.00	2	2025-11-14 13:56:18.694432
7	Прокопьева	Елена	Петровна	ann	porro_autu	45000.00	2	2025-11-14 13:56:18.694432
8	Смирнов	Алексей	Владимирович	smirnov	pass123	38000.00	2	2025-11-14 13:56:18.694432
9	Ковалева	Ольга	Игоревна	kovaleva	olga2024	42000.00	2	2025-11-14 13:56:18.694432
10	Никитин	Сергей	Петрович	nikitin	serg_pass	29000.00	2	2025-11-14 13:56:18.694432
11	Орлова	Мария	Сергеевна	orlova	maria88	51000.00	2	2025-11-14 13:56:18.694432
12	Громов	Денис	Александрович	gromov	denis_g	33000.00	2	2025-11-14 13:56:18.694432
13	Васнецова	Екатерина	Дмитриевна	vasnecova	katya_pass	47000.00	2	2025-11-14 13:56:18.694432
14	Белов	Андрей	Викторович	belov	andrey_b	39000.00	2	2025-11-14 13:56:18.694432
15	Соколова	Ирина	Олеговна	sokolova	irina_s	44000.00	2	2025-11-14 13:56:18.694432
16	Морозов	Виталий	Сергеевич	morozov	vitaliy_m	36000.00	2	2025-11-14 13:56:18.694432
17	Зайцева	Наталья	Владимировна	zayceva	nata_z	41000.00	2	2025-11-14 13:56:18.694432
18	Павлов	Роман	Иванович	pavlov	roman_p	32000.00	2	2025-11-14 13:56:18.694432
19	Козлова	Светлана	Анатольевна	kozlova	sveta_k	48000.00	2	2025-11-14 13:56:18.694432
20	Лебедев	Максим	Петрович	lebedev	max_l	37000.00	2	2025-11-14 13:56:18.694432
21	Новикова	Ангелина	Романовна	novikova	angel_n	43000.00	2	2025-11-14 13:56:18.694432
\.


--
-- Data for Name: article; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".article (id_article, number_of_article, description) FROM stdin;
1	20.10	Мелкое хулиганство
2	12.80	Управление транспортным средством в состоянии опьянения
3	6.90	Потребление наркотических средств без назначения врача
4	19.30	Неповиновение законному распоряжению сотрудника полиции
5	20.20	Распитие алкогольной продукции в запрещенных местах
\.


--
-- Data for Name: articles_and_responsibility; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".articles_and_responsibility (id_articles_and_responsibility, responsibility, article) FROM stdin;
1	1	1
2	1	2
3	2	3
4	1	4
5	1	5
\.


--
-- Data for Name: citizens; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".citizens (id_citizen, last_name, first_name, patronymic, birthday, settlement_citizen, place_registration, work_place, post, salary, criminal_record, count_record, family_status, passport) FROM stdin;
1	Иванов	Иван	Иванович	1985-05-15	1	ул. Ленина, д. 10, кв. 25	1	1	50000	f	0	2	1234567890
2	Петров	Петр	Петрович	1990-08-20	1	пр. Мира, д. 5, кв. 12	2	4	45000	f	0	1	2345678901
3	Сидоров	Алексей	Сергеевич	1988-03-10	2	ул. Садовая, д. 15, кв. 8	5	1	48000	t	2	3	3456789012
4	Кузнецов	Дмитрий	Александрович	1975-12-05	1	ул. Центральная, д. 20, кв. 30	3	2	75000	f	0	2	4567890123
5	Смирнова	Ольга	Владимировна	1992-07-18	1	ул. Молодежная, д. 7, кв. 15	4	3	42000	f	0	5	5678901234
6	Васильев	Сергей	Николаевич	1980-11-25	1	ул. Школьная, д. 3, кв. 10	\N	\N	0	t	1	3	6789012345
7	Николаев	Андрей	Павлович	1987-09-14	2	пр. Победы, д. 12, кв. 22	\N	\N	0	f	0	1	7890123456
8	Орлова	Елена	Дмитриевна	1995-04-30	1	ул. Лесная, д. 8, кв. 5	2	4	47000	f	0	1	8901234567
9	Подростков	Иван	Сергеевич	2010-03-15	1	ул. Школьная, д. 5, кв. 3	\N	\N	0	f	0	1	9012345678
10	Взрослов	Петр	Иванович	2005-08-20	1	ул. Взрослая, д. 12, кв. 7	\N	\N	0	f	0	1	9123456789
\.


--
-- Data for Name: citizens_and_posts; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".citizens_and_posts (id_citizens_and_posts, citizen, post) FROM stdin;
1	1	1
2	2	4
3	3	1
4	4	2
5	5	3
6	8	4
\.


--
-- Data for Name: family_status; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".family_status (id_family_status, family_status) FROM stdin;
1	Холост
2	Женат
3	Разведен
4	Вдовец
5	Гражданский-брак
\.


--
-- Data for Name: medical_examination_report; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".medical_examination_report (id_medical_examination_report, report, number_of_report, settlements_report, police_officers_in_report, patient, date_of_making, time_of_making, hospital_staff, sign_of_intoxication, access_for_report, first_witness, second_witness) FROM stdin;
1	1	2001	1	1	6	2024-01-15	15:00:00	2	Легкая степень алкогольного опьянения	t	7	\N
2	1	2002	1	1	3	2024-01-20	22:45:00	2	Средняя степень алкогольного опьянения	t	7	8
\.


--
-- Data for Name: post; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".post (id_post, post_name) FROM stdin;
1	Сотрудник-полиции
2	Судья
3	Сотрудник-КДМ
4	Врач
5	Свидетель
6	Инспектор-ГИБДД
\.


--
-- Data for Name: protocol; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".protocol (id_protocol, name_of_protocol, date_of_making_protocol, time_of_making_protocol, settlement_of_making, police_officers_in_protocol, offender, description, disputes, article_of_protocol, first_witness, second_witness) FROM stdin;
1	1001	2024-01-15	14:30:00	1	1	6	Гражданин Васильев С.Н. находился в общественном месте в состоянии алкогольного опьянения и нарушал общественный порядок	f	5	7	\N
2	1002	2024-01-20	22:15:00	1	1	3	Гражданин Сидоров А.С. управлял автомобилем в состоянии алкогольного опьянения	t	2	7	8
4	1004	2024-01-26	17:30:00	1	1	10	Успешное создание протокола на совершеннолетнего нарушителя	f	1	7	8
\.


--
-- Data for Name: resolution; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".resolution (id_resolution, number_of_protocol, settlements_resolution, court_staff, kdm_employee, resolution, punishment, sum_of_fine, days_of_arrest, days_of_forced_labor, id_article, id_responsibility) FROM stdin;
1	1	1	4	5	Признать гражданина Васильева С.Н. виновным по ст. 20.20 КоАП РФ	1	1500	\N	\N	5	1
2	2	1	4	5	Признать гражданина Сидорова А.С. виновным по ст. 12.8 КоАП РФ	1	30000	15	\N	2	1
\.


--
-- Data for Name: responsibility; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".responsibility (id_responsibility, type_of_responsibility) FROM stdin;
1	Административная
2	Уголовная
3	Дисциплинарная
4	Гражданско-правовая
\.


--
-- Data for Name: settlements; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".settlements (id_settlement, title_of_settlement) FROM stdin;
1	Москва
2	Санкт-Петербург
3	Новосибирск
4	Екатеринбург
5	Казань
6	Нижний-Новгород
7	Красноярск
8	Владивосток
\.


--
-- Data for Name: structures; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".structures (id_structure, name_structure, settlement_structures, description_structure) FROM stdin;
1	ОМВД-Центрального-округа	1	Отдел внутренних дел Центрального административного округа
2	Городская-больница-1	1	Городская клиническая больница №1
3	Районный-суд	1	Районный суд Центрального округа
4	КДМ-Москвы	1	Комиссия по делам несовершеннолетних
5	ОМВД-Петроградского-района	2	Отдел внутренних дел Петроградского района
\.


--
-- Data for Name: type_of_face; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".type_of_face (id_type_of_face, type_of_face) FROM stdin;
1	Физическое
2	Юридическое
3	Должностное
\.


--
-- Data for Name: type_of_punishment; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".type_of_punishment (id_type_of_punishment, type_of_punishment) FROM stdin;
1	Штраф
2	Арест
3	Обязательные-работы
4	Исправительные-работы
5	Лишение-прав
\.


--
-- Data for Name: type_of_report; Type: TABLE DATA; Schema: Practice 11/30/2025; Owner: postgres
--

COPY "Practice 11/30/2025".type_of_report (id_type_of_report, type_of_report) FROM stdin;
1	Медицинское-освидетельствование
2	Токсикологическая-экспертиза
3	Психиатрическая-экспертиза
\.


--
-- Data for Name: authors; Type: TABLE DATA; Schema: Study; Owner: postgres
--

COPY "Study".authors (id, name_author) FROM stdin;
\.


--
-- Data for Name: circles; Type: TABLE DATA; Schema: bilet1; Owner: postgres
--

COPY bilet1.circles (circle_id, circle_name, education_level) FROM stdin;
1	Юный художник	дошкольник
2	Робототехника	начальная школа
3	Английский язык	средняя
4	Программирование	старшая
5	Спортивные танцы	начальная школа
\.


--
-- Data for Name: leaders; Type: TABLE DATA; Schema: bilet1; Owner: postgres
--

COPY bilet1.leaders (leader_id, full_name, circle_id) FROM stdin;
1	Иванова Мария Петровна	1
2	Петров Сергей Иванович	2
3	Сидорова Анна Владимировна	3
4	Козлов Дмитрий Николаевич	4
5	Смирнова Елена Александровна	5
\.


--
-- Data for Name: visits; Type: TABLE DATA; Schema: bilet1; Owner: postgres
--

COPY bilet1.visits (visit_id, leader_id, visit_date, children_count) FROM stdin;
1	1	2025-05-01	8
2	2	2025-05-02	12
3	3	2025-05-03	10
4	4	2025-05-04	6
5	5	2025-05-05	15
6	1	2025-05-17	20
\.


--
-- Data for Name: administrative_protocol; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.administrative_protocol (id_protocol, protocol_number, making_date_and_time, deal, description, other_information, signature_for_knowing_everithing, first_witness, second_witness) FROM stdin;
9	5002	2024-03-10 10:00:00	8	Превышение скорости на 40 км/ч	Зафиксировано камерой	\N	1	2
8	5001	2024-01-15 11:00:00	6	Управление ТС в состоянии опьянения	Отбор пробы	\N	1	16
12	3213	2026-06-05 13:39:23.599985	8	Административный протокол	Отсутствует	\N	15	\N
13	3213	2026-06-05 13:39:41.874134	8	Административный протокол	Отсутствует	\N	15	\N
14	3213	2026-06-05 13:40:09.191919	8	Административный протокол	Отсутствует	\N	15	\N
15	12345	2026-06-05 13:41:36.023945	8	Превышение установленной скорости движения	Отсутствует	\N	15	\N
16	12345	2026-06-05 13:42:29.691308	8	Превышение скорости	Нет	\N	15	\N
17	345345	2026-06-14 07:55:57.03865	10	dssfdsfdfdf	dfdfdfdfdf	\N	16	\N
\.


--
-- Data for Name: albums; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.albums (album_id, album_name, artist_id) FROM stdin;
1	Very Ape (Instrumental).mp3	1
2	01. Adrenaline	2
3	02. Around the Fur	2
4	03. White Pony	2
5	04. Deftones	2
6	05. B-Sides & Rarities	2
7	06. Saturday Night Wrist	2
8	07. Diamond Eyes	2
9	08. Koi No Yokan	2
10	09. Gore	2
11	10. Ohms	2
12	11. private music	2
13	01. Foo Fighters	3
14	02. The Colour and the Shape	3
15	03. There Is Nothing Left to Lose	3
16	04. One By One	3
17	05. In Your Honor	3
18	06. Echoes, Silence, Patience & Grace	3
19	07. Wasting Light	3
20	08. Sonic Highways	3
21	09. Concrete and Gold	3
22	10. But Here We Are	3
23	It Feels Like I'm Wilting Away	4
24	Safe Indoors	4
25	watch me disappear	4
26	Meteora	5
27	01. Bleach	6
28	02. Nevermind	6
29	03. In Utero	6
30	04. Incesticide	6
31	05. MTV Unplugged in New York	6
32	01. Fearless	7
33	02. Red	7
34	03. 1989	7
35	04. reputation	7
36	05. Lover	7
37	06. folklore	7
38	07. evermore	7
39	08. The Tortured Poets Department	7
40	01. Please Please Me	8
41	02. With the Beatles	8
42	03. A Hard Day's Night	8
43	04. Beatles for Sale	8
44	05. Help!	8
45	07. Revolver	8
46	08. Sgt. Pepper's Lonely Hearts Club Band	8
47	09. White Album	8
48	10. Yellow Submarine	8
49	11. Abbey Road	8
50	12. Let It Be	8
51	Piano	9
\.


--
-- Data for Name: appeals; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.appeals (id_appeals, number, appeal_citizen, police_officer, content, making_date_and_time) FROM stdin;
1	4001	15	1	Жалоба на действия сотрудников ДПС	2024-02-21 09:00:00
2	4002	16	1	Вопрос о возврате изъятого имущества	2024-04-06 14:30:00
15	2132	16	1	вапвапапап	2026-05-20 08:46:00
27	34534	15	1	ghgfhgfhh	2026-05-31 08:46:00
28	120626	16	15	Кто-то кричит на улице поздно ночью	2026-06-12 07:02:00
29	120626	16	15	вапвапвапап	2026-06-12 07:03:00
30	120626	16	15	вапапап	2026-06-12 17:23:00
31	120626	16	15	ААААААААААААААААА	2026-06-12 17:24:00
32	23234234	29	17	ваывпвпвапапп	2026-06-13 10:28:00
33	23564	29	17	вапавпвапапап	2026-06-13 11:07:00
34	130626	29	15	Проверка	2026-06-13 18:46:00
35	130626	27	15	111111111111111111	2026-06-13 18:56:00
\.


--
-- Data for Name: article; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.article (id_article, number_of_article, description) FROM stdin;
1	1.10	Нарушение правил дорожного движения пешеходом
2	2.50	Невыполнение требований о прохождении медицинского освидетельствования
3	3.10	Управление транспортным средством в состоянии опьянения
4	4.20	Превышение установленной скорости движения
5	5.10	Нарушение правил проживания иностранных граждан
6	6.90	Потребление наркотических средств без назначения врача
7	7.10	Мелкое хулиганство
8	8.30	Нарушение тишины и покоя граждан
9	12.80	Управление ТС водителем в состоянии опьянения
10	19.30	Неповиновение законному распоряжению сотрудника полиции
11	20.10	Мелкое хулиганство
12	20.20	Потребление алкоголя в общественных местах
13	20.21	Появление в общественных местах в состоянии опьянения
14	22.50	Нарушение миграционного законодательства
\.


--
-- Data for Name: articles_and_responsobility; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.articles_and_responsobility (id_articles_and_responsibility, responsibility, article) FROM stdin;
1	1	1
2	1	2
3	1	3
4	1	4
5	1	5
6	1	6
7	1	7
8	1	8
9	1	9
10	1	10
11	1	11
12	1	12
13	1	13
14	1	14
15	2	3
16	2	6
17	2	9
\.


--
-- Data for Name: artists; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.artists (artist_id, artist_name) FROM stdin;
1	Instrumentals
2	Deftones
3	Foo Fighters
4	grayera
5	Linkin Park
6	Nirvana
7	Taylor Swift
8	The Beatles
9	Classic
\.


--
-- Data for Name: cap_ranks; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cap_ranks (id, user_citizen_link, rank) FROM stdin;
1	1	8
2	8	9
3	9	13
\.


--
-- Data for Name: citizen_phones; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.citizen_phones (id, phone_number, citizen, is_primary) FROM stdin;
42	+7-900-222-33-44	2	t
43	+7-900-333-44-55	3	t
44	+7-900-444-55-66	4	t
45	+7-900-555-66-77	5	t
46	+7-900-666-77-88	6	t
47	+7-900-777-88-99	7	t
48	+7-900-888-99-00	8	t
49	+7-900-999-00-11	9	t
50	+7-900-000-11-22	10	t
41	+7-900-111-22-33	15	t
51	+7-900-111-22-33	15	t
52	+7-900-111-22-33	15	t
53	+7-900-111-22-33	16	t
\.


--
-- Data for Name: citizens; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.citizens (id_citizens, last_name, first_name, patronymic, birthday, address_registration, working_place, post, criminal_record, count_record, passport_series_and_number, family_status, education, citizenship) FROM stdin;
16	Петров	Петр	Петрович	1990-07-22	г. Москва, ул. Пушкина, д. 25, кв. 8	1	1	f	0	4510 654321	1	1	1
17	Сидор	Сидоров	Сидорович	1990-12-10	Москва, ул. Пушкина 1, кв.1	1	1	f	0	4510 123457	1	1	1
21	Начальников	Начальник	Начальникович	1970-01-01	г. Москва, ул. Тверская, д. 1	1	1	f	0	0000 000001	1	1	1
27	Иванов	Иван	Иванович	1990-01-01	г. Москва, ул. Ленина, д. 1	1	1	f	0	4512 345678	1	1	1
28	Иванов	Иван	Иванович	1990-01-01	г. Москва, ул. Ленина, д. 1	1	1	f	0	4512 345678	1	1	1
29	Брэд	Питт	\N	1966-06-13	г. Москва, ул. Ленина, д. 1	1	1	t	2	9560 876290	1	1	1
32	Инспекторов	Инспектор	Инспекторович	1980-01-01	г. Москва, ул. Инспекторская, д. 1	1	2	f	0	1234 567890	1	1	1
15	Иванов	Иван	Иванович	1985-03-15	г. Москва, ул. Ленина, д. 10, кв. 5	1	1	f	4	4510 123456	1	1	1
26	Иванов	Иван	Иванович	1990-01-01	г. Москва, ул. Ленина, д. 1	1	1	f	4	4512 345678	1	1	1
\.


--
-- Data for Name: citizens_and_posts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.citizens_and_posts (id_citizens_and_posts, citizen, citizen_post) FROM stdin;
2	2	3
3	3	2
4	4	14
5	5	12
6	6	6
7	7	1
8	8	15
9	9	8
10	10	4
11	11	2
12	12	2
1	15	1
50	17	8
15	21	4
16	100	2
17	32	2
\.


--
-- Data for Name: citizenship; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.citizenship (id_citizenship, citizenship) FROM stdin;
1	Российская Федерация
2	Республика Беларусь
3	Республика Казахстан
4	Украина
5	Узбекистан
\.


--
-- Data for Name: deal; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.deal (id_deal, deal_number, settlement, offender, first_witness, second_witness, police_officer, article, responsibility, making_date) FROM stdin;
9	2002	1	16	1	2	1	1	1	2026-06-08 19:59:58.04495
8	2001	1	16	1	2	1	4	1	2026-06-08 19:59:58.04495
15	123213213	1	26	29	16	17	1	1	2026-06-13 16:40:14.37982
6	1001	1	15	1	2	1	2	1	2026-06-08 19:59:58.04495
10	140626	1	26	29	16	15	1	1	2026-06-13 16:40:14.37982
7	1002	1	15	1	2	1	3	1	2026-06-08 19:59:58.04495
\.


--
-- Data for Name: document_access_requests; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.document_access_requests (id, user_id, table_name, document_id, reason, request_date, status) FROM stdin;
2	1	medical_examination_report	18	вап	2026-05-09 06:14:47.879856	pending
3	1	medical_examination_report	18	dfg	2026-05-09 06:18:08.968844	pending
4	1	medical_examination_report	18	dfgdfg	2026-05-09 06:19:36.031119	pending
5	1	medical_examination_report	18	dfgfg	2026-05-09 06:19:56.380382	pending
6	1	explanation_protocol	8	dfgf	2026-05-09 19:38:15.66728	pending
7	1	explanation_protocol	8	dfgfg	2026-05-09 19:41:31.523172	pending
8	1	administrative_protocol	9	dfg	2026-05-09 19:41:36.0882	pending
12	1	explanation_protocol	8	вапап	2026-05-10 16:19:30.327078	pending
13	1	explanation_protocol	8	вап	2026-05-11 13:19:03.756021	pending
14	1	explanation_protocol	8	лох	2026-05-12 15:48:09.395264	pending
15	1	explanation_protocol	8	23213	2026-05-12 16:28:56.203373	pending
16	1	explanation_protocol	8	ло	2026-05-20 18:06:23.738455	pending
17	1	explanation_protocol	8	dfg	2026-05-20 18:11:39.287583	pending
18	1	explanation_protocol	8	fdg	2026-05-20 18:13:14.233701	pending
19	1	explanation_protocol	8	f	2026-05-20 18:15:02.610977	pending
20	1	explanation_protocol	8	dfg	2026-05-20 18:16:26.140446	pending
21	2	explanation_protocol	8	fdgfg	2026-05-29 15:48:11.765018	pending
22	2	explanation_protocol	7	gf	2026-05-29 15:48:15.81918	pending
23	2	administrative_protocol	8	fgfg	2026-05-29 15:48:19.845446	pending
24	2	medical_certificate	9	вап	2026-05-31 15:03:21.395923	pending
25	2	medical_certificate	10	gfdg	2026-05-31 15:06:23.766916	pending
26	2	medical_examination_report	18	пвапвап	2026-06-03 06:49:03.058517	pending
\.


--
-- Data for Name: documents_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.documents_type (id, document_type) FROM stdin;
1	Заявление
2	Обращение
3	Протокол объяснения
4	Направление на мед. освид.
5	Административный протокол
\.


--
-- Data for Name: drafts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.drafts (id_draft, user_id, document_type, form_data, created_at, updated_at) FROM stdin;
70	4	forensic_expertise	{"number": null, "content": "dfgdfg", "deal_id": null, "severity": true, "deal_name": null, "signature": true, "conclusion": "dfgfg", "could_occur": true, "making_date": "2026-06-08", "making_time": "06:56:00", "physical_injuries": true}	2026-06-08 06:56:39.554849	2026-06-08 06:56:39.554849
59	1	statement	{"number": null, "content": "dfgfg", "applicant": null, "date_and_time": "2026-05-31 13:12", "applicant_name": null, "signature_officer": false, "signature_applicant": false}	2026-05-31 13:12:07.814397	2026-05-31 13:12:08.4774
60	1	statement	{"number": null, "content": "234234", "applicant": null, "date_and_time": "2026-05-31 13:15", "applicant_name": null, "signature_officer": false, "signature_applicant": false}	2026-05-31 13:15:17.925782	2026-05-31 13:15:17.925782
61	1	explanation_protocol	{"deal": null, "number": null, "citizen": null, "content": "345", "deal_name": null, "making_date": "2026-05-31", "making_time": "13:15:00", "citizen_name": null, "citizen_signature": false, "officer_signature": false, "need_medical_certificate": false, "need_forensic_examination": false}	2026-05-31 13:15:26.571324	2026-05-31 13:15:26.571324
62	1	statement	{"number": null, "content": "435", "applicant": null, "date_and_time": "2026-05-31 13:17", "applicant_name": null, "signature_officer": false, "signature_applicant": false}	2026-05-31 13:17:29.412865	2026-05-31 13:17:29.412865
63	1	explanation_protocol	{"deal": null, "number": "34543", "citizen": null, "content": "gfhgh", "deal_name": null, "making_date": "2026-05-31", "making_time": "13:17:00", "citizen_name": null, "citizen_signature": false, "officer_signature": false, "need_medical_certificate": false, "need_forensic_examination": false}	2026-05-31 13:17:34.306746	2026-05-31 13:17:34.306746
64	1	medical_examination_report	{"signs": "", "number": "345", "content": "fghgh", "deal_id": null, "deal_name": null, "patient_id": null, "making_date": "2026-05-31", "making_time": "13:17:00", "report_type": "Судебно-психиатрическая экспертиза", "patient_name": null, "citizen_signature": false, "officer_signature": false}	2026-05-31 13:17:38.960939	2026-05-31 13:17:38.960939
65	1	administrative_protocol	{"deal": null, "witness1": null, "witness2": null, "deal_name": null, "signature": false, "description": "fghgh", "making_date": "2026-05-31", "making_time": "13:17:00", "witness1_name": null, "witness2_name": null, "protocol_number": null, "other_information": null}	2026-05-31 13:17:42.63285	2026-05-31 13:17:42.63285
68	4	forensic_expertise	{"number": null, "content": null, "deal_id": null, "severity": false, "deal_name": null, "signature": false, "conclusion": null, "could_occur": false, "making_date": "2026-06-07", "making_time": "16:06:00", "physical_injuries": false}	2026-06-07 16:06:48.613607	2026-06-07 16:06:48.613607
69	3	resolution	{"days": null, "fine": null, "number": null, "deal_id": null, "deal_name": null, "signature": false, "punishment": "Штраф", "resolution": null, "making_date": "2026-06-07", "making_time": "16:18:00", "forced_labor": null}	2026-06-07 16:18:09.550762	2026-06-07 16:18:09.550762
\.


--
-- Data for Name: education; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.education (id_education, education) FROM stdin;
1	Среднее общее
2	Среднее профессиональное
3	Высшее - бакалавриат
4	Высшее - специалитет
5	Высшее - магистратура
6	Неполное высшее
\.


--
-- Data for Name: explanation_protocol; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.explanation_protocol (id_explanation_protocol, number, making_date_and_time, citizen, deal, signature_for_error_testimony, signature_for_knowing_everithing, content, need_forensic_medical_examination, need_medical_examination_certificate, citizen_signature, police_officer_signature) FROM stdin;
7	1001	2024-01-15 11:30:00	15	6	\N	\N	С нарушением согласен	f	f	\N	\N
8	2001	2024-03-10 10:30:00	16	8	\N	\N	Спешил на работу	f	f	\N	\N
\.


--
-- Data for Name: family_status; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.family_status (id_family_status, family_status) FROM stdin;
1	Холост/Не замужем
2	Женат/Замужем
3	Разведен/Разведена
4	Вдовец/Вдова
\.


--
-- Data for Name: forensic_medical_examination; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.forensic_medical_examination (id_forensic_medical_examination, number, making_date_and_time, structure, deal, expert, content, physical_injuries, severity_of_harm_to_health, could_injuries_have_occurred_on_time, signature_expert) FROM stdin;
9	234	2026-05-25 08:17:00	1	8	6	dfg	t	t	t	t
10	213	2026-05-25 08:21:00	1	8	6	dfg	t	t	t	t
11	21	2026-05-25 13:16:00	1	8	1	dfg	t	t	t	t
12	213	2026-05-25 13:17:00	1	8	1	aaaaaaaaaaaaaaaaaaaaaaaaa	t	t	t	t
13	21	2026-05-25 13:29:00	1	8	1	dfg	t	t	t	t
14	234	2026-05-25 13:38:00	1	8	1	dfgdf	t	t	t	t
1	2	2026-05-22 12:55:46.407928	1	6	2	Проведена судебно-медицинская экспертиза	t	f	t	t
2	2	2026-05-22 13:18:18.745711	1	6	4	Проведена судебно-медицинская экспертиза	t	f	t	t
5	2	2026-05-25 08:06:00	1	8	6	dsfg	t	t	t	t
\.


--
-- Data for Name: medical_examination_certificate; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.medical_examination_certificate (id_medical_examination_certificate, number, medical_examination_report, making_date_and_time, medical_institution, doctor, signs_of_intoxication, result, type_intoxication, doctor_signature) FROM stdin;
1	1001	17	2026-05-22 12:55:44.252001	1	2	Запах алкоголя изо рта, неустойчивость позы	Установлено опьянение	1	t
4	3544	18	2026-05-24 16:54:00	1	1	прпр	впвпа	1	t
5	234	18	2026-05-24 16:55:00	1	1	впап	апапап	1	t
6	54	18	2026-05-24 16:56:00	1	1	вап	вапвапа	1	t
7	2323	18	2026-05-24 17:32:00	1	1	авпап	авпапапа	1	t
8	123	18	2026-05-24 19:24:00	1	1	впапаппп	папапапап	1	t
9	3005	18	2026-05-30 19:19:00	1	1	нет	нет	1	t
10	50000	17	2026-05-31 10:31:59.973033	1	50	Запах алкоголя изо рта, неустойчивость позы	Установлено состояние алкогольного опьянения	1	t
11	2222	18	2026-06-07 15:38:00	1	1	авпва	пвапап	1	t
12	100626	17	2026-06-10 06:17:00	1	1	Никаких	Прошел	1	t
\.


--
-- Data for Name: medical_examination_report; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.medical_examination_report (id_medical_examination_report, number, deal, report, patient, making_date_and_time, signs_of_intoxication, content, officer_signature, citizen_signature) FROM stdin;
17	2001	6	1	15	2024-01-15 10:45:00	Запах алкоголя, неустойчивость позы	Направление на мед. освидетельствование	\N	\N
18	2002	9	1	16	2024-04-05 09:30:00	Без признаков опьянения	Направление на мед. освидетельствование	\N	\N
19	2132133	8	2	16	2026-06-14 07:47:00	Запах изо рта	тестик	t	t
20	2132133	8	2	16	2026-06-14 07:47:00	Запах изо рта	тестик	t	t
21	2132133	8	2	16	2026-06-14 07:47:00	Запах изо рта	тестик	t	t
22	2132133	8	2	16	2026-06-14 07:47:00	Запах изо рта	тестик	t	t
23	40001	10	1	16	2026-06-14 07:48:00	Нет	Нет	t	t
\.


--
-- Data for Name: post; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.post (id_post, post) FROM stdin;
1	Участковый уполномоченный
2	Следователь
3	Инспектор ДПС
4	Начальник отдела
5	Заместитель начальника
6	Дознаватель
7	Эксперт-криминалист
8	Врач
9	Фельдшер
10	Бухгалтер
11	Юрисконсульт
12	Инженер
13	Водитель
14	Безработный
15	Пенсионер
16	Студент
\.


--
-- Data for Name: rank; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.rank (id, rank) FROM stdin;
1	Рядовой
2	Младший сержант
3	Сержант
4	Старший сержант
5	Прапорщик
6	Старший прапорщик
7	Младший лейтенант
8	Лейтенант
9	Старший лейтенант
10	Капитан
11	Майор
12	Подполковник
13	Полковник
14	Генерал-майор
15	Генерал-лейтенант
16	Генерал-полковник
\.


--
-- Data for Name: resolution; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.resolution (id_resolution, protocol_number, making_date_and_time, court_staff, deal, resolution, punishment, fine_sum) FROM stdin;
1	3001	2026-05-22 12:56:58.955288	2	6	Признать виновным, назначить наказание в виде штрафа	1	5000
3	1111	2026-05-25 07:51:00	2	8	2321dfdsfs	1	23
5	1111	2026-05-25 13:38:00	2	8	dfgfdgg	1	23
7	1222	2026-06-07 16:16:00	2	8	12	1	12
\.


--
-- Data for Name: responsibility; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.responsibility (id_responsibility, type_of_responsibility) FROM stdin;
1	Административная
2	Уголовная
3	Гражданско-правовая
4	Дисциплинарная
\.


--
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.roles (id, role) FROM stdin;
1	Police officer
3	Doctor
\.


--
-- Data for Name: settlements; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.settlements (id_settlements, title_of_settlements) FROM stdin;
1	Москва
2	Санкт-Петербург
3	Екатеринбург
4	Новосибирск
5	Казань
6	Нижний Новгород
7	Челябинск
8	Самара
9	Омск
10	Ростов-на-Дону
\.


--
-- Data for Name: songs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.songs (song_id, song_name, album_id) FROM stdin;
1	Very Ape (Instrumental)	1
2	Bored	2
3	Minus Blindfold	2
4	One Weak	2
5	Nosebleed	2
6	Lifter	2
7	Root	2
8	7 Words	2
9	Birthmark	2
10	Engine No. 9	2
11	Fireal	2
12	Fist	2
13	My Own Summer (Shove It)	3
14	Lhabia	3
15	Mascara	3
16	Around the Fur	3
17	Rickets	3
18	Be Quiet and Drive (Far Away)	3
19	Lotion	3
20	Dai the Flu	3
21	Head Up	3
22	MX	3
23	Bong Hit	3
24	Damone	3
25	Back To School (Mini Magit)	4
26	Feiticeira	4
27	Digital Bath	4
28	Elite	4
29	Rx Queen	4
30	Street Carp	4
31	Teenager	4
32	Knife Prty	4
33	Korea	4
34	Passenger	4
35	Change (In the House of Flies)	4
36	Pink Maggit	4
37	The Boy's Republic	4
38	Hexagram	5
39	Needles and Pins	5
40	Minerva	5
41	Good Morning Beautiful	5
42	Deathblow	5
43	When Girls Telephone Boys	5
44	Battle-axe	5
45	Lucky You	5
46	Bloody Cape	5
47	Anniversary of an Uninteresting Event	5
48	Moana	5
49	Savory	6
50	Wax and Wane	6
51	Change (In The House Of Flies) (Acoustic)	6
52	Simple Man	6
53	Sinatra	6
54	No Ordinary Love (Ft. Jonah Matranga)	6
55	Teenager (Idiot Version) (feat. Michael Harris)	6
56	Crenshaw Punch or I'll Throw Rocks at You	6
57	Black Moon	6
58	If Only Tonight We Could Sleep	6
59	Please Please Please Let Me Get What I Want	6
60	Digital Bath (Acoustic)	6
61	The Chauffeur	6
62	Be Quiet and Drive (Far Away)(Acoustic)	6
63	Night Boat	6
64	Hole in the Earth	7
65	Rapture	7
66	Beware	7
68	Mein	7
69	U,U,D,D,L,R,L,R,A,B,Select,Start	7
70	Xerces	7
71	Rats!Rats!Rats!	7
72	Pink Cellphone	7
73	Combat	7
74	Kimdracula	7
75	Drive	7
76	Riviere	7
77	Diamond Eyes	8
78	Royal	8
79	CMND-CTRL	8
80	You've Seen the Butcher	8
81	Beauty School	8
82	Prince	8
83	Rocket Skates	8
84	Sextape	8
85	Risk	8
86	976-EVIL	8
87	This Place Is Death	8
88	Do You Believe	8
89	Ghosts	8
90	Caress	8
91	Swerve City	9
92	Romantic Dreams	9
93	Leathers	9
94	Poltergeist	9
95	Entombed	9
96	Graphic Nature	9
97	Tempest	9
98	Gauze	9
99	Rosemary	9
100	Goon Squad	9
101	What Happened to You	9
102	Players or Triangles	10
103	Acid Hologram	10
104	Doomed User	10
105	Geometric Headdress	10
106	Hearts or Wires	10
107	Pittura Infamante	10
108	Xenon	10
109	(L)MIRL	10
110	Gore	10
111	Phantom Bride	10
112	Rubicon	10
113	Genesis	11
114	Ceremony	11
115	Urantia	11
116	Error	11
117	The Spell of Mathematics	11
118	Pompeji	11
119	This Link Is Dead	11
120	Radiant City	11
121	Headless	11
122	Ohms	11
123	my mind is a mountain	12
124	locked club	12
125	ecdysis	12
126	infinite source	12
127	souvenir	12
128	cXz	12
129	i think about you all the time	12
130	milk of the madonna	12
131	cut hands	12
132	~metal dream	12
133	departing the body	12
134	This Is a Call	13
135	I'll Stick Around	13
136	Oh, George	13
137	Big Me	13
138	Alone + Easy Target	13
139	Good Grief	13
140	Floaty	13
141	Weenie Beenie	13
142	For All the Cows	13
143	X-Static	13
144	Wattershed	13
145	Exhausted	13
146	Doll	14
147	Monkey Wrench	14
148	Hey, Johnny Park!	14
149	My Poor Brain	14
150	Wind Up	14
151	Up In Arms	14
152	My Hero	14
153	See You	14
154	Enough Space	14
155	February Stars	14
156	Everlong	14
157	Walking After You	14
158	New Way Home	14
159	The Colour And The Shape	14
160	Stacked Actors	15
161	Breakout	15
162	Learn to Fly	15
163	Gimme Stitches	15
164	Generator	15
165	Aurora	15
166	Live-In Skin	15
167	Next Year	15
168	Headwires	15
169	Ain't It The Life	15
170	M.I.A	15
171	All My Life	16
172	Low	16
173	Have It All	16
174	Times Like These	16
175	Disenchanted Lullaby	16
176	Tired Of You	16
177	Halo	16
178	Lonely As You	16
179	Overdrive	16
180	Burn Away	16
181	Come Back	16
182	Walking A Line	16
183	Sister Europe	16
184	Danny Says	16
185	Life Of Illusion	16
186	For All The Cows (Live In Amsterdam)	16
187	Monkey Wrench	16
188	In Your Honor	17
189	Still	17
190	No Way Back	17
191	What If I Do	17
192	Best of You	17
193	Miracle	17
194	Another Round	17
195	DOA	17
196	Friend Of A Friend	17
197	Hell	17
198	Over And Out	17
199	The Last Song	17
200	Free Me	17
201	On The Mend	17
202	Resolve	17
203	Virginia Moon	17
204	Cold Day In The Sun	17
205	The Deepest Blues Are Black	17
206	End Over End	17
207	Razor	17
208	The Pretender	18
209	Let It Die	18
210	Replace	18
211	Long Road To Ruin	18
212	Come Alive	18
213	Stranger Things Have Happened	18
214	Cheer Up, Boys (Your Make Up Is Running)	18
215	Summer s End	18
216	Ballad Of The Beaconsfield Miners	18
217	Statues	18
218	But, Honestly	18
219	Home	18
220	Bridge Burning	19
221	Rope	19
222	Dear Rosemary	19
223	White Limo	19
224	Arlandria	19
225	These Days	19
226	Back Forth	19
227	A Matter Of Time	19
228	Miss The Misery	19
229	I Should Have Known	19
230	Walk	19
231	Something from Nothing	20
232	The Feast and The Famine	20
233	Congregation	20
234	God As My Witness	20
235	Outside	20
236	In The Clear	20
237	Subterranean	20
238	I Am A River	20
239	T-Shirt	21
240	Run	21
241	Make It Right	21
242	The Sky Is A Neighborhood	21
243	La Dee Da	21
244	Dirty Water	21
245	Arrows	21
246	Happy Ever After (Zero Hour)	21
247	Sunday Rain	21
248	The Line	21
249	Concrete and Gold	21
250	Rescued	22
251	Under You	22
252	Hearing Voices	22
253	But Here We Are	22
254	The Glass	22
255	Nothing At All	22
256	Show Me How	22
257	Beyond Me	22
258	The Teacher	22
259	Rest	22
260	Wilting	23
261	Reaching For Something	23
262	Suntouched Shillelagh	23
263	Coffin Dance	23
264	If Only Your Best Was Good Enough	23
265	Loch Lomand	23
266	Armet Of Steel	23
267	The Answer Been Better	23
268	Could Be Worse	23
269	Safe Indoors	24
270	No Such Thing As Permanent	24
271	Blankets Over Head	24
272	Hallowed Ground	24
273	Cabin Aflame	24
274	Anywhere You Go	24
275	Think I'll Just Sleep	24
276	I	25
277	II	25
278	III	25
279	IV	25
280	Foreword	26
281	Don't Stay	26
282	Somewhere I Belong	26
283	Lying from You	26
284	Hit the Floor	26
285	Easier to Run	26
286	Faint	26
287	Figure.09	26
288	Breaking the Habit	26
289	From the Inside	26
290	Nobody's Listening	26
291	Session	26
292	Numb	26
293	Blew	27
294	Floyd The Barber	27
295	About A Girl	27
296	School	27
297	Love Buzz	27
298	Paper Cuts	27
299	Negative Creep	27
300	Scoff	27
301	Swap Meet	27
302	Mr. Moustache	27
303	Sifting	27
304	Big Cheese	27
305	Downer	27
306	Smells Like Teen Spirit	28
307	In Bloom	28
308	Come As You Are	28
309	Breed	28
310	Lithium	28
311	Polly	28
312	Territorial Pissings	28
313	Drain You	28
314	Lounge Act	28
315	Stay Away	28
316	On A Plain	28
317	Something In The Way	28
318	Serve The Servants	29
319	Scentless Apprentice	29
320	Heart-Shaped Box	29
321	Rape Me	29
322	Frances Farmer Will Have Her Revenge On Seattle	29
323	Dumb	29
324	Very Ape	29
325	Milk It	29
326	Pennyroyal Tea	29
327	Radio Friendly Unit Shifter	29
328	Tourette's	29
329	All Apologies	29
330	Dive	30
331	Sliver	30
332	Stain	30
333	Been A Son	30
334	Turnaround	30
335	Molly's Lips	30
336	Son Of A Gun	30
337	(New Wawe) Polly	30
338	Beeswax	30
339	Downer	30
340	Mexican Seafood	30
341	Hairspray Queen	30
342	Aero Zeppelin	30
343	Big Long Now	30
344	Aneurysm	30
345	About A Girl	31
346	Come As You Are	31
347	Jesus Doesn't Want Me For A Sunbe	31
348	The Man Who Sold The World	31
349	Pennyroyal Tea	31
350	Dumb	31
351	Polly	31
352	On A Plain	31
353	Something In The Way	31
354	Plateau	31
355	Oh, Me	31
356	Lake Of Fire	31
357	All Apologies	31
358	Where Did You Sleep Last Night	31
359	Fearless	32
360	Fifteen	32
361	Love Story	32
362	Hey Stephen	32
363	White Horse	32
364	You Belong With Me	32
365	Breathe	32
366	Tell Me Why	32
367	You're Not Sorry	32
368	The Way I Loved You	32
369	Forever & Always	32
370	The Best Day	32
371	Change	32
372	Jump Then Fall	32
373	Untouchable	32
374	Forever & Always (Acoustic Version)	32
375	Come In With The Rain	32
376	Superstar	32
377	The Other Side Of The Door	32
378	Today Was A Fairytale	32
379	You All Over Me	32
380	Mr. Perfectly Fine	32
381	We Were Happy	32
382	That's When	32
383	Don't You	32
384	Bye Bye Baby	32
385	State Of Grace	33
386	Red	33
387	Treacherous	33
388	I Knew You Were Trouble	33
389	All Too Well	33
390	22	33
391	I Almost Do	33
392	We Are Never Ever Getting Back Together	33
393	Stay Stay Stay	33
394	The Last Time	33
395	Holy Ground	33
396	Sad Beautiful Tragic	33
397	The Lucky One	33
398	Everything Has Changed	33
399	Starlight	33
400	Begin Again	33
401	The Moment I Knew	33
402	Come Back...Be Here	33
403	Girl At Home	33
404	State Of Grace (Acoustic Version)	33
405	Ronan	33
406	Better Man	33
407	Nothing New	33
408	Babe	33
409	Message In A Bottle	33
410	I Bet You Think	33
411	Forever Winter	33
412	Run	33
413	The Very First Night	33
414	Style	33
415	All Too Well 10 Minute Version	33
416	Welcome To New York	34
417	Blank Space	34
418	Style	34
419	Out Of The Woods	34
420	All You Had To Do Was Stay	34
421	Shake It Off	34
422	I Wish You Would	34
423	Bad Blood	34
424	Wildest Dreams	34
425	How You Get The Girl	34
426	This Love	34
427	I Know Places	34
428	Clean	34
429	Wonderland	34
430	You Are In Love	34
431	New Romantics	34
432	...Ready For It	35
433	End Game	35
434	I Did Something Bad	35
435	Don't Blame Me	35
436	Delicate	35
437	Look What You Made Me Do	35
438	So It Goes	35
439	Gorgeous	35
440	Getaway Car	35
441	King Of My Heart	35
442	Dancing With Our Hands Tied	35
443	Dress	35
444	This Is Why We Can't Have Nice Things	35
445	Call It What You Want	35
446	New Year's Day	35
447	I Forgot That You Existed	36
448	Cruel Summer	36
449	Lover	36
450	The Man	36
451	The Archer	36
452	I Think He Knows	36
453	Miss Americana The Heartbreak Prince	36
454	Paper Rings	36
455	Cornelia Street	36
456	Death By A Thousand Cuts	36
457	London Boy	36
458	Soon You'll Get Better	36
459	False God	36
460	You Need To Calm Down	36
461	Afterglow	36
462	ME!	36
463	It's Nice To Have A Friend	36
464	Daylight	36
465	the 1	37
466	cardigan	37
467	the last great american dynasty	37
468	exile	37
469	my tears ricochet	37
470	mirrorball	37
471	seven	37
472	august	37
473	this is me trying	37
474	illicit affairs	37
475	invisible string	37
476	mad woman	37
477	epiphany	37
478	betty	37
479	peace	37
480	hoax	37
481	the lakes	37
482	willow	38
483	champagne problems	38
484	gold rush	38
485	'tis the damn season	38
486	tolerate it	38
487	no body no crime	38
488	happiness	38
489	dorothea	38
490	coney island	38
491	ivy	38
492	cowboy like me	38
493	long story short	38
494	marjorie	38
495	closure	38
496	evermore	38
497	right where you left me	38
498	it's time to go	38
499	Fortnight	39
500	The Tortured Poets Department	39
501	My Boy Only Breaks His Favorite Toy	39
502	Down Bad	39
503	So Long, London	39
504	But Daddy I Love Him	39
505	Fresh Out The Slammer	39
506	Florida!!	39
507	Guilty as Sin	39
508	Who's Afraid Of Little Old Me	39
509	I Can Fix Him (No Really I Can)	39
510	loml	39
511	I Can Do It With a Broken Heart	39
512	The Smallest Man Who Ever Lived	39
513	The Alchemy	39
514	Clara Bow	39
515	The Black Dog	39
516	imgonnagetyouback	39
517	The Albatross	39
518	Chloe or Sam or Sophia or Marcus	39
519	How Did It End	39
520	So High School	39
521	I Hate It Here	39
522	thanK you aIMee	39
523	I Look in People's Windows	39
524	The Prophecy	39
525	Cassandra	39
526	Peter	39
527	The Bolter	39
528	Robin	39
529	The Manuscript	39
530	Fortnight (Acoustic Version)	39
531	Down Bad (Acoustic Version)	39
532	But Daddy I Love Him (Acoustic Vers	39
533	Guilty As Sin (Acoustic Version)	39
534	I Saw Her Standing There	40
535	Misery	40
536	Anna Go To Him	40
537	Chains	40
538	Boys	40
539	Ask Me Why	40
540	Please Please Me	40
541	Love Me Do	40
542	P.S. I Love You	40
543	Baby it's you	40
544	Do you want to know a secret	40
545	A Taste Of Honey	40
546	There's A Place	40
547	Twist And Shout	40
548	It Won't Be Long	41
549	All I've Got to Do	41
550	All My Loving	41
551	Don't Bother Me	41
552	Little Child	41
553	Till There Was You	41
554	Please Mister Postman	41
555	Roll Over Beethoven	41
556	Hold Me Tight	41
557	You Really Got a Hold on Me	41
558	I Wanna Be Your Man	41
559	Devil in Her Heart	41
560	Not a Second Time	41
561	Money (That's What I Want)	41
562	A Hard Day s Night	42
563	I Should Have Known Better	42
564	If I Fell	42
565	I'm Happy Just To Dance With You	42
566	And I Love Her	42
567	Tell Me Why	42
568	Can't Buy Me Love	42
569	Any Time At All	42
570	I'll Cry Instead	42
571	Things We Said Today	42
572	When I Get Home	42
573	You Can't Do That	42
574	I'll Be Back	42
575	No Reply	43
576	I'm a Loser	43
577	Baby's In Black	43
578	Rock And Roll Music	43
579	I'll Follow the Sun	43
580	Mr. Moonlight	43
581	Kansas City-Hey-Hey-Hey-Hey!	43
582	Eight Days A Week	43
583	Words of Love	43
584	Honey Don't	43
585	Every Little Thing	43
586	I Don't Want to Spoil the Party	43
587	What You're Doing	43
588	Everybody's Trying To Be My Baby	43
589	Help!	44
590	The Night Before	44
591	You've Got To Hide Your Love Away	44
592	I Need You	44
593	Another Girl	44
594	You're Going To Lose That Girl	44
595	07, Ticket to Ride	44
596	Act Naturally	44
597	It's Only Love	44
598	You Like Me Too Much	44
599	Tell Me What You See	44
600	I've Just Seen a Face	44
601	Yesterday	44
602	Dizzy Miss Lizzy	44
603	Taxman	45
604	Eleanor Rigby	45
605	I'm Only Sleeping	45
606	Love You To	45
607	Here, There And Everywhere	45
608	She Said She Said	45
609	Good Day Sunshine	45
610	And Your Bird Can Sing	45
611	For No One	45
612	Doctor Robert	45
613	I Want To Tell You	45
614	Got To Get You Into My Life	45
615	Tomorrow Never Knows	45
616	Sgt. Pepper's Lonely Hearts Club Band	46
617	With A Little Help From My Friends	46
618	Lucy In The Sky With Diamonds	46
619	Getting Better	46
620	Fixing A Hole	46
621	She's Leaving Home	46
622	Being For The Benefit Of Mr. Kite!	46
623	Within You Without You	46
624	When I'm Sixty-Four	46
625	Lovely Rita	46
626	Good Morning Good Morning	46
627	A Day In The Life	46
628	Sgt. Pepper's Lonely Hearts Club Band (Reprise)	46
629	Back In USSR	47
630	Dear Prudence	47
631	Glass Onion	47
632	Ob-La-Di-Ob-La-Da	47
633	Wild Honey Pie	47
634	Counting Story Of Bungalow Bill	47
635	While My Guitar Gently Weeps	47
636	08 Happiness Is a Warm Gun	47
637	Martha My Dear	47
638	I'm So Tired	47
639	Blackbird	47
640	Piggies	47
641	Rocky Raccoon	47
642	Don't Pass Me By	47
643	Why Don't We Do It In The Road	47
644	I Will	47
645	Julia	47
646	Birthday	47
647	Yer Blues	47
648	Mother Nature's Son	47
649	Everybody's Got Something to Hide Except of Me and My Monkey	47
650	Sexy Sadie	47
651	Helter Skelter	47
652	Long, Long, Long	47
653	Revolution 1	47
654	Honey Pie	47
655	Savoy Truffle	47
656	Cry Baby Cry	47
657	Revolution 9	47
658	Good Night	47
659	Yellow Submarine	48
660	Only A Northern Song	48
661	All Together Now	48
662	Hey Bulldog	48
663	It's All Too Much	48
664	All You Need Is Love	48
665	Pepperland	48
666	Sea Of Time	48
667	Sea Of Monsters	48
668	March Of The Meanies	48
669	Pepperland Laid Waste	48
670	Yellow Submarine In Pepperland	48
671	Come Together	49
672	Something	49
673	Maxwell's Silver Hammer	49
674	Oh! Darling	49
675	Octopus's Garden	49
676	I Want You (She's So Heavy)	49
677	Here Comes The Sun	49
678	Because	49
679	You Never Give Me Your Money	49
680	Sun King	49
681	Mean Mr. Mustard	49
682	Polythene Pam	49
683	She Came In Through The Bathroom Window	49
684	Golden Slumbers	49
685	Carry That Weight	49
686	The End	49
687	Her Majesty	49
688	Two Of Us	50
689	Dig A Pony	50
690	Across The Universe	50
691	I Me Mine	50
692	Dig It	50
693	Let It Be	50
694	Maggie Mae	50
695	I've Got A Feeling	50
696	One After 909	50
697	The Long and Winding Road	50
698	For You Blue	50
699	Get Back	50
700	Flamme	51
701	Berceau	51
702	L'eclipse lunaire	51
703	Noctiluka	51
704	Meteore	51
705	Mer de sable	51
706	Rainy Song	51
707	Mariee en juin	51
708	Bouquet de lumiere	51
709	Amour ex machina	51
710	Berceuse	51
711	Polka dot	51
712	Le chemin du soleil	51
713	Croissant De Lune	51
714	Foret de pierre	51
715	Requiem	51
716	Reflection	51
717	From Me To You	51
718	Sakurao	51
719	AYA	51
720	Moon Dance	51
721	Moonlight Arpeggio	51
722	Feather	51
723	Yellow Green	51
724	For Nao	51
725	Vintage Waltz	51
726	Mahora	51
727	Love letter	51
728	Snow	51
729	Roselight	51
730	I. Allegro	51
731	II. Adagio	51
732	III. Menuetto Allegretto	51
733	IV. Prestissimo	51
734	I. Allegro vivace	51
735	II. Largo appassionato	51
736	III. Scherzo	51
737	IV. Rondo Grazioso	51
738	I. Allegro con brio	51
739	II. Adagio	51
740	III. Schrezo, Allegro	51
741	IV. Allegro assai	51
742	I. Allegro molto e con brio	51
743	II. Largo con gran espressione	51
744	III. Allegretto	51
745	IV. Rondo, Poco allegretto e grazioso	51
746	I. Allegro molto e con brio	51
747	II. Adagio molto	51
748	III. Finale, Pretissimmo	51
749	I. Allegro	51
750	II. Menuetto, Allegretto	51
751	III. Presto	51
752	I. Presto	51
753	II. Largo e mesto	51
754	III. Menuetto, Allegro	51
755	IV. Rondo, Allegro	51
756	I. Grave, Allegro di molto e con brio	51
757	II. Adagio cantabile	51
758	III. Rondo, Allegro	51
759	I. Allegro	51
760	II. Allegretto	51
761	III. Rondo, Allegro comodo	51
762	I. Allegro	51
763	II. Andante	51
764	III. Schrezo, Allegro assai	51
765	I. Allegro con brio	51
766	II. Adagio con molta espressione	51
767	III. Menuetto	51
768	IV. Rondo, Allegretto	51
769	I. Andante con variazioni	51
770	II. Schrezo, allegro molto	51
771	III. Maestoso andante marcia funebre sulta d'un eroe	51
772	IV. Allegro Rondo	51
773	I. Andante	51
774	II. Allegro molto e vivace	51
775	III. Adagio con espressione	51
776	IV. Allegro vivace	51
777	I. Adagio sostenuto	51
778	II. Allegretto	51
779	III. Presto agitato	51
780	I. Allegro	51
781	II. Andante	51
782	III. Schrezo, Allegro vivace	51
783	IV. Rondo, Allegro ma non troppo	51
784	I. Allegro vivace	51
785	II. Adagio grazioso	51
786	III. Rondo, Allegretto	51
787	I. Largo, Allegro	51
788	II. Adagio	51
789	III. Allegretto	51
790	I. Allegro	51
791	II. Scherzo. Allegretto vivace	51
792	III. Menuetto. Moderato e grazioso	51
793	IV. Presto con fuoco	51
794	I. Andante	51
795	II. Rondo. Allegro	51
796	I. Allegro ma non troppo	51
797	II. Tempo di menuetto	51
798	I. Allegro con brio	51
799	II. Introduzione. Adagio molto	51
800	III. Rondo. Allegretto moderato	51
801	I. In tempo d'un Menuetto	51
802	II. Allegretto	51
803	I. Allegro assai	51
804	II. Andante con molto	51
805	III. Allegro ma non troppo	51
806	I. Adagio cantabile. Allegro ma non troppo	51
807	II. Allegro vivace	51
808	I. Presto alia tedsesca	51
809	II. Andante	51
810	III. Vivace	51
811	I. Adagio, Allegro	51
812	II. Andante espressivo	51
813	III. Vivacissimamente	51
814	I. Mit Lebhaftigkeit und durchaus mit Empfindung und Ausdruck (Con vivacita e sempre con sentimento ed espressione)	51
815	II. Nicht zu geschwind und sehr singbar vorzutragen (Non troppo vivace e cantabile assai)	51
816	I. Etwas lebhaft, und mit der inngsten Empfindung. (Allegretto, ma non troppo)	51
817	II. Lebhaft. Marschmaessig. (Vivace alla marcia)	51
818	III. Langsam und sehnsuchtsvoll. (Adagio, ma non troppo, con affetto)	51
819	IV. Geschwind, doch nicht zu sehr und mit Entschlossenheit. (Allegro)	51
820	I. Allegro	51
821	II. Scherzo, assai vivace	51
822	III. Adagio sostenuto. Appassionato e con molto sentimento	51
823	IV. Largo, Allegro risoluto	51
824	I. Vivace ma non troppo, Adagio expressivo	51
825	II. Prestissimo	51
826	III. Andante, molto cantabile con espressivo	51
827	I. Moderato cantabile molto espressivo	51
828	II. Allegro molto	51
829	III. Adagio, man non troppo, Fuga. Allegro, ma non troppo	51
830	I. Maestoso Allegro con brio ed appassionato	51
831	II. Arietta Adagio molto, semplice e cantabile	51
832	Etude No. 01	51
833	Etude No. 02	51
834	Etude No. 03	51
835	Etude No. 04	51
836	Etude No. 05	51
837	Etude No. 06	51
838	Etude No. 07	51
839	Etude No. 08	51
840	Etude No. 09	51
841	Etude No. 10	51
842	Etude No. 11	51
843	Etude No. 12	51
844	Very Ape (Instrumental)	1
845	Bored	2
846	Minus Blindfold	2
848	Nosebleed	2
849	Lifter	2
850	Root	2
851	7 Words	2
852	Birthmark	2
853	Engine No. 9	2
854	Fireal	2
855	Fist	2
856	My Own Summer (Shove It)	3
857	Lhabia	3
858	Mascara	3
859	Around the Fur	3
860	Rickets	3
861	Be Quiet and Drive (Far Away)	3
862	Lotion	3
863	Dai the Flu	3
864	Head Up	3
865	MX	3
866	Bong Hit	3
867	Damone	3
868	Back To School (Mini Magit)	4
869	Feiticeira	4
870	Digital Bath	4
871	Elite	4
872	Rx Queen	4
873	Street Carp	4
874	Teenager	4
875	Knife Prty	4
876	Korea	4
877	Passenger	4
878	Change (In the House of Flies)	4
879	Pink Maggit	4
880	The Boy's Republic	4
881	Hexagram	5
882	Needles and Pins	5
883	Minerva	5
884	Good Morning Beautiful	5
885	Deathblow	5
886	When Girls Telephone Boys	5
887	Battle-axe	5
888	Lucky You	5
889	Bloody Cape	5
890	Anniversary of an Uninteresting Event	5
891	Moana	5
892	Savory	6
893	Wax and Wane	6
894	Change (In The House Of Flies) (Acoustic)	6
895	Simple Man	6
896	Sinatra	6
897	No Ordinary Love (Ft. Jonah Matranga)	6
898	Teenager (Idiot Version) (feat. Michael Harris)	6
899	Crenshaw Punch or I'll Throw Rocks at You	6
900	Black Moon	6
901	If Only Tonight We Could Sleep	6
902	Please Please Please Let Me Get What I Want	6
903	Digital Bath (Acoustic)	6
904	The Chauffeur	6
905	Be Quiet and Drive (Far Away)(Acoustic)	6
906	Night Boat	6
907	Hole in the Earth	7
908	Rapture	7
909	Beware	7
910		7
911	Mein	7
912	U,U,D,D,L,R,L,R,A,B,Select,Start	7
913	Xerces	7
914	Rats!Rats!Rats!	7
915	Pink Cellphone	7
916	Combat	7
917	Kimdracula	7
918	Drive	7
919	Riviere	7
920	Diamond Eyes	8
921	Royal	8
922	CMND-CTRL	8
923	You've Seen the Butcher	8
924	Beauty School	8
925	Prince	8
926	Rocket Skates	8
927	Sextape	8
928	Risk	8
929	976-EVIL	8
930	This Place Is Death	8
931	Do You Believe	8
932	Ghosts	8
933	Caress	8
934	Swerve City	9
935	Romantic Dreams	9
936	Leathers	9
937	Poltergeist	9
938	Entombed	9
939	Graphic Nature	9
940	Tempest	9
941	Gauze	9
942	Rosemary	9
943	Goon Squad	9
944	What Happened to You	9
945	Players or Triangles	10
946	Acid Hologram	10
947	Doomed User	10
948	Geometric Headdress	10
949	Hearts or Wires	10
950	Pittura Infamante	10
951	Xenon	10
952	(L)MIRL	10
953	Gore	10
954	Phantom Bride	10
955	Rubicon	10
956	Genesis	11
957	Ceremony	11
958	Urantia	11
959	Error	11
960	The Spell of Mathematics	11
961	Pompeji	11
962	This Link Is Dead	11
963	Radiant City	11
964	Headless	11
965	Ohms	11
966	my mind is a mountain	12
967	locked club	12
968	ecdysis	12
969	infinite source	12
970	souvenir	12
971	cXz	12
972	i think about you all the time	12
973	milk of the madonna	12
974	cut hands	12
975	~metal dream	12
976	departing the body	12
977	This Is a Call	13
978	I'll Stick Around	13
979	Oh, George	13
980	Big Me	13
981	Alone + Easy Target	13
982	Good Grief	13
983	Floaty	13
984	Weenie Beenie	13
985	For All the Cows	13
986	X-Static	13
987	Wattershed	13
988	Exhausted	13
989	Doll	14
990	Monkey Wrench	14
991	Hey, Johnny Park!	14
992	My Poor Brain	14
993	Wind Up	14
994	Up In Arms	14
995	My Hero	14
996	See You	14
997	Enough Space	14
998	February Stars	14
999	Everlong	14
1000	Walking After You	14
1001	New Way Home	14
1002	The Colour And The Shape	14
1003	Stacked Actors	15
1004	Breakout	15
1005	Learn to Fly	15
1006	Gimme Stitches	15
1007	Generator	15
1008	Aurora	15
1009	Live-In Skin	15
1010	Next Year	15
1011	Headwires	15
1012	Ain't It The Life	15
1013	M.I.A	15
1014	All My Life	16
1015	Low	16
1016	Have It All	16
1017	Times Like These	16
1018	Disenchanted Lullaby	16
1019	Tired Of You	16
1020	Halo	16
1021	Lonely As You	16
1022	Overdrive	16
1023	Burn Away	16
1024	Come Back	16
1025	Walking A Line	16
1026	Sister Europe	16
1027	Danny Says	16
1028	Life Of Illusion	16
1029	For All The Cows (Live In Amsterdam)	16
1030	Monkey Wrench	16
1031	In Your Honor	17
1032	Still	17
1033	No Way Back	17
1034	What If I Do	17
1035	Best of You	17
1036	Miracle	17
1037	Another Round	17
1038	DOA	17
1039	Friend Of A Friend	17
1040	Hell	17
1041	Over And Out	17
1042	The Last Song	17
1043	Free Me	17
1044	On The Mend	17
1045	Resolve	17
1046	Virginia Moon	17
1047	Cold Day In The Sun	17
1048	The Deepest Blues Are Black	17
1049	End Over End	17
1050	Razor	17
1051	The Pretender	18
1052	Let It Die	18
1053	Replace	18
1054	Long Road To Ruin	18
1055	Come Alive	18
1056	Stranger Things Have Happened	18
1057	Cheer Up, Boys (Your Make Up Is Running)	18
1058	Summer s End	18
1059	Ballad Of The Beaconsfield Miners	18
1060	Statues	18
1061	But, Honestly	18
1062	Home	18
1063	Bridge Burning	19
1064	Rope	19
1065	Dear Rosemary	19
1066	White Limo	19
1067	Arlandria	19
1068	These Days	19
1069	Back Forth	19
1070	A Matter Of Time	19
1071	Miss The Misery	19
1072	I Should Have Known	19
1073	Walk	19
1074	Something from Nothing	20
1075	The Feast and The Famine	20
1076	Congregation	20
1077	God As My Witness	20
1078	Outside	20
1079	In The Clear	20
1080	Subterranean	20
1081	I Am A River	20
1082	T-Shirt	21
1083	Run	21
1084	Make It Right	21
1085	The Sky Is A Neighborhood	21
1086	La Dee Da	21
1087	Dirty Water	21
1088	Arrows	21
1089	Happy Ever After (Zero Hour)	21
1090	Sunday Rain	21
1091	The Line	21
1092	Concrete and Gold	21
1093	Rescued	22
1094	Under You	22
1095	Hearing Voices	22
1096	But Here We Are	22
1097	The Glass	22
1098	Nothing At All	22
1099	Show Me How	22
1100	Beyond Me	22
1101	The Teacher	22
1102	Rest	22
1103	Wilting	23
1104	Reaching For Something	23
1105	Suntouched Shillelagh	23
1106	Coffin Dance	23
1107	If Only Your Best Was Good Enough	23
1108	Loch Lomand	23
1109	Armet Of Steel	23
1110	The Answer Been Better	23
1111	Could Be Worse	23
1112	Safe Indoors	24
1113	No Such Thing As Permanent	24
1114	Blankets Over Head	24
1115	Hallowed Ground	24
1116	Cabin Aflame	24
1117	Anywhere You Go	24
1118	Think I'll Just Sleep	24
1119	I	25
1120	II	25
1121	III	25
1122	IV	25
1123	Foreword	26
1124	Don't Stay	26
1125	Somewhere I Belong	26
1126	Lying from You	26
1127	Hit the Floor	26
1128	Easier to Run	26
1129	Faint	26
1130	Figure.09	26
1131	Breaking the Habit	26
1132	From the Inside	26
1133	Nobody's Listening	26
1134	Session	26
1135	Numb	26
1136	Blew	27
1137	Floyd The Barber	27
1138	About A Girl	27
1139	School	27
1140	Love Buzz	27
1141	Paper Cuts	27
1142	Negative Creep	27
1143	Scoff	27
1144	Swap Meet	27
1145	Mr. Moustache	27
1146	Sifting	27
1147	Big Cheese	27
1148	Downer	27
1149	Smells Like Teen Spirit	28
1150	In Bloom	28
1151	Come As You Are	28
1152	Breed	28
1153	Lithium	28
1154	Polly	28
1155	Territorial Pissings	28
1156	Drain You	28
1157	Lounge Act	28
1158	Stay Away	28
1159	On A Plain	28
1160	Something In The Way	28
1161	Serve The Servants	29
1162	Scentless Apprentice	29
1163	Heart-Shaped Box	29
1164	Rape Me	29
1165	Frances Farmer Will Have Her Revenge On Seattle	29
1166	Dumb	29
1167	Very Ape	29
1168	Milk It	29
1169	Pennyroyal Tea	29
1170	Radio Friendly Unit Shifter	29
1171	Tourette's	29
1172	All Apologies	29
1173	Dive	30
1174	Sliver	30
1175	Stain	30
1176	Been A Son	30
1177	Turnaround	30
1178	Molly's Lips	30
1179	Son Of A Gun	30
1180	(New Wawe) Polly	30
1181	Beeswax	30
1182	Downer	30
1183	Mexican Seafood	30
1184	Hairspray Queen	30
1185	Aero Zeppelin	30
1186	Big Long Now	30
1187	Aneurysm	30
1188	About A Girl	31
1189	Come As You Are	31
1190	Jesus Doesn't Want Me For A Sunbe	31
1191	The Man Who Sold The World	31
1192	Pennyroyal Tea	31
1193	Dumb	31
1194	Polly	31
1195	On A Plain	31
1196	Something In The Way	31
1197	Plateau	31
1198	Oh, Me	31
1199	Lake Of Fire	31
1200	All Apologies	31
1201	Where Did You Sleep Last Night	31
1202	Fearless	32
1203	Fifteen	32
1204	Love Story	32
1205	Hey Stephen	32
1206	White Horse	32
1207	You Belong With Me	32
1208	Breathe	32
1209	Tell Me Why	32
1210	You're Not Sorry	32
1211	The Way I Loved You	32
1212	Forever & Always	32
1213	The Best Day	32
1214	Change	32
1215	Jump Then Fall	32
1216	Untouchable	32
1217	Forever & Always (Acoustic Version)	32
1218	Come In With The Rain	32
1219	Superstar	32
1220	The Other Side Of The Door	32
1221	Today Was A Fairytale	32
1222	You All Over Me	32
1223	Mr. Perfectly Fine	32
1224	We Were Happy	32
1225	That's When	32
1226	Don't You	32
1227	Bye Bye Baby	32
1228	State Of Grace	33
1229	Red	33
1230	Treacherous	33
1231	I Knew You Were Trouble	33
1232	All Too Well	33
1233	22	33
1234	I Almost Do	33
1235	We Are Never Ever Getting Back Together	33
1236	Stay Stay Stay	33
1237	The Last Time	33
1238	Holy Ground	33
1239	Sad Beautiful Tragic	33
1240	The Lucky One	33
1241	Everything Has Changed	33
1242	Starlight	33
1243	Begin Again	33
1244	The Moment I Knew	33
1245	Come Back...Be Here	33
1246	Girl At Home	33
1247	State Of Grace (Acoustic Version)	33
1248	Ronan	33
1249	Better Man	33
1250	Nothing New	33
1251	Babe	33
1252	Message In A Bottle	33
1253	I Bet You Think	33
1254	Forever Winter	33
1255	Run	33
1256	The Very First Night	33
1257	Style	33
1258	All Too Well 10 Minute Version	33
1259	Welcome To New York	34
1260	Blank Space	34
1261	Style	34
1262	Out Of The Woods	34
1263	All You Had To Do Was Stay	34
1264	Shake It Off	34
1265	I Wish You Would	34
1266	Bad Blood	34
1267	Wildest Dreams	34
1268	How You Get The Girl	34
1269	This Love	34
1270	I Know Places	34
1271	Clean	34
1272	Wonderland	34
1273	You Are In Love	34
1274	New Romantics	34
1275	...Ready For It	35
1276	End Game	35
1277	I Did Something Bad	35
1278	Don't Blame Me	35
1279	Delicate	35
1280	Look What You Made Me Do	35
1281	So It Goes	35
1282	Gorgeous	35
1283	Getaway Car	35
1284	King Of My Heart	35
1285	Dancing With Our Hands Tied	35
1286	Dress	35
1287	This Is Why We Can't Have Nice Things	35
1288	Call It What You Want	35
1289	New Year's Day	35
1290	I Forgot That You Existed	36
1291	Cruel Summer	36
1292	Lover	36
1293	The Man	36
1294	The Archer	36
1295	I Think He Knows	36
1296	Miss Americana The Heartbreak Prince	36
1297	Paper Rings	36
1298	Cornelia Street	36
1299	Death By A Thousand Cuts	36
1300	London Boy	36
1301	Soon You'll Get Better	36
1302	False God	36
1303	You Need To Calm Down	36
1304	Afterglow	36
1305	ME!	36
1306	It's Nice To Have A Friend	36
1307	Daylight	36
1308	the 1	37
1309	cardigan	37
1310	the last great american dynasty	37
1311	exile	37
1312	my tears ricochet	37
1313	mirrorball	37
1314	seven	37
1315	august	37
1316	this is me trying	37
1317	illicit affairs	37
1318	invisible string	37
1319	mad woman	37
1320	epiphany	37
1321	betty	37
1322	peace	37
1323	hoax	37
1324	the lakes	37
1325	willow	38
1326	champagne problems	38
1327	gold rush	38
1328	'tis the damn season	38
1329	tolerate it	38
1330	no body no crime	38
1331	happiness	38
1332	dorothea	38
1333	coney island	38
1334	ivy	38
1335	cowboy like me	38
1336	long story short	38
1337	marjorie	38
1338	closure	38
1339	evermore	38
1340	right where you left me	38
1341	it's time to go	38
1342	Fortnight	39
1343	The Tortured Poets Department	39
1344	My Boy Only Breaks His Favorite Toy	39
1345	Down Bad	39
1346	So Long, London	39
1347	But Daddy I Love Him	39
1348	Fresh Out The Slammer	39
1349	Florida!!	39
1350	Guilty as Sin	39
1351	Who's Afraid Of Little Old Me	39
1352	I Can Fix Him (No Really I Can)	39
1353	loml	39
1354	I Can Do It With a Broken Heart	39
1355	The Smallest Man Who Ever Lived	39
1356	The Alchemy	39
1357	Clara Bow	39
1358	The Black Dog	39
1359	imgonnagetyouback	39
1360	The Albatross	39
1361	Chloe or Sam or Sophia or Marcus	39
1362	How Did It End	39
1363	So High School	39
1364	I Hate It Here	39
1365	thanK you aIMee	39
1366	I Look in People's Windows	39
1367	The Prophecy	39
1368	Cassandra	39
1369	Peter	39
1370	The Bolter	39
1371	Robin	39
1372	The Manuscript	39
1373	Fortnight (Acoustic Version)	39
1374	Down Bad (Acoustic Version)	39
1375	But Daddy I Love Him (Acoustic Vers	39
1376	Guilty As Sin (Acoustic Version)	39
1377	I Saw Her Standing There	40
1378	Misery	40
1379	Anna Go To Him	40
1380	Chains	40
1381	Boys	40
1382	Ask Me Why	40
1383	Please Please Me	40
1384	Love Me Do	40
1385	P.S. I Love You	40
1386	Baby it's you	40
1387	Do you want to know a secret	40
1388	A Taste Of Honey	40
1389	There's A Place	40
1390	Twist And Shout	40
1391	It Won't Be Long	41
1392	All I've Got to Do	41
1393	All My Loving	41
1394	Don't Bother Me	41
1395	Little Child	41
1396	Till There Was You	41
1397	Please Mister Postman	41
1398	Roll Over Beethoven	41
1399	Hold Me Tight	41
1400	You Really Got a Hold on Me	41
1401	I Wanna Be Your Man	41
1402	Devil in Her Heart	41
1403	Not a Second Time	41
1404	Money (That's What I Want)	41
1405	A Hard Day s Night	42
1406	I Should Have Known Better	42
1407	If I Fell	42
1408	I'm Happy Just To Dance With You	42
1409	And I Love Her	42
1410	Tell Me Why	42
1411	Can't Buy Me Love	42
1412	Any Time At All	42
1413	I'll Cry Instead	42
1414	Things We Said Today	42
1415	When I Get Home	42
1416	You Can't Do That	42
1417	I'll Be Back	42
1418	No Reply	43
1419	I'm a Loser	43
1420	Baby's In Black	43
1421	Rock And Roll Music	43
1422	I'll Follow the Sun	43
1423	Mr. Moonlight	43
1424	Kansas City-Hey-Hey-Hey-Hey!	43
1425	Eight Days A Week	43
1426	Words of Love	43
1427	Honey Don't	43
1428	Every Little Thing	43
1429	I Don't Want to Spoil the Party	43
1430	What You're Doing	43
1431	Everybody's Trying To Be My Baby	43
1432	Help!	44
1433	The Night Before	44
1434	You've Got To Hide Your Love Away	44
1435	I Need You	44
1436	Another Girl	44
1437	You're Going To Lose That Girl	44
1438	07, Ticket to Ride	44
1439	Act Naturally	44
1440	It's Only Love	44
1441	You Like Me Too Much	44
1442	Tell Me What You See	44
1443	I've Just Seen a Face	44
1444	Yesterday	44
1445	Dizzy Miss Lizzy	44
1446	Taxman	45
1447	Eleanor Rigby	45
1448	I'm Only Sleeping	45
1449	Love You To	45
1450	Here, There And Everywhere	45
1451	She Said She Said	45
1452	Good Day Sunshine	45
1453	And Your Bird Can Sing	45
1454	For No One	45
1455	Doctor Robert	45
1456	I Want To Tell You	45
1457	Got To Get You Into My Life	45
1458	Tomorrow Never Knows	45
1459	Sgt. Pepper's Lonely Hearts Club Band	46
1460	With A Little Help From My Friends	46
1461	Lucy In The Sky With Diamonds	46
1462	Getting Better	46
1463	Fixing A Hole	46
1464	She's Leaving Home	46
1465	Being For The Benefit Of Mr. Kite!	46
1466	Within You Without You	46
1467	When I'm Sixty-Four	46
1468	Lovely Rita	46
1469	Good Morning Good Morning	46
1470	A Day In The Life	46
1471	Sgt. Pepper's Lonely Hearts Club Band (Reprise)	46
1472	Back In USSR	47
1473	Dear Prudence	47
1474	Glass Onion	47
1475	Ob-La-Di-Ob-La-Da	47
1476	Wild Honey Pie	47
1477	Counting Story Of Bungalow Bill	47
1478	While My Guitar Gently Weeps	47
1479	08 Happiness Is a Warm Gun	47
1480	Martha My Dear	47
1481	I'm So Tired	47
1482	Blackbird	47
1483	Piggies	47
1484	Rocky Raccoon	47
1485	Don't Pass Me By	47
1486	Why Don't We Do It In The Road	47
1487	I Will	47
1488	Julia	47
1489	Birthday	47
1490	Yer Blues	47
1491	Mother Nature's Son	47
1492	Everybody's Got Something to Hide Except of Me and My Monkey	47
1493	Sexy Sadie	47
1494	Helter Skelter	47
1495	Long, Long, Long	47
1496	Revolution 1	47
1497	Honey Pie	47
1498	Savoy Truffle	47
1499	Cry Baby Cry	47
1500	Revolution 9	47
1501	Good Night	47
1502	Yellow Submarine	48
1503	Only A Northern Song	48
1504	All Together Now	48
1505	Hey Bulldog	48
1506	It's All Too Much	48
1507	All You Need Is Love	48
1508	Pepperland	48
1509	Sea Of Time	48
1510	Sea Of Monsters	48
1511	March Of The Meanies	48
1512	Pepperland Laid Waste	48
1513	Yellow Submarine In Pepperland	48
1514	Come Together	49
1515	Something	49
1516	Maxwell's Silver Hammer	49
1517	Oh! Darling	49
1518	Octopus's Garden	49
1519	I Want You (She's So Heavy)	49
1520	Here Comes The Sun	49
1521	Because	49
1522	You Never Give Me Your Money	49
1523	Sun King	49
1524	Mean Mr. Mustard	49
1525	Polythene Pam	49
1526	She Came In Through The Bathroom Window	49
1527	Golden Slumbers	49
1528	Carry That Weight	49
1529	The End	49
1530	Her Majesty	49
1531	Two Of Us	50
1532	Dig A Pony	50
1533	Across The Universe	50
1534	I Me Mine	50
1535	Dig It	50
1536	Let It Be	50
1537	Maggie Mae	50
1538	I've Got A Feeling	50
1539	One After 909	50
1540	The Long and Winding Road	50
1541	For You Blue	50
1542	Get Back	50
1543	Flamme	51
1544	Berceau	51
1545	L'eclipse lunaire	51
1546	Noctiluka	51
1547	Meteore	51
1548	Mer de sable	51
1549	Rainy Song	51
1550	Mariee en juin	51
1551	Bouquet de lumiere	51
1552	Amour ex machina	51
1553	Berceuse	51
1554	Polka dot	51
1555	Le chemin du soleil	51
1556	Croissant De Lune	51
1557	Foret de pierre	51
1558	Requiem	51
1559	Reflection	51
1560	From Me To You	51
1561	Sakurao	51
1562	AYA	51
1563	Moon Dance	51
1564	Moonlight Arpeggio	51
1565	Feather	51
1566	Yellow Green	51
1567	For Nao	51
1568	Vintage Waltz	51
1569	Mahora	51
1570	Love letter	51
1571	Snow	51
1572	Roselight	51
1573	I. Allegro	51
1574	II. Adagio	51
1575	III. Menuetto Allegretto	51
1576	IV. Prestissimo	51
1577	I. Allegro vivace	51
1578	II. Largo appassionato	51
1579	III. Scherzo	51
1580	IV. Rondo Grazioso	51
1581	I. Allegro con brio	51
1582	II. Adagio	51
1583	III. Schrezo, Allegro	51
1584	IV. Allegro assai	51
1585	I. Allegro molto e con brio	51
1586	II. Largo con gran espressione	51
1587	III. Allegretto	51
1588	IV. Rondo, Poco allegretto e grazioso	51
1589	I. Allegro molto e con brio	51
1590	II. Adagio molto	51
1591	III. Finale, Pretissimmo	51
1592	I. Allegro	51
1593	II. Menuetto, Allegretto	51
1594	III. Presto	51
1595	I. Presto	51
1596	II. Largo e mesto	51
1597	III. Menuetto, Allegro	51
1598	IV. Rondo, Allegro	51
1599	I. Grave, Allegro di molto e con brio	51
1600	II. Adagio cantabile	51
1601	III. Rondo, Allegro	51
1602	I. Allegro	51
1603	II. Allegretto	51
1604	III. Rondo, Allegro comodo	51
1605	I. Allegro	51
1606	II. Andante	51
1607	III. Schrezo, Allegro assai	51
1608	I. Allegro con brio	51
1609	II. Adagio con molta espressione	51
1610	III. Menuetto	51
1611	IV. Rondo, Allegretto	51
1612	I. Andante con variazioni	51
1613	II. Schrezo, allegro molto	51
1614	III. Maestoso andante marcia funebre sulta d'un eroe	51
1615	IV. Allegro Rondo	51
1616	I. Andante	51
1617	II. Allegro molto e vivace	51
1618	III. Adagio con espressione	51
1619	IV. Allegro vivace	51
1620	I. Adagio sostenuto	51
1621	II. Allegretto	51
1622	III. Presto agitato	51
1623	I. Allegro	51
1624	II. Andante	51
1625	III. Schrezo, Allegro vivace	51
1626	IV. Rondo, Allegro ma non troppo	51
1627	I. Allegro vivace	51
1628	II. Adagio grazioso	51
1629	III. Rondo, Allegretto	51
1630	I. Largo, Allegro	51
1631	II. Adagio	51
1632	III. Allegretto	51
1633	I. Allegro	51
1634	II. Scherzo. Allegretto vivace	51
1635	III. Menuetto. Moderato e grazioso	51
1636	IV. Presto con fuoco	51
1637	I. Andante	51
1638	II. Rondo. Allegro	51
1639	I. Allegro ma non troppo	51
1640	II. Tempo di menuetto	51
1641	I. Allegro con brio	51
1642	II. Introduzione. Adagio molto	51
1643	III. Rondo. Allegretto moderato	51
1644	I. In tempo d'un Menuetto	51
1645	II. Allegretto	51
1646	I. Allegro assai	51
1647	II. Andante con molto	51
1648	III. Allegro ma non troppo	51
1649	I. Adagio cantabile. Allegro ma non troppo	51
1650	II. Allegro vivace	51
1651	I. Presto alia tedsesca	51
1652	II. Andante	51
1653	III. Vivace	51
1654	I. Adagio, Allegro	51
1655	II. Andante espressivo	51
1656	III. Vivacissimamente	51
1657	I. Mit Lebhaftigkeit und durchaus mit Empfindung und Ausdruck (Con vivacita e sempre con sentimento ed espressione)	51
1658	II. Nicht zu geschwind und sehr singbar vorzutragen (Non troppo vivace e cantabile assai)	51
1659	I. Etwas lebhaft, und mit der inngsten Empfindung. (Allegretto, ma non troppo)	51
1660	II. Lebhaft. Marschmaessig. (Vivace alla marcia)	51
1661	III. Langsam und sehnsuchtsvoll. (Adagio, ma non troppo, con affetto)	51
1662	IV. Geschwind, doch nicht zu sehr und mit Entschlossenheit. (Allegro)	51
1663	I. Allegro	51
1664	II. Scherzo, assai vivace	51
1665	III. Adagio sostenuto. Appassionato e con molto sentimento	51
1666	IV. Largo, Allegro risoluto	51
1667	I. Vivace ma non troppo, Adagio expressivo	51
1668	II. Prestissimo	51
1669	III. Andante, molto cantabile con espressivo	51
1670	I. Moderato cantabile molto espressivo	51
1671	II. Allegro molto	51
1672	III. Adagio, man non troppo, Fuga. Allegro, ma non troppo	51
1673	I. Maestoso Allegro con brio ed appassionato	51
1674	II. Arietta Adagio molto, semplice e cantabile	51
1675	Etude No. 01	51
1676	Etude No. 02	51
1677	Etude No. 03	51
1678	Etude No. 04	51
1679	Etude No. 05	51
1680	Etude No. 06	51
1681	Etude No. 07	51
1682	Etude No. 08	51
1683	Etude No. 09	51
1684	Etude No. 10	51
1685	Etude No. 11	51
1686	Etude No. 12	51
\.


--
-- Data for Name: statement; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.statement (id_statement, applicant, content, date_and_time, police_officer, signature_applicant, signature_police_officer, number) FROM stdin;
7	15	Прошу привлечь к ответственности гражданина, повредившего мое имущество	2024-02-20 10:00:00	1	\N	\N	3001
8	16	Прошу принять меры по факту мошенничества	2024-04-05 11:00:00	1	\N	\N	3002
9	16	выапва	2026-05-13 17:10:00	1	t	t	23
10	16	павапапапап	2026-05-20 08:46:00	1	t	t	23434
\.


--
-- Data for Name: structures; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.structures (id_structures, name, settlement, description) FROM stdin;
1	УМВД России по г. Москва	1	Управление Министерства внутренних дел
2	ОМВД России по району Арбат	1	Отдел Министерства внутренних дел
3	ГИБДД УМВД по г. Москва	1	Государственная инспекция безопасности дорожного движения
4	Следственное управление УМВД	1	Следственное подразделение
5	Центральная городская больница	1	Медицинское учреждение
6	Наркологический диспансер №1	1	Специализированное медицинское учреждение
7	УМВД России по г. Санкт-Петербург	2	Управление Министерства внутренних дел
8	ОМВД России по Центральному району	2	Отдел Министерства внутренних дел
9	Городская больница №40	2	Медицинское учреждение
10	УМВД России по г. Екатеринбург	3	Управление Министерства внутренних дел
\.


--
-- Data for Name: type_intoxication; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.type_intoxication (id_type_intoxication, type_intoxication) FROM stdin;
1	Алкогольное
2	Наркотическое
3	Токсическое
4	Не выявлено
\.


--
-- Data for Name: type_of_punishment; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.type_of_punishment (id_type_of_punishment, type_of_punishment) FROM stdin;
1	Штраф
2	Административный арест
3	Обязательные работы
4	Лишение специального права
5	Предупреждение
6	Исправительные работы
\.


--
-- Data for Name: type_report; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.type_report (id_type_report, type_report) FROM stdin;
5	Техническая экспертиза
1	Судебно-медицинская экспертиза
2	Судебно-психиатрическая экспертиза
4	Медицинское освидетельствование на опьянение
3	Судебно-наркологическая экспертиза
\.


--
-- Data for Name: user_citizen_link; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.user_citizen_link (id, user_id, citizen_post_id) FROM stdin;
1	1	1
2	3	2
3	4	1
8	6	15
9	5	17
\.


--
-- Data for Name: user_favorites; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.user_favorites (id, user_id, target_table, document_id, created_at) FROM stdin;
258	2	medical_examination_report	18	2026-06-11 06:32:38.188084
259	2	medical_examination_certificate	12	2026-06-11 06:36:27.593099
260	2	medical_examination_certificate	11	2026-06-11 06:36:28.53679
261	2	medical_examination_certificate	10	2026-06-11 06:36:28.899928
262	3	resolution	7	2026-06-11 11:27:16.524149
263	3	resolution	5	2026-06-11 11:27:17.129824
264	3	appeals	1	2026-06-11 11:27:20.603162
265	4	forensic_medical_examination	14	2026-06-11 11:31:11.661632
266	4	forensic_medical_examination	13	2026-06-11 11:31:12.201971
267	1	appeals	1	2026-06-12 06:52:56.302179
201	1	resolution	7	2026-06-09 16:09:29.924036
202	1	medical_examination_certificate	11	2026-06-09 16:09:31.625897
272	5	appeals	33	2026-06-13 18:20:26.634313
273	5	appeals	32	2026-06-13 18:20:26.905654
275	5	deal	15	2026-06-13 18:26:31.98869
156	4	forensic_medical_examination	12	2026-05-31 15:57:25.165541
157	4	forensic_medical_examination	11	2026-05-31 15:57:25.765785
235	1	administrative_protocol	16	2026-06-10 20:49:34.64223
236	1	administrative_protocol	15	2026-06-10 20:49:35.001892
237	1	administrative_protocol	14	2026-06-10 20:49:35.345978
238	1	administrative_protocol	13	2026-06-10 20:49:35.705828
239	1	administrative_protocol	12	2026-06-10 20:49:36.073761
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, username, password, last_name, first_name, patronymic, created_at, role) FROM stdin;
1	officer1	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	Иванов	Иван	Иванович	2026-03-31 08:54:44.027185	1
2	doctor1	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	Петрова	Анна	Ивановна	2026-05-21 13:59:40.5476	3
3	judge1	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	Сидорова	Елена	Александровна	2026-05-21 19:10:07.371558	2
5	inspector1	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	Инспекторов	Инспектор	Инспекторович	2026-06-01 13:34:38.302145	5
4	expert1	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	Кузнецов	Дмитрий	Сергеевич	2026-05-21 19:10:10.523043	4
6	chief1	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	Начальников	Начальник	Начальникович	2026-06-03 07:37:10.933037	6
\.


--
-- Data for Name: albums; Type: TABLE DATA; Schema: testdb; Owner: postgres
--

COPY testdb.albums (album_id, album_name, artist_id) FROM stdin;
1	Very Ape (Instrumental).mp3	1
2	01. Adrenaline	2
3	02. Around the Fur	2
4	03. White Pony	2
5	04. Deftones	2
6	05. B-Sides & Rarities	2
7	06. Saturday Night Wrist	2
8	07. Diamond Eyes	2
9	08. Koi No Yokan	2
10	09. Gore	2
11	10. Ohms	2
12	11. private music	2
13	01. Foo Fighters	3
14	02. The Colour and the Shape	3
15	03. There Is Nothing Left to Lose	3
16	04. One By One	3
17	05. In Your Honor	3
18	06. Echoes, Silence, Patience & Grace	3
19	07. Wasting Light	3
20	08. Sonic Highways	3
21	09. Concrete and Gold	3
22	10. But Here We Are	3
23	It Feels Like I'm Wilting Away	4
24	Safe Indoors	4
25	watch me disappear	4
26	Meteora	5
27	01. Bleach	6
28	02. Nevermind	6
29	03. In Utero	6
30	04. Incesticide	6
31	05. MTV Unplugged in New York	6
32	01. Fearless	7
33	02. Red	7
34	03. 1989	7
35	04. reputation	7
36	05. Lover	7
37	06. folklore	7
38	07. evermore	7
39	08. The Tortured Poets Department	7
40	01. Please Please Me	8
41	02. With the Beatles	8
42	03. A Hard Day's Night	8
43	04. Beatles for Sale	8
44	05. Help!	8
45	07. Revolver	8
46	08. Sgt. Pepper's Lonely Hearts Club Band	8
47	09. White Album	8
48	10. Yellow Submarine	8
49	11. Abbey Road	8
50	12. Let It Be	8
51	Piano	9
\.


--
-- Data for Name: artists; Type: TABLE DATA; Schema: testdb; Owner: postgres
--

COPY testdb.artists (artist_id, artist_name) FROM stdin;
1	Instrumentals
2	Deftones
3	Foo Fighters
4	grayera
5	Linkin Park
6	Nirvana
7	Taylor Swift
8	The Beatles
9	Classic
\.


--
-- Data for Name: rockgroups_notes; Type: TABLE DATA; Schema: testdb; Owner: postgres
--

COPY testdb.rockgroups_notes (id, song_name, group_name, album_name) FROM stdin;
30	The Pretender	Foo Fighters	Echoes, Patience, Silense & Grace
25	If Only Your Best Was Good Enough	grayera	It Feels Like I'm Wilting Away
28	champagne problems	Taylor Swift	1989
23	Around the Fur	Deftones	Around the Fur
24	February Stars	Foo Fighters	The Colour and the Shape
26	Numb	Linkin Park	Meteora
29	Come Together	The Beatles	Abbey Road
27	Drain You	Nirvana	Nevermind
\.


--
-- Data for Name: songs; Type: TABLE DATA; Schema: testdb; Owner: postgres
--

COPY testdb.songs (song_id, song_name, album_id) FROM stdin;
1	Very Ape (Instrumental)	1
2	Bored	2
3	Minus Blindfold	2
4	One Weak	2
5	Nosebleed	2
6	Lifter	2
7	Root	2
8	7 Words	2
9	Birthmark	2
10	Engine No. 9	2
11	Fireal	2
12	Fist	2
13	My Own Summer (Shove It)	3
14	Lhabia	3
15	Mascara	3
16	Around the Fur	3
17	Rickets	3
18	Be Quiet and Drive (Far Away)	3
19	Lotion	3
20	Dai the Flu	3
21	Head Up	3
22	MX	3
23	Bong Hit	3
24	Damone	3
25	Back To School (Mini Magit)	4
26	Feiticeira	4
27	Digital Bath	4
28	Elite	4
29	Rx Queen	4
30	Street Carp	4
31	Teenager	4
32	Knife Prty	4
33	Korea	4
34	Passenger	4
35	Change (In the House of Flies)	4
36	Pink Maggit	4
37	The Boy's Republic	4
38	Hexagram	5
39	Needles and Pins	5
40	Minerva	5
41	Good Morning Beautiful	5
42	Deathblow	5
43	When Girls Telephone Boys	5
44	Battle-axe	5
45	Lucky You	5
46	Bloody Cape	5
47	Anniversary of an Uninteresting Event	5
48	Moana	5
49	Savory	6
50	Wax and Wane	6
51	Change (In The House Of Flies) (Acoustic)	6
52	Simple Man	6
53	Sinatra	6
54	No Ordinary Love (Ft. Jonah Matranga)	6
55	Teenager (Idiot Version) (feat. Michael Harris)	6
56	Crenshaw Punch or I'll Throw Rocks at You	6
57	Black Moon	6
58	If Only Tonight We Could Sleep	6
59	Please Please Please Let Me Get What I Want	6
60	Digital Bath (Acoustic)	6
61	The Chauffeur	6
62	Be Quiet and Drive (Far Away)(Acoustic)	6
63	Night Boat	6
64	Hole in the Earth	7
65	Rapture	7
66	Beware	7
67		7
68	Mein	7
69	U,U,D,D,L,R,L,R,A,B,Select,Start	7
70	Xerces	7
71	Rats!Rats!Rats!	7
72	Pink Cellphone	7
73	Combat	7
74	Kimdracula	7
75	Drive	7
76	Riviere	7
77	Diamond Eyes	8
78	Royal	8
79	CMND-CTRL	8
80	You've Seen the Butcher	8
81	Beauty School	8
82	Prince	8
83	Rocket Skates	8
84	Sextape	8
85	Risk	8
86	976-EVIL	8
87	This Place Is Death	8
88	Do You Believe	8
89	Ghosts	8
90	Caress	8
91	Swerve City	9
92	Romantic Dreams	9
93	Leathers	9
94	Poltergeist	9
95	Entombed	9
96	Graphic Nature	9
97	Tempest	9
98	Gauze	9
99	Rosemary	9
100	Goon Squad	9
101	What Happened to You	9
102	Players or Triangles	10
103	Acid Hologram	10
104	Doomed User	10
105	Geometric Headdress	10
106	Hearts or Wires	10
107	Pittura Infamante	10
108	Xenon	10
109	(L)MIRL	10
110	Gore	10
111	Phantom Bride	10
112	Rubicon	10
113	Genesis	11
114	Ceremony	11
115	Urantia	11
116	Error	11
117	The Spell of Mathematics	11
118	Pompeji	11
119	This Link Is Dead	11
120	Radiant City	11
121	Headless	11
122	Ohms	11
123	my mind is a mountain	12
124	locked club	12
125	ecdysis	12
126	infinite source	12
127	souvenir	12
128	cXz	12
129	i think about you all the time	12
130	milk of the madonna	12
131	cut hands	12
132	~metal dream	12
133	departing the body	12
134	This Is a Call	13
135	I'll Stick Around	13
136	Oh, George	13
137	Big Me	13
138	Alone + Easy Target	13
139	Good Grief	13
140	Floaty	13
141	Weenie Beenie	13
142	For All the Cows	13
143	X-Static	13
144	Wattershed	13
145	Exhausted	13
146	Doll	14
147	Monkey Wrench	14
148	Hey, Johnny Park!	14
149	My Poor Brain	14
150	Wind Up	14
151	Up In Arms	14
152	My Hero	14
153	See You	14
154	Enough Space	14
155	February Stars	14
156	Everlong	14
157	Walking After You	14
158	New Way Home	14
159	The Colour And The Shape	14
160	Stacked Actors	15
161	Breakout	15
162	Learn to Fly	15
163	Gimme Stitches	15
164	Generator	15
165	Aurora	15
166	Live-In Skin	15
167	Next Year	15
168	Headwires	15
169	Ain't It The Life	15
170	M.I.A	15
171	All My Life	16
172	Low	16
173	Have It All	16
174	Times Like These	16
175	Disenchanted Lullaby	16
176	Tired Of You	16
177	Halo	16
178	Lonely As You	16
179	Overdrive	16
180	Burn Away	16
181	Come Back	16
182	Walking A Line	16
183	Sister Europe	16
184	Danny Says	16
185	Life Of Illusion	16
186	For All The Cows (Live In Amsterdam)	16
187	Monkey Wrench	16
188	In Your Honor	17
189	Still	17
190	No Way Back	17
191	What If I Do	17
192	Best of You	17
193	Miracle	17
194	Another Round	17
195	DOA	17
196	Friend Of A Friend	17
197	Hell	17
198	Over And Out	17
199	The Last Song	17
200	Free Me	17
201	On The Mend	17
202	Resolve	17
203	Virginia Moon	17
204	Cold Day In The Sun	17
205	The Deepest Blues Are Black	17
206	End Over End	17
207	Razor	17
208	The Pretender	18
209	Let It Die	18
210	Replace	18
211	Long Road To Ruin	18
212	Come Alive	18
213	Stranger Things Have Happened	18
214	Cheer Up, Boys (Your Make Up Is Running)	18
215	Summer s End	18
216	Ballad Of The Beaconsfield Miners	18
217	Statues	18
218	But, Honestly	18
219	Home	18
220	Bridge Burning	19
221	Rope	19
222	Dear Rosemary	19
223	White Limo	19
224	Arlandria	19
225	These Days	19
226	Back Forth	19
227	A Matter Of Time	19
228	Miss The Misery	19
229	I Should Have Known	19
230	Walk	19
231	Something from Nothing	20
232	The Feast and The Famine	20
233	Congregation	20
234	God As My Witness	20
235	Outside	20
236	In The Clear	20
237	Subterranean	20
238	I Am A River	20
239	T-Shirt	21
240	Run	21
241	Make It Right	21
242	The Sky Is A Neighborhood	21
243	La Dee Da	21
244	Dirty Water	21
245	Arrows	21
246	Happy Ever After (Zero Hour)	21
247	Sunday Rain	21
248	The Line	21
249	Concrete and Gold	21
250	Rescued	22
251	Under You	22
252	Hearing Voices	22
253	But Here We Are	22
254	The Glass	22
255	Nothing At All	22
256	Show Me How	22
257	Beyond Me	22
258	The Teacher	22
259	Rest	22
260	Wilting	23
261	Reaching For Something	23
262	Suntouched Shillelagh	23
263	Coffin Dance	23
264	If Only Your Best Was Good Enough	23
265	Loch Lomand	23
266	Armet Of Steel	23
267	The Answer Been Better	23
268	Could Be Worse	23
269	Safe Indoors	24
270	No Such Thing As Permanent	24
271	Blankets Over Head	24
272	Hallowed Ground	24
273	Cabin Aflame	24
274	Anywhere You Go	24
275	Think I'll Just Sleep	24
276	I	25
277	II	25
278	III	25
279	IV	25
280	Foreword	26
281	Don't Stay	26
282	Somewhere I Belong	26
283	Lying from You	26
284	Hit the Floor	26
285	Easier to Run	26
286	Faint	26
287	Figure.09	26
288	Breaking the Habit	26
289	From the Inside	26
290	Nobody's Listening	26
291	Session	26
292	Numb	26
293	Blew	27
294	Floyd The Barber	27
295	About A Girl	27
296	School	27
297	Love Buzz	27
298	Paper Cuts	27
299	Negative Creep	27
300	Scoff	27
301	Swap Meet	27
302	Mr. Moustache	27
303	Sifting	27
304	Big Cheese	27
305	Downer	27
306	Smells Like Teen Spirit	28
307	In Bloom	28
308	Come As You Are	28
309	Breed	28
310	Lithium	28
311	Polly	28
312	Territorial Pissings	28
313	Drain You	28
314	Lounge Act	28
315	Stay Away	28
316	On A Plain	28
317	Something In The Way	28
318	Serve The Servants	29
319	Scentless Apprentice	29
320	Heart-Shaped Box	29
321	Rape Me	29
322	Frances Farmer Will Have Her Revenge On Seattle	29
323	Dumb	29
324	Very Ape	29
325	Milk It	29
326	Pennyroyal Tea	29
327	Radio Friendly Unit Shifter	29
328	Tourette's	29
329	All Apologies	29
330	Dive	30
331	Sliver	30
332	Stain	30
333	Been A Son	30
334	Turnaround	30
335	Molly's Lips	30
336	Son Of A Gun	30
337	(New Wawe) Polly	30
338	Beeswax	30
339	Downer	30
340	Mexican Seafood	30
341	Hairspray Queen	30
342	Aero Zeppelin	30
343	Big Long Now	30
344	Aneurysm	30
345	About A Girl	31
346	Come As You Are	31
347	Jesus Doesn't Want Me For A Sunbe	31
348	The Man Who Sold The World	31
349	Pennyroyal Tea	31
350	Dumb	31
351	Polly	31
352	On A Plain	31
353	Something In The Way	31
354	Plateau	31
355	Oh, Me	31
356	Lake Of Fire	31
357	All Apologies	31
358	Where Did You Sleep Last Night	31
359	Fearless	32
360	Fifteen	32
361	Love Story	32
362	Hey Stephen	32
363	White Horse	32
364	You Belong With Me	32
365	Breathe	32
366	Tell Me Why	32
367	You're Not Sorry	32
368	The Way I Loved You	32
369	Forever & Always	32
370	The Best Day	32
371	Change	32
372	Jump Then Fall	32
373	Untouchable	32
374	Forever & Always (Acoustic Version)	32
375	Come In With The Rain	32
376	Superstar	32
377	The Other Side Of The Door	32
378	Today Was A Fairytale	32
379	You All Over Me	32
380	Mr. Perfectly Fine	32
381	We Were Happy	32
382	That's When	32
383	Don't You	32
384	Bye Bye Baby	32
385	State Of Grace	33
386	Red	33
387	Treacherous	33
388	I Knew You Were Trouble	33
389	All Too Well	33
390	22	33
391	I Almost Do	33
392	We Are Never Ever Getting Back Together	33
393	Stay Stay Stay	33
394	The Last Time	33
395	Holy Ground	33
396	Sad Beautiful Tragic	33
397	The Lucky One	33
398	Everything Has Changed	33
399	Starlight	33
400	Begin Again	33
401	The Moment I Knew	33
402	Come Back...Be Here	33
403	Girl At Home	33
404	State Of Grace (Acoustic Version)	33
405	Ronan	33
406	Better Man	33
407	Nothing New	33
408	Babe	33
409	Message In A Bottle	33
410	I Bet You Think	33
411	Forever Winter	33
412	Run	33
413	The Very First Night	33
414	Style	33
415	All Too Well 10 Minute Version	33
416	Welcome To New York	34
417	Blank Space	34
418	Style	34
419	Out Of The Woods	34
420	All You Had To Do Was Stay	34
421	Shake It Off	34
422	I Wish You Would	34
423	Bad Blood	34
424	Wildest Dreams	34
425	How You Get The Girl	34
426	This Love	34
427	I Know Places	34
428	Clean	34
429	Wonderland	34
430	You Are In Love	34
431	New Romantics	34
432	...Ready For It	35
433	End Game	35
434	I Did Something Bad	35
435	Don't Blame Me	35
436	Delicate	35
437	Look What You Made Me Do	35
438	So It Goes	35
439	Gorgeous	35
440	Getaway Car	35
441	King Of My Heart	35
442	Dancing With Our Hands Tied	35
443	Dress	35
444	This Is Why We Can't Have Nice Things	35
445	Call It What You Want	35
446	New Year's Day	35
447	I Forgot That You Existed	36
448	Cruel Summer	36
449	Lover	36
450	The Man	36
451	The Archer	36
452	I Think He Knows	36
453	Miss Americana The Heartbreak Prince	36
454	Paper Rings	36
455	Cornelia Street	36
456	Death By A Thousand Cuts	36
457	London Boy	36
458	Soon You'll Get Better	36
459	False God	36
460	You Need To Calm Down	36
461	Afterglow	36
462	ME!	36
463	It's Nice To Have A Friend	36
464	Daylight	36
465	the 1	37
466	cardigan	37
467	the last great american dynasty	37
468	exile	37
469	my tears ricochet	37
470	mirrorball	37
471	seven	37
472	august	37
473	this is me trying	37
474	illicit affairs	37
475	invisible string	37
476	mad woman	37
477	epiphany	37
478	betty	37
479	peace	37
480	hoax	37
481	the lakes	37
482	willow	38
483	champagne problems	38
484	gold rush	38
485	'tis the damn season	38
486	tolerate it	38
487	no body no crime	38
488	happiness	38
489	dorothea	38
490	coney island	38
491	ivy	38
492	cowboy like me	38
493	long story short	38
494	marjorie	38
495	closure	38
496	evermore	38
497	right where you left me	38
498	it's time to go	38
499	Fortnight	39
500	The Tortured Poets Department	39
501	My Boy Only Breaks His Favorite Toy	39
502	Down Bad	39
503	So Long, London	39
504	But Daddy I Love Him	39
505	Fresh Out The Slammer	39
506	Florida!!	39
507	Guilty as Sin	39
508	Who's Afraid Of Little Old Me	39
509	I Can Fix Him (No Really I Can)	39
510	loml	39
511	I Can Do It With a Broken Heart	39
512	The Smallest Man Who Ever Lived	39
513	The Alchemy	39
514	Clara Bow	39
515	The Black Dog	39
516	imgonnagetyouback	39
517	The Albatross	39
518	Chloe or Sam or Sophia or Marcus	39
519	How Did It End	39
520	So High School	39
521	I Hate It Here	39
522	thanK you aIMee	39
523	I Look in People's Windows	39
524	The Prophecy	39
525	Cassandra	39
526	Peter	39
527	The Bolter	39
528	Robin	39
529	The Manuscript	39
530	Fortnight (Acoustic Version)	39
531	Down Bad (Acoustic Version)	39
532	But Daddy I Love Him (Acoustic Vers	39
533	Guilty As Sin (Acoustic Version)	39
534	I Saw Her Standing There	40
535	Misery	40
536	Anna Go To Him	40
537	Chains	40
538	Boys	40
539	Ask Me Why	40
540	Please Please Me	40
541	Love Me Do	40
542	P.S. I Love You	40
543	Baby it's you	40
544	Do you want to know a secret	40
545	A Taste Of Honey	40
546	There's A Place	40
547	Twist And Shout	40
548	It Won't Be Long	41
549	All I've Got to Do	41
550	All My Loving	41
551	Don't Bother Me	41
552	Little Child	41
553	Till There Was You	41
554	Please Mister Postman	41
555	Roll Over Beethoven	41
556	Hold Me Tight	41
557	You Really Got a Hold on Me	41
558	I Wanna Be Your Man	41
559	Devil in Her Heart	41
560	Not a Second Time	41
561	Money (That's What I Want)	41
562	A Hard Day s Night	42
563	I Should Have Known Better	42
564	If I Fell	42
565	I'm Happy Just To Dance With You	42
566	And I Love Her	42
567	Tell Me Why	42
568	Can't Buy Me Love	42
569	Any Time At All	42
570	I'll Cry Instead	42
571	Things We Said Today	42
572	When I Get Home	42
573	You Can't Do That	42
574	I'll Be Back	42
575	No Reply	43
576	I'm a Loser	43
577	Baby's In Black	43
578	Rock And Roll Music	43
579	I'll Follow the Sun	43
580	Mr. Moonlight	43
581	Kansas City-Hey-Hey-Hey-Hey!	43
582	Eight Days A Week	43
583	Words of Love	43
584	Honey Don't	43
585	Every Little Thing	43
586	I Don't Want to Spoil the Party	43
587	What You're Doing	43
588	Everybody's Trying To Be My Baby	43
589	Help!	44
590	The Night Before	44
591	You've Got To Hide Your Love Away	44
592	I Need You	44
593	Another Girl	44
594	You're Going To Lose That Girl	44
595	07, Ticket to Ride	44
596	Act Naturally	44
597	It's Only Love	44
598	You Like Me Too Much	44
599	Tell Me What You See	44
600	I've Just Seen a Face	44
601	Yesterday	44
602	Dizzy Miss Lizzy	44
603	Taxman	45
604	Eleanor Rigby	45
605	I'm Only Sleeping	45
606	Love You To	45
607	Here, There And Everywhere	45
608	She Said She Said	45
609	Good Day Sunshine	45
610	And Your Bird Can Sing	45
611	For No One	45
612	Doctor Robert	45
613	I Want To Tell You	45
614	Got To Get You Into My Life	45
615	Tomorrow Never Knows	45
616	Sgt. Pepper's Lonely Hearts Club Band	46
617	With A Little Help From My Friends	46
618	Lucy In The Sky With Diamonds	46
619	Getting Better	46
620	Fixing A Hole	46
621	She's Leaving Home	46
622	Being For The Benefit Of Mr. Kite!	46
623	Within You Without You	46
624	When I'm Sixty-Four	46
625	Lovely Rita	46
626	Good Morning Good Morning	46
627	A Day In The Life	46
628	Sgt. Pepper's Lonely Hearts Club Band (Reprise)	46
629	Back In USSR	47
630	Dear Prudence	47
631	Glass Onion	47
632	Ob-La-Di-Ob-La-Da	47
633	Wild Honey Pie	47
634	Counting Story Of Bungalow Bill	47
635	While My Guitar Gently Weeps	47
636	08 Happiness Is a Warm Gun	47
637	Martha My Dear	47
638	I'm So Tired	47
639	Blackbird	47
640	Piggies	47
641	Rocky Raccoon	47
642	Don't Pass Me By	47
643	Why Don't We Do It In The Road	47
644	I Will	47
645	Julia	47
646	Birthday	47
647	Yer Blues	47
648	Mother Nature's Son	47
649	Everybody's Got Something to Hide Except of Me and My Monkey	47
650	Sexy Sadie	47
651	Helter Skelter	47
652	Long, Long, Long	47
653	Revolution 1	47
654	Honey Pie	47
655	Savoy Truffle	47
656	Cry Baby Cry	47
657	Revolution 9	47
658	Good Night	47
659	Yellow Submarine	48
660	Only A Northern Song	48
661	All Together Now	48
662	Hey Bulldog	48
663	It's All Too Much	48
664	All You Need Is Love	48
665	Pepperland	48
666	Sea Of Time	48
667	Sea Of Monsters	48
668	March Of The Meanies	48
669	Pepperland Laid Waste	48
670	Yellow Submarine In Pepperland	48
671	Come Together	49
672	Something	49
673	Maxwell's Silver Hammer	49
674	Oh! Darling	49
675	Octopus's Garden	49
676	I Want You (She's So Heavy)	49
677	Here Comes The Sun	49
678	Because	49
679	You Never Give Me Your Money	49
680	Sun King	49
681	Mean Mr. Mustard	49
682	Polythene Pam	49
683	She Came In Through The Bathroom Window	49
684	Golden Slumbers	49
685	Carry That Weight	49
686	The End	49
687	Her Majesty	49
688	Two Of Us	50
689	Dig A Pony	50
690	Across The Universe	50
691	I Me Mine	50
692	Dig It	50
693	Let It Be	50
694	Maggie Mae	50
695	I've Got A Feeling	50
696	One After 909	50
697	The Long and Winding Road	50
698	For You Blue	50
699	Get Back	50
700	Flamme	51
701	Berceau	51
702	L'eclipse lunaire	51
703	Noctiluka	51
704	Meteore	51
705	Mer de sable	51
706	Rainy Song	51
707	Mariee en juin	51
708	Bouquet de lumiere	51
709	Amour ex machina	51
710	Berceuse	51
711	Polka dot	51
712	Le chemin du soleil	51
713	Croissant De Lune	51
714	Foret de pierre	51
715	Requiem	51
716	Reflection	51
717	From Me To You	51
718	Sakurao	51
719	AYA	51
720	Moon Dance	51
721	Moonlight Arpeggio	51
722	Feather	51
723	Yellow Green	51
724	For Nao	51
725	Vintage Waltz	51
726	Mahora	51
727	Love letter	51
728	Snow	51
729	Roselight	51
730	I. Allegro	51
731	II. Adagio	51
732	III. Menuetto Allegretto	51
733	IV. Prestissimo	51
734	I. Allegro vivace	51
735	II. Largo appassionato	51
736	III. Scherzo	51
737	IV. Rondo Grazioso	51
738	I. Allegro con brio	51
739	II. Adagio	51
740	III. Schrezo, Allegro	51
741	IV. Allegro assai	51
742	I. Allegro molto e con brio	51
743	II. Largo con gran espressione	51
744	III. Allegretto	51
745	IV. Rondo, Poco allegretto e grazioso	51
746	I. Allegro molto e con brio	51
747	II. Adagio molto	51
748	III. Finale, Pretissimmo	51
749	I. Allegro	51
750	II. Menuetto, Allegretto	51
751	III. Presto	51
752	I. Presto	51
753	II. Largo e mesto	51
754	III. Menuetto, Allegro	51
755	IV. Rondo, Allegro	51
756	I. Grave, Allegro di molto e con brio	51
757	II. Adagio cantabile	51
758	III. Rondo, Allegro	51
759	I. Allegro	51
760	II. Allegretto	51
761	III. Rondo, Allegro comodo	51
762	I. Allegro	51
763	II. Andante	51
764	III. Schrezo, Allegro assai	51
765	I. Allegro con brio	51
766	II. Adagio con molta espressione	51
767	III. Menuetto	51
768	IV. Rondo, Allegretto	51
769	I. Andante con variazioni	51
770	II. Schrezo, allegro molto	51
771	III. Maestoso andante marcia funebre sulta d'un eroe	51
772	IV. Allegro Rondo	51
773	I. Andante	51
774	II. Allegro molto e vivace	51
775	III. Adagio con espressione	51
776	IV. Allegro vivace	51
777	I. Adagio sostenuto	51
778	II. Allegretto	51
779	III. Presto agitato	51
780	I. Allegro	51
781	II. Andante	51
782	III. Schrezo, Allegro vivace	51
783	IV. Rondo, Allegro ma non troppo	51
784	I. Allegro vivace	51
785	II. Adagio grazioso	51
786	III. Rondo, Allegretto	51
787	I. Largo, Allegro	51
788	II. Adagio	51
789	III. Allegretto	51
790	I. Allegro	51
791	II. Scherzo. Allegretto vivace	51
792	III. Menuetto. Moderato e grazioso	51
793	IV. Presto con fuoco	51
794	I. Andante	51
795	II. Rondo. Allegro	51
796	I. Allegro ma non troppo	51
797	II. Tempo di menuetto	51
798	I. Allegro con brio	51
799	II. Introduzione. Adagio molto	51
800	III. Rondo. Allegretto moderato	51
801	I. In tempo d'un Menuetto	51
802	II. Allegretto	51
803	I. Allegro assai	51
804	II. Andante con molto	51
805	III. Allegro ma non troppo	51
806	I. Adagio cantabile. Allegro ma non troppo	51
807	II. Allegro vivace	51
808	I. Presto alia tedsesca	51
809	II. Andante	51
810	III. Vivace	51
811	I. Adagio, Allegro	51
812	II. Andante espressivo	51
813	III. Vivacissimamente	51
814	I. Mit Lebhaftigkeit und durchaus mit Empfindung und Ausdruck (Con vivacita e sempre con sentimento ed espressione)	51
815	II. Nicht zu geschwind und sehr singbar vorzutragen (Non troppo vivace e cantabile assai)	51
816	I. Etwas lebhaft, und mit der inngsten Empfindung. (Allegretto, ma non troppo)	51
817	II. Lebhaft. Marschmaessig. (Vivace alla marcia)	51
818	III. Langsam und sehnsuchtsvoll. (Adagio, ma non troppo, con affetto)	51
819	IV. Geschwind, doch nicht zu sehr und mit Entschlossenheit. (Allegro)	51
820	I. Allegro	51
821	II. Scherzo, assai vivace	51
822	III. Adagio sostenuto. Appassionato e con molto sentimento	51
823	IV. Largo, Allegro risoluto	51
824	I. Vivace ma non troppo, Adagio expressivo	51
825	II. Prestissimo	51
826	III. Andante, molto cantabile con espressivo	51
827	I. Moderato cantabile molto espressivo	51
828	II. Allegro molto	51
829	III. Adagio, man non troppo, Fuga. Allegro, ma non troppo	51
830	I. Maestoso Allegro con brio ed appassionato	51
831	II. Arietta Adagio molto, semplice e cantabile	51
832	Etude No. 01	51
833	Etude No. 02	51
834	Etude No. 03	51
835	Etude No. 04	51
836	Etude No. 05	51
837	Etude No. 06	51
838	Etude No. 07	51
839	Etude No. 08	51
840	Etude No. 09	51
841	Etude No. 10	51
842	Etude No. 11	51
843	Etude No. 12	51
\.


--
-- Name: Альбомы_Код_seq; Type: SEQUENCE SET; Schema: Music; Owner: postgres
--

SELECT pg_catalog.setval('"Music"."Альбомы_Код_seq"', 1, false);


--
-- Name: Жанры_Код_seq; Type: SEQUENCE SET; Schema: Music; Owner: postgres
--

SELECT pg_catalog.setval('"Music"."Жанры_Код_seq"', 11, true);


--
-- Name: Жанры_и_исполнители_Код_seq; Type: SEQUENCE SET; Schema: Music; Owner: postgres
--

SELECT pg_catalog.setval('"Music"."Жанры_и_исполнители_Код_seq"', 1, false);


--
-- Name: Исполнители_Код_seq; Type: SEQUENCE SET; Schema: Music; Owner: postgres
--

SELECT pg_catalog.setval('"Music"."Исполнители_Код_seq"', 8, true);


--
-- Name: Композиции_Код_seq; Type: SEQUENCE SET; Schema: Music; Owner: postgres
--

SELECT pg_catalog.setval('"Music"."Композиции_Код_seq"', 92, true);


--
-- Name: Композиции_и_исполнители_Код_seq; Type: SEQUENCE SET; Schema: Music; Owner: postgres
--

SELECT pg_catalog.setval('"Music"."Композиции_и_исполнители_Код_seq"', 306, true);


--
-- Name: categories_id_seq; Type: SEQUENCE SET; Schema: Practice; Owner: postgres
--

SELECT pg_catalog.setval('"Practice".categories_id_seq', 1, false);


--
-- Name: limits_id_seq; Type: SEQUENCE SET; Schema: Practice; Owner: postgres
--

SELECT pg_catalog.setval('"Practice".limits_id_seq', 1, false);


--
-- Name: recipient_id_seq; Type: SEQUENCE SET; Schema: Practice; Owner: postgres
--

SELECT pg_catalog.setval('"Practice".recipient_id_seq', 1, false);


--
-- Name: roles_id_seq; Type: SEQUENCE SET; Schema: Practice; Owner: postgres
--

SELECT pg_catalog.setval('"Practice".roles_id_seq', 1, false);


--
-- Name: transactions_id_seq; Type: SEQUENCE SET; Schema: Practice; Owner: postgres
--

SELECT pg_catalog.setval('"Practice".transactions_id_seq', 68, true);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: Practice; Owner: postgres
--

SELECT pg_catalog.setval('"Practice".users_id_seq', 1, false);


--
-- Name: authors_id_seq; Type: SEQUENCE SET; Schema: Study; Owner: postgres
--

SELECT pg_catalog.setval('"Study".authors_id_seq', 1, false);


--
-- Name: circles_circle_id_seq; Type: SEQUENCE SET; Schema: bilet1; Owner: postgres
--

SELECT pg_catalog.setval('bilet1.circles_circle_id_seq', 5, true);


--
-- Name: leaders_leader_id_seq; Type: SEQUENCE SET; Schema: bilet1; Owner: postgres
--

SELECT pg_catalog.setval('bilet1.leaders_leader_id_seq', 5, true);


--
-- Name: visits_visit_id_seq; Type: SEQUENCE SET; Schema: bilet1; Owner: postgres
--

SELECT pg_catalog.setval('bilet1.visits_visit_id_seq', 6, true);


--
-- Name: administrative_protocol_id_protocol_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.administrative_protocol_id_protocol_seq', 17, true);


--
-- Name: albums_album_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.albums_album_id_seq', 51, true);


--
-- Name: appeals_id_appeals_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.appeals_id_appeals_seq', 35, true);


--
-- Name: article_id_article_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.article_id_article_seq', 1, false);


--
-- Name: articles_and_responsobility_id_articles_and_responsibility_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.articles_and_responsobility_id_articles_and_responsibility_seq', 1, false);


--
-- Name: artists_artist_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.artists_artist_id_seq', 9, true);


--
-- Name: citizen_phones_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.citizen_phones_id_seq', 53, true);


--
-- Name: citizens_and_posts_id_citizens_and_posts_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.citizens_and_posts_id_citizens_and_posts_seq', 17, true);


--
-- Name: citizens_id_citizens_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.citizens_id_citizens_seq', 32, true);


--
-- Name: citizenship_id_citizenship_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.citizenship_id_citizenship_seq', 1, false);


--
-- Name: deal_id_deal_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.deal_id_deal_seq', 15, true);


--
-- Name: document_access_requests_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.document_access_requests_id_seq', 26, true);


--
-- Name: documents_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.documents_type_id_seq', 1, false);


--
-- Name: drafts_id_draft_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.drafts_id_draft_seq', 70, true);


--
-- Name: education_id_education_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.education_id_education_seq', 1, false);


--
-- Name: explanation_protocol_id_explanation_protocol_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.explanation_protocol_id_explanation_protocol_seq', 8, true);


--
-- Name: family_status_id_family_status_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.family_status_id_family_status_seq', 1, false);


--
-- Name: forensic_medical_examination_id_forensic_medical_examinatio_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.forensic_medical_examination_id_forensic_medical_examinatio_seq', 14, true);


--
-- Name: medical_examination_certifica_id_medical_examination_certif_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.medical_examination_certifica_id_medical_examination_certif_seq', 12, true);


--
-- Name: medical_examination_report_id_medical_examination_report_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.medical_examination_report_id_medical_examination_report_seq', 23, true);


--
-- Name: post_id_post_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.post_id_post_seq', 1, false);


--
-- Name: resolution_id_resolution_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.resolution_id_resolution_seq', 7, true);


--
-- Name: responsibility_id_responsibility_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.responsibility_id_responsibility_seq', 1, false);


--
-- Name: settlements_id_settlements_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.settlements_id_settlements_seq', 1, false);


--
-- Name: songs_song_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.songs_song_id_seq', 1686, true);


--
-- Name: statement_id_statement_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.statement_id_statement_seq', 10, true);


--
-- Name: structures_id_structures_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.structures_id_structures_seq', 1, false);


--
-- Name: type_intoxication_id_type_intoxication_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.type_intoxication_id_type_intoxication_seq', 1, false);


--
-- Name: type_of_punishment_id_type_of_punishment_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.type_of_punishment_id_type_of_punishment_seq', 1, false);


--
-- Name: type_report_id_type_report_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.type_report_id_type_report_seq', 3, true);


--
-- Name: user_citizen_link_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.user_citizen_link_id_seq', 9, true);


--
-- Name: user_favorites_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.user_favorites_id_seq', 277, true);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_id_seq', 5, true);


--
-- Name: albums_album_id_seq; Type: SEQUENCE SET; Schema: testdb; Owner: postgres
--

SELECT pg_catalog.setval('testdb.albums_album_id_seq', 51, true);


--
-- Name: artists_artist_id_seq; Type: SEQUENCE SET; Schema: testdb; Owner: postgres
--

SELECT pg_catalog.setval('testdb.artists_artist_id_seq', 9, true);


--
-- Name: rockgroups_notes_id_seq; Type: SEQUENCE SET; Schema: testdb; Owner: postgres
--

SELECT pg_catalog.setval('testdb.rockgroups_notes_id_seq', 30, true);


--
-- Name: songs_song_id_seq; Type: SEQUENCE SET; Schema: testdb; Owner: postgres
--

SELECT pg_catalog.setval('testdb.songs_song_id_seq', 843, true);


--
-- Name: Альбомы Альбомы_pkey; Type: CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Альбомы"
    ADD CONSTRAINT "Альбомы_pkey" PRIMARY KEY ("Код");


--
-- Name: Жанры Жанры_pkey; Type: CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Жанры"
    ADD CONSTRAINT "Жанры_pkey" PRIMARY KEY ("Код");


--
-- Name: Жанры_и_исполнители Жанры_и_исполнители_pkey; Type: CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Жанры_и_исполнители"
    ADD CONSTRAINT "Жанры_и_исполнители_pkey" PRIMARY KEY ("Код");


--
-- Name: Исполнители Исполнители_pkey; Type: CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Исполнители"
    ADD CONSTRAINT "Исполнители_pkey" PRIMARY KEY ("Код");


--
-- Name: Композиции Композиции_pkey; Type: CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Композиции"
    ADD CONSTRAINT "Композиции_pkey" PRIMARY KEY ("Код");


--
-- Name: Композиции_и_исполнители Композиции_и_исполнители_pkey; Type: CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Композиции_и_исполнители"
    ADD CONSTRAINT "Композиции_и_исполнители_pkey" PRIMARY KEY ("Код");


--
-- Name: categories categories_name_key; Type: CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".categories
    ADD CONSTRAINT categories_name_key UNIQUE (name);


--
-- Name: categories categories_pkey; Type: CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".categories
    ADD CONSTRAINT categories_pkey PRIMARY KEY (id);


--
-- Name: limits limits_pkey; Type: CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".limits
    ADD CONSTRAINT limits_pkey PRIMARY KEY (id);


--
-- Name: recipient recipient_pkey; Type: CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".recipient
    ADD CONSTRAINT recipient_pkey PRIMARY KEY (id);


--
-- Name: roles roles_pkey; Type: CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".roles
    ADD CONSTRAINT roles_pkey PRIMARY KEY (id);


--
-- Name: sender sender_pkey; Type: CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".sender
    ADD CONSTRAINT sender_pkey PRIMARY KEY (id);


--
-- Name: transactions transactions_pkey; Type: CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".transactions
    ADD CONSTRAINT transactions_pkey PRIMARY KEY (id);


--
-- Name: users users_login_key; Type: CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".users
    ADD CONSTRAINT users_login_key UNIQUE (login);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: article article_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".article
    ADD CONSTRAINT article_pkey PRIMARY KEY (id_article);


--
-- Name: articles_and_responsibility articles_and_responsibility_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".articles_and_responsibility
    ADD CONSTRAINT articles_and_responsibility_pkey PRIMARY KEY (id_articles_and_responsibility);


--
-- Name: citizens_and_posts citizens_and_posts_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".citizens_and_posts
    ADD CONSTRAINT citizens_and_posts_pkey PRIMARY KEY (id_citizens_and_posts);


--
-- Name: citizens citizens_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".citizens
    ADD CONSTRAINT citizens_pkey PRIMARY KEY (id_citizen);


--
-- Name: family_status family_status_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".family_status
    ADD CONSTRAINT family_status_pkey PRIMARY KEY (id_family_status);


--
-- Name: medical_examination_report medical_examination_report_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".medical_examination_report
    ADD CONSTRAINT medical_examination_report_pkey PRIMARY KEY (id_medical_examination_report);


--
-- Name: post post_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".post
    ADD CONSTRAINT post_pkey PRIMARY KEY (id_post);


--
-- Name: protocol protocol_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".protocol
    ADD CONSTRAINT protocol_pkey PRIMARY KEY (id_protocol);


--
-- Name: resolution resolution_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".resolution
    ADD CONSTRAINT resolution_pkey PRIMARY KEY (id_resolution);


--
-- Name: responsibility responsibility_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".responsibility
    ADD CONSTRAINT responsibility_pkey PRIMARY KEY (id_responsibility);


--
-- Name: settlements settlements_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".settlements
    ADD CONSTRAINT settlements_pkey PRIMARY KEY (id_settlement);


--
-- Name: structures structures_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".structures
    ADD CONSTRAINT structures_pkey PRIMARY KEY (id_structure);


--
-- Name: type_of_face type_of_face_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".type_of_face
    ADD CONSTRAINT type_of_face_pkey PRIMARY KEY (id_type_of_face);


--
-- Name: type_of_punishment type_of_punishment_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".type_of_punishment
    ADD CONSTRAINT type_of_punishment_pkey PRIMARY KEY (id_type_of_punishment);


--
-- Name: type_of_report type_of_report_pkey; Type: CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".type_of_report
    ADD CONSTRAINT type_of_report_pkey PRIMARY KEY (id_type_of_report);


--
-- Name: authors authors_pkey; Type: CONSTRAINT; Schema: Study; Owner: postgres
--

ALTER TABLE ONLY "Study".authors
    ADD CONSTRAINT authors_pkey PRIMARY KEY (id);


--
-- Name: circles circles_pkey; Type: CONSTRAINT; Schema: bilet1; Owner: postgres
--

ALTER TABLE ONLY bilet1.circles
    ADD CONSTRAINT circles_pkey PRIMARY KEY (circle_id);


--
-- Name: leaders leaders_pkey; Type: CONSTRAINT; Schema: bilet1; Owner: postgres
--

ALTER TABLE ONLY bilet1.leaders
    ADD CONSTRAINT leaders_pkey PRIMARY KEY (leader_id);


--
-- Name: visits visits_pkey; Type: CONSTRAINT; Schema: bilet1; Owner: postgres
--

ALTER TABLE ONLY bilet1.visits
    ADD CONSTRAINT visits_pkey PRIMARY KEY (visit_id);


--
-- Name: administrative_protocol administrative_protocol_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.administrative_protocol
    ADD CONSTRAINT administrative_protocol_pkey PRIMARY KEY (id_protocol);


--
-- Name: albums albums_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.albums
    ADD CONSTRAINT albums_pkey PRIMARY KEY (album_id);


--
-- Name: appeals appeals_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.appeals
    ADD CONSTRAINT appeals_pkey PRIMARY KEY (id_appeals);


--
-- Name: article article_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.article
    ADD CONSTRAINT article_pkey PRIMARY KEY (id_article);


--
-- Name: articles_and_responsobility articles_and_responsobility_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.articles_and_responsobility
    ADD CONSTRAINT articles_and_responsobility_pkey PRIMARY KEY (id_articles_and_responsibility);


--
-- Name: artists artists_artist_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.artists
    ADD CONSTRAINT artists_artist_name_key UNIQUE (artist_name);


--
-- Name: artists artists_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.artists
    ADD CONSTRAINT artists_pkey PRIMARY KEY (artist_id);


--
-- Name: cap_ranks cap_ranks_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cap_ranks
    ADD CONSTRAINT cap_ranks_pkey PRIMARY KEY (id);


--
-- Name: citizen_phones citizen_phones_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizen_phones
    ADD CONSTRAINT citizen_phones_pkey PRIMARY KEY (id);


--
-- Name: citizens_and_posts citizens_and_posts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizens_and_posts
    ADD CONSTRAINT citizens_and_posts_pkey PRIMARY KEY (id_citizens_and_posts);


--
-- Name: citizens citizens_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizens
    ADD CONSTRAINT citizens_pkey PRIMARY KEY (id_citizens);


--
-- Name: citizenship citizenship_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizenship
    ADD CONSTRAINT citizenship_pkey PRIMARY KEY (id_citizenship);


--
-- Name: deal deal_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deal
    ADD CONSTRAINT deal_pkey PRIMARY KEY (id_deal);


--
-- Name: document_access_requests document_access_requests_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.document_access_requests
    ADD CONSTRAINT document_access_requests_pkey PRIMARY KEY (id);


--
-- Name: documents_type documents_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.documents_type
    ADD CONSTRAINT documents_type_pkey PRIMARY KEY (id);


--
-- Name: drafts drafts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.drafts
    ADD CONSTRAINT drafts_pkey PRIMARY KEY (id_draft);


--
-- Name: education education_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.education
    ADD CONSTRAINT education_pkey PRIMARY KEY (id_education);


--
-- Name: explanation_protocol explanation_protocol_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.explanation_protocol
    ADD CONSTRAINT explanation_protocol_pkey PRIMARY KEY (id_explanation_protocol);


--
-- Name: family_status family_status_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.family_status
    ADD CONSTRAINT family_status_pkey PRIMARY KEY (id_family_status);


--
-- Name: forensic_medical_examination forensic_medical_examination_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.forensic_medical_examination
    ADD CONSTRAINT forensic_medical_examination_pkey PRIMARY KEY (id_forensic_medical_examination);


--
-- Name: medical_examination_certificate medical_examination_certificate_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medical_examination_certificate
    ADD CONSTRAINT medical_examination_certificate_pkey PRIMARY KEY (id_medical_examination_certificate);


--
-- Name: medical_examination_report medical_examination_report_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medical_examination_report
    ADD CONSTRAINT medical_examination_report_pkey PRIMARY KEY (id_medical_examination_report);


--
-- Name: post post_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.post
    ADD CONSTRAINT post_pkey PRIMARY KEY (id_post);


--
-- Name: rank rank_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rank
    ADD CONSTRAINT rank_pkey PRIMARY KEY (id);


--
-- Name: resolution resolution_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.resolution
    ADD CONSTRAINT resolution_pkey PRIMARY KEY (id_resolution);


--
-- Name: responsibility responsibility_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.responsibility
    ADD CONSTRAINT responsibility_pkey PRIMARY KEY (id_responsibility);


--
-- Name: roles roles_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_pkey PRIMARY KEY (id);


--
-- Name: settlements settlements_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.settlements
    ADD CONSTRAINT settlements_pkey PRIMARY KEY (id_settlements);


--
-- Name: songs songs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.songs
    ADD CONSTRAINT songs_pkey PRIMARY KEY (song_id);


--
-- Name: statement statement_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statement
    ADD CONSTRAINT statement_pkey PRIMARY KEY (id_statement);


--
-- Name: structures structures_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.structures
    ADD CONSTRAINT structures_pkey PRIMARY KEY (id_structures);


--
-- Name: type_intoxication type_intoxication_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.type_intoxication
    ADD CONSTRAINT type_intoxication_pkey PRIMARY KEY (id_type_intoxication);


--
-- Name: type_of_punishment type_of_punishment_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.type_of_punishment
    ADD CONSTRAINT type_of_punishment_pkey PRIMARY KEY (id_type_of_punishment);


--
-- Name: type_report type_report_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.type_report
    ADD CONSTRAINT type_report_pkey PRIMARY KEY (id_type_report);


--
-- Name: user_citizen_link user_citizen_link_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_citizen_link
    ADD CONSTRAINT user_citizen_link_pkey PRIMARY KEY (id);


--
-- Name: user_citizen_link user_citizen_link_user_id_citizen_post_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_citizen_link
    ADD CONSTRAINT user_citizen_link_user_id_citizen_post_id_key UNIQUE (user_id, citizen_post_id);


--
-- Name: user_favorites user_favorites_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_favorites
    ADD CONSTRAINT user_favorites_pkey PRIMARY KEY (id);


--
-- Name: user_favorites user_favorites_user_id_target_table_document_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_favorites
    ADD CONSTRAINT user_favorites_user_id_target_table_document_id_key UNIQUE (user_id, target_table, document_id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: users users_username_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);


--
-- Name: albums albums_pkey; Type: CONSTRAINT; Schema: testdb; Owner: postgres
--

ALTER TABLE ONLY testdb.albums
    ADD CONSTRAINT albums_pkey PRIMARY KEY (album_id);


--
-- Name: artists artists_artist_name_key; Type: CONSTRAINT; Schema: testdb; Owner: postgres
--

ALTER TABLE ONLY testdb.artists
    ADD CONSTRAINT artists_artist_name_key UNIQUE (artist_name);


--
-- Name: artists artists_pkey; Type: CONSTRAINT; Schema: testdb; Owner: postgres
--

ALTER TABLE ONLY testdb.artists
    ADD CONSTRAINT artists_pkey PRIMARY KEY (artist_id);


--
-- Name: rockgroups_notes rockgroups_notes_pkey; Type: CONSTRAINT; Schema: testdb; Owner: postgres
--

ALTER TABLE ONLY testdb.rockgroups_notes
    ADD CONSTRAINT rockgroups_notes_pkey PRIMARY KEY (id);


--
-- Name: songs songs_pkey; Type: CONSTRAINT; Schema: testdb; Owner: postgres
--

ALTER TABLE ONLY testdb.songs
    ADD CONSTRAINT songs_pkey PRIMARY KEY (song_id);


--
-- Name: idx_citizen_phones_citizen; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_citizen_phones_citizen ON public.citizen_phones USING btree (citizen);


--
-- Name: idx_citizen_phones_number; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_citizen_phones_number ON public.citizen_phones USING btree (phone_number);


--
-- Name: transactions check_daily_spending_trigger; Type: TRIGGER; Schema: Practice; Owner: postgres
--

CREATE TRIGGER check_daily_spending_trigger BEFORE INSERT ON "Practice".transactions FOR EACH ROW EXECUTE FUNCTION "Practice".check_daily_spending_limit();


--
-- Name: transactions regular_payments_trigger; Type: TRIGGER; Schema: Practice; Owner: postgres
--

CREATE TRIGGER regular_payments_trigger BEFORE INSERT ON "Practice".transactions FOR EACH ROW EXECUTE FUNCTION "Practice".regular_payments_reminder();


--
-- Name: protocol tr_check_offender_age; Type: TRIGGER; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TRIGGER tr_check_offender_age BEFORE INSERT OR UPDATE ON "Practice 11/30/2025".protocol FOR EACH ROW EXECUTE FUNCTION "Practice 11/30/2025".check_offender_age();


--
-- Name: protocol tr_check_witness_age; Type: TRIGGER; Schema: Practice 11/30/2025; Owner: postgres
--

CREATE TRIGGER tr_check_witness_age BEFORE INSERT OR UPDATE ON "Practice 11/30/2025".protocol FOR EACH ROW EXECUTE FUNCTION "Practice 11/30/2025".check_witness_age();


--
-- Name: deal increment_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER increment_trigger AFTER INSERT ON public.deal FOR EACH ROW EXECUTE FUNCTION public.increment();


--
-- Name: deal increment_trigger1; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER increment_trigger1 AFTER INSERT ON public.deal FOR EACH ROW EXECUTE FUNCTION public.increment_function();


--
-- Name: Альбомы Альбомы_Жанр_fkey; Type: FK CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Альбомы"
    ADD CONSTRAINT "Альбомы_Жанр_fkey" FOREIGN KEY ("Жанр") REFERENCES "Music"."Жанры"("Код");


--
-- Name: Альбомы Альбомы_Исполнитель_fkey; Type: FK CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Альбомы"
    ADD CONSTRAINT "Альбомы_Исполнитель_fkey" FOREIGN KEY ("Исполнитель") REFERENCES "Music"."Исполнители"("Код");


--
-- Name: Жанры_и_исполнители Жанры_и_исполнител_Исполнитель_fkey; Type: FK CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Жанры_и_исполнители"
    ADD CONSTRAINT "Жанры_и_исполнител_Исполнитель_fkey" FOREIGN KEY ("Исполнитель") REFERENCES "Music"."Исполнители"("Код");


--
-- Name: Жанры_и_исполнители Жанры_и_исполнители_Жанр_fkey; Type: FK CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Жанры_и_исполнители"
    ADD CONSTRAINT "Жанры_и_исполнители_Жанр_fkey" FOREIGN KEY ("Жанр") REFERENCES "Music"."Жанры"("Код");


--
-- Name: Композиции Композиции_Жанр_fkey; Type: FK CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Композиции"
    ADD CONSTRAINT "Композиции_Жанр_fkey" FOREIGN KEY ("Жанр") REFERENCES "Music"."Жанры"("Код");


--
-- Name: Композиции_и_исполнители Композиции_и_испол_Исполнитель_fkey; Type: FK CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Композиции_и_исполнители"
    ADD CONSTRAINT "Композиции_и_испол_Исполнитель_fkey" FOREIGN KEY ("Исполнитель") REFERENCES "Music"."Исполнители"("Код");


--
-- Name: Композиции_и_исполнители Композиции_и_исполн_Композиция_fkey; Type: FK CONSTRAINT; Schema: Music; Owner: postgres
--

ALTER TABLE ONLY "Music"."Композиции_и_исполнители"
    ADD CONSTRAINT "Композиции_и_исполн_Композиция_fkey" FOREIGN KEY ("Композиция") REFERENCES "Music"."Композиции"("Код");


--
-- Name: limits limits_user_id_fkey; Type: FK CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".limits
    ADD CONSTRAINT limits_user_id_fkey FOREIGN KEY (user_id) REFERENCES "Practice".users(id);


--
-- Name: recipient recipient_user_id_fkey; Type: FK CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".recipient
    ADD CONSTRAINT recipient_user_id_fkey FOREIGN KEY (user_id) REFERENCES "Practice".users(id);


--
-- Name: transactions transactions_category_id_fkey; Type: FK CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".transactions
    ADD CONSTRAINT transactions_category_id_fkey FOREIGN KEY (category_id) REFERENCES "Practice".categories(id);


--
-- Name: transactions transactions_recipient_id_fkey; Type: FK CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".transactions
    ADD CONSTRAINT transactions_recipient_id_fkey FOREIGN KEY (recipient_id) REFERENCES "Practice".recipient(id);


--
-- Name: transactions transactions_sender_id_fkey; Type: FK CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".transactions
    ADD CONSTRAINT transactions_sender_id_fkey FOREIGN KEY (sender_id) REFERENCES "Practice".sender(id);


--
-- Name: transactions transactions_user_id1_fkey; Type: FK CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".transactions
    ADD CONSTRAINT transactions_user_id1_fkey FOREIGN KEY (user_id1) REFERENCES "Practice".users(id);


--
-- Name: transactions transactions_user_id2_fkey; Type: FK CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".transactions
    ADD CONSTRAINT transactions_user_id2_fkey FOREIGN KEY (user_id2) REFERENCES "Practice".users(id);


--
-- Name: users users_role_fkey; Type: FK CONSTRAINT; Schema: Practice; Owner: postgres
--

ALTER TABLE ONLY "Practice".users
    ADD CONSTRAINT users_role_fkey FOREIGN KEY (role) REFERENCES "Practice".roles(id);


--
-- Name: articles_and_responsibility articles_and_responsibility_article_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".articles_and_responsibility
    ADD CONSTRAINT articles_and_responsibility_article_fkey FOREIGN KEY (article) REFERENCES "Practice 11/30/2025".article(id_article);


--
-- Name: articles_and_responsibility articles_and_responsibility_responsibility_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".articles_and_responsibility
    ADD CONSTRAINT articles_and_responsibility_responsibility_fkey FOREIGN KEY (responsibility) REFERENCES "Practice 11/30/2025".responsibility(id_responsibility);


--
-- Name: citizens_and_posts citizens_and_posts_citizen_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".citizens_and_posts
    ADD CONSTRAINT citizens_and_posts_citizen_fkey FOREIGN KEY (citizen) REFERENCES "Practice 11/30/2025".citizens(id_citizen);


--
-- Name: citizens_and_posts citizens_and_posts_post_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".citizens_and_posts
    ADD CONSTRAINT citizens_and_posts_post_fkey FOREIGN KEY (post) REFERENCES "Practice 11/30/2025".post(id_post);


--
-- Name: citizens citizens_family_status_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".citizens
    ADD CONSTRAINT citizens_family_status_fkey FOREIGN KEY (family_status) REFERENCES "Practice 11/30/2025".family_status(id_family_status);


--
-- Name: citizens citizens_post_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".citizens
    ADD CONSTRAINT citizens_post_fkey FOREIGN KEY (post) REFERENCES "Practice 11/30/2025".post(id_post);


--
-- Name: citizens citizens_settlement_citizen_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".citizens
    ADD CONSTRAINT citizens_settlement_citizen_fkey FOREIGN KEY (settlement_citizen) REFERENCES "Practice 11/30/2025".settlements(id_settlement);


--
-- Name: citizens citizens_work_place_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".citizens
    ADD CONSTRAINT citizens_work_place_fkey FOREIGN KEY (work_place) REFERENCES "Practice 11/30/2025".structures(id_structure);


--
-- Name: medical_examination_report medical_examination_report_first_witness_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".medical_examination_report
    ADD CONSTRAINT medical_examination_report_first_witness_fkey FOREIGN KEY (first_witness) REFERENCES "Practice 11/30/2025".citizens(id_citizen);


--
-- Name: medical_examination_report medical_examination_report_hospital_staff_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".medical_examination_report
    ADD CONSTRAINT medical_examination_report_hospital_staff_fkey FOREIGN KEY (hospital_staff) REFERENCES "Practice 11/30/2025".citizens_and_posts(id_citizens_and_posts);


--
-- Name: medical_examination_report medical_examination_report_patient_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".medical_examination_report
    ADD CONSTRAINT medical_examination_report_patient_fkey FOREIGN KEY (patient) REFERENCES "Practice 11/30/2025".citizens(id_citizen);


--
-- Name: medical_examination_report medical_examination_report_police_officers_in_report_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".medical_examination_report
    ADD CONSTRAINT medical_examination_report_police_officers_in_report_fkey FOREIGN KEY (police_officers_in_report) REFERENCES "Practice 11/30/2025".citizens_and_posts(id_citizens_and_posts);


--
-- Name: medical_examination_report medical_examination_report_report_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".medical_examination_report
    ADD CONSTRAINT medical_examination_report_report_fkey FOREIGN KEY (report) REFERENCES "Practice 11/30/2025".type_of_report(id_type_of_report);


--
-- Name: medical_examination_report medical_examination_report_second_witness_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".medical_examination_report
    ADD CONSTRAINT medical_examination_report_second_witness_fkey FOREIGN KEY (second_witness) REFERENCES "Practice 11/30/2025".citizens(id_citizen);


--
-- Name: medical_examination_report medical_examination_report_settlements_report_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".medical_examination_report
    ADD CONSTRAINT medical_examination_report_settlements_report_fkey FOREIGN KEY (settlements_report) REFERENCES "Practice 11/30/2025".settlements(id_settlement);


--
-- Name: protocol protocol_article_of_protocol_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".protocol
    ADD CONSTRAINT protocol_article_of_protocol_fkey FOREIGN KEY (article_of_protocol) REFERENCES "Practice 11/30/2025".article(id_article);


--
-- Name: protocol protocol_first_witness_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".protocol
    ADD CONSTRAINT protocol_first_witness_fkey FOREIGN KEY (first_witness) REFERENCES "Practice 11/30/2025".citizens(id_citizen);


--
-- Name: protocol protocol_offender_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".protocol
    ADD CONSTRAINT protocol_offender_fkey FOREIGN KEY (offender) REFERENCES "Practice 11/30/2025".citizens(id_citizen);


--
-- Name: protocol protocol_police_officers_in_protocol_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".protocol
    ADD CONSTRAINT protocol_police_officers_in_protocol_fkey FOREIGN KEY (police_officers_in_protocol) REFERENCES "Practice 11/30/2025".citizens_and_posts(id_citizens_and_posts);


--
-- Name: protocol protocol_second_witness_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".protocol
    ADD CONSTRAINT protocol_second_witness_fkey FOREIGN KEY (second_witness) REFERENCES "Practice 11/30/2025".citizens(id_citizen);


--
-- Name: protocol protocol_settlement_of_making_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".protocol
    ADD CONSTRAINT protocol_settlement_of_making_fkey FOREIGN KEY (settlement_of_making) REFERENCES "Practice 11/30/2025".settlements(id_settlement);


--
-- Name: resolution resolution_court_staff_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".resolution
    ADD CONSTRAINT resolution_court_staff_fkey FOREIGN KEY (court_staff) REFERENCES "Practice 11/30/2025".citizens_and_posts(id_citizens_and_posts);


--
-- Name: resolution resolution_id_article_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".resolution
    ADD CONSTRAINT resolution_id_article_fkey FOREIGN KEY (id_article) REFERENCES "Practice 11/30/2025".article(id_article);


--
-- Name: resolution resolution_id_responsibility_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".resolution
    ADD CONSTRAINT resolution_id_responsibility_fkey FOREIGN KEY (id_responsibility) REFERENCES "Practice 11/30/2025".responsibility(id_responsibility);


--
-- Name: resolution resolution_kdm_employee_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".resolution
    ADD CONSTRAINT resolution_kdm_employee_fkey FOREIGN KEY (kdm_employee) REFERENCES "Practice 11/30/2025".citizens_and_posts(id_citizens_and_posts);


--
-- Name: resolution resolution_number_of_protocol_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".resolution
    ADD CONSTRAINT resolution_number_of_protocol_fkey FOREIGN KEY (number_of_protocol) REFERENCES "Practice 11/30/2025".protocol(id_protocol);


--
-- Name: resolution resolution_punishment_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".resolution
    ADD CONSTRAINT resolution_punishment_fkey FOREIGN KEY (punishment) REFERENCES "Practice 11/30/2025".type_of_punishment(id_type_of_punishment);


--
-- Name: resolution resolution_settlements_resolution_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".resolution
    ADD CONSTRAINT resolution_settlements_resolution_fkey FOREIGN KEY (settlements_resolution) REFERENCES "Practice 11/30/2025".settlements(id_settlement);


--
-- Name: structures structures_settlement_structures_fkey; Type: FK CONSTRAINT; Schema: Practice 11/30/2025; Owner: postgres
--

ALTER TABLE ONLY "Practice 11/30/2025".structures
    ADD CONSTRAINT structures_settlement_structures_fkey FOREIGN KEY (settlement_structures) REFERENCES "Practice 11/30/2025".settlements(id_settlement);


--
-- Name: visits visits_leader_id_fkey; Type: FK CONSTRAINT; Schema: bilet1; Owner: postgres
--

ALTER TABLE ONLY bilet1.visits
    ADD CONSTRAINT visits_leader_id_fkey FOREIGN KEY (leader_id) REFERENCES bilet1.leaders(leader_id) ON DELETE CASCADE;


--
-- Name: administrative_protocol administrative_protocol_deal_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.administrative_protocol
    ADD CONSTRAINT administrative_protocol_deal_fkey FOREIGN KEY (deal) REFERENCES public.deal(id_deal);


--
-- Name: albums albums_artist_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.albums
    ADD CONSTRAINT albums_artist_id_fkey FOREIGN KEY (artist_id) REFERENCES public.artists(artist_id);


--
-- Name: appeals appeals_police_officer_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.appeals
    ADD CONSTRAINT appeals_police_officer_fkey FOREIGN KEY (police_officer) REFERENCES public.citizens_and_posts(id_citizens_and_posts);


--
-- Name: articles_and_responsobility articles_and_responsobility_article_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.articles_and_responsobility
    ADD CONSTRAINT articles_and_responsobility_article_fkey FOREIGN KEY (article) REFERENCES public.article(id_article);


--
-- Name: articles_and_responsobility articles_and_responsobility_responsibility_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.articles_and_responsobility
    ADD CONSTRAINT articles_and_responsobility_responsibility_fkey FOREIGN KEY (responsibility) REFERENCES public.responsibility(id_responsibility);


--
-- Name: cap_ranks cap_ranks_rank_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cap_ranks
    ADD CONSTRAINT cap_ranks_rank_fkey FOREIGN KEY (rank) REFERENCES public.rank(id);


--
-- Name: cap_ranks cap_ranks_user_citizen_link_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cap_ranks
    ADD CONSTRAINT cap_ranks_user_citizen_link_fkey FOREIGN KEY (user_citizen_link) REFERENCES public.user_citizen_link(id);


--
-- Name: citizens_and_posts citizens_and_posts_citizen_post_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizens_and_posts
    ADD CONSTRAINT citizens_and_posts_citizen_post_fkey FOREIGN KEY (citizen_post) REFERENCES public.post(id_post);


--
-- Name: citizens citizens_citizenship_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizens
    ADD CONSTRAINT citizens_citizenship_fkey FOREIGN KEY (citizenship) REFERENCES public.citizenship(id_citizenship);


--
-- Name: citizens citizens_education_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizens
    ADD CONSTRAINT citizens_education_fkey FOREIGN KEY (education) REFERENCES public.education(id_education);


--
-- Name: citizens citizens_family_status_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizens
    ADD CONSTRAINT citizens_family_status_fkey FOREIGN KEY (family_status) REFERENCES public.family_status(id_family_status);


--
-- Name: citizens citizens_post_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizens
    ADD CONSTRAINT citizens_post_fkey FOREIGN KEY (post) REFERENCES public.post(id_post);


--
-- Name: citizens citizens_working_place_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.citizens
    ADD CONSTRAINT citizens_working_place_fkey FOREIGN KEY (working_place) REFERENCES public.structures(id_structures);


--
-- Name: deal deal_article_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deal
    ADD CONSTRAINT deal_article_fkey FOREIGN KEY (article) REFERENCES public.article(id_article);


--
-- Name: deal deal_police_officer_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deal
    ADD CONSTRAINT deal_police_officer_fkey FOREIGN KEY (police_officer) REFERENCES public.citizens_and_posts(id_citizens_and_posts);


--
-- Name: deal deal_responsibility_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deal
    ADD CONSTRAINT deal_responsibility_fkey FOREIGN KEY (responsibility) REFERENCES public.responsibility(id_responsibility);


--
-- Name: deal deal_settlement_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deal
    ADD CONSTRAINT deal_settlement_fkey FOREIGN KEY (settlement) REFERENCES public.settlements(id_settlements);


--
-- Name: document_access_requests document_access_requests_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.document_access_requests
    ADD CONSTRAINT document_access_requests_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: drafts drafts_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.drafts
    ADD CONSTRAINT drafts_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: explanation_protocol explanation_protocol_deal_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.explanation_protocol
    ADD CONSTRAINT explanation_protocol_deal_fkey FOREIGN KEY (deal) REFERENCES public.deal(id_deal);


--
-- Name: forensic_medical_examination fk_fe_deal; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.forensic_medical_examination
    ADD CONSTRAINT fk_fe_deal FOREIGN KEY (deal) REFERENCES public.deal(id_deal);


--
-- Name: medical_examination_report fk_mer_deal; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medical_examination_report
    ADD CONSTRAINT fk_mer_deal FOREIGN KEY (deal) REFERENCES public.deal(id_deal);


--
-- Name: forensic_medical_examination forensic_medical_examination_deal_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.forensic_medical_examination
    ADD CONSTRAINT forensic_medical_examination_deal_fkey FOREIGN KEY (deal) REFERENCES public.deal(id_deal);


--
-- Name: forensic_medical_examination forensic_medical_examination_expert_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.forensic_medical_examination
    ADD CONSTRAINT forensic_medical_examination_expert_fkey FOREIGN KEY (expert) REFERENCES public.citizens_and_posts(id_citizens_and_posts);


--
-- Name: forensic_medical_examination forensic_medical_examination_structure_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.forensic_medical_examination
    ADD CONSTRAINT forensic_medical_examination_structure_fkey FOREIGN KEY (structure) REFERENCES public.structures(id_structures);


--
-- Name: medical_examination_certificate medical_examination_certificate_doctor_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medical_examination_certificate
    ADD CONSTRAINT medical_examination_certificate_doctor_fkey FOREIGN KEY (doctor) REFERENCES public.citizens_and_posts(id_citizens_and_posts);


--
-- Name: medical_examination_certificate medical_examination_certificate_medical_institution_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medical_examination_certificate
    ADD CONSTRAINT medical_examination_certificate_medical_institution_fkey FOREIGN KEY (medical_institution) REFERENCES public.structures(id_structures);


--
-- Name: medical_examination_certificate medical_examination_certificate_type_intoxication_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medical_examination_certificate
    ADD CONSTRAINT medical_examination_certificate_type_intoxication_fkey FOREIGN KEY (type_intoxication) REFERENCES public.type_intoxication(id_type_intoxication);


--
-- Name: medical_examination_report medical_examination_report_deal_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medical_examination_report
    ADD CONSTRAINT medical_examination_report_deal_fkey FOREIGN KEY (deal) REFERENCES public.deal(id_deal);


--
-- Name: medical_examination_report medical_examination_report_patient_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medical_examination_report
    ADD CONSTRAINT medical_examination_report_patient_fkey FOREIGN KEY (patient) REFERENCES public.citizens(id_citizens);


--
-- Name: medical_examination_report medical_examination_report_report_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medical_examination_report
    ADD CONSTRAINT medical_examination_report_report_fkey FOREIGN KEY (report) REFERENCES public.type_report(id_type_report);


--
-- Name: resolution resolution_court_staff_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.resolution
    ADD CONSTRAINT resolution_court_staff_fkey FOREIGN KEY (court_staff) REFERENCES public.citizens_and_posts(id_citizens_and_posts);


--
-- Name: resolution resolution_deal_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.resolution
    ADD CONSTRAINT resolution_deal_fkey FOREIGN KEY (deal) REFERENCES public.deal(id_deal);


--
-- Name: resolution resolution_punishment_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.resolution
    ADD CONSTRAINT resolution_punishment_fkey FOREIGN KEY (punishment) REFERENCES public.type_of_punishment(id_type_of_punishment);


--
-- Name: songs songs_album_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.songs
    ADD CONSTRAINT songs_album_id_fkey FOREIGN KEY (album_id) REFERENCES public.albums(album_id);


--
-- Name: statement statement_police_officer_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statement
    ADD CONSTRAINT statement_police_officer_fkey FOREIGN KEY (police_officer) REFERENCES public.citizens_and_posts(id_citizens_and_posts);


--
-- Name: structures structures_settlement_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.structures
    ADD CONSTRAINT structures_settlement_fkey FOREIGN KEY (settlement) REFERENCES public.settlements(id_settlements);


--
-- Name: user_citizen_link user_citizen_link_citizen_post_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_citizen_link
    ADD CONSTRAINT user_citizen_link_citizen_post_id_fkey FOREIGN KEY (citizen_post_id) REFERENCES public.citizens_and_posts(id_citizens_and_posts);


--
-- Name: user_citizen_link user_citizen_link_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_citizen_link
    ADD CONSTRAINT user_citizen_link_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: user_favorites user_favorites_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_favorites
    ADD CONSTRAINT user_favorites_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: albums albums_artist_id_fkey; Type: FK CONSTRAINT; Schema: testdb; Owner: postgres
--

ALTER TABLE ONLY testdb.albums
    ADD CONSTRAINT albums_artist_id_fkey FOREIGN KEY (artist_id) REFERENCES testdb.artists(artist_id);


--
-- Name: songs songs_album_id_fkey; Type: FK CONSTRAINT; Schema: testdb; Owner: postgres
--

ALTER TABLE ONLY testdb.songs
    ADD CONSTRAINT songs_album_id_fkey FOREIGN KEY (album_id) REFERENCES testdb.albums(album_id);


--
-- Name: SCHEMA public; Type: ACL; Schema: -; Owner: pg_database_owner
--

GRANT CREATE ON SCHEMA public TO admin_role;


--
-- Name: FUNCTION create_medical_only(deal_id integer, citizen_id integer, need_medical boolean, need_forensic boolean); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.create_medical_only(deal_id integer, citizen_id integer, need_medical boolean, need_forensic boolean) TO admin_role;


--
-- Name: FUNCTION fill_medical_from_explanation(explanation_id integer); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.fill_medical_from_explanation(explanation_id integer) TO admin_role;


--
-- Name: FUNCTION trg_update_criminal_record_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.trg_update_criminal_record_function() TO admin_role;


--
-- Name: FUNCTION validate_admin_protocol_sequence_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_admin_protocol_sequence_function() TO admin_role;


--
-- Name: FUNCTION validate_date_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_date_function() TO admin_role;


--
-- Name: FUNCTION validate_deal_sequence_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_deal_sequence_function() TO admin_role;


--
-- Name: FUNCTION validate_explanation_date_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_explanation_date_function() TO admin_role;


--
-- Name: FUNCTION validate_explanation_protocol_sequence_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_explanation_protocol_sequence_function() TO admin_role;


--
-- Name: FUNCTION validate_forensic_exam_sequence_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_forensic_exam_sequence_function() TO admin_role;


--
-- Name: FUNCTION validate_forensic_examination_date_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_forensic_examination_date_function() TO admin_role;


--
-- Name: FUNCTION validate_medical_certificate_date_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_medical_certificate_date_function() TO admin_role;


--
-- Name: FUNCTION validate_medical_certificate_sequence_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_medical_certificate_sequence_function() TO admin_role;


--
-- Name: FUNCTION validate_medical_report_date_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_medical_report_date_function() TO admin_role;


--
-- Name: FUNCTION validate_medical_report_sequence_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_medical_report_sequence_function() TO admin_role;


--
-- Name: FUNCTION validate_offender_age_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_offender_age_function() TO admin_role;


--
-- Name: FUNCTION validate_resolution_date_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_resolution_date_function() TO admin_role;


--
-- Name: FUNCTION validate_resolution_sequence_function(); Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON FUNCTION public.validate_resolution_sequence_function() TO admin_role;


--
-- PostgreSQL database dump complete
--

\unrestrict GucGWTjIWskJf3aavSfTFESEnz8vQcWeuwDJkqLOymuBYKioubQ0DfQxMZQPtlL

