using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class EmailTextService: IEmailTextService
    {
        private readonly ApplicationDbContext context;
        public EmailTextService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<EmailText> Create(EmailText EmailText)
        {
            await context.EmailText.AddAsync(EmailText);
            context.SaveChanges();
            return EmailText;
        }



        public void Delete(EmailText EmailText)
        {
            context.Entry(EmailText).State = EntityState.Deleted;

            context.SaveChanges();

        }



        public async Task<IEnumerable<EmailText>> Get()
        {
            return await context.EmailText.ToListAsync();
        }

        public async Task<EmailText> GetById(int id)
        {
            return await context.EmailText.FirstOrDefaultAsync(a => a.Id == id);
        }



        public void Update(EmailText EmailText)
        {
            context.Entry(EmailText).State = EntityState.Modified;

            context.SaveChanges();

        }

    }
}
