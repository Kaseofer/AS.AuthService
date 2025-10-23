-- ============================================
-- CONSTRAINTS Y CLAVES - MICROSERVICIO AUTH SERVICE
-- ============================================
-- Proyecto: Sistema de Autenticación y Seguridad
-- Microservicio: AuthService (Security)
-- Descripción: Primary Keys, Unique Constraints y Foreign Keys
-- ============================================

-- ============================================
-- PRIMARY KEYS
-- ============================================

ALTER TABLE ONLY role
    ADD CONSTRAINT roles_pkey PRIMARY KEY (id);

ALTER TABLE ONLY "user"
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);

ALTER TABLE ONLY audit_log
    ADD CONSTRAINT audit_logs_pkey PRIMARY KEY (id);

ALTER TABLE ONLY password_reset
    ADD CONSTRAINT password_resets_pkey PRIMARY KEY (id);

ALTER TABLE ONLY external_login
    ADD CONSTRAINT external_logins_pkey PRIMARY KEY (id);

-- ============================================
-- UNIQUE CONSTRAINTS
-- ============================================

-- role
ALTER TABLE ONLY role
    ADD CONSTRAINT roles_name_key UNIQUE (name);

-- user
ALTER TABLE ONLY "user"
    ADD CONSTRAINT users_email_key UNIQUE (email);

-- ============================================
-- FOREIGN KEYS
-- ============================================

-- user -> role
ALTER TABLE ONLY "user"
    ADD CONSTRAINT users_role_id_fkey 
    FOREIGN KEY (role_id) 
    REFERENCES role(id) 
    ON DELETE RESTRICT;

-- audit_log -> user
ALTER TABLE ONLY audit_log
    ADD CONSTRAINT audit_logs_user_id_fkey 
    FOREIGN KEY (user_id) 
    REFERENCES "user"(id) 
    ON DELETE SET NULL;

-- password_reset -> user
ALTER TABLE ONLY password_reset
    ADD CONSTRAINT password_resets_user_id_fkey 
    FOREIGN KEY (user_id) 
    REFERENCES "user"(id) 
    ON DELETE CASCADE;

-- external_login -> user
ALTER TABLE ONLY external_login
    ADD CONSTRAINT external_logins_user_id_fkey 
    FOREIGN KEY (user_id) 
    REFERENCES "user"(id) 
    ON DELETE CASCADE;

-- ============================================
-- COMENTARIOS
-- ============================================

COMMENT ON CONSTRAINT users_role_id_fkey ON "user" 
    IS 'Relación usuario con su rol - RESTRICT evita eliminar roles en uso';

COMMENT ON CONSTRAINT audit_logs_user_id_fkey ON audit_log 
    IS 'Relación log con usuario - SET NULL permite mantener logs de usuarios eliminados';

COMMENT ON CONSTRAINT password_resets_user_id_fkey ON password_reset 
    IS 'Relación token con usuario - CASCADE elimina tokens al eliminar usuario';

COMMENT ON CONSTRAINT external_logins_user_id_fkey ON external_login 
    IS 'Relación login externo con usuario - CASCADE elimina vinculación al eliminar usuario';
