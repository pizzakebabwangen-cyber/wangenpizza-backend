using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WangenPizza.Dtos;
using WangenPizza.Helper;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using WangenPizza.Services;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IConfiguration configuration;
        #region Ctor

        private readonly IMapper mapper;
        private readonly IContactService contactService;
        private readonly IEmailHtmlTemplateService emailHtmlTemplateService;
        private readonly IMailService mailService;
        private readonly IHubContext<NotificationHub> _hubContext;


        public ContactController(IConfiguration configuration, IHubContext<NotificationHub> hubContext, IMapper mapper, IContactService contactService , IEmailHtmlTemplateService emailHtmlTemplateService , IMailService mailService)
        {
            this.configuration = configuration;
            this.mapper = mapper;
            this.contactService = contactService;
            this.emailHtmlTemplateService = emailHtmlTemplateService;
            this.mailService = mailService;
            _hubContext = hubContext;

        }
        #endregion

        #region Create Contact
        [HttpPost("Contact")]
        public async Task<IActionResult> Create([FromBody] ContactDto dto)
        {
           try
            {
                var data = mapper.Map<Contact>(dto);
                await contactService.Create(data);
                if (data != null)
                {
                    CustomResponse response = new CustomResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Contact us request created successfully !"
                       
                       
                    };
                    string Adminbody =  emailHtmlTemplateService.GetContactUsAdminTemplate(dto);
                    var  EmailSection = configuration.GetSection("MailSettings");
                    string AdminEmail = EmailSection["Mail"];
                    MailRequest AdminMailRequest = new MailRequest
                    {
                        ToEmail = AdminEmail,
                        Subject = "Wangen -Contact",
                        Body = Adminbody,

                    };
                    await mailService.SendEmailAsync(AdminMailRequest, default);
                    string body = emailHtmlTemplateService.GetContactUsTemplate(dto.Name);
                    MailRequest mailRequest = new MailRequest
                    {
                        ToEmail = dto.Email,
                        Subject = "Wangen Pizza",
                        Body = body,

                    };
                    await mailService.SendEmailAsync(mailRequest, default);
                    return Ok(response);
                }
                return StatusCode(400,value:  new CustomResponse { Code = "400", Message = "Error" });
            }
            catch(Exception)
            {
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });

            }

        }
        #endregion

        

        
    }
}
