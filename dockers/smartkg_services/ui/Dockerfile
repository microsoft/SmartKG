FROM node

COPY smartkgui.zip /tmp/
RUN unzip /tmp/smartkgui.zip -d /app

CMD cd /app/smartkgui && npm i && npm run serve