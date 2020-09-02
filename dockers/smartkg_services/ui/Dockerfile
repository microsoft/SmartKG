FROM node

ARG DOCKER_HOST

COPY smartkgui.zip /tmp/
RUN unzip /tmp/smartkgui.zip -d /app

COPY config.js /app/smartkgui/public/

RUN sed -i "s/localhost/${DOCKER_HOST}/g"  /app/smartkgui/public/config.js

CMD cd /app/smartkgui && npm i && npm run serve