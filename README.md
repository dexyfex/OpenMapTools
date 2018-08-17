# OpenMapTools by dexyfex 
OpenFormats map conversion toolkit

Package includes: 
VIPL - Convert SA IPL files into V .ymap.xml files, with LODs. 
VIDE - Convert IV IDE files into V .ytyp.xml files, with interiors. 
VOPL - Convert IV OPL files into V .ymap.xml files, with LODs and interiors. 
VONV - Convert IV ONV files into V .ynv files. 

This toolkit is intended to assist in converting map mods from GTASA and GTAIV to GTAV. 
Intended use for IV mods: 
1) Create a folder structure like: 
-- VideIn 
-- VideOut 
-- VoplIn 
-- VoplOut 
-- VonvIn 
-- VonvOut 
2) Copy your mod's IDE files into VideIn, and the OPL files into VoplIn. Copy ONV files to VonvIn. 
3) Run VIDE to convert your IDE files into .ytyp.xml files, specifying the VideIn and VideOut folders. 
4) Copy all the .vopl.txt files from the VideOut folder to the VoplIn folder. 
5) Run VOPL to convert your OPL files (and the .vopl.txt files) into .ymap.xml files, specifying the VoplIn and VoplOut folders. 
6) Run VONV to convert your ONV files into .ynv files, specifying the VonvIn and VonvOut folders. 
7) Use CodeWalker RPF Explorer "Import XML" option to import the .ytyp.xml and .ymap.xml files into an RPF archive for your map metadata. The gtxd.meta and .ynv files can be imported into the RPF as normal files. 
8) XML for a DLC _manifest.ymf file can be generated with CodeWalker, by loading all the converted .ymap files in the project window, and choosing the Generate Manifest tool. 
9) This isn't a full map conversion tutorial! 
