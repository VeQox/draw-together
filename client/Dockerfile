FROM node:20
WORKDIR /app

EXPOSE 80
EXPOSE 443

ENV HOST=0.0.0.0
ENV PORT=80

COPY . .
RUN npm ci
RUN npm run build

CMD ["node", "build"]