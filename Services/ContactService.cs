using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class ContactService: IContactService
    {
        private readonly ApplicationDbContext context;
        public ContactService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Contact> Create(Contact Contact)
        {
            await context.Contact.AddAsync(Contact);
            context.SaveChanges();
            return Contact;
        }



        public void Delete(Contact Contact)
        {
            context.Entry(Contact).State = EntityState.Deleted;
            context.SaveChanges();

        }



        public async Task<IEnumerable<Contact>> Get()
        {
            return await context.Contact.OrderByDescending(o=>o.Id).ToListAsync();
        }

        public async Task<Contact> GetByEmail(string email)
        {
            return await context.Contact.SingleOrDefaultAsync(a => a.Email == email);
        }

        public async Task<Contact> GetById(int id)
        {
            return await context.Contact.SingleOrDefaultAsync(a => a.Id == id);
        }



        public void Update(Contact Contact)
        {
            context.Entry(Contact).State = EntityState.Modified;

            context.SaveChanges();

        }
    }
}
