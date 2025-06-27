#!/bin/bash

echo "🚀 Iniciando Corporate Travel Database Setup..."

# Aguardar o SQL Server estar pronto
echo "⏳ Aguardando SQL Server estar pronto..."
sleep 30

# Executar migrações do Entity Framework
echo "📦 Executando migrações do Entity Framework..."
cd backend
dotnet ef database update --project src/CorporateTravel.Infrastructure --startup-project src/CorporateTravel.API

echo "✅ Setup do banco de dados concluído!"
echo "🌐 Backend disponível em: http://localhost:5000"
echo "🗄️  SQL Server disponível em: localhost,1433"
echo "   Usuário: sa"
echo "   Senha: YourStrong@Passw0rd"
echo "   Database: CorporateTravel" 