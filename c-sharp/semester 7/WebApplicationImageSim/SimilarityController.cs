using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApplicationImageSim
{
    public class CalculateRequest
    {
        public string Image1Name { get; set; } = "image1";
        public string Image1Base64 { get; set; } = string.Empty;
        public string Image2Name { get; set; } = "image2";
        public string Image2Base64 { get; set; } = string.Empty;
    }

    public class CalculateResponse
    {
        public float Similarity { get; set; }
        public float Distance { get; set; }
    }

    [ApiController]
    public class SimilarityController : ControllerBase
    {
        private readonly SimilarityService _similarityService;

        public SimilarityController(SimilarityService similarityService)
        {
            _similarityService = similarityService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Calculate([FromBody] CalculateRequest request)
        {
            try
            {
                var bytes1 = Convert.FromBase64String(request.Image1Base64);
                var bytes2 = Convert.FromBase64String(request.Image2Base64);

                var (sim, dist) = await _similarityService.GetOrComputeFromBytesAsync(
                    request.Image1Name ?? "image1",
                    request.Image2Name ?? "image2",
                    bytes1,
                    bytes2);

                var response = new CalculateResponse
                {
                    Similarity = sim,
                    Distance = dist
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpDelete("results")]
        public async Task<IActionResult> ClearResults()
        {
            try
            {
                await _similarityService.ClearDatabaseAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("results")]
        public async Task<IActionResult> GetResults()
        {
            try
            {
                var results = await _similarityService.GetResultsAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

    }
}
