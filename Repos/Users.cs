using Otaku16.Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otaku16.Repos
{
    public class UserRepo(ISqlSugarClient context) : BaseRepository<User>(context)
    {
        public static Logger logger => new Logger("REPO.USER");
        public void InitUserTableHeader()
        {
            Context.CodeFirst.InitTables<Post>();
        }

        public async Task<User?> GetUser(int id)
        {
            return await Queryable().Where(x => x.Id == id).FirstAsync().ConfigureAwait(false);
        }
        public async Task<bool> HasPermisson(long id,Permissions permissions)
        {
            var p =  await Queryable().Where(x => x.Id == id).FirstAsync().ConfigureAwait(false);
            return p.HasPermisson(permissions);
        }
    }
}
