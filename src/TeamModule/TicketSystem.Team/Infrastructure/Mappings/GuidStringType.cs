using System.Data;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace TicketSystem.Team.Infrastructure.Mappings;

public class GuidStringType : IUserType
{
    public SqlType[] SqlTypes => new[] { new SqlType(DbType.String) };

    public Type ReturnedType => typeof(Guid);

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
        var value = NHibernateUtil.String.NullSafeGet(rs, names[0], session);
        if (value == null) return Guid.Empty;
        return Guid.Parse((string)value);
    }

    public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
    {
        if (value == null || value is Guid guid && guid == Guid.Empty)
        {
            NHibernateUtil.String.NullSafeSet(cmd, null, index, session);
        }
        else
        {
            NHibernateUtil.String.NullSafeSet(cmd, ((Guid)value).ToString(), index, session);
        }
    }

    public object Replace(object original, object target, object owner) => original;
}
