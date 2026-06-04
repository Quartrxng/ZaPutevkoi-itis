using ModelsLibrary.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO.Interfaces
{
    public interface IAdminSessionDAO : IGenericDAO<AdminSession>
    {
        AdminSession? GetByToken(byte[] token);
        void DeleteExpiredSessions();
        bool IsTokenValid(byte[] token);
        void DeleteByToken(byte[] token);

    }
}
