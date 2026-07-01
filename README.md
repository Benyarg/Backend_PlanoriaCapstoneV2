# Planoria Backend API

API ASP.NET Core 8 para Planoria - plataforma de estudio con IA.

## Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Git

## Variables de entorno

Copia `.env.example` a `.env` y edita con tu API key de Gemini:

```bash
cp .env.example .env
```

| Variable | Descripción |
|---|---|
| `PLANORIA_GEMINI_KEY` | API Key de Google Gemini (obtener en [Google AI Studio](https://aistudio.google.com/)) |

## Cómo levantar

Desde la raíz del proyecto:

```bash
docker compose up -d --build
```

Esto levanta dos contenedores:

| Servicio | Puerto | Descripción |
|---|---|---|
| `planoria-sqlserver` | `1433` | SQL Server 2022 |
| `planoria-backend` | `7075` | API ASP.NET Core 8 |

## Acceder

- **Swagger UI:** http://localhost:7075/swagger
- **API Base:** http://localhost:7075

## Conexión a SQL Server

| Campo | Valor |
|---|---|
| Server | `localhost,1433` |
| Database | `PlanoriaDatabase` |
| User | `sa` |
| Password | `Planoria123*` |
| Auth | SQL Server Authentication |

## Comandos útiles

```bash
# Ver logs del backend en tiempo real
docker logs planoria-backend -f --tail 20

# Reiniciar solo backend (ej: después de cambiar API key)
docker-compose restart backend

# Reconstruir todo (backend + bdd)
docker compose up -d --build

# Reconstruir SOLO el backend (sin tocar la bdd)
docker compose up -d --build backend

# Detener contenedores (los datos persisten)
docker compose down

# Detener y borrar datos
docker compose down -v
```

## Notas

- El `.env` está en `.gitignore`. Cada desarrollador debe crear su propio `.env` desde `.env.example`.
- El backend aplica migraciones automáticamente al iniciar.
- Los datos del SQL Server persisten gracias al volumen `sqlserver-data`.
- La API key de Gemini en el `.env` usa la variable `PLANORIA_GEMINI_KEY` para evitar conflictos con variables de entorno del sistema.
