<script setup>
import { ref, onMounted, onUnmounted } from "vue";

const props = defineProps({
  lat: { type: Number, default: null },
  lng: { type: Number, default: null },
});

const mapEl = ref(null);
let mapObj = null;
let marker = null;

// Load Google Maps script dynamically (only once)
function loadGoogleMaps() {
  return new Promise((resolve) => {
    if (window.google?.maps) {
      resolve();
      return;
    }
    const key = import.meta.env.VITE_GOOGLE_MAPS_KEY;
    const script = document.createElement("script");
    script.src = `https://maps.googleapis.com/maps/api/js?key=${key}`;
    script.async = true;
    script.defer = true;
    script.onload = resolve;
    document.head.appendChild(script);
  });
}

async function initMap() {
  await loadGoogleMaps();
  const center =
    props.lat && props.lng
      ? { lat: props.lat, lng: props.lng }
      : { lat: 48.8566, lng: 2.3522 }; // Default: Paris

  mapObj = new window.google.maps.Map(mapEl.value, {
    center,
    zoom: 8,
    mapTypeId: "roadmap",
    styles: darkMapStyles,
  });

  if (props.lat && props.lng) {
    marker = new window.google.maps.Marker({
      position: center,
      map: mapObj,
      title: "Ship Location",
    });
  }
}

onMounted(initMap);
onUnmounted(() => {
  mapObj = null;
  marker = null;
});

// Dark map style
const darkMapStyles = [
  { elementType: "geometry", stylers: [{ color: "#1a1a2e" }] },
  { elementType: "labels.text.fill", stylers: [{ color: "#746855" }] },
  {
    featureType: "water",
    elementType: "geometry",
    stylers: [{ color: "#0f0f1a" }],
  },
  {
    featureType: "road",
    elementType: "geometry",
    stylers: [{ color: "#2c2c44" }],
  },
];
</script>

<template>
  <div ref="mapEl" class="order-map" />
</template>

<style
  lang="scss"
  src="../../assets/styles/Components/OrderMap.scss"
  scoped
></style>
