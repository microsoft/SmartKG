FROM node:7.7.2-alpine

COPY smartkgUI.zip /tmp/
RUN unzip /tmp/smartkgUI.zip -d /app

CMD cd /app/smartkgUI
CMD npm i && npm run serve