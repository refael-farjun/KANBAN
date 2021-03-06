using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer.objects
{
    public interface IUser
    {
        bool IsLoggedIn { get; set; }
        string Email { get;}
        public abstract MFResponse<IUser> Login(string password);
        public MFResponse logout();
    }
}
