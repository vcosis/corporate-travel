-- Script para adicionar a coluna CreatedAt à tabela AspNetUsers
-- Execute este script no banco de dados CorporateTravel

-- Verificar se a coluna CreatedAt já existe
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'CreatedAt')
BEGIN
    -- Adicionar a coluna CreatedAt
    ALTER TABLE AspNetUsers ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    
    -- Atualizar registros existentes com a data atual
    UPDATE AspNetUsers SET CreatedAt = GETUTCDATE() WHERE CreatedAt IS NULL
    
    PRINT 'Coluna "CreatedAt" adicionada com sucesso!'
END
ELSE
BEGIN
    PRINT 'Coluna "CreatedAt" já existe na tabela AspNetUsers.'
END 