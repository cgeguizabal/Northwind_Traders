import api from './index.js'

export const getAllSuppliers  = ()   => api.get('/suppliers')
export const getSupplierById  = (id) => api.get(`/suppliers/${id}`)
