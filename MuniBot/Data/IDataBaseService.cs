using Microsoft.EntityFrameworkCore;
using MuniBot.Common.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuniBot.Data
{
    public interface IDataBaseService
    {
        DbSet<UserModel> User { get; set; }

        Task<bool> SaveAsync();

    }
}
