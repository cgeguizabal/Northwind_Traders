import api from './index.js'

export const getAllCategories  = ()   => api.get('/categories')          // list with product count
export const getCategoryById   = (id) => api.get(`/categories/${id}`)    // detail with product list
