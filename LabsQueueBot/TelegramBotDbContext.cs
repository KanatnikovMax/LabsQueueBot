using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabsQueueBot
{
    internal class TelegramBotDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public TelegramBotDbContext() : base()
        {
            
        }
    }
}
