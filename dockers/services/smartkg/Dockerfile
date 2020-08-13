FROM aspnetcore

COPY SmartKGLocalBase.zip /tmp/
RUN unzip /tmp/SmartKGLocalBase.zip -d /app

COPY smartkg_backend.zip /tmp/
RUN unzip /tmp/smartkg_backend.zip -d /app

COPY appsettings.json /app/smartkg_backend/AppSettings/

ENV LD_LIBRARY_PATH=/app/smartkg_backend/lib64:$LD_LIBRARY_PATH

EXPOSE 5305

CMD cd /app/smartkg_backend && \
    dotnet SmartKG.KGBot.dll