using System;

public readonly struct Square : IEquatable<Square>
{
    public readonly int File; // 0 = a, 7 = h
    public readonly int Rank; // 0 = 1st rank, 7 = 8th rank

    public Square(int file, int rank)
    {
        File = file;
        Rank = rank;
    }

    public bool IsValid => File >= 0 && File < 8 && Rank >= 0 && Rank < 8;

    public static Square FromAlgebraic(string s) =>
        new Square(s[0] - 'a', s[1] - '1');

    public string ToAlgebraic() =>
        $"{(char)('a' + File)}{Rank + 1}";

    public bool Equals(Square other) => File == other.File && Rank == other.Rank;
    public override bool Equals(object obj) => obj is Square s && Equals(s);
    public override int GetHashCode() => File * 8 + Rank;
    public override string ToString() => ToAlgebraic();

    public static bool operator ==(Square a, Square b) => a.Equals(b);
    public static bool operator !=(Square a, Square b) => !a.Equals(b);
}
