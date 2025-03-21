﻿/*
 * Globalize Culture sr-RS
 *
 * https://github.com/nikgraf/jquery-global/blob/master/globinfo/Globalization.sr-Latn-RS.js
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

Globalize.addCultureInfo("sr-Latn-RS", "default", {
    name: "sr-Latn-RS",
    englishName: "Serbian (Serbia)",
    nativeName: "srpski (Srbija)",
    language: "sr",
    numberFormat: {
        ',': ".",
        '.': ",",
        percent: {
            pattern: ["-n%","n%"],
            ',': ".",
            '.': ","
        },
        currency: {
            pattern: ["-n $","n $"],
            ',': ".",
            '.': ",",
            symbol: "Din."
        }
    },
    calendars: {
        standard: {
            '/': ".",
            firstDay: 1,
            days: {
                names: ["nedelja","ponedeljak","utorak","sreda","četvrtak","petak","subota"],
                namesAbbr: ["ned","pon","uto","sre","čet","pet","sub"],
                namesShort: ["ne","po","ut","sr","če","pe","su"]
            },
            months: {
                names: ["januar","februar","mart","april","maj","jun","jul","avgust","septembar","oktobar","novembar","decembar",""],
                namesAbbr: ["jan","feb","mar","apr","maj","jun","jul","avg","sep","okt","nov","dec",""]
            },
            AM: null,
            PM: null,
            eras: [{"name":"n.e.","start":null,"offset":0}],
            patterns: {
                d: "d.M.yyyy",
                D: "d. MMMM yyyy",
                t: "H:mm",
                T: "H:mm:ss",
                f: "d. MMMM yyyy H:mm",
                F: "d. MMMM yyyy H:mm:ss",
                M: "d. MMMM",
                Y: "MMMM yyyy"
            }
        }
    }
});

}(this));