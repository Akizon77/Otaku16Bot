using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otaku16.Model
{
    public enum Permissions
    {
        Banned = 0,
        Post = 1,
        Aduit = 1 << 1,
        DirectPost = 1 << 2,
        User = Post,
        Admin = Post | Aduit,
        Owner = Admin | DirectPost
    }
}