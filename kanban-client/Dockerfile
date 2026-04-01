# Stage 1 — build Angular
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json .
RUN npm ci

COPY . .
RUN npm run build

# Stage 2 — serve with Nginx
FROM nginx:alpine
COPY --from=build /app/dist/kanban-client/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
