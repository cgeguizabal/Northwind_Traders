import { describe, it, expect, vi, beforeEach } from "vitest";
import { setActivePinia, createPinia } from "pinia";
import { useAuthStore } from "../stores/authStore.js";

// ── Mock API calls and jwt-decode ─────────────────────────────
vi.mock("../axiosInstance/authService.js", () => ({
  login: vi.fn(),
}));

vi.mock("../axiosInstance/employeeService.js", () => ({
  getEmployeeById: vi.fn(),
}));

vi.mock("jwt-decode", () => ({
  jwtDecode: vi.fn(),
}));

import { login } from "../axiosInstance/authService.js";
import { getEmployeeById } from "../axiosInstance/employeeService.js";
import { jwtDecode } from "jwt-decode";

// ─────────────────────────────────────────────────────────────
describe("authStore", () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
    localStorage.clear();
  });

  // ── isAuthenticated ─────────────────────────────────────────

  it("isAuthenticated — false when no token", () => {
    const store = useAuthStore();
    expect(store.isAuthenticated).toBe(false);
  });

  it("isAuthenticated — true after successful login", async () => {
    // ARRANGE
    const fakeDecoded = {
      nameid: "1",
      unique_name: "Nancy Davolio",
      exp: Math.floor(Date.now() / 1000) + 3600,
    };
    login.mockResolvedValue({ data: { token: "fake.jwt.token" } });
    jwtDecode.mockReturnValue(fakeDecoded);
    getEmployeeById.mockResolvedValue({ data: { title: "Sales Manager" } });

    const store = useAuthStore();

    // ACT
    await store.loginUser({ email: "nancy@northwind.com", password: "pass" });

    // ASSERT
    expect(store.isAuthenticated).toBe(true);
    expect(store.token).toBe("fake.jwt.token");
  });

  // ── displayName ─────────────────────────────────────────────

  it("displayName — returns unique_name from decoded token", async () => {
    // ARRANGE
    login.mockResolvedValue({ data: { token: "fake.jwt.token" } });
    jwtDecode.mockReturnValue({
      nameid: "1",
      unique_name: "Nancy Davolio",
      exp: Math.floor(Date.now() / 1000) + 3600,
    });
    getEmployeeById.mockResolvedValue({ data: { title: "Sales Rep" } });

    const store = useAuthStore();
    await store.loginUser({ email: "nancy@northwind.com", password: "pass" });

    // ASSERT
    expect(store.displayName).toBe("Nancy Davolio");
  });

  it('displayName — falls back to "Employee" when name is missing', () => {
    const store = useAuthStore();
    // user is null — no token
    expect(store.displayName).toBe("Employee");
  });

  // ── isManager ───────────────────────────────────────────────

  it("isManager — true when title is Sales Manager", async () => {
    // ARRANGE
    login.mockResolvedValue({ data: { token: "fake.jwt.token" } });
    jwtDecode.mockReturnValue({
      nameid: "1",
      unique_name: "Nancy",
      exp: Date.now() / 1000 + 3600,
    });
    getEmployeeById.mockResolvedValue({ data: { title: "Sales Manager" } });

    const store = useAuthStore();
    await store.loginUser({ email: "test@test.com", password: "pass" });

    // ASSERT
    expect(store.isManager).toBe(true);
  });

  it("isManager — true when title is Vice President, Sales", async () => {
    // ARRANGE
    login.mockResolvedValue({ data: { token: "fake.jwt.token" } });
    jwtDecode.mockReturnValue({
      nameid: "1",
      unique_name: "Andrew",
      exp: Date.now() / 1000 + 3600,
    });
    getEmployeeById.mockResolvedValue({
      data: { title: "Vice President, Sales" },
    });

    const store = useAuthStore();
    await store.loginUser({ email: "test@test.com", password: "pass" });

    // ASSERT
    expect(store.isManager).toBe(true);
  });

  it("isManager — false for regular employee", async () => {
    // ARRANGE
    login.mockResolvedValue({ data: { token: "fake.jwt.token" } });
    jwtDecode.mockReturnValue({
      nameid: "1",
      unique_name: "Janet",
      exp: Date.now() / 1000 + 3600,
    });
    getEmployeeById.mockResolvedValue({
      data: { title: "Sales Representative" },
    });

    const store = useAuthStore();
    await store.loginUser({ email: "test@test.com", password: "pass" });

    // ASSERT
    expect(store.isManager).toBe(false);
  });

  // ── logout ──────────────────────────────────────────────────

  it("logout — clears token, user and localStorage", async () => {
    // ARRANGE — log in first
    login.mockResolvedValue({ data: { token: "fake.jwt.token" } });
    jwtDecode.mockReturnValue({
      nameid: "1",
      unique_name: "Nancy",
      exp: Date.now() / 1000 + 3600,
    });
    getEmployeeById.mockResolvedValue({ data: { title: "Sales Manager" } });

    const store = useAuthStore();
    await store.loginUser({ email: "test@test.com", password: "pass" });

    // ACT
    store.logout();

    // ASSERT
    expect(store.token).toBeNull();
    expect(store.user).toBeNull();
    expect(store.isAuthenticated).toBe(false);
    expect(localStorage.getItem("nt_token")).toBeNull();
  });
});
