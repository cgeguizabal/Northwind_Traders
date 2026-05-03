<script setup>
import { computed } from "vue";
import { Bar } from "vue-chartjs";
import {
  Chart as ChartJS,
  BarElement,
  CategoryScale,
  LinearScale,
  Tooltip,
  Legend,
} from "chart.js";

ChartJS.register(BarElement, CategoryScale, LinearScale, Tooltip, Legend);

const props = defineProps({
  labels: { type: Array, required: true },
  data: { type: Array, required: true },
  label: { type: String, default: "Value" },
  title: { type: String, default: "" },
  color: { type: String, default: "rgba(124,58,237,0.75)" },
});

const chartData = computed(() => ({
  labels: props.labels,
  datasets: [
    {
      label: props.label,
      data: props.data,
      backgroundColor: props.color,
      borderRadius: 4,
      borderSkipped: false,
    },
  ],
}));

const options = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { labels: { color: "#e2e0ff", font: { size: 12 } } },
  },
  scales: {
    x: {
      ticks: { color: "#9ca3af", maxRotation: 45 },
      grid: { color: "rgba(255,255,255,0.05)" },
    },
    y: {
      ticks: { color: "#9ca3af" },
      grid: { color: "rgba(255,255,255,0.05)" },
    },
  },
};
</script>

<template>
  <div class="chart-wrapper">
    <h3 v-if="title" class="chart-title">{{ title }}</h3>
    <div class="chart-canvas">
      <Bar :data="chartData" :options="options" />
    </div>
  </div>
</template>

<style
  lang="scss"
  src="../../assets/styles/Components/BarChart.scss"
  scoped
></style>
