import api from './index.js'

export const getAllCustomers    = ()   => api.get('/customers')
export const getCustomerById   = (id) => api.get(`/customers/${id}`)
export const getCustomerMapPins = (id) => api.get(`/customers/${id}/map`)
