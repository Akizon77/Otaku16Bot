using SqlSugar;


namespace Otaku16.Model
{
    public class User : SqlReopBase
    {
        /// <summary>
        /// 用户或者角色的唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public long Id { get; set; }

        /// <summary>
        /// 用户或角色所拥有的权限
        /// </summary>
        public Permissions Permission { get; set; }
    }
    public static class UserExt
    {
        public static bool HasPermisson(this User user, Permissions permission)
        {
            if (user == null && permission == Permissions.Post) return true;
            if (user == null) return false;
            if ((user.Permission & permission) == permission) return true;
            return false;
        }
    }
}
