using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/add");
});

app.MapGet("/add", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(Path.Combine("wwwroot", "add.html"));
});

app.MapPost("/upload", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var files = form.Files.GetFiles("files");

    if (files == null || files.Count == 0)
    {
        context.Response.StatusCode = 400; 
        await context.Response.WriteAsync("Файлы не выбраны.");
        return;
    }

    string uploadPath = Path.Combine("wwwroot", "UploadedFiles");

    // Создание папки, если она не существует
    if (!Directory.Exists(uploadPath))
    {
        Directory.CreateDirectory(uploadPath);
    }

    foreach (var file in files)
    {
        if (file.Length > 0)
        {
            // Генерация безопасного имени файла
            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            // Сохранение файла
            using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
        }
    }

    context.Response.Redirect("/files");
});

app.MapGet("/files", async context =>
{
    string uploadPath = Path.Combine("wwwroot", "UploadedFiles");

    if (!Directory.Exists(uploadPath))
    {
        Directory.CreateDirectory(uploadPath);
    }

    var files = Directory.GetFiles(uploadPath);
    var fileNames = files.Select(f => Path.GetFileName(f)).ToList();

    // Генерация HTML-страницы со списком файлов
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("<!DOCTYPE html>");
    sb.AppendLine("<html lang=\"ru\">");
    sb.AppendLine("<head>");
    sb.AppendLine("    <meta charset=\"UTF-8\">");
    sb.AppendLine("    <title>Список Файлов</title>");
    sb.AppendLine("    <link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css\" rel=\"stylesheet\">");
    sb.AppendLine("</head>");
    sb.AppendLine("<body>");

    // Меню
    sb.AppendLine("    <nav class=\"navbar navbar-expand-lg navbar-light bg-light\">");
    sb.AppendLine("        <div class=\"container-fluid\">");
    sb.AppendLine("            <a class=\"navbar-brand\" href=\"/\">Мой Сайт</a>");
    sb.AppendLine("            <button class=\"navbar-toggler\" type=\"button\" data-bs-toggle=\"collapse\" data-bs-target=\"#navbarNav\" aria-controls=\"navbarNav\" aria-expanded=\"false\" aria-label=\"Toggle navigation\">");
    sb.AppendLine("                <span class=\"navbar-toggler-icon\"></span>");
    sb.AppendLine("            </button>");
    sb.AppendLine("            <div class=\"collapse navbar-collapse\" id=\"navbarNav\">");
    sb.AppendLine("                <ul class=\"navbar-nav\">");
    sb.AppendLine("                    <li class=\"nav-item\">");
    sb.AppendLine("                        <a class=\"nav-link\" href=\"/add\">Загрузить Файлы</a>");
    sb.AppendLine("                    </li>");
    sb.AppendLine("                    <li class=\"nav-item\">");
    sb.AppendLine("                        <a class=\"nav-link active\" aria-current=\"page\" href=\"/files\">Просмотреть Файлы</a>");
    sb.AppendLine("                    </li>");
    sb.AppendLine("                </ul>");
    sb.AppendLine("            </div>");
    sb.AppendLine("        </div>");
    sb.AppendLine("    </nav>");

    // Список Файлов
    sb.AppendLine("    <div class=\"container mt-5\">");
    sb.AppendLine("        <h2 class=\"text-center\">Список Загруженных Файлов</h2>");
    sb.AppendLine("        <ul class=\"list-group mt-4\">");

    if (fileNames.Count == 0)
    {
        sb.AppendLine("            <li class=\"list-group-item\">Файлы отсутствуют.</li>");
    }
    else
    {
        foreach (var file in fileNames)
        {
            sb.AppendLine($"            <li class=\"list-group-item\"><a href=\"/UploadedFiles/{file}\" target=\"_blank\">{file}</a></li>");
        }
    }

    sb.AppendLine("        </ul>");
    sb.AppendLine("    </div>");

    sb.AppendLine("    <script src=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js\"></script>");
    sb.AppendLine("</body>");
    sb.AppendLine("</html>");

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(sb.ToString());
});

app.Run();