<script setup>
import { ref, onMounted, onUnmounted } from "vue";

const props = defineProps({
  pins: { type: Array, default: () => [] }, // [{ lat, lng, orderId, shipCity }]
});

const mapEl = ref(null);
let mapObj = null;
const markers = [];

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
    script.onload = resolve;
    document.head.appendChild(script);
  });
}

async function initMap() {
  await loadGoogleMaps();
  mapObj = new window.google.maps.Map(mapEl.value, {
    center: { lat: 48.8566, lng: 2.3522 },
    zoom: 3,
    mapTypeId: "roadmap",
    styles: darkMapStyles,
  });

  // Normalise pin coordinates – API returns shipLatitude/shipLongitude as decimals
  const validPins = props.pins
    .map((p) => ({
      ...p,
      lat:
        p.lat != null
          ? Number(p.lat)
          : p.shipLatitude != null
            ? Number(p.shipLatitude)
            : null,
      lng:
        p.lng != null
          ? Number(p.lng)
          : p.shipLongitude != null
            ? Number(p.shipLongitude)
            : null,
    }))
    .filter((p) => p.lat != null && p.lng != null);
  validPins.forEach((pin) => {
    const m = new window.google.maps.Marker({
      position: { lat: pin.lat, lng: pin.lng },
      map: mapObj,
      title: `Order #${pin.orderId} — ${pin.shipName || ""}`,
    });
    const info = new window.google.maps.InfoWindow({
      content: `<div style="color:#1e1b4b;font-size:13px;"><b>Order #${pin.orderId}</b><br/>${pin.shipName || ""}</div>`,
    });
    m.addListener("click", () => info.open(mapObj, m));
    markers.push(m);
  });

  // Auto-fit bounds if we have pins
  if (validPins.length > 1) {
    const bounds = new window.google.maps.LatLngBounds();
    validPins.forEach((p) => bounds.extend({ lat: p.lat, lng: p.lng }));
    mapObj.fitBounds(bounds);
  } else if (validPins.length === 1) {
    mapObj.setCenter({ lat: validPins[0].lat, lng: validPins[0].lng });
    mapObj.setZoom(8);
  }
}

onMounted(initMap);
onUnmounted(() => {
  mapObj = null;
  markers.length = 0;
});

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
  <div ref="mapEl" class="customer-map" />
</template>

<style
  lang="scss"
  src="../../assets/styles/Components/CustomerMap.scss"
  scoped
></style>
