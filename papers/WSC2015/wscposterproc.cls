% %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% 					Class file for WSC poster abstracts
%
%  Effective with WSC 09, the two column format was replaced with 
%  a single column format
%
% Parameters (like names of editors) have to be setup using the wscsetup.sty style file.
%
% All settings valid for papers and poster abstract should be placed into wsc.sty .
% %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
\ProvidesClass{wscposterproc}
\NeedsTeXFormat{LaTeX2e}
\DeclareOption*{\PassOptionsToClass{\CurrentOption}{article}}
\ProcessOptions 
\LoadClass[11pt, twoside, onecolumn]{article}

% --------------------------------- REQUIRED PACKAGES ---------------------------------
\RequirePackage{wsc}