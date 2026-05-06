import api from "./index.js";

export const validateAddress = (address) =>
  api.post("/geocoding/validate-address", { address });
