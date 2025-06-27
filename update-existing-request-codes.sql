-- Script para atualizar códigos das solicitações de viagem existentes
-- Formato: TR-YYYYMMDD-XXXX

DECLARE @CurrentDate DATE = GETUTCDATE();
DECLARE @DatePrefix NVARCHAR(8) = FORMAT(@CurrentDate, 'yyyyMMdd');
DECLARE @BaseCode NVARCHAR(20) = 'TR-' + @DatePrefix;

-- Atualizar solicitações existentes com códigos baseados na data de criação
WITH NumberedRequests AS (
    SELECT 
        Id,
        CreatedAt,
        ROW_NUMBER() OVER (ORDER BY CreatedAt) as RowNum
    FROM TravelRequests 
    WHERE RequestCode = '' OR RequestCode IS NULL
)
UPDATE tr
SET RequestCode = @BaseCode + '-' + RIGHT('0000' + CAST(nr.RowNum AS NVARCHAR(10)), 4)
FROM TravelRequests tr
INNER JOIN NumberedRequests nr ON tr.Id = nr.Id;

-- Verificar resultado
SELECT 
    Id,
    RequestCode,
    CreatedAt,
    Origin,
    Destination
FROM TravelRequests 
ORDER BY CreatedAt; 