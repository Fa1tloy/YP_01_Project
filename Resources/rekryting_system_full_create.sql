-- =========================================================
-- Полное создание БД rekryting_system (MySQL 8+)
-- Актуально для текущих моделей WebReckrytingSystem
-- =========================================================

DROP DATABASE IF EXISTS rekryting_system;
CREATE DATABASE rekryting_system
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE rekryting_system;

-- ---------------------------------------------------------
-- companies
-- ---------------------------------------------------------
CREATE TABLE companies (
    name            VARCHAR(255) NOT NULL,
    description     TEXT NULL,
    website         TEXT NULL,
    logo_url        TEXT NULL,
    verified        TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (name)
) ENGINE=InnoDB;

-- ---------------------------------------------------------
-- users
-- ---------------------------------------------------------
CREATE TABLE users (
    email           VARCHAR(255) NOT NULL,
    password_hash   VARCHAR(255) NOT NULL,
    first_name      VARCHAR(100) NOT NULL,
    last_name       VARCHAR(100) NOT NULL,
    role            VARCHAR(20) NOT NULL,
    company_name    VARCHAR(255) NULL,
    PRIMARY KEY (email),
    CONSTRAINT fk_users_company
        FOREIGN KEY (company_name)
        REFERENCES companies(name)
        ON DELETE SET NULL
        ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ---------------------------------------------------------
-- resumes
-- ---------------------------------------------------------
CREATE TABLE resumes (
    user_email              VARCHAR(255) NOT NULL,
    desired_position        VARCHAR(255) NOT NULL,
    city                    VARCHAR(100) NOT NULL,
    business_trip_readiness VARCHAR(20) NOT NULL,
    search_status           VARCHAR(50) NOT NULL,
    age                     INT NULL,
    employment_type         VARCHAR(50) NOT NULL,
    work_schedule           VARCHAR(50) NOT NULL,
    specialty               VARCHAR(255) NOT NULL,
    gender                  VARCHAR(20) NOT NULL,
    has_car                 TINYINT(1) NOT NULL DEFAULT 0,
    driver_license_category VARCHAR(20) NULL,
    experience_description  TEXT NULL,
    education_description   TEXT NULL,
    skills                  TEXT NULL,
    salary_expectations     INT NULL,
    is_published            TINYINT(1) NOT NULL DEFAULT 0,
    practices_json          TEXT NULL,
    PRIMARY KEY (user_email),
    CONSTRAINT fk_resumes_user
        FOREIGN KEY (user_email)
        REFERENCES users(email)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT chk_resumes_age
        CHECK (age IS NULL OR (age >= 14 AND age <= 100)),
    CONSTRAINT chk_resumes_salary
        CHECK (salary_expectations IS NULL OR (salary_expectations >= 0 AND salary_expectations <= 9999999))
) ENGINE=InnoDB;

-- ---------------------------------------------------------
-- vacancies
-- ---------------------------------------------------------
CREATE TABLE vacancies (
    company_name        VARCHAR(255) NOT NULL,
    title               VARCHAR(255) NOT NULL,
    region              VARCHAR(100) NOT NULL DEFAULT '',
    description         TEXT NOT NULL,
    requirements        TEXT NOT NULL,
    salary_from         INT NULL,
    salary_to           INT NULL,
    employment_type     VARCHAR(50) NOT NULL,
    work_schedule       VARCHAR(50) NOT NULL,
    work_hours_per_day  INT NULL,
    work_format         VARCHAR(50) NOT NULL DEFAULT '',
    salary_period       VARCHAR(20) NOT NULL DEFAULT '',
    payment_frequency   VARCHAR(50) NOT NULL DEFAULT '',
    specialty           VARCHAR(255) NOT NULL DEFAULT '',
    author_email        VARCHAR(255) NOT NULL,

    PRIMARY KEY (company_name, title),

    CONSTRAINT fk_vacancies_company
        FOREIGN KEY (company_name)
        REFERENCES companies(name)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    CONSTRAINT fk_vacancies_author
        FOREIGN KEY (author_email)
        REFERENCES users(email)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,

    CONSTRAINT chk_vacancies_salary_from
        CHECK (salary_from IS NULL OR (salary_from >= 0 AND salary_from <= 9999999)),
    CONSTRAINT chk_vacancies_salary_to
        CHECK (salary_to IS NULL OR (salary_to >= 0 AND salary_to <= 9999999)),
    CONSTRAINT chk_vacancies_salary_range
        CHECK (salary_from IS NULL OR salary_to IS NULL OR salary_from <= salary_to),
    CONSTRAINT chk_vacancies_work_hours
        CHECK (work_hours_per_day IS NULL OR (work_hours_per_day >= 1 AND work_hours_per_day <= 24))
) ENGINE=InnoDB;

CREATE INDEX idx_vacancies_author_email ON vacancies(author_email);
CREATE INDEX idx_vacancies_title ON vacancies(title);
CREATE INDEX idx_vacancies_region ON vacancies(region);

-- ---------------------------------------------------------
-- resume_views
-- ---------------------------------------------------------
CREATE TABLE resume_views (
    id              INT NOT NULL AUTO_INCREMENT,
    resume_email    VARCHAR(255) NOT NULL,
    viewer_email    VARCHAR(255) NOT NULL,
    viewed_at       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    viewed_from_ip  VARCHAR(45) NULL,
    PRIMARY KEY (id)
) ENGINE=InnoDB;

CREATE INDEX idx_resume_views_resume_email ON resume_views(resume_email);
CREATE INDEX idx_resume_views_viewer_email ON resume_views(viewer_email);

-- ---------------------------------------------------------
-- job_applications
-- ---------------------------------------------------------
CREATE TABLE job_applications (
    id                    INT NOT NULL AUTO_INCREMENT,
    student_email         VARCHAR(255) NOT NULL,
    vacancy_company_name  VARCHAR(255) NOT NULL,
    vacancy_title         VARCHAR(255) NOT NULL,
    cover_letter          TEXT NULL,
    status                VARCHAR(20) NOT NULL DEFAULT 'pending',
    applied_at            DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id)
) ENGINE=InnoDB;

CREATE INDEX idx_job_applications_student_email ON job_applications(student_email);
CREATE INDEX idx_job_applications_vacancy ON job_applications(vacancy_company_name, vacancy_title);

-- ---------------------------------------------------------
-- saved_vacancies
-- ---------------------------------------------------------
CREATE TABLE saved_vacancies (
    id                    INT NOT NULL AUTO_INCREMENT,
    student_email         VARCHAR(255) NOT NULL,
    vacancy_company_name  VARCHAR(255) NOT NULL,
    vacancy_title         VARCHAR(255) NOT NULL,
    saved_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_saved_vacancies_unique (student_email, vacancy_company_name, vacancy_title)
) ENGINE=InnoDB;

CREATE INDEX idx_saved_vacancies_student_email ON saved_vacancies(student_email);
CREATE INDEX idx_saved_vacancies_vacancy ON saved_vacancies(vacancy_company_name, vacancy_title);

-- ---------------------------------------------------------
-- daily_analytics
-- ---------------------------------------------------------
CREATE TABLE daily_analytics (
    id                  INT NOT NULL AUTO_INCREMENT,
    user_email          VARCHAR(255) NOT NULL,
    date                DATE NOT NULL,
    profile_views       INT NOT NULL DEFAULT 0,
    applications_sent   INT NOT NULL DEFAULT 0,
    saved_vacancies     INT NOT NULL DEFAULT 0,
    PRIMARY KEY (id),
    UNIQUE KEY uq_daily_analytics_user_date (user_email, date)
) ENGINE=InnoDB;

CREATE INDEX idx_daily_analytics_user_email ON daily_analytics(user_email);

-- ---------------------------------------------------------
-- Базовые данные: заранее созданный администратор
-- Логин:    admin@careerflow.local
-- Пароль:   Admin123! (legacy-совместимый формат)
-- Роль:     admin
-- ---------------------------------------------------------
INSERT INTO users (email, password_hash, first_name, last_name, role, company_name)
VALUES (
    'admin@careerflow.local',
    'Admin123!',
    'System',
    'Administrator',
    'admin',
    NULL
);
