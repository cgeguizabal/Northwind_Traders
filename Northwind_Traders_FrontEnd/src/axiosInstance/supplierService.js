import api from "./index.js";

export const getAllSuppliers = () => api.get("/suppliers"); // list with product count
export const getSupplierById = (id) => api.get(`/suppliers/${id}`); // detail with product list
