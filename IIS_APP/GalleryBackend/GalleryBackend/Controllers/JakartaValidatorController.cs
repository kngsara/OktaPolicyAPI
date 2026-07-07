using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace OktaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "FullAccess")]
    public class JakartaValidatorController : ControllerBase
    {
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateWithJakarta()
        {
            var javaProjectPath = @"C:\Users\sarap_g40jeyd\Desktop\GalleryXmlValidator";
            var mavenPath = @"C:\Program Files\Apache NetBeans\java\maven\bin\mvn.cmd";

            if (!Directory.Exists(javaProjectPath))
            {
                return NotFound(new
                {
                    message = "Java projekt nije pronađen.",
                    path = javaProjectPath
                });
            }

            if (!System.IO.File.Exists(mavenPath))
            {
                return NotFound(new
                {
                    message = "Maven nije pronađen.",
                    path = mavenPath
                });
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = mavenPath,
                Arguments = "-q exec:java -Dexec.mainClass=hr.algebra.XmlValidator",
                WorkingDirectory = javaProjectPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.Environment["JAVA_HOME"] = @"C:\Program Files\Apache NetBeans\jdk";

            using var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            var finished = await Task.Run(() => process.WaitForExit(30000));

            if (!finished)
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                }

                return StatusCode(500, new
                {
                    message = "Java validator se nije završio u očekivanom vremenu."
                });
            }

            var output = await outputTask;
            var error = await errorTask;

            return Ok(new
            {
                message = process.ExitCode == 0
                    ? "Java/Jakarta validator je izvršen."
                    : "Java/Jakarta validator je završio s greškom.",
                exitCode = process.ExitCode,
                output,
                error
            });
        }
    }
}