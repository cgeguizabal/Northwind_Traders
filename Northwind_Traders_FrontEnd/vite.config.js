import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import { fileURLToPath, URL } from "node:url";

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],

  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },

  server: {
    port: 5173,
    // Proxy API calls during dev to avoid CORS issues
    proxy: {
      "/api": {
        target: "http://localhost:5272",
        changeOrigin: true,
      },
    },
  },
  // ── Vitest configuration ──────────────────────────────────────
  test: {
    environment: "jsdom", // simulates a browser DOM in Node
    globals: true, // lets you use describe/it/expect without importing
  },
});
