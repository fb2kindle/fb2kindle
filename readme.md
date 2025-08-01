# Fb2Kindle

[![Release](https://img.shields.io/github/v/release/fb2kindle/fb2kindle)](https://github.com/fb2kindle/fb2kindle/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/fb2kindle/fb2kindle/total?color=ff4f42)](https://sergiye.github.io/github-release-stats/?username=fb2kindle&repository=fb2kindle&page=1&per_page=100)
![Last commit](https://img.shields.io/github/last-commit/fb2kindle/fb2kindle?color=00AD00)


*Fb2Kindle is a portable, open-source fb2 books converter into Amazon Kindle formats (mobi & epub).*

----
## What does it look like?

Here's a preview of the app's explorer integration (context menu) on Windows 10:

[<img src="https://github.com/fb2kindle/fb2kindle/raw/master/preview.png" alt="preview"/>](https://github.com/fb2kindle/fb2kindle/raw/master/preview.png)


## Download Latest Version

Fb2Kindle provides an easy to use solution to convert, format with customizable css, combine multiple files into one and send your e-books to the Amazon Kindle devices.

The latest stable version can be downloaded from the [releases](https://github.com/fb2kindle/fb2kindle/releases) page, or get the newer one directly from:
[Latest Version](https://github.com/fb2kindle/fb2kindle/releases/latest)

Features include:

  * Support for `.fb2` files as input and `.mobi`/`.epub` files as output
  * Joining several `.fb2` books from folder to one output file (for book series)
  * Send generated book to email (see arguments)
  * Multiple commandline arguments for customizing behavior

# Build

**The recommended way to get the program is BUILD from source**
- Install git, Visual Studio (2022 or higher)
- `git clone https://github.com/fb2kindle/fb2kindle.git`
- build

----

### How To Use

To use:
  * Download the latest version from releases
  * There is no installation required, just start executable file from anywhere on your computer
  * Drag and drop any `.fb2` file to an app icon in file explorer to convert single file (with default options)


### Start and possible command-line arguments

  Fb2Kindle.exe [options]

  - `<path>`: input `.fb2` file path or files mask (ex: `*.fb2`) or path to `.fb2` files
  - `-epub`: create file in epub format
  - `-css` <styles.css>: styles used in destination book (example can be found here: [Fb2Kindle/Fb2Kindle.css](https://github.com/fb2kindle/fb2kindle/raw/master/Fb2Kindle/Fb2Kindle.css))
  - `-a`: process all `.fb2` books in app folder
  - `-r`: process files in subfolders (work with -a key)
  - `-j`: join files from each folder to the single book
  - `-o`: hide detailed output
  - `-w`: wait for key press on finish
  - `-mailto`: send document to email (kindle send-by-email delivery, see `-save` option to configure SMTP server)
  - `-save`: save parameters (listed below) to be used at the next start (`Fb2Kindle.json` file)
  - `-register`: add explorer integration (context menu)
  - `-unregister`: remove explorer integration
  - `-d`: delete source file after successful conversion
  - `-u` or `-update`: update application to the latest version. You can combine it with the `-save` option to enable auto-update on every run
  - `-s`: add sequence and number to the document title
  - `-c` (same as `-c1`) or `-c2`: use compression (slow)
  - `-optimizeSource`: optimize images in source file (decrease to 600x800 by default)
  - `-optimize`: optimize images in target (decrease to 600x800 by default)
  - `-ni`: no images
  - `-dc`: DropCaps mode
  - `-g`: grayscale images
  - `-jpeg`: save images in jpeg
  - `-ntoc`: no table of content
  - `-nch`: no chapters

### Examples:

    Fb2Kindle.exe somebook.fb2 -epub
    Fb2Kindle.exe "c:\booksFolder\*.fb2"
    Fb2Kindle.exe -a -r -j -d -w
    Fb2Kindle.exe "c:\bookSeries\*.fb2" -j -epub -mailto user@kindle.com -update -save

also you can check cmd script examples in archive here [other/scripts.7z](https://github.com/fb2kindle/fb2kindle/raw/master/other/scripts.7z)

----

## How can I help improve it?
The fb2kindle team welcomes feedback and contributions!<br/>
You can check if it works properly on your PC. If you notice any inaccuracies, please send us a pull request. If you have any suggestions or improvements, don't hesitate to create an issue.

Also, don't forget to star the repository to help other people find it.

## Donate!
Every [cup of coffee](https://patreon.com/SergiyE) you donate will help this app become better and let me know that this project is in demand.

## License

This program is free software: you can redistribute it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
