using System;

[Serializable]
public class HexPos
{
    public int Q;
    public int R;

    public HexPos(int q, int r) { Q = q; R = r; }

    public override bool Equals(object obj)
    {
        HexPos other = obj as HexPos;
        if(other == null) return false;
        return Q == other.Q && R == other.R;
    }

    public override int GetHashCode()
    {
        unchecked { return (Q * 397) ^ R; }
    }

    public static bool operator ==(HexPos a, HexPos b)
    {
        if(ReferenceEquals(a, b)) return true;
        if((object)a == null || (object)b == null) return false;
        return a.Q == b.Q && a.R == b.R;
    }

    public static bool operator !=(HexPos a, HexPos b) => !(a == b);

    public override string ToString() => "(" + Q + "," + R + ")";
}
