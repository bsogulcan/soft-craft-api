using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SoftCraft.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DownloadProjectController : Controller
{
    public DownloadProjectController()
    {
    }

    [HttpGet]
    [Route("GetProjectZipFile")]
    public async Task<IActionResult> GetProjectZipFile(int projectId, string projectName)
    {
        var filePath = @$"C:\SoftCraft\DownloadableProjects\{projectId + "-" + projectName}.zip";

        byte[] byteArray =
            await System.IO.File.ReadAllBytesAsync(
                filePath);

        System.IO.File.Delete(filePath);

        return new FileContentResult(byteArray, "application/octet-stream")
        {
            FileDownloadName = "EFProject.zip"
        };
    }
}