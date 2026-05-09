# ── Stage 1: Build ────────────────────────────────────────────────
FROM node:lts-alpine AS build
WORKDIR /app

ARG VITE_GOOGLE_MAPS_KEY
ENV VITE_GOOGLE_MAPS_KEY=$VITE_GOOGLE_MAPS_KEY

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# ── Stage 2: Serve with Nginx ─────────────────────────────────────
FROM nginx:stable-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80