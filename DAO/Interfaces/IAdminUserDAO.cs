using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelsLibrary;
using ModelsLibrary.Admin;

namespace DAO.Interfaces
{
    public interface IAdminUserDAO : IGenericDAO<AdminUser>
    {
        AdminUser? GetByUsername(string username);
        bool ValidatePassword(string username, string password);
    }
}
