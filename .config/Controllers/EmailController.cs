using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WangenPizza.Dtos;
using WangenPizza.Helper;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using WangenPizza.Services;

namespace WangenPizza.Controllers
{
    public class EmailController : Controller
    {
        #region Ctor
        private readonly IContactService contactService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;
        private readonly IMailService mailService;
        private readonly IEmailTextService emailTextService;

        public EmailController(IContactService contactService, UserManager<ApplicationUser> _UserManager, IMapper mapper , IMailService mailService , IEmailTextService EmailTextService)
        {
            this.contactService = contactService;
            userManager = _UserManager;
            this.mapper = mapper;
            this.mailService = mailService;
            emailTextService = EmailTextService;
        }
        #endregion



        #region Send to Contact Requests
        [HttpGet]
        public async Task<IActionResult> Send(int id)
        {
            var data = await contactService.GetById(id);
            var model = mapper.Map<ContactDto>(data);
            TempData["UserEmail"] = model.Email;
            var textData = await emailTextService.Get();
            ViewBag.Texts = textData;
            ViewBag.Email = model.Email;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Send(EmailDto dto)
        {
            var email = Convert.ToString(TempData["UserEmail"]);
            MailRequest MailRequest = new MailRequest
            {
                ToEmail = email,
                Subject = "Wangen",
                Body = dto.Message,


            };
            await mailService.SendEmailAsync(MailRequest, default);

            return RedirectToAction("Index", "Contact");
        }

        #endregion

        #region Send to users 
        [HttpGet]
        public async Task<IActionResult> SendToUsers(string id)
        {
            var userModel = await userManager.FindByIdAsync(id);
            TempData["UserEmail"] = userModel.Email;
            var textData = await emailTextService.Get();
            ViewBag.Texts = textData;
            ViewBag.Email = userModel.Email;
            ViewBag.Username = userModel.UserName;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendToUsers(EmailDto dto)
        {
            var email = Convert.ToString(TempData["UserEmail"]);
            MailRequest MailRequest = new MailRequest
            {
                ToEmail = email,
                Subject = "Wangen",
                Body = dto.Message,

            };
            await mailService.SendEmailAsync(MailRequest, default);

            return RedirectToAction("IndexOfUsers", "Users");
        }


        #endregion
        [HttpPost]
        public async Task<IActionResult> SendToAll(List<string> SelectedEmails)
        {
            var textData = await emailTextService.Get();
            var users = await contactService.Get();

            // لو فيه إيميلات مختارة فقط
            if (SelectedEmails != null && SelectedEmails.Any())
            {
                users = users.Where(u => SelectedEmails.Contains(u.Email)).ToList();
            }

            ViewBag.Texts = textData;
            ViewBag.Users = users;
            ViewBag.SelectedEmails = SelectedEmails;

            return View("SendToAll");
        }


        [HttpPost]
        public async Task<IActionResult> SendSelectedEmails(EmailDto dto)
        {
            var allUsers = await contactService.Get();
            var userList = allUsers.ToList();

            // لو تم تحديد إيميلات معينة فقط
            if (dto.SelectedEmails != null && dto.SelectedEmails.Any())
            {
                userList = userList.Where(u => dto.SelectedEmails.Contains(u.Email)).ToList();
            }

            var batchSize = 10;
            int sentCount = 0;

            for (int i = 0; i < userList.Count; i += batchSize)
            {
                var batch = userList.Skip(i).Take(batchSize);
                var emailTasks = new List<Task>();

                foreach (var client in batch)
                {
                    var mailRequest = new MailRequest
                    {
                        ToEmail = client.Email,
                        Subject = "Wangen",
                        Body = dto.Message,
                    };

                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            await mailService.SendEmailAsync(mailRequest, default);
                            Interlocked.Increment(ref sentCount);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ فشل إرسال إلى {client.Email}: {ex.Message}");
                        }
                    });

                    emailTasks.Add(task);
                }

                await Task.WhenAll(emailTasks);
                await Task.Delay(2000);
            }

            TempData["SuccessEmailMessage"] = $"✅ تم إرسال {sentCount} رسالة بنجاح";
            return RedirectToAction("Index", "Home");
        }

        //[HttpPost("upload-image")]
        //public async Task<IActionResult> UploadImage(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest(new { message = "No file uploaded." });
        //    }

        //    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
        //    if (!Directory.Exists(uploadPath))
        //    {
        //        Directory.CreateDirectory(uploadPath);
        //    }

        //    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        //    var filePath = Path.Combine(uploadPath, fileName);

        //    using (var stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        await file.CopyToAsync(stream);
        //    }

        //    var fileUrl = Url.Content($"~/uploads/{fileName}");

        //    return Ok(new { location = fileUrl });
        //}

        [HttpPost]
        public IActionResult UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid file type.");

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var imageUrl = Url.Content($"~/uploads/{fileName}");
            return Json(new { location = imageUrl });
        }
    }
}
