using System.Data;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace TicketSystem.Team.Infrastructure.Mappings;

public class LongType : IUserType
{
    public SqlType[] SqlTypes => new[] { new SqlType(DbType.Int64) };

    public Type ReturnedType => typeof(long);

    public bool IsMutable => false;

    public object Assemble(object cached, object owner) => cached;

    public object DeepCopy(object value) => value;

    public object Disassemble(object value) => value;

    public new bool Equals(object? x, object? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;
        return x.Equals(y);
    }

    public int GetHashCode(object x) => x.GetHashCode();

    public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        var value = NHibernateUtil.Int64.NullSafeGet(rs, names[0], session);
        if (value == null) return 0L;
        return (long)value;
    }

    public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
    {
        if (value == null || value is long id && id == 0)
        {
            NHibernateUtil.Int64.NullSafeSet(cmd, null, index, session);
        }
        else
        {
            NHibernateUtil.Int64.NullSafeSet(cmd, (long)value, index, session);
        }
    }

    public object Replace(object original, object target, object owner) => original;
}
