-- ============================================
-- DATOS DE PRUEBA (SOLO DESARROLLO) - MICROSERVICIO AUTH SERVICE
-- ============================================
-- Proyecto: Sistema de Autenticación y Seguridad
-- Microservicio: AuthService (Security)
-- Descripción: Usuarios de ejemplo para testing
-- ⚠️ ADVERTENCIA: NO ejecutar en producción
-- ⚠️ Las contraseñas están hasheadas pero son conocidas
-- ============================================

-- ============================================
-- USUARIOS DE PRUEBA
-- ============================================

-- NOTA: Estos hashes corresponden a contraseñas de prueba
-- En producción, SIEMPRE usar hashes generados con un algoritmo seguro
-- y sal única por cada contraseña

INSERT INTO "user" (
    id,
    email,
    password_hash,
    full_name,
    role_id,
    is_active,
    created_at,
    failed_attempts,
    is_locked,
    force_password_change
) VALUES
    -- Usuario Admin de prueba
    (
        '01993db9-6715-70cd-8d6a-ae231452261f',
        'admin@test.com',
        'pqgtXTwlEg/GN+xOHdhPy94vmDi1LKAepGCyE15Bw0I=',  -- Contraseña: Test123!
        'Admin de Prueba',
        '01993db7-c802-7b52-b429-c57112efae51',  -- Admin
        true,
        CURRENT_TIMESTAMP,
        0,
        false,
        false
    ),
    -- Usuario Professional de prueba
    (
        '01993471-90a4-7ce0-baa7-28015ca145bf',
        'doctor@test.com',
        'r1fWg2m0dT70IqKtHGhOThbAduVVZaGVAVhZ8jfw0Oc=',  -- Contraseña: Test123!
        'Dr. Juan Pérez',
        'e96fc6a7-8981-4f25-96c5-c3204724ccb1',  -- Professional
        true,
        CURRENT_TIMESTAMP,
        0,
        false,
        false
    ),
    -- Usuario Patient de prueba
    (
        gen_random_uuid(),
        'paciente@test.com',
        'r1fWg2m0dT70IqKtHGhOThbAduVVZaGVAVhZ8jfw0Oc=',  -- Contraseña: Test123!
        'María González',
        '8fedead3-081a-409c-8ee5-f7b871b2277b',  -- Patient
        true,
        CURRENT_TIMESTAMP,
        0,
        false,
        false
    ),
    -- Usuario ScheduleManager de prueba
    (
        gen_random_uuid(),
        'agenda@test.com',
        'r1fWg2m0dT70IqKtHGhOThbAduVVZaGVAVhZ8jfw0Oc=',  -- Contraseña: Test123!
        'Ana Martínez',
        '3da5ae49-cbe1-4fe0-a600-528df1d53ade',  -- ScheduleManager
        true,
        CURRENT_TIMESTAMP,
        0,
        false,
        false
    )
ON CONFLICT (email) DO NOTHING;

-- ============================================
-- VERIFICACIÓN
-- ============================================

DO $$
BEGIN
    RAISE NOTICE '';
    RAISE NOTICE '═══════════════════════════════════════════════════════';
    RAISE NOTICE '✓ USUARIOS DE PRUEBA INSERTADOS';
    RAISE NOTICE '═══════════════════════════════════════════════════════';
    RAISE NOTICE '';
    RAISE NOTICE '📊 Total usuarios: %', (SELECT COUNT(*) FROM "user");
    RAISE NOTICE '';
    RAISE NOTICE '🔐 Credenciales de prueba (SOLO DESARROLLO):';
    RAISE NOTICE '';
    RAISE NOTICE '  👤 Admin:';
    RAISE NOTICE '     Email: admin@test.com';
    RAISE NOTICE '     Password: Test123!';
    RAISE NOTICE '';
    RAISE NOTICE '  👨‍⚕️ Professional:';
    RAISE NOTICE '     Email: doctor@test.com';
    RAISE NOTICE '     Password: Test123!';
    RAISE NOTICE '';
    RAISE NOTICE '  🧑 Patient:';
    RAISE NOTICE '     Email: paciente@test.com';
    RAISE NOTICE '     Password: Test123!';
    RAISE NOTICE '';
    RAISE NOTICE '  📅 ScheduleManager:';
    RAISE NOTICE '     Email: agenda@test.com';
    RAISE NOTICE '     Password: Test123!';
    RAISE NOTICE '';
    RAISE NOTICE '⚠️  ¡IMPORTANTE! ⚠️';
    RAISE NOTICE '  - Estos usuarios son SOLO para desarrollo/testing';
    RAISE NOTICE '  - NUNCA usar en producción';
    RAISE NOTICE '  - Las contraseñas son conocidas públicamente';
    RAISE NOTICE '  - Eliminar este archivo antes de deploy a producción';
    RAISE NOTICE '';
    RAISE NOTICE '═══════════════════════════════════════════════════════';
    RAISE NOTICE '';
END $$;
