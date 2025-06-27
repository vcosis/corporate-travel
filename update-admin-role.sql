-- Script para atualizar o usuário admin@corporatetravel.com para ter role Admin
-- Execute este script no banco de dados CorporateTravel

-- Primeiro, garantir que o role Admin existe
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID())
    PRINT 'Role "Admin" criado com sucesso!'
END

-- Encontrar o usuário admin
DECLARE @AdminUserId UNIQUEIDENTIFIER
SELECT @AdminUserId = Id FROM AspNetUsers WHERE Email = 'admin@corporatetravel.com'

IF @AdminUserId IS NOT NULL
BEGIN
    -- Encontrar o role Admin
    DECLARE @AdminRoleId UNIQUEIDENTIFIER
    SELECT @AdminRoleId = Id FROM AspNetRoles WHERE Name = 'Admin'
    
    -- Remover role Manager se existir
    DECLARE @ManagerRoleId UNIQUEIDENTIFIER
    SELECT @ManagerRoleId = Id FROM AspNetRoles WHERE Name = 'Manager'
    
    IF @ManagerRoleId IS NOT NULL
    BEGIN
        DELETE FROM AspNetUserRoles 
        WHERE UserId = @AdminUserId AND RoleId = @ManagerRoleId
        PRINT 'Role "Manager" removido do usuário admin.'
    END
    
    -- Adicionar role Admin se não existir
    IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId)
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        VALUES (@AdminUserId, @AdminRoleId)
        PRINT 'Role "Admin" adicionado ao usuário admin@corporatetravel.com com sucesso!'
    END
    ELSE
    BEGIN
        PRINT 'Usuário admin@corporatetravel.com já possui o role "Admin".'
    END
END
ELSE
BEGIN
    PRINT 'Usuário admin@corporatetravel.com não encontrado.'
END 