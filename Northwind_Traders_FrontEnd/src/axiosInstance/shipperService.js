import api from './index.js'

export const getAllShippers  = ()   => api.get('/shippers')           // list with order count
export const getShipperById  = (id) => api.get(`/shippers/${id}`)    // detail with order history
