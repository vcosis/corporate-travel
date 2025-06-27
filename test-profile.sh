#!/bin/bash

echo "Testando endpoint de profile..."

# Fazer login e obter token
echo "1. Fazendo login..."
LOGIN_RESPONSE=$(curl -s -X POST http://localhost:5178/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@corporatetravel.com","password":"Admin@123"}')

echo "Resposta do login: $LOGIN_RESPONSE"

# Extrair token
TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
    echo "Erro: Não foi possível obter o token"
    exit 1
fi

echo "Token obtido: ${TOKEN:0:50}..."

# Testar endpoint de profile
echo "2. Testando endpoint /api/profile..."
PROFILE_RESPONSE=$(curl -s -X GET http://localhost:5178/api/profile \
  -H "Authorization: Bearer $TOKEN")

echo "Resposta do profile: $PROFILE_RESPONSE"

if [[ $PROFILE_RESPONSE == *"id"* ]]; then
    echo "✅ Sucesso! Endpoint /api/profile está funcionando"
else
    echo "❌ Erro: Endpoint /api/profile não está funcionando"
    echo "Resposta completa: $PROFILE_RESPONSE"
fi 