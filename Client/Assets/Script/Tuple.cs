using UnityEngine;
using System.Collections;

public struct Tuple<T>
{
    public T field;
    public Tuple(T field) {
        this.field = field;
    }
}

public struct Tuple<T1,T2>
{
    public T1 field1;
    public T2 field2;
    public Tuple(T1 field1,T2 field2) {
        this.field2=field2;
        this.field1=field1;
    }
}

public struct Tuple<T1,T2,T3>
{
    public T1 field1;
    public T2 field2;
    public T3 field3;

    public Tuple(T1 field1,T2 field2,T3 field3) {
        this.field1 = field1;
        this.field2 = field2;
        this.field3 = field3;
    }
}

public struct Tuple<T1, T2,T3,T4> {
    public T1 field1;
    public T2 field2;
    public T3 field3;
    public T4 field4;

    public Tuple(T1 field1, T2 field2,T3 field3,T4 field4) {
        this.field4 = field4;
        this.field1 = field1;
        this.field2 = field2;
        this.field3 = field3;
    }
}