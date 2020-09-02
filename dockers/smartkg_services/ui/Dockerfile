FROM node

ARG DOCKER_HOST

COPY smartkgui.zip /tmp/
RUN unzip /tmp/smartkgui.zip -d /app

RUN -i 's/localhost/${DOCKER_HOST}/g'  config.js

COPY config.js /app/smartkgui/public/

CMD cd /app/smartkgui && npm i && npm run serve