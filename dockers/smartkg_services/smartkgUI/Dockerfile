FROM node:7.7.2-alpine

COPY smartkgui.zip /tmp/
RUN unzip /tmp/smartkgui.zip -d /app

CMD cd /app/smartkgui && npm i && npm run serve