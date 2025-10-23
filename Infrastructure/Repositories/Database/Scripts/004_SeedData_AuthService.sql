-- ============================================
-- DATOS INICIALES (SEED DATA) - MICROSERVICIO AUTH SERVICE
-- ============================================
-- Proyecto: Sistema de Autenticación y Seguridad
-- Microservicio: AuthService (Security)
-- Descripción: Roles del sistema
-- ============================================

-- ============================================
-- ROLES DEL SISTEMA
-- ============================================

INSERT INTO role (id, name, description, created_at) VALUES
    (
        '8fedead3-081a-409c-8ee5-f7b871b2277b',
        'Patient',
        'Usuario que solicita turnos y servicios médicos',
        CURRENT_TIMESTAMP
    ),
    (
        'e96fc6a7-8981-4f25-96c5-c3204724ccb1',
        'Professional',
        'Prestador de servicios médicos',
        CURRENT_TIMESTAMP
    ),
    (
        '3da5ae49-cbe1-4fe0-a600-528df1d53ade',
        'ScheduleManager',
        'Administrador de agenda y disponibilidad',
        CURRENT_TIMESTAMP
    ),
    (
        '01993db7-c802-7b52-b429-c57112efae51',
        'Admin',
        'Administrador del Sistema',
        CURRENT_TIMESTAMP
    )
ON CONFLICT (name) DO UPDATE SET
    description = EXCLUDED.description;

-- ============================================
-- VERIFICACIÓN
-- ============================================

DO $$
BEGIN
    RAISE NOTICE '';
    RAISE NOTICE '═══════════════════════════════════════════════════════';
    RAISE NOTICE '✓ ROLES DEL SISTEMA INSERTADOS';
    RAISE NOTICE '═══════════════════════════════════════════════════════';
    RAISE NOTICE '';
    RAISE NOTICE '📊 Roles disponibles: %', (SELECT COUNT(*) FROM role);
    RAISE NOTICE '';
    RAISE NOTICE '🔐 Roles configurados:';
    RAISE NOTICE '  1. Patient (8fedead3-081a-409c-8ee5-f7b871b2277b)';
    RAISE NOTICE '     → Usuario que solicita turnos y servicios médicos';
    RAISE NOTICE '';
    RAISE NOTICE '  2. Professional (e96fc6a7-8981-4f25-96c5-c3204724ccb1)';
    RAISE NOTICE '     → Prestador de servicios médicos';
    RAISE NOTICE '';
    RAISE NOTICE '  3. ScheduleManager (3da5ae49-cbe1-4fe0-a600-528df1d53ade)';
    RAISE NOTICE '     → Administrador de agenda y disponibilidad';
    RAISE NOTICE '';
    RAISE NOTICE '  4. Admin (01993db7-c802-7b52-b429-c57112efae51)';
    RAISE NOTICE '     → Administrador del Sistema';
    RAISE NOTICE '';
    RAISE NOTICE '⚠️  IMPORTANTE:';
    RAISE NOTICE '  - NO eliminar estos roles, están vinculados al sistema';
    RAISE NOTICE '  - Los UUIDs deben permanecer constantes en todos los ambientes';
    RAISE NOTICE '  - Estos roles son compartidos por todos los microservicios';
    RAISE NOTICE '';
    RAISE NOTICE '═══════════════════════════════════════════════════════';
    RAISE NOTICE '';
END $$;
