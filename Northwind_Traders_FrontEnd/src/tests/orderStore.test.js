import { describe, it, expect, vi, beforeEach } from "vitest";
import { setActivePinia, createPinia } from "pinia";
import { useOrderStore } from "../stores/orderStore.js";

// ── Mock all axios service calls ──────────────────────────────
vi.mock("../axiosInstance/orderService.js", () => ({
  getAllOrders: vi.fn(),
  getOrderById: vi.fn(),
  createOrder: vi.fn(),
  updateOrder: vi.fn(),
  updateOrderStatus: vi.fn(),
  exportOrdersExcel: vi.fn(),
  geocodeOrder: vi.fn(),
}));

import {
  getAllOrders,
  getOrderById,
  createOrder,
  updateOrder,
  updateOrderStatus,
  geocodeOrder,
} from "../axiosInstance/orderService.js";

// ─────────────────────────────────────────────────────────────
describe("orderStore", () => {
  beforeEach(() => {
    // Fresh Pinia for every test — no state leaks between tests
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  // ── fetchOrders ─────────────────────────────────────────────

  it("fetchOrders — sets orders and clears loading on success", async () => {
    // ARRANGE
    const fakeOrders = [
      { orderId: 1, customerId: "ALFKI" },
      { orderId: 2, customerId: "WOLZA" },
    ];
    getAllOrders.mockResolvedValue({ data: fakeOrders });

    const store = useOrderStore();

    // ACT
    await store.fetchOrders();

    // ASSERT
    expect(store.orders).toEqual(fakeOrders); // orders populated
    expect(store.loading).toBe(false); // loading reset
    expect(store.error).toBeNull(); // no error
  });

  it("fetchOrders — sets error and clears loading on failure", async () => {
    // ARRANGE
    const fakeError = new Error("Network error");
    getAllOrders.mockRejectedValue(fakeError);

    const store = useOrderStore();

    // ACT & ASSERT
    await expect(store.fetchOrders()).rejects.toThrow("Network error");
    expect(store.error).toBe(fakeError); // error stored
    expect(store.loading).toBe(false); // loading still reset
  });

  it("fetchOrders — sets loading to true during fetch", async () => {
    // ARRANGE — never resolves so we can check loading mid-flight
    let loadingDuringFetch = false;
    getAllOrders.mockImplementation(async () => {
      loadingDuringFetch = useOrderStore().loading;
      return { data: [] };
    });

    const store = useOrderStore();

    // ACT
    await store.fetchOrders();

    // ASSERT
    expect(loadingDuringFetch).toBe(true); // was true during the call
    expect(store.loading).toBe(false); // false after
  });

  // ── fetchOrder ──────────────────────────────────────────────

  it("fetchOrder — sets current order on success", async () => {
    // ARRANGE
    const fakeOrder = { orderId: 10248, customerId: "VINET" };
    getOrderById.mockResolvedValue({ data: fakeOrder });

    const store = useOrderStore();

    // ACT
    const result = await store.fetchOrder(10248);

    // ASSERT
    expect(store.current).toEqual(fakeOrder);
    expect(result).toEqual(fakeOrder);
  });

  it("fetchOrder — throws and stores error on failure", async () => {
    // ARRANGE
    getOrderById.mockRejectedValue(new Error("Not found"));

    const store = useOrderStore();

    // ACT & ASSERT
    await expect(store.fetchOrder(999)).rejects.toThrow("Not found");
    expect(store.error).toBeTruthy();
  });

  // ── submitCreateOrder ───────────────────────────────────────

  it("submitCreateOrder — calls createOrder and then geocodeOrder", async () => {
    // ARRANGE
    const fakeCreated = { orderId: 1 };
    createOrder.mockResolvedValue({ data: fakeCreated });
    geocodeOrder.mockResolvedValue({});

    const store = useOrderStore();

    // ACT
    const result = await store.submitCreateOrder({ lines: [{ productId: 1 }] });

    // ASSERT
    expect(createOrder).toHaveBeenCalledTimes(1);
    expect(geocodeOrder).toHaveBeenCalledWith(1); // geocode called with new orderId
    expect(result).toEqual(fakeCreated);
  });

  it("submitCreateOrder — silently ignores geocode failure", async () => {
    // ARRANGE — order creates fine but geocode crashes
    createOrder.mockResolvedValue({ data: { orderId: 5 } });
    geocodeOrder.mockRejectedValue(new Error("Google Maps down"));

    const store = useOrderStore();

    // ACT — should NOT throw even though geocode failed
    const result = await store.submitCreateOrder({ lines: [{ productId: 1 }] });

    // ASSERT
    expect(result.orderId).toBe(5); // order returned fine
    expect(geocodeOrder).toHaveBeenCalled(); // geocode was attempted
  });

  // ── submitUpdateStatus ──────────────────────────────────────

  it("submitUpdateStatus — calls updateOrderStatus with correct args", async () => {
    // ARRANGE
    updateOrderStatus.mockResolvedValue({});

    const store = useOrderStore();

    // ACT
    await store.submitUpdateStatus(10, 3);

    // ASSERT
    expect(updateOrderStatus).toHaveBeenCalledWith(10, 3);
  });

  // ── submitUpdateOrder ───────────────────────────────────────

  it("submitUpdateOrder — calls updateOrder and returns data", async () => {
    // ARRANGE
    const fakeUpdated = { orderId: 1, freight: 99 };
    updateOrder.mockResolvedValue({ data: fakeUpdated });

    const store = useOrderStore();

    // ACT
    const result = await store.submitUpdateOrder(1, { freight: 99 });

    // ASSERT
    expect(updateOrder).toHaveBeenCalledWith(1, { freight: 99 });
    expect(result).toEqual(fakeUpdated);
  });
});
