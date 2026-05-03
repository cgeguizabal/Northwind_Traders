<script setup>
import { computed } from "vue";
import { Doughnut } from "vue-chartjs";
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from "chart.js";

ChartJS.register(ArcElement, Tooltip, Legend);

const props = defineProps({
  labels: { type: Array, required: true },
  data: { type: Array, required: true },
  title: { type: String, default: "" },
});

const chartData = computed(() => ({
  labels: props.labels,
  datasets: [
    {
      data: props.data,
      backgroundColor: [
        "rgba(124,58,237,0.8)",
        "rgba(167,139,250,0.8)",
        "rgba(245,158,11,0.8)",
        "rgba(16,185,129,0.8)",
        "rgba(239,68,68,0.8)",
        "rgba(59,130,246,0.8)",
        "rgba(236,72,153,0.8)",
        "rgba(14,165,233,0.8)",
      ],
      borderColor: "rgba(255,255,255,0.05)",
      borderWidth: 2,
    },
  ],
}));

const options = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: "bottom",
      labels: { color: "#e2e0ff", padding: 12, font: { size: 12 } },
    },
    tooltip: {
      callbacks: {
        label: (ctx) => ` ${ctx.label}: ${ctx.parsed}`,
      },
    },
  },
  cutout: "60%",
};
</script>

<template>
  <div class="chart-wrapper">
    <h3 v-if="title" class="chart-title">{{ title }}</h3>
    <div class="chart-canvas">
      <Doughnut :data="chartData" :options="options" />
    </div>
  </div>
</template>

<style
  lang="scss"
  src="../../assets/styles/Components/DonutChart.scss"
  scoped
></style>
