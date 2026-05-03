import api from './index.js'

export const getAllProducts        = ()   => api.get('/products')
export const getProductById        = (id) => api.get(`/products/${id}`)
export const getActiveProducts     = ()   => api.get('/products')          // no /active endpoint – returns all
export const getProductsByCategory = (id) => api.get(`/products/category/${id}`)
