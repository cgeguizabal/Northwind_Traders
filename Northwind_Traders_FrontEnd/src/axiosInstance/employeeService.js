import api from './index.js'

export const getAllEmployees   = ()         => api.get('/employees')
export const getEmployeeById  = (id)        => api.get(`/employees/${id}`)
export const getEmployeePhoto = (id)        => api.get(`/employees/${id}/photo`, { responseType: 'blob' })
export const updateEmployee   = (id, data)  => api.put(`/employees/${id}`, data)
export const updateEmployeeTitle = (id, title) => api.put(`/employees/${id}/title`, JSON.stringify(title), {
  headers: { 'Content-Type': 'application/json' }
})
