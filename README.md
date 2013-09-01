## WHAT IS THIS?

This tool features the basic BREACH attack.
We're working on the browser-based prototype shown at BlackHat 2013.

This is intended for self-assessment only.  Don't do bad things.


## NOTES

- MITM: This is not required for the PoC. Instead, we suggest a simple HOSTS entry to enable measurement of the encrypted traffic.

- Browserless: At this time we provide a simple HTTP client that simulates browser behavior.
The full-featured browser-based tool will follow.

- Block Ciphers: The tool isn't smart enough to work against block ciphers yet.
Maybe you can send us a pull request to fix this!


## HOW TO USE IT
### Requirements:
1. Windows OS (7+ tested)
2. .NET 3.5+ Framework
3. Visual Studio 2010+ (if you want to modify the code)


### How to run: 
1. Build the projects to get the executables, download them at http://breachattack.com/precompiled/.
2. Run `(echo. && echo 127.0.0.1 malbot.net) >> %windir%\system32\drivers\etc\hosts` in a command shell with admin privs.
3. Launch `SSLProxy.exe`.
4. Launch `BREACH Basic.exe`.
5. Verify the secret extracted is correct. (Take a look at the source of https://malbot.net/poc/.)


### How to customize:
1. Edit your hosts file entry with your new target.
2. Edit `TargetIP` address in `SSLProxy.cs`.
3. Edit `KeySpace` in `BREACH Basic.cs` to reflect the target secret's alphabet.
4. Edit `TargetURL` in `BREACH Basic.cs`.
5. Edit `canary` to specify your bootstrapping sequence in 'BREACH Basic.cs'.
6. Compile & Run.

### How to contribute:
Fork this repo.  Make some awesome changes.  Send us a pull request.

## CONTACT

- Paper+Slides: http://breachattack.com
- Email: contact@breachattack.com
