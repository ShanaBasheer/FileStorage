# Stage 1: Build Angular app
FROM node:20 AS build
WORKDIR /app
COPY package*.json ./
RUN npm install --legacy-peer-deps
COPY . .
RUN npm run build -- --configuration production

# Stage 2: Serve with Nginx
FROM nginx:alpine
# Add metadata labels
LABEL maintainer="Shana"
LABEL project="Flora File Storage"
LABEL version="1.0.0"
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/file-storage-ui/browser /usr/share/nginx/html
EXPOSE 80