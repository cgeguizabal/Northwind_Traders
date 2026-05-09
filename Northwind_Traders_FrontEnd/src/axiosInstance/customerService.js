import api from "./index.js";

// Paginated list with optional full-text search
export const getAllCustomers = (page = 1, pageSize = 10, search = "") =>
  api.get("/customers", {
    params: { page, pageSize, search: search || undefined },
  });
export const getCustomerById = (id) => api.get(`/customers/${id}`);
// Returns geocoded order locations for dropping map pins
export const getCustomerMapPins = (id) => api.get(`/customers/${id}/map`);
