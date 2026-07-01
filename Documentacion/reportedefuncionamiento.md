# Reporte de Funcionamiento - Planoria Backend API

## 1. Resumen Técnico

| Aspecto | Detalle |
|---|---|
| **Lenguaje** | C# (.NET 8.0) |
| **Framework** | ASP.NET Core 8.0 Web API |
| **Arquitectura** | Capas (Controller → BLL → DAL → EF Core → SQL Server) |
| **Base de Datos** | SQL Server 2022 (Entity Framework Core) |
| **Autenticación** | JWT Bearer Tokens |
| **IA** | Google Gemini API |
| **Reportes PDF** | iText7 |
| **Testing** | MSTest |
| **Contenedores** | Docker + docker-compose |

---

## 2. Arquitectura del Proyecto

```
Backend_PlanoriaCapstone (API - punto de entrada)
    ↓
PlanoriaCapstone.Bll (BLL - lógica de negocio / servicios)
    ↓
PlanoriaCapstone.Dal (DAL - repositorios / DbContext)
    ↓
PlanoriaCapstone.Models (Entidades del dominio)
     ↑
PlanoriaCapstone.DTOs (DTOs - contratos de entrada/salida)
```

**Dependencias entre proyectos:**
- `Backend_PlanoriaCapstone` → `Bll` → `Dal` → `Models`
- `Bll` → `DTOs`, `Models`
- `PlanoriaCapstone.Tests` → `Backend_PlanoriaCapstone`

---

## 3. Punto de Entrada (`Program.cs`)

El archivo `Backend_PlanoriaCapstone/Program.cs` configura:

1. **Base de Datos**: SQL Server mediante `AppDbContext` con EF Core
2. **Repositorios (11)**: Scoped services para cada entidad
3. **Servicios BLL (21)**: Scoped services con lógica de negocio
4. **Controladores**: Con `ReferenceHandler.IgnoreCycles` y `JsonIgnoreCondition.WhenWritingNull`
5. **Swagger/OpenAPI**: Con definición de seguridad JWT Bearer
6. **JWT**: Validación de issuer, audience, lifetime, signing key
7. **CORS**: Política `AllowAll` (cualquier origen, método, header)
8. **Auto-Migrate**: Aplica migraciones al iniciar con hasta 10 reintentos
9. **Static Files**: Servir archivos desde `wwwroot`
10. **HTTPS**: Solo en entorno de desarrollo

---

## 4. Modelos de Dominio (Entidades) - 25 Tablas

### 4.1 Usuarios y Cursos

| Entidad | Tabla | Propiedades clave |
|---|---|---|
| `User` | `Users` | Id, FullName, Email, PasswordHash, PreferredLanguage, Theme, Timezone, NotificationEnabled, EmailNotifications, DefaultSpacedRepetitionDays, CreatedAt, UpdatedAt, DeletedAt (soft delete) |
| `UserCourse` | `UserCourses` | Id, UserId, CourseId, Role ("owner"), JoinedAt |
| `Course` | `Courses` | Id, UserId, Name, Description, ExamDate, ExamTime, ColorHex, IsArchived, CreatedAt, UpdatedAt |
| `UserCourseExamProgress` | `UserCourseExamProgresses` | UserId, CourseId, TotalFlashcards, FlashcardsStudied, FlashcardsMastered, TotalQuizzes, QuizzesCompleted, QuizzesPassed, ExamReadinessScore |
| `ExamReadinessScore` | `ExamReadinessScores` | UserId, CourseId, Score, DaysUntilExam, CalculatedAt |

### 4.2 Flashcards y Repetición Espaciada

| Entidad | Tabla | Propiedades clave |
|---|---|---|
| `FlashcardDeck` | `FlashcardDecks` | Id, CourseId, Name, Description, TotalCards, SpacedRepetitionEnabled |
| `Flashcard` | `Flashcards` | Id, DeckId, Question, Answer, Difficulty, Tags, Position |
| `FlashcardStudySession` | `FlashcardStudySessions` | Id, UserId, DeckId, StartedAt, EndedAt, CardsReviewed, CardsKnown, CardsUnknown, SessionType |
| `FlashcardReview` | `FlashcardReviews` | Id, FlashcardId, SessionId, UserId, KnewIt, ResponseTimeMs, EaseFactor, IntervalDays, NextReviewDate |
| `SpacedRepetitionSetting` | `SpacedRepetitionSettings` | Id, UserId, DeckId, InitialIntervalDays, MaxIntervalDays, EasyBonus, HardPenalty |
| `UserProgressFlashcard` | `UserProgressFlashcards` | UserId, DeckId, TotalStudySessions, TotalReviews, CardsMastered, CardsInLearning, AverageEaseFactor |

### 4.3 Quizzes

| Entidad | Tabla | Propiedades clave |
|---|---|---|
| `Quiz` | `Quizzes` | Id, CourseId, Title, Description, TotalQuestions, PassingScore (70%), TimeLimitMinutes, ShuffleQuestions, ShuffleOptions, AttemptsAllowed |
| `QuizQuestion` | `QuizQuestions` | Id, QuizId, QuestionText, QuestionType, Explanation, Points, OrderPosition |
| `QuizOption` | `QuizOptions` | Id, QuestionId, OptionText, IsCorrect, OrderPosition |
| `QuizAttempt` | `QuizAttempts` | Id, UserId, QuizId, StartedAt, CompletedAt, ScorePercentage, Passed, TimeSpentSeconds |
| `QuizAnswer` | `QuizAnswers` | Id, AttemptId, QuestionId, SelectedOptionId, ShortAnswerText, IsCorrect, PointsEarned |
| `UserProgressQuiz` | `UserProgressQuizzes` | UserId, QuizId, TotalAttempts, BestScore, AverageScore, PassedCount |

### 4.4 Archivos e IA

| Entidad | Tabla | Propiedades clave |
|---|---|---|
| `FileUpload` | `FileUploads` | UserId, OriginalFilename, FilePath, FileSizeBytes, FileType, MimeType, UploadedAt, ProcessedAt |
| `GeneratedContent` | `GeneratedContents` | FileUploadId, CourseId, ContentType ("flashcard"/"quiz"), GeneratedEntityId, TopicSpecified, GenerationConfig |

### 4.5 Cronograma de Estudio

| Entidad | Tabla | Propiedades clave |
|---|---|---|
| `StudySchedule` | `StudySchedules` | UserId, Title, StartDatetime, EndDatetime, IsCompleted, NotificationSent |
| `ScheduleInterval` | `ScheduleIntervals` | ScheduleId, IntervalType, DurationMinutes, OrderPosition, StartedAt, EndedAt |
| `ScheduleContent` | `ScheduleContents` | ScheduleId, ContentType, ContentId, EstimatedMinutes, Completed |

### 4.6 Otros

| Entidad | Tabla | Propiedades clave |
|---|---|---|
| `Notification` | `Notifications` | UserId, Type, Title, Message, RelatedEntityType, IsRead, ScheduledFor, SentAt |
| `SystemConfiguration` | `SystemConfigurations` | ConfigKey (único), ConfigValue, UpdatedBy |
| `ActivityLog` | `ActivityLogs` | UserId, Action, EntityType, EntityId, Details, IpAddress, UserAgent |

---

## 5. Controladores (20) - Endpoints y Lógica

### 5.1 `AuthController` (`/api/auth`)
| Método | Endpoint | Lógica |
|---|---|---|
| POST | `/register` | Valida email único, crea User con BCrypt, registra ActivityLog, devuelve JWT |
| POST | `/login` | Busca por email, verifica BCrypt, valida soft-delete, registra log, devuelve JWT |
| POST | `/logout` | [Authorize] - Registra ActivityLog de cierre |
| POST | `/refresh` | Stub (no implementado) |
| POST | `/verify-email` | Stub (no implementado) |
| POST | `/resend-verification` | Retorna respuesta simulada |
| POST | `/forgot-password` | Stub (no implementado) |
| POST | `/reset-password` | Stub (no implementado) |
| POST | `/change-password` | Verifica currentPassword con BCrypt, confirma match, hashea nueva |

### 5.2 `UserController` (`/api/user`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/profile` | Retorna perfil del usuario autenticado |
| PUT | `/profile` | Actualiza nombre, email, etc. |
| POST | `/avatar` | Sube archivo de avatar |
| DELETE | `/avatar` | Elimina avatar |
| GET | `/preferences` | Retorna preferencias (idioma, tema, timezone) |
| PUT | `/preferences` | Actualiza preferencias |
| POST | `/preferences/reset` | Restablece valores por defecto |
| GET | `/notification-settings` | Retorna config de notificaciones |
| PUT | `/notification-settings` | Actualiza notificaciones |
| POST | `/notification-settings/test` | Envía notificación de prueba |
| DELETE | `/account` | Soft-delete (DeletedAt) con verificación de contraseña |
| POST | `/export` | Exporta datos del usuario |
| POST | `/deactivate` | Desactiva cuenta |

### 5.3 `CourseController` (`/api/courses`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/` | Lista cursos del usuario |
| GET | `/{id}` | Obtiene curso por ID |
| POST | `/` | Crea curso con userId, color hex, etc. |
| PUT | `/{id}` | Actualiza curso |
| DELETE | `/{id}` | Elimina curso (retorna 204 si ok) |
| PATCH | `/{id}/archive` | Archiva curso |
| PATCH | `/{id}/restore` | Restaura curso archivado |
| GET | `/{id}/exam` | Obtiene fecha de examen |
| PUT | `/{id}/exam` | Establece fecha de examen |
| DELETE | `/{id}/exam` | Elimina fecha de examen |
| GET | `/{id}/members` | Lista miembros del curso |
| POST | `/{id}/members` | Agrega miembro (owner invite) |
| DELETE | `/{id}/members/{userId}` | Remueve miembro |
| PUT | `/{id}/members/{userId}/role` | Cambia rol de miembro |
| GET | `/{id}/stats` | Estadísticas del curso |
| GET | `/search` | Búsqueda con paginación, filtros |

### 5.4 `DecksController` (`/api/decks`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/` | Lista decks por courseId |
| GET | `/{id}` | Obtiene deck por ID |
| POST | `/` | Crea deck asociado a un curso |
| PUT | `/{id}` | Actualiza deck |
| DELETE | `/{id}` | Elimina deck |
| POST | `/{id}/duplicate` | Duplica deck con nuevo nombre |
| GET | `/{id}/cards` | Obtiene todas las flashcards del deck |
| POST | `/{id}/cards` | Agrega flashcards en bulk al deck |
| DELETE | `/{id}/cards` | Remueve flashcards específicas del deck |
| PUT | `/{id}/cards/reorder` | Reordena flashcards del deck |

### 5.5 `FlashcardsController` (`/api/flashcards`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/` | Lista flashcards por deckId |
| GET | `/{id}` | Obtiene flashcard por ID |
| POST | `/` | Crea flashcard individual |
| PUT | `/{id}` | Actualiza flashcard |
| DELETE | `/{id}` | Elimina flashcard |
| POST | `/bulk` | Crea múltiples flashcards |
| PUT | `/bulk` | Actualiza múltiples flashcards |
| GET | `/search` | Busca flashcards por query, tags, dificultad |
| POST | `/import/csv` | Importa desde CSV |
| POST | `/import/json` | Importa desde JSON |

### 5.6 `StudyController` (`/api/study`) - [Authorize] - **Repetición Espaciada**
| Método | Endpoint | Lógica |
|---|---|---|
| POST | `/sessions` | Inicia sesión de estudio (crea FlashcardStudySession) |
| GET | `/sessions/{id}/next` | Obtiene siguiente flashcard no revisada de la sesión |
| POST | `/sessions/{id}/answer` | Envía respuesta (KnewIt? → actualiza SM-2) |
| POST | `/sessions/{id}/end` | Finaliza sesión, calcula performance |
| GET | `/decks/{deckId}/due` | Obtiene flashcards pendientes de repaso (nextReviewDate ≤ now) |
| GET | `/decks/{deckId}/overdue` | Obtiene flashcards vencidas (nextReviewDate < now) |
| POST | `/reviews/schedule` | Programa revisión forzada de una flashcard |
| GET | `/sessions` | Historial de sesiones (por deck opcional) |
| GET | `/sessions/{id}` | Obtiene sesión específica |
| GET | `/sessions/{id}/summary` | Resumen de sesión (duración, avg response time) |
| GET | `/decks/{deckId}/performance` | Rendimiento general del deck |

### 5.7 `QuizzesController` (`/api/quizzes`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/` | Lista quizzes (por courseId opcional) |
| GET | `/{id}` | Obtiene quiz completo |
| POST | `/` | Crea quiz con preguntas y opciones |
| PUT | `/{id}` | Actualiza quiz |
| DELETE | `/{id}` | Elimina quiz |
| POST | `/{id}/duplicate` | Duplica quiz |
| GET | `/{id}/questions` | Obtiene preguntas del quiz |
| POST | `/{id}/questions` | Crea pregunta |
| PUT | `/{id}/questions/{questionId}` | Actualiza pregunta |
| DELETE | `/{id}/questions/{questionId}` | Elimina pregunta |
| PUT | `/{id}/questions/reorder` | Reordena preguntas |
| POST | `/{id}/questions/{qId}/options` | Crea opción de respuesta |
| PUT | `/{id}/questions/{qId}/options/{oId}` | Actualiza opción |
| DELETE | `/{id}/questions/{qId}/options/{oId}` | Elimina opción |
| GET | `/{id}/settings` | Obtiene configuración del quiz |
| PUT | `/{id}/settings` | Actualiza configuración |
| POST | `/{id}/settings/reset` | Restablece configuración |
| GET | `/{id}/preview` | Vista previa del quiz |
| POST | `/{id}/simulate` | Simula el quiz |

### 5.8 `QuizAttemptsController` (`/api/quiz-attempts`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| POST | `/start` | Inicia intento, valida límite de intentos |
| POST | `/{id}/submit` | Envía respuestas, auto-califica, actualiza UserProgressQuiz |
| GET | `/{id}/result` | Obtiene resultado con respuestas correctas/incorrectas |
| GET | `/` | Lista intentos del usuario (por quizId opcional) |
| POST | `/answer` | Guarda respuesta individual |
| PUT | `/answer` | Actualiza respuesta individual |
| POST | `/answers/bulk` | Guarda múltiples respuestas |
| POST | `/{id}/grade` | Auto-calificación (compara opciones seleccionadas con correctas) |
| POST | `/{id}/regrade` | Re-calificación, actualiza progreso |
| GET | `/history` | Historial de intentos de un quiz |
| GET | `/best` | Mejor intento de un quiz |
| GET | `/compare` | Compara dos intentos (score diff, time diff) |

### 5.9 `FlashcardProgressController` (`/api/progress/flashcards`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/decks/{deckId}` | Progreso de flashcards en un deck |
| GET | `/courses/{courseId}` | Progreso en un curso |
| GET | `/` | Progreso global |
| GET | `/decks/{deckId}/mastery` | Nivel de maestría |
| GET | `/decks/{deckId}/mastery/trend` | Tendencia de maestría |
| GET | `/decks/{deckId}/predictions` | Predicciones de progreso |
| GET | `/decks/{deckId}/timeline` | Línea de tiempo de progreso |
| GET | `/weekly` | Progreso semanal |
| GET | `/monthly` | Reporte mensual |

### 5.10 `QuizProgressController` (`/api/progress/quizzes`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/{quizId}` | Progreso de un quiz |
| GET | `/courses/{courseId}` | Progreso en un curso |
| GET | `/` | Progreso global de quizzes |
| GET | `/average` | Puntaje promedio |
| GET | `/courses/{courseId}/weak-topics` | Temas débiles |
| GET | `/improvement` | Mejora a través del tiempo |
| GET | `/compare` | Compara dos quizzes |
| GET | `/compare-courses` | Compara dos cursos |
| GET | `/compare-timeframes` | Compara dos períodos de tiempo |

### 5.11 `CourseExamProgressController` (`/api/progress/exam`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/courses/{courseId}` | Progreso de examen (readiness, días restantes, onTrack) |
| GET | `/courses/{courseId}/readiness` | Readiness score con factores (flashcards, quizzes, consistencia) |
| GET | `/courses/{courseId}/recommendations` | Recomendaciones basadas en progreso |
| GET | `/courses/{courseId}/readiness/history` | Historial de readiness |
| GET | `/courses/{courseId}/readiness/trend` | Tendencia de readiness |
| GET | `/courses/{courseId}/predictions` | Predicción de score futuro |
| GET | `/courses/{courseId}/weaknesses` | Identifica debilidades |
| GET | `/courses/{courseId}/weaknesses/priority` | Temas prioritarios |
| GET | `/courses/{courseId}/suggest-focus` | Sugiere enfoque de estudio |

### 5.12 `DashboardController` (`/api/dashboard`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/overview` | Resumen (tiempo estudio hoy/semana/mes, streak, cards, quizzes, exámenes, pendientes) |
| GET | `/activity` | Actividad reciente (desde ActivityLogs) |
| GET | `/deadlines` | Próximas fechas de examen (con urgencia) |
| GET | `/metrics/study-time` | Métrica de tiempo de estudio |
| GET | `/metrics/cards-reviewed` | Tarjetas revisadas en período |
| GET | `/metrics/quizzes-completed` | Quizzes completados |
| GET | `/charts/progress` | Datos para gráfico de progreso (7 días) |
| GET | `/charts/heatmap` | Datos para heatmap anual |
| GET | `/charts/distribution` | Distribución mastered/learning/no iniciado |
| GET | `/export/pdf` | Exporta dashboard a PDF |
| GET | `/export/csv` | Exporta dashboard a CSV |
| POST | `/export/report` | Genera reporte en JSON |

### 5.13 `PerformanceController` (`/api/performance`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/global` | Estadísticas globales |
| GET | `/ranking` | Ranking de usuario |
| GET | `/achievements` | Logros obtenidos |
| GET | `/trends/weekly` | Tendencia semanal |
| GET | `/trends/monthly` | Tendencia mensual |
| GET | `/trends/yearly` | Reporte anual |
| POST | `/goals` | Establece metas |
| GET | `/goals` | Obtiene metas |
| PUT | `/goals/progress` | Actualiza progreso de metas |
| GET | `/goals/check` | Verifica logros |

### 5.14 `SchedulesController` (`/api/schedules`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/` | Lista cronogramas del usuario |
| GET | `/range` | Cronogramas en rango de fechas |
| GET | `/{id}` | Obtiene cronograma |
| POST | `/` | Crea cronograma con intervalos y contenido |
| PUT | `/{id}` | Actualiza cronograma |
| DELETE | `/{id}` | Elimina cronograma |
| GET | `/calendar/month` | Vista mensual |
| GET | `/calendar/week` | Vista semanal |
| GET | `/calendar/day` | Vista diaria |
| GET | `/calendar/agenda` | Agenda en rango |
| POST | `/recurring` | Crea cronogramas recurrentes |
| PUT | `/recurring/{id}` | Actualiza recurrencia |
| DELETE | `/recurring/{id}` | Elimina recurrencia |
| PATCH | `/{id}/complete` | Marca como completado |
| PATCH | `/{id}/incomplete` | Marca como no completado |
| POST | `/bulk-complete` | Completa múltiples |

### 5.15 `ScheduleContentsController` (`/api/schedules/{id}/contents`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| POST | `/` | Adjunta contenido (flashcard deck o quiz) a un cronograma |
| DELETE | `/` | Desadjunta contenido |
| PUT | `/reorder` | Reordena contenido |
| GET | `/` | Contenido asignado |
| POST | `/auto-assign` | Asignación automática basada en progreso |
| POST | `/prioritize-exam` | Prioriza contenido según examen próximo |
| POST | `/prioritize-weakness` | Prioriza según debilidades |
| GET | `/suggest-session` | Sugiere sesión de estudio |
| GET | `/suggest-content` | Sugiere contenido para completar |
| GET | `/optimize` | Optimiza el cronograma completo |

### 5.16 `NotificationsController` (`/api/notifications`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/` | Lista notificaciones (filtro unreadOnly) |
| GET | `/{id}` | Obtiene notificación |
| PATCH | `/{id}/read` | Marca como leída |
| PATCH | `/read-all` | Marca todas como leídas |
| DELETE | `/{id}` | Elimina notificación |
| POST | `/reminders` | Crea recordatorio |
| GET | `/reminders/pending` | Recordatorios pendientes |
| DELETE | `/reminders/{id}` | Cancela recordatorio |
| POST | `/email/test` | Envía email de prueba |
| GET | `/email/logs` | Logs de emails |
| POST | `/email/retry/{id}` | Reintenta email fallido |
| POST | `/push/register` | Registra dispositivo para push |
| POST | `/push/unregister` | Desregistra dispositivo |
| POST | `/push/send` | Envía push notification |

### 5.17 `AiGenerationController` (`/api/ai`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| POST | `/generate/flashcards` | Solicita generación de flashcards desde archivo (registra GeneratedContent con status "pending") |
| POST | `/generate/quiz` | Solicita generación de quiz desde archivo |
| GET | `/generate/{id}/status` | Estado de la generación (pending/processing/completed) |
| PUT | `/config` | Configura proveedor AI (Gemini key, modelo, etc.) |
| GET | `/config` | Obtiene configuración actual |
| POST | `/config/test` | Prueba conexión con el proveedor AI |
| POST | `/regenerate` | Solicita regeneración de contenido |
| POST | `/improve` | Mejora preguntas con feedback |
| POST | `/adjust-difficulty` | Ajusta dificultad del contenido generado |
| GET | `/history` | Historial de generaciones |
| GET | `/history/{id}` | Contenido generado específico |
| DELETE | `/history/{id}` | Elimina historial |

### 5.18 `FilesController` (`/api/files`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| POST | `/upload` | Sube archivo (PDF, imagen, etc.) a `wwwroot/assets/` |
| GET | `/{id}/status` | Estado de subida |
| GET | `/history` | Historial de archivos subidos |
| DELETE | `/{id}` | Elimina archivo |
| POST | `/{id}/process` | Procesa archivo (extrae contenido para AI) |
| GET | `/{id}/processing-status` | Estado del procesamiento |
| POST | `/{id}/reprocess` | Reprocesa archivo |
| GET | `/{id}/download` | Descarga archivo |
| GET | `/{id}/stream` | Stream del archivo |

### 5.19 `ReportsController` (`/api/reports`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| POST | `/study` | Genera reporte de estudio en rango |
| GET | `/study/insights` | Insights de estudio |
| POST | `/performance` | Reporte de rendimiento |
| GET | `/performance/summary` | Resumen de rendimiento |
| POST | `/custom` | Reporte personalizado |
| POST | `/templates` | Guarda plantilla de reporte |
| GET | `/templates` | Lista plantillas |
| POST | `/schedule` | Programa reporte |

### 5.20 `SystemController` (`/api/system`) - [Authorize]
| Método | Endpoint | Lógica |
|---|---|---|
| GET | `/config` | Lista configuraciones del sistema |
| GET | `/config/{key}` | Configuración específica |
| PUT | `/config` | Actualiza configuración |
| POST | `/config/{key}/reset` | Restablece configuración |
| GET | `/health` | Health check |
| GET | `/status` | Estado del sistema |
| GET | `/metrics` | Métricas del sistema |
| POST | `/cache/clear` | Limpia caché |
| GET | `/cache/stats` | Estadísticas de caché |
| POST | `/cache/warmup` | Precarga caché |
| GET | `/logs` | Logs del sistema |
| GET | `/logs/search` | Busca en logs |
| GET | `/logs/export` | Exporta logs a JSON |

---

## 6. Lógica de Negocio Clave

### 6.1 Autenticación JWT
- **Registro**: BCrypt hash de contraseña, crea User, genera JWT con claims (NameIdentifier, Email, Name)
- **Login**: Busca por email, verifica BCrypt, valida `DeletedAt == null`, genera JWT
- **JWT Claims**: `Id`, `Email`, `FullName` - expira en `Jwt:ExpireMinutes` (60 por defecto)
- **Refresh Token**: Implementación stub (no funcional)

### 6.2 Algoritmo SM-2 (Repetición Espaciada) - `FlashcardStudyService`
- **Parámetros**: EaseFactor (2.5), IntervalDays, NextReviewDate
- **Respuesta Correcta (KnewIt=true)**:
  - `interval`: 0→1, 1→6, ≥2→`interval * easeFactor`
  - `easeFactor += 0.1` (máx 3.0)
- **Respuesta Incorrecta (KnewIt=false)**:
  - `interval = 1` (reinicia)
  - `easeFactor -= 0.2` (mín 1.3)
- **NextReviewDate**: `DateTime.UtcNow.Date.AddDays(interval)`
- **Due Cards**: FlashcardReview con `NextReviewDate <= now`
- **Overdue Cards**: `NextReviewDate < now`
- **Average Ease Factor**: Promedio del último review de cada flashcard en el deck

### 6.3 Auto-Calificación de Quizzes - `QuizAttemptService`
1. Por cada respuesta, se compara `SelectedOptionId` con `QuizOption.IsCorrect`
2. Se suman los `Points` de las respuestas correctas
3. `ScorePercentage = (totalPoints / maxPossiblePoints) * 100`
4. `Passed = score >= quiz.PassingScore` (default 70%)
5. Se actualiza `UserProgressQuiz`: TotalAttempts, BestScore, AverageScore, PassedCount

### 6.4 Generación con IA - `AiGenerationService`
- **Flujo**: Usuario sube archivo → solicita generación → se crea `GeneratedContent` con status "pending"
- **Configuración**: Provider Gemini, modelo configurable, maxTokens, temperature
- **Historial**: Se almacena en `GeneratedContent` con referencia al `FileUpload` original
- **Nota**: La generación real con Gemini API está en estado stub (registra pero no llama a la API)

### 6.5 Dashboard y Métricas - `DashboardService`
- **Streak**: Cuenta días consecutivos con actividad desde hoy hacia atrás
- **Tiempo de estudio**: Estimado (30 min por ActivityLog)
- **Próximos exámenes**: Cursos con `ExamDate > now` y ≤ 30 días
- **Urgencia**: High (≤7 días), Medium (≤30), Low (>30)
- **Heatmap**: Actividad por día con intensidad 0-4

### 6.6 Course Progress / Exam Readiness - `CourseProgressService`
- **Readiness Score**: Compuesto por:
  - `FlashcardsMastery`: (Mastered / Total) * 100
  - `QuizzesPerformance`: (Passed / Total) * 100
  - `StudyConsistency`: 50 (fijo)
  - `TimeUntilExam`: 50 (fijo)
- **OnTrack**: ReadinessScore ≥ 70
- **Predicciones**: Score proyectado = `avgScore + (100 - avgScore) / diasRestantes * 7`

### 6.7 Manejo de Errores
- `KeyNotFoundException` → recurso no encontrado
- `InvalidOperationException` → validación de negocio
- Retornos HTTP: 200, 201 (Created), 204 (NoContent), 400 (BadRequest), 401 (Unauthorized), 404 (NotFound)

### 6.8 Auditoría
- `ActivityLogRepository.LogAsync()` registra todas las acciones: Register, Login, Logout, ChangePassword, StartStudySession, EndStudySession, StartQuizAttempt, SubmitQuizAttempt, GenerateFlashcards, GenerateQuiz, etc.
- Cada log contiene: UserId, Action, EntityType, EntityId, Details, CreatedAt

---

## 7. Base de Datos - Relaciones Clave

### 7.1 Índices Únicos
| Tabla | Columnas |
|---|---|
| `Users` | Email (único) |
| `UserCourses` | UserId + CourseId (único) |
| `UserCourseExamProgresses` | UserId + CourseId (único) |
| `UserProgressFlashcards` | UserId + DeckId (único) |
| `UserProgressQuizzes` | UserId + QuizId (único) |
| `SpacedRepetitionSettings` | UserId + DeckId (único con filtro NOT NULL) |
| `SystemConfigurations` | ConfigKey (único) |

### 7.2 Reglas de Borrado
- **Cascade**: Course→FlashcardDecks, Course→Quizzes, FlashcardDeck→Flashcards, Quiz→Questions, Question→Options, etc.
- **NoAction**: UserCourse→Course, FlashcardReview→User, FlashcardReview→Flashcard, QuizAttempt→User, etc.

---

## 8. Autenticación y Seguridad
- **JWT Bearer**: Header `Authorization: Bearer <token>`
- **Claims**: NameIdentifier (userId), Email, Name (fullName)
- **Key**: Simétrica configurada en `appsettings.json`
- **Expiración**: 60 minutos (configurable)
- **Protección**: Endpoints marcados con `[Authorize]` requieren token válido
- **Soft Delete**: User tiene `DeletedAt` para eliminación lógica

---

## 9. Flujo de Datos Típico

### Ejemplo: Estudio con Flashcards
1. **POST `/api/study/sessions`** → Crea `FlashcardStudySession`
2. **GET `/api/study/sessions/{id}/next`** → Obtiene siguiente flashcard no revisada
3. **POST `/api/study/sessions/{id}/answer`** → Crea `FlashcardReview`, ejecuta SM-2, actualiza sesión
4. **POST `/api/study/sessions/{id}/end`** → Finaliza sesión, calcula performance, actualiza `UserProgressFlashcard`

### Ejemplo: Tomar un Quiz
1. **POST `/api/quiz-attempts/start`** → Crea `QuizAttempt`, valida límite de intentos
2. **POST `/api/quiz-attempts/answer`** (opcional, múltiples) → Guarda respuestas
3. **POST `/api/quiz-attempts/{id}/submit`** → Envía respuestas, auto-califica, actualiza `UserProgressQuiz`
4. **GET `/api/quiz-attempts/{id}/result`** → Retorna resultados con respuestas correctas/incorrectas

### Ejemplo: Generar Contenido con IA
1. **POST `/api/files/upload`** → Sube PDF a `wwwroot/assets/`
2. **POST `/api/ai/generate/flashcards`** (con `fileId`) → Crea `GeneratedContent` status "pending"
3. **GET `/api/ai/generate/{id}/status`** → Verifica estado (stub, siempre retorna "processing" hasta completion manual)

---

## 10. Configuración y Despliegue

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=PAVILION;Database=Backend_PlanoriaCapstone;..."
  },
  "Jwt": {
    "Key": "...", "Issuer": "PlanoriaAPI", "Audience": "PlanoriaClient", "ExpireMinutes": 60
  },
  "Gemini": {
    "ApiKey": "..."
  }
}
```

### Docker Compose
- **sqlserver**: `mcr.microsoft.com/mssql/server:2022-latest`, puerto 1433, SA password `Planoria123*`
- **backend**: Construye desde `.`, mapea `7075:8080`, depende de sqlserver
- **Red**: `planoria-network` (bridge)

### Dockerfile (multi-stage)
- **Build**: SDK 8.0, restaura y publica
- **Runtime**: ASP.NET 8.0, expone puerto 8080, entry `dotnet Backend_PlanoriaCapstone.dll`

---

## 11. Dependencias NuGet Principales
- `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.0)
- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.0)
- `BCrypt.Net-Next` (4.2.0)
- `Swashbuckle.AspNetCore` (6.6.2)
- `itext7` (9.6.0) - solo en BLL
- `MSTest` (3.6.2) - solo en Tests

---

## 12. Resumen de Funcionalidades por Módulo

| Módulo | Funcionalidad |
|---|---|
| **Auth** | Registro, Login, Logout, Refresh Token, Verificación Email, Reset/Change Password |
| **Usuarios** | Perfil, Avatar, Preferencias, Notificaciones, Exportar datos, Desactivar/Eliminar cuenta |
| **Cursos** | CRUD, Miembros con roles, Fechas de examen, Archivar/Restaurar, Estadísticas, Búsqueda |
| **Flashcards** | CRUD individual/bulk, Import CSV/JSON, Búsqueda, Reordenar |
| **Decks** | CRUD, Duplicar, Agregar/Remover/Reordenar cartas |
| **Estudio** | Sesiones, SM-2 Spaced Repetition, Due/Overdue cards, Performance tracking |
| **Quizzes** | CRUD, Preguntas con opciones, Settings, Vista previa, Simulación |
| **Quiz Attempts** | Iniciar/Enviar intentos, Auto-grade, Comparar, Historial, Mejor intento |
| **Flashcard Progress** | Maestría por deck/curso/global, Tendencias, Predicciones, Timeline, Reportes |
| **Quiz Progress** | Scores, Temas débiles, Mejora, Comparativas |
| **Course Progress** | Exam Readiness Score, Recomendaciones, Predicciones, Debilidades |
| **Dashboard** | Overview, Actividad, Deadlines, Métricas, Charts, Heatmap, Export PDF/CSV |
| **Performance** | Stats globales, Ranking, Logros, Tendencias, Metas |
| **Cronogramas** | CRUD, Vistas calendar (día/semana/mes/agenda), Recurrencias, Intervalos |
| **Schedule Content** | Asignar contenido, Auto-assign, Priorizar por examen/debilidad, Optimizar |
| **Notificaciones** | In-app, Email, Push, Recordatorios, Logs de email |
| **Archivos** | Upload, Download, Stream, Procesamiento para AI |
| **IA** | Generación flashcards/quizzes, Configuración, Historial, Mejora, Dificultad |
| **Reportes** | Estudio, Rendimiento, Personalizados, Plantillas |
| **Sistema** | Configuración, Health check, Estado, Métricas, Caché, Logs |
