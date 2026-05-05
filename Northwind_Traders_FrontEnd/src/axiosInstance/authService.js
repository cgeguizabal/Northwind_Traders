import api from "./index.js";

export const login = (credentials) => api.post("/auth/login", credentials);
export const changePassword = (data) => api.post("/auth/change-password", data);
