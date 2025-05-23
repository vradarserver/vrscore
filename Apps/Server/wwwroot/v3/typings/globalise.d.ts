﻿// Type definitions for Globalize
// Project: https://github.com/jquery/globalize
// Definitions by: Boris Yankov <https://github.com/borisyankov/>
// Definitions: https://github.com/borisyankov/DefinitelyTyped

// agw: Had to rename the "bool" type to "boolean" for TS 1.7
// agw: Had to make GlobalizeStatic.findClosestCulture optional
// agw: Had to add monthsGenitive and shortYearCutoff to GlobalizeCalendar

interface GlobalizePercent {
    pattern: string[];
    decimals: number;
    groupSizes: number[];
    //",": string;
    //".": string;
    symbol: string;
}

interface GlobalizeCurrency {
    pattern: string[];
    decimals: number;
    groupSizes: number[];
    //",": string;
    //".": string;
    symbol: string;
}

interface GlobalizeNumberFormat {
    pattern: string[];
    decimals: string;
    //",": string;
    //".": string;
    groupSizes: number[];
    //"+": string;
    //"-": string;
    NaN: string;
    negativeInfinity: string;
    positiveInfinity: string;
    percent: GlobalizePercent;
    currency: GlobalizeCurrency;
}

interface GlobalizeEra {
    name: string;
    start: any;
    offset: number;
}

interface GlobalizeDays {
    names: string[];
    namesAbbr: string[];
    namesShort: string[];
}

interface GlobalizeMonths {
    names: string[];
    namesAbbr: string[];
}

interface GlobalizePatterns {
    d: string;
    D: string;
    t: string;
    T: string;
    f: string;
    F: string;
    M: string;
    Y: string;
    S: string;
}

interface GlobalizeCalendar {
    name: string;
    // "/": string,
    // ":": string,
    firstDay: number;
    days: GlobalizeDays;
    months: GlobalizeMonths;
    AM: string[];
    PM: string[];
    eras: GlobalizeEra[];
    twoDigitYearMax: number;
    patterns: GlobalizePatterns;
    monthsGenitive?: GlobalizeMonths;
    shortYearCutoff: number|string;
}

interface GlobalizeCalendars {
    standard: GlobalizeCalendar;
}

interface GlobalizeCulture {
    name: string;
    englishName: string;
    nativeName: string;
    isRTL: boolean;
    language: string;
    numberFormat: GlobalizeNumberFormat;
    calendars: GlobalizeCalendars;
    messages: any;
}
interface GlobalizeCultures {
    [index: number]: GlobalizeCulture;
}

interface GlobalizeStatic {
    cultures: GlobalizeCultures;
    init(cultureSelector: string);
    cultureSelector: string;

    culture(): GlobalizeCulture;
    culture(cultureSelector: string): GlobalizeCulture;
    culture(cultureSelector: string[]): GlobalizeCulture;

    addCultureInfo(cultureName, baseCultureName, info? );
    findClosestCulture(cultureSelector?: string);
    format(value, format, cultureSelector? );
    localize(key, cultureSelector?);

    parseDate(value: string, formats? , cultureSelector?: string): Date;
    parseInt(value: string, radix? , cultureSelector?: string): number;
    parseFloat(value: string, radix? , cultureSelector?: string): number;
}

declare var Globalize: GlobalizeStatic;
