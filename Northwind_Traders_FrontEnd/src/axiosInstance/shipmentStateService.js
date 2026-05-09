import api from "./index.js";

// Returns all shipment states (Pending, Shipped, Completed, Cancelled)
export const getAllShipmentStates = () => api.get("/shipmentstates");
