import api from './index.js'

export const login = (credentials) => api.post('/auth/login', credentials)
