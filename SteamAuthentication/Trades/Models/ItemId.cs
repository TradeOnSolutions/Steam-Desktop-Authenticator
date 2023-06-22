namespace SteamAuthentication.Trades.Models;

public readonly struct ItemId : IEquatable<ItemId>
{
    public long ClassId { get; }

    public long InstanceId { get; }

    public ItemId(long classId, long instanceId)
    {
        ClassId = classId;
        InstanceId = instanceId;
    }

    private sealed class ClassIdInstanceIdEqualityComparer : IEqualityComparer<ItemId>
    {
        public bool Equals(ItemId x, ItemId y)
        {
            return x.ClassId == y.ClassId && x.InstanceId == y.InstanceId;
        }

        public int GetHashCode(ItemId obj)
        {
            return HashCode.Combine(obj.ClassId, obj.InstanceId);
        }
    }

    public static IEqualityComparer<ItemId> ClassIdInstanceIdComparer { get; } =
        new ClassIdInstanceIdEqualityComparer();

    public void Deconstruct(out long classId, out long instanceId)
    {
        classId = ClassId;
        instanceId = InstanceId;
    }

    public bool Equals(ItemId other) => ClassId == other.ClassId && InstanceId == other.InstanceId;

    public override bool Equals(object? obj) => obj is ItemId other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(ClassId, InstanceId);

    public static bool operator ==(ItemId left, ItemId right) => left.Equals(right);

    public static bool operator !=(ItemId left, ItemId right) => !(left == right);
}