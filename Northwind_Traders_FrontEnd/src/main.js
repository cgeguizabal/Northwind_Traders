import { createApp } from "vue";
import { createPinia } from "pinia";
import router from "./router/index.js";
import Toast, { POSITION } from "vue-toastification";
import "vue-toastification/dist/index.css";
import "./assets/styles/main.scss";
import App from "./App.vue";

const app = createApp(App);

app.use(createPinia());
app.use(router);
app.use(Toast, {
  position: POSITION.BOTTOM_RIGHT,
  timeout: 4000,
  closeOnClick: true,
  pauseOnHover: true,
  draggable: true,
  hideProgressBar: false,
});

app.mount("#app");
