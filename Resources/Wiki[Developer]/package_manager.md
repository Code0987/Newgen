# Package Manager

Simple package manager for any kind of packages.


## Features

- Install/Update packages
    - (s)ftp
    - http(s)
    - git
    - local file
- Multiple packages locations, source as well destination


## Package

**Layout**

    <Root>
        /.pkg
            /id (text)
                This contains id of package.
                NOTE: This must be equal to <Root> folder name.

            /version (text)
                This contains version of package.

            /logo (png)
                This is max-resolution logo (image) file.

            /sources (text) [optional]
                This is sources file, containing links for updating package.
                {
                    'git': {
                        'uri': ''
                    },
                    '7zip': {
                        'uri': ''
                    }
                }

            /runtimes (text) [optional]
                This file containing runtimes supported.
                Default provider is for compiled packages on `Newgen.Core.dll!Newgen.Package`.
                Other providers must be pre-loaded.
                {
                    'NodeWebkit-Newgen-Runtime': {
                        'uri': ''
                    }
                }

            /license (text) [optional]
                This is license file.

            /description (text) [optional]
                This is description file, containing notes, etc.

            /authors (json) [optional]
                This is authors file.
                {
                    'someone': {
                        'website': '',
                        'email': ''
                    }
                }

            /dependency-cache
                This file tell where explicit dependencies folders are located (relative to <Root>).
                These packages will not be discovered other than for updating, if any.
                DEAFULT: .pkg/dependencies
            /dependency-map
                This file tell how dependencies are linked.
                {
                    'id': '(>=, <=)version|*' 

                    // Example:
                    // 'nuget': '<=2.0,>=1.0' // This will be installed in `dependency-cache`
                    // 'npm': '>=1.0' // This will be installed in `dependency-cache`
                    // 'git': '*' // This will **not** be installed in `dependency-cache`
                }
            /dependencies [optional] (can be changed)
                This contains all explicit hard coded packages required to run current package.


## Dependency management

**Case: A package depends on latest version of a package**

Let MyZip package depend on any version 7Zip package.
No problem here, simple dependency.

**Case: A package depends on particular version of a package**

As per above layout.

## Implementation (Abstract)

    /PackageManager

        /Packages (Package providers, first priority)
            These are packages that contain logic for loading other packages.
            Like `GitBasedPackageProviderPackage` would be loaded first and register itself as package provider / factory and then would be given instructions to check and load any git based package. This would not be a git dist. itself.

        /Packages (Tiles providers, second priority)
            These are the packages that provider `Tiles` for `StartScreen`.

        /Packages (Everything else, triggered by above)
            Literally every folder is a package.

        /Get (Install or update)
            This would do as said.

            `-Get ID=<ID> From=<URI> Type=<GitBasedPackageProvider>` To=<URI>

            This command would call `GitBasedPackageProvider` package
                `<Provider>.Get('<ID>', '<URI>')`
            Now, provider should respond accordingly.

            ID optional, From provided
                Installs fresh package, if their is no conflict.

            ID provided, From optional
                Installs package from default repository, if not installed.
                Else, updates by calling appropriate provider.
                `<Provider>.Get('<ID>')`
                Rest depends on provider.

            Dependencies
                These can be downloaded in parallel, with similar command
