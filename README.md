# Sextant - Navigate MS Windows like in ye olden days!

Sextant allows you to switch between application windows in Microsoft Windows using only your keyboard. 
The selection of windows is heavily influenced by [emacs' ace-window package](https://github.com/abo-abo/ace-window). 

This is still in very early development. Handle with care! :)

## Building

Sextant is built using [Paket](https://fsprojects.github.io/Paket/) and [Fake](https://fake.build/fake-dotnetcore.html) using [.NET Core (.NET SDK > 2.1.300)](https://www.microsoft.com/net/download/dotnet-core/2.1). 

- `./fake.cmd build` produces a release build.
- `./fake.cmd build -- --debug` produces a debug build.
- `./fake.cmd build -- --debug --watch` will continuously watch the filesystem for changes and trigger new builds anytime the source code is altered.
- The project should be buildable and debuggable in [VS Code](https://code.visualstudio.com/) directly after cloning the repo.