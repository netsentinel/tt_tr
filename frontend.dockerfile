FROM node:23-alpine as build

WORKDIR /app
COPY ./frontend/package.json ./package.json
COPY ./frontend/package-lock.json ./package-lock.json
RUN npm i

COPY ./frontend/src ./src
COPY ./frontend/*.html ./
COPY ./frontend/*.js* ./
COPY ./frontend/*.ts* ./

RUN npm run build



FROM nginx:1.27-alpine as final

WORKDIR /usr/share/nginx/html
RUN rm -rf *

COPY --from=build /app/dist /usr/share/nginx/html
COPY ./frontend/nginx.conf /etc/nginx/nginx.conf

CMD ["nginx", "-g", "daemon off;"]