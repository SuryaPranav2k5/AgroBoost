-- =====================================================
-- AgroBoost Database Scripts - Seed Reference Data
-- =====================================================

-- Note: Categories are seeded via EF Core DbInitializer in code
-- This script is for any additional reference data needed

-- Verify Categories exist (they are seeded by EF Core)
SELECT * FROM Categories WHERE IsActive = 1 ORDER BY Name;

-- Verify AspNetRoles exist (seeded by DbInitializer)
SELECT * FROM AspNetRoles;

-- Verify Admin User exists (seeded by DbInitializer)
SELECT u.*, r.Name AS RoleName
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@agroboost.com';