﻿using System;
using System.Collections.Generic;
using SuccincT.Functional;
using SuccincT.Unions.PatternMatchers;
using static SuccincT.Functional.Unit;

namespace SuccincT.Unions
{
    public sealed class Union<T1, T2> : IUnion<T1, T2, Unit, Unit>
    {
        private readonly Dictionary<Variant, Func<int>> _hashCodes;
        private readonly Dictionary<Variant, Func<Union<T1, T2>, bool>> _unionsMatch;
        private readonly T1 _value1;
        private readonly T2 _value2;

        public Union(T1 value) : this(unit)
        {
            _value1 = value;
            Case = Variant.Case1;
        }

        public Union(T2 value) : this(unit)
        {
            _value2 = value;
            Case = Variant.Case2;
        }

        // ReSharper disable once UnusedParameter.Local - unit param used to 
        // prevent JSON serializer from using this constructor to create an invalid union.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "_")]
        private Union(Unit _)
        {
            _hashCodes = new Dictionary<Variant, Func<int>>
            {
                {Variant.Case1, () => _value1.GetHashCode()},
                {Variant.Case2, () => _value2.GetHashCode()}
            };
            _unionsMatch = new Dictionary<Variant, Func<Union<T1, T2>, bool>>
            {
                {Variant.Case1, other => EqualityComparer<T1>.Default.Equals(_value1, other._value1)},
                {Variant.Case2, other => EqualityComparer<T2>.Default.Equals(_value2, other._value2)}
            };
        }

        public Variant Case { get; }

        public T1 Case1 => Case == Variant.Case1 ? _value1 : throw new InvalidCaseException(Variant.Case1, Case);
        public T2 Case2 => Case == Variant.Case2 ? _value2 : throw new InvalidCaseException(Variant.Case2, Case);

        public IUnionFuncPatternMatcher<T1, T2, TResult> Match<TResult>() =>
            new UnionPatternMatcher<T1, T2, TResult>(this);

        public IUnionActionPatternMatcher<T1, T2> Match() => new UnionPatternMatcher<T1, T2, Unit>(this);

        public override bool Equals(object obj) => obj is Union<T1, T2> union && UnionsEqual(union);

        public override int GetHashCode() => _hashCodes[Case]();

        public static bool operator ==(Union<T1, T2> p1, Union<T1, T2> p2)
        {
            var aObj = (object) p1;
            var bObj = (object) p2;
            return aObj == null && bObj == null || aObj != null && p1.Equals(p2);
        }

        public static bool operator !=(Union<T1, T2> a, Union<T1, T2> b) => !(a == b);

        private bool UnionsEqual(Union<T1, T2> testObject) => Case == testObject.Case && _unionsMatch[Case](testObject);

        public static implicit operator Union<T1, T2>(T1 value) => new Union<T1, T2>(value);
        public static implicit operator Union<T1, T2>(T2 value) => new Union<T1, T2>(value);

        Unit IUnion<T1, T2, Unit, Unit>.Case3 => throw new InvalidCaseException(Variant.Case3, Case);
        Unit IUnion<T1, T2, Unit, Unit>.Case4 => throw new InvalidCaseException(Variant.Case4, Case);
    }
}