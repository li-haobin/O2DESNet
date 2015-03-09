WSC proceedings Latex style

The WSC proceedings style is usually adapted each year. Thus you have to make sure that you are using the style for the current year.


Please check the 

wscYYpaper.tex

or

the 

wscYYposter.tex

(with YY the current year)

files for examples and further instructions.






The style comprises the following files which you should never change:

wscpaperproc.cls - the class file for a normal paper to appear in the proceedings
wscposterproc.cls - the class file for a poster abstract

wsc.sty - the general setup for the proceedings (page sizes, etc ...) is defined in there - is used in the class files
wscsetup.sty - the "branding" file which contains the year and editor information - is used in wsc.sty

wsc.bst - the bib entries formatting file


wscYYstyle.tex - a file containg the class and style files named above (YY the current year)