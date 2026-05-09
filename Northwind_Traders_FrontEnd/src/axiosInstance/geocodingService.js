import api from "./index.js";

// Sends a free-form address to the backend, which validates it via Google Maps
export const validateAddress = (address) =>
  api.post("/geocoding/validate-address", { address });
