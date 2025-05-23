﻿// Based on
// https://raw.githubusercontent.com/nikgraf/jquery-global/master/globinfo/Globalization.tr-TR.js


(function( window, undefined ) {
    var Globalize;

    if ( typeof require !== "undefined" &&
        typeof exports !== "undefined" &&
        typeof module !== "undefined" ) {
        // Assume CommonJS
        Globalize = require( "globalize" );
    } else {
        // Global variable
        Globalize = window.Globalize;
    }

    Globalize.addCultureInfo("tr", "default", {
        name: "tr",
        englishName: "Turkish",
        nativeName: "Türkçe",
        language: "tr",
        numberFormat: {
            ',': ".",
            '.': ",",
            percent: {
                pattern: ["-%n","%n"],
                ',': ".",
                '.': ","
            },
            currency: {
                pattern: ["-n $","n $"],
                ',': ".",
                '.': ",",
                symbol: "TL"
            }
        },
        calendars: {
            standard: {
                '/': ".",
                firstDay: 1,
                days: {
                    names: ["Pazar","Pazartesi","Salı","Çarşamba","Perşembe","Cuma","Cumartesi"],
                    namesAbbr: ["Paz","Pzt","Sal","Çar","Per","Cum","Cmt"],
                    namesShort: ["Pz","Pt","Sa","Ça","Pe","Cu","Ct"]
                },
                months: {
                    names: ["Ocak","Şubat","Mart","Nisan","Mayıs","Haziran","Temmuz","Ağustos","Eylül","Ekim","Kasım","Aralık",""],
                    namesAbbr: ["Oca","Şub","Mar","Nis","May","Haz","Tem","Ağu","Eyl","Eki","Kas","Ara",""]
                },
                AM: null,
                PM: null,
                patterns: {
                    d: "dd.MM.yyyy",
                    D: "dd MMMM yyyy dddd",
                    t: "HH:mm",
                    T: "HH:mm:ss",
                    f: "dd MMMM yyyy dddd HH:mm",
                    F: "dd MMMM yyyy dddd HH:mm:ss",
                    M: "dd MMMM",
                    Y: "MMMM yyyy"
                }
            }
        }
    });

}(this));
