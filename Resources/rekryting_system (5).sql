-- phpMyAdmin SQL Dump
-- version 4.8.5
-- https://www.phpmyadmin.net/
--
-- Хост: localhost
-- Время создания: Дек 07 2025 г., 16:06
-- Версия сервера: 5.7.25
-- Версия PHP: 7.1.26

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- База данных: `rekryting_system`
--

-- --------------------------------------------------------

--
-- Структура таблицы `applications`
--

CREATE TABLE `applications` (
  `company_name` varchar(255) NOT NULL,
  `vacancy_title` varchar(255) NOT NULL,
  `user_email` varchar(255) NOT NULL,
  `cover_letter` text,
  `status` enum('sent','viewed','interview','hired','rejected') DEFAULT 'sent'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `companies`
--

CREATE TABLE `companies` (
  `name` varchar(255) NOT NULL,
  `description` text,
  `website` varchar(255) DEFAULT NULL,
  `logo_url` varchar(255) DEFAULT NULL,
  `verified` tinyint(1) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `daily_analytics`
--

CREATE TABLE `daily_analytics` (
  `id` int(11) NOT NULL,
  `user_email` varchar(255) NOT NULL,
  `date` date NOT NULL,
  `profile_views` int(11) DEFAULT '0',
  `applications_sent` int(11) DEFAULT '0',
  `saved_vacancies` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура таблицы `job_applications`
--

CREATE TABLE `job_applications` (
  `id` int(11) NOT NULL,
  `student_email` varchar(255) NOT NULL,
  `vacancy_company_name` varchar(255) NOT NULL,
  `vacancy_title` varchar(255) NOT NULL,
  `cover_letter` text,
  `status` varchar(20) DEFAULT 'pending',
  `applied_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура таблицы `resumes`
--

CREATE TABLE `resumes` (
  `user_email` varchar(255) NOT NULL,
  `desired_position` varchar(255) NOT NULL,
  `experience_description` text,
  `education_description` text,
  `skills` text,
  `salary_expectations` int(11) DEFAULT NULL,
  `is_published` tinyint(1) DEFAULT '0',
  `practices_json` text
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `resume_views`
--

CREATE TABLE `resume_views` (
  `id` int(11) NOT NULL,
  `resume_email` varchar(255) NOT NULL,
  `viewer_email` varchar(255) NOT NULL,
  `viewed_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `viewed_from_ip` varchar(45) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура таблицы `saved_vacancies`
--

CREATE TABLE `saved_vacancies` (
  `id` int(11) NOT NULL,
  `student_email` varchar(255) NOT NULL,
  `vacancy_company_name` varchar(255) NOT NULL,
  `vacancy_title` varchar(255) NOT NULL,
  `saved_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура таблицы `users`
--

CREATE TABLE `users` (
  `email` varchar(255) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `first_name` varchar(100) NOT NULL,
  `last_name` varchar(100) NOT NULL,
  `role` enum('job_seeker','employer','admin') NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `vacancies`
--

CREATE TABLE `vacancies` (
  `company_name` varchar(255) NOT NULL,
  `title` varchar(255) NOT NULL,
  `description` text NOT NULL,
  `requirements` text NOT NULL,
  `salary_from` int(11) DEFAULT NULL,
  `salary_to` int(11) DEFAULT NULL,
  `employment_type` enum('full','part','project','internship','volunteer') NOT NULL,
  `work_schedule` enum('full_day','shifts','flexible','remote','shift_work') NOT NULL,
  `author_email` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `__efmigrationshistory`
--

CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Дамп данных таблицы `__efmigrationshistory`
--

INSERT INTO `__efmigrationshistory` (`MigrationId`, `ProductVersion`) VALUES
('20251207144215_initialcreate', '9.0.10');

--
-- Индексы сохранённых таблиц
--

--
-- Индексы таблицы `applications`
--
ALTER TABLE `applications`
  ADD PRIMARY KEY (`company_name`,`vacancy_title`,`user_email`),
  ADD KEY `user_email` (`user_email`);

--
-- Индексы таблицы `companies`
--
ALTER TABLE `companies`
  ADD PRIMARY KEY (`name`);

--
-- Индексы таблицы `daily_analytics`
--
ALTER TABLE `daily_analytics`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_user_date` (`user_email`,`date`),
  ADD KEY `idx_user` (`user_email`);

--
-- Индексы таблицы `job_applications`
--
ALTER TABLE `job_applications`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_student_email` (`student_email`),
  ADD KEY `idx_vacancy` (`vacancy_company_name`,`vacancy_title`);

--
-- Индексы таблицы `resumes`
--
ALTER TABLE `resumes`
  ADD PRIMARY KEY (`user_email`,`desired_position`);

--
-- Индексы таблицы `resume_views`
--
ALTER TABLE `resume_views`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_resume_email` (`resume_email`),
  ADD KEY `idx_viewer_email` (`viewer_email`);

--
-- Индексы таблицы `saved_vacancies`
--
ALTER TABLE `saved_vacancies`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_save` (`student_email`,`vacancy_company_name`,`vacancy_title`),
  ADD KEY `idx_student` (`student_email`);

--
-- Индексы таблицы `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`email`);

--
-- Индексы таблицы `vacancies`
--
ALTER TABLE `vacancies`
  ADD PRIMARY KEY (`company_name`,`title`),
  ADD KEY `author_email` (`author_email`);

--
-- Индексы таблицы `__efmigrationshistory`
--
ALTER TABLE `__efmigrationshistory`
  ADD PRIMARY KEY (`MigrationId`);

--
-- AUTO_INCREMENT для сохранённых таблиц
--

--
-- AUTO_INCREMENT для таблицы `daily_analytics`
--
ALTER TABLE `daily_analytics`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `job_applications`
--
ALTER TABLE `job_applications`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `resume_views`
--
ALTER TABLE `resume_views`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `saved_vacancies`
--
ALTER TABLE `saved_vacancies`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- Ограничения внешнего ключа сохраненных таблиц
--

--
-- Ограничения внешнего ключа таблицы `applications`
--
ALTER TABLE `applications`
  ADD CONSTRAINT `applications_ibfk_1` FOREIGN KEY (`company_name`,`vacancy_title`) REFERENCES `vacancies` (`company_name`, `title`),
  ADD CONSTRAINT `applications_ibfk_2` FOREIGN KEY (`user_email`) REFERENCES `users` (`email`);

--
-- Ограничения внешнего ключа таблицы `resumes`
--
ALTER TABLE `resumes`
  ADD CONSTRAINT `resumes_ibfk_1` FOREIGN KEY (`user_email`) REFERENCES `users` (`email`);

--
-- Ограничения внешнего ключа таблицы `vacancies`
--
ALTER TABLE `vacancies`
  ADD CONSTRAINT `vacancies_ibfk_1` FOREIGN KEY (`company_name`) REFERENCES `companies` (`name`),
  ADD CONSTRAINT `vacancies_ibfk_2` FOREIGN KEY (`author_email`) REFERENCES `users` (`email`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
