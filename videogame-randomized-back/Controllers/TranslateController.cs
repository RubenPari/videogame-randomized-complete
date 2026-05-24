using Microsoft.AspNetCore.Mvc;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Services;

namespace videogame_randomized_back.Controllers;

[ApiController]
[Route("api/translate")]
public class TranslateController(ITranslateService translate) : ControllerBase
{
    private const string MissingKeyType = "urn:videogame-randomizer:google-translate:missing-api-key";

    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslateRequestDto req)
    {
        if (!translate.IsConfigured)
        {
            return Problem(
                title: "Google Translate API key is not configured",
                detail: "Set GOOGLE_TRANSLATE_API_KEY in the backend environment (.env / docker-compose) to enable translation.",
                statusCode: StatusCodes.Status503ServiceUnavailable,
                type: MissingKeyType);
        }

        if (string.IsNullOrWhiteSpace(req.Text))
        {
            return Problem(
                title: "Validation Error",
                detail: "Text is required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var source = string.IsNullOrWhiteSpace(req.Source) ? "en" : req.Source;
        var target = string.IsNullOrWhiteSpace(req.Target) ? "it" : req.Target;

        var (status, body) = await translate.TranslateAsync(req.Text, source, target);

        return new ContentResult
        {
            StatusCode = status,
            ContentType = "application/json",
            Content = body
        };
    }
}
