-- Script para adicionar os roles Admin, Manager e User ao sistema
-- Execute este script no banco de dados CorporateTravel

-- Verificar e adicionar o role "Admin"
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID())
    PRINT 'Role "Admin" adicionado com sucesso!'
END
ELSE
BEGIN
    PRINT 'Role "Admin" já existe no sistema.'
END

-- Verificar e adicionar o role "Manager"
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Manager')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Manager', 'MANAGER', NEWID())
    PRINT 'Role "Manager" adicionado com sucesso!'
END
ELSE
BEGIN
    PRINT 'Role "Manager" já existe no sistema.'
END

-- Verificar e adicionar o role "User"
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'User')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'User', 'USER', NEWID())
    PRINT 'Role "User" adicionado com sucesso!'
END
ELSE
BEGIN
    PRINT 'Role "User" já existe no sistema.'
END 