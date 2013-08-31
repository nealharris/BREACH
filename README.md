#### As of August 2013 we are releasing “BREACH Basic”


## WHAT IS IT
-------------
BREACH Basic is a PoC, browser-less tool (for self-assessment and educational purposes) featuring only the basic BREACH oracle. 
We will soon release the more elaborated Browser-based prototype shown at BlackHat as well as custom ARP spoofing toolset. 

This PoC tool is not intended to locate vulnerable pages and parameters.
Rather, it allows you to potentially validate a page you expect could be exploitable in your application. 

For additional details, please refer to the 'Am I Affected' section at http://breachattack.com



## RELEVANT NOTES
-----------------
- MITM: This is not required for the PoC. Instead, we suggest a simple HOSTS entry to enable measurement of the encrypted traffic.

- Browser-less: A browser is not required for evaluating impact. 
At this time we provide a simple HTTP client that simulates browser behavior (The tool does not look at the responses).
The full-featured browser-based BREACH will follow.


- Stream Ciphers: BREACH Basic is not Block-Cipher aware at the moment, this might be incorporated into future releases. 
For testing purposes a cipher such as RC4 would be recommended.




## HOW TO USE IT
----------------
### Requirements:
1. Windows OS (7+ tested)
2. .NET 3.5+ Framework
3. Visual Studio 2010+ (if you want to modify the code)


The PoC is automatically wired up to extract a secret from a sample page (malbot.net).
While this page does not feature authentication or any useful functionality, it demonstrates a secret can be extracted from encrypted traffic. 


### How to run: 
1. Launch cmd.exe with administrator privileges
2. Type:  (echo. && echo 127.0.0.1 malbot.net) >> %windir%\system32\drivers\etc\hosts
3. Launch "SSLProxy.exe"
4. Launch "BREACH Basic.exe"
5. Verify the secret extracted is correct ( view-source:https://malbot.net/poc/ )


### How to customize:
1. Edit your hosts file entry with your new target
2. Edit 'TargetIP' address in 'SSLProxy.cs'
3. Edit 'KeySpace' in 'BREACH Basic.cs'
4. Edit 'TargetURL' in 'BREACH Basic.cs'
5. Edit 'canary' to specify your bootstrapping sequence in 'BREACH Basic.cs'
6. Compile & Run




## CONTACT
----------
Paper+Slides: http://breachattack.com
Email: contact@breachattack.com
