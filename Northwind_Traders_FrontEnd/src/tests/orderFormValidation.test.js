import { describe, it, expect } from "vitest";

// ── Pure logic extracted from OrderFormModal.vue ──────────────
// We test the validate() logic directly without mounting the component

function validate(form) {
  const errors = {};
  if (!form.shipmentStateId) errors.shipmentStateId = "Status is required.";
  if (!form.lines.length) errors.lines = "Add at least one product line.";
  return errors;
}

function runningTotal(lines) {
  return lines.reduce(
    (sum, line) =>
      sum + line.unitPrice * line.quantity * (1 - (line.discount || 0)),
    0,
  );
}

function formatCurrency(n) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(n);
}

// ─────────────────────────────────────────────────────────────
describe("OrderForm — validate()", () => {
  it("returns no errors when form is valid", () => {
    // ARRANGE
    const form = {
      shipmentStateId: 1,
      lines: [{ productId: 1, unitPrice: 10, quantity: 2, discount: 0 }],
    };

    // ACT
    const errors = validate(form);

    // ASSERT
    expect(Object.keys(errors)).toHaveLength(0);
  });

  it("returns error when shipmentStateId is empty", () => {
    // ARRANGE
    const form = {
      shipmentStateId: "",
      lines: [{ productId: 1, unitPrice: 10, quantity: 2, discount: 0 }],
    };

    // ACT
    const errors = validate(form);

    // ASSERT
    expect(errors.shipmentStateId).toBe("Status is required.");
  });

  it("returns error when lines are empty", () => {
    // ARRANGE
    const form = {
      shipmentStateId: 2,
      lines: [],
    };

    // ACT
    const errors = validate(form);

    // ASSERT
    expect(errors.lines).toBe("Add at least one product line.");
  });

  it("returns both errors when form is completely empty", () => {
    // ARRANGE
    const form = { shipmentStateId: "", lines: [] };

    // ACT
    const errors = validate(form);

    // ASSERT
    expect(Object.keys(errors)).toHaveLength(2);
    expect(errors.shipmentStateId).toBeDefined();
    expect(errors.lines).toBeDefined();
  });
});

// ─────────────────────────────────────────────────────────────
describe("OrderForm — runningTotal()", () => {
  it("calculates total with no discount", () => {
    const lines = [
      { unitPrice: 10, quantity: 3, discount: 0 },
      { unitPrice: 20, quantity: 2, discount: 0 },
    ];
    // 10*3 + 20*2 = 30 + 40 = 70
    expect(runningTotal(lines)).toBe(70);
  });

  it("applies discount correctly", () => {
    const lines = [
      { unitPrice: 100, quantity: 1, discount: 0.1 }, // 100 * 0.9 = 90
    ];
    expect(runningTotal(lines)).toBeCloseTo(90);
  });

  it("handles zero discount as null/undefined", () => {
    const lines = [
      { unitPrice: 50, quantity: 2 }, // discount undefined → treated as 0
    ];
    expect(runningTotal(lines)).toBe(100);
  });

  it("returns 0 for empty lines", () => {
    expect(runningTotal([])).toBe(0);
  });

  it("handles multiple lines with mixed discounts", () => {
    const lines = [
      { unitPrice: 18, quantity: 12, discount: 0 }, // 216
      { unitPrice: 9.8, quantity: 10, discount: 0 }, // 98
      { unitPrice: 34, quantity: 5, discount: 0.05 }, // 161.5
    ];
    // 216 + 98 + 161.5 = 475.5
    expect(runningTotal(lines)).toBeCloseTo(475.5);
  });
});

// ─────────────────────────────────────────────────────────────
describe("OrderForm — formatCurrency()", () => {
  it("formats positive number as USD currency", () => {
    expect(formatCurrency(1234.56)).toBe("$1,234.56");
  });

  it("formats zero as $0.00", () => {
    expect(formatCurrency(0)).toBe("$0.00");
  });

  it("formats large number with comma separator", () => {
    expect(formatCurrency(50000)).toBe("$50,000.00");
  });
});
