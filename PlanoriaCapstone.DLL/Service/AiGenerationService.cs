using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.IA.Requests;
using PlanoriaCapstone.DTOs.IA.Responses;
using PlanoriaCapstone.DTOs.Flashcards.Decks.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Requests;
using PlanoriaCapstone.DTOs.Quiz.Requests;
using PlanoriaCapstone.Models;
using UglyToad.PdfPig;

namespace PlanoriaCapstone.Bll.Service;

public class AiGenerationService : IAiGenerationService
{
    private readonly IFileUploadRepository _fileUploadRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IFlashcardDeckService _deckService;
    private readonly IFlashcardService _flashcardService;
    private readonly IQuizService _quizService;
    private readonly IConfiguration _configuration;

    private static readonly ConcurrentDictionary<int, (int FileUploadId, int CourseId)>
        _generatedIndex = new();

    private static AIConfigRequestDto? _currentConfig;
    private static bool _configLoaded;
    private static readonly object _configLock = new();

    public AiGenerationService(
        IFileUploadRepository fileUploadRepository,
        IActivityLogRepository activityLogRepository,
        IFlashcardDeckService deckService,
        IFlashcardService flashcardService,
        IQuizService quizService,
        IConfiguration configuration)
    {
        _fileUploadRepository = fileUploadRepository;
        _activityLogRepository = activityLogRepository;
        _deckService = deckService;
        _flashcardService = flashcardService;
        _quizService = quizService;
        _configuration = configuration;
    }

    // ============================================
    // GENERAR FLASHCARDS
    // ============================================

    public async Task<GenerationResponseDto> GenerateFlashcardsAsync(int userId, GenerateContentRequestDto request)
    {
        var file = await _fileUploadRepository.GetByIdAsync(request.FileId);
        if (file == null)
            throw new KeyNotFoundException($"Archivo con ID {request.FileId} no encontrado");

        var content = await _fileUploadRepository.CreateGeneratedContentAsync(new GeneratedContent
        {
            FileUploadId = request.FileId,
            CourseId = request.TargetCourseId,
            ContentType = "flashcard",
            GeneratedEntityId = 0,
            TopicSpecified = request.Topic,
            GenerationConfig = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.NumberOfItems,
                request.Difficulty,
                request.Language
            }),
            CreatedAt = DateTime.UtcNow
        });

        _generatedIndex[content.Id] = (content.FileUploadId, content.CourseId);

        await LogActivitySafeAsync(userId, "GenerateFlashcards", "GeneratedContent", content.Id,
            $"Generación de flashcards desde archivo ID {request.FileId}");

        await ProcessFlashcardGenerationAsync(content.Id, request.TargetCourseId,
            request.NumberOfItems, request.Difficulty ?? "medium",
            request.Language ?? "es", userId);

        var updatedContent = await FindGeneratedContentByIdAsync(content.Id);

        return new GenerationResponseDto
        {
            GenerationId = content.Id,
            FileId = request.FileId,
            ContentType = "flashcard",
            Status = updatedContent?.GeneratedEntityId > 0 ? "completed" : "failed",
            Progress = updatedContent?.GeneratedEntityId > 0 ? 100 : 0,
            EstimatedTime = 0,
            CreatedAt = content.CreatedAt
        };
    }

    // ============================================
    // GENERAR QUIZ
    // ============================================

    public async Task<GenerationResponseDto> GenerateQuizAsync(int userId, GenerateContentRequestDto request)
    {
        var file = await _fileUploadRepository.GetByIdAsync(request.FileId);
        if (file == null)
            throw new KeyNotFoundException($"Archivo con ID {request.FileId} no encontrado");

        var content = await _fileUploadRepository.CreateGeneratedContentAsync(new GeneratedContent
        {
            FileUploadId = request.FileId,
            CourseId = request.TargetCourseId,
            ContentType = "quiz",
            GeneratedEntityId = 0,
            TopicSpecified = request.Topic,
            GenerationConfig = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.NumberOfItems,
                request.Difficulty,
                request.Language
            }),
            CreatedAt = DateTime.UtcNow
        });

        _generatedIndex[content.Id] = (content.FileUploadId, content.CourseId);

        await LogActivitySafeAsync(userId, "GenerateQuiz", "GeneratedContent", content.Id,
            $"Generación de quiz desde archivo ID {request.FileId}");

        await ProcessQuizGenerationAsync(content.Id, request.TargetCourseId,
            request.NumberOfItems, request.Difficulty ?? "medium",
            request.Language ?? "es", userId);

        var updatedContent = await FindGeneratedContentByIdAsync(content.Id);

        return new GenerationResponseDto
        {
            GenerationId = content.Id,
            FileId = request.FileId,
            ContentType = "quiz",
            Status = updatedContent?.GeneratedEntityId > 0 ? "completed" : "failed",
            Progress = updatedContent?.GeneratedEntityId > 0 ? 100 : 0,
            EstimatedTime = 0,
            CreatedAt = content.CreatedAt
        };
    }

    // ============================================
    // ESTADO DE GENERACIÓN
    // ============================================

    public async Task<GenerationResponseDto> GetGenerationStatusAsync(int generationId)
    {
        var generated = await FindGeneratedContentByIdAsync(generationId);
        if (generated == null)
            throw new KeyNotFoundException($"Generación con ID {generationId} no encontrada");

        bool isCompleted = generated.GeneratedEntityId > 0;

        return new GenerationResponseDto
        {
            GenerationId = generated.Id,
            FileId = generated.FileUploadId,
            ContentType = generated.ContentType,
            Status = isCompleted ? "completed" : "processing",
            Progress = isCompleted ? 100 : 50,
            EstimatedTime = isCompleted ? 0 : 30,
            CreatedAt = generated.CreatedAt
        };
    }

    // ============================================
    // CONFIGURACIÓN IA
    // ============================================

    public Task SetConfigAsync(AIConfigRequestDto request)
    {
        lock (_configLock)
        {
            _currentConfig = request;
            _configLoaded = true;
        }
        return Task.CompletedTask;
    }

    public Task<AIConfigResponseDto> GetConfigAsync()
    {
        lock (_configLock)
        {
            if (!_configLoaded || _currentConfig == null)
            {
                return Task.FromResult(new AIConfigResponseDto
                {
                    Provider = GetProvider(),
                    Model = GetModel(),
                    MaxTokens = 2000,
                    Temperature = 0.7m,
                    IsActive = !string.IsNullOrEmpty(GetApiKey()),
                    LastUsedAt = null
                });
            }

            return Task.FromResult(new AIConfigResponseDto
            {
                Provider = _currentConfig.Provider,
                Model = _currentConfig.Model,
                MaxTokens = _currentConfig.MaxTokens,
                Temperature = _currentConfig.Temperature,
                IsActive = !string.IsNullOrEmpty(_currentConfig.ApiKey),
                LastUsedAt = DateTime.UtcNow
            });
        }
    }

    public async Task TestConnectionAsync()
    {
        string apiKey = GetApiKey();
        string provider = GetProvider();

        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("API Key no configurada. Usa PUT /api/ai/config");

        string testPrompt = "Responde solo: OK";

        string response = provider switch
        {
            "groq" => await CallGroqApiAsync(testPrompt, apiKey, GetModel()),
            "deepseek" => await CallDeepSeekApiAsync(testPrompt, apiKey),
            _ => await CallGeminiApiAsync(testPrompt, apiKey, GetModel())
        };

        await LogActivitySafeAsync(1, "TestAiConnection", "System", null,
            $"Conexión a {provider} exitosa");
    }

    // ============================================
    // REGENERACIÓN Y MEJORA
    // ============================================

    public async Task<GenerationResponseDto> RegenerateAsync(ImproveContentRequestDto request)
    {
        var generated = await FindGeneratedContentByIdAsync(request.GeneratedContentId);
        if (generated == null)
            throw new KeyNotFoundException($"Contenido generado con ID {request.GeneratedContentId} no encontrado");

        generated.GeneratedEntityId = 0;
        generated.GenerationConfig = System.Text.Json.JsonSerializer.Serialize(new
        {
            feedback = request.Feedback,
            adjustComplexity = request.AdjustComplexity,
            focusTopics = request.FocusTopics
        });

        await _fileUploadRepository.UpdateGeneratedContentAsync(generated);
        await LogActivitySafeAsync(1, "RegenerateContent", "GeneratedContent", generated.Id, "Regeneración solicitada");

        return new GenerationResponseDto
        {
            GenerationId = generated.Id,
            FileId = generated.FileUploadId,
            ContentType = generated.ContentType,
            Status = "pending",
            Progress = 0,
            EstimatedTime = 30,
            CreatedAt = generated.CreatedAt
        };
    }

    public async Task<GenerationResponseDto> ImproveQuestionsAsync(ImproveContentRequestDto request)
    {
        return await RegenerateAsync(request);
    }

    public async Task<GenerationResponseDto> AdjustDifficultyAsync(int generatedContentId, string newDifficulty)
    {
        var generated = await FindGeneratedContentByIdAsync(generatedContentId);
        if (generated == null)
            throw new KeyNotFoundException($"Contenido generado con ID {generatedContentId} no encontrado");

        generated.GenerationConfig = System.Text.Json.JsonSerializer.Serialize(new { difficulty = newDifficulty });
        await _fileUploadRepository.UpdateGeneratedContentAsync(generated);

        return new GenerationResponseDto
        {
            GenerationId = generated.Id,
            FileId = generated.FileUploadId,
            ContentType = generated.ContentType,
            Status = "pending",
            Progress = 0,
            EstimatedTime = 20,
            CreatedAt = generated.CreatedAt
        };
    }

    // ============================================
    // HISTORIAL
    // ============================================

    public async Task<IEnumerable<GeneratedContentResponseDto>> GetHistoryAsync(int userId, int? fileId)
    {
        var results = new List<GeneratedContentResponseDto>();

        if (fileId.HasValue)
        {
            var file = await _fileUploadRepository.GetByIdAsync(fileId.Value);
            if (file?.GeneratedContents != null)
            {
                foreach (var gc in file.GeneratedContents)
                {
                    _generatedIndex[gc.Id] = (gc.FileUploadId, gc.CourseId);
                    results.Add(MapToGeneratedResponse(gc));
                }
            }
        }
        else
        {
            var files = await _fileUploadRepository.GetByUserIdAsync(userId);
            foreach (var file in files)
            {
                var fileWithContents = await _fileUploadRepository.GetByIdAsync(file.Id);
                if (fileWithContents?.GeneratedContents != null)
                {
                    foreach (var gc in fileWithContents.GeneratedContents)
                    {
                        _generatedIndex[gc.Id] = (gc.FileUploadId, gc.CourseId);
                        results.Add(MapToGeneratedResponse(gc));
                    }
                }
            }
        }

        return results.OrderByDescending(r => r.CreatedAt);
    }

    public async Task<GeneratedContentResponseDto> GetGeneratedContentAsync(int id)
    {
        var generated = await FindGeneratedContentByIdAsync(id);
        if (generated == null)
            throw new KeyNotFoundException($"Contenido generado con ID {id} no encontrado");

        return MapToGeneratedResponse(generated);
    }

    public async Task<bool> DeleteHistoryAsync(int id)
    {
        _generatedIndex.TryRemove(id, out _);
        if (_generatedIndex.TryGetValue(id, out var entry))
        {
            var file = await _fileUploadRepository.GetByIdAsync(entry.FileUploadId);
            if (file?.GeneratedContents != null)
            {
                var toRemove = file.GeneratedContents.FirstOrDefault(g => g.Id == id);
                if (toRemove != null)
                {
                    file.GeneratedContents.Remove(toRemove);
                    await _fileUploadRepository.UpdateAsync(file);
                    return true;
                }
            }
        }
        return false;
    }

    // ============================================
    // PROCESAMIENTO REAL
    // ============================================

    private async Task ProcessFlashcardGenerationAsync(int generatedContentId, int courseId,
        int numberOfItems, string difficulty, string language, int userId)
    {
        var generated = await FindGeneratedContentByIdAsync(generatedContentId);
        if (generated == null) return;

        var file = await _fileUploadRepository.GetByIdAsync(generated.FileUploadId);
        if (file == null) return;

        string fileContent = await ExtractTextFromFileAsync(file);
        string prompt = BuildFlashcardPrompt(fileContent, numberOfItems, difficulty, language);
        string aiResponse = await CallAIApiAsync(prompt);
        var flashcards = ParseFlashcardsFromAIResponse(aiResponse);

        if (flashcards.Count == 0)
            throw new InvalidOperationException("La IA no generó ninguna flashcard");

        var deckRequest = new CreateDeckRequestDto
        {
            Name = $"Mazo - {file.OriginalFilename}",
            Description = $"Generado por IA desde {file.OriginalFilename}",
            CourseId = courseId,
            SpacedRepetitionEnabled = true
        };

        var deck = await _deckService.CreateAsync(userId, deckRequest);

        int position = 1;
        foreach (var card in flashcards)
        {
            await _flashcardService.CreateAsync(new CreateFlashcardRequestDto
            {
                Question = card.Question,
                Answer = card.Answer,
                Hint = card.Hint,
                Difficulty = card.Difficulty ?? difficulty,
                Tags = card.Tags ?? new List<string>(),
                DeckId = deck.Id,
                Position = position++
            });
        }

        generated.GeneratedEntityId = deck.Id;
        await _fileUploadRepository.UpdateGeneratedContentAsync(generated);

        await LogActivitySafeAsync(userId, "GenerateFlashcards_Completed", "GeneratedContent",
            generatedContentId, $"Deck ID: {deck.Id}, Cards: {flashcards.Count}");
    }

    private async Task ProcessQuizGenerationAsync(int generatedContentId, int courseId,
        int numberOfItems, string difficulty, string language, int userId)
    {
        var generated = await FindGeneratedContentByIdAsync(generatedContentId);
        if (generated == null) return;

        var file = await _fileUploadRepository.GetByIdAsync(generated.FileUploadId);
        if (file == null) return;

        string fileContent = await ExtractTextFromFileAsync(file);
        string prompt = BuildQuizPrompt(fileContent, numberOfItems, difficulty, language);
        string aiResponse = await CallAIApiAsync(prompt);
        var quizQuestions = ParseQuizFromAIResponse(aiResponse);

        if (quizQuestions.Count == 0)
            throw new InvalidOperationException("La IA no generó ninguna pregunta");

        var quizRequest = new CreateQuizRequestDto
        {
            Title = $"Quiz - {file.OriginalFilename}",
            Description = $"Quiz generado por IA desde {file.OriginalFilename}",
            CourseId = courseId,
            PassingScore = 70.00m,
            ShuffleQuestions = true,
            ShuffleOptions = true,
            AttemptsAllowed = 3
        };

        var quiz = await _quizService.CreateAsync(userId, quizRequest);

        int questionPosition = 1;
        foreach (var question in quizQuestions)
        {
            var options = new List<CreateOptionRequestDto>();
            if (question.Options != null)
            {
                int optionPosition = 1;
                foreach (var opt in question.Options)
                {
                    options.Add(new CreateOptionRequestDto
                    {
                        OptionText = opt.OptionText,
                        IsCorrect = opt.IsCorrect,
                        OrderPosition = optionPosition++
                    });
                }
            }

            await _quizService.CreateQuestionAsync(quiz.Id, new CreateQuestionRequestDto
            {
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType ?? "multiple_choice",
                Explanation = question.Explanation,
                Points = 1.00m,
                OrderPosition = questionPosition++,
                Options = options
            });
        }

        generated.GeneratedEntityId = quiz.Id;
        await _fileUploadRepository.UpdateGeneratedContentAsync(generated);

        await LogActivitySafeAsync(userId, "GenerateQuiz_Completed", "GeneratedContent",
            generatedContentId, $"Quiz ID: {quiz.Id}, Questions: {quizQuestions.Count}");
    }

    // ============================================
    // EXTRACCIÓN DE TEXTO DEL PDF
    // ============================================

    private async Task<string> ExtractTextFromFileAsync(FileUpload file)
    {
        try
        {
            if (!string.IsNullOrEmpty(file.FilePath) && File.Exists(file.FilePath))
            {
                string extension = Path.GetExtension(file.FilePath).ToLower();

                if (extension == ".pdf")
                {
                    using var pdf = PdfDocument.Open(file.FilePath);
                    var pages = pdf.GetPages().ToList();
                    var text = string.Join("\n", pages.Select(p => p.Text));

                    if (text.Length > 30000)
                        text = text.Substring(0, 30000) + "\n\n[Contenido truncado - se procesaron las primeras páginas]";

                    return string.IsNullOrWhiteSpace(text)
                        ? $"Archivo PDF: {file.OriginalFilename}. Genera contenido sobre este tema."
                        : text;
                }
                else if (extension == ".txt")
                {
                    var text = await File.ReadAllTextAsync(file.FilePath);
                    if (text.Length > 30000)
                        text = text.Substring(0, 30000) + "\n\n[Contenido truncado]";
                    return text;
                }
            }
        }
        catch (Exception ex)
        {
            await LogActivitySafeAsync(1, "ExtractTextError", "File", file.Id, $"Error: {ex.Message}");
        }

        return $"Archivo: {file.OriginalFilename}\nGenera contenido educativo basado en este archivo.";
    }

    // ============================================
    // PROMPTS
    // ============================================

    private string BuildFlashcardPrompt(string content, int numberOfItems, string difficulty, string language)
    {
        return $@"Eres un profesor experto. Crea {numberOfItems} flashcards educativas basadas EXCLUSIVAMENTE en el siguiente contenido.

CONTENIDO:
{content}

REQUISITOS:
- Dificultad: {difficulty} | Idioma: {language}
- Exactamente {numberOfItems} flashcards
- Preguntas claras y respuestas concisas basadas en el contenido
- Incluye pistas y etiquetas

IMPORTANTE: Devuelve SOLO un array JSON válido, sin texto adicional:
[
  {{
    ""question"": ""pregunta"",
    ""answer"": ""respuesta"",
    ""hint"": ""pista"",
    ""difficulty"": ""{difficulty}"",
    ""tags"": [""tema1"", ""tema2""]
  }}
]";
    }

    private string BuildQuizPrompt(string content, int numberOfItems, string difficulty, string language)
    {
        return $@"Eres un profesor experto. Crea {numberOfItems} preguntas de opción múltiple basadas EXCLUSIVAMENTE en el siguiente contenido.

CONTENIDO:
{content}

REQUISITOS:
- Dificultad: {difficulty} | Idioma: {language}
- 4 opciones por pregunta, solo una correcta
- Incluye explicación de la respuesta correcta

IMPORTANTE: Devuelve SOLO un array JSON válido, sin texto adicional:
[
  {{
    ""questionText"": ""pregunta"",
    ""questionType"": ""multiple_choice"",
    ""explanation"": ""explicación"",
    ""options"": [
      {{ ""optionText"": ""A"", ""isCorrect"": false }},
      {{ ""optionText"": ""B"", ""isCorrect"": true }},
      {{ ""optionText"": ""C"", ""isCorrect"": false }},
      {{ ""optionText"": ""D"", ""isCorrect"": false }}
    ]
  }}
]";
    }

    // ============================================
    // LLAMADA A IA (GROQ + GEMINI + DEEPSEEK)
    // ============================================

    private async Task<string> CallAIApiAsync(string prompt)
    {
        string apiKey = GetApiKey();
        string provider = GetProvider();

        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("API Key no configurada. Usa PUT /api/ai/config");

        return provider switch
        {
            "groq" => await CallGroqApiAsync(prompt, apiKey, GetModel()),
            "deepseek" => await CallDeepSeekApiAsync(prompt, apiKey),
            _ => await CallGeminiApiAsync(prompt, apiKey, GetModel())
        };
    }

    // ✅ NUEVO: Groq API
    private async Task<string> CallGroqApiAsync(string prompt, string apiKey, string model)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(60);

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 2000
        };

        var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var response = await httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Error Groq API ({response.StatusCode}): {responseJson}");

        using var doc = System.Text.Json.JsonDocument.Parse(responseJson);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? throw new InvalidOperationException("Groq devolvió respuesta vacía");
    }

    private async Task<string> CallGeminiApiAsync(string prompt, string apiKey, string model)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(60);

        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.7, maxOutputTokens = 2000 }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var response = await httpClient.PostAsync(url, content);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Error Gemini API ({response.StatusCode}): {responseJson}");

        using var doc = System.Text.Json.JsonDocument.Parse(responseJson);
        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "";
    }

    private async Task<string> CallDeepSeekApiAsync(string prompt, string apiKey)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(60);

        var requestBody = new
        {
            model = "deepseek-chat",
            messages = new[] { new { role = "user", content = prompt } },
            temperature = 0.7,
            max_tokens = 2000
        };

        var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var response = await httpClient.PostAsync("https://api.deepseek.com/v1/chat/completions", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Error DeepSeek API ({response.StatusCode}): {responseJson}");

        using var doc = System.Text.Json.JsonDocument.Parse(responseJson);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";
    }

    // ============================================
    // PARSEO
    // ============================================

    private List<FlashcardFromAI> ParseFlashcardsFromAIResponse(string aiResponse)
    {
        string cleanJson = ExtractJsonFromResponse(aiResponse);
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<List<FlashcardFromAI>>(cleanJson, options)
               ?? new List<FlashcardFromAI>();
    }

    private List<QuizQuestionFromAI> ParseQuizFromAIResponse(string aiResponse)
    {
        string cleanJson = ExtractJsonFromResponse(aiResponse);
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<List<QuizQuestionFromAI>>(cleanJson, options)
               ?? new List<QuizQuestionFromAI>();
    }

    private string ExtractJsonFromResponse(string response)
    {
        var startIndex = response.IndexOf('[');
        var endIndex = response.LastIndexOf(']') + 1;
        if (startIndex >= 0 && endIndex > startIndex)
            return response.Substring(startIndex, endIndex - startIndex);
        return response;
    }

    // ============================================
    // HELPERS
    // ============================================

    private string GetApiKey()
    {
        lock (_configLock)
        {
            if (_configLoaded && _currentConfig != null && !string.IsNullOrEmpty(_currentConfig.ApiKey))
                return _currentConfig.ApiKey;
        }
        return _configuration["AI:ApiKey"] ?? "";
    }

    private string GetProvider()
    {
        lock (_configLock)
        {
            if (_configLoaded && _currentConfig != null && !string.IsNullOrEmpty(_currentConfig.Provider))
                return _currentConfig.Provider.ToLower();
        }
        return _configuration["AI:Provider"] ?? "groq";
    }

    private string GetModel()
    {
        lock (_configLock)
        {
            if (_configLoaded && _currentConfig != null && !string.IsNullOrEmpty(_currentConfig.Model))
                return _currentConfig.Model;
        }
        return _configuration["AI:Model"] ?? "llama-3.1-70b-versatile";
    }

    private async Task LogActivitySafeAsync(int userId, string action, string entityType,
        int? entityId, string details)
    {
        try
        {
            await _activityLogRepository.LogAsync(new ActivityLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch { }
    }

    private async Task<GeneratedContent?> FindGeneratedContentByIdAsync(int id)
    {
        if (_generatedIndex.TryGetValue(id, out var entry))
        {
            var file = await _fileUploadRepository.GetByIdAsync(entry.FileUploadId);
            return file?.GeneratedContents?.FirstOrDefault(g => g.Id == id);
        }
        return null;
    }

    private static GeneratedContentResponseDto MapToGeneratedResponse(GeneratedContent gc)
    {
        return new GeneratedContentResponseDto
        {
            Id = gc.Id,
            FileId = gc.FileUploadId,
            FileOriginalName = gc.FileUpload?.OriginalFilename ?? "Desconocido",
            ContentType = gc.ContentType,
            GeneratedEntityId = gc.GeneratedEntityId,
            EntityName = gc.GeneratedEntityId > 0 ? $"Entidad #{gc.GeneratedEntityId}" : "Pendiente",
            TopicSpecified = gc.TopicSpecified,
            GenerationConfig = gc.GenerationConfig,
            CreatedAt = gc.CreatedAt
        };
    }
}

internal class FlashcardFromAI
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Hint { get; set; }
    public string? Difficulty { get; set; }
    public List<string>? Tags { get; set; }
}

internal class QuizQuestionFromAI
{
    public string QuestionText { get; set; } = string.Empty;
    public string? QuestionType { get; set; }
    public string? Explanation { get; set; }
    public List<OptionFromAI>? Options { get; set; }
}

internal class OptionFromAI
{
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}