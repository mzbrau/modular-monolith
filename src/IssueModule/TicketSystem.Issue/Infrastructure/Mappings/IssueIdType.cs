using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using TicketSystem.Issue.Domain;

namespace TicketSystem.Issue.Infrastructure.Mappings;

public class IssueIdType : IUserType
{
    public SqlType[] SqlTypes => new[] { NHibernateUtil.String.SqlType };
    public Type ReturnedType => typeof(IssueId);
    public bool IsMutable => false;

    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Equals(y);
    }

    public int GetHashCode(object x) => x.GetHashCode();

    public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        var value = NHibernateUtil.String.NullSafeGet(rs, names[0], session);
        if (value == null) return default(IssueId);
        return new IssueId(Guid.Parse((string)value));
    }

    public void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session)
    {
        if (value == null || value is IssueId issueId && issueId.Value == Guid.Empty)
        {
            NHibernateUtil.String.NullSafeSet(cmd, null, index, session);
            return;
        }

        NHibernateUtil.String.NullSafeSet(cmd, ((IssueId)value).Value.ToString(), index, session);
    }

    public object DeepCopy(object value) => value;

    public object Replace(object original, object target, object owner) => original;

    public object Assemble(object cached, object owner) => cached;

    public object Disassemble(object value) => value;
}
