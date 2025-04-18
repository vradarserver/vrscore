﻿/*
 * Globalize Culture sr-RS
 *
 * https://raw.githubusercontent.com/nikgraf/jquery-global/master/globinfo/Globalization.pt-BR.js
 *
 */

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

Globalize.addCultureInfo("pt-BR", "default", {
    name: "pt-BR",
    englishName: "Portuguese (Brazil)",
    nativeName: "Português (Brasil)",
    language: "pt",
    numberFormat: {
        ',': ".",
        '.': ",",
        percent: {
            pattern: ["-n%","n%"],
            ',': ".",
            '.': ","
        },
        currency: {
            pattern: ["-$ n","$ n"],
            ',': ".",
            '.': ",",
            symbol: "R$"
        }
    },
    calendars: {
        standard: {
            days: {
                names: ["domingo","segunda-feira","terça-feira","quarta-feira","quinta-feira","sexta-feira","sábado"],
                namesAbbr: ["dom","seg","ter","qua","qui","sex","sáb"],
                namesShort: ["D","S","T","Q","Q","S","S"]
            },
            months: {
                names: ["janeiro","fevereiro","março","abril","maio","junho","julho","agosto","setembro","outubro","novembro","dezembro",""],
                namesAbbr: ["jan","fev","mar","abr","mai","jun","jul","ago","set","out","nov","dez",""]
            },
            AM: null,
            PM: null,
            eras: [{"name":"d.C.","start":null,"offset":0}],
            patterns: {
                d: "dd/MM/yyyy",
                D: "dddd, d' de 'MMMM' de 'yyyy",
                t: "HH:mm",
                T: "HH:mm:ss",
                f: "dddd, d' de 'MMMM' de 'yyyy HH:mm",
                F: "dddd, d' de 'MMMM' de 'yyyy HH:mm:ss",
                M: "dd' de 'MMMM",
                Y: "MMMM' de 'yyyy"
            }
        }
    }
});

}(this));
