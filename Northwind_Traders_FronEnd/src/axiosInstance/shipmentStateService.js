import api from './index.js'

export const getAllShipmentStates = () => api.get('/shipmentstates')
