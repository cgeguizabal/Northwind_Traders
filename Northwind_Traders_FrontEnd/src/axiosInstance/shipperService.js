import api from './index.js'

export const getAllShippers  = ()   => api.get('/shippers')
export const getShipperById  = (id) => api.get(`/shippers/${id}`)
