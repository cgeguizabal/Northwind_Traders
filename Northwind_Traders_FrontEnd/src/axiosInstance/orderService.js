import api from "./index.js";

export const getAllOrders = () => api.get("/orders");
export const getOrderById = (id) => api.get(`/orders/${id}`);
export const getOrdersByCustomer = (id) => api.get(`/orders/customer/${id}`);
export const getOrdersByStatus = (statusId) =>
  api.get(`/orders/status/${statusId}`);
export const createOrder = (data) => api.post("/orders", data);
export const updateOrder = (id, data) => api.put(`/orders/${id}`, data);
export const updateOrderStatus = (id, statusId) =>
  api.put(`/orders/${id}/status`, statusId);
// responseType: 'blob' — all export/pdf calls return binary file data
export const getOrderPdf = (id) =>
  api.get(`/orders/${id}/pdf`, { responseType: "blob" });
export const exportOrdersExcel = () =>
  api.get("/orders/export/excel", { responseType: "blob" });
export const exportOrdersPdf = () =>
  api.get("/orders/export/pdf", { responseType: "blob" });
// Triggers geocoding of ship/bill addresses for one order
export const geocodeOrder = (id) => api.post(`/orders/${id}/geocode`);
// Bulk-geocodes all orders that still lack coordinates
export const geocodeAllOrders = () => api.post("/orders/geocode-all");
export const deactivateOrder = (id) => api.put(`/orders/${id}/deactivate`);
