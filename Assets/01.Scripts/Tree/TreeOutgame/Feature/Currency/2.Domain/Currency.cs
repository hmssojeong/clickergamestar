
// 재화를 의미하는 도메인 모델
// 우리에 게임에서의 재화 규칙을 

// 1. 음수이면 안된다.
//    1-1. 생성될때
//    1-2. +,-연산할때

// 2. 정해진 표기법이 있다.
// 3. 재화간에 +-가 되야된다.
// 4. 재화간에 ><>=

// [만들어야 하는 경우]
// 1. 재화가 여러 곳에 사용된다. (UI, 상점, 업그레이드 등 다양한 콘텐츠)
// 2. 포맷팅이 통일되어야 한다. 무적권!
// 3. 재화끼리 연산이 빈번하다.
// 4. 팀 프로젝트에서 있어서 실수를 방지하고 싶다.
// [안만들어도 되는 경우]
// 1. 게임을 빠르게 만들고 싶다.
// 2. 재화가 한 종류 뿐이고 사용처도 많지 않다.
// 3. 팀원 없이 혼자 개발해서 도메인에 대한 지식이 곧 뇌다.

// struct vs class
// struct은 int, double처럼 값으로 동작하기에 딱이다.
// 재화는 "값"이 중요하다.
using System;

public readonly struct Currency
{
    public readonly double Value;

    public Currency(double value)
    {
        // 유효성 검사
        if (value < 0)
        {
            throw new Exception("Currency값은 0보다 작을 수 없습니다.");
            // 이런 잘못된 데이터가 들어왔다는 것은 여러가지 부작용이 생길수있다.
            // 게임 플레이 도중에 그 부작용을 느끼는 것보다
            // 애초에 시작단계에서 에러를 뱉어버리는게 유지보수 면에서 편한다.
        }

        Value = value;
    }

    // 연산자 오버라이딩 : 객체간의 연산자(+,-, >, <)할때 암시적으로 호출되는 메서드

    // 1. 재화끼리 더하기
    public static Currency operator +(Currency currency1, Currency currency2)
    {
        return new Currency(currency1.Value + currency2.Value);
    }

    // 2. 재화끼리 빼기
    public static Currency operator -(Currency a, Currency b)
    {
        return new Currency(a.Value - b.Value);
    }

    // 3. 비교 연산자들
    public static bool operator >=(Currency a, Currency b)
    {
        return a.Value >= b.Value;
    }

    public static bool operator <=(Currency a, Currency b)
    {
        return a.Value <= b.Value;
    }

    public static bool operator >(Currency a, Currency b)
    {
        return a.Value > b.Value;
    }

    public static bool operator <(Currency a, Currency b)
    {
        return a.Value < b.Value;
    }

    // double → Currency 암시적 변환    
    public static implicit operator Currency(double value)
    {
        return new Currency(value);
    }

    // Currency -> double 암시적 변환
    public static explicit operator double(Currency currency)
    {
        return currency.Value;
    }

    // ToString이란 객체를 문자열로 변환될때 암시적으로 호출되는 메서드인데..
    // 이걸 개조(메서드 오버라이)해서 특정 포맷으로 문자 변환되게끔 강제한다.
    public override string ToString()
    {
        return Value.ToFormattedString();
    }
}