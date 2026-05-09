import api from "./index.js";

// params = { dateFrom, dateTo } — both optional ISO date strings
export const getDashboardStats = (params) => api.get("/dashboard", { params });
