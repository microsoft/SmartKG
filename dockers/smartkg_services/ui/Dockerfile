FROM node

COPY smartkgui.zip /tmp/
RUN unzip /tmp/smartkgui.zip -d /app

COPY config.js /app/smartkgui/public/

CMD cd /app/smartkgui && npm i && npm run serve