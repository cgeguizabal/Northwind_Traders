import api from "./index.js";

export const getDashboardStats = (params) => api.get("/dashboard", { params });
