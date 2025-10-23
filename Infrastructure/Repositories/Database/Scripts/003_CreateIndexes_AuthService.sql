-- ============================================
-- ÍNDICES - MICROSERVICIO AUTH SERVICE
-- ============================================
-- Proyecto: Sistema de Autenticación y Seguridad
-- Microservicio: AuthService (Security)
-- Descripción: Índices para optimizar autenticación y auditoría
-- ============================================

-- ============================================
-- ÍNDICES - role
-- ============================================

CREATE INDEX IF NOT EXISTS idx_role_name 
    ON role USING btree (name);

COMMENT ON INDEX idx_role_name IS 'Optimiza búsqueda de roles por nombre';

-- ============================================
-- ÍNDICES - user
-- ============================================

-- Índice para login por email (búsqueda más frecuente)
CREATE INDEX IF NOT EXISTS idx_user_email 
    ON "user" USING btree (email);

-- Índice para búsqueda por rol
CREATE INDEX IF NOT EXISTS idx_user_role_id 
    ON "user" USING btree (role_id);

-- Índice para filtrar usuarios activos
CREATE INDEX IF NOT EXISTS idx_user_is_active 
    ON "user" USING btree (is_active) 
    WHERE (is_active = true);

-- Índice para identificar cuentas bloqueadas
CREATE INDEX IF NOT EXISTS idx_user_is_locked 
    ON "user" USING btree (is_locked) 
    WHERE (is_locked = true);

-- Índice para usuarios que deben cambiar contraseña
CREATE INDEX IF NOT EXISTS idx_user_force_password_change 
    ON "user" USING btree (force_password_change) 
    WHERE (force_password_change = true);

-- Índice para búsqueda por nombre completo
CREATE INDEX IF NOT EXISTS idx_user_full_name 
    ON "user" USING btree (full_name);

-- Índice para detectar múltiples intentos fallidos
CREATE INDEX IF NOT EXISTS idx_user_failed_attempts 
    ON "user" USING btree (failed_attempts) 
    WHERE (failed_attempts > 0);

-- Índice para usuarios con bloqueo temporal pendiente
CREATE INDEX IF NOT EXISTS idx_user_next_allowed_login 
    ON "user" USING btree (next_allowed_login) 
    WHERE (next_allowed_login IS NOT NULL);

COMMENT ON INDEX idx_user_email IS 'CRÍTICO: Optimiza el proceso de login';
COMMENT ON INDEX idx_user_is_active IS 'Optimiza filtrado de usuarios activos';
COMMENT ON INDEX idx_user_is_locked IS 'Identifica rápidamente cuentas bloqueadas';
COMMENT ON INDEX idx_user_failed_attempts IS 'Identifica usuarios con intentos fallidos (posibles ataques)';
COMMENT ON INDEX idx_user_next_allowed_login IS 'Identifica usuarios con bloqueos temporales activos';

-- ============================================
-- ÍNDICES - audit_log
-- ============================================

-- Índice para búsqueda por usuario
CREATE INDEX IF NOT EXISTS idx_audit_log_user_id 
    ON audit_log USING btree (user_id);

-- Índice para búsqueda por timestamp (consultas por rango de fechas)
CREATE INDEX IF NOT EXISTS idx_audit_log_timestamp 
    ON audit_log USING btree ("timestamp" DESC);

-- Índice para búsqueda por acción
CREATE INDEX IF NOT EXISTS idx_audit_log_action 
    ON audit_log USING btree (action);

-- Índice compuesto: usuario + fecha (consultas de actividad por usuario)
CREATE INDEX IF NOT EXISTS idx_audit_log_user_timestamp 
    ON audit_log USING btree (user_id, "timestamp" DESC);

-- Índice para búsqueda por IP (detectar actividad sospechosa)
CREATE INDEX IF NOT EXISTS idx_audit_log_ip_address 
    ON audit_log USING btree (ip_address);

-- Índice para búsqueda por área de aplicación
CREATE INDEX IF NOT EXISTS idx_audit_log_application_area 
    ON audit_log USING btree (application_area);

COMMENT ON INDEX idx_audit_log_timestamp IS 'Optimiza consultas de auditoría por fecha (más recientes primero)';
COMMENT ON INDEX idx_audit_log_user_timestamp IS 'Optimiza consultas de actividad por usuario';
COMMENT ON INDEX idx_audit_log_ip_address IS 'Permite detectar múltiples accesos desde misma IP';

-- ============================================
-- ÍNDICES - password_reset
-- ============================================

-- Índice para búsqueda por usuario
CREATE INDEX IF NOT EXISTS idx_password_reset_user_id 
    ON password_reset USING btree (user_id);

-- Índice para búsqueda por token (validación de token)
CREATE INDEX IF NOT EXISTS idx_password_reset_token 
    ON password_reset USING btree (reset_token);

-- Índice para identificar tokens no usados
CREATE INDEX IF NOT EXISTS idx_password_reset_used 
    ON password_reset USING btree (used) 
    WHERE (used = false);

-- Índice para limpieza de tokens expirados
CREATE INDEX IF NOT EXISTS idx_password_reset_expires_at 
    ON password_reset USING btree (expires_at);

-- Índice compuesto: token válido (no usado y no expirado)
CREATE INDEX IF NOT EXISTS idx_password_reset_valid 
    ON password_reset USING btree (reset_token, expires_at, used) 
    WHERE (used = false);

COMMENT ON INDEX idx_password_reset_token IS 'CRÍTICO: Optimiza validación de tokens de reseteo';
COMMENT ON INDEX idx_password_reset_valid IS 'Optimiza búsqueda de tokens válidos';

-- ============================================
-- ÍNDICES - external_login
-- ============================================

-- Índice para búsqueda por usuario
CREATE INDEX IF NOT EXISTS idx_external_login_user_id 
    ON external_login USING btree (user_id);

-- Índice para búsqueda por proveedor
CREATE INDEX IF NOT EXISTS idx_external_login_provider 
    ON external_login USING btree (provider);

-- Índice compuesto: proveedor + external_id (login OAuth)
CREATE INDEX IF NOT EXISTS idx_external_login_provider_external_id 
    ON external_login USING btree (provider, external_id);

-- Índice para búsqueda por external_id
CREATE INDEX IF NOT EXISTS idx_external_login_external_id 
    ON external_login USING btree (external_id);

COMMENT ON INDEX idx_external_login_provider_external_id IS 'CRÍTICO: Optimiza login mediante proveedores externos (OAuth)';
