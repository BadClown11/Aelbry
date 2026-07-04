#!/bin/bash
# Espera a que SQL Server acepte conexiones y luego aplica todos los scripts numerados de
# database/ en orden. Pensado para correr como servicio "db-init" de docker-compose.yml,
# usando la misma imagen de mssql/server (trae sqlcmd) contra el servicio "sqlserver".
set -e

SQLCMD=$(command -v sqlcmd || echo /opt/mssql-tools18/bin/sqlcmd)
SQLCMD_EXTRA_ARGS=""
if [ "$SQLCMD" = "/opt/mssql-tools18/bin/sqlcmd" ]; then
    SQLCMD_EXTRA_ARGS="-C"
fi
if [ ! -x "$SQLCMD" ]; then
    SQLCMD=/opt/mssql-tools/bin/sqlcmd
fi

echo "Esperando a que SQL Server ($SQLSERVER_HOST) acepte conexiones..."
until "$SQLCMD" -S "$SQLSERVER_HOST" -U sa -P "$SA_PASSWORD" $SQLCMD_EXTRA_ARGS -Q "SELECT 1" > /dev/null 2>&1; do
    sleep 2
done

echo "SQL Server listo. Aplicando scripts de /scripts en orden..."
for f in $(ls /scripts/*.sql | sort); do
    echo "-> $f"
    "$SQLCMD" -S "$SQLSERVER_HOST" -U sa -P "$SA_PASSWORD" $SQLCMD_EXTRA_ARGS -i "$f"
done

echo "Listo: base de datos inicializada."
