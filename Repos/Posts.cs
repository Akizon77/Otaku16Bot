using Otaku16.Model;
using SqlSugar;

namespace Otaku16.Repos;

public class PostRepo(ISqlSugarClient content) : BaseRepository<Post>(content)
{
    public static Logger logger => new Logger("REPO.POST");

    public void InitGroupTableHeader()
    {
        Context.CodeFirst.InitTables<Post>();
    }

    public async Task<Post?> GetPostByID(int id)
    {
        return await Queryable().Where(x => x.Id == id).FirstAsync().ConfigureAwait(false);
    }

}