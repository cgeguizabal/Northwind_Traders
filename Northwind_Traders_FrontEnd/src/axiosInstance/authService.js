import api from "./index.js";

// POST /auth/login — returns JWT token and employee info
export const login = (credentials) => api.post("/auth/login", credentials);
// POST /auth/change-password — requires Authorization header (JWT)
export const changePassword = (data) => api.post("/auth/change-password", data);
