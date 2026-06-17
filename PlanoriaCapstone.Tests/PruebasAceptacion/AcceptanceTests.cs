using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanoriaCapstone.Bll.Service;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Auth.Requests;
using PlanoriaCapstone.DTOs.Courses.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Decks.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Study.Requests;
using PlanoriaCapstone.DTOs.Quiz.Requests;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Tests.Acceptance
{
    [TestClass]
    public class AcceptanceTests
    {
        private AppDbContext _context = null!;
        private AuthService _authService = null!;
        private CourseService _courseService = null!;
        private FlashcardDeckService _deckService = null!;
        private FlashcardService _flashcardService = null!;
        private FlashcardStudyService _studyService = null!;
        private QuizService _quizService = null!;
        private QuizAttemptService _attemptService = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"AcceptanceDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);

            // Repositorios
            var userRepo = new UserRepository(_context);
            var courseRepo = new CourseRepository(_context);
            var examRepo = new UserCourseExamProgressRepository(_context);
            var deckRepo = new FlashcardDeckRepository(_context);
            var flashcardRepo = new FlashcardRepository(_context);
            var progressRepo = new UserProgressFlashcardRepository(_context);
            var quizRepo = new QuizRepository(_context);
            var attemptRepo = new QuizAttemptRepository(_context);
            var quizProgressRepo = new UserProgressQuizRepository(_context);
            var logRepo = new ActivityLogRepository(_context);

            // Configuración JWT
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "EstaEsUnaClaveSuperSecretaDe32Caracteres!" },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:ExpireMinutes", "60" }
                });
            var config = configBuilder.Build();

            // Servicios
            _authService = new AuthService(userRepo, logRepo, config);
            _courseService = new CourseService(courseRepo, examRepo, userRepo, logRepo);
            _deckService = new FlashcardDeckService(deckRepo, flashcardRepo, progressRepo, logRepo);
            _flashcardService = new FlashcardService(flashcardRepo, deckRepo, logRepo);
            _studyService = new FlashcardStudyService(_context, flashcardRepo, deckRepo, progressRepo, logRepo);
            _quizService = new QuizService(quizRepo, courseRepo, logRepo);
            _attemptService = new QuizAttemptService(attemptRepo, quizRepo, quizProgressRepo, logRepo);
        }

        #region Historia 1: Registro y Login

        [TestMethod]
        public async Task Historia1_UsuarioSeRegistraYLogin()
        {
            // Paso 1: Registro
            var registerRequest = new RegisterRequestDto
            {
                Nombre = "María",
                Apellido = "González",
                Email = "maria@test.com",
                Password = "Maria123!",
                PreferredLanguage = "es"
            };

            var authResponse = await _authService.RegisterAsync(registerRequest);

            Assert.IsNotNull(authResponse);
            Assert.IsNotNull(authResponse.AccessToken);
            Assert.AreEqual("maria@test.com", authResponse.User.Email);

            // Paso 2: Login
            var loginRequest = new LoginRequestDto
            {
                Email = "maria@test.com",
                Password = "Maria123!"
            };

            var loginResponse = await _authService.LoginAsync(loginRequest);

            Assert.IsNotNull(loginResponse);
            Assert.IsNotNull(loginResponse.AccessToken);
        }

        #endregion

        #region Historia 2: Crear Curso y Flashcards

        [TestMethod]
        public async Task Historia2_CrearCurso_Deck_Flashcards_Estudiar()
        {
            // Paso 1: Crear curso
            var course = await _courseService.CreateAsync(1, new CreateCourseRequestDto
            {
                Name = "Biología",
                Description = "Curso de biología celular",
                ColorHex = "#27ae60"
            });

            Assert.AreEqual("Biología", course.Name);

            // Paso 2: Crear deck
            var deck = await _deckService.CreateAsync(1, new CreateDeckRequestDto
            {
                Name = "Células",
                Description = "Mazo sobre tipos de células",
                CourseId = course.Id,
                SpacedRepetitionEnabled = true
            });

            Assert.AreEqual("Células", deck.Name);

            // Paso 3: Crear flashcards
            var card1 = await _flashcardService.CreateAsync(new CreateFlashcardRequestDto
            {
                Question = "¿Qué es una célula eucariota?",
                Answer = "Célula con núcleo definido y organelos membranosos",
                Difficulty = "medium",
                DeckId = deck.Id,
                Position = 1
            });

            var card2 = await _flashcardService.CreateAsync(new CreateFlashcardRequestDto
            {
                Question = "¿Qué es una célula procariota?",
                Answer = "Célula sin núcleo definido, como las bacterias",
                Difficulty = "easy",
                DeckId = deck.Id,
                Position = 2
            });

            Assert.AreEqual("¿Qué es una célula eucariota?", card1.Question);
            Assert.AreEqual(2, (await _flashcardService.GetByDeckIdAsync(deck.Id)).Count());

            // Paso 4: Iniciar sesión de estudio
            var session = await _studyService.StartSessionAsync(1, new StartStudySessionRequestDto
            {
                DeckId = deck.Id,
                SessionType = "normal"
            });

            Assert.IsNotNull(session);
            Assert.AreEqual(deck.Id, session.DeckId);

            // Paso 5: Obtener siguiente tarjeta
            var nextCard = await _studyService.GetNextCardAsync(session.Id);

            Assert.IsNotNull(nextCard.Flashcard);
            Assert.AreEqual(1, nextCard.Current);

            // Paso 6: Responder tarjeta
            await _studyService.SubmitAnswerAsync(1, new SubmitFlashcardAnswerRequestDto
            {
                SessionId = session.Id,
                FlashcardId = nextCard.Flashcard!.Id,
                KnewIt = true,
                ResponseTimeMs = 3000
            });

            // Paso 7: Finalizar sesión
            var endedSession = await _studyService.EndSessionAsync(1, new EndStudySessionRequestDto
            {
                SessionId = session.Id
            });

            Assert.AreEqual(1, endedSession.CardsReviewed);
            Assert.AreEqual(1, endedSession.CardsKnown);
            Assert.AreEqual(100, endedSession.PerformanceScore);
        }

        #endregion

        #region Historia 3: Crear Quiz y Realizar Intento

        [TestMethod]
        public async Task Historia3_CrearQuiz_RealizarIntento_Aprobar()
        {
            // Paso 1: Crear curso
            var course = await _courseService.CreateAsync(1, new CreateCourseRequestDto
            {
                Name = "Historia",
                ColorHex = "#e74c3c"
            });

            // Paso 2: Crear quiz
            var quiz = await _quizService.CreateAsync(1, new CreateQuizRequestDto
            {
                Title = "Revolución Francesa",
                Description = "Quiz sobre la revolución francesa",
                CourseId = course.Id,
                PassingScore = 60,
                ShuffleQuestions = false,
                ShuffleOptions = false,
                AttemptsAllowed = 3
            });

            Assert.AreEqual("Revolución Francesa", quiz.Title);

            // Paso 3: Crear preguntas con opciones
            var q1 = await _quizService.CreateQuestionAsync(quiz.Id, new CreateQuestionRequestDto
            {
                QuestionText = "¿En qué año inició la Revolución Francesa?",
                QuestionType = "multiple_choice",
                Explanation = "La Revolución Francesa inició en 1789",
                Points = 2,
                OrderPosition = 1,
                Options = new List<CreateOptionRequestDto>
                {
                    new() { OptionText = "1789", IsCorrect = true, OrderPosition = 1 },
                    new() { OptionText = "1776", IsCorrect = false, OrderPosition = 2 },
                    new() { OptionText = "1804", IsCorrect = false, OrderPosition = 3 },
                    new() { OptionText = "1810", IsCorrect = false, OrderPosition = 4 }
                }
            });

            var q2 = await _quizService.CreateQuestionAsync(quiz.Id, new CreateQuestionRequestDto
            {
                QuestionText = "¿Quién fue el rey durante la Revolución?",
                QuestionType = "multiple_choice",
                Explanation = "Luis XVI era el rey de Francia",
                Points = 2,
                OrderPosition = 2,
                Options = new List<CreateOptionRequestDto>
                {
                    new() { OptionText = "Luis XIV", IsCorrect = false, OrderPosition = 1 },
                    new() { OptionText = "Luis XVI", IsCorrect = true, OrderPosition = 2 },
                    new() { OptionText = "Napoleón", IsCorrect = false, OrderPosition = 3 },
                    new() { OptionText = "Carlos X", IsCorrect = false, OrderPosition = 4 }
                }
            });

            // Obtener IDs de opciones correctas
            var questions = await _quizService.GetQuestionsAsync(quiz.Id);
            var questionsList = questions.ToList();
            var correctOption1 = questionsList[0].Options.First(o => o.IsCorrect == true);
            var correctOption2 = questionsList[1].Options.First(o => o.IsCorrect == true);

            // Paso 4: Iniciar intento
            var attempt = await _attemptService.StartAsync(1, new StartQuizAttemptRequestDto
            {
                QuizId = quiz.Id
            });

            Assert.IsNotNull(attempt);

            // Paso 5: Enviar respuestas (todas correctas)
            var result = await _attemptService.SubmitAsync(1, new SubmitQuizRequestDto
            {
                AttemptId = attempt.Id,
                Answers = new List<SubmitAnswerRequestDto>
                {
                    new()
                    {
                        AttemptId = attempt.Id,
                        QuestionId = q1.Id,
                        SelectedOptionId = correctOption1.Id,
                        ShortAnswerText = ""
                    },
                    new()
                    {
                        AttemptId = attempt.Id,
                        QuestionId = q2.Id,
                        SelectedOptionId = correctOption2.Id,
                        ShortAnswerText = ""
                    }
                }
            });

            // Paso 6: Verificar resultado
            Assert.AreEqual(100, result.Attempt.ScorePercentage);
            Assert.IsTrue(result.Attempt.Passed == true);
            Assert.AreEqual(2, result.Attempt.CorrectAnswersCount);
        }

        #endregion

        #region Historia 4: Flujo Completo - Curso con Examen

        [TestMethod]
        public async Task Historia4_FlujoCompleto_ConExamen()
        {
            // 1. Crear curso con fecha de examen
            var course = await _courseService.CreateAsync(1, new CreateCourseRequestDto
            {
                Name = "Física",
                ColorHex = "#9b59b6"
            });

            await _courseService.SetExamDateAsync(course.Id, new SetExamDateRequestDto
            {
                ExamDate = DateTime.UtcNow.AddDays(30),
                ExamTime = "09:00",
                NotifyMe = true
            });

            // 2. Crear deck y flashcards
            var deck = await _deckService.CreateAsync(1, new CreateDeckRequestDto
            {
                Name = "Mecánica",
                CourseId = course.Id
            });

            await _flashcardService.BulkCreateAsync(new BulkCreateFlashcardsRequestDto
            {
                DeckId = deck.Id,
                Cards = new List<CreateFlashcardRequestDto>
                {
                    new() { Question = "¿Qué es la velocidad?", Answer = "Cambio de posición respecto al tiempo", DeckId = deck.Id, Position = 1 },
                    new() { Question = "¿Qué es la aceleración?", Answer = "Cambio de velocidad respecto al tiempo", DeckId = deck.Id, Position = 2 }
                }
            });

            // 3. Estudiar
            var session = await _studyService.StartSessionAsync(1, new StartStudySessionRequestDto
            {
                DeckId = deck.Id,
                SessionType = "normal"
            });

            var nextCard = await _studyService.GetNextCardAsync(session.Id);
            Assert.IsNotNull(nextCard.Flashcard);

            await _studyService.SubmitAnswerAsync(1, new SubmitFlashcardAnswerRequestDto
            {
                SessionId = session.Id,
                FlashcardId = nextCard.Flashcard!.Id,
                KnewIt = true,
                ResponseTimeMs = 2000
            });

            await _studyService.EndSessionAsync(1, new EndStudySessionRequestDto
            {
                SessionId = session.Id
            });

            // 4. Crear quiz y aprobarlo
            var quiz = await _quizService.CreateAsync(1, new CreateQuizRequestDto
            {
                Title = "Quiz de Física",
                CourseId = course.Id,
                PassingScore = 50
            });

            var q = await _quizService.CreateQuestionAsync(quiz.Id, new CreateQuestionRequestDto
            {
                QuestionText = "¿Qué mide la velocidad?",
                QuestionType = "multiple_choice",
                Points = 10,
                OrderPosition = 1,
                Options = new List<CreateOptionRequestDto>
                {
                    new() { OptionText = "Cambio de posición", IsCorrect = true, OrderPosition = 1 },
                    new() { OptionText = "Cambio de masa", IsCorrect = false, OrderPosition = 2 }
                }
            });

            var questions = await _quizService.GetQuestionsAsync(quiz.Id);
            var correctOpt = questions.First().Options.First(o => o.IsCorrect == true);

            var attempt = await _attemptService.StartAsync(1, new StartQuizAttemptRequestDto { QuizId = quiz.Id });
            var result = await _attemptService.SubmitAsync(1, new SubmitQuizRequestDto
            {
                AttemptId = attempt.Id,
                Answers = new List<SubmitAnswerRequestDto>
                {
                    new()
                    {
                        AttemptId = attempt.Id,
                        QuestionId = q.Id,
                        SelectedOptionId = correctOpt.Id,
                        ShortAnswerText = ""
                    }
                }
            });

            // 5. Verificaciones finales
            Assert.AreEqual(100, result.Attempt.ScorePercentage);
            Assert.IsTrue(result.Attempt.Passed == true);
            Assert.AreEqual("Física", course.Name);
            Assert.AreEqual(2, (await _flashcardService.GetByDeckIdAsync(deck.Id)).Count());
        }

        #endregion
    }
}