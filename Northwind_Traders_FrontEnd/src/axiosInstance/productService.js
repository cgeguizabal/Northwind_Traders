import api from "./index.js";

export const getAllProducts = () => api.get("/products");                        // all products
export const getProductById = (id) => api.get(`/products/${id}`);               // single product detail
export const getProductsByCategory = (id) =>
  api.get(`/products/category/${id}`);                                           // filtered by category
