-- ============================================
-- CREACIÓN DE TABLAS - MICROSERVICIO AUTH SERVICE
-- ============================================
-- Proyecto: Sistema de Autenticación y Seguridad
-- Microservicio: AuthService (Security)
-- Base de Datos: PostgreSQL 17
-- Fecha: 2025-10-23
-- ============================================

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

-- ============================================
-- TABLA: role
-- Descripción: Roles del sistema (permisos)
-- ============================================

CREATE TABLE IF NOT EXISTS role (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying(50) NOT NULL,
    description text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE role IS 'Roles y permisos del sistema';
COMMENT ON COLUMN role.name IS 'Nombre único del rol: Admin, Patient, Professional, ScheduleManager';

-- ============================================
-- TABLA: user
-- Descripción: Usuarios del sistema con autenticación
-- ============================================

CREATE TABLE IF NOT EXISTS "user" (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    email character varying(255) NOT NULL,
    password_hash text NOT NULL,
    full_name character varying(255),
    role_id uuid NOT NULL,
    is_active boolean DEFAULT true,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone,
    
    -- Control de seguridad
    failed_attempts integer DEFAULT 0,
    is_locked boolean DEFAULT false,
    last_failed_login timestamp with time zone,
    next_allowed_login timestamp with time zone,
    last_successful_login timestamp with time zone,
    
    -- Gestión de contraseñas
    password_changed_at timestamp with time zone,
    force_password_change boolean DEFAULT false
);

COMMENT ON TABLE "user" IS 'Usuarios del sistema con credenciales y control de acceso';
COMMENT ON COLUMN "user".password_hash IS 'Hash de la contraseña (nunca almacenar en texto plano)';
COMMENT ON COLUMN "user".failed_attempts IS 'Contador de intentos fallidos de login';
COMMENT ON COLUMN "user".is_locked IS 'true = cuenta bloqueada por intentos fallidos';
COMMENT ON COLUMN "user".force_password_change IS 'true = debe cambiar contraseña en próximo login';
COMMENT ON COLUMN "user".next_allowed_login IS 'Fecha/hora hasta cuando está bloqueado temporalmente';

-- ============================================
-- TABLA: audit_log
-- Descripción: Registro de auditoría de todas las acciones
-- ============================================

CREATE TABLE IF NOT EXISTS audit_log (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid,
    action character varying(100) NOT NULL,
    ip_address character varying(50),
    user_agent text,
    endpoint character varying(200),
    http_method character varying(8),
    application_area character varying(20),
    "timestamp" timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

COMMENT ON TABLE audit_log IS 'Log de auditoría de todas las acciones del sistema';
COMMENT ON COLUMN audit_log.action IS 'Acción realizada: LOGIN, LOGOUT, CREATE, UPDATE, DELETE, etc.';
COMMENT ON COLUMN audit_log.ip_address IS 'Dirección IP del usuario';
COMMENT ON COLUMN audit_log.application_area IS 'Área de la aplicación: AUTH, APPOINTMENTS, PATIENTS, etc.';

-- ============================================
-- TABLA: password_reset
-- Descripción: Tokens para reseteo de contraseñas
-- ============================================

CREATE TABLE IF NOT EXISTS password_reset (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid,
    reset_token text NOT NULL,
    expires_at timestamp without time zone NOT NULL,
    used boolean DEFAULT false,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE password_reset IS 'Tokens de reseteo de contraseña (one-time use)';
COMMENT ON COLUMN password_reset.reset_token IS 'Token único para resetear contraseña';
COMMENT ON COLUMN password_reset.expires_at IS 'Fecha de expiración del token (típicamente 1-24 horas)';
COMMENT ON COLUMN password_reset.used IS 'true = token ya fue utilizado';

-- ============================================
-- TABLA: external_login
-- Descripción: Logins externos (OAuth, Google, Facebook, etc.)
-- ============================================

CREATE TABLE IF NOT EXISTS external_login (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid,
    provider character varying(50) NOT NULL,
    external_id text NOT NULL,
    linked_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE external_login IS 'Autenticación mediante proveedores externos (OAuth)';
COMMENT ON COLUMN external_login.provider IS 'Proveedor: google, facebook, microsoft, apple, etc.';
COMMENT ON COLUMN external_login.external_id IS 'ID del usuario en el proveedor externo';
COMMENT ON COLUMN external_login.linked_at IS 'Fecha cuando se vinculó la cuenta externa';
