// Script de teste para verificar a funcionalidade de exclusão de usuários
const axios = require('axios');

const API_BASE_URL = 'http://localhost:5178/api';

async function testDeleteUser() {
  try {
    console.log('=== Teste de Exclusão de Usuário ===');
    
    // 1. Primeiro, vamos fazer login para obter um token
    console.log('1. Fazendo login...');
    const loginResponse = await axios.post(`${API_BASE_URL}/auth/login`, {
      email: 'admin@corporatetravel.com',
      password: 'Admin@123'
    });
    
    const token = loginResponse.data.token;
    console.log('Token obtido:', token ? 'Sim' : 'Não');
    
    // 2. Listar usuários para obter um ID para deletar
    console.log('2. Listando usuários...');
    const usersResponse = await axios.get(`${API_BASE_URL}/users`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    const users = usersResponse.data.items;
    console.log(`Usuários encontrados: ${users.length}`);
    
    // Encontrar um usuário que não seja admin para deletar
    const userToDelete = users.find(user => !user.roles.includes('Admin') && user.email !== 'admin@corporatetravel.com');
    
    if (!userToDelete) {
      console.log('Nenhum usuário não-admin encontrado para deletar');
      return;
    }
    
    console.log(`Usuário selecionado para deletar: ${userToDelete.name} (${userToDelete.email})`);
    console.log(`ID do usuário: ${userToDelete.id}`);
    
    // 3. Tentar deletar o usuário
    console.log('3. Tentando deletar usuário...');
    const deleteResponse = await axios.delete(`${API_BASE_URL}/users/${userToDelete.id}`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    console.log('Resposta da exclusão:', deleteResponse.status);
    console.log('Usuário deletado com sucesso!');
    
    // 4. Verificar se o usuário foi realmente deletado
    console.log('4. Verificando se o usuário foi deletado...');
    const usersAfterDelete = await axios.get(`${API_BASE_URL}/users`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    const userStillExists = usersAfterDelete.data.items.find(u => u.id === userToDelete.id);
    console.log(`Usuário ainda existe: ${userStillExists ? 'Sim' : 'Não'}`);
    
  } catch (error) {
    console.error('Erro no teste:', error.response?.data || error.message);
    console.error('Status:', error.response?.status);
    console.error('Headers:', error.response?.headers);
  }
}

// Executar o teste
testDeleteUser(); 