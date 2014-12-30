@echo off

set Packer=D:\Projects\iApps\Newgen\Build\Resources\WPE.exe
set WidgetsDir=D:\Projects\iApps\Newgen\Build\Cache\Widgets\
set OutputDir=D:\Projects\NS\Data\Cache\c3373d77-29c6-4670-8afb-43f0830bc3cf\12\widgets

set AW="http://nsapps.net/Apps/Newgen"
set A="NS"

call %Packer% -P "%WidgetsDir%Calendar"      -O "%OutputDir%" -W "Calendar"      -I "NW-O-C1"  -V "12.0" -D "Calendar !" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Clock"         -O "%OutputDir%" -W "Clock"         -I "NW-O-C2"  -V "12.0" -D "Shows the time and date" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Computer"      -O "%OutputDir%" -W "Computer"      -I "NW-O-C3"  -V "12.0" -D "A shortcut to My Computer" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Control Panel" -O "%OutputDir%" -W "Control Panel" -I "NW-O-C4"  -V "12.0" -D "A shortcut to Control Panel" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%CPUMonitor"    -O "%OutputDir%" -W "CPUMonitor"    -I "NW-O-C5"  -V "12.3" -D "CPU Monitor" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Desktop"       -O "%OutputDir%" -W "Desktop"       -I "NW-O-D1"  -V "12.1" -D "Shows the desktop" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Gmail"         -O "%OutputDir%" -W "Gmail"         -I "NW-O-G1"  -V "12.0" -D "Show emails from your gmail account" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Newgen GPlus"  -O "%OutputDir%" -W "Newgen GPlus"  -I "NW-O-HG1" -V "12.0" -D "+Newgen and +You" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Hotmail"       -O "%OutputDir%" -W "Hotmail"       -I "NW-O-H1"  -V "12.0" -D "Show emails from your hotmail account" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Internet"      -O "%OutputDir%" -W "Internet"      -I "NW-O-I1"  -V "12.4" -D "Just a Web Browser" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Me"            -O "%OutputDir%" -W "Me"            -I "NW-O-M1"  -V "12.0" -D "Just You and Newgen" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Music"         -O "%OutputDir%" -W "Music"         -I "NW-O-M2"  -V "12.0" -D "Plays your Music" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Socialite"     -O "%OutputDir%" -W "Socialite"     -I "NW-O-S1"  -V "12.0" -D "Find and connect people that you know" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Pictures"      -O "%OutputDir%" -W "Pictures"      -I "NW-O-P2"  -V "12.3" -D "Shows your Pictures" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Quotes"        -O "%OutputDir%" -W "Quotes"        -I "NW-O-Q1"  -V "12.3" -D "Shows daily updated quotes" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%QuickNotes"    -O "%OutputDir%" -W "QuickNotes"    -I "NW-O-Q2"  -V "12.0" -D "QuickNotes right at your StartScreen." -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Store"         -O "%OutputDir%" -W "Store"         -I "NW-O-S1"  -V "12.3" -D "Newgen Store" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Twitter"       -O "%OutputDir%" -W "Twitter"       -I "NW-O-T1"  -V "12.0" -D "Show twitts from your Twitter account" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Video"         -O "%OutputDir%" -W "Video"         -I "NW-O-V1"  -V "12.3" -D "Shows your Videos" -A "%A%" -AW "%AW%"
call %Packer% -P "%WidgetsDir%Weather"       -O "%OutputDir%" -W "Weather"       -I "NW-O-W1"  -V "12.3" -D "Shows the correct Weather in your city" -A "%A%" -AW "%AW%"