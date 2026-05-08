import api from "./index.js";

export const getAllCustomers = (page = 1, pageSize = 10, search = "") =>
  api.get("/customers", {
    params: { page, pageSize, search: search || undefined },
  });
export const getCustomerById = (id) => api.get(`/customers/${id}`);
export const getCustomerMapPins = (id) => api.get(`/customers/${id}/map`);
